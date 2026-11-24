namespace Rake2D.Script;

public class OnEachHitThrow : ThrowObjectBehavior
{
	private float _repeatDelay;

	private float _timer;

	private GameObject _hitPart;

	public override void RegisterEvents()
	{
		_hitPart.OnHit += OnHit;
		if (_hitPart != MyObject)
		{
			_hitPart.OnReRegisterEvents += OnReregisterEvents;
		}
		MyObject.OnUpdate += OnUpdate;
		_timer = 0f;
	}

	private void OnReregisterEvents(GameObject go)
	{
		_hitPart.OnHit += OnHit;
	}

	public override void Init()
	{
		base.Init();
		_repeatDelay = Param("repeat_delay", 100f) / 1000f;
		if (Utils.Equals(ParamString("hit_part"), "main"))
		{
			_hitPart = MyObject.GetMainPart();
		}
		if (_hitPart == null)
		{
			_hitPart = MyObject;
		}
	}

	private void OnHit(float damage, DamageTypeEnum damageType, GameObject hittingObject)
	{
		if (_timer <= 0f)
		{
			StartThrow();
			_timer = _repeatDelay;
		}
	}

	private void OnUpdate(float elapsedTime)
	{
		Utils.ChangeValueToTarget(ref _timer, 0f, elapsedTime);
	}
}
