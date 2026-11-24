using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class ScaleFader : ExitStateBehavior
{
	private Vector2 _originalScale;

	private Vector2 _deltaScale;

	private Vector2 _targetScale;

	private float _duration;

	private float _timer;

	public override void Init()
	{
		base.Init();
		_duration = Param("duration", 1000f) / 1000f;
		_targetScale = ParamVector2("scale", 200f, 200f) / 100f;
	}

	public override void RegisterEvents()
	{
		if (DoInEditor)
		{
			MyObject.OnEditorUpdate += OnUpdate;
		}
		MyObject.OnUpdate += OnUpdate;
		_timer = 0f;
		_originalScale = MyObject.Scale;
		_deltaScale = _targetScale - _originalScale;
	}

	private void OnUpdate(float elapsedTime)
	{
		if (Utils.ChangeValueToTarget(ref _timer, _duration, elapsedTime))
		{
			StartExit();
		}
		MyObject.SetScale(_originalScale + _deltaScale * (_timer / _duration));
	}
}
