namespace Rake2D.Script;

public class SpineAnimLooper : ExitStateBehavior
{
	private float _loopTimer;

	private int _loopCount;

	private IntRange _loopCountRange;

	private FloatRange _beginDelayRange;

	private FloatRange _loopDelayRange;

	private bool _newLoop;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		MyObject.OnSpineAnimCompleted += OnSpineAnimCompleted;
		_loopCount = _loopCountRange.GetRandom();
		_loopTimer = _beginDelayRange.GetRandom();
		StartNewLoop();
	}

	public override void Init()
	{
		base.Init();
		_beginDelayRange = ParamRange("begin_delay") / 1000f;
		_loopDelayRange = ParamRange("loop_delay") / 1000f;
		_loopCountRange = ParamIntRange("loop_count", 999);
	}

	private void OnUpdate(float elapsedTime)
	{
		if (_newLoop)
		{
			_loopTimer -= elapsedTime;
			if (_loopTimer <= 0f)
			{
				StartNewLoop();
			}
		}
	}

	private void StartNewLoop()
	{
		string name = ParamRandomString("anim_id", MyObject.CurrentState.SpineAnimId);
		MyObject.GetCurrentSpineRenderer().SetAnimation(MyObject.PartData.SpineTrack, name, loop: false);
		_newLoop = false;
	}

	private void OnSpineAnimCompleted(int trackIndex, int loopCount)
	{
		if (trackIndex == MyObject.PartData.SpineTrack)
		{
			if ((_loopCountRange.From == 999 && _loopCountRange.From == _loopCountRange.To) || --_loopCount > 0)
			{
				_loopTimer = _loopDelayRange.GetRandom();
				_newLoop = true;
			}
			else
			{
				MyObject.OnUpdate -= OnUpdate;
				StartExit();
			}
		}
	}
}
