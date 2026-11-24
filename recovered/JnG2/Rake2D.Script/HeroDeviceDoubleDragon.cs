namespace Rake2D.Script;

public class HeroDeviceDoubleDragon : ItemBehaviorScript
{
	private float _damageFactor;

	private float _duration;

	private GameObject _effectObject;

	private PartData _effectData;

	public override void Init()
	{
		base.Init();
		_effectData = GameCore.GetPartData("hero_double_dragon");
	}

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		_damageFactor = Param("damage_factor", 200f) / 100f;
		_duration = Param("duration");
	}

	public override void RegisterEvents()
	{
		MyItem.OnActivated += OnActivated;
	}

	private void OnActivated()
	{
		_effectObject?.Destroy();
		_effectObject = MyHero.MyObject.GetChildByPartDataFlat(_effectData);
		if (_effectObject != null)
		{
			_effectObject.Destroy();
		}
		_effectObject = MyHero.MyObject.CreateChildByPartData(_effectData, createPhysBody: false);
		if (MyItem.Level > 0)
		{
			_effectObject.SetScale(1f + (float)MyItem.Level / 10f);
		}
		_effectObject.LifeTime = _duration;
		_effectObject.Damage = _damageFactor;
		MyHero.UseMana(100f);
	}

	public override void Update(float elapsedTime)
	{
		if (_effectObject != null && !_effectObject.IsAlive)
		{
			_effectObject.Destroy();
			_effectObject = null;
		}
	}
}
