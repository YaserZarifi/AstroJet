namespace Rake2D.Script;

public class HeroDeviceEnergyBarrier : ItemBehaviorScript
{
	private GameObject _effectObject;

	private float _duration;

	private float _totalDPSFactor;

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		_duration = Param("duration");
		_totalDPSFactor = Param("total_dps_percent", 100f) / 100f;
	}

	public override void RegisterEvents()
	{
		MyItem.OnActivated += OnActivated;
	}

	private void OnActivated()
	{
		_effectObject?.Destroy();
		PartData partData = GameCore.GetPartData($"device_energy_barrier_{MyItem.Level}");
		_effectObject = MyHero.MyObject.CreateChildByPartData(partData, createPhysBody: true);
		_effectObject.LifeTime = _duration;
		_effectObject.Damage = MyHero.GetTotalDPS() * _totalDPSFactor;
		_effectObject.MyItem = MyItem;
		MyHero.UseMana(100f);
	}
}
