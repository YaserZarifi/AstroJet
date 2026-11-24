namespace Rake2D.Script;

public class OnActivateExit : ExitStateBehavior
{
	public override void RegisterEvents()
	{
		if (DoInEditor)
		{
			MyObject.OnActivate += OnActivate;
		}
	}

	private void OnActivate()
	{
		StartExit();
	}
}
