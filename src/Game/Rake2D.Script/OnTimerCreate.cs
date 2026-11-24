using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnTimerCreate : CreateObjectBehavior
{
	private FloatRange _timerRange;

	private int _loopCount;

	private float _timer;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
		if (_doInEditor && GameCore.IsEditorEnabled)
		{
			MyObject.OnEditorUpdate += Update;
		}
	}

	public override void Init()
	{
		base.Init();
		_timerRange = ParamRange("timer", 1000f, 1000f) / 1000f;
		_loopCount = ParamRandomInt("loop", 9999);
		_timer = Utils.GetRandom(_timerRange);
	}

	public void Update(float elapsedTime)
	{
		_timer -= elapsedTime;
		if (!(_timer <= 0f))
		{
			return;
		}
		if (_loopCount <= 0)
		{
			MyObject.OnUpdate -= Update;
			MyObject.OnEditorUpdate -= Update;
			return;
		}
		if (_loopCount < 9999)
		{
			_loopCount--;
		}
		_timer = _timerRange.GetRandom();
		CreateObjectList(null, Vector2.Zero);
	}
}
