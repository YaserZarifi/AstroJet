using System.Collections.Generic;
using Rake2D.JnG;

namespace Rake2D.Script;

public class HeroEquipmentBlastProjector : ItemBehaviorScript
{
	protected float _shotTimer;

	protected List<HeroSlot> _slotList;

	protected HeroItem _originalItem;

	protected ItemWeapon _weaponBehavior;

	protected bool _doRefresh;

	public override void StartInSlot()
	{
		_shotTimer = 0f;
		_doRefresh = true;
		_originalItem = MyHero.GetSlot(HeroSlotEnum.front_gun).Item;
		MyHero.IsBlastProjectorEnabled = true;
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
	}

	public override void RecalculateParams()
	{
		if (_slotList != null)
		{
			foreach (HeroSlot slot in _slotList)
			{
				if (slot.MyObject != null)
				{
					slot.Destroy();
				}
			}
			_slotList.Clear();
		}
		else
		{
			_slotList = new List<HeroSlot>();
		}
	}

	protected void RefreshItemList()
	{
		if (_originalItem == null)
		{
			return;
		}
		_weaponBehavior = _originalItem.GetBehaviorByType(typeof(ItemWeapon)) as ItemWeapon;
		if (_weaponBehavior == null)
		{
			return;
		}
		foreach (GameObject drone in MyHero.DroneList)
		{
			GameObject parentObject = drone.GetChildByIdDeep("drone_muzzle") ?? drone;
			if (_slotList == null || _slotList.Find((HeroSlot s) => s.ParentObject == parentObject) == null)
			{
				HeroItem heroItem = new HeroItem(_originalItem.Data);
				HeroSlot heroSlot = new HeroSlot(MyHero.MyShip, HeroSlotEnum.drone);
				heroSlot.ParentObject = parentObject;
				heroSlot.Init();
				heroSlot.SetItem(heroItem, bReconfigurateShip: false);
				heroSlot.FireKey = _originalItem.MySlot.FireKey;
				heroItem.Level = _originalItem.Level;
				heroItem.RecalculateBehaviorParams();
				heroSlot.Start();
				heroSlot.MyObject.ParentObject = parentObject;
				if (MyHero.IsMainGemEnabled)
				{
					MyHero.ApplySlotToSlot(MyHero.GetSlot(HeroSlotEnum.gem_top), heroSlot);
					MyHero.ApplySlotToSlot(MyHero.GetSlot(HeroSlotEnum.gem_front), heroSlot);
					MyHero.ApplySlotToSlot(MyHero.GetSlot(HeroSlotEnum.gem_bottom), heroSlot);
				}
				else
				{
					MyHero.ApplySlotToSlot(MyHero.GetSlot(HeroSlotEnum.gem_front), heroSlot);
				}
				MyHero.ApplyEquipSlotsToSlot(heroSlot);
				_slotList.Add(heroSlot);
			}
		}
		_doRefresh = false;
	}

	public override void Update(float elapsedTime)
	{
		if (_doRefresh || _slotList.Count < MyHero.DroneList.Count)
		{
			RefreshItemList();
		}
		for (int num = _slotList.Count - 1; num >= 0; num--)
		{
			HeroSlot heroSlot = _slotList[num];
			if (heroSlot.IsDestroyed)
			{
				_slotList.RemoveAt(num);
				_doRefresh = true;
			}
			else
			{
				if (!_weaponBehavior.IsEnabledAutorack && !_weaponBehavior.IsEnabledMouseGun)
				{
					float rotationTo = Utils.GetRotationTo(MyHero.GetControlledObject().WorldPosition, heroSlot.MyObject.WorldPosition, heroSlot.WorldRotation);
					heroSlot.MyObject.SetRotation(rotationTo);
				}
				heroSlot.Update(elapsedTime);
			}
		}
		if (_doRefresh || _slotList.Count < MyHero.DroneList.Count)
		{
			RefreshItemList();
		}
	}
}
