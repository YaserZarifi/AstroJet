using System.Collections.Generic;

namespace Rake2D.Script;

public class ChooseTargetPart : BehaviorScript
{
	private List<GameObject> _targetPartList;

	private bool _forMain;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
	}

	public override void Init()
	{
		base.Init();
		string[] array = ParamStringArray("target_part");
		_forMain = ParamBool("for_main");
		_targetPartList = new List<GameObject>();
		if (array.IsNullOrEmpty())
		{
			return;
		}
		GameObject gameObject = (_forMain ? MyObject.GetMainPart() : MyObject);
		string[] array2 = array;
		foreach (string childId in array2)
		{
			GameObject childByIdDeep = gameObject.GetChildByIdDeep(childId);
			if (childByIdDeep != null)
			{
				_targetPartList.Add(childByIdDeep);
			}
		}
		if (_targetPartList.Count > 0)
		{
			MyObject.TargetPart = _targetPartList[0];
		}
	}

	private GameObject GetChildTargetPart(GameObject go)
	{
		if (go.TargetPart != null && go.TargetPart != go)
		{
			return GetChildTargetPart(go.TargetPart);
		}
		return go;
	}

	private void OnUpdate(float elapsedTime)
	{
		GameObject gameObject = (_forMain ? MyObject.GetMainPart() : MyObject);
		if (gameObject.TargetPart != null && gameObject.TargetPart.IsAlive && gameObject.TargetPart.IsEnabled && gameObject.TargetPart.IsVisible)
		{
			return;
		}
		gameObject.TargetPart = null;
		for (int num = _targetPartList.Count - 1; num >= 0; num--)
		{
			if (!_targetPartList[num].IsAlive)
			{
				_targetPartList.RemoveAt(num);
			}
		}
		for (int i = 0; i < _targetPartList.Count; i++)
		{
			GameObject childTargetPart = GetChildTargetPart(_targetPartList[i]);
			if (childTargetPart.IsEnabled && childTargetPart.IsVisible && !childTargetPart.IsImmortal)
			{
				gameObject.TargetPart = childTargetPart;
				break;
			}
		}
	}
}
