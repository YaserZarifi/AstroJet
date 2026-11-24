namespace Rake2D.Script;

public class HeroEquipmentShield : ItemBehaviorScript
{
	private GameObject _shieldObject;

	private PartData _shieldData;

	private float _oldShieldPercent;

	private float _oldShieldWaitTimer;

	public override void Init()
	{
		base.Init();
		_shieldData = GameCore.GetPartData("hero_shield");
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		_shieldObject = null;
		MyHero.OnBeforeApplyLoadout += OnBeforeApplyLoadout;
		MyHero.OnApplyLoadout += OnApplyLoadout;
	}

	private void OnBeforeApplyLoadout()
	{
		if (MyHero != null)
		{
			_oldShieldPercent = MyHero.ShieldPercent;
			_oldShieldWaitTimer = MyHero.ShieldWaitTimer;
		}
		else
		{
			_oldShieldPercent = 0f;
			_oldShieldWaitTimer = 0f;
		}
	}

	private void OnApplyLoadout()
	{
		MyHero.SetShield(MyHero.MaxShield * _oldShieldPercent);
		MyHero.ShieldWaitTimer = _oldShieldWaitTimer;
	}

	public override void Update(float elapsedTime)
	{
		if (MyHero.ShieldPercent <= 0f)
		{
			if (_shieldObject != null)
			{
				_shieldObject.Destroy();
				_shieldObject = null;
			}
		}
		else
		{
			if (_shieldObject == null)
			{
				_shieldObject = MyObject.CreateChildByPartData(_shieldData, createPhysBody: true);
			}
			_shieldObject.Alpha = (byte)(255f * MyHero.ShieldPercent);
		}
	}
}
