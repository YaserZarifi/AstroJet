using System;
using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script.HeroDrone;

public class DroneFollower : DroneBehavior
{
	protected BehaviorDataDrone _behData;

	protected PartData _hitPartData;

	public override void Init()
	{
		base.Init();
		_behData = MyObject.BehaviorData as BehaviorDataDrone;
		_hitPartData = GameCore.GetPartData("hit_drone");
		if (MyObject.MyHero == null)
		{
			MyObject.MyHero = MyObject.FindNearestHero();
		}
		_hitDelay = 0.2f;
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		if (MyObject.MyHero != null && _behData != null)
		{
			MyObject.OnUpdate += Update;
			MyObject.OnCollision += OnCollision;
		}
	}

	protected virtual void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
	}

	protected override void Update(float elapsedTime)
	{
		Vector2 vector;
		if (MyObject.MyHero.IsJetPackActive && MyObject.MyItem.MySlot.SlotType == HeroSlotEnum.front_gun)
		{
			GameObject controlledObject = MyObject.MyHero.GetControlledObject();
			vector = controlledObject.WorldPosition + Utils.RotateVector2(_behData.OffsetToHero * MyObject.MyHero.DroneRadiusFactor * controlledObject.Scale, MyObject.MyHero.MyObject.WorldRotation);
		}
		else
		{
			vector = MyObject.MyHero.MyObject.WorldPosition + Utils.RotateVector2(MyObject.MyHero.DroneCenterOffset * MyObject.MyHero.MyObject.Scale + _behData.OffsetToHero * MyObject.MyHero.DroneRadiusFactor * MyObject.MyHero.MyObject.Scale, MyObject.MyHero.MyObject.WorldRotation);
		}
		if (MyObject.MyHero != null && MyObject.MyHero.AutopilotMode == HeroAutopilotMode.None)
		{
			vector = Utils.Clamp(vector, new Vector2(MyLevel.Camera.Position.X, 0f), new Vector2(MyLevel.Camera.Position.X + 1920f, MyLevel.Size.Y));
		}
		float rotationTo = Utils.GetRotationTo(MyObject.WorldPosition, vector);
		float distance = Utils.GetDistance(MyObject.WorldPosition, vector);
		Vector2 velocity = (IsHeroInLevelAutopilotMode ? (Utils.GetVelocityVector2(Utils.Clamp(_behData.Acceleration * distance, 0f, Math.Max(_behData.MaxSpeed, MyObject.MyHero.MyObject.VelocityNoScroll.Length())), rotationTo) + MyLevel.ScrollSpeed) : (Utils.GetVelocityVector2(Utils.Clamp(_behData.Acceleration * distance, 0f, _behData.MaxSpeed), rotationTo) + MyLevel.ScrollSpeed));
		MyObject.SetVelocity(velocity);
		MyObject.IgnoreCollisionMode = 0;
		if (_rotateWithHero && _initState != MyObject.CurrentState)
		{
			MyObject.SetRotation(MyObject.MyHero.MyObject.WorldRotation);
		}
		_hitTimer -= elapsedTime;
		base.Update(elapsedTime);
	}
}
