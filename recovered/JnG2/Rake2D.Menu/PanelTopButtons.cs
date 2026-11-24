using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class PanelTopButtons : ShopMenuPanel
{
	private const int BUTTON_OFFSET_X = 0;

	private const int BUTTON_OFFSET_Y = 21;

	private const int BUTTON_WIDTH = 190;

	private float _targetY;

	private float _positionY;

	private Button _buttonMission;

	private Button _buttonShop;

	private Button _buttonProfile;

	private Button _buttonOptions;

	private Button _buttonEnd;

	private List<Button> _buttonList;

	public int SelectedButtonIndex { get; private set; }

	public PanelTopButtons(ShopMenu shopMenu)
		: base(shopMenu)
	{
		_buttonList = new List<Button>();
		_buttonMission = NewButton(ShopButtonType.Tab);
		_buttonMission.CaptionScale = ShopMenu.FONT_SCALE_BUTTON_BIG;
		_buttonMission.OnClick += OnButtonMissionClick;
		_buttonShop = NewButton(ShopButtonType.Tab);
		_buttonShop.CaptionScale = ShopMenu.FONT_SCALE_BUTTON_BIG;
		_buttonShop.OnClick += OnButtonShopClick;
		_buttonProfile = NewButton(ShopButtonType.Tab);
		_buttonProfile.CaptionScale = ShopMenu.FONT_SCALE_BUTTON_BIG;
		_buttonProfile.OnClick += OnButtonProfileClick;
		_buttonOptions = NewButton(ShopButtonType.Normal);
		_buttonOptions.HasPressEffectEnabled = true;
		_buttonOptions.CaptionScale = ShopMenu.FONT_SCALE_BUTTON_BIG;
		_buttonOptions.OnClick += OnButtonOptionsClick;
		_buttonEnd = NewButton(ShopButtonType.Normal);
		_buttonEnd.HasPressEffectEnabled = true;
		_buttonEnd.CaptionScale = ShopMenu.FONT_SCALE_BUTTON_BIG;
		_buttonEnd.OnClick += OnButtonEndClick;
		_targetY = 0f;
		_positionY = -50f;
	}

	public void SetTexts()
	{
		_buttonEnd.Caption = TextBundle.GetText("button_end_campaign");
		_buttonOptions.Caption = TextBundle.GetText("button_options");
		_buttonProfile.Caption = TextBundle.GetText("button_profile");
		_buttonShop.Caption = TextBundle.GetText("button_shop");
		_buttonMission.Caption = TextBundle.GetText("button_mission");
	}

	private Button NewButton(ShopButtonType type)
	{
		Button button = new Button(_shopMenu, type);
		button.ParentTransformation = this;
		button.Position = new Vector2(_buttonList.Count * 190, 21f);
		_buttonList.Add(button);
		return button;
	}

	public void OnButtonMissionClick(Button button)
	{
		_shopMenu.SwitchScreen(ShopScreenEnum.Mission);
	}

	public void OnButtonShopClick(Button button)
	{
		_shopMenu.SwitchScreen(ShopScreenEnum.Shop);
	}

	public void OnButtonProfileClick(Button button)
	{
		_shopMenu.SwitchScreen(ShopScreenEnum.HeroProfile);
	}

	public void OnButtonOptionsClick(Button button)
	{
		GameJnG.PopupMenuOptions.Show();
		SelectButton(-1);
	}

	public void OnButtonEndClick(Button button)
	{
		_shopMenu.YesNoEndCampaign.Show();
		SelectButton(-1);
	}

	public void CancelSelection()
	{
	}

	public void SelectButton(int index, bool bPlaySound = true)
	{
		for (int i = 0; i < _buttonList.Count; i++)
		{
			Button button = _buttonList[i];
			if (index == i)
			{
				button.Select(bPlaySound);
			}
			else
			{
				button.Unselect();
			}
		}
	}

	public void SelectButtonByX(float posX)
	{
		bool flag = false;
		for (int i = 0; i < _buttonList.Count; i++)
		{
			Button button = _buttonList[i];
			if (!flag && button.WorldPosition.X >= posX)
			{
				button.Select();
				flag = true;
			}
			else
			{
				button.Unselect();
			}
		}
		if (!flag)
		{
			_buttonList[_buttonList.Count - 1].Select();
		}
	}

	public override void Show(float showDelay)
	{
		base.Show(showDelay);
		_targetY = 0f;
		SelectedButtonIndex = -1;
		_buttonOptions.CancelPressEffect();
		_buttonEnd.CancelPressEffect();
	}

	public override void Hide()
	{
		base.Hide();
		_targetY = -60f;
	}

	private void ParseInput()
	{
		if (_shopMenu.IsAnyPopupMenuVisible)
		{
			return;
		}
		if (SelectedButtonIndex < 0)
		{
			if (_shopMenu.KeyPressed(ShopMenuKeys.PreviousTab) || _shopMenu.KeyPressed(ShopMenuKeys.NextTab))
			{
				_shopMenu.CancelCursorSelection();
				_shopMenu.PanelCargo.UnselectSortButton();
				_shopMenu.ButtonTestRange.Unselect();
				_shopMenu.ShopBackground.CancelSelection();
				_shopMenu.ScreenMission.SetMouseSelectedLevelData(null);
				if (_shopMenu.CurrentScreen == ShopScreenEnum.Mission)
				{
					SelectButton(0);
				}
				else if (_shopMenu.CurrentScreen == ShopScreenEnum.Shop)
				{
					SelectButton(1);
				}
				else
				{
					SelectButton(2);
				}
			}
			return;
		}
		if (GameInput.SystemGPButtonPressed(Buttons.Start))
		{
			SelectButton(0);
		}
		else if (GameInput.SystemGPButtonPressed(Buttons.Back))
		{
			SelectButton(4);
		}
		if (_shopMenu.KeyPressed(ShopMenuKeys.MoveDownWithControl))
		{
			if (_shopMenu.CurrentScreen != ShopScreenEnum.Mission)
			{
				SelectButton(-1);
				_shopMenu.BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.front_gun);
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeftWithControl))
		{
			if (_shopMenu.CurrentScreen == ShopScreenEnum.Shop)
			{
				SelectButton(-1);
				_shopMenu.PanelCargo.SetVisibleCursorSlot(0, 2);
			}
			else if (_shopMenu.CurrentScreen == ShopScreenEnum.HeroProfile)
			{
				SelectButton(-1);
				_shopMenu.PanelHeroStats.SelectFaceTV();
			}
			else
			{
				SelectButton(0);
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRightWithControl))
		{
			if (_shopMenu.CurrentScreen == ShopScreenEnum.Shop)
			{
				SelectButton(-1);
				_shopMenu.PanelShop.SetCursorSlotByY(0f);
			}
			else if (_shopMenu.CurrentScreen == ShopScreenEnum.HeroProfile)
			{
				_shopMenu.PanelShipParams.SelectSlider(0);
			}
			else
			{
				SelectButton(4);
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRight))
		{
			if (SelectedButtonIndex >= _buttonList.Count - 1)
			{
				if (_shopMenu.CurrentScreen == ShopScreenEnum.Shop)
				{
					SelectButton(-1);
					_shopMenu.PanelShop.SetCursorSlotByY(100f);
				}
				else if (_shopMenu.CurrentScreen == ShopScreenEnum.HeroProfile)
				{
					SelectButton(-1);
					_shopMenu.PanelShipParams.SelectSlider(0);
				}
				else
				{
					SelectButton(0);
				}
			}
			else if (SelectedButtonIndex >= 0)
			{
				SelectButton(SelectedButtonIndex + 1);
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeft))
		{
			if (SelectedButtonIndex == 0)
			{
				if (_shopMenu.CurrentScreen == ShopScreenEnum.Shop)
				{
					SelectButton(-1);
					_shopMenu.PanelCargo.SelectSortButton();
				}
				else if (_shopMenu.CurrentScreen == ShopScreenEnum.HeroProfile)
				{
					SelectButton(-1);
					_shopMenu.PanelHeroStats.SelectFaceTV();
				}
				else
				{
					SelectButton(4);
				}
			}
			else if (SelectedButtonIndex >= 0)
			{
				SelectButton(SelectedButtonIndex - 1);
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.NextTab))
		{
			if (SelectedButtonIndex >= _buttonList.Count - 1)
			{
				SelectButton(0);
			}
			else
			{
				SelectButton(SelectedButtonIndex + 1);
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.PreviousTab))
		{
			if (SelectedButtonIndex == 0)
			{
				SelectButton(4);
			}
			else
			{
				SelectButton(SelectedButtonIndex - 1);
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveDown))
		{
			SelectButton(-1);
			if (_shopMenu.CurrentScreen == ShopScreenEnum.Shop && SelectedButtonIndex == 0)
			{
				_shopMenu.PanelCargo.SelectSortButton();
			}
			else if (_shopMenu.CurrentScreen != ShopScreenEnum.Mission)
			{
				if (SelectedButtonIndex == 0)
				{
					_shopMenu.BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.rear_gun);
				}
				else if (SelectedButtonIndex == 1)
				{
					_shopMenu.BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.missile);
				}
				else if (SelectedButtonIndex == 2)
				{
					_shopMenu.BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.top_gun);
				}
				else if (SelectedButtonIndex == 4)
				{
					_shopMenu.ShopBackground.SelectGateTV();
				}
				else if (SelectedButtonIndex > 2)
				{
					_shopMenu.SelectTestRangeButton();
				}
			}
			else
			{
				_shopMenu.ScreenMission.SetMouseSelectedLevelData(_shopMenu.ScreenMission.CurrentLevelData);
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp) && _shopMenu.PanelInfo.SelectDefaultButton())
		{
			SelectButton(-1, bPlaySound: false);
		}
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (Utils.ChangeValueToTarget(ref _positionY, _targetY, 400f * elapsedTime))
		{
			if (State == ShopMenuState.Showing)
			{
				State = ShopMenuState.Visible;
			}
			else if (State == ShopMenuState.Hiding)
			{
				State = ShopMenuState.Hidden;
				return;
			}
		}
		Position = new Vector2(580f, _positionY);
		SelectedButtonIndex = -1;
		for (int i = 0; i < _buttonList.Count; i++)
		{
			Button button = _buttonList[i];
			button.IsInputEnabled = !_shopMenu.IsAnyPopupMenuVisible && !_shopMenu.ScreenMission.IsDraggingPlanets;
			button.Update(elapsedTime);
			if (button.IsSelected)
			{
				SelectedButtonIndex = i;
				_shopMenu.ChangeFocus(this);
			}
		}
		ParseInput();
		if (_shopMenu.CurrentScreen == ShopScreenEnum.Mission)
		{
			_buttonMission.IsPressed = true;
			_buttonShop.IsPressed = false;
			_buttonProfile.IsPressed = false;
		}
		else if (_shopMenu.CurrentScreen == ShopScreenEnum.Shop)
		{
			_buttonMission.IsPressed = false;
			_buttonShop.IsPressed = true;
			_buttonProfile.IsPressed = false;
		}
		else if (_shopMenu.CurrentScreen == ShopScreenEnum.HeroProfile)
		{
			_buttonMission.IsPressed = false;
			_buttonShop.IsPressed = false;
			_buttonProfile.IsPressed = true;
		}
		if (SelectedButtonIndex >= 0)
		{
			_shopMenu.SetHintText($"hint_top_button_{SelectedButtonIndex + 1}");
			_shopMenu.CursorType = CursorTypeEnum.Select;
		}
	}

	public override void Draw()
	{
		_shopMenu.SpriteBatchBegin(bLight: true);
		foreach (Button button in _buttonList)
		{
			button.Draw();
		}
		_shopMenu.SpriteBatchEnd(bLight: true);
	}
}
