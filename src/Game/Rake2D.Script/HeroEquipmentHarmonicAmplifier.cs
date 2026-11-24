namespace Rake2D.Script;

public class HeroEquipmentHarmonicAmplifier : ItemBehaviorScript
{
	private GameObject _effectObject;

	private PartData _effectData;

	public override void Init()
	{
		base.Init();
		_effectData = GameCore.GetPartData("hero_harmonic_amplifier");
	}

	public override void StartInSlot()
	{
		base.StartInSlot();
		if (_effectObject != null)
		{
			_effectObject.Destroy();
			_effectObject = null;
		}
	}

	public override void Update(float elapsedTime)
	{
		if (MyHero.MaxShield > 0f && MyHero.Shield >= MyHero.MaxShield)
		{
			MyHero.DamageFactorHarmonicAmplifier = 1f + MyHero.MaxShield / 100f;
			if (_effectObject == null || !_effectObject.IsAlive)
			{
				_effectObject = MyHero.MyObject.CreateChildByPartData(_effectData, createPhysBody: true);
			}
			return;
		}
		MyHero.DamageFactorHarmonicAmplifier = 1f;
		if (_effectObject != null)
		{
			_effectObject.Destroy();
			_effectObject = null;
		}
	}
}
