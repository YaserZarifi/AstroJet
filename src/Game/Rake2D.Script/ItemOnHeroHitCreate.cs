namespace Rake2D.Script;

public class ItemOnHeroHitCreate : ItemBehaviorCreateBase
{
	private bool _checkShield;

	public override void RegisterEvents()
	{
		MyHero.OnHitBegin += OnHitBegin;
	}

	public override void Init()
	{
		base.Init();
		_checkShield = ParamBool("check_shield");
	}

	private void OnHitBegin(ref float damage, DamageTypeEnum damageType, GameObject hittingObject)
	{
		if (!_checkShield || MyHero.Shield > 0f)
		{
			CreateObject(MyHero.MyObject.Position, MyObject);
		}
	}
}
