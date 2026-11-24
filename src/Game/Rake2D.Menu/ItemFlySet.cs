using System.Collections.Generic;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class ItemFlySet
{
	public float PlaceSoundTimer;

	private List<ItemFly> _flyList;

	private ShopMenu _shopMenu;

	public ItemFlySet(ShopMenu shopMenu)
	{
		_shopMenu = shopMenu;
		_flyList = new List<ItemFly>();
	}

	public bool Contains(HeroItem item)
	{
		foreach (ItemFly fly in _flyList)
		{
			if (fly.MyItem == item)
			{
				return true;
			}
		}
		return false;
	}

	private ItemFly GetItemFlyForItem(HeroItem item)
	{
		foreach (ItemFly fly in _flyList)
		{
			if (fly.MyItem == item)
			{
				return fly;
			}
		}
		ItemFly itemFly = new ItemFly(_shopMenu);
		_flyList.Add(itemFly);
		return itemFly;
	}

	public void PlayPlaceSound()
	{
		if (PlaceSoundTimer <= 0f)
		{
			GameCore.PlaySound("menu_item_place");
			PlaceSoundTimer = 0.1f;
		}
	}

	public void StartToCargo(HeroItem item, int targetCargoSlotIndex)
	{
		GetItemFlyForItem(item).StartToCargo(item, targetCargoSlotIndex);
	}

	public void StartToHeroSlot(HeroItem item, HeroSlotEnum slotType)
	{
		GetItemFlyForItem(item).StartToHeroSlot(item, slotType);
	}

	public void StartToShop(HeroItem item)
	{
		GetItemFlyForItem(item).StartToShop(item);
	}

	public void Update(float elapsedTime)
	{
		PlaceSoundTimer -= elapsedTime;
		for (int num = _flyList.Count - 1; num >= 0; num--)
		{
			ItemFly itemFly = _flyList[num];
			if (itemFly.MyItem == null)
			{
				_flyList.RemoveAt(num);
			}
			else
			{
				itemFly.Update(elapsedTime);
			}
		}
	}

	public void Draw()
	{
		foreach (ItemFly fly in _flyList)
		{
			if (fly.MyItem != null)
			{
				fly.Draw();
			}
		}
	}
}
