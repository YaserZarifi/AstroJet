namespace Rake2D.Script;

public class OnLifeTimeExit : OnLifeTimeBehavior
{
	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyObject.OnUpdate += OnUpdate;
	}

	public override void Init()
	{
		base.Init();
		_exitTimeRange = ParamRange("exit_time", 1000f) / 1000f;
	}

	private void OnUpdate(float elapsedTime)
	{
		if (MyObject.Timer >= _exitTime)
		{
			MyObject.OnUpdate -= OnUpdate;
			StartExit();
			IsDone = true;
		}
	}
}
