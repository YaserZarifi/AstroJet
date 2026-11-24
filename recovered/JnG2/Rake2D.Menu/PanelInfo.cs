using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.JnG;
using Rake2D.Script;

namespace Rake2D.Menu;

public class PanelInfo : ShopMenuPanel
{
	public static Vector2 TV_CENTER = new Vector2(133f, 97f);

	private const int LINE_HEIGHT = 25;

	private const int BUTTON_WIDTH = 190;

	public Transformation TvTransformation;

	public HeroItem TVItem;

	private SpriteAtlas _atlasPriceTV;

	private SpriteAtlas _atlasParamBox;

	private SpriteAtlas _atlasParamArrow;

	private SpriteAtlas _atlasItemLevel;

	private SpriteAtlas _atlasLevelPreview;

	private Texture2D _textureLevelPreview1;

	private Texture2D _textureLevelPreview2;

	private SpriteAtlas _atlasTVDiode;

	private SpriteAtlas _atlasTVDiodeFlare;

	private SpriteRenderer _spriteRendererTVRedDiode;

	private float _tvDiodeAlphaRed;

	private float _tvDiodeAlphaGreen;

	private float _tvDiodeAlphaBlue;

	private float _tvTargetDiodeAlphaRed;

	private float _tvTargetDiodeAlphaGreen;

	private float _tvTargetDiodeAlphaBlue;

	private float _wheelTimer;

	private int _medalIndex;

	private int _switchToMedalIndex;

	private LevelData _levelData;

	private LevelData _switchToLevelData;

	private PanelInfoSwitchingModeEnum _switchingMode;

	private TVNoise _tvNoise;

	private GameObject _switchToTVObject;

	private PartData _tvItemPartData;

	private PartData _tvAdvertPartData;

	private Button _buttonBuy;

	private Button _buttonSell;

	private Button _buttonUpgrade;

	private Button _buttonDowngrade;

	private Button _buttonStartLevel;

	private Button _buttonAction;

	public bool IsAnyButtonSelected
	{
		get
		{
			if (!IsStartButtonSelected && !_buttonBuy.IsSelected && !_buttonSell.IsSelected && !_buttonUpgrade.IsSelected && !_buttonDowngrade.IsSelected)
			{
				return _buttonAction.IsSelected;
			}
			return true;
		}
	}

	public bool IsStartButtonSelected
	{
		get
		{
			return _buttonStartLevel.IsSelected;
		}
		set
		{
			if (value)
			{
				_buttonStartLevel.Select();
				_buttonUpgrade.Unselect();
				_buttonDowngrade.Unselect();
				_buttonSell.Unselect();
				_buttonBuy.Unselect();
				_buttonAction.Unselect();
			}
			else
			{
				_buttonStartLevel.Unselect();
			}
		}
	}

	public int GetMedalIndex()
	{
		if (_switchToMedalIndex == _medalIndex || _switchToMedalIndex < 0)
		{
			return _medalIndex;
		}
		return _switchToMedalIndex;
	}

	public PanelInfo(ShopMenu shopMenu)
		: base(shopMenu)
	{
		Init();
	}

	public override void Init()
	{
		base.Init();
		_atlasFrame = GameCore.GetSpriteAtlas("shop_panel_info", logWhenNotFound: true);
		_atlasPriceTV = GameCore.GetSpriteAtlas("shop_price_tv", logWhenNotFound: true);
		_atlasParamBox = GameCore.GetSpriteAtlas("shop_param_box", logWhenNotFound: true);
		_atlasBackground = GameCore.GetSpriteAtlas("shop_panel_info_bgr", logWhenNotFound: true);
		_atlasParamArrow = GameCore.GetSpriteAtlas("shop_param_arrow", logWhenNotFound: true);
		_atlasItemLevel = GameCore.GetSpriteAtlas("shop_item_level", logWhenNotFound: true);
		_atlasTVDiode = GameCore.GetSpriteAtlas("shop_tv_diode", logWhenNotFound: true);
		_atlasTVDiodeFlare = GameCore.GetSpriteAtlas("flare", logWhenNotFound: true);
		_textureLevelPreview1 = GameCore.GetTexture2D("level_preview_1");
		_textureLevelPreview2 = GameCore.GetTexture2D("level_preview_2");
		_atlasLevelPreview = GameCore.GetSpriteAtlas("level_preview", logWhenNotFound: true);
		_spriteRendererTVRedDiode = GameCore.Allocator.GetSpriteRenderer(_atlasTVDiode);
		_spriteRendererTVRedDiode.ChangeAnim("red_light", 0.5f);
		_tvItemPartData = GameCore.GetPartData("tv_item");
		_tvAdvertPartData = GameCore.GetPartData("tv_item_ad");
		Position = new Vector2(375f, 1210f);
		TargetPosition = Position;
		TvTransformation = new Transformation();
		TvTransformation.ParentTransformation = this;
		TvTransformation.Position.X = -34f;
		TvTransformation.Position.Y = 20f;
		_tvNoise = new TVNoise(_shopMenu);
		_tvNoise.ParentTransformation = this;
		_tvNoise.Init();
		_tvNoise.Position = new Vector2(100f, 116f);
		_tvNoise.IsAutoHiding = true;
		_tvNoise.FadeSpeed = 1500f;
		_buttonBuy = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonBuy.ParentTransformation = this;
		_buttonBuy.HasPressEffectEnabled = true;
		_buttonBuy.OnClick += DoOnButtonBuyClick;
		_buttonSell = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonSell.ParentTransformation = this;
		_buttonSell.OnClick += DoOnButtonSellClick;
		_buttonUpgrade = new Button(_shopMenu, ShopButtonType.Price);
		_buttonUpgrade.ParentTransformation = this;
		_buttonUpgrade.HasPressEffectEnabled = true;
		_buttonUpgrade.OnClick += DoOnButtonUpgradeClick;
		_buttonDowngrade = new Button(_shopMenu, ShopButtonType.Price);
		_buttonDowngrade.ParentTransformation = this;
		_buttonDowngrade.HasPressEffectEnabled = true;
		_buttonDowngrade.OnClick += DoOnButtonDowngradeClick;
		_buttonStartLevel = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonStartLevel.ParentTransformation = this;
		_buttonStartLevel.OnClick += DoOnButtonStartLevelClick;
		_buttonAction = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonAction.ParentTransformation = this;
		_buttonAction.HasPressEffectEnabled = true;
		_buttonAction.OnClick += DoOnButtonActionClick;
		_buttonBuy.Position = new Vector2(1072f, 268f);
		_buttonSell.Position = _buttonBuy.Position;
		_buttonDowngrade.Position = _buttonBuy.Position;
		_buttonDowngrade.Position.X -= 190f;
		_buttonUpgrade.Position = _buttonDowngrade.Position;
		_buttonUpgrade.Position.X -= 190f;
		_buttonStartLevel.Position = _buttonBuy.Position;
		_buttonAction.Position = _buttonDowngrade.Position;
	}

	public void SetTexts()
	{
		_buttonBuy.Caption = TextBundle.GetText("button_buy");
		_buttonSell.Caption = TextBundle.GetText("button_sell");
		_buttonUpgrade.Caption = TextBundle.GetText("button_upgrade");
		_buttonDowngrade.Caption = TextBundle.GetText("button_downgrade");
		_buttonStartLevel.Caption = TextBundle.GetText("button_start_level");
	}

	public bool SelectDefaultButton()
	{
		bool flag = false;
		if (_buttonBuy.IsVisible)
		{
			_buttonBuy.Select();
			flag = true;
		}
		else if (_buttonSell.IsVisible)
		{
			_buttonSell.Select();
			flag = true;
		}
		else if (_buttonStartLevel.IsVisible)
		{
			_buttonStartLevel.Select();
			flag = true;
		}
		if (flag)
		{
			_shopMenu.ChangeFocus(this);
		}
		return flag;
	}

	public void UnselectAllButtons()
	{
		_buttonStartLevel.Unselect();
		_buttonUpgrade.Unselect();
		_buttonDowngrade.Unselect();
		_buttonSell.Unselect();
		_buttonBuy.Unselect();
		_buttonAction.Unselect();
	}

	public int GetPlanetPreviewFrame(int planetNumber)
	{
		return planetNumber % 15;
	}

	public void DoOnButtonBuyClick(Button button)
	{
		if (!button.IsDisabled && !button.IsDenied)
		{
			if (_shopMenu.SelectedItem != null && _shopMenu.SelectedItem.Place == ItemPlaceEnum.InShop)
			{
				_shopMenu.PanelCargo.BuyItem(GameJnG.Cargo.GetFirstEmptyIndex(), _shopMenu.SelectedItem);
			}
			return;
		}
		if (button.IsDenied)
		{
			_shopMenu.AddCursorMessage(TextBundle.GetText("cursor_text_too_expensive"), button.WorldPosition, Color.Red);
		}
		GameCore.PlaySound("menu_denied");
	}

	public void DoOnButtonSellClick(Button button)
	{
		_shopMenu.SellItem(_shopMenu.SelectedItem);
	}

	public void DoOnButtonUpgradeClick(Button button)
	{
		if (!button.IsDisabled && !button.IsDenied)
		{
			_shopMenu.UpgradeItem(_shopMenu.SelectedItem);
			return;
		}
		if (button.IsDenied)
		{
			_shopMenu.AddCursorMessage(TextBundle.GetText("cursor_text_too_expensive"), button.WorldPosition, Color.Red);
		}
		else if (button.IsDisabled)
		{
			_shopMenu.AddCursorMessage(TextBundle.GetText("cursor_text_max_upgrade"), button.WorldPosition, Color.LightGray);
		}
		GameCore.PlaySound("menu_denied");
	}

	public void DoOnButtonDowngradeClick(Button button)
	{
		if (!button.IsDisabled && !button.IsDenied)
		{
			_shopMenu.DowngradeItem(_shopMenu.SelectedItem);
			return;
		}
		if (button.IsDisabled)
		{
			_shopMenu.AddCursorMessage(TextBundle.GetText("cursor_text_min_downgrade"), button.WorldPosition, Color.LightGray);
		}
		GameCore.PlaySound("menu_denied");
	}

	public void DoOnButtonStartLevelClick(Button button)
	{
		if (_switchToLevelData != null || _levelData != null)
		{
			_shopMenu.Hide(toMainMenu: false, _switchToLevelData ?? _levelData);
		}
		else
		{
			GameCore.PlaySound("menu_denied");
		}
	}

	public void DoItemAction(HeroItem item)
	{
		if (item.IsShopActionDisabled)
		{
			return;
		}
		switch (item.Data.ShopAction)
		{
		case ItemShopActionEnum.ViewObject:
			_shopMenu.ItemView.Show(item);
			break;
		case ItemShopActionEnum.ApplyLoadout:
			if (item.Data.ItemType == ItemTypeEnum.Loadout && item.Loadout != null)
			{
				_shopMenu.ApplyLoadoutItem(item);
			}
			break;
		case ItemShopActionEnum.ShowAtarix:
			_shopMenu.Atarix.ShowToMainMenu();
			break;
		case ItemShopActionEnum.Hack:
			_shopMenu.Atarix.ShowToHackMenu();
			break;
		case ItemShopActionEnum.DrinkBeer:
			GameJnG.IsDrunkenEffectEnabled = true;
			GameCore.PlaySound("hero_drunken_0");
			FxEffect.StartDrunkenEffect(_shopMenu.HeroPreviewLevel);
			GameJnG.Cargo.RemoveItem(item);
			_shopMenu.CancelSelection();
			break;
		case ItemShopActionEnum.GetItem:
			if (item.DoGetItem() != null)
			{
				_shopMenu.CancelSelection();
				GameCore.PlaySound("menu_item_tv_hide");
			}
			break;
		}
	}

	private void DoOnButtonActionClick(Button button)
	{
		if (TVItem != null)
		{
			DoItemAction(TVItem);
		}
	}

	private void DoOnButtonSaveLoadoutClick(Button button)
	{
	}

	public override void Show(float showDelay)
	{
		base.Show(showDelay);
		TargetPosition.Y = 740f;
		_switchToTVObject = null;
		_medalIndex = -1;
		_switchToMedalIndex = -1;
		_wheelTimer = 0f;
	}

	public override void Hide()
	{
		base.Hide();
		TargetPosition.Y = 1210f;
		_switchToLevelData = null;
	}

	private void UpdateButtons(float elapsedTime)
	{
		_buttonUpgrade.IsVisible = false;
		_buttonDowngrade.IsVisible = false;
		if (_shopMenu.IsAnyPopupMenuVisible)
		{
			_buttonBuy.IsInputEnabled = false;
			_buttonDowngrade.IsInputEnabled = false;
			_buttonUpgrade.IsInputEnabled = false;
			_buttonSell.IsInputEnabled = false;
			_buttonStartLevel.IsInputEnabled = false;
			_buttonAction.IsInputEnabled = false;
		}
		else
		{
			_buttonBuy.IsInputEnabled = true;
			_buttonDowngrade.IsInputEnabled = true;
			_buttonUpgrade.IsInputEnabled = true;
			_buttonSell.IsInputEnabled = true;
			_buttonStartLevel.IsInputEnabled = true;
			_buttonAction.IsInputEnabled = true;
		}
		if (TVItem != null)
		{
			if (TVItem.Place == ItemPlaceEnum.InShop)
			{
				if (TVItem.Price > GameJnG.Cash)
				{
					_buttonBuy.IsDenied = true;
				}
				else
				{
					_buttonBuy.IsDenied = false;
				}
				_buttonBuy.PriceValue = TVItem.Price;
				_buttonBuy.Update(elapsedTime);
			}
			else
			{
				if (TVItem.Data.ShopAction != ItemShopActionEnum.None && !TVItem.IsShopActionDisabled)
				{
					_buttonAction.Caption = TextBundle.GetText(TVItem.Data.ShopActionButtonLabel);
					_buttonAction.Update(elapsedTime);
					if (TVItem == null)
					{
						return;
					}
				}
				if (TVItem.IsUpgradable)
				{
					_buttonDowngrade.IsVisible = true;
					_buttonUpgrade.IsVisible = true;
					if (TVItem.Level >= TVItem.Data.MaxLevel)
					{
						_buttonUpgrade.IsDisabled = true;
						_buttonUpgrade.PriceValue = -1L;
						_buttonUpgrade.IsDenied = false;
					}
					else
					{
						_buttonUpgrade.PriceValue = TVItem.GetUpgradePrice(TVItem.Level + 1);
						_buttonUpgrade.IsDisabled = false;
						if (TVItem.IsUpgradeTooExpensive)
						{
							_buttonUpgrade.IsDenied = true;
						}
						else
						{
							_buttonUpgrade.IsDenied = false;
						}
					}
					_buttonUpgrade.Update(elapsedTime);
					if (TVItem.Level <= 0)
					{
						_buttonDowngrade.IsDisabled = true;
						_buttonDowngrade.PriceValue = -1L;
					}
					else
					{
						_buttonDowngrade.IsDisabled = false;
						_buttonDowngrade.PriceValue = TVItem.GetUpgradePrice(TVItem.Level - 1);
					}
					_buttonDowngrade.Update(elapsedTime);
				}
				if (TVItem == null)
				{
					return;
				}
				if (TVItem.Data.ShopAction != ItemShopActionEnum.None && !TVItem.IsShopActionDisabled)
				{
					_buttonAction.IsVisible = true;
				}
				else
				{
					_buttonAction.IsVisible = false;
				}
				_buttonSell.PriceValue = TVItem.Price;
				_buttonSell.Update(elapsedTime);
			}
		}
		if (_shopMenu.CurrentScreen == ShopScreenEnum.Mission)
		{
			_buttonStartLevel.IsVisible = true;
			_buttonStartLevel.Update(elapsedTime);
		}
		else
		{
			_buttonStartLevel.IsVisible = false;
		}
		if (_buttonBuy.IsSelected)
		{
			if (_buttonBuy.IsDenied)
			{
				_shopMenu.CursorType = CursorTypeEnum.TooExpensive;
				_shopMenu.SetHintText("hint_too_expensive", TVItem);
			}
			else
			{
				_shopMenu.CursorType = CursorTypeEnum.Buy;
				_shopMenu.SetHintText("hint_buy", TVItem);
			}
		}
		else if (_buttonSell.IsSelected)
		{
			_shopMenu.CursorType = CursorTypeEnum.Buy;
			_shopMenu.SetHintText("hint_sell", TVItem);
		}
		else if (_buttonAction.IsSelected)
		{
			if (TVItem != null && TVItem.Data.ShopActionButtonHint != null)
			{
				_shopMenu.SetHintText(TVItem.Data.ShopActionButtonHint, TVItem);
			}
		}
		else if (_buttonUpgrade.IsSelected)
		{
			if (_buttonUpgrade.IsDenied)
			{
				_shopMenu.CursorType = CursorTypeEnum.TooExpensive;
				_shopMenu.SetHintText("hint_too_expensive_upgrade", TVItem);
			}
			else if (_buttonUpgrade.IsDisabled)
			{
				_shopMenu.SetHintText("hint_no_upgrade");
			}
			else
			{
				_shopMenu.SetHintText("hint_upgrade", TVItem);
			}
		}
		else if (_buttonDowngrade.IsSelected)
		{
			if (_buttonDowngrade.IsDisabled)
			{
				_shopMenu.SetHintText("hint_no_downgrade");
			}
			else
			{
				_shopMenu.SetHintText("hint_downgrade", TVItem);
			}
		}
		else if (_buttonStartLevel.IsSelected)
		{
			_shopMenu.SetHintText("hint_play_level");
		}
		if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.GamePad)
		{
			_buttonUpgrade.CaptionSymbol = TextBundle.GetText("button_upgrade_symbol");
			_buttonDowngrade.CaptionSymbol = TextBundle.GetText("button_downgrade_symbol");
			_buttonAction.CaptionSymbol = TextBundle.GetText("button_action_symbol");
		}
		else
		{
			_buttonBuy.CaptionSymbol = null;
			_buttonSell.CaptionSymbol = null;
			_buttonUpgrade.CaptionSymbol = null;
			_buttonDowngrade.CaptionSymbol = null;
			_buttonAction.CaptionSymbol = null;
		}
	}

	private void UpdateTVDiode(float elapsedTime)
	{
		if (Utils.ChangeValueToTarget(ref _tvDiodeAlphaRed, _tvTargetDiodeAlphaRed, 1f * elapsedTime))
		{
			_tvTargetDiodeAlphaRed = Utils.GetRandom(0.1f, 1f);
		}
		if (Utils.ChangeValueToTarget(ref _tvDiodeAlphaGreen, _tvTargetDiodeAlphaGreen, 2f * elapsedTime))
		{
			_tvTargetDiodeAlphaGreen = Utils.GetRandom(0.1f, 1f);
		}
		if (Utils.ChangeValueToTarget(ref _tvDiodeAlphaBlue, _tvTargetDiodeAlphaBlue, 1.5f * elapsedTime))
		{
			_tvTargetDiodeAlphaBlue = Utils.GetRandom(0.1f, 1f);
		}
		_spriteRendererTVRedDiode.Update(elapsedTime);
	}

	public override void CancelCursorSelection()
	{
		base.CancelCursorSelection();
		UnselectAllButtons();
	}

	private void ParseInput()
	{
		if (_buttonUpgrade.IsVisible && _shopMenu.KeyPressed(ShopMenuKeys.ButtonY) && !GameJnG.Mia)
		{
			_buttonUpgrade.Click();
		}
		else if (_buttonDowngrade.IsVisible && _shopMenu.KeyPressed(ShopMenuKeys.ButtonX) && !GameJnG.Mia)
		{
			_buttonDowngrade.Click();
		}
		else if (_buttonAction.IsVisible && _shopMenu.KeyPressed(ShopMenuKeys.ButtonX) && !GameJnG.Mia)
		{
			_buttonAction.Click();
		}
		HeroItem cursorItem = _shopMenu.CursorItem;
		if (cursorItem != null && _shopMenu.CursorItem.Place == ItemPlaceEnum.InHeroSlot && _wheelTimer < 0f && !GameJnG.Mia)
		{
			if (GameInput.GetMouseWheelDelta() > 0)
			{
				_wheelTimer = 0.1f;
				if (cursorItem.IsUpgradable && cursorItem.Level < cursorItem.Data.MaxLevel)
				{
					if (cursorItem.GetUpgradePrice(cursorItem.Level + 1) <= GameJnG.Cash)
					{
						_shopMenu.UpgradeItem(cursorItem);
					}
					else
					{
						_shopMenu.AddCursorMessage(TextBundle.GetText("cursor_text_too_expensive"), _shopMenu.BigShipSlotSet.GetSlotCenterPosition(cursorItem.MySlot.SlotType), Color.Red);
						GameCore.PlaySound("menu_denied");
					}
				}
				else
				{
					_shopMenu.AddCursorMessage(TextBundle.GetText("cursor_text_max_upgrade"), _shopMenu.BigShipSlotSet.GetSlotCenterPosition(cursorItem.MySlot.SlotType), Color.LightGray);
					GameCore.PlaySound("menu_denied");
				}
			}
			else if (GameInput.GetMouseWheelDelta() < 0)
			{
				_wheelTimer = 0.1f;
				if (cursorItem.IsUpgradable && cursorItem.Level > 0)
				{
					_shopMenu.DowngradeItem(cursorItem);
				}
				else
				{
					_shopMenu.AddCursorMessage(TextBundle.GetText("cursor_text_min_downgrade"), _shopMenu.BigShipSlotSet.GetSlotCenterPosition(cursorItem.MySlot.SlotType), Color.LightGray);
					GameCore.PlaySound("menu_denied");
				}
			}
		}
		if (!IsAnyButtonSelected)
		{
			return;
		}
		if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeftWithControl))
		{
			if (_shopMenu.CurrentScreen == ShopScreenEnum.Shop)
			{
				_shopMenu.PanelCargo.SetCursorSlotByY(_buttonBuy.WorldPosition.Y - 36f, 2);
				_shopMenu.PanelCargo.ScrollToSlot(_shopMenu.PanelCargo.CursorSlotIndex);
			}
			else
			{
				_shopMenu.PanelHeroStats.SelectFaceTV();
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRightWithControl))
		{
			if (_shopMenu.CurrentScreen == ShopScreenEnum.Shop)
			{
				_shopMenu.PanelShop.SetCursorSlotByY(_buttonBuy.WorldPosition.Y - 20f);
			}
			else
			{
				_shopMenu.PanelShipParams.SelectSliderByY(_buttonBuy.WorldPosition.Y - 20f);
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeft))
		{
			if (_shopMenu.CurrentScreen != ShopScreenEnum.Mission)
			{
				if (_buttonSell.IsSelected && _buttonDowngrade.IsVisible)
				{
					_buttonSell.Unselect();
					_buttonDowngrade.Select();
				}
				else if (_buttonSell.IsSelected && _buttonAction.IsVisible)
				{
					_buttonSell.Unselect();
					_buttonAction.Select();
				}
				else if (_buttonDowngrade.IsSelected && _buttonUpgrade.IsVisible)
				{
					_buttonDowngrade.Unselect();
					_buttonUpgrade.Select();
				}
				else
				{
					UnselectAllButtons();
					_shopMenu.BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.bottom_gun);
				}
			}
		}
		else if (IsAnyButtonSelected && _shopMenu.KeyPressed(ShopMenuKeys.MoveRight))
		{
			if (_buttonSell.IsSelected || _buttonBuy.IsSelected)
			{
				UnselectAllButtons();
				if (_shopMenu.CurrentScreen == ShopScreenEnum.Shop)
				{
					_shopMenu.PanelShop.SetCursorSlotByY(_buttonSell.WorldPosition.Y - 60f);
				}
				else if (_shopMenu.CurrentScreen == ShopScreenEnum.HeroProfile)
				{
					_shopMenu.PanelShipParams.SelectSlider(7);
				}
			}
			else if (_buttonAction.IsSelected && _buttonSell.IsVisible)
			{
				_buttonAction.Unselect();
				_buttonSell.Select();
			}
			else if (_buttonDowngrade.IsSelected && _buttonSell.IsVisible)
			{
				_buttonDowngrade.Unselect();
				_buttonSell.Select();
			}
			else if (_buttonUpgrade.IsSelected && _buttonDowngrade.IsVisible)
			{
				_buttonUpgrade.Unselect();
				_buttonDowngrade.Select();
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp))
		{
			if (_shopMenu.CurrentScreen != ShopScreenEnum.Mission)
			{
				CancelCursorSelection();
				_shopMenu.BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.equipment_1);
			}
			else
			{
				_shopMenu.ScreenMission.SetMouseSelectedLevelData(_shopMenu.ScreenMission.CurrentLevelData);
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveDown))
		{
			Button selectedButton = GetSelectedButton();
			if (selectedButton != null)
			{
				CancelCursorSelection();
				_shopMenu.PanelTopButtons.SelectButtonByX(selectedButton.WorldPosition.X - 150f);
			}
		}
	}

	public Button GetSelectedButton()
	{
		if (_buttonAction.IsSelected)
		{
			return _buttonAction;
		}
		if (_buttonBuy.IsSelected)
		{
			return _buttonBuy;
		}
		if (_buttonSell.IsSelected)
		{
			return _buttonSell;
		}
		if (_buttonUpgrade.IsSelected)
		{
			return _buttonUpgrade;
		}
		if (_buttonDowngrade.IsSelected)
		{
			return _buttonDowngrade;
		}
		if (_buttonStartLevel.IsSelected)
		{
			return _buttonStartLevel;
		}
		return null;
	}

	public override void Update(float elapsedTime)
	{
		if (State == ShopMenuState.Hidden)
		{
			return;
		}
		base.Update(elapsedTime);
		if (IsAnyButtonSelected)
		{
			_shopMenu.ChangeFocus(this);
		}
		TvTransformation.ResetPrecalculations();
		_wheelTimer -= elapsedTime;
		if (!_shopMenu.IsAnyPopupMenuVisible)
		{
			ParseInput();
		}
		if (_shopMenu.CurrentScreen != ShopScreenEnum.Mia)
		{
			UpdateButtons(elapsedTime);
		}
		_tvNoise.Update(elapsedTime);
		if (_tvNoise.IsHiding)
		{
			switch (_switchingMode)
			{
			case PanelInfoSwitchingModeEnum.Item:
				SetTVObject(_switchToTVObject);
				_medalIndex = -1;
				break;
			case PanelInfoSwitchingModeEnum.Medal:
				_medalIndex = _switchToMedalIndex;
				SetTVObject(_switchToTVObject);
				TVItem = null;
				break;
			case PanelInfoSwitchingModeEnum.LevelData:
				_levelData = _switchToLevelData;
				_medalIndex = -1;
				break;
			}
			_switchingMode = PanelInfoSwitchingModeEnum.None;
		}
		UpdateTVDiode(elapsedTime);
	}

	private GameObject GetItemTVObject(HeroItem item)
	{
		GameObject gameObject = null;
		if (item != null)
		{
			string tVObjectId = item.GetTVObjectId();
			if (tVObjectId != null)
			{
				gameObject = GameObject.CreateByPartDataSimple(GameCore.GetPartData(tVObjectId));
			}
			else
			{
				Texture2D texture2D = null;
				gameObject = GameObject.CreateByPartDataSimple(_tvItemPartData);
				string text = ((item.Place != ItemPlaceEnum.InShop || !item.Data.TvAdvertExtraPicture) ? $"ui/item/{item.Data.Id}.png" : $"ui/item/{item.Data.Id}_ad.png");
				texture2D = GameCore.TextureCache.Load(text);
				gameObject.SpriteRenderer.CachedTextureFileName = text;
				if (texture2D == null)
				{
					GameCore.LogErr("Unable to load tv advert texture for item '{0}'", item.Data.Id);
					texture2D = gameObject.SpriteRenderer.Atlas.Texture;
				}
				gameObject.SetTexture(texture2D);
				if (texture2D != null)
				{
					Rectangle frameRectangle = gameObject.SpriteRenderer.Atlas.GetFrameRectangle(0);
					int value = texture2D.Width / frameRectangle.Width * (texture2D.Height / frameRectangle.Height) - 1;
					gameObject.Frame = MathHelper.Min(value, item.Level);
				}
			}
			if (item.Place == ItemPlaceEnum.InShop)
			{
				GameObject gameObject2 = GameObject.CreateByPartDataSimple(_tvAdvertPartData);
				GameObject childByIdDeep = gameObject2.GetChildByIdDeep("item");
				if (gameObject != null)
				{
					if (childByIdDeep != null)
					{
						childByIdDeep.AddChildRelative(gameObject);
						gameObject.Rotation = 0f - childByIdDeep.WorldRotation;
					}
					else
					{
						GameCore.LogErr("Unable to find bone 'item' in object '{0}'", item.Data.Id);
					}
					gameObject = gameObject2;
					gameObject.SetSpineAnimation(0, item.Data.TvAdvertAnimId, bLooped: true);
					gameObject.GetChildByIdDeep("foreground")?.SetSpineAnimation(0, item.Data.TvAdvertAnimIdForeground, bLooped: true);
				}
			}
		}
		return gameObject;
	}

	private GameObject GetMedalTVObject(int medaIndex)
	{
		GameObject gameObject = null;
		Texture2D texture2D = null;
		gameObject = GameObject.CreateByPartDataSimple(_tvItemPartData);
		if (medaIndex >= 0)
		{
			string text = $"ui/item/medal/medal_{medaIndex}.png";
			texture2D = GameCore.TextureCache.Load(text);
			gameObject.SpriteRenderer.CachedTextureFileName = text;
			if (texture2D == null)
			{
				GameCore.LogErr("Unable to load tv texture for medal '{0}'", medaIndex);
				texture2D = gameObject.SpriteRenderer.Atlas.Texture;
			}
		}
		gameObject.SetTexture(texture2D);
		return gameObject;
	}

	public void SetTVItem(HeroItem item)
	{
		if (item != TVItem)
		{
			TVItem = item;
			_switchToTVObject = GetItemTVObject(item);
			_switchingMode = PanelInfoSwitchingModeEnum.Item;
			_tvNoise.Show();
			if (item != null)
			{
				GameCore.PlaySound("menu_item_tv_show");
			}
		}
		_levelData = null;
		_buttonAction.CancelPressEffect();
	}

	public void SetTVObject(GameObject go)
	{
		_shopMenu.LayerTV.DestroyAll();
		_shopMenu.LayerTV.Clear();
		_shopMenu.GUILevel.SetCurrentLayer(_shopMenu.LayerTV);
		if (go != null)
		{
			_shopMenu.GUILevel.CurrentLayer.AddGameObject(go);
			go.SetPosition(TV_CENTER);
			go.ParentTransformation = TvTransformation;
		}
	}

	public void SetPreviewLevel(LevelData levelData)
	{
		if (levelData != _levelData && levelData != _switchToLevelData)
		{
			_switchingMode = PanelInfoSwitchingModeEnum.LevelData;
			_switchToLevelData = levelData;
			if (levelData != null && _shopMenu.ScreenMission.State == ShopMenuState.Visible)
			{
				GameCore.PlaySound("menu_item_tv_show");
			}
			_tvNoise.Show();
		}
	}

	public void SetPreviewMedal(int medalIndex)
	{
		if (medalIndex != _medalIndex && medalIndex != _switchToMedalIndex)
		{
			_switchingMode = PanelInfoSwitchingModeEnum.Medal;
			_switchToMedalIndex = medalIndex;
			_switchToTVObject = GetMedalTVObject(medalIndex);
			if (medalIndex >= 0)
			{
				GameCore.PlaySound("menu_item_tv_show");
			}
			_tvNoise.Show();
		}
		if (medalIndex >= 0)
		{
			_levelData = null;
		}
	}

	public void CancelSelection()
	{
		SetTVItem(null);
		UnselectAllButtons();
	}

	public void DrawTV()
	{
		Vector2 vector = new Vector2(1920f, 1080f) * GameCore.VirtualAspectRatio;
		Rectangle rectangle = new Rectangle((int)((float)(GameCore.GraphicsDeviceStatic.Viewport.Width / 2) - vector.X / 2f), (int)((float)(GameCore.GraphicsDeviceStatic.Viewport.Height / 2) - vector.Y / 2f), (int)(vector.X + 2f), (int)(vector.Y + 2f));
		rectangle.X += (int)(TvTransformation.WorldPosition.X * GameCore.VirtualAspectRatio.X);
		rectangle.Y += (int)(TvTransformation.WorldPosition.Y * GameCore.VirtualAspectRatio.Y);
		rectangle.Width = (int)(265f * GameCore.VirtualAspectRatio.X);
		rectangle.Height = (int)(200f * GameCore.VirtualAspectRatio.Y);
		if (rectangle.Bottom > GameCore.ScissorRectangle.Bottom)
		{
			rectangle.Height = GameCore.ScissorRectangle.Bottom - rectangle.Y;
			if (rectangle.Height <= 0)
			{
				rectangle.Height = 0;
			}
		}
		_shopMenu.SpriteBatchEnd(bLight: true);
		GameCore.GraphicsDeviceStatic.ScissorRectangle = rectangle;
		GameCore.TriangleBatch.Begin(BlendState.AlphaBlend, Matrix.Identity);
		GameCore.TriangleBatch.DrawRect(rectangle, Color.Black);
		GameCore.TriangleBatch.End();
		Transformation transformation;
		if (_levelData != null)
		{
			_shopMenu.SpriteBatchBegin(bLight: true);
			if (_levelData.PreviewNumber < 15)
			{
				_atlasLevelPreview.Texture = _textureLevelPreview1;
			}
			else
			{
				_atlasLevelPreview.Texture = _textureLevelPreview2;
			}
			transformation = Transformation.CreateByPosition(TV_CENTER);
			transformation.ParentTransformation = TvTransformation;
			_atlasLevelPreview.DrawFrame(_shopMenu.SpriteBatchLight, GetPlanetPreviewFrame(_levelData.PreviewNumber), Color.White, transformation.WorldTransformation, BlendModeEnum.Normal);
		}
		else
		{
			if (TVItem != null || _medalIndex >= 0)
			{
				_shopMenu.GUILevel.SetCurrentLayer(_shopMenu.LayerTV);
				_shopMenu.LayerTV.Draw();
				GameCore.CurrentLevel.CurrentLayer = null;
				_shopMenu.GUILevel.SetCurrentLayer(_shopMenu.LayerShop);
				_shopMenu.SpriteBatchBegin(bLight: true);
			}
			else
			{
				_shopMenu.SpriteBatchBegin(bLight: true);
				_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("shop_tv_no_data"), TvTransformation.WorldPosition.X + 140f, TvTransformation.WorldPosition.Y + 85f, ShopMenu.FONT_SCALE_TV_SMALL, new Color(110, 110, 90, 200), TextAlign.Center);
			}
			if (TVItem != null && TVItem.IsUpgradable && TVItem.Place != ItemPlaceEnum.InShop)
			{
				transformation = new Transformation(TvTransformation.WorldTransformation);
				transformation.Position.X += 240f;
				transformation.Position.Y += 180f;
				_atlasItemLevel.DrawFrame(_shopMenu.SpriteBatchLight, TVItem.Level, Color.White, transformation, BlendModeEnum.Normal);
				_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, string.Format("{0}:", TextBundle.GetText("shop_item_level")), transformation.WorldPosition.X - 12f, transformation.WorldPosition.Y - 19f, new Vector2(0.5f), Color.White, TextAlign.Right);
			}
		}
		_tvNoise.Draw();
		_shopMenu.AtlasTV.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color.White, TvTransformation.WorldTransformation, BlendModeEnum.Normal);
		transformation = new Transformation(TvTransformation.WorldTransformation);
		transformation.Position.X += 281f;
		transformation.Position.Y += 81f;
		_atlasTVDiode.DrawFrame(_shopMenu.SpriteBatchLight, 0, new Color(1f, 1f, 1f, _tvDiodeAlphaRed), transformation, BlendModeEnum.Normal);
		_atlasTVDiodeFlare.DrawFrame(_shopMenu.SpriteBatchLight, 0, new Color(1f, 0.2f, 0.2f, _tvDiodeAlphaRed * 0.5f), transformation, BlendModeEnum.Additive);
		if (TVItem != null)
		{
			transformation.Position.Y += 14f;
			_atlasTVDiode.DrawFrame(_shopMenu.SpriteBatchLight, 1, new Color(1f, 1f, 1f, _tvDiodeAlphaGreen), transformation, BlendModeEnum.Normal);
			_atlasTVDiodeFlare.DrawFrame(_shopMenu.SpriteBatchLight, 1, new Color(0.2f, 1f, 0.2f, _tvDiodeAlphaGreen * 0.3f), transformation, BlendModeEnum.Additive);
			transformation.Position.Y += 14f;
			_atlasTVDiode.DrawFrame(_shopMenu.SpriteBatchLight, 2, new Color(1f, 1f, 1f, _tvDiodeAlphaBlue), transformation, BlendModeEnum.Normal);
			_atlasTVDiodeFlare.DrawFrame(_shopMenu.SpriteBatchLight, 2, new Color(0.2f, 0.2f, 1f, _tvDiodeAlphaBlue * 0.5f), transformation, BlendModeEnum.Additive);
			transformation = new Transformation(TvTransformation.WorldTransformation);
			transformation.Position.X += 132f;
			transformation.Position.Y += 205f;
			_spriteRendererTVRedDiode.Draw(transformation, _shopMenu.SpriteBatchLight, BlendModeEnum.Normal);
		}
	}

	public void DrawLevelInfo()
	{
		LevelData levelData = _switchToLevelData ?? _levelData;
		if (levelData != null)
		{
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("shop_mission_briefing_caption") + ":", Position.X + 272f, Position.Y + 32f, ShopMenu.FONT_SCALE_ITEM_TYPE, new Color(255, 255, 255), TextAlign.Left, 1000);
			_shopMenu.FontDefault.WrapWrite(_shopMenu.SpriteBatchLight, TextBundle.GetText($"level_desc_{levelData.TextId ?? levelData.Id}"), Position.X + 292f, Position.Y + 68f, 870, ShopMenu.FONT_SCALE_PARAMS, null, 1f, 25);
		}
	}

	public void DrawParamBox(HeroItem item)
	{
		Transformation transformation = new Transformation();
		_shopMenu.SpriteBatchBegin(bLight: true);
		switch (item.Place)
		{
		case ItemPlaceEnum.InCargo:
			transformation.Position = new Vector2(345f, _shopMenu.PanelCargo.GetCursorSlotPosition().Y - 60f);
			break;
		case ItemPlaceEnum.InShop:
			transformation.Position = new Vector2(1260f, _shopMenu.PanelShop.GetCursorSlotPosition().Y - 60f);
			break;
		case ItemPlaceEnum.InHeroSlot:
			if (_shopMenu.BigShipSlotSet.CursorSlot == null)
			{
				_shopMenu.SpriteBatchEnd(bLight: true);
				return;
			}
			transformation.Position = _shopMenu.BigShipSlotSet.GetCursorSlotPosition() + new Vector2(-100f, -350f);
			break;
		default:
			transformation.Position = new Vector2(1260f, 50f);
			break;
		}
		if (transformation.Position.Y < 50f)
		{
			transformation.Position.Y = 50f;
		}
		else if (transformation.Position.Y > 410f)
		{
			transformation.Position.Y = 410f;
		}
		_atlasParamBox.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color.White, transformation, BlendModeEnum.Normal);
		if (item != null)
		{
			DrawItemParams(item, transformation.Position + new Vector2(0f, 0f));
		}
		Transformation transformation2 = Transformation.CreateByPosition(147f, 280f);
		transformation2.ParentTransformation = transformation;
		DrawSmallTV(item, transformation2);
		_shopMenu.SpriteBatchEnd(bLight: true);
	}

	private void DrawSmallTV(HeroItem item, Transformation trans)
	{
		_atlasPriceTV.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color.White, trans.WorldTransformation, BlendModeEnum.Normal);
		if (item != null)
		{
			string arg = ((item.Price <= GameJnG.Cash || item.Place != ItemPlaceEnum.InShop) ? "\\1" : "\\9");
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, string.Format("\0{0}: {1}✐ {2}", TextBundle.GetText("shop_label_price"), arg, Utils.FormatMoney(item.Price)), trans.WorldPosition.X, trans.WorldPosition.Y - 13f, ShopMenu.FONT_SCALE_PRICE_TV, null, TextAlign.Center);
		}
		else if (_levelData != null)
		{
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, $"\\1{TextBundle.GetText($"level_name_{_levelData.TextId ?? _levelData.Id}")}", trans.WorldPosition.X, trans.WorldPosition.Y - 13f, ShopMenu.FONT_SCALE_PRICE_TV, null, TextAlign.Center);
		}
	}

	public void DrawHintBackground()
	{
		_shopMenu.SpriteBatchBegin(bLight: true);
		Transformation transformation = new Transformation(this);
		transformation.Position.X -= 12f;
		transformation.Position.Y += 280f;
		_atlasBackground.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color.White, transformation.WorldTransformation, BlendModeEnum.Normal);
		_shopMenu.SpriteBatchEnd(bLight: true);
	}

	private float DrawItemParamValue(Vector2 pos, HeroItem item, string paramName)
	{
		string paramText = _shopMenu.GetParamText(item, paramName);
		if (!item.TryGetBaseParamSummary(paramName, out var value, out var finalValue))
		{
			return 0f;
		}
		int num = _shopMenu.FontDefault.WrapWrite(_shopMenu.SpriteBatchLight, paramText, pos.X, pos.Y, 280, ShopMenu.FONT_SCALE_PARAMS, null, 1f, 25);
		int num2 = -1;
		HeroItem heroItem = null;
		heroItem = ((item != _shopMenu.SelectedItem) ? _shopMenu.SelectedItem : _shopMenu.CursorItem);
		if (heroItem != null && heroItem != item && heroItem.TryGetBaseParamSummary(paramName, out var value2, out finalValue))
		{
			if (value > value2)
			{
				num2 = (HeroItem.IsParamLessBetter(paramName) ? 1 : 0);
			}
			else if (value < value2)
			{
				num2 = ((!HeroItem.IsParamLessBetter(paramName)) ? 1 : 0);
			}
			if (num2 >= 0)
			{
				Transformation transform = Transformation.CreateByPosition(pos + new Vector2(-16f, 9f));
				_atlasParamArrow.DrawFrame(_shopMenu.SpriteBatchLight, num2, new Color(1f, 1f, 1f, _shopMenu.ShowHidePercentage), transform, BlendModeEnum.Normal);
			}
		}
		return num * 25;
	}

	private void DrawItemParams(HeroItem item, Vector2 pos)
	{
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, item.GetShopName(), pos.X - 2f, pos.Y + 1f, ShopMenu.FONT_SCALE_ITEM_NAME);
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, "\\3" + TextBundle.GetText("item_type_" + item.Data.ItemType), pos.X - 2f, pos.Y + 33f, ShopMenu.FONT_SCALE_ITEM_TYPE);
		int frameNumber = -1;
		if (item.IsTypeWeapon)
		{
			frameNumber = 0;
		}
		else if (item.IsTypeMissile)
		{
			frameNumber = 2;
		}
		else if (item.IsTypeBomb)
		{
			frameNumber = 3;
		}
		else if (item.IsTypeDevice)
		{
			frameNumber = 4;
		}
		else if (item.IsTypeEquipment)
		{
			frameNumber = 5;
		}
		else if (item.IsTypeGem)
		{
			frameNumber = 6;
		}
		_shopMenu.AtlasSlotTypeIcon.DrawFrame(_shopMenu.SpriteBatchLight, frameNumber, Color.White, Transformation.CreateByPosition(pos.X + 270f, pos.Y + 23f));
		pos.X += 20f;
		pos.Y += 68f;
		string key = $"quickinfo_{item.Data.TextId}";
		if (TextBundle.KeyExists(key))
		{
			pos.Y += 25 * _shopMenu.FontDefault.WrapWrite(_shopMenu.SpriteBatchLight, TextBundle.GetText(key), pos.X, pos.Y, 283, ShopMenu.FONT_SCALE_PARAMS, null, 1f, 25);
			pos.Y += 25f;
		}
		if (item.IsTypeWeapon)
		{
			float value = 0f;
			float finalValue = 0f;
			if (item.TryGetBaseParamSummary("stream_count", out value, out finalValue) && value > 1f)
			{
				_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, $"\\0✦ \\1{finalValue}", pos.X + 252f, pos.Y - 35f, ShopMenu.FONT_SCALE_PARAMS);
			}
			if (item.Data.ItemType != ItemTypeEnum.Rammer)
			{
				pos.Y += DrawItemParamValue(pos, item, "dps");
				pos.Y += DrawItemParamValue(pos, item, "fire_rate");
				pos.Y += DrawItemParamValue(pos, item, "heat_per_sec");
			}
			if (item.GetBehaviorByType(typeof(GemableItemBehavior)) is GemableItemBehavior gemableItemBehavior)
			{
				if (gemableItemBehavior.GetFinalChanceToBurn() > 0)
				{
					pos.Y += DrawItemParamValue(pos, item, "chance_to_burn");
				}
				if (gemableItemBehavior.GetFinalChanceToAcid() > 0)
				{
					pos.Y += DrawItemParamValue(pos, item, "chance_to_acid");
				}
			}
		}
		else if (item.IsTypeMissile)
		{
			pos.Y += DrawItemParamValue(pos, item, "dps");
			pos.Y += DrawItemParamValue(pos, item, "fire_rate");
		}
		else if (item.Data.ItemType == ItemTypeEnum.Bomb)
		{
			pos.Y += DrawItemParamValue(pos, item, "bomb_damage");
			pos.Y += DrawItemParamValue(pos, item, "dps");
			pos.Y += DrawItemParamValue(pos, item, "batch_size");
			pos.Y += DrawItemParamValue(pos, item, "reload_delay");
		}
		foreach (KeyValuePair<string, HeroItemDataParam> item2 in item.Data.Params)
		{
			pos.Y += DrawItemParamValue(pos, item, item2.Key);
		}
		if (item.IsBlockedBySameItem)
		{
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, string.Format("{0}", TextBundle.GetText("item_duplicity")), pos.X, pos.Y, ShopMenu.FONT_SCALE_PARAMS);
		}
	}

	private void DrawItemButtons()
	{
		_buttonSell.IsVisible = false;
		_buttonBuy.IsVisible = false;
		if (TVItem == null)
		{
			return;
		}
		if (TVItem.Place == ItemPlaceEnum.InShop)
		{
			_buttonBuy.IsVisible = true;
			_buttonBuy.Draw();
			return;
		}
		_buttonSell.IsVisible = true;
		_buttonSell.Draw();
		if (TVItem.IsUpgradable)
		{
			_buttonUpgrade.Draw();
			_buttonDowngrade.Draw();
		}
		else if (_buttonAction.IsVisible)
		{
			_buttonAction.Draw();
		}
	}

	public override void Draw()
	{
		if (State == ShopMenuState.Hidden)
		{
			return;
		}
		_shopMenu.SpriteBatchBegin(bLight: true);
		_atlasFrame.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color.White, this, BlendModeEnum.Normal);
		HeroItem heroItem = ((_shopMenu.CurrentScreen != ShopScreenEnum.Mission) ? _shopMenu.SelectedItem : null);
		Transformation transformation;
		if (_shopMenu.CurrentScreen != ShopScreenEnum.Mission)
		{
			if (_medalIndex >= 0)
			{
				_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText($"name_medal/medal_{_medalIndex}"), Position.X + 272f, Position.Y + 10f, ShopMenu.FONT_SCALE_ITEM_NAME, new Color(255, 240, 20), TextAlign.Left, 1000);
				_shopMenu.FontDefault.WrapWrite(_shopMenu.SpriteBatchLight, TextBundle.GetText($"desc_medal/medal_{_medalIndex}"), Position.X + 292f, Position.Y + 68f, 870, ShopMenu.FONT_SCALE_PARAMS, ShopMenu.FONT_COLOR_LABEL, 1f, 25);
			}
			else
			{
				if (heroItem != null)
				{
					DrawItemParams(heroItem, WorldPosition + new Vector2(272f, 0f));
					if (heroItem.Data.ItemType == ItemTypeEnum.Medal)
					{
						_shopMenu.FontDefault.WrapWrite(_shopMenu.SpriteBatchLight, "\\2" + heroItem.GetShopDescription(), Position.X + 292f, Position.Y + 68f, 545, ShopMenu.FONT_SCALE_TEXT_NORMAL, null, _shopMenu.ShowHidePercentage, 25);
					}
					else
					{
						_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("shop_label_item_descripion") + ":", Position.X + 606f, Position.Y + 33f, ShopMenu.FONT_SCALE_ITEM_TYPE, null, TextAlign.Left, 0, _shopMenu.ShowHidePercentage);
						_shopMenu.FontDefault.WrapWrite(_shopMenu.SpriteBatchLight, "\\2" + heroItem.GetShopDescription(), Position.X + 615f, Position.Y + 68f, 545, ShopMenu.FONT_SCALE_TEXT_NORMAL, null, _shopMenu.ShowHidePercentage, 25);
					}
				}
				if (_shopMenu.CurrentScreen != ShopScreenEnum.Mia)
				{
					DrawItemButtons();
				}
			}
			if (_medalIndex < 0 && (heroItem == null || heroItem.Data.ItemType != ItemTypeEnum.Medal) && _switchingMode != PanelInfoSwitchingModeEnum.Medal)
			{
				transformation = Transformation.CreateByPosition(586f, 133f);
				transformation.ParentTransformation = this;
				_atlasFrame.DrawFrame(_shopMenu.SpriteBatchLight, 1, Color.White, transformation.WorldTransformation, BlendModeEnum.Normal);
			}
		}
		else
		{
			_buttonSell.IsVisible = false;
			_buttonBuy.IsVisible = false;
			_buttonUpgrade.IsVisible = false;
			_buttonDowngrade.IsVisible = false;
			_buttonAction.IsVisible = false;
			DrawLevelInfo();
			_buttonStartLevel.Draw();
		}
		transformation = Transformation.CreateByPosition(100f, 272f);
		transformation.ParentTransformation = this;
		DrawSmallTV(heroItem, transformation);
		DrawTV();
		_shopMenu.SpriteBatchEnd(bLight: true);
	}
}
