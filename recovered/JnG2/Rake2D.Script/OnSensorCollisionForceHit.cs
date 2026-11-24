using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnSensorCollisionForceHit : BehaviorScript
{
	private List<GameObject> _contactList;

	private float _physForce;

	private float _firstHitRadius = 10f;

	private DamageTypeEnum _damageType;

	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
		CheckFirstHit();
	}

	public override void Init()
	{
		string text = ParamString("damage_type");
		if (text != null)
		{
			_damageType = DataUtils.GetDamageTypeByString(text);
		}
		else
		{
			_damageType = MyObject.DamageType;
		}
		_physForce = Param("phys_force");
	}

	private void CheckFirstHit()
	{
		_contactList = MyLevel.GetPhysPartsInRangeInAllPhsLayers(MyObject.WorldPosition, (TargetClass)8440, _firstHitRadius);
		foreach (GameObject contact in _contactList)
		{
			DoHit(contact);
		}
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (_contactList != null && !_contactList.Contains(collisionObject))
		{
			_contactList.Add(collisionObject);
			DoHit(collisionObject);
		}
	}

	private void DoHit(GameObject go)
	{
		if (!go.IsDynamic || !(_physForce > 0f) || !(go.Velocity.Length() < 500f) || go.PartData.IgnoreShock)
		{
			return;
		}
		Vector2 vector = go.RigidBody.WorldCenter - MyObject.WorldPosition;
		vector = ((!(vector != Vector2.Zero)) ? Utils.GetVelocityVector2(_physForce, Utils.GetRandom(0f, (float)Math.PI * 2f)) : (Vector2.Normalize(vector) * _physForce));
		go.RigidBody.ApplyLinearImpulse(vector);
		if (go.RigidBody.AngularVelocity > -40f && go.RigidBody.AngularVelocity < 40f)
		{
			float rotationFromZero = Utils.GetRotationFromZero(vector);
			float num = go.RigidBody.Mass;
			if (rotationFromZero < 0f)
			{
				num *= -1f;
			}
			go.RigidBody.ApplyAngularImpulse(num);
			if (go.RigidBody.AngularVelocity > 20f)
			{
				go.RigidBody.AngularVelocity = 20f;
			}
			else if (go.RigidBody.AngularVelocity < -20f)
			{
				go.RigidBody.AngularVelocity = -20f;
			}
		}
		go.DoOnShockHit(_damageType, vector, MyObject);
	}
}
