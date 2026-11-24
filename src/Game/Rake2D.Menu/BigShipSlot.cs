using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class BigShipSlot : Transformation
{
	public HeroSlot HeroSlot;

	private ShopMenu _shopMenu;

	public BigShipSlot(ShopMenu shopMenu, HeroSlot heroSlot)
	{
		_shopMenu = shopMenu;
		HeroSlot = heroSlot;
		Init();
	}

	public void Init()
	{
	}

	public bool IsCompatible(HeroItem item)
	{
		if (_shopMenu.CurrentScreen == ShopScreenEnum.Mia)
		{
			return false;
		}
		return HeroSlot.IsCompatible(item);
	}

	public void Update(float elapsedTime)
	{
		ResetPrecalculations();
		switch (HeroSlot.SlotType)
		{
		case HeroSlotEnum.top_gun:
			Position = _shopMenu.BigShipSlotSet.FrontGunPadPosition + new Vector2(105f, 36f);
			break;
		case HeroSlotEnum.front_gun:
			Position = _shopMenu.BigShipSlotSet.FrontGunPadPosition + new Vector2(141f, 106f);
			break;
		case HeroSlotEnum.bottom_gun:
			Position = _shopMenu.BigShipSlotSet.FrontGunPadPosition + new Vector2(105f, 176f);
			break;
		case HeroSlotEnum.rear_gun:
			Position = _shopMenu.BigShipSlotSet.RearGunPadPosition + new Vector2(33f, 45f);
			break;
		case HeroSlotEnum.gem_top:
			Position = _shopMenu.BigShipSlotSet.FrontGunPadPosition + new Vector2(22f, 37f);
			break;
		case HeroSlotEnum.gem_front:
			Position = _shopMenu.BigShipSlotSet.FrontGunPadPosition + new Vector2(57f, 107f);
			break;
		case HeroSlotEnum.gem_bottom:
			Position = _shopMenu.BigShipSlotSet.FrontGunPadPosition + new Vector2(22f, 177f);
			break;
		case HeroSlotEnum.gem_rear:
			Position = _shopMenu.BigShipSlotSet.RearGunPadPosition + new Vector2(116f, 47f);
			break;
		case HeroSlotEnum.missile:
			Position = _shopMenu.BigShipSlotSet.HexPadPosition + new Vector2(82f, 34f);
			break;
		case HeroSlotEnum.bomb:
			Position = _shopMenu.BigShipSlotSet.HexPadPosition + new Vector2(82f, 176f);
			break;
		case HeroSlotEnum.primary_device:
			Position = _shopMenu.BigShipSlotSet.HexPadPosition + new Vector2(34f, 106f);
			break;
		case HeroSlotEnum.secondary_device:
			Position = _shopMenu.BigShipSlotSet.HexPadPosition + new Vector2(129f, 106f);
			break;
		case HeroSlotEnum.equipment_1:
			Position = _shopMenu.BigShipSlotSet.EquipPadPosition + new Vector2(29 + (int)(HeroSlot.SlotType - 8) * 87, 52f);
			break;
		case HeroSlotEnum.equipment_2:
			Position = _shopMenu.BigShipSlotSet.EquipPadPosition + new Vector2(29 + (int)(HeroSlot.SlotType - 8) * 87, 52f);
			break;
		case HeroSlotEnum.equipment_3:
			Position = _shopMenu.BigShipSlotSet.EquipPadPosition + new Vector2(29 + (int)(HeroSlot.SlotType - 8) * 87, 52f);
			break;
		case HeroSlotEnum.equipment_4:
			Position = _shopMenu.BigShipSlotSet.EquipPadPosition + new Vector2(29 + (int)(HeroSlot.SlotType - 8) * 87, 52f);
			break;
		case HeroSlotEnum.equipment_5:
			Position = _shopMenu.BigShipSlotSet.EquipPadPosition + new Vector2(29 + (int)(HeroSlot.SlotType - 8) * 87, 52f);
			break;
		case HeroSlotEnum.equipment_6:
			Position = _shopMenu.BigShipSlotSet.EquipPadPosition + new Vector2(29 + (int)(HeroSlot.SlotType - 8) * 87, 52f);
			break;
		}
	}

	public void Draw()
	{
		SpriteAtlas spriteAtlas = (HeroSlot.IsGemType ? _shopMenu.AtlasGemSlot : _shopMenu.AtlasSlot);
		HeroItem tVItem = _shopMenu.PanelInfo.TVItem;
		if (tVItem != null && tVItem.Loadout != null)
		{
			HeroItem heroItem = null;
			if (tVItem.Loadout != null)
			{
				LoadoutEntry entryForSlot = tVItem.Loadout.GetEntryForSlot(HeroSlot);
				if (entryForSlot != null)
				{
					heroItem = new HeroItem(entryForSlot.ItemData);
					heroItem.Level = entryForSlot.ItemLevel;
				}
			}
			_shopMenu.DrawSlot(spriteAtlas, WorldTransformation, heroItem, (float)(int)_shopMenu.BigShipSlotSet.Color.A / 255f, bAsLoadoutPreview: true);
		}
		else
		{
			_shopMenu.DrawSlot(spriteAtlas, WorldTransformation, HeroSlot.Item, (float)(int)_shopMenu.BigShipSlotSet.Color.A / 255f, bAsLoadoutPreview: false);
			Color color = new Color(1f, 1f, 1f, _shopMenu.ShowHidePercentage);
			if (HeroSlot.Item != null && HeroSlot.Item.IsBlockedBySameItem)
			{
				spriteAtlas.DrawFrame(_shopMenu.SpriteBatchLight, 4, color, WorldTransformation);
			}
			if (_shopMenu.SelectedItem != null && !IsCompatible(_shopMenu.SelectedItem) && _shopMenu.SelectedItem.Data.ItemType != ItemTypeEnum.Loadout)
			{
				spriteAtlas.DrawFrame(_shopMenu.SpriteBatchLight, 3, color, WorldTransformation, BlendModeEnum.Normal);
			}
		}
	}
}
