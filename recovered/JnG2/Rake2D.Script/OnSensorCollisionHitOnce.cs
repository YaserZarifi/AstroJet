using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnSensorCollisionHitOnce : BehaviorScript
{
	private List<GameObject> _contactList;

	public override void Init()
	{
		_contactList = GameCore.Allocator.GetGameObjectList();
	}

	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
		MyObject.RigidBody?.ResetDynamics();
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (_contactList != null && !_contactList.Contains(collisionObject))
		{
			_contactList.Add(collisionObject);
			collisionObject.Hit(MyObject.Damage, MyObject.DamageType, MyObject);
		}
	}
}
