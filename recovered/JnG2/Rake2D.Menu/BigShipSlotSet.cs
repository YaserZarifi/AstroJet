using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class BigShipSlotSet : ShopMenuSection
{
	public ShopMenuState State;

	public Hero MyHero;

	public BigShipSlot[] SlotArray;

	public BigShipSlot CursorSlot;

	public Vector2 EquipPadPosition;

	public Vector2 FrontGunPadPosition;

	public Vector2 RearGunPadPosition;

	public Vector2 HexPadPosition;

	public Vector2 TargetPosition;

	private ShopMenu _shopMenu;

	private SpriteAtlas _atlasBarbiePad;

	private SpriteAtlas _atlasSlotConnection;

	private Button _buttonPlayerSwitch;

	private float _ledFadeFactor;

	private int _slotConenectonFrame;

	private float _slotConenectonFrameTimer;

	public bool IsPlayerButtonSelected => _buttonPlayerSwitch.IsSelected;

	public BigShipSlotSet(ShopMenu menu)
	{
		_shopMenu = menu;
		Enum.GetValues(typeof(HeroSlotEnum));
	}

	public void Init(Hero hero)
	{
		MyHero = hero;
		SlotArray = new BigShipSlot[18];
		_atlasBarbiePad = GameCore.GetSpriteAtlas("shop_barbie_pad", logWhenNotFound: true);
		_atlasSlotConnection = GameCore.GetSpriteAtlas("shop_slot_connection", logWhenNotFound: true);
		Position = new Vector2(345f, 800f);
		foreach (KeyValuePair<HeroSlotEnum, HeroSlot> slot in MyHero.MyShip.Slots)
		{
			BigShipSlot bigShipSlot = new BigShipSlot(_shopMenu, slot.Value);
			SlotArray[(int)slot.Key] = bigShipSlot;
		}
		_buttonPlayerSwitch = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonPlayerSwitch.OnClick += OnPlayerSwitchClick;
	}

	public void SetTexts()
	{
		if (_buttonPlayerSwitch != null)
		{
			_buttonPlayerSwitch.Caption = TextBundle.GetText($"button_player_switch_{MyHero.PlayerNumber + 1}");
		}
	}

	public void SelectPlayerButton()
	{
		CancelCursorSelection();
		_buttonPlayerSwitch.Select();
	}

	public void UnselectPlayerButton()
	{
		_buttonPlayerSwitch?.Unselect();
	}

	private void OnPlayerSwitchClick(Button button)
	{
		_shopMenu.SwapHero();
	}

	public void Show()
	{
		State = ShopMenuState.Showing;
		Position = new Vector2(345f, 800f);
		TargetPosition = new Vector2(345f, 469f);
		_buttonPlayerSwitch.IsVisible = GameJnG.PlayerCount > 1;
	}

	public void Hide()
	{
		State = ShopMenuState.Hiding;
		TargetPosition = new Vector2(345f, 1200f);
	}

	public void CancelSelection()
	{
	}

	public override void CancelCursorSelection()
	{
		CursorSlot = null;
	}

	public void RemoveItemToCargo(HeroItem item)
	{
		if (item != null && item.Place == ItemPlaceEnum.InHeroSlot)
		{
			HeroSlot mySlot = item.MySlot;
			int firstEmptyIndex = GameJnG.Cargo.GetFirstEmptyIndex();
			_shopMenu.ItemFlySet.StartToCargo(item, firstEmptyIndex);
			GameJnG.Cargo.SetItem(firstEmptyIndex, item);
			_shopMenu.PanelCargo.ScrollToSlot(firstEmptyIndex);
			mySlot.SetItem(null);
			_shopMenu.CancelSelection();
		}
	}

	private void ClickToSlot(BigShipSlot slot)
	{
		HeroItem item = slot.HeroSlot.Item;
		if (item != null && item != _shopMenu.SelectedItem)
		{
			SelectSlotItem(slot);
			SetCursorSlot(slot);
		}
		else
		{
			RightClickToSlot(slot);
		}
	}

	private void RightClickToSlot(BigShipSlot slot)
	{
		bool flag = false;
		HeroItem item = slot.HeroSlot.Item;
		if (_shopMenu.SelectedItem == null)
		{
			if (item != null)
			{
				SelectSlotItem(slot);
				SetCursorSlot(slot);
			}
		}
		else if (item == _shopMenu.SelectedItem)
		{
			_shopMenu.CancelSelection();
			GameCore.PlaySound("menu_item_tv_hide");
		}
		else if (_shopMenu.SelectedItem.Place == ItemPlaceEnum.InShop)
		{
			if (!slot.IsCompatible(_shopMenu.SelectedItem))
			{
				_shopMenu.AddCursorMessage(TextBundle.GetText("cursor_text_incompatible_slot"), slot.Position + ShopMenu.SLOT_CENTER_OFFSET, Color.Red);
				GameCore.PlaySound("menu_denied");
			}
			else if (_shopMenu.SelectedItem.IsTooExpensive)
			{
				_shopMenu.AddCursorMessage(TextBundle.GetText("cursor_text_too_expensive"), slot.Position + ShopMenu.SLOT_CENTER_OFFSET, Color.Red);
				GameCore.PlaySound("menu_denied");
			}
			else
			{
				BuyItem(slot, _shopMenu.SelectedItem);
				flag = true;
			}
		}
		else if (_shopMenu.SelectedItem.Place == ItemPlaceEnum.InHeroSlot)
		{
			if (slot.IsCompatible(_shopMenu.SelectedItem))
			{
				_shopMenu.ItemFlySet.StartToHeroSlot(_shopMenu.SelectedItem, slot.HeroSlot.SlotType);
				if (item != null)
				{
					_shopMenu.ItemFlySet.StartToHeroSlot(item, _shopMenu.SelectedItem.MySlot.SlotType);
					_shopMenu.SelectedItem.MySlot.SetItem(item);
				}
				else
				{
					_shopMenu.SelectedItem.MySlot.SetItem(null);
				}
				slot.HeroSlot.SetItem(_shopMenu.SelectedItem);
				flag = true;
			}
			else
			{
				_shopMenu.AddCursorMessage(TextBundle.GetText("cursor_text_incompatible_slot"), slot.Position + ShopMenu.SLOT_CENTER_OFFSET, Color.Red);
				GameCore.PlaySound("menu_denied");
			}
		}
		else if (_shopMenu.SelectedItem.Place == ItemPlaceEnum.InCargo)
		{
			HeroItem selectedItem = _shopMenu.SelectedItem;
			if (selectedItem == null || slot.IsCompatible(selectedItem))
			{
				_shopMenu.ItemFlySet.StartToHeroSlot(selectedItem, slot.HeroSlot.SlotType);
				if (item != null)
				{
					_shopMenu.ItemFlySet.StartToCargo(item, selectedItem.CargoIndex);
					GameJnG.Cargo.SetItem(selectedItem.CargoIndex, item);
					flag = true;
				}
				else
				{
					GameJnG.Cargo.RemoveItem(selectedItem);
					flag = true;
				}
				if (flag)
				{
					slot.HeroSlot.SetItem(selectedItem);
				}
			}
			else
			{
				_shopMenu.AddCursorMessage(TextBundle.GetText("cursor_text_incompatible_slot"), slot.Position + ShopMenu.SLOT_CENTER_OFFSET, Color.Red);
				GameCore.PlaySound("menu_denied");
			}
		}
		if (flag)
		{
			_shopMenu.CancelSelection();
			_shopMenu.ChangeFocus(this);
		}
	}

	private void DoubleClickToSlot(BigShipSlot slot)
	{
		if (_shopMenu.CurrentScreen != ShopScreenEnum.Mia)
		{
			HeroItem item = slot.HeroSlot.Item;
			if (item != null)
			{
				BigShipSlot cursorSlot = CursorSlot;
				RemoveItemToCargo(item);
				CursorSlot = cursorSlot;
			}
		}
	}

	public bool IsAnySlotFreeForItem(HeroItem item)
	{
		BigShipSlot[] slotArray = SlotArray;
		foreach (BigShipSlot bigShipSlot in slotArray)
		{
			if (bigShipSlot.HeroSlot.Item == null && bigShipSlot.IsCompatible(item))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryAddItemToFreeSlot(HeroItem item)
	{
		BigShipSlot[] slotArray = SlotArray;
		foreach (BigShipSlot bigShipSlot in slotArray)
		{
			if (bigShipSlot.HeroSlot.Item == null && bigShipSlot.IsCompatible(item))
			{
				_shopMenu.ItemFlySet.StartToHeroSlot(item, bigShipSlot.HeroSlot.SlotType);
				bigShipSlot.HeroSlot.SetItem(item);
				return true;
			}
		}
		return false;
	}

	public void BuyItem(BigShipSlot slot, HeroItem shopItem)
	{
		_shopMenu.BuyItem(shopItem);
		_shopMenu.ItemFlySet.StartToHeroSlot(shopItem, slot.HeroSlot.SlotType);
		GameJnG.ShopOffer.RemoveItem(shopItem);
		HeroItem item = slot.HeroSlot.Item;
		if (item != null)
		{
			RemoveItemToCargo(item);
		}
		slot.HeroSlot.SetItem(shopItem);
	}

	public void SelectSlotItem(BigShipSlot slot)
	{
		_shopMenu.CancelSelection();
		_shopMenu.SetSelectedItem(slot.HeroSlot.Item);
		_shopMenu.SetMode(SelectionModeEnum.Selected);
	}

	public void SetCursorSlot(BigShipSlot slot)
	{
		_shopMenu.ChangeFocus(this);
		if (CursorSlot != slot)
		{
			CursorSlot = slot;
			GameCore.PlaySound("menu_item_select");
		}
		_shopMenu.SetCursorItem(slot.HeroSlot.Item);
		_shopMenu.IsCursorOnSlot = true;
		_shopMenu.PanelInfo.UnselectAllButtons();
		_shopMenu.PanelCargo.UnselectSortButton();
	}

	public void SetCursorSlotByType(HeroSlotEnum type)
	{
		if (type < HeroSlotEnum.MaxSlotCount)
		{
			SetCursorSlot(SlotArray[(int)type]);
		}
	}

	private void ParseInput()
	{
		BigShipSlot bigShipSlot = CursorSlot;
		if (bigShipSlot == null && _shopMenu.SelectedItem != null && _shopMenu.SelectedItem.Place == ItemPlaceEnum.InHeroSlot && (_shopMenu.CursorItem == null || _shopMenu.CursorItem.Place == ItemPlaceEnum.InHeroSlot) && _shopMenu.PanelCargo.CursorSlotIndex < 0 && !_shopMenu.PanelInfo.IsAnyButtonSelected)
		{
			bigShipSlot = SlotArray[(int)_shopMenu.SelectedItem.MySlot.SlotType];
		}
		if (bigShipSlot == null)
		{
			return;
		}
		if (_shopMenu.PanelTopButtons.State == ShopMenuState.Visible && CursorSlot != null && _shopMenu.KeyPressed(ShopMenuKeys.MoveUpWithControl))
		{
			_shopMenu.PanelTopButtons.SelectButtonByX(bigShipSlot.Position.X);
			CancelCursorSelection();
		}
		else if (_shopMenu.CurrentScreen != ShopScreenEnum.Mia && CursorSlot != null && _shopMenu.KeyPressed(ShopMenuKeys.MoveDownWithControl))
		{
			if (_shopMenu.PanelInfo.SelectDefaultButton())
			{
				_shopMenu.CancelCursorSelection();
			}
		}
		else if (CursorSlot != null && _shopMenu.KeyPressed(ShopMenuKeys.MoveLeftWithControl))
		{
			if (_shopMenu.CurrentScreen == ShopScreenEnum.Shop)
			{
				_shopMenu.PanelCargo.SetCursorSlotByY(bigShipSlot.Position.Y - 36f, 2);
			}
			else if (_shopMenu.CurrentScreen != ShopScreenEnum.Mia)
			{
				_shopMenu.PanelHeroStats.SelectFaceTV();
			}
		}
		else if (CursorSlot != null && _shopMenu.KeyPressed(ShopMenuKeys.MoveRightWithControl))
		{
			if (_shopMenu.CurrentScreen == ShopScreenEnum.Shop)
			{
				_shopMenu.PanelShop.SetCursorSlotByY(bigShipSlot.Position.Y - 36f);
			}
			else if (_shopMenu.CurrentScreen != ShopScreenEnum.Mia)
			{
				_shopMenu.PanelShipParams.SelectSliderByY(bigShipSlot.Position.Y - 20f);
			}
			else if (_shopMenu.CurrentScreen == ShopScreenEnum.Mia)
			{
				_shopMenu.PanelMia.SelectDefaultButton();
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp))
		{
			if (CursorSlot != null && CursorSlot.HeroSlot.SlotType == HeroSlotEnum.rear_gun && _shopMenu.CurrentScreen == ShopScreenEnum.Shop)
			{
				CancelCursorSelection();
				_shopMenu.PanelCargo.SelectSortButton();
				return;
			}
			if (CursorSlot != null && _shopMenu.PanelTopButtons.State == ShopMenuState.Visible && (CursorSlot.HeroSlot.SlotType == HeroSlotEnum.missile || CursorSlot.HeroSlot.SlotType == HeroSlotEnum.rear_gun || CursorSlot.HeroSlot.SlotType == HeroSlotEnum.gem_rear || CursorSlot.HeroSlot.SlotType == HeroSlotEnum.top_gun || CursorSlot.HeroSlot.SlotType == HeroSlotEnum.gem_top))
			{
				_shopMenu.PanelTopButtons.SelectButtonByX(bigShipSlot.Position.X);
				return;
			}
			if (bigShipSlot.HeroSlot.SlotType >= HeroSlotEnum.equipment_1 && bigShipSlot.HeroSlot.SlotType <= HeroSlotEnum.equipment_6 && _shopMenu.CurrentScreen != ShopScreenEnum.Mia)
			{
				_shopMenu.SelectTestRangeButton();
				return;
			}
			switch (bigShipSlot.HeroSlot.SlotType)
			{
			case HeroSlotEnum.top_gun:
				SetCursorSlotByType(HeroSlotEnum.bottom_gun);
				break;
			case HeroSlotEnum.front_gun:
				SetCursorSlotByType(HeroSlotEnum.top_gun);
				break;
			case HeroSlotEnum.bottom_gun:
				SetCursorSlotByType(HeroSlotEnum.front_gun);
				break;
			case HeroSlotEnum.rear_gun:
				SetCursorSlotByType(HeroSlotEnum.rear_gun);
				break;
			case HeroSlotEnum.gem_front:
				SetCursorSlotByType(HeroSlotEnum.gem_top);
				break;
			case HeroSlotEnum.gem_bottom:
				SetCursorSlotByType(HeroSlotEnum.gem_front);
				break;
			case HeroSlotEnum.primary_device:
			case HeroSlotEnum.secondary_device:
				SetCursorSlotByType(HeroSlotEnum.missile);
				break;
			case HeroSlotEnum.bomb:
				SetCursorSlotByType(HeroSlotEnum.primary_device);
				break;
			case HeroSlotEnum.missile:
			case HeroSlotEnum.equipment_1:
			case HeroSlotEnum.equipment_2:
			case HeroSlotEnum.equipment_3:
			case HeroSlotEnum.equipment_4:
			case HeroSlotEnum.equipment_5:
			case HeroSlotEnum.equipment_6:
			case HeroSlotEnum.gem_top:
				break;
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveDown))
		{
			switch (bigShipSlot.HeroSlot.SlotType)
			{
			case HeroSlotEnum.top_gun:
				SetCursorSlotByType(HeroSlotEnum.front_gun);
				break;
			case HeroSlotEnum.front_gun:
				SetCursorSlotByType(HeroSlotEnum.bottom_gun);
				break;
			case HeroSlotEnum.rear_gun:
			case HeroSlotEnum.gem_rear:
				if (_buttonPlayerSwitch.IsVisible)
				{
					_shopMenu.SelectPlayerButton();
				}
				break;
			case HeroSlotEnum.gem_top:
				SetCursorSlotByType(HeroSlotEnum.gem_front);
				break;
			case HeroSlotEnum.gem_front:
				SetCursorSlotByType(HeroSlotEnum.gem_bottom);
				break;
			case HeroSlotEnum.primary_device:
			case HeroSlotEnum.secondary_device:
				SetCursorSlotByType(HeroSlotEnum.bomb);
				break;
			case HeroSlotEnum.missile:
				SetCursorSlotByType(HeroSlotEnum.primary_device);
				break;
			case HeroSlotEnum.bottom_gun:
			case HeroSlotEnum.bomb:
			case HeroSlotEnum.equipment_1:
			case HeroSlotEnum.equipment_2:
			case HeroSlotEnum.equipment_3:
			case HeroSlotEnum.equipment_4:
			case HeroSlotEnum.equipment_5:
			case HeroSlotEnum.equipment_6:
			case HeroSlotEnum.gem_bottom:
				if (_shopMenu.CurrentScreen != ShopScreenEnum.Mia && _shopMenu.PanelInfo.SelectDefaultButton())
				{
					_shopMenu.CancelCursorSelection();
				}
				break;
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeft))
		{
			switch (bigShipSlot.HeroSlot.SlotType)
			{
			case HeroSlotEnum.top_gun:
				SetCursorSlotByType(HeroSlotEnum.gem_top);
				break;
			case HeroSlotEnum.front_gun:
			{
				HeroSlotEnum cursorSlotByType = HeroSlotEnum.gem_front;
				SetCursorSlotByType(cursorSlotByType);
				break;
			}
			case HeroSlotEnum.bottom_gun:
				SetCursorSlotByType(HeroSlotEnum.gem_bottom);
				break;
			case HeroSlotEnum.rear_gun:
				if (_shopMenu.CurrentScreen == ShopScreenEnum.Shop)
				{
					_shopMenu.PanelCargo.SetVisibleCursorSlot(6, 2);
					break;
				}
				_shopMenu.CancelCursorSelection();
				_shopMenu.PanelHeroStats.HighliteMedal(5);
				break;
			case HeroSlotEnum.gem_top:
				SetCursorSlotByType(HeroSlotEnum.missile);
				break;
			case HeroSlotEnum.gem_front:
				SetCursorSlotByType(HeroSlotEnum.secondary_device);
				break;
			case HeroSlotEnum.gem_bottom:
				SetCursorSlotByType(HeroSlotEnum.bomb);
				break;
			case HeroSlotEnum.gem_rear:
				SetCursorSlotByType(HeroSlotEnum.rear_gun);
				break;
			case HeroSlotEnum.primary_device:
				SetCursorSlotByType(HeroSlotEnum.gem_rear);
				break;
			case HeroSlotEnum.secondary_device:
				SetCursorSlotByType(HeroSlotEnum.primary_device);
				break;
			case HeroSlotEnum.missile:
				SetCursorSlotByType(HeroSlotEnum.gem_rear);
				break;
			case HeroSlotEnum.bomb:
			{
				HeroSlotEnum cursorSlotByType = HeroSlotEnum.primary_device;
				SetCursorSlotByType(cursorSlotByType);
				break;
			}
			case HeroSlotEnum.equipment_1:
				SetCursorSlotByType(HeroSlotEnum.bottom_gun);
				break;
			case HeroSlotEnum.equipment_2:
			case HeroSlotEnum.equipment_3:
			case HeroSlotEnum.equipment_4:
			case HeroSlotEnum.equipment_5:
			case HeroSlotEnum.equipment_6:
				if (CursorSlot != null)
				{
					SetCursorSlotByType(CursorSlot.HeroSlot.SlotType - 1);
				}
				break;
			}
		}
		else
		{
			if (!_shopMenu.KeyPressed(ShopMenuKeys.MoveRight))
			{
				return;
			}
			switch (bigShipSlot.HeroSlot.SlotType)
			{
			case HeroSlotEnum.top_gun:
				SetCursorSlotByType(HeroSlotEnum.equipment_1);
				break;
			case HeroSlotEnum.front_gun:
				SetCursorSlotByType(HeroSlotEnum.equipment_1);
				break;
			case HeroSlotEnum.bottom_gun:
				SetCursorSlotByType(HeroSlotEnum.equipment_1);
				break;
			case HeroSlotEnum.rear_gun:
			{
				HeroSlotEnum cursorSlotByType = HeroSlotEnum.gem_rear;
				SetCursorSlotByType(cursorSlotByType);
				break;
			}
			case HeroSlotEnum.gem_top:
				SetCursorSlotByType(HeroSlotEnum.top_gun);
				break;
			case HeroSlotEnum.gem_front:
				SetCursorSlotByType(HeroSlotEnum.front_gun);
				break;
			case HeroSlotEnum.gem_bottom:
				SetCursorSlotByType(HeroSlotEnum.bottom_gun);
				break;
			case HeroSlotEnum.gem_rear:
				SetCursorSlotByType(HeroSlotEnum.primary_device);
				break;
			case HeroSlotEnum.primary_device:
				SetCursorSlotByType(HeroSlotEnum.secondary_device);
				break;
			case HeroSlotEnum.secondary_device:
				SetCursorSlotByType(HeroSlotEnum.gem_front);
				break;
			case HeroSlotEnum.missile:
				SetCursorSlotByType(HeroSlotEnum.gem_top);
				break;
			case HeroSlotEnum.bomb:
				SetCursorSlotByType(HeroSlotEnum.gem_bottom);
				break;
			case HeroSlotEnum.equipment_1:
			case HeroSlotEnum.equipment_2:
			case HeroSlotEnum.equipment_3:
			case HeroSlotEnum.equipment_4:
			case HeroSlotEnum.equipment_5:
				if (CursorSlot != null)
				{
					SetCursorSlotByType(CursorSlot.HeroSlot.SlotType + 1);
				}
				break;
			case HeroSlotEnum.equipment_6:
				if (_shopMenu.CurrentScreen == ShopScreenEnum.Shop)
				{
					_shopMenu.PanelShop.SetCursorSlotByY(bigShipSlot.Position.Y - 36f);
				}
				else if (_shopMenu.CurrentScreen == ShopScreenEnum.HeroProfile)
				{
					_shopMenu.PanelShipParams.SelectSlider(3);
				}
				else if (_shopMenu.CurrentScreen == ShopScreenEnum.Mia)
				{
					_shopMenu.PanelMia.SelectDefaultButton();
				}
				break;
			}
		}
	}

	public void Update(float elapsedTime)
	{
		if (Utils.ChangeValueToTarget(ref Position, TargetPosition, 1500f * elapsedTime))
		{
			switch (State)
			{
			case ShopMenuState.Hiding:
				State = ShopMenuState.Hidden;
				break;
			case ShopMenuState.Showing:
				State = ShopMenuState.Visible;
				break;
			}
		}
		_ledFadeFactor += elapsedTime * 3f;
		if (Utils.ChangeValueToTarget(ref _slotConenectonFrameTimer, 0f, elapsedTime))
		{
			_slotConenectonFrameTimer = 0.04f;
			_slotConenectonFrame++;
			if (_slotConenectonFrame > 3)
			{
				_slotConenectonFrame = 0;
			}
		}
		RearGunPadPosition = Position;
		HexPadPosition = RearGunPadPosition + new Vector2(199f, -62f);
		FrontGunPadPosition = HexPadPosition + new Vector2(209f, 3f);
		EquipPadPosition = Position + new Vector2(660f, 70f);
		bool flag = false;
		if (!_shopMenu.IsAnyPopupMenuVisible)
		{
			if (_shopMenu.PanelTopButtons.SelectedButtonIndex < 0 && !_shopMenu.PanelHeroStats.IsAnyItemHighlited && _shopMenu.CurrentScreen != ShopScreenEnum.Mission)
			{
				ParseInput();
			}
			BigShipSlot[] slotArray = SlotArray;
			foreach (BigShipSlot bigShipSlot in slotArray)
			{
				bigShipSlot.Update(elapsedTime);
				if (_shopMenu.PanelTopButtons.SelectedButtonIndex < 0 && _shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse && _shopMenu.Cursor.IsInRect(bigShipSlot.WorldPosition, 82, 65) && State == ShopMenuState.Visible)
				{
					SetCursorSlot(bigShipSlot);
					flag = true;
				}
			}
			if (!flag && _shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
			{
				CancelCursorSelection();
			}
			if (CursorSlot != null && _shopMenu.PanelTopButtons.SelectedButtonIndex < 0)
			{
				if (_shopMenu.KeyPressed(ShopMenuKeys.ActionWithControl) && _shopMenu.CurrentScreen != ShopScreenEnum.Mia)
				{
					BigShipSlot cursorSlot = CursorSlot;
					_shopMenu.SellItem(CursorSlot.HeroSlot.Item);
					CursorSlot = cursorSlot;
				}
				else if (_shopMenu.KeyPressed(ShopMenuKeys.ActionWithShift))
				{
					DoubleClickToSlot(CursorSlot);
				}
				else if (_shopMenu.KeyPressed(ShopMenuKeys.Select))
				{
					if (GameInput.DoubleClick && (_shopMenu.SelectedItem == null || _shopMenu.SelectedItem == CursorSlot.HeroSlot.Item))
					{
						DoubleClickToSlot(CursorSlot);
					}
					else
					{
						ClickToSlot(CursorSlot);
					}
				}
				else if (_shopMenu.KeyPressed(ShopMenuKeys.Action))
				{
					if (GameInput.DoubleClick && (_shopMenu.SelectedItem == null || _shopMenu.SelectedItem == CursorSlot.HeroSlot.Item))
					{
						DoubleClickToSlot(CursorSlot);
					}
					else
					{
						RightClickToSlot(CursorSlot);
					}
				}
			}
			_buttonPlayerSwitch.IsInputEnabled = true;
		}
		else
		{
			_buttonPlayerSwitch.IsInputEnabled = false;
			BigShipSlot[] slotArray = SlotArray;
			for (int i = 0; i < slotArray.Length; i++)
			{
				slotArray[i].Update(elapsedTime);
			}
		}
		_buttonPlayerSwitch.Position = RearGunPadPosition + new Vector2(105f, 190f);
		_buttonPlayerSwitch.Update(elapsedTime);
		if (_buttonPlayerSwitch.IsSelected)
		{
			_shopMenu.CursorType = CursorTypeEnum.Select;
			_shopMenu.SetHintText("hint_switch_players");
		}
	}

	public Vector2 GetCursorSlotPosition()
	{
		if (CursorSlot != null)
		{
			return CursorSlot.Position;
		}
		return Vector2.Zero;
	}

	public Vector2 GetSlotCenterPosition(HeroSlotEnum heroSlot)
	{
		return SlotArray[(int)heroSlot].WorldPosition + new Vector2(37f, 27f);
	}

	public void DrawTitle(BigShipSlot slot)
	{
		float num = slot.Position.X + 41f - 4f;
		float num2 = slot.Position.Y - 25f;
		int num3 = 0;
		switch (slot.HeroSlot.SlotType)
		{
		default:
			return;
		case HeroSlotEnum.top_gun:
			num -= 40f;
			num3 = 0;
			break;
		case HeroSlotEnum.rear_gun:
			num += 40f;
			num3 = 1;
			break;
		case HeroSlotEnum.missile:
			num3 = 2;
			break;
		case HeroSlotEnum.bomb:
			num3 = 3;
			num2 += 103f;
			break;
		case HeroSlotEnum.primary_device:
			num -= 50f;
			num2 += 55f;
			num3 = 4;
			break;
		case HeroSlotEnum.equipment_3:
			num += 40f;
			num3 = 5;
			break;
		case HeroSlotEnum.bottom_gun:
		case HeroSlotEnum.secondary_device:
		case HeroSlotEnum.equipment_1:
		case HeroSlotEnum.equipment_2:
			return;
		}
		_shopMenu.AtlasSlotTypeIcon.DrawFrame(_shopMenu.SpriteBatchLight, num3, Color, Transformation.CreateByPosition(num, num2));
		if (slot.HeroSlot.SlotType == HeroSlotEnum.primary_device)
		{
			_shopMenu.AtlasSlotTypeIcon.DrawFrame(_shopMenu.SpriteBatchLight, num3, Color, Transformation.CreateByPosition(num + 196f, num2));
		}
	}

	private void DrawSlotConnections()
	{
		Color color = new Color(1f, 1f, 1f, _shopMenu.ShowHidePercentage * (3f + Utils.Sin(_ledFadeFactor)) / 4f);
		bool flag = false;
		BigShipSlot bigShipSlot;
		if (MyHero.IsMainGemEnabled)
		{
			bigShipSlot = SlotArray[15];
			if (bigShipSlot.HeroSlot.Item != null)
			{
				BigShipSlot obj = SlotArray[0];
				flag = true;
				int frameNumber = ((obj.HeroSlot.Item == null) ? _atlasSlotConnection.AnimFrames["RED_SE"][_slotConenectonFrame] : _atlasSlotConnection.AnimFrames["SE"][_slotConenectonFrame]);
				Transformation transform = Transformation.CreateByPosition(bigShipSlot.WorldPosition + new Vector2(55f, 61f));
				_atlasSlotConnection.DrawFrame(_shopMenu.SpriteBatchLight, frameNumber, color, transform, BlendModeEnum.Normal);
			}
			bigShipSlot = SlotArray[16];
			if (bigShipSlot.HeroSlot.Item != null)
			{
				BigShipSlot obj2 = SlotArray[0];
				flag = true;
				int frameNumber = ((obj2.HeroSlot.Item == null) ? _atlasSlotConnection.AnimFrames["RED_NE"][_slotConenectonFrame] : _atlasSlotConnection.AnimFrames["NE"][_slotConenectonFrame]);
				Transformation transform2 = Transformation.CreateByPosition(bigShipSlot.WorldPosition + new Vector2(55f, -6f));
				_atlasSlotConnection.DrawFrame(_shopMenu.SpriteBatchLight, frameNumber, color, transform2, BlendModeEnum.Normal);
			}
			bigShipSlot = SlotArray[14];
			if ((bigShipSlot.HeroSlot.Item != null) | flag)
			{
				int frameNumber = ((SlotArray[0].HeroSlot.Item == null) ? _atlasSlotConnection.AnimFrames["RED_E"][_slotConenectonFrame] : _atlasSlotConnection.AnimFrames["E"][_slotConenectonFrame]);
				Transformation transform3 = Transformation.CreateByPosition(bigShipSlot.WorldPosition + new Vector2(74f, 27f));
				_atlasSlotConnection.DrawFrame(_shopMenu.SpriteBatchLight, frameNumber, color, transform3, BlendModeEnum.Normal);
			}
		}
		else
		{
			bigShipSlot = SlotArray[15];
			if (bigShipSlot.HeroSlot.Item != null)
			{
				int frameNumber = ((SlotArray[1].HeroSlot.Item == null) ? _atlasSlotConnection.AnimFrames["RED_E"][_slotConenectonFrame] : _atlasSlotConnection.AnimFrames["E"][_slotConenectonFrame]);
				Transformation transform4 = Transformation.CreateByPosition(bigShipSlot.WorldPosition + new Vector2(74f, 27f));
				_atlasSlotConnection.DrawFrame(_shopMenu.SpriteBatchLight, frameNumber, color, transform4, BlendModeEnum.Normal);
			}
			bigShipSlot = SlotArray[14];
			if (bigShipSlot.HeroSlot.Item != null)
			{
				int frameNumber = ((SlotArray[0].HeroSlot.Item == null) ? _atlasSlotConnection.AnimFrames["RED_E"][_slotConenectonFrame] : _atlasSlotConnection.AnimFrames["E"][_slotConenectonFrame]);
				Transformation transform5 = Transformation.CreateByPosition(bigShipSlot.WorldPosition + new Vector2(74f, 27f));
				_atlasSlotConnection.DrawFrame(_shopMenu.SpriteBatchLight, frameNumber, color, transform5, BlendModeEnum.Normal);
			}
			bigShipSlot = SlotArray[16];
			if (bigShipSlot.HeroSlot.Item != null)
			{
				int frameNumber = ((SlotArray[2].HeroSlot.Item == null) ? _atlasSlotConnection.AnimFrames["RED_E"][_slotConenectonFrame] : _atlasSlotConnection.AnimFrames["E"][_slotConenectonFrame]);
				Transformation transform6 = Transformation.CreateByPosition(bigShipSlot.WorldPosition + new Vector2(74f, 27f));
				_atlasSlotConnection.DrawFrame(_shopMenu.SpriteBatchLight, frameNumber, color, transform6, BlendModeEnum.Normal);
			}
		}
		bigShipSlot = SlotArray[17];
		if (bigShipSlot.HeroSlot.Item != null)
		{
			int frameNumber = ((SlotArray[3].HeroSlot.Item == null) ? _atlasSlotConnection.AnimFrames["RED_W"][_slotConenectonFrame] : _atlasSlotConnection.AnimFrames["W"][_slotConenectonFrame]);
			Transformation transform7 = Transformation.CreateByPosition(bigShipSlot.WorldPosition + new Vector2(-1f, 27f));
			_atlasSlotConnection.DrawFrame(_shopMenu.SpriteBatchLight, frameNumber, color, transform7, BlendModeEnum.Normal);
		}
		if (MyHero.IsConnectedDevicesEnabled)
		{
			BigShipSlot obj3 = SlotArray[5];
			BigShipSlot bigShipSlot2 = SlotArray[6];
			int frameNumber = ((obj3.HeroSlot.Item == null || bigShipSlot2.HeroSlot.Item == null) ? _atlasSlotConnection.AnimFrames["RED_E"][_slotConenectonFrame] : _atlasSlotConnection.AnimFrames["E"][_slotConenectonFrame]);
			Transformation transformation = Transformation.CreateByPosition(obj3.WorldPosition + new Vector2(84f, 18f));
			_atlasSlotConnection.DrawFrame(_shopMenu.SpriteBatchLight, frameNumber, color, transformation, BlendModeEnum.Normal);
			frameNumber = ((obj3.HeroSlot.Item == null || bigShipSlot2.HeroSlot.Item == null) ? _atlasSlotConnection.AnimFrames["RED_W"][_slotConenectonFrame] : _atlasSlotConnection.AnimFrames["W"][_slotConenectonFrame]);
			transformation.Position.Y += 20f;
			_atlasSlotConnection.DrawFrame(_shopMenu.SpriteBatchLight, frameNumber, color, transformation, BlendModeEnum.Normal);
		}
	}

	public void Draw()
	{
		_shopMenu.SpriteBatchBegin(bLight: true);
		Transformation transformation = Transformation.CreateByPosition(FrontGunPadPosition);
		_atlasBarbiePad.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color, transformation);
		transformation.Position = RearGunPadPosition;
		_atlasBarbiePad.DrawFrame(_shopMenu.SpriteBatchLight, 1, Color, transformation);
		transformation.Position = HexPadPosition;
		_atlasBarbiePad.DrawFrame(_shopMenu.SpriteBatchLight, 2, Color, transformation);
		transformation.Position = EquipPadPosition;
		_atlasBarbiePad.DrawFrame(_shopMenu.SpriteBatchLight, 3, Color, transformation);
		Color color = new Color(1f, 1f, 1f, _shopMenu.ShowHidePercentage * (3f + Utils.Sin(_ledFadeFactor)) / 4f);
		BigShipSlot[] slotArray = SlotArray;
		foreach (BigShipSlot bigShipSlot in slotArray)
		{
			bigShipSlot.Draw();
			if (bigShipSlot.IsCompatible(_shopMenu.SelectedItem))
			{
				Transformation transformation2 = new Transformation(bigShipSlot.WorldTransformation);
				transformation2.Position += new Vector2(37f, 27f);
				_shopMenu.AtlasGemSlot.DrawFrame(_shopMenu.SpriteBatchLight, 7, color, transformation2, BlendModeEnum.Additive);
			}
			SpriteAtlas spriteAtlas = (bigShipSlot.HeroSlot.IsGemType ? _shopMenu.AtlasGemSlot : _shopMenu.AtlasSlot);
			if (_shopMenu.SelectedItem != null && bigShipSlot.HeroSlot.Item == _shopMenu.SelectedItem)
			{
				Color slotSelectedColor = _shopMenu.GetSlotSelectedColor();
				slotSelectedColor.A = (byte)((float)(slotSelectedColor.A * Color.A) / 255f);
				spriteAtlas.DrawFrame(_shopMenu.SpriteBatchLight, 2, slotSelectedColor, bigShipSlot.WorldTransformation, BlendModeEnum.Additive);
			}
			if (bigShipSlot == CursorSlot)
			{
				Color slotSelectedColor = _shopMenu.GetSlotCursorColor();
				slotSelectedColor.A = (byte)((float)(slotSelectedColor.A * Color.A) / 255f);
				spriteAtlas.DrawFrame(_shopMenu.SpriteBatchLight, 1, slotSelectedColor, bigShipSlot.WorldTransformation, BlendModeEnum.Additive);
			}
		}
		HeroItem tVItem = _shopMenu.PanelInfo.TVItem;
		if (tVItem != null && tVItem.Loadout != null)
		{
			List<HeroItem> list = new List<HeroItem>(GameJnG.Cargo.Items.Values);
			slotArray = SlotArray;
			foreach (BigShipSlot bigShipSlot2 in slotArray)
			{
				if (bigShipSlot2.HeroSlot.Item != null)
				{
					list.Add(bigShipSlot2.HeroSlot.Item);
				}
			}
			slotArray = SlotArray;
			foreach (BigShipSlot bigShipSlot3 in slotArray)
			{
				LoadoutEntry entry = tVItem.Loadout.GetEntryForSlot(bigShipSlot3.HeroSlot);
				if (entry != null)
				{
					HeroItem heroItem = list.FirstOrDefault((HeroItem o) => Utils.Equals(o.Data.Id, entry.ItemData.Id) && o.Level == entry.ItemLevel);
					if (heroItem != null)
					{
						list.Remove(heroItem);
					}
					else
					{
						_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("shop_loadout_missing"), bigShipSlot3.WorldTransformation.Position.X + 37f, bigShipSlot3.WorldTransformation.Position.Y + 21f, new Vector2(0.45f), new Color(255, 0, 0, (int)Color.A), TextAlign.Center);
					}
				}
			}
		}
		DrawSlotConnections();
		slotArray = SlotArray;
		foreach (BigShipSlot slot in slotArray)
		{
			DrawTitle(slot);
		}
		_buttonPlayerSwitch.Color = Color;
		_buttonPlayerSwitch.Draw();
		_shopMenu.SpriteBatchEnd(bLight: true);
	}
}
