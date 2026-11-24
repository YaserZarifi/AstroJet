using Rake2D.JnG;

namespace Rake2D.Script;

public class OnZeldorExit : ExitStateBehavior
{
	public override void RegisterEvents()
	{
		if (GameCore.IsEditorEnabled)
		{
			return;
		}
		foreach (Hero hero in GameJnG.HeroList)
		{
			if (hero.IsZeldorPanzerEnabled)
			{
				StartExit();
				return;
			}
		}
		MyObject.Destroy();
	}
}
