using System.Collections.Generic;

namespace Rake2D.Script;

public class OnStateImpulsInRadius : ExitStateBehavior
{
	private TargetClass _targetClass;

	private float _radius;

	private Vector2Range _impulseRange;

	public override void RegisterEvents()
	{
		DoImpulse();
		StartExit();
	}

	public override void Init()
	{
		base.Init();
		_radius = Param("radius", 500f);
		_impulseRange = ParamVector2Range("impulse", 0f, -5f, 0f, -10f);
		string[] array = ParamStringArray("target_class");
		if (array != null)
		{
			_targetClass = DataUtils.GetTargetClassByStringArray(array);
		}
		else
		{
			_targetClass = TargetClass.AllEnemy;
		}
	}

	private void DoImpulse()
	{
		List<GameObject> visibleObjectsInRangeInAllPhysLayers = MyLevel.GetVisibleObjectsInRangeInAllPhysLayers(MyObject.WorldPosition, _targetClass, _radius, null, bOlyHittable: true);
		foreach (GameObject item in visibleObjectsInRangeInAllPhysLayers)
		{
			if (item.IsDynamic)
			{
				item.ApplyLinearImpulse(item.RigidBody.Mass * _impulseRange.GetRandom());
			}
		}
		GameCore.Allocator.DeleteAliasingGameObjectList(visibleObjectsInRangeInAllPhysLayers);
	}
}
