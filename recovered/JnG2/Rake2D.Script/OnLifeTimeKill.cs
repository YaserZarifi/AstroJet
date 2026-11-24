namespace Rake2D.Script;

public class OnLifeTimeKill : OnLifeTimeBehavior
{
	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyObject.OnUpdate += OnUpdate;
	}

	public override void Init()
	{
		base.Init();
		if (HasParam("time"))
		{
			_exitTimeRange = ParamRange("time", 1000f) / 1000f;
		}
		else
		{
			_exitTimeRange = new FloatRange(MyObject.LifeTime - 0.033f);
		}
	}

	private void OnUpdate(float elapsedTime)
	{
		if (MyObject.Timer >= _exitTime)
		{
			MyObject.OnUpdate -= OnUpdate;
			IsDone = true;
			MyObject.Kill();
		}
	}
}
