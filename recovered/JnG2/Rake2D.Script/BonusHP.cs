using System;
using Rake2D.JnG;

namespace Rake2D.Script;

public class BonusHP : BonusBehavior
{
	protected override void ApplyBonus(Hero hero)
	{
		float num = _bonusValue;
		if (hero.IsZeldorPanzerEnabled)
		{
			num = Math.Min(1f, num / 10f);
		}
		if (hero.IsQuazitronEnabled)
		{
			hero.AddMana(num, MyObject.WorldPosition, bTextParticle: true);
		}
		else
		{
			hero.AddHP(num, _textMode);
		}
	}
}
