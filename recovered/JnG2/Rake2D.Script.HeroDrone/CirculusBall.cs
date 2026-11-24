using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script.HeroDrone;

public class CirculusBall : DroneBehavior
{
	private List<GameObject> _contactList;

	private PartData _hitEffectData;

	public override void Init()
	{
		_hitDelay = 0.25f;
		_contactList = GameCore.Allocator.GetGameObjectList();
		_hitEffectData = GameCore.GetPartData("hit_hero_equipment_circulus");
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyObject.OnUpdate += Update;
		MyObject.OnCollision += OnCollision;
		MyObject.OnSeparation += OnSeparation;
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (collisionObject.PhysClass == PhysClass.ShotEnemy)
		{
			collisionObject.DoOnCollision(MyObject, collisionObject.WorldPosition, normal);
			collisionObject.Destroy();
			DoHitEffect();
		}
		else if (_contactList != null && !_contactList.Contains(collisionObject))
		{
			_contactList.Add(collisionObject);
			DoHit(collisionObject);
		}
	}

	public void OnSeparation(GameObject gameObject)
	{
		if (_contactList != null && _contactList.Contains(gameObject))
		{
			_contactList.Remove(gameObject);
		}
	}

	protected override void Update(float elapsedTime)
	{
		_hitTimer -= elapsedTime;
		if (!(_hitTimer <= 0f))
		{
			return;
		}
		for (int num = _contactList.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = _contactList[num];
			if (gameObject.IsDestroyed)
			{
				_contactList.RemoveAt(num);
			}
			else
			{
				DoHit(gameObject);
			}
		}
	}

	private void DoHit(GameObject go)
	{
		go.ApplyLinearImpulse(Vector2.Normalize(MyObject.Position - MyObject.OldPosition) * 1000f);
		if (!go.IsImmortal && !go.IsKilled)
		{
			_hitTimer = _hitDelay;
			go.Hit(MyObject.Damage * _hitDelay * MyObject.MyHero.MyObject.Scale.X, MyObject.DamageType, MyObject);
			DoHitEffect();
		}
	}

	private void DoHitEffect()
	{
		MyObject.CreateChildByPartData(_hitEffectData, createPhysBody: true);
	}
}
