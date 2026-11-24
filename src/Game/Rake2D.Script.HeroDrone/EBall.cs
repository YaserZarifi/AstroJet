using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script.HeroDrone;

public class EBall : DroneFollower
{
	private PartData _endObjectData;

	private PartData _startObjectData;

	public override void Init()
	{
		base.Init();
		_startObjectData = GameCore.GetPartData("eball_lightning_start");
		_endObjectData = GameCore.GetPartData("eball_lightning_end");
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyObject.OnLink += OnLink;
	}

	private void Fire(float damage)
	{
		HeroSlotEnum heroSlotEnum = MyObject.MyItem.MySlot.SlotType switch
		{
			HeroSlotEnum.top_gun => HeroSlotEnum.bottom_gun, 
			HeroSlotEnum.bottom_gun => HeroSlotEnum.top_gun, 
			HeroSlotEnum.front_gun => HeroSlotEnum.rear_gun, 
			HeroSlotEnum.rear_gun => HeroSlotEnum.front_gun, 
			_ => HeroSlotEnum.Undefined, 
		};
		GameObject gameObject = null;
		foreach (GameObject drone in MyObject.MyHero.DroneList)
		{
			BehaviorDataDrone behaviorDataDrone = drone.BehaviorData as BehaviorDataDrone;
			if (drone != MyObject && behaviorDataDrone != null && behaviorDataDrone.DroneType == DroneTypeEnum.EBall && drone.MyItem != null && drone.MyItem.MySlot.SlotType == heroSlotEnum)
			{
				gameObject = drone;
				break;
			}
		}
		GameObject gameObject2 = MyObject.CreateChildByPartData(_startObjectData, createPhysBody: false);
		if (gameObject2 != null)
		{
			gameObject2.MyItem = MyObject.MyItem;
			gameObject2.Damage = damage;
			LightningLine lightningLine = gameObject2.GetBehaviorScriptById("LightningLine") as LightningLine;
			if (gameObject != null)
			{
				_ = gameObject.WorldPosition;
				lightningLine.EndObject = gameObject.CreateChildByPartData(_endObjectData, createPhysBody: false);
			}
			else
			{
				_ = MyObject.WorldPosition + new Vector2(500f, -100f);
				lightningLine.EndObject = MyObject.MyHero.GetControlledObject().CreateChildByPartData(_endObjectData, createPhysBody: false);
			}
		}
	}

	protected override void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (collisionObject.TargetClass == TargetClass.Man)
		{
			MyObject.SetPosition(MyObject.WorldPosition + normal * 3f);
			collisionObject.Hit(MyObject.Damage * _hitDelay, DamageTypeEnum.Collision, MyObject);
			MyObject.IgnoreCollisionMode = 2;
			return;
		}
		MyObject.SetPosition(MyObject.WorldPosition + normal * 10f);
		if (_hitTimer <= 0f)
		{
			_hitTimer = _hitDelay;
			float damage = MyObject.Damage * _hitDelay * MyObject.Scale.X;
			if (MyObject.MyItem != null)
			{
				MyObject.MyItem.DoOnHitCollision(MyObject, collisionObject, contactPoint, normal, damage);
			}
			collisionObject.Hit(damage, DamageTypeEnum.Collision, MyObject);
		}
		else
		{
			MyObject.IgnoreCollisionMode = 3;
		}
	}

	private void OnLink(GameObject senderobject, string linkName, int linkValue)
	{
		if (Utils.Equals(linkName, "fire"))
		{
			Fire((float)linkValue / 100f);
		}
	}
}
