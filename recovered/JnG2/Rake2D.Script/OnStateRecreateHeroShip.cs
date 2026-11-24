using Rake2D.JnG;

namespace Rake2D.Script;

public class OnStateRecreateHeroShip : BehaviorScript
{
	public override void RegisterEvents()
	{
		Hero hero = MyObject.MyHero ?? MyObject.GetMainPart().MyHero;
		if (hero != null)
		{
			hero.MyObject = null;
			hero.CreateSmallShip(MyLevel.MainLayer, MyObject.WorldPosition);
		}
	}
}
