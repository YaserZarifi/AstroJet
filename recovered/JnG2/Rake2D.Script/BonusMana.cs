using Rake2D.JnG;

namespace Rake2D.Script;

public class BonusMana : BonusBehavior
{
	protected override void ApplyBonus(Hero hero)
	{
		hero.AddMana(_bonusValue, MyObject.WorldPosition, bTextParticle: true);
	}
}
