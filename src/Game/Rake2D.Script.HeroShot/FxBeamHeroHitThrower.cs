using Microsoft.Xna.Framework;

namespace Rake2D.Script.HeroShot;

public class FxBeamHeroHitThrower : FxBeamHero
{
	private FloatRange _throwerDelayRange;

	private float _throwerTimer;

	private float _throwerBeginTimer;

	private PartData _throwerData;

	public override void Init()
	{
		base.Init();
		_throwerData = GameCore.GetPartData(ParamString("thrower_id", "undefined"));
		_throwerDelayRange = ParamRange("thrower_delay", 100f) / 1000f;
		_throwerBeginTimer = ParamRandom("thrower_begin_timer", 0f) / 1000f;
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		if (MyObject.MyItem != null)
		{
			MyObject.MyItem.OnHitCollision += OnHitCollision;
		}
	}

	protected override void OnUpdate(float elapsedTime)
	{
		base.OnUpdate(elapsedTime);
		_throwerBeginTimer -= elapsedTime;
		_throwerTimer -= elapsedTime;
	}

	private void OnHitCollision(GameObject shotObject, GameObject targetObject, Vector2 contactPoint, Vector2 normal, float damage)
	{
		if (_throwerData != null && shotObject.MyItem != null && !targetObject.IsImmortal && targetObject.MaxHP > 0f && targetObject.IsAlive && _throwerBeginTimer <= 0f && _throwerTimer <= 0f)
		{
			targetObject.MyLayer.CreateObjectByPartData(_throwerData, contactPoint, Vector2.One, shotObject.MyItem.MySlot.WorldRotation);
			_throwerTimer = _throwerDelayRange.GetRandom();
		}
	}
}
