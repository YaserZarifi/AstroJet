namespace Rake2D.Script;

public class HeroEquipmentCriticalHit : ItemBehaviorScript
{
	private float _probability;

	private float _powerFactor;

	public override void RegisterEvents()
	{
		MyHero.OnShotStart += OnShotStart;
	}

	public override void Init()
	{
		_probability = Param("probability", 10f);
		_powerFactor = Param("power_factor") / 100f;
	}

	private void OnShotStart(GameObject go)
	{
	}
}
