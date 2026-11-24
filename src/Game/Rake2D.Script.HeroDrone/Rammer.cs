using Microsoft.Xna.Framework;

namespace Rake2D.Script.HeroDrone;

public class Rammer : DroneFollower
{
	protected override void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (collisionObject.TargetClass == TargetClass.Man)
		{
			MyObject.SetPosition(MyObject.WorldPosition + normal * 5f);
			collisionObject.Hit(MyObject.Damage * _hitDelay, DamageTypeEnum.Collision, MyObject);
			MyObject.IgnoreCollisionMode = 2;
			return;
		}
		MyObject.SetPosition(MyObject.WorldPosition + normal * 10f);
		if (_hitTimer <= 0f)
		{
			_hitTimer = _hitDelay;
			float damage = MyObject.Damage * _hitTimer * MyObject.MyHero.MyObject.Scale.X;
			if (MyObject.MyItem != null)
			{
				MyObject.MyItem.DoOnHitCollision(MyObject, collisionObject, contactPoint, normal, damage);
			}
			collisionObject.Hit(damage, DamageTypeEnum.Collision, MyObject);
			MyLayer.CreateObjectByPartData(_hitPartData, contactPoint, Vector2.One, 0f, collisionObject.ZOrder).SetVelocity(collisionObject.Velocity);
		}
		else
		{
			MyObject.IgnoreCollisionMode = 3;
		}
	}
}
