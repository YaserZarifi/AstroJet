namespace Rake2D.Script;

public class OnActivateNoBulletTime : BehaviorScript
{
	private bool _bEnabled;

	public override void Init()
	{
		base.Init();
		_bEnabled = ParamBool("enabled", defaultValue: true);
	}

	public override void RegisterEvents()
	{
		MyObject.OnActivate += OnActivate;
	}

	private void OnActivate()
	{
		MyObject.OnActivate -= OnActivate;
		MyLevel.NoBulletTime = _bEnabled;
		IsDone = true;
	}
}
