namespace Rake2D.Script;

public class HeroDeviceHullBoost : ItemBehaviorScript
{
	private PartData _effectData;

	private PartData _effectData2;

	private float _HPpercent;

	private float _duration;

	private bool _effectCreated;

	private float _timer;

	public override void Init()
	{
		base.Init();
		_effectData = GameCore.GetPartData("device_hull_boost");
		_effectData2 = GameCore.GetPartData("device_hull_boost_2");
	}

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		_duration = Param("duration");
		_HPpercent = Param("hp_percent", 100f) / 100f;
	}

	public override void RegisterEvents()
	{
		MyItem.OnActivated += OnActivated;
		_effectCreated = true;
	}

	private void OnActivated()
	{
		MyHero.AddHP(_HPpercent * MyHero.MyObject.MaxHP, 1);
		MyHero.UseMana(100f);
		MyHero.MyObject.GetChildByPartDataFlat(_effectData)?.Destroy();
		MyHero.MyObject.GetChildByPartDataFlat(_effectData2)?.Destroy();
		MyHero.MyObject.CreateChildByPartData(_effectData, createPhysBody: false);
		_effectCreated = false;
		_timer = 0.4f;
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (!_effectCreated && Utils.ChangeValueToTarget(ref _timer, 0f, elapsedTime))
		{
			_effectCreated = true;
			MyHero.MyObject.CreateChildByPartData(_effectData2, createPhysBody: false).LifeTime = _duration;
		}
	}
}
