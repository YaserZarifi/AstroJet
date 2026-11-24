namespace Rake2D.Script.HeroShot;

public class BeamHero : BehaviorBeamBase
{
	public override void Init()
	{
		base.Init();
		_hitTo = (PhysClass)43004984;
		if (MyObject.MyItem != null)
		{
			ItemWeapon itemWeapon = MyObject.MyItem.GetBehaviorByType(typeof(ItemWeapon)) as ItemWeapon;
			if (itemWeapon.ExtraHitTo != PhysClass.Undefined)
			{
				_hitTo |= itemWeapon.ExtraHitTo;
			}
		}
		MyObject.TargetClass = TargetClass.ShotHero;
	}
}
