namespace Rake2D.Script;

public abstract class HeroDeviceManaPerSec : ItemBehaviorScript
{
	protected float _manaPerSec;

	protected bool _isEnabled;

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		_manaPerSec = Param("mana_per_sec");
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (_isEnabled)
		{
			float num = _manaPerSec * elapsedTime;
			if (MyHero.Mana >= num)
			{
				MyHero.UseMana(num);
				return;
			}
			MyHero.SetMana(0f);
			OnManaOut();
		}
	}

	public abstract void OnManaOut();
}
