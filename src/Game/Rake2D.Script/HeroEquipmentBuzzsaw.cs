namespace Rake2D.Script;

public class HeroEquipmentBuzzsaw : ItemBehaviorScript
{
	public override void StartInSlot()
	{
		MyHero.IsBuzzsawEnabled = true;
		MyHero.DroneCollisionDamageFactor += Param("damage_factor") / 100f;
	}
}
