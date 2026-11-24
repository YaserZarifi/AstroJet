namespace Rake2D.Script;

public class HeroWeaponNapalmator : ItemWeapon
{
	public override void RecalculateParams()
	{
		base.RecalculateParams();
		ShotStreamData shotStreamData;
		if ((shotStreamData = AddStreamData()) != null)
		{
			shotStreamData.SoundId = "machine_gun";
			shotStreamData.IsSoundLooped = true;
			shotStreamData.GunFireId = "hero_gun_fire_basic";
		}
	}
}
