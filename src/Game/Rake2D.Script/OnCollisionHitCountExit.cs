using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnCollisionHitCountExit : ExitStateBehavior
{
	private float _hitTimer;

	private float _hitDelay;

	private int _hitCount;

	private int _exitHitCount;

	private List<GameObject> _contactList;

	private List<GameObject> _removeFromContactList;

	private List<PhysClass> _ignorePhysClassList;

	public override void Init()
	{
		base.Init();
		_contactList = GameCore.Allocator.GetGameObjectList();
		_removeFromContactList = GameCore.Allocator.GetGameObjectList();
		_hitDelay = Param("hit_delay", 1000f) / 1000f;
		_exitHitCount = ParamInt("hit_count", 1);
		string[] array = ParamStringArray("ignore_phys_class");
		if (array == null)
		{
			return;
		}
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			PhysClass physClassByString = DataUtils.GetPhysClassByString(array2[i]);
			if (physClassByString != PhysClass.Undefined)
			{
				if (_ignorePhysClassList == null)
				{
					_ignorePhysClassList = new List<PhysClass>();
				}
				_ignorePhysClassList.Add(physClassByString);
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
		if (_ignorePhysClassList != null && _ignorePhysClassList.Contains(collisionObject.PhysClass))
		{
			MyObject.IgnoreCollisionMode = 2;
		}
		else if (!_contactList.Contains(collisionObject))
		{
			_contactList.Add(collisionObject);
			MyObject.IgnoreCollisionMode = 3;
			DoHit(collisionObject);
		}
		else
		{
			MyObject.IgnoreCollisionMode = 2;
		}
	}

	public void OnSeparation(GameObject gameObject)
	{
		if (_contactList != null && _contactList.Contains(gameObject))
		{
			_removeFromContactList.Add(gameObject);
		}
	}

	private void DoHit(GameObject collisionObject)
	{
		_hitCount++;
		_hitTimer = _hitDelay;
		if (_hitCount >= _exitHitCount)
		{
			StartExit();
		}
		MyObject.Hit(1f, DamageTypeEnum.Collision, collisionObject);
	}

	public void Update(float elapsedTime)
	{
		MyObject.IgnoreCollisionMode = 0;
		_hitTimer -= elapsedTime;
		if (_contactList.Count <= 0 || !(_hitTimer <= 0f))
		{
			return;
		}
		foreach (GameObject removeFromContact in _removeFromContactList)
		{
			_contactList.Remove(removeFromContact);
		}
		_removeFromContactList.Clear();
		for (int num = _contactList.Count - 1; num >= 0; num--)
		{
			if (_contactList[num].IsDestroyed)
			{
				_contactList.RemoveAt(num);
			}
		}
		if (_contactList.Count > 0)
		{
			DoHit(_contactList[0]);
		}
	}
}
