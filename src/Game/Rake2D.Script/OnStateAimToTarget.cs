using System.Collections.Generic;

namespace Rake2D.Script;

public class OnStateAimToTarget : BehaviorScript
{
	private TargetClass _targetClass;

	private TargetClass _targetClass2;

	private float _maxRadius;

	private FloatRange _aimArcRange;

	private bool _useAimLimit;

	public override void Init()
	{
		base.Init();
		_maxRadius = Param("max_radius", 500f);
		_aimArcRange = ParamRange("arc", 0f, 359f);
		_useAimLimit = ParamBool("aim_limit");
		string[] array = ParamStringArray("target_class");
		if (array != null)
		{
			_targetClass = DataUtils.GetTargetClassByStringArray(array);
		}
		else
		{
			_targetClass = TargetClass.AllEnemy;
		}
		array = ParamStringArray("target_class_2");
		if (array != null)
		{
			_targetClass2 = DataUtils.GetTargetClassByStringArray(array);
		}
		else
		{
			_targetClass2 = TargetClass.Man;
		}
	}

	public override void RegisterEvents()
	{
		List<GameObject> gameObjectList = GameCore.Allocator.GetGameObjectList();
		gameObjectList.Add(MyObject);
		List<GameObject> visibleObjectsInRangeInAllPhysLayers = MyLevel.GetVisibleObjectsInRangeInAllPhysLayers(MyObject.WorldPosition, _targetClass, _maxRadius, gameObjectList, bOlyHittable: true);
		if (visibleObjectsInRangeInAllPhysLayers.Count == 0)
		{
			GameCore.Allocator.DeleteAliasingGameObjectList(visibleObjectsInRangeInAllPhysLayers);
			visibleObjectsInRangeInAllPhysLayers = MyLevel.GetVisibleObjectsInRangeInAllPhysLayers(MyObject.WorldPosition, _targetClass2, _maxRadius, gameObjectList, bOlyHittable: true);
		}
		if (_useAimLimit)
		{
			for (int num = visibleObjectsInRangeInAllPhysLayers.Count - 1; num >= 0; num--)
			{
				int num2 = (int)Utils.RadToDeg(MyObject.GetRotationToTarget(visibleObjectsInRangeInAllPhysLayers[num]));
				num2 -= (int)Utils.RadToDeg(MyObject.WorldRotation) % 360;
				if ((float)num2 < _aimArcRange.From || (float)num2 > _aimArcRange.To)
				{
					visibleObjectsInRangeInAllPhysLayers.RemoveAt(num);
				}
			}
		}
		if (visibleObjectsInRangeInAllPhysLayers.Count == 0)
		{
			MyObject.SetRotation(Utils.DegToRad(_aimArcRange.GetRandom()));
		}
		else
		{
			float rotationToTarget = MyObject.GetRotationToTarget(Utils.GetRandomItem(visibleObjectsInRangeInAllPhysLayers));
			if (MyObject.ParentObject != null)
			{
				MyObject.SetRotation(rotationToTarget - MyObject.ParentObject.WorldRotation);
			}
			else
			{
				MyObject.SetRotation(rotationToTarget);
			}
		}
		GameCore.Allocator.DeleteAliasingGameObjectList(visibleObjectsInRangeInAllPhysLayers);
		GameCore.Allocator.DeleteAliasingGameObjectList(gameObjectList);
	}
}
