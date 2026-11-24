using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script.Man;

public class BehaviorManFireBase : BehaviorKillableMan
{
	protected GameObject _armObject;

	protected SpriteAtlas _showHideAtlas;

	protected SpriteAtlas _stanceAtlas;

	protected float _targetArmRotation;

	protected float _fireTimer;

	protected int _burstCount;

	protected float _burstTimer;

	protected IntRange _burstCountRange;

	protected FloatRange _burstDelayRange;

	protected PartData _shotData;

	protected FloatRange _fireDelayRange;

	protected float _shotSpeed;

	protected int _hideWeaponProbability;

	protected string _jetpackId;

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyObject.OnUpdate += Update;
		MyObject.OnKill += OnKill;
		MyObject.OnAnimEnd += OnAnimEnd;
	}

	public override void Init()
	{
		base.Init();
		_stanceAtlas = GameCore.GetSpriteAtlas("man_fire_stance", logWhenNotFound: true);
		_armObject = MyObject.GetChildByIdFlat("arm");
		_shotSpeed = Param("shot_speed", 700f);
		_burstCountRange = ParamIntRange("burst");
		_burstDelayRange = ParamRange("burst_delay") / 1000f;
		_fireDelayRange = ParamRange("fire_delay") / 1000f;
	}

	protected override void SetState(ManStateEnum state, ManDirectionEnum direction)
	{
		base.SetState(state, direction);
		_state = state;
		switch (_state)
		{
		case ManStateEnum.WeaponShow:
			MyObject.SpriteRenderer.Atlas = _showHideAtlas;
			MyObject.SpriteRenderer.ChangeAnim("show", 0.75f);
			MyObject.SpriteRenderer.Animator.IsLooped = false;
			_armObject.SetRotation(-(float)Math.PI / 2f);
			_armObject.IsEnabled = false;
			break;
		case ManStateEnum.WeaponHide:
			MyObject.SpriteRenderer.Atlas = _showHideAtlas;
			MyObject.SpriteRenderer.ChangeAnim("hide", 0.75f);
			MyObject.SpriteRenderer.Animator.IsLooped = false;
			_armObject.IsEnabled = false;
			break;
		case ManStateEnum.Idle:
			MyObject.SpriteRenderer.Atlas = _stanceAtlas;
			MyObject.SpriteRenderer.ChangeAnim("idle", 0.5f);
			_armObject.IsEnabled = true;
			_armObject.SpriteRenderer.ChangeAnim("aim", 0.4f);
			_fireTimer = _fireDelayRange.GetRandom();
			_burstCount = _burstCountRange.GetRandom() - 1;
			break;
		case ManStateEnum.Fire:
		{
			_armObject.SpriteRenderer.ChangeAnim("fire", _armObject.CurrentState.AnimDuration);
			_armObject.SpriteRenderer.Animator.IsLooped = false;
			Vector2 velocityVector = Utils.GetVelocityVector2(20f, _armObject.WorldRotation);
			MyObject.MyLayer.CreateObjectByPartData(_shotData, new Vector2(_armObject.WorldPosition.X, _armObject.WorldPosition.Y) + velocityVector, Vector2.One, _armObject.WorldRotation, MyObject.ZOrder - 1).SetVelocity(Utils.GetVelocityVector2(_shotSpeed, _armObject.WorldRotation));
			_burstTimer = _burstDelayRange.GetRandom();
			break;
		}
		}
	}

	private float AimToHero(float elapsedTime)
	{
		_targetArmRotation = _armObject.GetRotationToNearestHero();
		float currentValue = _armObject.Rotation;
		if (MyObject.Scale.X < 0f)
		{
			_targetArmRotation *= -1f;
		}
		Utils.ChangeValueToTarget(ref currentValue, _targetArmRotation, 5f * elapsedTime);
		if (currentValue < Utils.DegToRad(-120f))
		{
			currentValue = Utils.DegToRad(-120f);
		}
		else if (currentValue > Utils.DegToRad(120f))
		{
			currentValue = Utils.DegToRad(120f);
		}
		if (currentValue > 0f && _targetArmRotation > 0f)
		{
			if (MyObject.Scale.X < 0f)
			{
				SetDirection(ManDirectionEnum.West);
			}
			else
			{
				SetDirection(ManDirectionEnum.East);
			}
		}
		else if (_targetArmRotation < 0f)
		{
			if (MyObject.Scale.X < 0f)
			{
				SetDirection(ManDirectionEnum.East);
			}
			else
			{
				SetDirection(ManDirectionEnum.West);
			}
		}
		_armObject.SetRotation(currentValue);
		return currentValue;
	}

	protected void OnAnimEnd()
	{
		switch (_state)
		{
		case ManStateEnum.WeaponShow:
			if (Utils.GetRandom(5) == 0)
			{
				MyObject.SwitchToObject(_jetpackId, bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Destroy).Position.Y -= 16f;
			}
			else
			{
				SetState(ManStateEnum.Idle, _direction);
			}
			break;
		case ManStateEnum.WeaponHide:
			MyObject.SwitchToObject("man", bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Destroy);
			break;
		case ManStateEnum.Command:
			break;
		}
	}

	protected override void Update(float elapsedTime)
	{
		if (!_isInitialized)
		{
			SetDirectionByFlip();
			SetState(ManStateEnum.WeaponShow, _direction);
			_isInitialized = true;
		}
		switch (_state)
		{
		case ManStateEnum.Idle:
		{
			_stateTimer -= elapsedTime;
			float num = AimToHero(elapsedTime);
			_fireTimer -= elapsedTime;
			if (_fireTimer <= 0f && Math.Abs(_targetArmRotation - num) < 1f)
			{
				SetState(ManStateEnum.Fire, _direction);
			}
			break;
		}
		case ManStateEnum.Fire:
			AimToHero(elapsedTime);
			if (!Utils.ChangeValueToTarget(ref _burstTimer, 0f, elapsedTime))
			{
				break;
			}
			if (_burstCount-- <= 0)
			{
				if (Utils.GetRandom(99) < _hideWeaponProbability)
				{
					SetState(ManStateEnum.WeaponHide, _direction);
				}
				else
				{
					SetState(ManStateEnum.Idle, _direction);
				}
			}
			else
			{
				SetState(ManStateEnum.Fire, _direction);
			}
			break;
		}
	}

	protected void OnKill()
	{
		_armObject.Destroy();
	}
}
