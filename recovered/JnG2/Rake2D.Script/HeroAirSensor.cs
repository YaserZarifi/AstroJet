using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class HeroAirSensor : HeroHitSensor
{
	private List<GameObject> _contactList;

	private float _hitTimer;

	private bool _newHit;

	public HeroAirSensor()
	{
		_contactList = GameCore.Allocator.GetGameObjectList();
	}

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
		MyObject.OnCollision += OnCollision;
		MyObject.OnSeparation += OnSeparation;
	}

	public override void Init()
	{
		base.Init();
		_hitTimer = 0f;
	}

	protected void DoHitEffect(GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (_heroMainObject.MyHero.Shield <= 0f)
		{
			if (collisionObject.Damage >= 0f)
			{
				if (normal != Vector2.Zero)
				{
					_heroMainObject.SetPosition(_heroMainObject.WorldPosition + normal * 16f);
				}
				else
				{
					_heroMainObject.SetPosition(_heroMainObject.WorldPosition.X + (float)Utils.GetRandom(8, 12, bRandonNegative: true), _heroMainObject.WorldPosition.Y + (float)Utils.GetRandom(8, 2, bRandonNegative: true));
				}
				if (_newHit && _contactPoint != Vector2.Zero)
				{
					HitEffect(collisionObject, _contactPoint);
				}
				else
				{
					HitEffect(collisionObject, MyObject.WorldPosition + Utils.RotateVector2(Utils.GetRandomVector2(-70f, -10f, 40f, 10f), MyObject.WorldRotation));
				}
			}
		}
		else if (_newHit && _contactPoint != Vector2.Zero)
		{
			ShieldHitEffect(collisionObject, _contactPoint);
		}
		else
		{
			ShieldHitEffect(collisionObject, MyObject.WorldPosition + Utils.RotateVector2(Utils.GetRandomVector2(-100f, -30f, 80f, 30f), MyObject.WorldRotation));
		}
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (collisionObject.PhysClass != PhysClass.Bonus && !_contactList.Contains(collisionObject))
		{
			_contactList.Add(collisionObject);
			_contactPoint = contactPoint;
			_newHit = true;
			if (_heroMainObject != null && _hitTimer > 0f)
			{
				DoHitEffect(collisionObject, contactPoint, normal);
				DoHUDHit(collisionObject, contactPoint);
			}
		}
	}

	public void OnSeparation(GameObject go)
	{
		_contactList.Remove(go);
	}

	public void Update(float elapsedTime)
	{
		if (_hitTimer > 0f)
		{
			_hitTimer -= elapsedTime;
		}
		int num = 0;
		if (!(_hitTimer <= 0f))
		{
			return;
		}
		for (int num2 = _contactList.Count - 1; num2 >= 0; num2--)
		{
			GameObject gameObject = _contactList[num2];
			if (gameObject.IsDestroyed)
			{
				_contactList.RemoveAt(num2);
			}
			else
			{
				if (gameObject.RigidBody != null && (!gameObject.RigidBody.IsSensor || gameObject.Damage > 0f))
				{
					Hit(gameObject, gameObject.WorldPosition, bDamageToHero: true);
					DoHUDHit(gameObject, _contactPoint);
				}
				if (num == 0)
				{
					DoHitEffect(gameObject, _contactPoint, Vector2.Zero);
				}
				num++;
				_hitTimer = 0.2f;
			}
		}
		_newHit = false;
	}
}
