using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class PanelCargo : PanelWithSlider
{
	private const int MAX_VISIBLE_ROWS = 13;

	private const int SLOT_Y_DISTANCE = 66;

	public int OldSlotIndexOnRight;

	public bool IsSortButtonSelected;

	private SpriteRenderer _sortButtonSpriteRenderer;

	private SpriteAtlas _atlasSortButtonHighlihgt;

	private Vector2 _sortButtonPosition;

	private float _sortButtonAlpha;

	private float _scrollOffsetY;

	private float _targetScrollOffsetY;

	private float _wheelRotation;

	private float _cashTimer;

	public int CursorSlotIndex { get; protected set; } = -1;

	public bool IsSelectAvailable => true;

	public PanelCargo(ShopMenu shopMenu)
		: base(shopMenu)
	{
		Init();
	}

	public override void Init()
	{
		base.Init();
		Position = new Vector2(-400f, 0f);
		_sliderObject.MyObject.Position.X = 293f;
		_atlasFrame = GameCore.GetSpriteAtlas("shop_panel_cargo", logWhenNotFound: true);
		_atlasBackground = GameCore.GetSpriteAtlas("shop_panel_cargo_bgr", logWhenNotFound: true);
		_atlasSortButtonHighlihgt = GameCore.GetSpriteAtlas("shop_sort_button_highlight", logWhenNotFound: true);
		_sortButtonSpriteRenderer = GameCore.Allocator.GetSpriteRenderer(GameCore.GetSpriteAtlas("shop_sort_button", logWhenNotFound: true));
		_sortButtonSpriteRenderer.ChangeAnim("idle", 0.5f);
		_sortButtonSpriteRenderer.Animator.IsLooped = false;
	}

	public void SetCashEffect()
	{
		_cashTimer = 1f;
	}

	public void ResetToDefault()
	{
		_scrollOffsetY = 0f;
		_targetScrollOffsetY = 0f;
		_wheelRotation = 0f;
		OldSlotIndexOnRight = 0;
	}

	public override void Show(float showDelay)
	{
		base.Show(showDelay);
		TargetPosition = new Vector2(0f, 0f);
	}

	public override void Hide()
	{
		base.Hide();
		TargetPosition = new Vector2(-400f, 0f);
	}

	public HeroItem GetSlotItem(int slotIndex)
	{
		return GameJnG.Cargo.GetItem(slotIndex);
	}

	public Vector2 GetSlotPosition(int index)
	{
		return Position + new Vector2(34 + index % 3 * 83, (float)(131 + index / 3 * 66) + _scrollOffsetY);
	}

	public int GetSlotIndex(int indexX, int indexY)
	{
		return indexY * 3 + indexX;
	}

	public int GetSlotIndex(HeroItem item)
	{
		if (item == null)
		{
			return -1;
		}
		foreach (KeyValuePair<int, HeroItem> item2 in GameJnG.Cargo.Items)
		{
			if (item2.Value == item)
			{
				return item2.Key;
			}
		}
		return -1;
	}

	public int GetSlotRow(int index)
	{
		return index / 3;
	}

	public IntRange GetVisibleSlotRange()
	{
		IntRange result = default;
		result.From = (int)((0f - _scrollOffsetY) / 66f) * 3;
		result.To = result.From + 39 + 2;
		return result;
	}

	public void DoubleClickToSlot(int slotIndex)
	{
		HeroItem slotItem = GetSlotItem(CursorSlotIndex);
		if (slotItem != null)
		{
			SelectItem(slotItem);
			if (_shopMenu.BigShipSlotSet.TryAddItemToFreeSlot(slotItem))
			{
				GameJnG.Cargo.RemoveItem(slotItem);
				_shopMenu.CancelSelection();
			}
			else if (slotItem.Data.ShopAction != ItemShopActionEnum.None)
			{
				_shopMenu.PanelInfo.DoItemAction(slotItem);
			}
			else
			{
				_shopMenu.AddCursorMessage(TextBundle.GetText("cursor_text_no_free_slot"), GetSlotPosition(slotIndex) + ShopMenu.SLOT_CENTER_OFFSET, Color.Red);
				GameCore.PlaySound("menu_denied");
			}
		}
	}

	public void ClickToSlot(int cargoSlotIndex)
	{
		HeroItem slotItem = GetSlotItem(cargoSlotIndex);
		if (slotItem != null && slotItem != _shopMenu.SelectedItem)
		{
			SelectItem(slotItem);
		}
		else
		{
			RightClickToSlot(cargoSlotIndex);
		}
	}

	public void RightClickToSlot(int cargoSlotIndex)
	{
		HeroItem slotItem = GetSlotItem(cargoSlotIndex);
		bool flag = false;
		if (_shopMenu.SelectedItem != null)
		{
			if (_shopMenu.SelectedItem.Place == ItemPlaceEnum.InShop)
			{
				if (slotItem != null)
				{
					_shopMenu.AddCursorMessage(TextBundle.GetText("cursor_text_full_slot"), GetSlotPosition(cargoSlotIndex) + ShopMenu.SLOT_CENTER_OFFSET, Color.Red);
					GameCore.PlaySound("menu_denied");
				}
				else if (_shopMenu.SelectedItem.IsTooExpensive)
				{
					_shopMenu.AddCursorMessage(TextBundle.GetText("cursor_text_too_expensive"), GetSlotPosition(cargoSlotIndex) + ShopMenu.SLOT_CENTER_OFFSET, Color.Red);
					GameCore.PlaySound("menu_denied");
				}
				else
				{
					BuyItem(cargoSlotIndex, _shopMenu.SelectedItem);
					flag = true;
				}
			}
			else if (slotItem == _shopMenu.SelectedItem)
			{
				_shopMenu.CancelSelection();
				GameCore.PlaySound("menu_item_tv_hide");
			}
			else if (_shopMenu.SelectedItem.Place == ItemPlaceEnum.InHeroSlot)
			{
				if (slotItem == null || _shopMenu.SelectedItem.MySlot.IsCompatible(slotItem))
				{
					_shopMenu.ItemFlySet.StartToCargo(_shopMenu.SelectedItem, cargoSlotIndex);
					if (slotItem != null)
					{
						_shopMenu.ItemFlySet.StartToHeroSlot(slotItem, _shopMenu.SelectedItem.MySlot.SlotType);
					}
					GameJnG.Cargo.SwapItemWithSlot(cargoSlotIndex, _shopMenu.MyHero.MyShip, _shopMenu.SelectedItem.MySlot.SlotType);
					flag = true;
				}
				else
				{
					_shopMenu.AddCursorMessage(TextBundle.GetText("cursor_text_incompatible_swap"), GetSlotPosition(cargoSlotIndex) + ShopMenu.SLOT_CENTER_OFFSET, Color.Red);
				}
			}
			else if (slotItem == null || _shopMenu.SelectedItem.Place != ItemPlaceEnum.InHeroSlot)
			{
				_shopMenu.ItemFlySet.StartToCargo(_shopMenu.SelectedItem, cargoSlotIndex);
				if (slotItem != null)
				{
					_shopMenu.ItemFlySet.StartToCargo(slotItem, _shopMenu.SelectedItem.CargoIndex);
				}
				GameJnG.Cargo.SwapItems(cargoSlotIndex, _shopMenu.SelectedItem.CargoIndex);
				flag = true;
			}
		}
		else
		{
			SelectItem(slotItem);
		}
		if (flag)
		{
			_shopMenu.CancelSelection();
			_shopMenu.ChangeFocus(this);
		}
	}

	public void PlaceItem(int slotIndex, HeroItem item)
	{
		switch (item.Place)
		{
		case ItemPlaceEnum.InShop:
			GameJnG.ShopOffer.RemoveItem(item);
			break;
		case ItemPlaceEnum.InHeroSlot:
			item?.MySlot.SetItem(null);
			break;
		}
		GameJnG.Cargo.SetItem(slotIndex, item);
	}

	public void BuyItem(int slotIndex, HeroItem shopItem, bool tryInstalToBigShip = false)
	{
		_shopMenu.CancelSelection();
		if (shopItem.Data.ItemType == ItemTypeEnum.Loadout && shopItem.Loadout == null)
		{
			shopItem.CreateFreeLoadout();
		}
		if (tryInstalToBigShip && shopItem != null && _shopMenu.BigShipSlotSet.TryAddItemToFreeSlot(shopItem))
		{
			_shopMenu.BuyItem(shopItem);
			GameJnG.ShopOffer.RemoveItem(shopItem);
			return;
		}
		_shopMenu.ItemFlySet.StartToCargo(shopItem, slotIndex);
		_shopMenu.BuyItem(shopItem);
		PlaceItem(slotIndex, shopItem);
		ScrollToSlot(slotIndex);
	}

	public void SelectItem(HeroItem item)
	{
		_shopMenu.CancelSelection();
		_shopMenu.SetSelectedItem(item);
		_shopMenu.ChangeFocus(this);
		_shopMenu.SetCursorItem(item);
		ScrollToSlot(GetSlotIndex(item));
	}

	public override void SetFocus(bool focus)
	{
		base.SetFocus(focus);
	}

	public void CancelSelection()
	{
	}

	private void ScrollToSlotTop(int index)
	{
		_targetScrollOffsetY = -GetSlotRow(index) * 66;
	}

	public void ScrollToSlot(int index)
	{
		IntRange visibleSlotRange = GetVisibleSlotRange();
		if (index > visibleSlotRange.To - 6)
		{
			_targetScrollOffsetY = -(GetSlotRow(index) - 13 + 1) * 66 - 22;
		}
		else if (index < visibleSlotRange.From)
		{
			ScrollToSlotTop(index);
		}
	}

	public void SetCursorSlot(int index)
	{
		index = Utils.Clamp(index, 0, Math.Max(GameJnG.Cargo.GetLastIndex() + 5 - GameJnG.Cargo.GetLastIndex() % 3, GetVisibleSlotRange().To - 3));
		if (CursorSlotIndex != index)
		{
			CursorSlotIndex = index;
			GameCore.PlaySound("menu_item_select");
		}
		_shopMenu.SetCursorItem(GetSlotItem(CursorSlotIndex));
		_shopMenu.IsCursorOnSlot = true;
		_shopMenu.ChangeFocus(this);
	}

	public void SetCursorSlotByY(float slotY, int column)
	{
		IntRange visibleSlotRange = GetVisibleSlotRange();
		CursorSlotIndex = visibleSlotRange.To;
		UnselectSortButton();
		for (int i = (visibleSlotRange.From + column) % 3; i <= visibleSlotRange.To; i += 3)
		{
			Vector2 slotPosition = GetSlotPosition(i);
			if (slotPosition.Y > 950f)
			{
				CursorSlotIndex = i - 3;
				break;
			}
			if (slotPosition.Y >= slotY)
			{
				CursorSlotIndex = i;
				break;
			}
		}
		GameCore.PlaySound("menu_item_select");
		_shopMenu.SetCursorItem(GetSlotItem(CursorSlotIndex));
		_shopMenu.ChangeFocus(this);
	}

	public void SetVisibleCursorSlot(int row, int column)
	{
		CursorSlotIndex = GetVisibleSlotRange().From + row * 3 + column;
		_shopMenu.SetCursorItem(GetSlotItem(CursorSlotIndex));
		_shopMenu.ChangeFocus(this);
		GameCore.PlaySound("menu_item_select");
	}

	public override void CancelCursorSelection()
	{
		base.CancelCursorSelection();
		CursorSlotIndex = -1;
	}

	private void ParseInput()
	{
		if (_shopMenu.IsAnyPopupMenuVisible)
		{
			return;
		}
		if (GameInput.ControlDown && GameInput.SystemKeyPressed(Keys.S))
		{
			Sort();
		}
		if (_shopMenu.Cursor.Position.X >= Position.X && _shopMenu.Cursor.Position.X <= Position.X + 360f)
		{
			if (GameInput.GetMouseWheelDelta() > 0)
			{
				_targetScrollOffsetY = (int)(_targetScrollOffsetY / 66f) * 66 + 66;
			}
			else if (GameInput.GetMouseWheelDelta() < 0)
			{
				_targetScrollOffsetY = (int)(_targetScrollOffsetY / 66f) * 66 - 66;
			}
		}
		if (!HasFocus || _isSliderDragged)
		{
			return;
		}
		if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRightWithControl))
		{
			UnselectSortButton();
			_shopMenu.CancelCursorSelection();
			_shopMenu.BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.front_gun);
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeftWithControl))
		{
			UnselectSortButton();
			_shopMenu.PanelShop.SetCursorSlotByY(GetSlotPosition(CursorSlotIndex).Y - 36f);
			CancelCursorSelection();
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp))
		{
			if (IsSortButtonSelected)
			{
				_shopMenu.SelectTopButton(0);
			}
			else if (CursorSlotIndex >= 3)
			{
				SetCursorSlot(CursorSlotIndex - 3);
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveDown))
		{
			if (IsSortButtonSelected)
			{
				_shopMenu.BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.rear_gun);
			}
			else if (GetSlotRow(CursorSlotIndex) <= GetSlotRow(GameJnG.Cargo.GetLastIndex()) || GetSlotRow(CursorSlotIndex) < 12)
			{
				SetCursorSlot(CursorSlotIndex + 3);
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.PageUp) || _shopMenu.KeyPressed(ShopMenuKeys.MoveUpWithControl))
		{
			if (CursorSlotIndex >= 0)
			{
				IntRange visibleSlotRange = GetVisibleSlotRange();
				SetCursorSlot(CursorSlotIndex - 39 + 3);
				ScrollToSlotTop(visibleSlotRange.From - 39 + 3);
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.PageDown) || _shopMenu.KeyPressed(ShopMenuKeys.MoveDownWithControl))
		{
			IntRange visibleSlotRange2 = GetVisibleSlotRange();
			SetCursorSlot(CursorSlotIndex + 39 - 3);
			ScrollToSlotTop(visibleSlotRange2.From + 39 - 3);
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.Home))
		{
			SetCursorSlot(0);
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.End))
		{
			SetCursorSlot(GameJnG.Cargo.GetLastIndex());
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRight))
		{
			if (IsSortButtonSelected)
			{
				_shopMenu.SelectTopButton(0);
				return;
			}
			if (CursorSlotIndex % 3 < 2)
			{
				SetCursorSlot(CursorSlotIndex + 1);
				return;
			}
			float y = GetSlotPosition(CursorSlotIndex).Y;
			_shopMenu.CancelCursorSelection();
			if (y < 300f)
			{
				SelectSortButton();
			}
			else
			{
				_shopMenu.BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.rear_gun);
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeft))
		{
			if (IsSortButtonSelected)
			{
				SetCursorSlotByY(_sortButtonPosition.Y - 36f, 2);
				return;
			}
			if (CursorSlotIndex % 3 > 0)
			{
				SetCursorSlot(CursorSlotIndex - 1);
				return;
			}
			GetVisibleSlotRange();
			_shopMenu.PanelShop.SetCursorSlotByY(GetSlotPosition(CursorSlotIndex).Y - 36f);
			CancelCursorSelection();
		}
	}

	public float GetMaxScrollOffsetY()
	{
		int num = GameJnG.Cargo.GetLastIndex() / 3 - 13 + 2;
		if (num < 1)
		{
			num = 1;
		}
		return 66 * num + 87;
	}

	public Vector2 GetCursorSlotPosition()
	{
		return GetSlotPosition(CursorSlotIndex);
	}

	public void UpdateSlider(float elapsedTime)
	{
		GetVisibleSlotRange();
		float num = 742f;
		float num2 = 168f;
		float maxScrollOffsetY = GetMaxScrollOffsetY();
		if (!_shopMenu.IsAnyPopupMenuVisible && _shopMenu.Cursor.IsInRect(_sliderObject.MyObject.WorldPosition.X - 11f, _sliderObject.MyObject.WorldPosition.Y - 50f, 22, 100))
		{
			_isSliderUnderCursor = true;
			_isSliderBarUnderCursor = true;
			if (GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				_isSliderDragged = true;
				_currsorSliderOffset = _shopMenu.Cursor.Position.Y - _sliderObject.MyObject.WorldPosition.Y;
			}
		}
		else
		{
			_isSliderUnderCursor = false;
			if (_shopMenu.Cursor.IsInRect(WorldPosition.X + 287f, WorldPosition.Y + 128f, 20, 836))
			{
				_isSliderBarUnderCursor = true;
			}
			else
			{
				_isSliderBarUnderCursor = false;
			}
		}
		if (!_shopMenu.IsAnyPopupMenuVisible && GameInput.MouseDown(GameInput.MouseButton.Left))
		{
			if (_isSliderBarUnderCursor && !_isSliderDragged)
			{
				_isSliderDragged = true;
				_currsorSliderOffset = 0f;
			}
			if (_isSliderDragged)
			{
				_targetScrollOffsetY = (0f - (_shopMenu.Cursor.Position.Y - num2 - _currsorSliderOffset) / num) * maxScrollOffsetY;
			}
		}
		else
		{
			_isSliderDragged = false;
		}
		UpdateSliderLight(elapsedTime, _scrollOffsetY != _targetScrollOffsetY);
		if (_scrollOffsetY < 0f - maxScrollOffsetY)
		{
			_sliderObject.MyObject.Position.Y = num2 + num;
		}
		else
		{
			_sliderObject.MyObject.Position.Y = num2 - _scrollOffsetY / maxScrollOffsetY * num;
		}
		_sliderObject.MyObject.Update(elapsedTime);
		if (_isSliderDragged)
		{
			_shopMenu.CursorType = CursorTypeEnum.Selected;
		}
		else if (_isSliderUnderCursor || _isSliderBarUnderCursor)
		{
			_shopMenu.CursorType = CursorTypeEnum.Select;
			_shopMenu.SetHintText("hint_scroll_cargo");
		}
	}

	public void UpdateScrolling(float elapsedTime)
	{
		float maxScrollOffsetY = GetMaxScrollOffsetY();
		if (_targetScrollOffsetY > 0f)
		{
			_targetScrollOffsetY = 0f;
		}
		else if (_targetScrollOffsetY < 0f - maxScrollOffsetY)
		{
			_targetScrollOffsetY = 0f - maxScrollOffsetY;
		}
		float num = (_scrollOffsetY - _targetScrollOffsetY) * 8f;
		if (!(num > 0f))
		{
			_ = 0f;
		}
		if (_isSliderDragged)
		{
			num *= 10f;
		}
		_wheelRotation += num / 50f * elapsedTime;
		num = Math.Abs(num);
		if (num < 100f)
		{
			num = 100f;
		}
		Utils.ChangeValueToTarget(ref _scrollOffsetY, _targetScrollOffsetY, num * elapsedTime);
	}

	public void SelectSortButton()
	{
		if (!IsSortButtonSelected)
		{
			CancelCursorSelection();
			IsSortButtonSelected = true;
			GameCore.PlaySound("menu_item_select");
			_shopMenu.ChangeFocus(this);
		}
	}

	public void UnselectSortButton()
	{
		IsSortButtonSelected = false;
	}

	private void UpdateSortButton(float elapsedTime)
	{
		_sortButtonPosition = Position + new Vector2(342f, 158f);
		if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
		{
			if (_shopMenu.Cursor.IsInRect(_sortButtonPosition.X - 32f, _sortButtonPosition.Y - 32f, 64, 64))
			{
				SelectSortButton();
			}
			else
			{
				IsSortButtonSelected = false;
			}
		}
		if (IsSortButtonSelected)
		{
			Utils.ChangeValueToTarget(ref _sortButtonAlpha, 255f, 1200f * elapsedTime);
			_shopMenu.SetHintText("hint_sort_cargo");
			_shopMenu.CursorType = CursorTypeEnum.Select;
			if (_shopMenu.ShopInput.KeyPressed(ShopMenuKeys.Select))
			{
				Sort();
			}
		}
		else
		{
			Utils.ChangeValueToTarget(ref _sortButtonAlpha, 0f, 1200f * elapsedTime);
		}
		_sortButtonSpriteRenderer.Update(elapsedTime);
	}

	private void Sort()
	{
		GameJnG.Cargo.Sort();
		_sortButtonSpriteRenderer.ChangeAnim("shop_sort_button_press", 0.25f);
		ScrollToSlot(0);
	}

	public override void Update(float elapsedTime)
	{
		if (State == ShopMenuState.Hidden)
		{
			return;
		}
		base.Update(elapsedTime);
		if (!IsSelectAvailable)
		{
			return;
		}
		UpdateSortButton(elapsedTime);
		ParseInput();
		Utils.ChangeValueToTarget(ref _cashTimer, 0f, 5f * elapsedTime);
		UpdateScrolling(elapsedTime);
		UpdateSlider(elapsedTime);
		bool flag = false;
		if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse && !_isSliderDragged && !_isSliderUnderCursor && !_shopMenu.IsAnyPopupMenuVisible && !_isSliderBarUnderCursor && _shopMenu.Cursor.IsInRect(Position.X + 30f, Position.Y + 115f, 280, 852))
		{
			IntRange visibleSlotRange = GetVisibleSlotRange();
			for (int i = visibleSlotRange.From; i <= visibleSlotRange.To; i++)
			{
				Vector2 slotPosition = GetSlotPosition(i);
				if (_shopMenu.Cursor.IsInRect(slotPosition.X - 3f, slotPosition.Y - 1f, 85, 66))
				{
					SetCursorSlot(i);
					flag = true;
					break;
				}
			}
		}
		if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse && !flag && _shopMenu.CursorItem == null)
		{
			CursorSlotIndex = -1;
		}
		if (_isSliderDragged || !HasFocus || CursorSlotIndex < 0 || _shopMenu.IsAnyPopupMenuVisible)
		{
			return;
		}
		if (_shopMenu.KeyPressed(ShopMenuKeys.ActionWithControl))
		{
			HeroItem slotItem = GetSlotItem(CursorSlotIndex);
			_shopMenu.SellItem(slotItem);
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.ActionWithShift))
		{
			DoubleClickToSlot(CursorSlotIndex);
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.Action))
		{
			RightClickToSlot(CursorSlotIndex);
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.Select))
		{
			if (GameInput.DoubleClick && (_shopMenu.SelectedItem == null || _shopMenu.SelectedItem == GetSlotItem(CursorSlotIndex)))
			{
				DoubleClickToSlot(CursorSlotIndex);
			}
			else
			{
				ClickToSlot(CursorSlotIndex);
			}
		}
	}

	private void DrawWheels()
	{
		Transformation transformation = new Transformation(this);
		transformation.Position.X += 332f;
		transformation.Position.Y += 333f;
		transformation.Rotation = _wheelRotation;
		_shopMenu.AtlasWheel.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color.White, transformation, BlendModeEnum.Normal);
		transformation.Position.X += 3f;
		transformation.Position.Y += 59f;
		transformation.Rotation = (0f - _wheelRotation) * 1.5f;
		_shopMenu.AtlasWheel.DrawFrame(_shopMenu.SpriteBatchLight, 1, Color.White, transformation, BlendModeEnum.Normal);
	}

	private void DrawSortButton()
	{
		Transformation transform = Transformation.CreateByPosition(_sortButtonPosition);
		_sortButtonSpriteRenderer.Draw(transform, _shopMenu.SpriteBatchLight, BlendModeEnum.Normal);
		if (_sortButtonAlpha > 0f)
		{
			_atlasSortButtonHighlihgt.DrawFrame(_shopMenu.SpriteBatchLight, 0, new Color(255, 255, 255, (int)_sortButtonAlpha), transform, BlendModeEnum.Additive);
		}
	}

	public override void Draw()
	{
		if (State == ShopMenuState.Hidden)
		{
			return;
		}
		_shopMenu.SpriteBatchBegin(bLight: true);
		_spriteRenderer.SetAtlas(_atlasBackground);
		_spriteRenderer.Frame = 0;
		_spriteRenderer.Draw(this, _shopMenu.SpriteBatchLight, BlendModeEnum.Normal);
		IntRange visibleSlotRange = GetVisibleSlotRange();
		visibleSlotRange.From = Math.Max(0, visibleSlotRange.From - 3);
		for (int i = visibleSlotRange.From; i < visibleSlotRange.To + 7; i++)
		{
			Transformation transformation = Transformation.CreateByPosition(GetSlotPosition(i));
			HeroItem slotItem = GetSlotItem(i);
			_shopMenu.DrawSlot(_shopMenu.AtlasSlot, transformation, slotItem, 1f, bAsLoadoutPreview: false);
			if (State == ShopMenuState.Visible)
			{
				if (_shopMenu.SelectedItem != null && _shopMenu.SelectedItem.Place == ItemPlaceEnum.InCargo && i == _shopMenu.SelectedItem.CargoIndex)
				{
					_shopMenu.AtlasSlot.DrawFrame(_shopMenu.SpriteBatchLight, 2, _shopMenu.GetSlotSelectedColor(), transformation, BlendModeEnum.Additive);
				}
				if (i == CursorSlotIndex)
				{
					_shopMenu.AtlasSlot.DrawFrame(_shopMenu.SpriteBatchLight, 1, _shopMenu.GetSlotCursorColor(), transformation, BlendModeEnum.Additive);
				}
			}
		}
		foreach (Hero hero in GameJnG.HeroList)
		{
			if (hero.IsLoadoutSwapperEnabled)
			{
				HeroItem firstLoadoutItem = GameJnG.Cargo.GetFirstLoadoutItem(hero);
				if (firstLoadoutItem != null)
				{
					Transformation transformation2 = Transformation.CreateByPosition(GetSlotPosition(GetSlotIndex(firstLoadoutItem)));
					transformation2.Position.X += 40f;
					transformation2.Position.Y += 25f;
					_shopMenu.AtlasLoadout.DrawFrame(_shopMenu.SpriteBatchLight, 1 + hero.PlayerNumber, new Color(255, 255, 255, (int)Color.A), transformation2, BlendModeEnum.Normal);
				}
			}
		}
		_spriteRenderer.SetAtlas(_atlasFrame);
		for (int j = 0; j <= 4; j++)
		{
			_spriteRenderer.Frame = j;
			_spriteRenderer.Draw(this, _shopMenu.SpriteBatchLight, BlendModeEnum.Normal);
			if (j == 0)
			{
				DrawWheels();
			}
		}
		Transformation transformation3 = new Transformation(this);
		transformation3.Position += new Vector2(158f, 111f);
		_shopMenu.AtlasPanelLabel.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color.White, transformation3);
		GameCore.FontNoBorder.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("shop_panel_label_cargo"), transformation3.Position.X, transformation3.Position.Y - 8.5f * Options.FontScale, ShopMenu.FONT_SCALE_LABEL_SMALL, new Color(45, 30, 20, 220), TextAlign.Center, 130);
		transformation3.Position.Y = 976f;
		_shopMenu.AtlasPanelLabel.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color.White, transformation3);
		GameCore.FontNoBorder.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("shop_panel_label_cash"), transformation3.Position.X, transformation3.Position.Y - 8.5f * Options.FontScale, ShopMenu.FONT_SCALE_LABEL_SMALL, new Color(45, 30, 20, 220), TextAlign.Center, 130);
		DrawSortButton();
		Transformation transform = Transformation.CreateByPosition(Position + new Vector2(288f, 122f));
		_atlasFrame.DrawFrame(_shopMenu.SpriteBatchLight, 6, new Color(255, 255, 255, (int)_currSliderBarLightAlpha), transform, BlendModeEnum.Normal);
		_sliderObject.Draw();
		string text = $"✐ {Utils.FormatMoney(GameJnG.Cash)}";
		if (_cashTimer > 0f)
		{
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, Position.X + 159f, Position.Y + 995f - 4f * _cashTimer, ShopMenu.FONT_SCALE_CASH + new Vector2(0.1f * _cashTimer, 0.3f * _cashTimer), new Color(255, 255, (int)(255f * _cashTimer)), TextAlign.Center);
		}
		else
		{
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, Position.X + 159f, Position.Y + 995f, ShopMenu.FONT_SCALE_CASH, new Color(255, 255, 0), TextAlign.Center);
		}
		_shopMenu.SpriteBatchEnd(bLight: true);
	}
}
