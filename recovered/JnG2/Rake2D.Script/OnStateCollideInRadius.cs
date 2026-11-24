using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnStateCollideInRadius : BehaviorScript
{
	private TargetClass _targetClasses;

	private float _radius;

	public override void Init()
	{
		base.Init();
		_radius = Param("radius");
		_targetClasses = DataUtils.GetTargetClassByStringArray(ParamStringArray("target_class"));
	}

	public override void RegisterEvents()
	{
		Collide();
	}

	protected void Collide()
	{
		List<GameObject> physPartsInRangeInAllPhsLayers = MyLevel.GetPhysPartsInRangeInAllPhsLayers(MyObject.WorldPosition, _targetClasses, _radius);
		foreach (GameObject item in physPartsInRangeInAllPhsLayers)
		{
			item.DoOnCollision(MyObject, item.WorldPosition, new Vector2(0f, -1f));
		}
		GameCore.Allocator.DeleteAliasingGameObjectList(physPartsInRangeInAllPhsLayers);
	}
}
