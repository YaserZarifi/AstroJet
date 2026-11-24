using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Rake2D.Menu;

public class OptionsTabControls : Transformation
{
	public SwitchButton WaitingToButton;

	public SwitchButton[][] ControlsButtonArray;

	public SwitchButton CurrentControllerButton;

	public HorizontalSlider SliderMouseSensitivity;

	private GameInput.ControllerTypeEnum _currentController = GameInput.ControllerTypeEnum.Keyboard;

	private SwitchButton _selectedControlsButton;

	private ShopMenu _shopMenu;

	private PopupMenuOptions _popupMenu;

	public OptionsTabControls(ShopMenu shopMenu, PopupMenuOptions popupMenu)
	{
		_shopMenu = shopMenu;
		_popupMenu = popupMenu;
		ParentTransformation = _popupMenu;
		CurrentControllerButton = new SwitchButton(_shopMenu);
		CurrentControllerButton.Type = ControlsButtonTypeEnum.Big;
		CurrentControllerButton.LabelOffset.X = -299f;
		CurrentControllerButton.Position = Position + new Vector2(91f, -160f);
		CurrentControllerButton.ParentTransformation = this;
		CurrentControllerButton.OnClick += OnCurrentControllerButton;
		CurrentControllerButton.IsClickable = true;
		SliderMouseSensitivity = new HorizontalSlider(_shopMenu);
		SliderMouseSensitivity.MaxValue = 100;
		SliderMouseSensitivity.LabelValueMode = SliderLabeValueModeEnum.Percentage;
		SliderMouseSensitivity.LabelValueOffset = 0;
		SliderMouseSensitivity.LabelOffset = new Vector2(-321f, -13f);
		SliderMouseSensitivity.ParentTransformation = this;
		SliderMouseSensitivity.Position = new Vector2(113f, 140f);
		SliderMouseSensitivity.OnSetValue += OnSliderMouseSensitivityValue;
		ControlsButtonArray = new SwitchButton[6][];
		for (int i = 0; i < 6; i++)
		{
			ControlsButtonArray[i] = new SwitchButton[8];
			for (int j = 0; j < 8; j++)
			{
				ControlsButtonArray[i][j] = new SwitchButton(_shopMenu);
				SwitchButton obj = ControlsButtonArray[i][j];
				obj.ControlsGroup = (ControlsGroupEnum)i;
				obj.KeyType = (GameInput.KeyTypeEnum)j;
				obj.Position = Position + new Vector2(-23 + i % 2 * 160, -56 + j * 41);
				obj.ParentTransformation = this;
				obj.OnClick += OnControlsRedefineButonClick;
			}
		}
		SetButtonValues();
	}

	public bool IsAnyItemSelected()
	{
		for (int i = 0; i < 6; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				if (ControlsButtonArray[i][j].IsSelected)
				{
					return true;
				}
			}
		}
		if (SliderMouseSensitivity.IsActivated || SliderMouseSensitivity.IsDragged || SliderMouseSensitivity.IsSelected)
		{
			return true;
		}
		if (CurrentControllerButton.IsSelected)
		{
			return true;
		}
		return false;
	}

	private void OnSliderMouseSensitivityValue(HorizontalSlider slider)
	{
		Options.MouseSensitivity = slider.Value;
	}

	private void OnCurrentControllerButton(SwitchButton button)
	{
		switch (_currentController)
		{
		case GameInput.ControllerTypeEnum.GamePad:
			_currentController = GameInput.ControllerTypeEnum.Mouse;
			break;
		case GameInput.ControllerTypeEnum.Mouse:
			_currentController = GameInput.ControllerTypeEnum.Keyboard;
			break;
		case GameInput.ControllerTypeEnum.Keyboard:
			_currentController = GameInput.ControllerTypeEnum.GamePad;
			break;
		}
		SetCurrentControllerButtonCaption();
	}

	public void SetTexts()
	{
		CurrentControllerButton.Label = TextBundle.GetText("options_current_controller");
		SliderMouseSensitivity.Label = TextBundle.GetText("options_mouse_sensitivity");
		for (int i = 0; i < 6; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				SwitchButton switchButton = ControlsButtonArray[i][j];
				string label = null;
				switch (switchButton.KeyType)
				{
				case GameInput.KeyTypeEnum.MoveUp:
					label = TextBundle.GetText("controls_up");
					break;
				case GameInput.KeyTypeEnum.MoveDown:
					label = TextBundle.GetText("controls_down");
					break;
				case GameInput.KeyTypeEnum.MoveLeft:
					label = TextBundle.GetText("controls_left");
					break;
				case GameInput.KeyTypeEnum.MoveRight:
					label = TextBundle.GetText("controls_right");
					break;
				case GameInput.KeyTypeEnum.FireGuns:
					label = TextBundle.GetText("controls_fire");
					break;
				case GameInput.KeyTypeEnum.FireBombs:
					label = TextBundle.GetText("controls_bomb");
					break;
				case GameInput.KeyTypeEnum.PrimaryDevice:
					label = TextBundle.GetText("controls_device_1");
					break;
				case GameInput.KeyTypeEnum.SecondaryDevice:
					label = TextBundle.GetText("controls_device_2");
					break;
				}
				if (i % 2 == 0)
				{
					switchButton.Label = label;
				}
			}
			SetCurrentControllerButtonCaption();
		}
	}

	private void SetCurrentControllerButtonCaption()
	{
		switch (_currentController)
		{
		case GameInput.ControllerTypeEnum.Mouse:
			CurrentControllerButton.Text = TextBundle.GetText("options_controller_mouse");
			break;
		case GameInput.ControllerTypeEnum.Keyboard:
			CurrentControllerButton.Text = TextBundle.GetText("options_controller_keyboard");
			break;
		case GameInput.ControllerTypeEnum.GamePad:
			CurrentControllerButton.Text = TextBundle.GetText("options_controller_gamepad");
			break;
		}
	}

	private void SetMouseButtonName(SwitchButton controlsBtn, int playerNumber)
	{
		GameInput.MouseButton? mouseButtonCode = GameInput.GetMouseButtonCode(controlsBtn.KeyType, playerNumber);
		Color white = Color.White;
		if (!mouseButtonCode.HasValue)
		{
			controlsBtn.Text = "";
		}
		else
		{
			switch (mouseButtonCode)
			{
			case GameInput.MouseButton.Left:
				controlsBtn.Text = "✑";
				break;
			case GameInput.MouseButton.Right:
				controlsBtn.Text = "✒";
				break;
			case GameInput.MouseButton.Middle:
				controlsBtn.Text = "✓";
				break;
			case GameInput.MouseButton.X1:
				controlsBtn.Text = "✔";
				break;
			case GameInput.MouseButton.X2:
				controlsBtn.Text = "✕";
				break;
			}
		}
		controlsBtn.TextColor = white;
	}

	private void SetGamePadButtonName(SwitchButton controlsBtn, int playerNumber)
	{
		Buttons gamePadButtonCode = GameInput.GetGamePadButtonCode(controlsBtn.KeyType, playerNumber);
		Color textColor = Color.White;
		if (gamePadButtonCode == 0)
		{
			controlsBtn.Text = "";
		}
		else
		{
			switch (gamePadButtonCode)
			{
			case Buttons.X:
				controlsBtn.Text = "✖";
				break;
			case Buttons.Y:
				controlsBtn.Text = "✗";
				break;
			case Buttons.A:
				controlsBtn.Text = "✘";
				break;
			case Buttons.B:
				controlsBtn.Text = "✙";
				break;
			case Buttons.DPadUp:
				controlsBtn.Text = "✚";
				break;
			case Buttons.DPadRight:
				controlsBtn.Text = "✛";
				break;
			case Buttons.DPadDown:
				controlsBtn.Text = "✜";
				break;
			case Buttons.DPadLeft:
				controlsBtn.Text = "✝";
				break;
			case Buttons.LeftShoulder:
				controlsBtn.Text = "✞";
				break;
			case Buttons.RightShoulder:
				controlsBtn.Text = "✟";
				break;
			case Buttons.LeftTrigger:
				controlsBtn.Text = "✠";
				break;
			case Buttons.RightTrigger:
				controlsBtn.Text = "✡";
				break;
			case (Buttons)0:
				controlsBtn.Text = "";
				break;
			default:
				controlsBtn.Text = GameInput.GetGamePadButtonName(controlsBtn.KeyType, playerNumber);
				textColor = ShopMenu.FONT_COLOR_PARAM_VALUE;
				break;
			}
		}
		controlsBtn.TextColor = textColor;
	}

	private void SetButtonValues()
	{
		for (int i = 0; i < 6; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				SwitchButton switchButton = ControlsButtonArray[i][j];
				switch ((ControlsGroupEnum)i)
				{
				case ControlsGroupEnum.Keyboard_1:
					switchButton.Text = GameInput.GetKeyName(switchButton.KeyType, 0) ?? "";
					break;
				case ControlsGroupEnum.Keyboard_2:
					switchButton.Text = GameInput.GetKeyName(switchButton.KeyType, 1) ?? "";
					break;
				case ControlsGroupEnum.Gamepad_1:
					SetGamePadButtonName(switchButton, 0);
					break;
				case ControlsGroupEnum.Gamepad_2:
					SetGamePadButtonName(switchButton, 1);
					break;
				case ControlsGroupEnum.Mouse_1:
					SetMouseButtonName(switchButton, 0);
					break;
				case ControlsGroupEnum.Mouse_2:
					SetMouseButtonName(switchButton, 1);
					break;
				default:
					switchButton.Text = "";
					break;
				}
			}
		}
	}

	private void OnControlsRedefineButonClick(SwitchButton button)
	{
		WaitingToButton = button;
		GameCore.PlaySound("menu_yesno_show");
		_shopMenu.ShopInput.CancelKeyPressed();
	}

	public void ResetCurrentControls()
	{
		switch (_currentController)
		{
		case GameInput.ControllerTypeEnum.GamePad:
			GameInput.SetDefaultGamePadButtonCodes();
			break;
		case GameInput.ControllerTypeEnum.Keyboard:
			GameInput.SetDefaultKeyCodes();
			break;
		case GameInput.ControllerTypeEnum.Mouse:
			GameInput.SetDefaultMouseButtonCodes();
			Options.MouseSensitivity = 50;
			SliderMouseSensitivity.SetValue(Options.MouseSensitivity, bCallEvents: false);
			break;
		}
		SetButtonValues();
	}

	public void Show()
	{
		WaitingToButton = null;
		DeactivateControlsButtons();
		SliderMouseSensitivity.SetValue(Options.MouseSensitivity, bCallEvents: false);
	}

	private void DeactivateControlsButtons()
	{
		for (int i = 0; i < 6; i++)
		{
			SwitchButton[] array = ControlsButtonArray[i];
			for (int j = 0; j < array.Length; j++)
			{
				array[j].IsActivated = false;
			}
		}
	}

	public void SelectControlsButton(ControlsGroupEnum group, GameInput.KeyTypeEnum type)
	{
		for (int i = 0; i < 6; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				SwitchButton switchButton = ControlsButtonArray[i][j];
				if (i == (int)group && j == (int)type)
				{
					switchButton.Select();
				}
				else
				{
					switchButton.Unselect();
				}
			}
		}
	}

	public void UnselectAll()
	{
		for (int i = 0; i < 2; i++)
		{
			SwitchButton[] array = ControlsButtonArray[i];
			for (int j = 0; j < array.Length; j++)
			{
				array[j].Unselect();
			}
		}
		CurrentControllerButton.Unselect();
		SliderMouseSensitivity.Unselect();
		SliderMouseSensitivity.SetDeactivated();
	}

	private void ParseInput()
	{
		if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp))
		{
			if (CurrentControllerButton.IsSelected)
			{
				CurrentControllerButton.Unselect();
				_popupMenu.TabButtonControls.Select();
			}
			else if (SliderMouseSensitivity.IsSelected && !SliderMouseSensitivity.IsActivated)
			{
				SliderMouseSensitivity.Unselect();
				SelectControlsButton(ControlsGroupEnum.Mouse_1, GameInput.KeyTypeEnum.SecondaryDevice);
			}
			else if (_selectedControlsButton != null)
			{
				if (_selectedControlsButton.KeyType > GameInput.KeyTypeEnum.FireGuns)
				{
					SelectControlsButton(_selectedControlsButton.ControlsGroup, _selectedControlsButton.KeyType - 1);
					return;
				}
				SelectControlsButton(ControlsGroupEnum.None, GameInput.KeyTypeEnum.None);
				CurrentControllerButton.Select();
			}
			else if (_popupMenu.ButtonReset.IsSelected)
			{
				_popupMenu.ButtonReset.Unselect();
				switch (_currentController)
				{
				case GameInput.ControllerTypeEnum.Keyboard:
					SelectControlsButton(ControlsGroupEnum.Keyboard_1, GameInput.KeyTypeEnum.MoveRight);
					break;
				case GameInput.ControllerTypeEnum.GamePad:
					SelectControlsButton(ControlsGroupEnum.Gamepad_1, GameInput.KeyTypeEnum.SecondaryDevice);
					break;
				case GameInput.ControllerTypeEnum.Mouse:
					SliderMouseSensitivity.Select();
					break;
				}
			}
			else if (_popupMenu.ButtonClose.IsSelected)
			{
				_popupMenu.ButtonClose.Unselect();
				switch (_currentController)
				{
				case GameInput.ControllerTypeEnum.Keyboard:
					SelectControlsButton(ControlsGroupEnum.Keyboard_2, GameInput.KeyTypeEnum.MoveRight);
					break;
				case GameInput.ControllerTypeEnum.GamePad:
					SelectControlsButton(ControlsGroupEnum.Gamepad_2, GameInput.KeyTypeEnum.SecondaryDevice);
					break;
				case GameInput.ControllerTypeEnum.Mouse:
					SliderMouseSensitivity.Select();
					break;
				}
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveDown))
		{
			if (SliderMouseSensitivity.IsSelected && !SliderMouseSensitivity.IsActivated)
			{
				SliderMouseSensitivity.Unselect();
				_popupMenu.ButtonClose.Select();
			}
			else if (CurrentControllerButton.IsSelected)
			{
				CurrentControllerButton.Unselect();
				switch (_currentController)
				{
				case GameInput.ControllerTypeEnum.Keyboard:
					SelectControlsButton(ControlsGroupEnum.Keyboard_1, GameInput.KeyTypeEnum.FireGuns);
					break;
				case GameInput.ControllerTypeEnum.GamePad:
					SelectControlsButton(ControlsGroupEnum.Gamepad_1, GameInput.KeyTypeEnum.FireGuns);
					break;
				case GameInput.ControllerTypeEnum.Mouse:
					SelectControlsButton(ControlsGroupEnum.Mouse_1, GameInput.KeyTypeEnum.FireGuns);
					break;
				}
			}
			else
			{
				if (_selectedControlsButton == null || _selectedControlsButton.KeyType < GameInput.KeyTypeEnum.FireGuns)
				{
					return;
				}
				if ((int)_selectedControlsButton.KeyType < ((_currentController == GameInput.ControllerTypeEnum.Keyboard) ? 7 : 3))
				{
					SelectControlsButton(_selectedControlsButton.ControlsGroup, _selectedControlsButton.KeyType + 1);
					return;
				}
				if (_selectedControlsButton.ControlsGroup == ControlsGroupEnum.Keyboard_1 || _selectedControlsButton.ControlsGroup == ControlsGroupEnum.Gamepad_1)
				{
					_popupMenu.ButtonReset.Select();
				}
				else if (_selectedControlsButton.ControlsGroup == ControlsGroupEnum.Mouse_1)
				{
					SliderMouseSensitivity.Select();
				}
				else
				{
					_popupMenu.ButtonReset.Unselect();
					_popupMenu.ButtonClose.Select();
				}
				SelectControlsButton(ControlsGroupEnum.None, GameInput.KeyTypeEnum.None);
			}
		}
		else
		{
			if (SliderMouseSensitivity.IsActivated)
			{
				return;
			}
			if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeft))
			{
				if (_selectedControlsButton != null && (_selectedControlsButton.ControlsGroup == ControlsGroupEnum.Keyboard_2 || _selectedControlsButton.ControlsGroup == ControlsGroupEnum.Gamepad_2))
				{
					SelectControlsButton(_selectedControlsButton.ControlsGroup - 1, _selectedControlsButton.KeyType);
				}
			}
			else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRight) && _selectedControlsButton != null && (_selectedControlsButton.ControlsGroup == ControlsGroupEnum.Keyboard_1 || _selectedControlsButton.ControlsGroup == ControlsGroupEnum.Gamepad_1))
			{
				SelectControlsButton(_selectedControlsButton.ControlsGroup + 1, _selectedControlsButton.KeyType);
			}
		}
	}

	private void UpdateWaitingToButton()
	{
		int playerNumber = 0;
		switch (WaitingToButton.ControlsGroup)
		{
		case ControlsGroupEnum.Keyboard_1:
		case ControlsGroupEnum.Keyboard_2:
		{
			if (WaitingToButton.ControlsGroup == ControlsGroupEnum.Keyboard_2)
			{
				playerNumber = 1;
			}
			Keys keys = GameInput.SystemGetPressedKey(bIgnoreControlKeys: false);
			if (keys != Keys.None)
			{
				GameInput.SetKeyCode(WaitingToButton.KeyType, keys, playerNumber);
				GameInput.RemoveKeyCodesDuplicity(WaitingToButton.KeyType, playerNumber);
				SetButtonValues();
				DeactivateControlsButtons();
				_shopMenu.ShopInput.CancelKeyPressed();
				WaitingToButton = null;
				GameCore.PlaySound("menu_option_set");
			}
			break;
		}
		case ControlsGroupEnum.Gamepad_1:
		case ControlsGroupEnum.Gamepad_2:
		{
			if (WaitingToButton.ControlsGroup == ControlsGroupEnum.Gamepad_2)
			{
				playerNumber = 1;
			}
			bool flag2 = true;
			if (GameInput.SystemGPButtonPressedAll(Buttons.A))
			{
				GameInput.SetGamePadButtonCode(WaitingToButton.KeyType, Buttons.A, playerNumber);
			}
			else if (GameInput.SystemGPButtonPressedAll(Buttons.B))
			{
				GameInput.SetGamePadButtonCode(WaitingToButton.KeyType, Buttons.B, playerNumber);
			}
			else if (GameInput.SystemGPButtonPressedAll(Buttons.X))
			{
				GameInput.SetGamePadButtonCode(WaitingToButton.KeyType, Buttons.X, playerNumber);
			}
			else if (GameInput.SystemGPButtonPressedAll(Buttons.Y))
			{
				GameInput.SetGamePadButtonCode(WaitingToButton.KeyType, Buttons.Y, playerNumber);
			}
			else if (GameInput.SystemGPButtonPressedAll(Buttons.BigButton))
			{
				GameInput.SetGamePadButtonCode(WaitingToButton.KeyType, Buttons.BigButton, playerNumber);
			}
			else if (GameInput.SystemGPButtonPressedAll(Buttons.DPadUp))
			{
				GameInput.SetGamePadButtonCode(WaitingToButton.KeyType, Buttons.DPadUp, playerNumber);
			}
			else if (GameInput.SystemGPButtonPressedAll(Buttons.DPadDown))
			{
				GameInput.SetGamePadButtonCode(WaitingToButton.KeyType, Buttons.DPadDown, playerNumber);
			}
			else if (GameInput.SystemGPButtonPressedAll(Buttons.DPadLeft))
			{
				GameInput.SetGamePadButtonCode(WaitingToButton.KeyType, Buttons.DPadLeft, playerNumber);
			}
			else if (GameInput.SystemGPButtonPressedAll(Buttons.DPadRight))
			{
				GameInput.SetGamePadButtonCode(WaitingToButton.KeyType, Buttons.DPadRight, playerNumber);
			}
			else if (GameInput.SystemGPButtonPressedAll(Buttons.LeftShoulder))
			{
				GameInput.SetGamePadButtonCode(WaitingToButton.KeyType, Buttons.LeftShoulder, playerNumber);
			}
			else if (GameInput.SystemGPButtonPressedAll(Buttons.RightShoulder))
			{
				GameInput.SetGamePadButtonCode(WaitingToButton.KeyType, Buttons.RightShoulder, playerNumber);
			}
			else if (GameInput.SystemGPButtonPressedAll(Buttons.LeftTrigger))
			{
				GameInput.SetGamePadButtonCode(WaitingToButton.KeyType, Buttons.LeftTrigger, playerNumber);
			}
			else if (GameInput.SystemGPButtonPressedAll(Buttons.RightTrigger))
			{
				GameInput.SetGamePadButtonCode(WaitingToButton.KeyType, Buttons.RightTrigger, playerNumber);
			}
			else
			{
				flag2 = false;
			}
			if (flag2)
			{
				GameInput.RemoveButtonCodesDuplicity(WaitingToButton.KeyType, playerNumber);
				SetButtonValues();
				DeactivateControlsButtons();
				_shopMenu.ShopInput.CancelKeyPressed();
				WaitingToButton = null;
				GameCore.PlaySound("menu_option_set");
			}
			break;
		}
		case ControlsGroupEnum.Mouse_1:
		case ControlsGroupEnum.Mouse_2:
		{
			if (WaitingToButton.ControlsGroup == ControlsGroupEnum.Mouse_2)
			{
				playerNumber = 1;
			}
			bool flag = true;
			if (GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				GameInput.SetMouseButtonCode(WaitingToButton.KeyType, GameInput.MouseButton.Left, playerNumber);
			}
			else if (GameInput.MousePressed(GameInput.MouseButton.Right))
			{
				GameInput.SetMouseButtonCode(WaitingToButton.KeyType, GameInput.MouseButton.Right, playerNumber);
			}
			else if (GameInput.MousePressed(GameInput.MouseButton.Middle))
			{
				GameInput.SetMouseButtonCode(WaitingToButton.KeyType, GameInput.MouseButton.Middle, playerNumber);
			}
			else if (GameInput.MousePressed(GameInput.MouseButton.X1))
			{
				GameInput.SetMouseButtonCode(WaitingToButton.KeyType, GameInput.MouseButton.X1, playerNumber);
			}
			else if (GameInput.MousePressed(GameInput.MouseButton.X2))
			{
				GameInput.SetMouseButtonCode(WaitingToButton.KeyType, GameInput.MouseButton.X2, playerNumber);
			}
			else
			{
				flag = false;
			}
			if (flag)
			{
				GameInput.RemoveMouseButtonCodesDuplicity(WaitingToButton.KeyType, playerNumber);
				SetButtonValues();
				DeactivateControlsButtons();
				_shopMenu.ShopInput.CancelKeyPressed();
				WaitingToButton = null;
				GameCore.PlaySound("menu_option_set");
			}
			break;
		}
		}
	}

	public void Update(float elapsedTime)
	{
		_selectedControlsButton = null;
		Color = _popupMenu.Color;
		if (WaitingToButton == null)
		{
			CurrentControllerButton.Update(elapsedTime);
			if (_currentController == GameInput.ControllerTypeEnum.Mouse)
			{
				SliderMouseSensitivity.Update(elapsedTime);
			}
			for (int i = 0; i < 2; i++)
			{
				SwitchButton[] array = ControlsButtonArray[(int)_currentController * 2 + i];
				for (int j = 0; j < ((_currentController == GameInput.ControllerTypeEnum.Keyboard) ? array.Length : 4); j++)
				{
					SwitchButton switchButton = array[j];
					switchButton.Update(elapsedTime);
					if (switchButton.IsSelected)
					{
						_selectedControlsButton = switchButton;
					}
				}
			}
			_popupMenu.ButtonClose.IsInputEnabled = true;
			ParseInput();
		}
		else
		{
			_popupMenu.ButtonClose.IsInputEnabled = false;
			if (_shopMenu.KeyPressed(ShopMenuKeys.CancelAll))
			{
				GameCore.PlaySound("menu_yesno_hide");
				WaitingToButton = null;
				_shopMenu.ShopInput.CancelKeyPressed();
				DeactivateControlsButtons();
			}
			else
			{
				UpdateWaitingToButton();
			}
		}
	}

	public void Draw()
	{
		float num = (float)(int)Color.A / 255f;
		float y = WorldPosition.Y - 109f;
		if (_currentController == GameInput.ControllerTypeEnum.Keyboard)
		{
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("options_keyboard_profile") + " 1", WorldPosition.X - 26f, y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Center, 0, num);
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("options_keyboard_profile") + " 2", WorldPosition.X + 134f, y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Center, 0, num);
		}
		else if (_currentController == GameInput.ControllerTypeEnum.GamePad)
		{
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("options_gamepad_profile") + " 1", WorldPosition.X - 26f, y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Center, 0, num);
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("options_gamepad_profile") + " 2", WorldPosition.X + 134f, y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Center, 0, num);
		}
		else
		{
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("options_mouse_profile") + " 1", WorldPosition.X - 26f, y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Center, 0, num);
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("options_mouse_profile") + " 2", WorldPosition.X + 134f, y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Center, 0, num);
		}
		CurrentControllerButton.Draw();
		if (_currentController == GameInput.ControllerTypeEnum.Mouse)
		{
			SliderMouseSensitivity.Draw();
		}
		for (int i = 0; i < 2; i++)
		{
			SwitchButton[] array = ControlsButtonArray[(int)_currentController * 2 + i];
			for (int j = 0; j < ((_currentController == GameInput.ControllerTypeEnum.Keyboard) ? array.Length : 4); j++)
			{
				array[j].Draw();
			}
		}
		if (WaitingToButton != null)
		{
			_shopMenu.SpriteBatchEnd(bLight: true);
			_shopMenu.SpriteBatchBegin(bLight: true);
			_shopMenu.TriangleBatch.Begin(BlendState.NonPremultiplied, GameCore.ViewPortMatrix);
			_shopMenu.TriangleBatch.DrawRect(WorldPosition.X - 230f, WorldPosition.Y - 204f, 460f, 572f, new Color(0, 0, 0, (int)(200f * num)));
			_shopMenu.TriangleBatch.End();
			string text = ((_currentController != GameInput.ControllerTypeEnum.Keyboard) ? TextBundle.GetText("controls_press_wanted_button") : TextBundle.GetText("controls_press_wanted_key"));
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, 960f, 465f, Vector2.One, new Color(255, 255, 255, (int)(255f * num)), TextAlign.Center);
		}
	}
}
