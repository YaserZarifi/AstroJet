using Rake2D.JnG;

namespace Rake2D.Script;

public class BonusIce : BonusBehavior
{
	protected override void ApplyBonus(Hero hero)
	{
		if (hero.RemoveHeat(_bonusValue) < 0f)
		{
			hero.AddNoHeatTimer(5f);
		}
	}
}
