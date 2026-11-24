using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script.Man;

public class OnGround : BehaviorKillableMan
{
	private ManStateEnum _startState;

	private bool _no_weapons;

	protected float _fall_speed;

	protected float _jump_speed = 200f;

	public override void Init()
	{
		base.Init();
		_maxSpeed.X = Math.Abs(MyObject.Velocity.X);
		_fall_speed = Param("fall_speed", 450f);
		_no_weapons = ParamBool("no_weapons");
		switch (ParamInt("start_state"))
		{
		case 1:
			_startState = ManStateEnum.Walk;
			break;
		case 2:
			_startState = ManStateEnum.Run;
			break;
		case 3:
			_startState = ManStateEnum.Jump;
			break;
		}
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyObject.OnUpdate += Update;
		MyObject.OnCollision += OnCollision;
		MyObject.OnAnimEnd += OnAnimEnd;
		MyObject.GetMainPart().OnLink += OnLink;
	}

	protected override void SetState(ManStateEnum state, ManDirectionEnum direction)
	{
		base.SetState(state, direction);
		if (_state != state)
		{
			_state = state;
			switch (_state)
			{
			case ManStateEnum.Idle:
				MyObject.SpriteRenderer.Atlas = GameCore.GetSpriteAtlas("man_idle", logWhenNotFound: true);
				MyObject.SpriteRenderer.ChangeAnim("idle", Utils.GetRandom(0.4f, 0.7f));
				MyObject.SetVelocity(0f, MyObject.VelocityY);
				_stateTimer = Utils.GetRandom(0.75f, 3f);
				_maxSpeed = new Vector2(0f, 999f);
				break;
			case ManStateEnum.Walk:
				MyObject.SpriteRenderer.Atlas = GameCore.GetSpriteAtlas("man_walk", logWhenNotFound: true);
				MyObject.SpriteRenderer.ChangeAnim("walk", 0.4f);
				_stateTimer = Utils.GetRandom(1.75f, 3f);
				_maxSpeed = new Vector2(80f, 999f);
				break;
			case ManStateEnum.Run:
				MyObject.SpriteRenderer.Atlas = GameCore.GetSpriteAtlas("man_run", logWhenNotFound: true);
				MyObject.SpriteRenderer.ChangeAnim("run", 0.5f);
				_stateTimer = Utils.GetRandom(1.75f, 3f);
				_maxSpeed = new Vector2(200f, 999f);
				break;
			case ManStateEnum.Command:
				MyObject.SpriteRenderer.Atlas = GameCore.GetSpriteAtlas("man_command_gogo", logWhenNotFound: true);
				MyObject.SpriteRenderer.ChangeAnim("command", 0.5f);
				_stateTimer = 10f;
				_maxSpeed = new Vector2(0f, 999f);
				MyObject.PlaySound("man_command_" + Utils.GetRandom(3));
				break;
			case ManStateEnum.Jump:
				MyObject.SpriteRenderer.Atlas = GameCore.GetSpriteAtlas("man_jump", logWhenNotFound: true);
				MyObject.SpriteRenderer.ChangeAnim("jump", 0.4f);
				_maxSpeed = new Vector2(_maxSpeed.X, 999f);
				break;
			case ManStateEnum.Fall:
				MyObject.SpriteRenderer.Atlas = GameCore.GetSpriteAtlas("man_fall", logWhenNotFound: true);
				MyObject.SpriteRenderer.ChangeAnim("fall", 0.4f);
				_maxSpeed = new Vector2(_maxSpeed.X, 999f);
				MyObject.PlaySound("man_fall_" + Utils.GetRandom(4));
				MyObject.OnAnimEnd -= OnAnimEnd;
				break;
			case ManStateEnum.ParachuteOpen:
			case ManStateEnum.Parachute:
				break;
			}
		}
	}

	private void UpdateIdle(float elapsedTime)
	{
		if (_stateTimer <= 0f)
		{
			switch (Utils.GetRandom(3))
			{
			case 0:
				SetState(ManStateEnum.Run, BehaviorManBase.GetRandomDirection());
				break;
			case 1:
				SetState(ManStateEnum.Command, BehaviorManBase.GetRandomDirection());
				break;
			default:
				SetState(ManStateEnum.Walk, BehaviorManBase.GetRandomDirection());
				break;
			}
		}
	}

	private void UpdateWalk(float elapsedTime)
	{
		if (_stateTimer <= 0f)
		{
			switch (Utils.GetRandom(3))
			{
			case 0:
				SetState(ManStateEnum.Idle, _direction);
				break;
			case 1:
				SetState(ManStateEnum.Walk, BehaviorManBase.GetRandomDirection());
				break;
			case 2:
				SetState(ManStateEnum.Run, BehaviorManBase.GetRandomDirection());
				break;
			case 3:
				SetState(ManStateEnum.Command, BehaviorManBase.GetRandomDirection());
				break;
			}
		}
		ApplyDirectionForceX(500f);
	}

	private void UpdateRun(float elapsedTime)
	{
		if (_stateTimer <= 0f)
		{
			switch (Utils.GetRandom(3))
			{
			case 0:
				SetState(ManStateEnum.Idle, _direction);
				break;
			case 1:
				SetState(ManStateEnum.Walk, _direction);
				break;
			case 2:
				SetState(ManStateEnum.Run, BehaviorManBase.GetRandomDirection());
				break;
			case 3:
				SetState(ManStateEnum.Command, BehaviorManBase.GetRandomDirection());
				break;
			}
		}
		ApplyDirectionForceX(1500f);
	}

	protected void OnAnimEnd()
	{
		if (_state == ManStateEnum.Command)
		{
			if (Utils.GetRandom(5) == 0)
			{
				_state = ManStateEnum.Undefined;
				SetState(ManStateEnum.Command, _direction);
			}
			else
			{
				SetState(ManStateEnum.Run, _direction);
			}
		}
	}

	private void OnLink(GameObject senderobject, string eventName, int value)
	{
		if (Utils.Equals(eventName, "block_west") && _direction == ManDirectionEnum.West)
		{
			SwitchDirection();
		}
		else if (Utils.Equals(eventName, "block_east") && _direction == ManDirectionEnum.East)
		{
			SwitchDirection();
		}
	}

	protected override void Update(float elapsedTime)
	{
		_stateTimer -= elapsedTime;
		if (!_isInitialized)
		{
			if (_startState == ManStateEnum.Undefined)
			{
				switch (Utils.GetRandom(1))
				{
				case 0:
					SetState(ManStateEnum.Walk, BehaviorManBase.GetRandomDirection());
					break;
				case 1:
					SetState(ManStateEnum.Run, BehaviorManBase.GetRandomDirection());
					break;
				}
			}
			else
			{
				SetDirectionByFlip();
				SetState(_startState, _direction);
			}
			_isInitialized = true;
		}
		if (_state != ManStateEnum.Fall && MyObject.VelocityY >= _fall_speed)
		{
			SetState(ManStateEnum.Fall, _direction);
		}
		else if (_state != ManStateEnum.Jump && _state != ManStateEnum.Fall && MyObject.VelocityY >= _jump_speed)
		{
			SetState(ManStateEnum.Jump, _direction);
		}
		if (!_no_weapons && _stateTimer < 0f && (_state == ManStateEnum.Walk || _state == ManStateEnum.Run || _state == ManStateEnum.Idle))
		{
			switch (Utils.GetRandom(6))
			{
			case 0:
				MyObject.SwitchToObject("man_bazooka", bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Destroy);
				return;
			case 1:
				MyObject.SwitchToObject("man_gun", bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Destroy);
				return;
			}
		}
		switch (_state)
		{
		case ManStateEnum.Idle:
			UpdateIdle(elapsedTime);
			break;
		case ManStateEnum.Walk:
			UpdateWalk(elapsedTime);
			break;
		case ManStateEnum.Run:
			UpdateRun(elapsedTime);
			break;
		}
		base.Update(elapsedTime);
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		switch (_state)
		{
		case ManStateEnum.Jump:
			switch (Utils.GetRandom(1))
			{
			case 0:
				SetState(ManStateEnum.Walk, _direction);
				break;
			case 1:
				SetState(ManStateEnum.Run, _direction);
				break;
			}
			if (MyObject.VelocityY >= _jump_speed)
			{
				MyObject.VelocityY = _jump_speed / 2f;
			}
			break;
		case ManStateEnum.Fall:
			MyObject.SwitchToObject("man_impact", bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Kill);
			break;
		}
	}
}
