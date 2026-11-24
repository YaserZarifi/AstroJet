using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class HeroGroundSensor : HeroHitSensor
{
	private bool _isHitReady;

	private float _hitTimer;

	private float _firstHitTimer;

	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
		MyObject.OnUpdate += OnUpdate;
	}

	public void OnUpdate(float elapsedTime)
	{
		Utils.ChangeValueToTarget(ref _firstHitTimer, 0f, elapsedTime);
		if (Utils.ChangeValueToTarget(ref _hitTimer, 0f, elapsedTime))
		{
			_isHitReady = true;
			_hitTimer = 0.2f;
		}
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (collisionObject.PhysClass == PhysClass.Bonus)
		{
			return;
		}
		if ((collisionObject.PhysClass & (PhysClass)0x2103408) != 0)
		{
			if (_heroMainObject.MyHero.Shield <= 0f && _isHitReady)
			{
				HitEffect(collisionObject, contactPoint);
			}
			_heroMainObject.SetPosition(_heroMainObject.WorldPosition + normal * 16f * MyObject.Scale.Y);
		}
		if (_isHitReady)
		{
			if (_heroMainObject.MyHero.Shield > 0f)
			{
				ShieldHitEffect(collisionObject, contactPoint);
			}
			bool bDamageToHero = _firstHitTimer > 0f || (collisionObject.PhysClass & PhysClass.EnemyGround) != 0;
			Hit(collisionObject, contactPoint, bDamageToHero);
			DoHUDHit(collisionObject, contactPoint);
			_firstHitTimer = 2f;
			_isHitReady = false;
		}
	}
}
