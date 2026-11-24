using System;
using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public class HeroDroneDeployerRammer : HeroDroneDeployer
{
	protected override GameObject Deploy()
	{
		string partDataId;
		if (MyItem.MySlot.SlotType != HeroSlotEnum.rear_gun)
		{
			partDataId = ((MyItem.Level < 2) ? "hero_drone_rammer_small" : "hero_drone_rammer_big");
		}
		else
		{
			partDataId = ((MyItem.Level < 2) ? "hero_drone_rammer_small_rear" : "hero_drone_rammer_big_rear");
		}
		GameObject gameObject = MyObject.MyLayer.CreateObjectByPartData(partDataId, MyObject.WorldPosition, Vector2.One, 0f, -1);
		if (gameObject != null)
		{
			BehaviorDataDrone behaviorDataDrone = new BehaviorDataDrone(DroneTypeEnum.Rammer)
			{
				MaxSpeed = Param("speed"),
				Acceleration = Param("acceleration") / 100f
			};
			SetOffset(behaviorDataDrone);
			gameObject.BehaviorData = behaviorDataDrone;
			gameObject.MyItem = MyItem;
			gameObject.SetVelocity(behaviorDataDrone.OffsetToHero + MyLevel.ScrollSpeed);
			gameObject.SetRotation(((behaviorDataDrone.OffsetToHero.Y > 0f) ? ((float)Math.PI / 2f) : (-(float)Math.PI / 2f)) - gameObject.WorldRotation);
		}
		return gameObject;
	}

	public override float GetFinalCollisionDPS()
	{
		return base.GetFinalCollisionDPS() * GetDamageFactor();
	}

	protected override void SetOffset(BehaviorDataDrone droneData)
	{
		switch (MyItem.MySlot.SlotType)
		{
		case HeroSlotEnum.top_gun:
			droneData.OffsetToHero = new Vector2(150f, -150f);
			break;
		case HeroSlotEnum.front_gun:
			droneData.OffsetToHero = new Vector2(250f, 0f);
			break;
		case HeroSlotEnum.bottom_gun:
			droneData.OffsetToHero = new Vector2(150f, 150f);
			break;
		case HeroSlotEnum.rear_gun:
			droneData.OffsetToHero = new Vector2(-200f, 0f);
			break;
		}
	}
}
