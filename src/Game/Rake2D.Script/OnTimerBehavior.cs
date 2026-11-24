namespace Rake2D.Script;

public abstract class OnTimerBehavior : ExitStateBehavior
{
	protected FloatRange _delayRange;

	protected float _timer;

	protected float _lastDelay;

	protected int _loopCount;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		if (_doInEditor)
		{
			MyObject.OnEditorUpdate += OnUpdate;
		}
		_timer = Utils.GetRandom(_delayRange);
		_lastDelay = _timer;
		_loopCount = ParamRandomInt("loop", 1);
	}

	public override void Init()
	{
		base.Init();
		_delayRange = ParamRange("timer") / 1000f;
	}

	private void OnUpdate(float elapsedTime)
	{
		if (_loopCount > 0)
		{
			_timer -= elapsedTime;
			if (_timer <= 0f)
			{
				_loopCount--;
				_timer = Utils.GetRandom(_delayRange);
				DoAction();
				_lastDelay = _timer;
			}
		}
		if (_loopCount <= 0)
		{
			MyObject.OnUpdate -= OnUpdate;
			MyObject.OnEditorUpdate -= OnUpdate;
			StartExit();
		}
	}

	protected virtual void DoAction()
	{
	}
}
