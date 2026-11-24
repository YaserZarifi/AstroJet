using System;
using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script.EnemyGround;

public class Vehicle : BehaviorScript
{
	private float _force;

	private float _forceRotation;

	private float _maxSpeed;

	private bool _hasContact;

	private bool _flipOnStuck;

	private float _flipTimer;

	private Vector2 _rayOffset;

	private Vector2 _oldPosition;

	private int _stuckCount;

	private float _cotactTimer;

	private int _rayLength;

	private float _heroHuntFactor;

	private float _impactSpeed;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
	}

	public override void Init()
	{
		_force = Param("force", 1000f);
		_forceRotation = Utils.DegToRad(Param("force_angle", -90f));
		_maxSpeed = Param("max_speed", 100f);
		_heroHuntFactor = Param("hero_hunt") / 100f;
		_impactSpeed = Param("impact_speed", 300f);
		_flipOnStuck = ParamBool("flip_on_stuck");
		if (MyObject.RigidBody != null)
		{
			MyObject.RigidBody.AngularDamping = 0.9f;
		}
		_rayLength = ParamInt("contact_ray_length", -1);
		GameObject childByIdDeep = MyObject.GetChildByIdDeep("contact_point");
		if (childByIdDeep != null)
		{
			_rayOffset = childByIdDeep.Position;
			if (_rayLength < 0)
			{
				_rayLength = (int)childByIdDeep.Scale.Y;
			}
		}
		else if (_rayLength < 0)
		{
			_rayLength = 5;
		}
	}

	private void DoOnStuck()
	{
		if (_flipOnStuck)
		{
			MyObject.SetScale(MyObject.Scale.X * -1f, MyObject.Scale.Y);
			_flipTimer = 0.5f;
		}
		_stuckCount = 0;
	}

	private void Update(float elapsedTime)
	{
		if (!MyObject.IsTriggerFree)
		{
			return;
		}
		_flipTimer -= elapsedTime;
		if (_oldPosition == MyObject.WorldPosition && elapsedTime > 0f)
		{
			if (_stuckCount++ > 2 && _flipTimer <= 0f)
			{
				DoOnStuck();
			}
		}
		else
		{
			_oldPosition = MyObject.WorldPosition;
			_stuckCount = 0;
		}
		Vector2 start = Vector2.Transform(_rayOffset, MyObject.WorldMatrix);
		RayCastResult rayCastResult = MyLevel.GetRayCastResult(start, MyObject.WorldRotation + (float)Math.PI, (float)_rayLength * MyObject.Scale.Y, (PhysClass)5120);
		_hasContact = rayCastResult.TargetObject != null;
		if (_hasContact)
		{
			_cotactTimer = 0.1f;
		}
		else
		{
			_cotactTimer -= elapsedTime;
		}
		if ((!_hasContact && !(_cotactTimer > 0f)) || CheckImpact())
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		if (_heroHuntFactor != 0f)
		{
			Hero hero = MyObject.FindNearestHero();
			if (hero != null && hero.MyObject.WorldPosition.X > MyObject.WorldPosition.X)
			{
				num2 = (float)Math.PI;
				num = _heroHuntFactor;
			}
		}
		Vector2 velocityVector = Utils.GetVelocityVector2(_maxSpeed * MyObject.WorldScale.X, MyObject.WorldRotation + _forceRotation + num2);
		velocityVector.X += MyObject.MyLevel.ScrollSpeed.X * num;
		if (_force == 0f)
		{
			MyObject.SetVelocity(velocityVector);
		}
		else
		{
			MyObject.ApplyForce((velocityVector.X - MyObject.Velocity.X) * _force, (velocityVector.Y - MyObject.Velocity.Y) * _force);
		}
	}

	private bool CheckImpact()
	{
		if (_impactSpeed > 0f && MyObject.Velocity.Y > _impactSpeed)
		{
			MyObject.Kill();
			return true;
		}
		return false;
	}
}
