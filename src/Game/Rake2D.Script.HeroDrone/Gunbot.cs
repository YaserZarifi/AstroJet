using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script.HeroDrone;

public class Gunbot : DroneBehavior
{
	private BehaviorDataDrone _behData;

	private PartData _hitPartData;

	private bool _isDetached;

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		if (MyObject.MyHero != null && _behData != null)
		{
			MyObject.OnUpdate += Update;
			MyObject.OnLink += OnLink;
			MyObject.OnCollision += OnCollision;
		}
	}

	public override void Init()
	{
		base.Init();
		if (MyObject.MyHero == null)
		{
			MyObject.MyHero = MyObject.FindNearestHero();
		}
		_hitDelay = 0.1f;
		_behData = MyObject.BehaviorData as BehaviorDataDrone;
		_hitPartData = GameCore.GetPartData("hit_drone");
	}

	protected void OnLink(GameObject senderobject, string eventName, int value)
	{
		if (Utils.Equals(eventName, "detach"))
		{
			_isDetached = true;
		}
		else if (Utils.Equals(eventName, "attach"))
		{
			_isDetached = false;
		}
	}

	protected override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (_isDetached)
		{
			MyObject.SetVelocity(0f, 0f);
		}
		else
		{
			Vector2 vector = MyObject.MyHero.MyObject.WorldPosition + Utils.RotateVector2(MyObject.MyHero.DroneCenterOffset * MyObject.MyHero.MyObject.Scale + _behData.OffsetToHero * MyObject.MyHero.DroneRadiusFactor * MyObject.MyHero.MyObject.Scale, MyObject.MyHero.MyObject.WorldRotation);
			float rotationTo = Utils.GetRotationTo(MyObject.WorldPosition, vector);
			float distance = Utils.GetDistance(MyObject.WorldPosition, vector);
			Vector2 velocity = (IsHeroInLevelAutopilotMode ? (Utils.GetVelocityVector2(Utils.Clamp(_behData.Acceleration * distance, 0f, Math.Max(_behData.MaxSpeed, MyObject.MyHero.MyObject.VelocityNoScroll.Length())), rotationTo) + MyLevel.ScrollSpeed) : (Utils.GetVelocityVector2(Utils.Clamp(_behData.Acceleration * distance, 0f, _behData.MaxSpeed), rotationTo) + MyLevel.ScrollSpeed));
			if (elapsedTime > 0f)
			{
				MyObject.SetVelocity(velocity);
			}
		}
		if (_rotateWithHero && _initState != MyObject.CurrentState)
		{
			MyObject.SetRotation(MyObject.MyHero.MyObject.WorldRotation);
		}
		_hitTimer -= elapsedTime;
	}

	protected void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		MyObject.SetPosition(MyObject.WorldPosition + normal * 5f);
		if (_hitTimer <= 0f)
		{
			_hitTimer = _hitDelay;
			float damage = MyObject.Damage * _hitTimer * MyObject.MyHero.MyObject.Scale.X;
			if (MyObject.MyItem != null)
			{
				MyObject.MyItem.DoOnHitCollision(MyObject, collisionObject, contactPoint, normal, damage);
			}
			collisionObject.Hit(damage, DamageTypeEnum.Collision, MyObject);
			MyLayer.CreateObjectByPartData(_hitPartData, contactPoint, new Vector2(0.8f), 0f, collisionObject.ZOrder);
		}
	}
}
