using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script.Man;

public class BehaviorManJetPackBase : BehaviorManBase
{
	protected PartData _shotData;

	protected FloatRange _fireDelayRange;

	protected float _shotSpeed;

	protected int _burstCount;

	protected float _burstTimer;

	protected IntRange _burstCountRange;

	protected FloatRange _burstDelayRange;

	protected float _shotBackForceFactor;

	private GameObject _nearestHeroObject;

	private Vector2 _thrust;

	private float _rotationToHero;

	private float _fireTimer;

	private GameObject _armObject;

	private bool _readyToFire;

	private bool _ejectMode;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		MyObject.OnShockHit += OnShockHit;
		MyObject.OnHitBegin += OnHitBegin;
		MyObject.OnAnimEnd += OnAnimEnd;
	}

	public override void Init()
	{
		base.Init();
		_armObject = MyObject.GetChildByIdFlat("arm");
		_ejectMode = ParamBool("eject_mode");
		SetImmortal(_ejectMode);
		_burstCountRange = ParamIntRange("burst");
		_burstDelayRange = ParamRange("burst_delay") / 1000f;
		_fireDelayRange = ParamRange("fire_delay") / 1000f;
		_shotSpeed = Param("shot_speed", 700f);
		SetDirectionByFlip();
		SetState(ManStateEnum.JetPackIdle, _direction);
		_fireTimer = _fireDelayRange.GetRandom();
		_stateTimer = Utils.GetRandom(0.2f, 0.35f);
		_thrust = new Vector2(Utils.GetRandom(-200, 200), Utils.GetRandom(-900, -1500));
	}

	protected override void SetState(ManStateEnum state, ManDirectionEnum direction)
	{
		base.SetState(state, direction);
		_state = state;
	}

	private void OnAnimEnd()
	{
		_readyToFire = true;
	}

	private void UpdateArm(float elapsedTime)
	{
		float rotation = _armObject.Rotation;
		if ((double)rotation >= -0.6 || rotation < -2f)
		{
			if (MyObject.Scale.X < 0f)
			{
				if (_rotationToHero < 0f)
				{
					SetDirection(ManDirectionEnum.West);
				}
			}
			else if (_rotationToHero > 0f)
			{
				SetDirection(ManDirectionEnum.East);
			}
		}
		if (MyObject.Scale.X < 0f)
		{
			MyObject.Rotation = -0.6f - rotation / 4f;
		}
		else
		{
			MyObject.Rotation = 0.6f + rotation / 4f;
		}
		if (_readyToFire && Utils.ChangeValueToTarget(ref _burstTimer, 0f, elapsedTime))
		{
			_fireTimer -= elapsedTime;
		}
		if (_fireTimer <= 0f)
		{
			_readyToFire = false;
			_armObject.SpriteRenderer.ChangeAnim("fire", _armObject.CurrentState.AnimDuration);
			_armObject.SpriteRenderer.Animator.IsLooped = false;
			Vector2 pos = _armObject.WorldPosition + Utils.GetVelocityVector2(16f, _armObject.WorldRotation) + new Vector2(0f, -3f);
			GameObject gameObject = MyObject.MyLayer.CreateObjectByPartData(_shotData, pos, Vector2.One, _armObject.WorldRotation, MyObject.ZOrder - 1);
			Vector2 vector = Utils.GetVelocityVector2(_shotSpeed, gameObject.Rotation) + MyLevel.ScrollSpeed * MyLayer.ScrollFactor;
			gameObject.SetVelocity(vector);
			MyObject.ApplyLinearImpulse(vector * -1f * _shotBackForceFactor);
			_burstCount--;
			if (_burstCount <= 0)
			{
				_fireTimer = _fireDelayRange.GetRandom();
				_burstCount = _burstCountRange.GetRandom();
				_burstTimer = _burstDelayRange.GetRandom();
			}
			else
			{
				_fireTimer = _burstDelayRange.GetRandom();
				_burstTimer = 0f;
			}
		}
	}

	protected void OnUpdate(float elapsedTime)
	{
		_nearestHeroObject = MyObject.FindNearestHeroObject(0);
		_rotationToHero = MyObject.GetRotationToTarget(_nearestHeroObject);
		if (_ejectMode)
		{
			if (!(MyObject.Velocity.Y >= 0f))
			{
				return;
			}
			_ejectMode = false;
			SetImmortal(isImmortal: false);
		}
		UpdateArm(elapsedTime);
		_stateTimer -= elapsedTime;
		switch (_state)
		{
		case ManStateEnum.JetPackIdle:
			if (_stateTimer <= 0f)
			{
				_thrust = new Vector2(Utils.GetRandom(-350, 350), Utils.GetRandom(-400, -930));
				_stateTimer = Utils.GetRandom(0.2f, 0.5f);
				if (Utils.GetRandom(3) == 0)
				{
					SetState(ManStateEnum.JetPackHunt, _direction);
				}
			}
			break;
		case ManStateEnum.JetPackHunt:
			if (_stateTimer <= 0f)
			{
				_stateTimer = Utils.GetRandom(0.5f, 1.5f);
				SetState(ManStateEnum.JetPackIdle, _direction);
			}
			_thrust = Utils.GetVelocityVector2(1000f, _rotationToHero) + new Vector2(0f, -800f);
			break;
		}
		if (_nearestHeroObject != null)
		{
			float distance = Utils.GetDistance(MyObject.WorldPosition, _nearestHeroObject.WorldPosition);
			if (distance < 300f)
			{
				_thrust = Utils.GetVelocityVector2(Math.Max(0f, 300f - distance) * 4f, _rotationToHero + (float)Math.PI) + new Vector2(0f, -600f);
			}
		}
		MyObject.ApplyForce(_thrust.X, _thrust.Y);
		Vector2 vector = MyLevel.ScrollSpeed * MyLayer.ScrollFactor;
		MyObject.SetVelocity(Utils.Clamp(MyObject.Velocity - vector, new Vector2(-200f, -150f), new Vector2(200f, 100f)) + vector);
	}

	protected void OnShockHit(DamageTypeEnum damageType, Vector2 physForce, GameObject hittingObject)
	{
		float num = Math.Max(Math.Abs(physForce.X), Math.Abs(physForce.Y));
		if (num > 1000f)
		{
			num = 1000f;
		}
		if (Utils.GetRandom(3) == 0)
		{
			MyObject.SwitchToObject("man_death_explo", bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Kill);
			return;
		}
		TurnToSalto(Utils.GetRandomVector2(physForce.X / 4f, (0f - num) / 2f, physForce.X, 0f - num), bUseOriginalVelocity: true);
		MyObject.Kill();
	}

	protected void OnHitBegin(ref float damage, DamageTypeEnum damageType, GameObject hittingObject)
	{
		if (!(damage <= 0f))
		{
			if (Utils.GetRandom(3) == 0)
			{
				MyObject.SwitchToObject("man_death_explo", bAssumeChildren: true, _switchOffset);
				return;
			}
			hittingObject.Velocity.Normalize();
			TurnToSalto(new Vector2(Utils.GetRandom(100, 600) * ((hittingObject.Velocity.X >= 0f) ? 1 : (-1)), Utils.GetRandom(-100, -600)), bUseOriginalVelocity: true);
		}
	}
}
