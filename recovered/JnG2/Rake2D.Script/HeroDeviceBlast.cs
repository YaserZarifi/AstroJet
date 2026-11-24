namespace Rake2D.Script;

public abstract class HeroDeviceBlast : ManaDeviceBehaviorScript
{
	protected float _shotTimer;

	protected float _activateTimer;

	protected float _shotCounter;

	protected float _totalDamageFactor;

	protected float _shotSpeed;

	protected int _zorder;

	protected PartData _partData;

	protected float[] _rotationArray;

	protected float _shotDelay;

	protected float _activateDelay;

	protected int _shotCount;

	protected DamageTypeEnum _damageType;

	protected PartData _startObjectData;

	protected bool asChild;

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyItem.OnActivated += OnActivated;
	}

	public override void Init()
	{
		base.Init();
		asChild = ParamBool("as_child");
	}

	public override void StartInSlot()
	{
		base.StartInSlot();
		_partData = GameCore.GetPartData(ParamString("shot_id"));
		_totalDamageFactor = Param("total_damage_percent") / 100f;
		_shotDelay = Param("shot_delay", 200f) / 1000f;
		_activateDelay = Param("activate_delay", 500f) / 1000f;
		_shotCount = ParamInt("shot_count", 1);
		_shotSpeed = Param("shot_speed", 1000f);
		_zorder = ParamInt("zorder", 1);
		_damageType = DataUtils.GetDamageTypeByString(ParamString("damage_type", "energy"));
		string text = ParamString("start_id");
		if (text != null)
		{
			_startObjectData = GameCore.GetPartData(text);
		}
		_shotTimer = 0f;
		_activateTimer = _activateDelay;
		_isActive = false;
	}

	public virtual void OnActivated()
	{
		if (!_isActive)
		{
			MyHero.UseMana(_mana);
			_shotCounter = _shotCount;
			_isActive = true;
			_activateTimer = _activateDelay;
		}
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		Utils.ChangeValueToTarget(ref _shotTimer, 0f, elapsedTime);
		if (_shotCounter > 0f && _isActive)
		{
			if (!(_shotTimer <= 0f))
			{
				return;
			}
			_shotCounter--;
			if (_startObjectData != null)
			{
				MySlot.MyObject.CreateChildByPartData(_startObjectData, createPhysBody: false);
			}
			_shotTimer = _shotDelay;
			float num = (MyHero.IsShrinkerEnabled ? 0.5f : 1f);
			for (int i = 0; i < _rotationArray.Length; i++)
			{
				float rotation = _rotationArray[i] + MyObject.WorldRotation;
				GameObject gameObject;
				if (asChild)
				{
					gameObject = MySlot.MyObject.CreateChildByPartData(_partData, createPhysBody: true);
					gameObject.SetRotation(_rotationArray[i]);
					gameObject.Activate();
				}
				else
				{
					gameObject = MyObject.MyLayer.CreateObjectByPartData(_partData, MyObject.WorldPosition, MyHero.MyObject.Scale, rotation, MyObject.WorldZOrder + _zorder);
					gameObject.SetVelocity(Utils.GetVelocityVector2(_shotSpeed * num, rotation) + MyLayer.ScrollSpeed);
				}
				gameObject.MyHero = MyHero;
				GameObject gameObject2 = gameObject.GetChildByIdDeep("hit") ?? gameObject;
				gameObject2.Damage = _totalDamageFactor * num * MyHero.GetTotalDPS() / (float)_rotationArray.Length;
				gameObject2.DamageType = _damageType;
				gameObject2.MyHero = MyHero;
			}
		}
		else if (Utils.ChangeValueToTarget(ref _activateTimer, 0f, elapsedTime))
		{
			_isActive = false;
		}
	}
}
