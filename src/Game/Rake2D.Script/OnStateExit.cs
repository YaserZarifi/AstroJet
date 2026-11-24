namespace Rake2D.Script;

public class OnStateExit : ExitStateBehavior
{
	public override void RegisterEvents()
	{
		if (DoInEditor)
		{
			StartExit();
		}
	}

	public override void Init()
	{
		base.Init();
	}
}
