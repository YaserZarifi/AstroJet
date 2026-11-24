namespace Rake2D.Script;

public class HeroEquipmentKineticRecharger : ItemBehaviorScript
{
	private float _shieldRecoverySpeed;

	public override void StartInSlot()
	{
		_shieldRecoverySpeed = Param("shield_recovery");
		MyHero.OnShotStart += OnShotStart;
	}

	private void OnShotStart(GameObject go)
	{
		MyHero.AddShield(_shieldRecoverySpeed);
	}
}
