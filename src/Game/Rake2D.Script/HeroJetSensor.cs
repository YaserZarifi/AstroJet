using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class HeroJetSensor : HeroHitSensor
{
	private List<GameObject> _contactList;

	private float _hitTimer;

	private float _hitDelay = 0.1f;

	private float _dps;

	public HeroJetSensor()
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
		_dps = Param("dps");
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (collisionObject.PhysClass != PhysClass.Bonus && !_contactList.Contains(collisionObject))
		{
			_contactList.Add(collisionObject);
			_contactPoint = contactPoint;
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
			else if (!gameObject.IsImmortal && !gameObject.IsKilled && (gameObject.MaxHP > 0f || (!gameObject.IsMainPart && gameObject.GetMainPart().MaxHP > 0f)) && gameObject.RigidBody != null && (!gameObject.RigidBody.IsSensor || gameObject.Damage > 0f))
			{
				gameObject.Hit(_dps * _hitDelay, MyObject.DamageType, MyObject);
			}
			_hitTimer = _hitDelay;
		}
	}

	protected override void HitEffect(GameObject go, Vector2 contactPoint)
	{
		if (contactPoint != Vector2.Zero)
		{
			GameObject gameObject = go.MyLayer.CreateObjectByPartData(_hitData, contactPoint, Vector2.One, 0f);
			go.AddChildRelative(gameObject, gameObject.WorldPosition.X - go.WorldPosition.X, gameObject.WorldPosition.Y - go.WorldPosition.Y);
		}
	}
}
