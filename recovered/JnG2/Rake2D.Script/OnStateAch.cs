namespace Rake2D.Script;

public class OnStateAch : AchBehavior
{
	public override void RegisterEvents()
	{
		if (DoInEditor)
		{
			UnlockAch();
		}
	}
}
