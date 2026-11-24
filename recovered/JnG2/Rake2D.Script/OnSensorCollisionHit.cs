using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnSensorCollisionHit : BehaviorScript
{
	private float _hitTimer;

	private float _hitDelay;

	private bool _dpsMode;

	private bool _useScaleToDamage;

	private bool _onContact;

	private bool _parentScale;

	private List<GameObject> _contactList;

	private List<GameObject> _removeFromContactList;

	private List<PartData> _partDataList;

	private string[] _soundIdArray;

	private Vector2 _lastHitOffset;

	private float _damage;

	private float _parentDamageFactor;

	public override void Init()
	{
		_contactList = GameCore.Allocator.GetGameObjectList();
		_removeFromContactList = GameCore.Allocator.GetGameObjectList();
		_hitDelay = Param("hit_delay", 1000f) / 1000f;
		_dpsMode = ParamBool("dps_mode");
		_soundIdArray = ParamStringArray("sound_id");
		_damage = Param("damage", -100f);
		_parentDamageFactor = Param("parent_damage") / 100f;
		_useScaleToDamage = ParamBool("scale_damage");
		_parentScale = ParamBool("parent_scale", defaultValue: true);
		_onContact = ParamBool("on_contact", defaultValue: true);
		string[] array = ParamStringArray("object_id");
		if (array == null)
		{
			return;
		}
		_partDataList = new List<PartData>();
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			PartData partData = GameCore.GetPartData(array2[i]);
			if (partData != null)
			{
				_partDataList.Add(partData);
			}
		}
	}

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
		MyObject.OnCollision += OnCollision;
		MyObject.OnSeparation += OnSeparation;
		_contactList.Clear();
		_removeFromContactList.Clear();
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (!_contactList.Contains(collisionObject))
		{
			_contactList.Add(collisionObject);
			MyObject.IgnoreCollisionMode = 3;
			if (_onContact)
			{
				if (_hitTimer <= 0f)
				{
					CreateObject(contactPoint);
				}
				if (contactPoint != Vector2.Zero)
				{
					_lastHitOffset = contactPoint - MyObject.WorldPosition;
				}
				else
				{
					_lastHitOffset = Vector2.Zero;
				}
			}
			else if (_hitTimer <= 0f)
			{
				CreateObject(MyObject.WorldPosition);
				_lastHitOffset = Vector2.Zero;
			}
		}
		else
		{
			MyObject.IgnoreCollisionMode = 2;
		}
	}

	private void CreateObject(Vector2 position)
	{
		if (_partDataList != null && _partDataList.Count > 0)
		{
			MyLayer.CreateObjectByPartData(Utils.GetRandomItem(_partDataList), position, _parentScale ? MyObject.WorldScale : Vector2.One, 0f, MyObject.ZOrder);
		}
	}

	public void OnSeparation(GameObject gameObject)
	{
		if (_contactList != null && _contactList.Contains(gameObject))
		{
			_removeFromContactList.Add(gameObject);
		}
	}

	public void Update(float elapsedTime)
	{
		MyObject.IgnoreCollisionMode = 0;
		_hitTimer -= elapsedTime;
		if (_contactList.Count <= 0 || !(_hitTimer <= 0f))
		{
			return;
		}
		float num;
		if (!(_parentDamageFactor > 0f) || MyObject.ParentObject == null)
		{
			num = ((_damage >= 0f) ? _damage : (MyObject.Damage * (_damage / -100f)));
		}
		else
		{
			num = MyObject.ParentObject.Damage * _parentDamageFactor;
		}
		if (_dpsMode)
		{
			num *= _hitDelay;
		}
		if (_useScaleToDamage)
		{
			num *= MyObject.GetMainPart().WorldScale.X;
		}
		foreach (GameObject removeFromContact in _removeFromContactList)
		{
			_contactList.Remove(removeFromContact);
		}
		_removeFromContactList.Clear();
		for (int num2 = _contactList.Count - 1; num2 >= 0; num2--)
		{
			GameObject gameObject;
			try
			{
				gameObject = _contactList[num2];
			}
			catch (Exception)
			{
				continue;
			}
			if (gameObject.IsDestroyed)
			{
				_contactList.RemoveAt(num2);
				continue;
			}
			if (gameObject.IsImmortal)
			{
				_contactList.RemoveAt(num2);
			}
			MyObject.MyItem?.DoOnHitCollision(MyObject, gameObject, MyObject.WorldPosition, new Vector2(0f, -1f), num);
			gameObject.Hit(num, MyObject.DamageType, MyObject);
			CreateObject(MyObject.WorldPosition + _lastHitOffset);
			if (_soundIdArray != null)
			{
				gameObject.PlaySound(Utils.GetRandomItem(_soundIdArray));
			}
		}
		_hitTimer = _hitDelay;
	}
}
