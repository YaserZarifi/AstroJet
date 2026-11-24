namespace Rake2D.Script;

public class OnScreenBase : ExitStateBehavior
{
	private IntRange _screenLeftRange;

	private IntRange _screenRightRange;

	private IntRange _screenTopRange;

	private IntRange _screenBottomRange;

	private float _screenLeft;

	private float _screenRight;

	private float _screenTop;

	private float _screenBottom;

	private float _beginTimer;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		_screenLeft = (float)_screenLeftRange.GetRandom() / 100f * 1920f;
		_screenRight = (float)_screenRightRange.GetRandom() / 100f * 1920f;
		_screenTop = (float)_screenTopRange.GetRandom() / 100f * 1080f;
		_screenBottom = (float)_screenBottomRange.GetRandom() / 100f * 1080f;
	}

	public override void Init()
	{
		base.Init();
		_screenLeftRange = ParamIntRange("screen_left", -9999);
		_screenRightRange = ParamIntRange("screen_right", -9999);
		_screenTopRange = ParamIntRange("screen_top", -9999);
		_screenBottomRange = ParamIntRange("screen_bottom", -9999);
		_beginTimer = Param("begin_delay") / 1000f;
	}

	private void OnUpdate(float elapsedTime)
	{
		if (MyObject.IsTriggerFree && Utils.ChangeValueToTarget(ref _beginTimer, 0f, elapsedTime))
		{
			if (_screenRightRange.From != -9999 && MyObject.VirtualScreenPosition.X >= _screenRight)
			{
				DoAction(1);
			}
			else if (_screenLeftRange.From != -9999 && MyObject.VirtualScreenPosition.X <= _screenLeft)
			{
				DoAction(3);
			}
			if (_screenBottomRange.From != -9999 && MyObject.VirtualScreenPosition.Y >= _screenBottom)
			{
				DoAction(2);
			}
			else if (_screenTopRange.From != -9999 && MyObject.VirtualScreenPosition.Y <= _screenTop)
			{
				DoAction(0);
			}
		}
	}

	protected virtual void DoAction(int position)
	{
		MyObject.OnUpdate -= OnUpdate;
	}
}
