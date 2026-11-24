namespace Rake2D.Script;

public class OnStateFinishLevel : FinishLevelBehavior
{
	public override void RegisterEvents()
	{
		if (DoInEditor)
		{
			FinishLevel();
		}
	}
}
