namespace Rake2D.Script;

public class OnStateSpineAnim : ExitStateBehavior
{
	private int _trackNumber;

	private string[] _animNameArray;

	private IntRange _loopCountRange;

	private FloatRange _pauseRange;

	private bool _bNeverendingLoop;

	private int _loopCounter;

	private float _pauseTimer;

	public override void RegisterEvents()
	{
		_loopCounter = _loopCountRange.GetRandom();
		if (_loopCounter < 0)
		{
			_bNeverendingLoop = true;
		}
		else
		{
			_bNeverendingLoop = false;
		}
		MyObject.OnUpdate += UpdatePause;
	}

	public override void Init()
	{
		base.Init();
		_trackNumber = ParamInt("track");
		_animNameArray = ParamStringArray("anim");
		_loopCountRange = ParamIntRange("loop", 1);
		_pauseRange = ParamRange("pause", 0f, 0f) / 1000f;
	}

	private void StartAnim()
	{
		string randomItem = Utils.GetRandomItem(_animNameArray);
		MyObject.ResetSpineTrack(_trackNumber);
		if (MyObject.SetSpineAnimation(_trackNumber, randomItem, bLooped: false))
		{
			MyObject.OnSpineAnimCompleted += OnSpineAnimCompleted;
		}
		else
		{
			LogErr("Unable to set spine anim to '{0}'", randomItem);
		}
		MyObject.OnUpdate -= UpdatePause;
	}

	private void UpdatePause(float elapsedTime)
	{
		_pauseTimer -= elapsedTime;
		if (_pauseTimer <= 0f)
		{
			StartAnim();
		}
	}

	private void OnSpineAnimCompleted(int trackNumber, int loopCount)
	{
		if (trackNumber != _trackNumber)
		{
			return;
		}
		MyObject.OnSpineAnimCompleted -= OnSpineAnimCompleted;
		if (!_bNeverendingLoop)
		{
			_loopCounter--;
			if (_loopCounter <= 0)
			{
				MyObject.OnUpdate -= UpdatePause;
				StartExit();
				return;
			}
		}
		_pauseTimer = _pauseRange.GetRandom();
		MyObject.OnUpdate += UpdatePause;
	}
}
