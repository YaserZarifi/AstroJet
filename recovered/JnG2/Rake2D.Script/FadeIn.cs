namespace Rake2D.Script;

public class FadeIn : ExitStateBehavior
{
	private float _alphaPecent;

	private float _fadeSpeed;

	private bool _fromZero;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
		if (DoInEditor)
		{
			MyObject.OnEditorUpdate += Update;
		}
		if (_fromZero)
		{
			_alphaPecent = 0f;
		}
		else
		{
			_alphaPecent = (float)(int)MyObject.Alpha / 255f;
		}
	}

	public override void Init()
	{
		base.Init();
		_fadeSpeed = Param("fade_speed", 1000f) / 1000f;
		_fromZero = ParamBool("from_zero");
	}

	public void Update(float elapsedTime)
	{
		if (Utils.ChangeValueToTarget(ref _alphaPecent, 1f, _fadeSpeed * elapsedTime))
		{
			MyObject.OnUpdate -= Update;
			MyObject.OnEditorUpdate -= Update;
			MyObject.SetFadeAlpha(_alphaPecent, bAffectChildren: true);
			StartExit();
		}
		else
		{
			MyObject.SetFadeAlpha(_alphaPecent, bAffectChildren: true);
		}
	}
}
