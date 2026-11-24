using System.Collections.Generic;
using Rake2D.JnG;

namespace Rake2D.Script;

public class WormBuilder : BehaviorScript
{
	private int _partIndex;

	private int _exploIndex;

	private float _timer;

	private float _exploDelay;

	private float[] _delayArray;

	private List<PartData> _partDataList;

	private List<GameObject> _partList;

	private List<GameObject> _bossMeterPartList;

	private int _state;

	private string _exploState;

	private string _bossMeterName;

	public override void Init()
	{
		base.Init();
		_partIndex = 0;
		_delayArray = ParamArray("delay");
		_exploDelay = Param("explo_delay", 500f) / 1000f;
		_exploState = ParamString("explo_state");
		_bossMeterName = ParamString("boss_meter_name");
		_partDataList = new List<PartData>();
		string[] array = ParamStringArray("object_id");
		for (int i = 0; i < array.Length; i++)
		{
			PartData partData = GameCore.GetPartData(array[i]);
			if (partData != null)
			{
				_partDataList.Add(partData);
			}
		}
		_partList = GameCore.Allocator.GetGameObjectList();
		if (_bossMeterName != null)
		{
			_bossMeterName = TextBundle.GetText(_bossMeterName);
			_bossMeterPartList = GameCore.Allocator.GetGameObjectList();
		}
	}

	public override void RegisterEvents()
	{
		if (DoInEditor)
		{
			MyObject.OnUpdate += OnUpdate;
		}
		if (_bossMeterName != null)
		{
			GameJnG.AddBossMeter(MyObject, _bossMeterName);
		}
	}

	private void OnUpdate(float elapsedTime)
	{
		if (_state == 0)
		{
			if (Utils.ChangeValueToTarget(ref _timer, 0f, elapsedTime))
			{
				if (_partIndex < _partDataList.Count)
				{
					GameObject gameObject = MyLayer.CreateObjectByPartData(_partDataList[_partIndex], MyObject.WorldPosition, MyObject.Scale, MyObject.Rotation, MyObject.ZOrder);
					if (gameObject != null)
					{
						BezierPath bezierPath = MyObject.MyPath.Clone(gameObject, -1L);
						bezierPath.ControlPointList[0] = gameObject.WorldPosition;
						bezierPath.CalculateLength();
						gameObject.ChangePath(bezierPath);
						if (MyObject.MyTimeLine != null)
						{
							gameObject.MyTimeLine = MyObject.MyTimeLine.Clone(gameObject, -1L);
							gameObject.SetCurrentTimeLine(gameObject.MyTimeLine);
						}
						_partList.Add(gameObject);
						if (_bossMeterPartList != null)
						{
							GameObject childByIdDeep = gameObject.GetChildByIdDeep("boss_meter");
							_bossMeterPartList.Add(childByIdDeep ?? gameObject);
						}
					}
					if (_partIndex < _delayArray.Length)
					{
						_timer = _delayArray[_partIndex];
					}
					else
					{
						_timer = _delayArray[_delayArray.Length - 1];
					}
					_timer /= 1000f;
					_partIndex++;
				}
				else if (_exploState != null)
				{
					_state = 1;
				}
				else
				{
					MyObject.Destroy();
				}
			}
		}
		else if (_state == 1)
		{
			bool flag = true;
			foreach (GameObject part in _partList)
			{
				if (part.IsAlive)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				_state = 2;
				_timer = 0f;
				_exploIndex = _partList.Count - 1;
			}
		}
		if (_state == 2)
		{
			UpdateExplo(elapsedTime);
		}
		if (_bossMeterPartList == null)
		{
			return;
		}
		MyObject.CurrentHP = 0f;
		MyObject.MaxHP = 0f;
		foreach (GameObject bossMeterPart in _bossMeterPartList)
		{
			MyObject.CurrentHP += bossMeterPart.CurrentHP;
			MyObject.MaxHP += bossMeterPart.MaxHP;
		}
	}

	private void UpdateExplo(float elapsedTime)
	{
		if (Utils.ChangeValueToTarget(ref _timer, 0f, elapsedTime))
		{
			_partList[_exploIndex].ChangePartState(_exploState);
			_exploIndex--;
			_timer = _exploDelay;
			if (_exploIndex < 0)
			{
				_state = 3;
				MyObject.Destroy();
			}
		}
	}
}
