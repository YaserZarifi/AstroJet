using System;
using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class ItemFly : Transformation
{
	public HeroItem MyItem;

	public Vector2 TargetPosition;

	private ShopMenu _shopMenu;

	private int _targetCargoSlotIndex;

	private float _moveSpeed;

	public ItemFly(ShopMenu shopMenu)
	{
		_shopMenu = shopMenu;
	}

	private Vector2 GetStartPositionByItem(HeroItem item)
	{
		if (item != null)
		{
			switch (item?.Place)
			{
			case ItemPlaceEnum.InHeroSlot:
				return _shopMenu.BigShipSlotSet.SlotArray[(int)item.MySlot.SlotType].WorldPosition;
			case ItemPlaceEnum.InCargo:
				return _shopMenu.PanelCargo.GetSlotPosition(item.CargoIndex);
			case ItemPlaceEnum.InShop:
				return _shopMenu.PanelShop.GetSlotPosition(item);
			}
		}
		return Vector2.Zero;
	}

	public void StartToCargo(HeroItem item, int targetCargoSlotIndex)
	{
		MyItem = item;
		_targetCargoSlotIndex = targetCargoSlotIndex;
		Position = GetStartPositionByItem(item);
		TargetPosition = _shopMenu.PanelCargo.GetSlotPosition(_targetCargoSlotIndex);
		_moveSpeed = Utils.Clamp((TargetPosition - Position).Length() * 5f, 700f, 3500f);
		GameCore.PlaySound("menu_item_move");
	}

	public void StartToHeroSlot(HeroItem item, HeroSlotEnum slotType)
	{
		MyItem = item;
		_targetCargoSlotIndex = -1;
		Position = GetStartPositionByItem(item);
		TargetPosition = _shopMenu.BigShipSlotSet.SlotArray[(int)slotType].WorldPosition;
		_moveSpeed = Utils.Clamp((TargetPosition - Position).Length() * 5f, 700f, 3500f);
		GameCore.PlaySound("menu_item_move");
	}

	public void StartToShop(HeroItem item)
	{
		MyItem = item;
		_targetCargoSlotIndex = -1;
		Position = GetStartPositionByItem(item);
		_moveSpeed = Utils.Clamp((TargetPosition - Position).Length() * 5f, 700f, 3500f);
	}

	public void Update(float elapsedTime)
	{
		if (MyItem == null)
		{
			return;
		}
		if (MyItem.Place == ItemPlaceEnum.InShop)
		{
			if (_shopMenu.CurrentScreen == ShopScreenEnum.Shop)
			{
				TargetPosition = _shopMenu.PanelShop.GetSlotPosition(_shopMenu.PanelShop.GetSlotIndex(MyItem));
			}
			else
			{
				TargetPosition = new Vector2(1970f, Position.Y);
			}
		}
		else if (_targetCargoSlotIndex >= 0)
		{
			if (_shopMenu.CurrentScreen == ShopScreenEnum.Shop)
			{
				TargetPosition = _shopMenu.PanelCargo.GetSlotPosition(_targetCargoSlotIndex);
				TargetPosition.Y = Math.Min(950f, TargetPosition.Y);
			}
			else
			{
				TargetPosition = new Vector2(-50f, Position.Y);
			}
		}
		TargetPosition = Utils.Clamp(TargetPosition, new Vector2(-50f, 0f), new Vector2(1920f, 1080f));
		_moveSpeed = Utils.Clamp((TargetPosition - Position).Length() * 5f, 2000f, 3500f);
		if (Utils.ChangeValueToTarget(ref Position, TargetPosition, _moveSpeed * elapsedTime))
		{
			MyItem = null;
			_shopMenu.ItemFlySet.PlayPlaceSound();
		}
	}

	public void Draw()
	{
		Color color = new Color(255, 255, 255, 255);
		if (MyItem != null && MyItem.Data.IconAtlas != null)
		{
			_shopMenu.SpriteBatchBegin(bLight: true);
			MyItem.Data.IconAtlas.DrawFrame(_shopMenu.SpriteBatchLight, MyItem.GetIconFrame(), color, this);
			_shopMenu.SpriteBatchEnd(bLight: true);
		}
	}
}
