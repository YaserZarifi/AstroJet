namespace Rake2D.Script.Man;

public class FallToJetpack : BehaviorManParachuteBase
{
	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyObject.OnUpdate += OnUpdate;
	}

	public override void Init()
	{
		base.Init();
		SetState(ManStateEnum.Fall, _direction);
		SetImmortal(isImmortal: true);
	}

	protected override void SetState(ManStateEnum state, ManDirectionEnum direction)
	{
		base.SetState(state, direction);
		_state = state;
	}

	protected void OnUpdate(float elapsedTime)
	{
		if (!_isInitialized)
		{
			SetDirectionByFlip();
			_isInitialized = true;
		}
		if (_state == ManStateEnum.Fall && MyObject.VelocityY >= 0f)
		{
			MyObject.SwitchToObject("man_jetpack_gun", bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Destroy).SetVelocity(MyObject.VelocityX * 0.8f, MyObject.VelocityY * 0.8f);
		}
	}
}
