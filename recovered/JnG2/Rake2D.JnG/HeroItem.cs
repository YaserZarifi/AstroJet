using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Rake2D.Script;

namespace Rake2D.JnG;

public class HeroItem
{
	public delegate void OnHitCollisionHandler(GameObject shotObject, GameObject targetObject, Vector2 contactPoint, Vector2 normal, float damage);

	public delegate void NoParamHandler();

	public const int MAX_LEVEL = 3;

	public const int MAX_CHILD_ITEM_COUNT = 2;

	public HeroItemData Data;

	public Loadout Loadout;

	public bool IsInShop;

	public float ActivateTimer;

	public int CargoIndex = -1;

	public int Level;

	public bool IsShopActionDisabled;

	public bool IsBlockedBySameItem;

	public bool IsMyItemSetComplete;

	public static HashSet<string> LessIsBetterParams;

	internal Dictionary<string, float> Params;

	private List<ItemBehaviorScript> _behaviorList;

	public int Price => GetLevelPrice(Level);

	public HeroSlot MySlot { get; private set; }

	public HeroShip MyShip
	{
		get
		{
			if (MySlot != null)
			{
				return MySlot.MyShip;
			}
			return null;
		}
	}

	public bool IsTooExpensive => Price > GameJnG.Cash;

	public bool IsUpgradeTooExpensive => GetUpgradePrice(Level + 1) > GameJnG.Cash;

	public bool IsTypeWeapon => Data.IsShopTypeWeapon;

	public bool IsTypeMissile => Data.ItemType == ItemTypeEnum.Missile;

	public bool IsTypeShield => Data.ItemType == ItemTypeEnum.Shield;

	public bool IsTypeArmor => Data.ItemType == ItemTypeEnum.Armor;

	public bool IsTypeBomb
	{
		get
		{
			if (Data.ItemType != ItemTypeEnum.Bomb)
			{
				return Data.ItemType == ItemTypeEnum.Gunbot;
			}
			return true;
		}
	}

	public bool IsTypeDevice
	{
		get
		{
			if (Data.ItemType != ItemTypeEnum.Device)
			{
				return Data.ItemType == ItemTypeEnum.Mana_device;
			}
			return true;
		}
	}

	public bool IsTypeManaDevice => Data.ItemType == ItemTypeEnum.Mana_device;

	public bool IsTypeEquipment
	{
		get
		{
			if (Data.ItemType != ItemTypeEnum.Equipment)
			{
				return Data.ItemType == ItemTypeEnum.Satelite;
			}
			return true;
		}
	}

	public bool IsTypeGem => Data.ItemType == ItemTypeEnum.Gem;

	public bool IsTypeOther => Data.ItemType == ItemTypeEnum.Other;

	public bool IsReady
	{
		get
		{
			if (ActivateTimer > 0f)
			{
				return false;
			}
			if (MySlot == null || !IsEnabled || GameCore.IsPaused || (MySlot.MyHero.IsOverheated && (MySlot.IsWeaponType || MySlot.SlotType == HeroSlotEnum.drone)))
			{
				return false;
			}
			if (MySlot.SlotType == HeroSlotEnum.front_gun && MySlot.MyHero.IsBlastProjectorEnabled && MySlot.MyHero.DroneList.Count > 0)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsButtonDownAndReady
	{
		get
		{
			if (IsReady)
			{
				return MySlot.IsButtonDown;
			}
			return false;
		}
	}

	public bool IsEnabled => !IsBlockedBySameItem;

	public bool IsUpgradable => Data.MaxLevel > 0;

	public ItemPlaceEnum Place
	{
		get
		{
			if (IsInShop && CargoIndex < 0 && MySlot == null)
			{
				return ItemPlaceEnum.InShop;
			}
			if (CargoIndex >= 0 && MySlot == null)
			{
				return ItemPlaceEnum.InCargo;
			}
			if (CargoIndex < 0 && MySlot != null)
			{
				return ItemPlaceEnum.InHeroSlot;
			}
			return ItemPlaceEnum.Nowhere;
		}
	}

	public event OnHitCollisionHandler OnHitCollision;

	public event NoParamHandler OnActivated;

	public event NoParamHandler OnStartInSlotEnd;

	public int GetLevelPrice(int level)
	{
		if (level >= 0 && level < Data.PriceList.Count)
		{
			return Data.PriceList[level];
		}
		if (Data.PriceList.Count > 0)
		{
			return Data.PriceList.Last();
		}
		return 0;
	}

	public int GetUpgradePrice(int toLevel)
	{
		int levelPrice = GetLevelPrice(Level);
		return Math.Abs(GetLevelPrice(toLevel) - levelPrice);
	}

	public static void AddLessIsBetterParam(string name)
	{
		LessIsBetterParams.Add(name.ToLowerLazy());
	}

	static HeroItem()
	{
		LessIsBetterParams = new HashSet<string>();
	}

	internal HeroItem(HeroItemData data)
	{
		Data = data;
		Params = new Dictionary<string, float>();
		InitData();
	}

	public string GetShopName()
	{
		return Data.GetShopName();
	}

	public string GetShopDescription()
	{
		return TextBundle.GetText($"desc_{Data.TextId}").Replace("|item_desc_gem|", TextBundle.GetText("item_desc_gem"));
	}

	internal void InitData()
	{
		if (Data == null)
		{
			GameCore.LogErr("Item without data");
			return;
		}
		_behaviorList = null;
		if (Data.BehaviorDataList.IsNullOrEmpty())
		{
			return;
		}
		foreach (BehaviorData behaviorData in Data.BehaviorDataList)
		{
			ItemBehaviorScript itemBehaviorScript = AddBehaviorScript(behaviorData);
			if (itemBehaviorScript == null)
			{
				GameCore.LogErr("Unable to add behavior '{0}' to hero device '{1}'", behaviorData.Id, Data.Id);
				continue;
			}
			try
			{
				itemBehaviorScript.Init();
			}
			catch (Exception ex)
			{
				GameCore.LogErr("Error in [item = {0}] script class {1} - Ex: {2}", Data.Id, itemBehaviorScript.ClassName, ex.ToString());
			}
		}
	}

	public void CreateFreeLoadout()
	{
		if (Data.ItemType == ItemTypeEnum.Loadout)
		{
			int freeLoadoutNumber = GameJnG.GetFreeLoadoutNumber();
			Loadout = new Loadout(freeLoadoutNumber);
		}
	}

	private void ClearEvents()
	{
		OnHitCollision = null;
		OnActivated = null;
		OnStartInSlotEnd = null;
	}

	internal void RegisterEvents()
	{
		ClearEvents();
		if (Place == ItemPlaceEnum.InHeroSlot)
		{
			_behaviorList?.ForEach((ItemBehaviorScript script) =>
			{
				script.RegisterEvents();
			});
		}
	}

	public bool TryGetParamValue(string paramName, out float value)
	{
		return Params.TryGetValue(paramName, out value);
	}

	public bool TryGetBaseParamValue(string paramName, out float value)
	{
		if (Data.Params.TryGetValue(paramName, out var value2))
		{
			value = value2.GetValue(Level);
			return true;
		}
		value = 0f;
		return false;
	}

	public void RemoveFromAllPlaces()
	{
		GameJnG.ShopOffer.RemoveItem(this);
		if (CargoIndex >= 0)
		{
			GameJnG.Cargo.RemoveItem(this);
			CargoIndex = -1;
		}
		if (MySlot != null)
		{
			MySlot.SetItem(null);
			MySlot = null;
		}
	}

	public int GetIconFrame()
	{
		if (Data.IconFrameList.IsNullOrEmpty())
		{
			return Math.Min(Level, Data.IconAtlas.FrameCount - 1);
		}
		if (Level < Data.IconFrameList.Count)
		{
			return Data.IconFrameList[Level];
		}
		return Data.IconFrameList.Last();
	}

	public string GetTVObjectId()
	{
		if (Data.TVObjectIdList.IsNullOrEmpty())
		{
			return null;
		}
		if (Level < Data.TVObjectIdList.Count)
		{
			return Data.TVObjectIdList[Level];
		}
		return Data.TVObjectIdList.Last();
	}

	public HeroItem GetWeaponItemForGem()
	{
		if (MySlot != null)
		{
			HeroSlot heroSlot = null;
			switch (MySlot.SlotType)
			{
			case HeroSlotEnum.gem_top:
				heroSlot = ((!MyShip.MyHero.IsMainGemEnabled) ? MyShip.GetSlot(HeroSlotEnum.top_gun) : MyShip.GetSlot(HeroSlotEnum.front_gun));
				break;
			case HeroSlotEnum.gem_front:
				heroSlot = MyShip.GetSlot(HeroSlotEnum.front_gun);
				break;
			case HeroSlotEnum.gem_bottom:
				heroSlot = ((!MyShip.MyHero.IsMainGemEnabled) ? MyShip.GetSlot(HeroSlotEnum.bottom_gun) : MyShip.GetSlot(HeroSlotEnum.front_gun));
				break;
			case HeroSlotEnum.gem_rear:
				heroSlot = MyShip.GetSlot(HeroSlotEnum.rear_gun);
				break;
			}
			return heroSlot?.Item;
		}
		return null;
	}

	internal void Update(float elapsedTime)
	{
		Utils.ChangeValueToTarget(ref ActivateTimer, 0f, elapsedTime);
		_behaviorList?.ForEach((ItemBehaviorScript script) =>
		{
			try
			{
				script.Update(elapsedTime);
			}
			catch (Exception ex)
			{
				GameCore.LogErr("Error in [item = {0}] script class {1} - Ex: {2}", Data.Id, script.ClassName, ex.ToString());
			}
		});
	}

	internal ItemBehaviorScript AddBehaviorScript(BehaviorData data)
	{
		ItemBehaviorScript itemBehaviorScript = ItemBehaviorScript.CreateFromBehaviorData(this, data);
		AddBehaviorScript(itemBehaviorScript);
		return itemBehaviorScript;
	}

	public void AddBehaviorScript(ItemBehaviorScript script)
	{
		if (script != null)
		{
			if (_behaviorList == null)
			{
				_behaviorList = new List<ItemBehaviorScript>();
			}
			_behaviorList.Add(script);
		}
	}

	public ItemBehaviorScript GetBehaviorByType(Type type)
	{
		ItemBehaviorScript foudedScript = null;
		_behaviorList?.ForEach((ItemBehaviorScript script) =>
		{
			if (script.GetType() == type || script.GetType().IsSubclassOf(type))
			{
				foudedScript = script;
			}
		});
		return foudedScript;
	}

	internal void SetSlot(HeroSlot slot)
	{
		MySlot = slot;
		if (slot == null)
		{
			ClearEvents();
		}
	}

	internal void StartInSlot()
	{
		ActivateTimer = 0f;
		if (MySlot != null)
		{
			_behaviorList?.ForEach((ItemBehaviorScript script) =>
			{
				script.StartInSlot();
			});
			RegisterEvents();
		}
	}

	internal void StartInSlotEnd()
	{
		OnStartInSlotEnd?.Invoke();
	}

	public void ApplyToItem(HeroItem item)
	{
		if (item != null)
		{
			_behaviorList?.ForEach((ItemBehaviorScript beh) =>
			{
				beh.ApplyToItem(item);
			});
		}
	}

	internal void AddParam(string name, float value)
	{
		if (Params.ContainsKey(name.ToLowerLazy()))
		{
			GameCore.LogErr("Param '@{0}' already defined in item '{1}'", name, Data.Id);
		}
		Params[name.ToLowerLazy()] = value;
	}

	internal void SetParam(string name, float value)
	{
		if (!Params.ContainsKey(name.ToLowerLazy()))
		{
			GameCore.LogErr("Undefined param '@{0}' in item '{1}'", name, Data.Id);
		}
		Params[name.ToLowerLazy()] = value;
	}

	public float GetBaseParam(string name, float defaultValue = 0f)
	{
		if (name != null && Data.Params.TryGetValue(name.ToLowerLazy(), out var value))
		{
			return value.GetValue(Level);
		}
		return defaultValue;
	}

	public bool TryGetBaseParamSummary(string name, out float value, out float finalValue)
	{
		ItemBehaviorScript itemBehaviorScript = _behaviorList?.First();
		if (itemBehaviorScript != null && itemBehaviorScript.TryGetParamSummary(name, out value, out finalValue))
		{
			return true;
		}
		bool result = TryGetBaseParamValue(name, out value);
		finalValue = value;
		return result;
	}

	public float GetParam(string name, float defaultValue = 0f)
	{
		if (!Params.TryGetValue(name.ToLowerLazy(), out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public static bool IsParamLessBetter(string name)
	{
		return LessIsBetterParams.Contains(name.ToLowerLazy());
	}

	public void Activate()
	{
		try
		{
			OnActivated?.Invoke();
		}
		catch (Exception ex)
		{
			GameCore.LogErr("OnButtonPressed script exception: {0} in item '{1}'", ex, Data.Id);
		}
		if (IsTypeManaDevice)
		{
			ActivateTimer = 0.5f;
		}
	}

	public void DoOnRemoveFromSlot()
	{
		foreach (ItemBehaviorScript behavior in _behaviorList)
		{
			behavior.RemoveFromSlot();
		}
	}

	public void DoOnCollision(GameObject shotObject, GameObject targetObject, Vector2 contactPoint, Vector2 normal)
	{
		DoOnHitCollision(shotObject, targetObject, contactPoint, normal, shotObject.HitOnCollision ? shotObject.Damage : 0f);
	}

	public virtual void DoOnHitCollision(GameObject shotObject, GameObject targetObject, Vector2 contactPoint, Vector2 normal, float damage)
	{
		try
		{
			OnHitCollision?.Invoke(shotObject, targetObject, contactPoint, normal, damage);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("OnCollision script exception: {0} in item '{1}'", ex, Data.Id);
		}
	}

	public HeroItem DoGetItem()
	{
		int index = -1;
		HeroItem heroItem = null;
		switch (Place)
		{
		case ItemPlaceEnum.InCargo:
			index = CargoIndex;
			GameJnG.Cargo.RemoveItem(this);
			break;
		case ItemPlaceEnum.InHeroSlot:
			MySlot.SetItem(null);
			break;
		}
		if (Data.ItemIdToGetArray != null)
		{
			List<HeroItemData> list = new List<HeroItemData>();
			string[] itemIdToGetArray = Data.ItemIdToGetArray;
			foreach (string text in itemIdToGetArray)
			{
				HeroItemData heroItemData = GameCore.GetHeroItemData(text);
				if (heroItemData != null)
				{
					list.Add(heroItemData);
					continue;
				}
				GameCore.LogErr("Unable to get undefined item '{0}'", text);
			}
			if (Data.ItemIdToGetCount > 0)
			{
				for (int j = 0; j < Data.ItemIdToGetCount; j++)
				{
					if (list.Count <= 0)
					{
						break;
					}
					int random = Utils.GetRandom(list.Count - 1);
					HeroItem heroItem2 = new HeroItem(list[random]);
					list.RemoveAt(random);
					if (Place == ItemPlaceEnum.InHeroSlot && heroItem == null)
					{
						MySlot.SetItem(heroItem2);
					}
					else
					{
						GameJnG.Cargo.SetItem(index, heroItem2);
					}
					if (heroItem == null)
					{
						index = -1;
						heroItem = heroItem2;
					}
				}
			}
			else
			{
				foreach (HeroItemData item in list)
				{
					HeroItem heroItem3 = new HeroItem(item);
					if (Place == ItemPlaceEnum.InHeroSlot && heroItem == null)
					{
						MySlot.SetItem(heroItem3);
					}
					else
					{
						GameJnG.Cargo.SetItem(index, heroItem3);
					}
					if (heroItem == null)
					{
						index = -1;
						heroItem = heroItem3;
					}
				}
			}
		}
		return heroItem;
	}

	public void RefreshActionState()
	{
		IsShopActionDisabled = Data.ShopActionNeedItemIdInCargo != null && !GameJnG.IsItemIdInCargo(Data.ShopActionNeedItemIdInCargo);
	}

	public void RecalculateBehaviorParams()
	{
		_behaviorList?.ForEach((ItemBehaviorScript behavior) =>
		{
			behavior.RecalculateParams();
		});
	}

	internal void UpdateParamModifier(ref ParamModifier modifier, string paramName, HeroItem item)
	{
		if (_behaviorList == null)
		{
			return;
		}
		foreach (ItemBehaviorScript behavior in _behaviorList)
		{
			behavior.UpdateParamModifier(ref modifier, paramName, item);
		}
	}

	public override string ToString()
	{
		return Data?.Id;
	}
}
