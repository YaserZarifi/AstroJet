using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class PanelShop : PanelWithSlider
{
	private const int MAX_VISIBLE_ROWS = 14;

	private const int BUTTON_WIDTH = 70;

	private const int BUTTON_HEIGHT = 68;

	private const int SLOT_Y_DISTANCE = 66;

	private const int DIVIDER_Y_DISTANCE = 33;

	public int CursorSlotIndex = -1;

	private int _selectedSlotIndex = -1;

	private float _scrollOffsetY;

	private float _targetScrollOffsetY;

	private bool _isSliderVisible;

	private readonly Vector2 _butonOffset = new Vector2(-334f, 45f);

	private List<ShopSlot> _slotList;

	private List<ShopDivider> _dividerList;

	public PanelShop(ShopMenu shopMenu)
		: base(shopMenu)
	{
		Init();
	}

	public override void Init()
	{
		base.Init();
		Position = new Vector2(2320f, 0f);
		_sliderObject.MyObject.Position.X = -25f;
		_atlasFrame = GameCore.GetSpriteAtlas("shop_panel", logWhenNotFound: true);
		_atlasBackground = GameCore.GetSpriteAtlas("shop_panel_bgr", logWhenNotFound: true);
	}

	public override void Show(float showDelay)
	{
		base.Show(showDelay);
		TargetPosition = new Vector2(1920f, 0f);
		PrepareSlots();
	}

	public override void Hide()
	{
		base.Hide();
		TargetPosition = new Vector2(2320f, 0f);
	}

	private void SelectSlotItem(int slotIndex)
	{
		HeroItem slotItem = GetSlotItem(slotIndex);
		_shopMenu.CancelSelection();
		_selectedSlotIndex = slotIndex;
		_shopMenu.SetSelectedItem(slotItem);
		_shopMenu.ChangeFocus(this);
		_shopMenu.SetMode(SelectionModeEnum.Selected);
		SetCursorSlot(slotIndex);
	}

	public void ClickToSlot(int slotIndex)
	{
		if (slotIndex == _selectedSlotIndex)
		{
			_shopMenu.CancelSelection();
			GameCore.PlaySound("menu_item_tv_hide");
			return;
		}
		HeroItem slotItem = GetSlotItem(slotIndex);
		if (slotItem != null && slotItem != _shopMenu.SelectedItem)
		{
			SelectSlotItem(slotIndex);
		}
	}

	public void DoubleClickToSlot(int slotIndex)
	{
		HeroItem slotItem = GetSlotItem(slotIndex);
		if (slotItem == null)
		{
			return;
		}
		if (_shopMenu.BigShipSlotSet.IsAnySlotFreeForItem(slotItem))
		{
			if (slotItem.IsTooExpensive)
			{
				_shopMenu.AddCursorMessage(TextBundle.GetText("cursor_text_too_expensive"), GetSlotPosition(slotIndex) + ShopMenu.SLOT_CENTER_OFFSET, Color.Red);
				GameCore.PlaySound("menu_denied");
			}
			else
			{
				_shopMenu.PanelCargo.BuyItem(GameJnG.Cargo.GetFirstEmptyIndex(), slotItem, tryInstalToBigShip: true);
				_shopMenu.CancelSelection();
			}
		}
		else
		{
			_shopMenu.PanelCargo.BuyItem(GameJnG.Cargo.GetFirstEmptyIndex(), slotItem);
		}
	}

	public void ScrollToSlot(int index)
	{
		if (index >= 0 && index < _slotList.Count)
		{
			ShopSlot shopSlot = _slotList[index];
			if (shopSlot.Position.Y < 50f || index == 0)
			{
				_targetScrollOffsetY = 0f - shopSlot.Offset.Y + 50f;
			}
			else if (shopSlot.Position.Y >= 1070f || index == _slotList.Count - 1)
			{
				_targetScrollOffsetY = 0f - shopSlot.Offset.Y + 830f;
			}
		}
	}

	public void SetCursorSlotByY(float slotY)
	{
		for (int i = 0; i < _slotList.Count; i++)
		{
			if (_slotList[i].Position.Y >= slotY || _slotList[i].Position.Y + 66f > slotY)
			{
				SetCursorSlot(i);
				break;
			}
		}
		_shopMenu.ChangeFocus(this);
	}

	public void SetCursorSlot(int index)
	{
		if (index < 0)
		{
			index = 0;
		}
		else if (index >= _slotList.Count)
		{
			index = _slotList.Count - 1;
		}
		if (CursorSlotIndex != index && _targetScrollOffsetY == _scrollOffsetY)
		{
			GameCore.PlaySound("menu_item_select");
		}
		CursorSlotIndex = index;
		_shopMenu.SetCursorItem(GetSlotItem(index));
		ScrollToSlot(index);
		_shopMenu.IsCursorOnSlot = true;
		_shopMenu.ChangeFocus(this);
	}

	public override void CancelCursorSelection()
	{
		base.CancelCursorSelection();
		CursorSlotIndex = -1;
	}

	private void ParseInput()
	{
		if (Options.MasterOfTheUniverse && !GameCore.Console.IsActive && GameInput.SystemKeyPressed(Keys.I))
		{
			_shopMenu.CancelSelection();
			if (GameInput.ControlDown)
			{
				if (GameInput.ShiftDown)
				{
					GameJnG.ShopOffer.Build(999999, 999999);
				}
				else
				{
					GameJnG.ShopOffer.Build(GameJnG.StageNumber, 999999);
				}
			}
			else
			{
				GameJnG.ShopOffer.Build(GameJnG.StageNumber);
			}
		}
		if (_shopMenu.IsAnyPopupMenuVisible)
		{
			return;
		}
		if (_shopMenu.Cursor.Position.X >= Position.X - 330f && _shopMenu.Cursor.Position.X <= Position.X && !GameInput.ControlDown)
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
		if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeftWithControl))
		{
			CancelCursorSelection();
			_shopMenu.BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.front_gun);
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveDownWithControl) || (GameInput.ControlDown && GameInput.GetMouseWheelDelta() < 0))
		{
			ShopDivider.ShopDividerTypeEnum shopDividerTypeEnum = ShopDivider.ShopDividerTypeEnum.None;
			if (CursorSlotIndex >= 0)
			{
				shopDividerTypeEnum = ShopDivider.GetDividerType(GetSlotItem(CursorSlotIndex));
			}
			for (int i = CursorSlotIndex; i < _slotList.Count; i++)
			{
				if (shopDividerTypeEnum != ShopDivider.GetDividerType(GetSlotItem(i)))
				{
					SetCursorSlot(i);
					_targetScrollOffsetY = 0f - _slotList[i].Offset.Y + 36f;
					break;
				}
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUpWithControl) || (GameInput.ControlDown && GameInput.GetMouseWheelDelta() > 0))
		{
			ShopDivider.ShopDividerTypeEnum shopDividerTypeEnum2 = ShopDivider.ShopDividerTypeEnum.None;
			if (CursorSlotIndex >= 0)
			{
				shopDividerTypeEnum2 = ShopDivider.GetDividerType(GetSlotItem(CursorSlotIndex));
			}
			int num = 0;
			for (int num2 = CursorSlotIndex; num2 >= 0; num2--)
			{
				if (shopDividerTypeEnum2 != ShopDivider.GetDividerType(GetSlotItem(num2)))
				{
					shopDividerTypeEnum2 = ShopDivider.GetDividerType(GetSlotItem(num2));
					num = num2;
					break;
				}
			}
			int num3 = 0;
			for (int num4 = num - 1; num4 >= 0; num4--)
			{
				if (shopDividerTypeEnum2 != ShopDivider.GetDividerType(GetSlotItem(num4)))
				{
					num3 = num4 + 1;
					break;
				}
			}
			SetCursorSlot(num3);
			_targetScrollOffsetY = 0f - _slotList[num3].Offset.Y + 36f;
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp))
		{
			if (CursorSlotIndex > 0)
			{
				SetCursorSlot(CursorSlotIndex - 1);
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveDown))
		{
			SetCursorSlot(CursorSlotIndex + 1);
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.PageUp))
		{
			bool flag = false;
			for (int num5 = _slotList.Count - 1; num5 >= 0; num5--)
			{
				if (_slotList[num5].Position.Y < -750f)
				{
					SetCursorSlot(num5);
					_targetScrollOffsetY = 0f - _slotList[num5].Offset.Y;
					flag = true;
					break;
				}
			}
			if (!flag && _slotList.Count > 0)
			{
				SetCursorSlot(0);
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.PageDown))
		{
			bool flag2 = false;
			for (int j = 0; j < _slotList.Count; j++)
			{
				if (_slotList[j].Position.Y > 1000f)
				{
					SetCursorSlot(j);
					_targetScrollOffsetY = 0f - _slotList[j].Offset.Y;
					flag2 = true;
					break;
				}
			}
			if (!flag2 && _slotList.Count > 0)
			{
				SetCursorSlot(_slotList.Count - 1);
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.Home))
		{
			SetCursorSlot(0);
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.End))
		{
			SetCursorSlot(9999);
		}
		else if (!_shopMenu.PanelInfo.IsAnyButtonSelected && _shopMenu.KeyPressed(ShopMenuKeys.MoveLeft))
		{
			Vector2 cursorSlotPosition = GetCursorSlotPosition();
			if (cursorSlotPosition.Y > 850f)
			{
				if (!_shopMenu.PanelInfo.SelectDefaultButton())
				{
					_shopMenu.BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.equipment_6);
				}
			}
			else if (cursorSlotPosition.Y > 500f)
			{
				_shopMenu.BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.equipment_6);
			}
			else if (cursorSlotPosition.Y > 250f)
			{
				_shopMenu.SelectTestRangeButton();
			}
			else
			{
				_shopMenu.SelectTopButton(4);
			}
			CancelCursorSelection();
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRight) || _shopMenu.KeyPressed(ShopMenuKeys.MoveRightWithControl))
		{
			_shopMenu.PanelCargo.SetCursorSlotByY(GetCursorSlotPosition().Y - 36f, 0);
			CancelCursorSelection();
		}
	}

	public int GetSlotIndex(HeroItem item)
	{
		for (int i = 0; i < _slotList.Count; i++)
		{
			if (_slotList[i].Item == item)
			{
				return i;
			}
		}
		return -1;
	}

	public float GetMaxScrollOffsetY()
	{
		int num = _slotList.Count;
		if (num < 1)
		{
			num = 1;
		}
		return 66 * num + _dividerList.Count * 33 - 930;
	}

	public void UpdateSlider(float elapsedTime)
	{
		float num = 836f;
		float num2 = 173f;
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
			if (_shopMenu.Cursor.IsInRect(WorldPosition.X - 31f, WorldPosition.Y + 118f, 20, 950))
			{
				_isSliderBarUnderCursor = true;
			}
			else
			{
				_isSliderBarUnderCursor = false;
			}
		}
		if (GameInput.MouseDown(GameInput.MouseButton.Left))
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
		UpdateSliderLight(elapsedTime, bForceLight: false);
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
			_shopMenu.SetHintText("hint_scroll_shop");
		}
	}

	public void UpdateScrolling(float elapsedTime)
	{
		float maxScrollOffsetY = GetMaxScrollOffsetY();
		if (!_isSliderVisible)
		{
			_targetScrollOffsetY = 0f;
		}
		else if (_targetScrollOffsetY > 0f)
		{
			_targetScrollOffsetY = 0f;
		}
		else if (_targetScrollOffsetY < 0f - maxScrollOffsetY)
		{
			_targetScrollOffsetY = 0f - maxScrollOffsetY;
		}
		float value = (_scrollOffsetY - _targetScrollOffsetY) * 8f;
		value = Math.Abs(value);
		if (_isSliderDragged)
		{
			value *= 10f;
		}
		if (value < 100f)
		{
			value = 100f;
		}
		Utils.ChangeValueToTarget(ref _scrollOffsetY, _targetScrollOffsetY, value * elapsedTime);
	}

	public void CancelSelection()
	{
		_selectedSlotIndex = -1;
	}

	public override void Update(float elapsedTime)
	{
		if (State == ShopMenuState.Hidden)
		{
			return;
		}
		base.Update(elapsedTime);
		if (_shopMenu.CursorItem == null && _shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
		{
			CursorSlotIndex = -1;
		}
		_isSliderVisible = _slotList.Count > 0 && _slotList[_slotList.Count - 1].Offset.Y > 900f;
		UpdateScrolling(elapsedTime);
		if (_isSliderVisible)
		{
			UpdateSlider(elapsedTime);
		}
		ParseInput();
		UpdateSlots(elapsedTime);
		if (_shopMenu.IsAnyPopupMenuVisible)
		{
			return;
		}
		if (!_isSliderDragged && HasFocus && CursorSlotIndex >= 0)
		{
			if (_shopMenu.KeyPressed(ShopMenuKeys.ActionWithShift))
			{
				DoubleClickToSlot(CursorSlotIndex);
			}
			else if (_shopMenu.KeyPressed(ShopMenuKeys.Select))
			{
				if (GameInput.DoubleClick)
				{
					DoubleClickToSlot(CursorSlotIndex);
				}
				else
				{
					ClickToSlot(CursorSlotIndex);
				}
			}
			else if (_shopMenu.KeyPressed(ShopMenuKeys.ActionWithControl))
			{
				HeroItem slotItem = GetSlotItem(CursorSlotIndex);
				if (slotItem != null)
				{
					if (!slotItem.IsTooExpensive)
					{
						_shopMenu.PanelCargo.BuyItem(GameJnG.Cargo.GetFirstEmptyIndex(), slotItem);
					}
					else
					{
						_shopMenu.AddCursorMessage(TextBundle.GetText("cursor_text_too_expensive"), GetSlotPosition(CursorSlotIndex) + ShopMenu.SLOT_CENTER_OFFSET, Color.Red);
						GameCore.PlaySound("menu_denied");
					}
				}
			}
			else if (_shopMenu.KeyPressed(ShopMenuKeys.Action))
			{
				ClickToSlot(CursorSlotIndex);
			}
		}
		if (_shopMenu.KeyPressed(ShopMenuKeys.CancelAll) && _shopMenu.SelectionMode == SelectionModeEnum.Selected)
		{
			_shopMenu.CancelSelection();
		}
	}

	public HeroItem GetSlotItem(int slotIndex)
	{
		if (slotIndex >= 0 && slotIndex < _slotList.Count)
		{
			return _slotList[slotIndex].Item;
		}
		return null;
	}

	public Vector2 GetSlotPosition(HeroItem item)
	{
		return _slotList.FirstOrDefault((ShopSlot s) => s.Item == item)?.Position ?? Vector2.Zero;
	}

	public Vector2 GetSlotPosition(int slotIndex)
	{
		if (slotIndex >= 0 && slotIndex < _slotList.Count)
		{
			return _slotList[slotIndex].Position;
		}
		return Vector2.Zero;
	}

	public Vector2 GetCursorSlotPosition()
	{
		return GetSlotPosition(CursorSlotIndex);
	}

	public void PrepareSlots()
	{
		_slotList = new List<ShopSlot>();
		_dividerList = new List<ShopDivider>();
		ShopDivider shopDivider = null;
		float num = 0f;
		for (int i = 0; i < GameJnG.ShopOffer.ItemList.Count; i++)
		{
			if (shopDivider == null || shopDivider.Type != ShopDivider.GetDividerType(GameJnG.ShopOffer.ItemList[i]))
			{
				shopDivider = new ShopDivider();
				shopDivider.Type = ShopDivider.GetDividerType(GameJnG.ShopOffer.ItemList[i]);
				switch (shopDivider.Type)
				{
				case ShopDivider.ShopDividerTypeEnum.Weapon:
					shopDivider.SymbolFrame = 0;
					shopDivider.SymbolOffset = new Vector2(55f, 11f);
					break;
				case ShopDivider.ShopDividerTypeEnum.Missile:
					shopDivider.SymbolFrame = 2;
					shopDivider.SymbolOffset = new Vector2(40f, 11f);
					break;
				case ShopDivider.ShopDividerTypeEnum.Bomb:
					shopDivider.SymbolFrame = 3;
					shopDivider.SymbolOffset = new Vector2(33f, 11f);
					break;
				case ShopDivider.ShopDividerTypeEnum.Device:
					shopDivider.SymbolFrame = 4;
					shopDivider.SymbolOffset = new Vector2(29f, 12f);
					break;
				case ShopDivider.ShopDividerTypeEnum.Equipment:
					shopDivider.SymbolFrame = 5;
					shopDivider.SymbolOffset = new Vector2(47f, 12f);
					break;
				case ShopDivider.ShopDividerTypeEnum.Gem:
					shopDivider.SymbolFrame = 6;
					shopDivider.SymbolOffset = new Vector2(25f, 12f);
					break;
				case ShopDivider.ShopDividerTypeEnum.Other:
					shopDivider.SymbolFrame = 8;
					shopDivider.SymbolOffset = new Vector2(32f, 14f);
					break;
				default:
					shopDivider.SymbolFrame = -1;
					break;
				}
				shopDivider.Offset = new Vector2(-286f, num);
				_dividerList.Add(shopDivider);
				num += 33f;
			}
			ShopSlot shopSlot = new ShopSlot();
			shopSlot.Item = GameJnG.ShopOffer.ItemList[i];
			shopSlot.Offset = new Vector2(-289f, num);
			num += 66f;
			_slotList.Add(shopSlot);
		}
		UpdateSlots(0f);
	}

	private void UpdateSlots(float elapsedTime)
	{
		for (int i = 0; i < _dividerList.Count; i++)
		{
			ShopDivider shopDivider = _dividerList[i];
			shopDivider.Position = Position + shopDivider.Offset;
			shopDivider.Position.Y += 123f + _scrollOffsetY;
		}
		for (int j = 0; j < _slotList.Count; j++)
		{
			ShopSlot shopSlot = _slotList[j];
			shopSlot.Position = Position + shopSlot.Offset;
			shopSlot.Position.Y += 124f + _scrollOffsetY;
			if (!_shopMenu.IsAnyPopupMenuVisible && !_isSliderDragged && !_isSliderUnderCursor && _shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse && shopSlot.Position.Y > 50f && shopSlot.Position.Y < 1070f && _shopMenu.Cursor.IsInRect(shopSlot.Position.X - 3f, shopSlot.Position.Y - 1f, 255, 66))
			{
				SetCursorSlot(j);
			}
		}
	}

	private void DrawSlots()
	{
		float x = Position.X - 205f;
		for (int i = 0; i < _dividerList.Count; i++)
		{
			ShopDivider shopDivider = _dividerList[i];
			_shopMenu.AtlasBigButton.DrawFrame(_shopMenu.SpriteBatchLight, 2, Color.White, shopDivider, BlendModeEnum.Normal);
			Transformation transformation = Transformation.CreateByPosition(shopDivider.Position + shopDivider.SymbolOffset);
			transformation.Scale *= 0.75f;
			transformation.Position.X -= _shopMenu.AtlasSlotTypeIcon.GetFrameRectangle(shopDivider.SymbolFrame).Width / 2;
			_shopMenu.AtlasSlotTypeIcon.DrawFrame(_shopMenu.SpriteBatchLight, shopDivider.SymbolFrame, Color.White, transformation, BlendModeEnum.Normal);
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, $"{TextBundle.GetText($"shop_divider_{shopDivider.Type.ToString()}")}", x, shopDivider.Position.Y + 10f - 6f * Options.FontScale, ShopMenu.FONT_SCALE_TEXT_NORMAL);
		}
		for (int j = 0; j < _slotList.Count; j++)
		{
			ShopSlot shopSlot = _slotList[j];
			if (!(shopSlot.Position.Y < 50f))
			{
				if (shopSlot.Position.Y > 1080f)
				{
					break;
				}
				_shopMenu.DrawSlot(_shopMenu.AtlasSlot, shopSlot, shopSlot.Item, 1f, bAsLoadoutPreview: false);
				if (shopSlot.Item != null && shopSlot.Item == _shopMenu.SelectedItem)
				{
					_shopMenu.AtlasSlot.DrawFrame(_shopMenu.SpriteBatchLight, 2, _shopMenu.GetSlotSelectedColor(), shopSlot);
				}
				if (CursorSlotIndex == j)
				{
					_shopMenu.AtlasSlot.DrawFrame(_shopMenu.SpriteBatchLight, 1, _shopMenu.GetSlotCursorColor(), shopSlot);
				}
				float num = shopSlot.Position.Y + 2f;
				_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, $"{shopSlot.Item.GetShopName()}", x, num, ShopMenu.FONT_SCALE_TEXT_NORMAL);
				_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, $"{Utils.FormatPrice(shopSlot.Item.Price, shopSlot.Item.Price > GameJnG.Cash)}", x, num + 26f, ShopMenu.FONT_SCALE_TEXT_NORMAL);
			}
		}
		if (_slotList.Count == 0)
		{
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("shop_no_offer"), WorldPosition.X - 160f, WorldPosition.Y + 520f, ShopMenu.FONT_SCALE_TEXT_NORMAL, new Color(200, 200, 200), TextAlign.Center);
		}
	}

	public override void Draw()
	{
		if (State != ShopMenuState.Hidden)
		{
			_shopMenu.SpriteBatchBegin(bLight: true);
			_atlasBackground.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color.White, this);
			DrawSlots();
			_spriteRenderer.SetAtlas(_atlasFrame);
			for (int i = 0; i < 4; i++)
			{
				_spriteRenderer.Frame = i;
				_spriteRenderer.Draw(this, _shopMenu.SpriteBatchLight, BlendModeEnum.Normal);
			}
			Vector2 vector = Position + new Vector2(-158f, 46f);
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("shop_panel_label_shop"), vector.X, vector.Y - 11f * Options.FontScale, ShopMenu.FONT_SCALE_PRICE_TV, null, TextAlign.Center);
			Transformation transform = Transformation.CreateByPosition(Position + new Vector2(-30f, 122f));
			_atlasFrame.DrawFrame(_shopMenu.SpriteBatchLight, 4, new Color(255, 255, 255, (int)_currSliderBarLightAlpha), transform, BlendModeEnum.Normal);
			if (_isSliderVisible)
			{
				_sliderObject.Draw();
			}
			_shopMenu.SpriteBatchEnd(bLight: true);
		}
	}
}
