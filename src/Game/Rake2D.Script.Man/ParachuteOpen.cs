using Microsoft.Xna.Framework;

namespace Rake2D.Script.Man;

public class ParachuteOpen : BehaviorManParachuteBase
{
	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyObject.OnUpdate += OnUpdate;
		MyObject.OnAnimEnd += OnAnimEnd;
	}

	public override void Init()
	{
		base.Init();
		SetState(ManStateEnum.Fall, _direction);
		_stateTimer = Utils.GetRandom(0.4f, 0.6f);
		SetImmortal(isImmortal: true);
	}

	protected override void SetState(ManStateEnum state, ManDirectionEnum direction)
	{
		base.SetState(state, direction);
		_state = state;
	}

	protected void OnAnimEnd()
	{
		if (_state == ManStateEnum.ParachuteOpen)
		{
			_switchOffset = new Vector2(0f, -70f);
			MyObject.SwitchToObject("man_parachute", bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Destroy).SetVelocity(MyObject.VelocityX * 0.8f, MyObject.VelocityY / 3f);
		}
	}

	protected void OnUpdate(float elapsedTime)
	{
		if (!_isInitialized)
		{
			SetDirectionByFlip();
			_isInitialized = true;
		}
		if (_state == ManStateEnum.Fall)
		{
			if (MyObject.VelocityY >= 0f)
			{
				_stateTimer -= elapsedTime;
				SetImmortal(isImmortal: false);
			}
			if (_stateTimer < 0f)
			{
				SetState(ManStateEnum.ParachuteOpen, _direction);
				MyObject.SpriteRenderer.ChangeAnim("open", 0.4f);
				MyObject.PlaySound("man_para_open_0");
				MyObject.SpriteRenderer.Animator.IsLooped = false;
			}
		}
	}
}
