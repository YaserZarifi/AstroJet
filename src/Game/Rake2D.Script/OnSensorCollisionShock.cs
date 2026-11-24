using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnSensorCollisionShock : ShockBehavior
{
	private List<GameObject> _hittedList;

	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
	}

	public override void Init()
	{
		base.Init();
		_hittedList = new List<GameObject>();
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (!_hittedList.Contains(collisionObject))
		{
			_hittedList.Add(collisionObject);
			float damage = ((_damage < 0f) ? (MyObject.Damage * _damage / -100f) : _damage);
			MyLevel.ShockHit(MyObject, new List<GameObject> { collisionObject }, MyObject.WorldPosition, _targetClass, _damageType, damage, _physForce, null, onlyHitable: false, wantResult: false);
		}
	}
}
