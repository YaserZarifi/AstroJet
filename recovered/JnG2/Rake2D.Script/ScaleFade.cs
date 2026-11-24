using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class ScaleFade : ExitStateBehavior
{
	private Vector2 _targetScale;

	private float _fadeSpeed;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
		if (DoInEditor)
		{
			MyObject.OnEditorUpdate += Update;
		}
	}

	public override void Init()
	{
		base.Init();
		_fadeSpeed = Param("fade_speed", 100f) / 100f;
		_targetScale = ParamVector2("target_scale", 200f) / 100f;
	}

	public void Update(float elapsedTime)
	{
		Vector2 currentValue = MyObject.Scale;
		if (Utils.ChangeValueToTarget(ref currentValue, _targetScale, _fadeSpeed * elapsedTime))
		{
			MyObject.OnUpdate -= Update;
			MyObject.OnEditorUpdate -= Update;
			MyObject.SetScale(currentValue);
			StartExit();
		}
		else
		{
			MyObject.SetScale(currentValue);
		}
	}
}
