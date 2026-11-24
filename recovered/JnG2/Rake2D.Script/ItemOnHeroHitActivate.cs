namespace Rake2D.Script;

public class ItemOnHeroHitActivate : ItemBehaviorCreateBase
{
	public override void RegisterEvents()
	{
		MyHero.OnHitBegin += OnHitBegin;
	}

	private void OnHitBegin(ref float damage, DamageTypeEnum damageType, GameObject hittingObject)
	{
		MyItem.Activate();
	}
}
