namespace Rake2D.JnG;

public class HeroCheckPointInfo
{
	public float HP;

	public float Shield;

	public float Mana;

	public HeroCheckPointInfo(Hero hero)
	{
		HP = hero.MyObject.CurrentHP;
		Shield = hero.Shield;
		Mana = hero.Mana;
	}
}
