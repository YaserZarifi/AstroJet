namespace Rake2D.Script;

public class OnSpineEndScript : ExitStateBehavior
{
	private int _track;

	private float _safeTimer;

	private float _safeTimerValue;

	public override void RegisterEvents()
	{
		if (_doInEditor || !GameCore.IsEditorEnabled)
		{
			MyObject.OnSpineAnimCompleted += OnSpineAnimCompleted;
			if (_safeTimerValue > 0f)
			{
				_safeTimer = _safeTimerValue;
				MyObject.OnUpdate += OnUpdate;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		_track = ParamInt("track", MyObject.PartData.SpineTrack);
		_safeTimerValue = Param("safe_timer") / 1000f;
	}

	public virtual void DoAction()
	{
	}

	public void OnSpineAnimCompleted(int trackIndex, int loopCount)
	{
		if (trackIndex == _track)
		{
			DoAction();
		}
	}

	public void OnUpdate(float elapsedTime)
	{
		_safeTimer -= elapsedTime;
		if (_safeTimer <= 0f)
		{
			DoAction();
		}
	}
}
