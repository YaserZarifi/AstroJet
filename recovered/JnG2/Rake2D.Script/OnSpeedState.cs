namespace Rake2D.Script;

public class OnSpeedState : ExitStateBehavior
{
	private float _minSpeed;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		MyObject.OnEditorUpdate += OnUpdate;
	}

	public override void Init()
	{
		base.Init();
		_minSpeed = Param("min_speed", -1f);
	}

	private void OnUpdate(float elapsedTime)
	{
		float num = MyObject.Velocity.Length();
		if (_minSpeed >= 0f && num <= _minSpeed)
		{
			StartExit();
			MyObject.OnUpdate -= OnUpdate;
			MyObject.OnEditorUpdate -= OnUpdate;
		}
	}
}
