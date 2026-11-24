using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public class HeroDroneDeployerEBall : HeroDroneDeployer
{
	private float _fireRate;

	private float _fireTimer;

	private float _fireDamage;

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		_fireRate = Param("fire_rate", 1f);
		_fireDamage = Param("damage");
	}

	protected override GameObject Deploy()
	{
		string partDataId = "hero_drone_eball";
		GameObject gameObject = MyObject.MyLayer.CreateObjectByPartData(partDataId, MyObject.WorldPosition, Vector2.One, 0f, 1);
		if (gameObject != null)
		{
			BehaviorDataDrone behaviorDataDrone = new BehaviorDataDrone(DroneTypeEnum.EBall)
			{
				MaxSpeed = Param("speed"),
				Acceleration = Param("acceleration") / 100f
			};
			SetOffset(behaviorDataDrone);
			gameObject.BehaviorData = behaviorDataDrone;
			gameObject.MyItem = MyItem;
			gameObject.SetVelocity(behaviorDataDrone.OffsetToHero + MyLevel.ScrollSpeed);
		}
		return gameObject;
	}

	protected override void SetOffset(BehaviorDataDrone droneData)
	{
		switch (MyItem.MySlot.SlotType)
		{
		case HeroSlotEnum.top_gun:
			droneData.OffsetToHero = new Vector2(250f, -300f);
			break;
		case HeroSlotEnum.front_gun:
			droneData.OffsetToHero = new Vector2(500f, 0f);
			break;
		case HeroSlotEnum.bottom_gun:
			droneData.OffsetToHero = new Vector2(250f, 300f);
			break;
		case HeroSlotEnum.rear_gun:
			droneData.OffsetToHero = new Vector2(-500f, 0f);
			break;
		}
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (Utils.ChangeValueToTarget(ref _fireTimer, 0f, elapsedTime) && MyHero.KeyDown(GameInput.KeyTypeEnum.FireGuns))
		{
			SendLinkToAllDrones("fire", (int)(_fireDamage * 100f * GetDamageFactor()));
			_fireTimer = 1f / (_fireRate * FireRateFactor);
		}
	}

	public override bool TryGetParamSummary(string name, out float value, out float finalValue)
	{
		if (Utils.Equals(name, "dps"))
		{
			value = _fireDamage * _fireRate;
			finalValue = value * FireRateFactor * GetDamageFactor();
		}
		else
		{
			if (!Utils.Equals(name, "fire_rate"))
			{
				return base.TryGetParamSummary(name, out value, out finalValue);
			}
			value = _fireRate;
			finalValue = _fireRate * FireRateFactor;
		}
		return true;
	}
}
