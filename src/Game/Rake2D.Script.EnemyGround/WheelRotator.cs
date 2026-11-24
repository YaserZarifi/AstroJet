using System;
using System.Collections.Generic;

namespace Rake2D.Script.EnemyGround;

public class WheelRotator : BehaviorScript
{
	private List<GameObject> _wheelList;

	private List<GameObject> _dustList;

	private float _speedFactor;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
		MyObject.OnEditorUpdate += Update;
	}

	public override void Init()
	{
		base.Init();
		_speedFactor = Param("speed_factor", 100f) / 100f;
		string[] array = ParamStringArray("dust_part");
		if (!array.IsNullOrEmpty())
		{
			_dustList = GameCore.Allocator.GetGameObjectList();
			string[] array2 = array;
			foreach (string childId in array2)
			{
				GameObject childByIdDeep = MyObject.GetMainPart().GetChildByIdDeep(childId);
				if (childByIdDeep != null)
				{
					_dustList.Add(childByIdDeep);
				}
			}
		}
		_wheelList = GameCore.Allocator.GetGameObjectList();
		string text = ParamString("wheel_id", "wheel");
		for (int j = 0; j < 10; j++)
		{
			GameObject childByIdDeep2 = MyObject.GetMainPart().GetChildByIdDeep($"{text}{j + 1}");
			if (childByIdDeep2 == null)
			{
				break;
			}
			_wheelList.Add(childByIdDeep2);
		}
		if (_wheelList.Count == 0)
		{
			LogErr("No wheel part found (wheel_id = '{0}')", text);
		}
	}

	private void Update(float elapsedTime)
	{
		float num = MyObject.Velocity.Length() * elapsedTime * ((MyObject.Velocity.X > 0f) ? (0f - _speedFactor) : _speedFactor);
		if (!_dustList.IsNullOrEmpty())
		{
			foreach (GameObject dust in _dustList)
			{
				dust.Alpha = (byte)((float)(int)dust.CurrentState.Color.A * Utils.Clamp(Math.Abs(num), 0f, 1f));
				if (dust.Alpha == 0)
				{
					dust.IsEnabled = false;
				}
				else
				{
					dust.IsEnabled = true;
				}
			}
		}
		if (_wheelList.IsNullOrEmpty())
		{
			return;
		}
		if (MyObject.Scale.X < 0f)
		{
			num *= -1f;
		}
		foreach (GameObject wheel in _wheelList)
		{
			if (wheel.SpineConnector != null)
			{
				wheel.SpineConnector.SetAngle(wheel.SpineConnector.GetAngle() - num, bWorld: false);
			}
		}
	}
}
