using System.Collections.Generic;
using System.Linq;

namespace Rake2D.JnG;

public class HeroCargo
{
	public int LoadouCount;

	public SortedDictionary<int, HeroItem> Items;

	public HeroCargo()
	{
		Items = new SortedDictionary<int, HeroItem>();
	}

	public void Clear()
	{
		Items.Clear();
	}

	public HeroItem GetItem(int index)
	{
		if (Items.Keys.Contains(index))
		{
			return Items[index];
		}
		return null;
	}

	public void RemoveItem(HeroItem item)
	{
		KeyValuePair<int, HeroItem> keyValuePair = Items.FirstOrDefault((KeyValuePair<int, HeroItem> o) => o.Value == item);
		if (keyValuePair.Value == item)
		{
			Items.Remove(keyValuePair.Key);
		}
		RefreshAllItemActionStates();
	}

	public void SetItem(int index, HeroItem item)
	{
		if (index < 0)
		{
			index = GetFirstEmptyIndex();
		}
		if (item != null)
		{
			item.SetSlot(null);
			item.CargoIndex = index;
			item.IsBlockedBySameItem = false;
			Items[index] = item;
			item.RecalculateBehaviorParams();
		}
		else
		{
			Items.Remove(index);
		}
		RefreshAllItemActionStates();
	}

	public HeroItem GetFirstItem(HeroItemData data, int level)
	{
		return Items.Values.FirstOrDefault((HeroItem o) => Utils.Equals(o.Data.Id, data.Id) && o.Level == level);
	}

	public HeroItem GetFirstLoadoutItem(Hero hero)
	{
		int num = 0;
		foreach (HeroItem value in Items.Values)
		{
			if (value.Loadout != null)
			{
				if (hero.PlayerNumber < 1 || num > 0)
				{
					return value;
				}
				num++;
			}
		}
		return null;
	}

	public void RefreshAllItemActionStates()
	{
		LoadouCount = 0;
		foreach (HeroItem value in Items.Values)
		{
			value.RefreshActionState();
			if (value.Loadout != null)
			{
				LoadouCount++;
			}
		}
	}

	public void Sort()
	{
		List<HeroItem> list = (from i in Items.Values
			orderby i.Data.GetSlotType(), i.Data.GetShopName()
			select i).ToList();
		Items.Clear();
		int num = 0;
		SlotTypeEnum slotTypeEnum = SlotTypeEnum.Undefined;
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			HeroItem heroItem = list[num2];
			if (slotTypeEnum != heroItem.Data.GetSlotType())
			{
				slotTypeEnum = heroItem.Data.GetSlotType();
				for (; num % 3 != 0; num++)
				{
				}
			}
			heroItem.CargoIndex = num;
			Items[num] = heroItem;
			num++;
		}
		GameCore.PlaySound("menu_item_place");
	}

	public void SwapItems(int index1, int index2)
	{
		if (index1 != index2)
		{
			HeroItem item = GetItem(index1);
			HeroItem item2 = GetItem(index2);
			SetItem(index1, item2);
			SetItem(index2, item);
		}
	}

	public void SwapItemWithSlot(int cargoIndex, HeroShip ship, HeroSlotEnum slotType)
	{
		HeroItem item = GetItem(cargoIndex);
		HeroItem item2 = ship.Slots[slotType].Item;
		SetItem(cargoIndex, item2);
		if (item2 != null)
		{
			ship.Slots[slotType].SetItem(item);
		}
	}

	public int GetFirstEmptyIndex()
	{
		int num = -1;
		foreach (int key in Items.Keys)
		{
			if (key > num + 1)
			{
				break;
			}
			num = key;
		}
		return num + 1;
	}

	public int GetLastIndex()
	{
		if (Items.Keys.IsNullOrEmpty())
		{
			return -1;
		}
		return Items.Keys.Last();
	}
}
