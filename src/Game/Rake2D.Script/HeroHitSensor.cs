using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public abstract class HeroHitSensor : BehaviorScript
{
	protected GameObject _heroMainObject;

	protected Vector2 _contactPoint;

	protected PartData _hitData;

	protected PartData _shiledHitData;

	protected int _heroDamage;

	public override void Init()
	{
		base.Init();
		_hitData = GameCore.GetPartData("hero_hit");
		_shiledHitData = GameCore.GetPartData("hit_hero_shield");
		_heroDamage = (int)Param("hero_damage");
		_heroMainObject = MyObject.GetMainPart();
	}

	protected void DoHUDHit(GameObject collisionObject, Vector2 contactPoint)
	{
		if ((collisionObject.TargetClass & TargetClass.Man) == 0 && !(collisionObject.Damage < 0f))
		{
			if (contactPoint != Vector2.Zero)
			{
				MyObject.MyHero.Hud.AddHit(Utils.GetRotationTo(MyObject.WorldPosition, contactPoint));
			}
			else
			{
				MyObject.MyHero.Hud.AddHit(Utils.GetRotationTo(MyObject.WorldPosition, collisionObject.GetMainLayerPosition()));
			}
		}
	}

	protected void Hit(GameObject go, Vector2 contactPoint, bool bDamageToHero)
	{
		if (bDamageToHero && _heroDamage > 0 && go.Damage == 0f && (go.PhysClass & (PhysClass)0x103418) != 0)
		{
			_heroMainObject.Hit(_heroDamage, go.DamageType, go);
		}
		go.Hit(MyObject.MyHero.GetCollisionDamage(), MyObject.DamageType, MyObject);
	}

	protected virtual void HitEffect(GameObject go, Vector2 contactPoint)
	{
		if (MyObject.MyHero == null || MyObject.MyHero.Shield <= 0f)
		{
			if (go.TargetClass != TargetClass.Man)
			{
				_heroMainObject.PlaySound("hero_hit_" + Utils.GetRandom(1, 4), Utils.GetRandom(80, 100));
			}
			if (contactPoint != Vector2.Zero)
			{
				GameObject gameObject = go.MyLayer.CreateObjectByPartData(_hitData, contactPoint, Vector2.One, 0f);
				MyObject.AddChildRelative(gameObject, gameObject.WorldPosition.X - MyObject.WorldPosition.X, gameObject.WorldPosition.Y - MyObject.WorldPosition.Y);
			}
		}
	}

	protected virtual void ShieldHitEffect(GameObject go, Vector2 contactPoint)
	{
		GameObject gameObject = go.MyLayer.CreateObjectByPartData(_shiledHitData, contactPoint, Vector2.One, 0f);
		MyObject.AddChildRelative(gameObject, gameObject.WorldPosition.X - MyObject.WorldPosition.X, gameObject.WorldPosition.Y - MyObject.WorldPosition.Y);
		gameObject.SetRotation(Utils.GetRotationTo(MyObject.WorldPosition, contactPoint));
	}
}
