namespace Rake2D.Script;

public class FadeOut : ExitStateBehavior
{
	private float _alphaPecent;

	private float _fadeSpeed;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
		if (DoInEditor)
		{
			MyObject.OnEditorUpdate += Update;
		}
		_alphaPecent = (float)(int)MyObject.Alpha / 255f;
	}

	public override void Init()
	{
		base.Init();
		_fadeSpeed = Param("fade_speed", 1000f) / 1000f;
	}

	public void Update(float elapsedTime)
	{
		if (Utils.ChangeValueToTarget(ref _alphaPecent, 0f, _fadeSpeed * elapsedTime))
		{
			MyObject.OnUpdate -= Update;
			StartExit();
		}
		MyObject.SetFadeAlpha(_alphaPecent, bAffectChildren: true);
	}
}
