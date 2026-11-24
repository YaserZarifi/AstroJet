using System;
using System.Collections.Generic;
using System.Linq;
using Rake2D.Menu;

namespace Rake2D.JnG;

public class ShopOffer
{
	public enum orderModeEnum
	{
		ByName,
		ByPrice
	}

	public orderModeEnum OrderMode;

	public List<HeroItem> ItemList;

	public ShopOffer()
	{
		ItemList = new List<HeroItem>();
	}

	public void Clear()
	{
		ItemList.Clear();
	}

	public void Order()
	{
		switch (OrderMode)
		{
		case orderModeEnum.ByPrice:
			ItemList = (from item in ItemList
				orderby (int)ShopDivider.GetDividerType(item), item.Price
				select item).ToList();
			break;
		case orderModeEnum.ByName:
			ItemList = (from item in ItemList
				orderby (int)ShopDivider.GetDividerType(item), item.GetShopName()
				select item).ToList();
			break;
		}
		GameJnG.ShopMenu.PanelShop.PrepareSlots();
	}

	private int GetItemCount(HeroItemData data)
	{
		if (data.IsShopTypeWeapon)
		{
			return Utils.GetRandom(1, 3);
		}
		if (data.ItemType == ItemTypeEnum.Gem)
		{
			return Utils.GetRandom(1, 3);
		}
		return 1;
	}

	private List<HeroItemData> GetItemDataList(List<HeroItemData> dataList, int maxCount)
	{
		List<HeroItemData> list = new List<HeroItemData>(dataList);
		List<HeroItemData> list2 = dataList.Where((HeroItemData d) => d.ShopStageRange.From == GameJnG.StageNumber + Math.Max(1, GameJnG.Difficulty) * 1000).ToList();
		foreach (HeroItemData item in list2)
		{
			if (list.Contains(item))
			{
				list.Remove(item);
			}
		}
		list2.AddList(list.GetRandomItemList(maxCount - list2.Count));
		return list2;
	}

	private void AddItems(List<HeroItemData> dataList, int maxCount, bool bMultipleItems)
	{
		if (maxCount < 99999)
		{
			foreach (HeroItemData itemData in GetItemDataList(dataList.Where((HeroItemData d) => d.IsShopTypeWeapon).ToList(), 14))
			{
				AddItem(new HeroItem(itemData));
			}
			foreach (HeroItemData itemData2 in GetItemDataList(dataList.Where((HeroItemData d) => d.IsShopTypeGem).ToList(), 5))
			{
				AddItem(new HeroItem(itemData2));
			}
			foreach (HeroItemData itemData3 in GetItemDataList(dataList.Where((HeroItemData d) => d.IsShopTypeMissile).ToList(), 5))
			{
				AddItem(new HeroItem(itemData3));
			}
			foreach (HeroItemData itemData4 in GetItemDataList(dataList.Where((HeroItemData d) => d.IsShopTypeBomb).ToList(), 5))
			{
				AddItem(new HeroItem(itemData4));
			}
			foreach (HeroItemData itemData5 in GetItemDataList(dataList.Where((HeroItemData d) => d.IsShopTypeEquipment).ToList(), 5))
			{
				AddItem(new HeroItem(itemData5));
			}
			foreach (HeroItemData itemData6 in GetItemDataList(dataList.Where((HeroItemData d) => d.IsShopTypeDevice).ToList(), 5))
			{
				AddItem(new HeroItem(itemData6));
			}
			foreach (HeroItemData itemData7 in GetItemDataList(dataList.Where((HeroItemData d) => d.IsShopTypeOther).ToList(), 5))
			{
				AddItem(new HeroItem(itemData7));
			}
		}
		else
		{
			foreach (HeroItemData data in dataList)
			{
				if (bMultipleItems)
				{
					for (int num = GetItemCount(data); num > 0; num--)
					{
						AddItem(new HeroItem(data));
					}
				}
				else
				{
					AddItem(new HeroItem(data));
				}
			}
		}
		GameJnG.ShopMenu.PanelShop.PrepareSlots();
	}

	private List<HeroItemData> GetAllAvailableItems(int shopStage)
	{
		List<HeroItemData> list = new List<HeroItemData>(from data in GameCore.CurrentLevel.Content.GetHeroItemDataList()
			where data.ShopStageRange.From <= shopStage && data.ShopStageRange.To >= shopStage
			select data);
		list.AddRange(from data in GameCore.GlobalContent.GetHeroItemDataList()
			where data.ShopStageRange.From <= shopStage && data.ShopStageRange.To >= shopStage
			select data);
		return list;
	}

	private List<HeroItemData> GetAllItems()
	{
		List<HeroItemData> list = new List<HeroItemData>(GameCore.CurrentLevel.Content.GetHeroItemDataList());
		list.AddRange(GameCore.GlobalContent.GetHeroItemDataList());
		return list;
	}

	public void Build(int shopStage, int maxCount = -1)
	{
		Clear();
		List<HeroItemData> list;
		bool bMultipleItems;
		if (shopStage >= 999999)
		{
			list = GetAllItems();
			bMultipleItems = false;
		}
		else
		{
			bMultipleItems = false;
			if (GameJnG.GameMode == GameModeEnum.Casual)
			{
				list = new List<HeroItemData>(from data in GameCore.CurrentLevel.Content.GetHeroItemDataList()
					where data.ShopStageRange.From >= 0 && data.ShopStageRange.To >= 0
					select data);
				list.AddRange(from data in GameCore.GlobalContent.GetHeroItemDataList()
					where data.ShopStageRange.From >= 0 && data.ShopStageRange.To >= 0
					select data);
			}
			else
			{
				list = GetAllAvailableItems(shopStage + Math.Max(1, GameJnG.Difficulty) * 1000);
			}
		}
		if (maxCount < 0)
		{
			maxCount = 5;
		}
		AddItems(list, maxCount, bMultipleItems);
		Order();
		GameJnG.RecalculateItemParams(null);
	}

	public void AddItem(HeroItem item)
	{
		if (item != null)
		{
			ItemList.Add(item);
			item.CargoIndex = -1;
			item.IsInShop = true;
			item.SetSlot(null);
		}
	}

	public void RemoveItem(HeroItem item)
	{
		if (ItemList.Contains(item))
		{
			ItemList.Remove(item);
			GameJnG.ShopMenu.PanelShop.PrepareSlots();
		}
	}
}
