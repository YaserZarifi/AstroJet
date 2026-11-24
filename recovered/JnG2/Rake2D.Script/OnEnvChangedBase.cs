namespace Rake2D.Script;

public abstract class OnEnvChangedBase : ExitStateBehavior
{
	protected EnvironmentEnum _env;

	public override void RegisterEvents()
	{
		MyObject.OnEnvironmentChanged += OnEvironmentChanged;
	}

	public override void Init()
	{
		base.Init();
		_env = DataUtils.GetEnvironmentByString(ParamString("env"));
	}

	private void OnEvironmentChanged(EnvironmentEnum env)
	{
		if (env == _env)
		{
			DoAction();
			StartExit();
		}
	}

	protected abstract void DoAction();
}
