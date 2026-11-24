using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnCollisionShock : ShockBehavior
{
	private bool _bIgnoreCollisionObject;

	private bool _bOnContact;

	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
	}

	public override void Init()
	{
		base.Init();
		_bIgnoreCollisionObject = ParamBool("ignore_collision_object");
		_bOnContact = ParamBool("on_contact");
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		List<GameObject> list = null;
		if (_bIgnoreCollisionObject)
		{
			list = GameCore.Allocator.GetGameObjectList();
			list.Add(collisionObject);
		}
		if (MyObject.MyItem != null)
		{
			MyObject.MyItem.DoOnHitCollision(myObject, collisionObject, contactPoint, normal, GetShockDamage());
		}
		if (_bOnContact)
		{
			Shock(contactPoint, list);
		}
		else
		{
			Shock(MyObject.WorldPosition, list);
		}
		GameCore.Allocator.DeleteAliasingGameObjectList(list);
	}
}
