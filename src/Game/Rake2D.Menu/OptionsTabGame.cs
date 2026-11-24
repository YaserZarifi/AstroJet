using Microsoft.Xna.Framework;

namespace Rake2D.Menu;

public class OptionsTabGame : Transformation
{
	public SwitchButton[] PlayerControlsButtonArray;

	private ShopMenu _shopMenu;

	private PopupMenuOptions _popupMenu;

	private SwitchButton _buttonWarnings;

	private SwitchButton _buttonHUDMode;

	private SwitchButton _buttonHeroColisoid;

	private SwitchButton _buttonFontSize;

	public OptionsTabGame(ShopMenu shopMenu, PopupMenuOptions popupMenu)
	{
		_shopMenu = shopMenu;
		_popupMenu = popupMenu;
		ParentTransformation = _popupMenu;
		PlayerControlsButtonArray = new SwitchButton[2];
		for (int i = 0; i < 2; i++)
		{
			PlayerControlsButtonArray[i] = new SwitchButton(_shopMenu);
			PlayerControlsButtonArray[i].Type = ControlsButtonTypeEnum.Big;
			PlayerControlsButtonArray[i].LabelOffset.X = -299f;
			PlayerControlsButtonArray[i].Position = new Vector2(91f, -160 + i * 41);
			PlayerControlsButtonArray[i].ParentTransformation = this;
			PlayerControlsButtonArray[i].OnClick += OnPlayerControlerButton;
			PlayerControlsButtonArray[i].IsClickable = true;
		}
		Vector2 position = new Vector2(115f, -60f);
		float num = 50f;
		_buttonWarnings = new SwitchButton(_shopMenu);
		_buttonWarnings.IsClickable = true;
		_buttonWarnings.OnClick += OnTextWarningsButtonClick;
		_buttonWarnings.LabelOffset = new Vector2(-325f, -9f);
		_buttonWarnings.ParentTransformation = this;
		_buttonWarnings.Position = position;
		position.Y += num;
		_buttonHUDMode = new SwitchButton(_shopMenu);
		_buttonHUDMode.IsClickable = true;
		_buttonHUDMode.OnClick += OnHUDModeButtonClick;
		_buttonHUDMode.LabelOffset = new Vector2(-325f, -9f);
		_buttonHUDMode.ParentTransformation = this;
		_buttonHUDMode.Position = position;
		position.Y += num;
		_buttonHeroColisoid = new SwitchButton(_shopMenu);
		_buttonHeroColisoid.IsClickable = true;
		_buttonHeroColisoid.OnClick += OnHeroCollisoidButtonClick;
		_buttonHeroColisoid.LabelOffset = new Vector2(-325f, -9f);
		_buttonHeroColisoid.ParentTransformation = this;
		_buttonHeroColisoid.Position = position;
		position.Y += num;
		_buttonFontSize = new SwitchButton(_shopMenu);
		_buttonFontSize.IsClickable = true;
		_buttonFontSize.OnClick += OnFontSizeButtonClick;
		_buttonFontSize.LabelOffset = new Vector2(-325f, -9f);
		_buttonFontSize.ParentTransformation = this;
		_buttonFontSize.Position = position;
	}

	private void OnTextWarningsButtonClick(SwitchButton button)
	{
		Options.WarningTexts = !Options.WarningTexts;
		ActualizeValues();
		GameCore.PlaySound("menu_option_set");
	}

	private void OnHUDModeButtonClick(SwitchButton button)
	{
		Options.ToggleHUDMode();
		ActualizeValues();
		GameCore.PlaySound("menu_option_set");
	}

	private void OnHeroCollisoidButtonClick(SwitchButton button)
	{
		Options.ShowHeroColisoid = !Options.ShowHeroColisoid;
		ActualizeValues();
		GameCore.PlaySound("menu_option_set");
	}

	private void OnFontSizeButtonClick(SwitchButton button)
	{
		if (Options.FontScale <= 1.1f)
		{
			Options.FontScale = 1.4f;
		}
		else
		{
			Options.FontScale = 1.1f;
		}
		ActualizeValues();
		GameCore.PlaySound("menu_option_set");
	}

	private void OnPlayerControlerButton(SwitchButton button)
	{
		if (button == PlayerControlsButtonArray[0])
		{
			if (Options.PlayerController[0] == GameInput.PlayerControllerEnum.Gamepad)
			{
				Options.GamePadNumber[0] = GameInput.GetFirstConnectedGamePad(new int[2]
				{
					Options.GamePadNumber[0],
					Options.GamePadNumber[1]
				});
				if (Options.GamePadNumber[0] < 0)
				{
					Options.PlayerController[0] = GameInput.PlayerControllerEnum.Mouse;
				}
			}
			else if (Options.PlayerController[0] < GameInput.PlayerControllerEnum.Mouse)
			{
				Options.PlayerController[0]++;
				if (Options.PlayerController[0] == GameInput.PlayerControllerEnum.Gamepad)
				{
					Options.GamePadNumber[0] = GameInput.GetFirstConnectedGamePad(new int[1] { Options.GamePadNumber[1] });
				}
				else if (Options.PlayerController[0] == Options.PlayerController[1] && Options.PlayerController[0] == GameInput.PlayerControllerEnum.Mouse)
				{
					Options.PlayerController[1] = GameInput.PlayerControllerEnum.Keyboard;
				}
			}
			else
			{
				Options.PlayerController[0] = GameInput.PlayerControllerEnum.Keyboard;
			}
		}
		else if (button == PlayerControlsButtonArray[1])
		{
			if (Options.PlayerController[1] == GameInput.PlayerControllerEnum.Gamepad)
			{
				Options.GamePadNumber[1] = GameInput.GetFirstConnectedGamePad(new int[2]
				{
					Options.GamePadNumber[0],
					Options.GamePadNumber[1]
				});
				if (Options.GamePadNumber[1] < 0)
				{
					Options.PlayerController[1] = GameInput.PlayerControllerEnum.Mouse;
				}
			}
			else if (Options.PlayerController[1] < GameInput.PlayerControllerEnum.Mouse)
			{
				Options.PlayerController[1]++;
				if (Options.PlayerController[1] == GameInput.PlayerControllerEnum.Gamepad)
				{
					Options.GamePadNumber[1] = GameInput.GetFirstConnectedGamePad(new int[1] { Options.GamePadNumber[0] });
				}
				else if (Options.PlayerController[0] == Options.PlayerController[1] && Options.PlayerController[0] == GameInput.PlayerControllerEnum.Mouse)
				{
					Options.PlayerController[0] = GameInput.PlayerControllerEnum.Keyboard;
				}
			}
			else
			{
				Options.PlayerController[1] = GameInput.PlayerControllerEnum.Keyboard;
			}
		}
		GameCore.PlaySound("menu_option_set");
		ActualizeValues();
	}

	public void SetTexts()
	{
		for (int i = 0; i < 2; i++)
		{
			PlayerControlsButtonArray[i].Label = TextBundle.GetText($"options_player_controls_{i + 1}");
		}
		_buttonWarnings.Label = TextBundle.GetText("options_warnings");
		_buttonHUDMode.Label = TextBundle.GetText("options_hud_mode");
		_buttonHeroColisoid.Label = TextBundle.GetText("options_hero_colisoid");
		_buttonFontSize.Label = TextBundle.GetText("options_font_size");
		ActualizeValues();
	}

	public void ActualizeValues()
	{
		for (int i = 0; i < 2; i++)
		{
			if (Options.PlayerController[i] == GameInput.PlayerControllerEnum.Keyboard)
			{
				PlayerControlsButtonArray[i].Text = TextBundle.GetText("options_controller_keyboard");
				continue;
			}
			if (Options.PlayerController[i] == GameInput.PlayerControllerEnum.Mouse)
			{
				PlayerControlsButtonArray[i].Text = TextBundle.GetText("options_controller_mouse");
				continue;
			}
			PlayerControlsButtonArray[i].Text = TextBundle.GetText("options_controller_gamepad");
			if (Options.GamePadNumber[i] >= 0)
			{
				PlayerControlsButtonArray[i].Text += $" {Options.GamePadNumber[i] + 1}";
			}
		}
		if (Options.WarningTexts)
		{
			_buttonWarnings.Text = TextBundle.GetText("options_value_on");
		}
		else
		{
			_buttonWarnings.Text = TextBundle.GetText("options_value_off");
		}
		if (Options.ShowHeroColisoid)
		{
			_buttonHeroColisoid.Text = TextBundle.GetText("options_value_on");
		}
		else
		{
			_buttonHeroColisoid.Text = TextBundle.GetText("options_value_off");
		}
		switch (Options.HUDMode)
		{
		case HUDModeEnum.None:
			_buttonHUDMode.Text = TextBundle.GetText("options_hud_mode_none");
			break;
		case HUDModeEnum.Both:
			_buttonHUDMode.Text = TextBundle.GetText("options_hud_mode_both");
			break;
		case HUDModeEnum.MiniOnly:
			_buttonHUDMode.Text = TextBundle.GetText("options_hud_mode_mini");
			break;
		case HUDModeEnum.BigOnly:
			_buttonHUDMode.Text = TextBundle.GetText("options_hud_mode_big");
			break;
		}
		if (Options.FontScale <= 1.1f)
		{
			_buttonFontSize.Text = TextBundle.GetText("options_font_size_small");
		}
		else
		{
			_buttonFontSize.Text = TextBundle.GetText("options_font_size_big");
		}
	}

	public bool IsAnyItemSelected()
	{
		SwitchButton[] playerControlsButtonArray = PlayerControlsButtonArray;
		for (int i = 0; i < playerControlsButtonArray.Length; i++)
		{
			if (playerControlsButtonArray[i].IsSelected)
			{
				return true;
			}
		}
		if (_buttonHUDMode.IsSelected || _buttonWarnings.IsSelected || _buttonHeroColisoid.IsSelected || _buttonFontSize.IsSelected)
		{
			return true;
		}
		return false;
	}

	public void Show()
	{
		ActualizeValues();
	}

	public void SelectPlayerControlsButton(int player)
	{
		PlayerControlsButtonArray[player].Select();
	}

	public void UnselectAll()
	{
		SwitchButton[] playerControlsButtonArray = PlayerControlsButtonArray;
		for (int i = 0; i < playerControlsButtonArray.Length; i++)
		{
			playerControlsButtonArray[i].Unselect();
		}
	}

	private void ParseInput()
	{
		if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp))
		{
			if (PlayerControlsButtonArray[1].IsSelected)
			{
				PlayerControlsButtonArray[1].IsSelected = false;
				SelectPlayerControlsButton(0);
			}
			else if (PlayerControlsButtonArray[0].IsSelected)
			{
				PlayerControlsButtonArray[0].IsSelected = false;
				_popupMenu.TabButtonGame.Select();
			}
			else if (_buttonWarnings.IsSelected)
			{
				_buttonWarnings.Unselect();
				PlayerControlsButtonArray[1].Select();
			}
			else if (_buttonHUDMode.IsSelected)
			{
				_buttonHUDMode.Unselect();
				_buttonWarnings.Select();
			}
			else if (_buttonHeroColisoid.IsSelected)
			{
				_buttonHeroColisoid.Unselect();
				_buttonHUDMode.Select();
			}
			else if (_buttonFontSize.IsSelected)
			{
				_buttonFontSize.Unselect();
				_buttonHeroColisoid.Select();
			}
			else if (_popupMenu.ButtonClose.IsSelected || _popupMenu.ButtonReset.IsSelected)
			{
				_popupMenu.ButtonClose.Unselect();
				_popupMenu.ButtonReset.Unselect();
				_buttonFontSize.Select();
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveDown))
		{
			if (PlayerControlsButtonArray[0].IsSelected)
			{
				PlayerControlsButtonArray[0].Unselect();
				PlayerControlsButtonArray[1].Select();
			}
			else if (PlayerControlsButtonArray[1].IsSelected)
			{
				PlayerControlsButtonArray[1].Unselect();
				_buttonWarnings.Select();
			}
			else if (_buttonWarnings.IsSelected)
			{
				_buttonWarnings.Unselect();
				_buttonHUDMode.Select();
			}
			else if (_buttonHUDMode.IsSelected)
			{
				_buttonHUDMode.Unselect();
				_buttonHeroColisoid.Select();
			}
			else if (_buttonHeroColisoid.IsSelected)
			{
				_buttonHeroColisoid.Unselect();
				_buttonFontSize.Select();
			}
			else if (_buttonFontSize.IsSelected)
			{
				_buttonFontSize.Unselect();
				_popupMenu.ButtonClose.Select();
			}
		}
	}

	public void Update(float elapsedTime)
	{
		ParseInput();
		Color = _popupMenu.Color;
		_buttonWarnings.Update(elapsedTime);
		_buttonHUDMode.Update(elapsedTime);
		_buttonHeroColisoid.Update(elapsedTime);
		_buttonFontSize.Update(elapsedTime);
		for (int i = 0; i < 2; i++)
		{
			SwitchButton switchButton = PlayerControlsButtonArray[i];
			switchButton.Update(elapsedTime);
			if (Options.PlayerController[0] == Options.PlayerController[1] && Options.PlayerController[0] == GameInput.PlayerControllerEnum.Mouse)
			{
				switchButton.TextColor = new Color(255, 0, 0);
			}
			else if (Options.PlayerController[i] == GameInput.PlayerControllerEnum.Gamepad && !GameInput.IsGamepadConnected(Options.GamePadNumber[i]))
			{
				switchButton.TextColor = new Color(100, 100, 100, 200);
			}
			else
			{
				switchButton.TextColor = ShopMenu.FONT_COLOR_PARAM_VALUE;
			}
		}
		if (Options.PlayerController[0] == GameInput.PlayerControllerEnum.Gamepad && Options.GamePadNumber[0] < 0)
		{
			Options.GamePadNumber[0] = GameInput.GetFirstConnectedGamePad(new int[1] { Options.GamePadNumber[1] });
			if (Options.GamePadNumber[0] >= 0)
			{
				ActualizeValues();
			}
		}
		if (Options.PlayerController[1] == GameInput.PlayerControllerEnum.Gamepad && Options.GamePadNumber[1] < 0)
		{
			Options.GamePadNumber[1] = GameInput.GetFirstConnectedGamePad(new int[1] { Options.GamePadNumber[0] });
			if (Options.GamePadNumber[1] >= 0)
			{
				ActualizeValues();
			}
		}
	}

	public void Draw()
	{
		SwitchButton[] playerControlsButtonArray = PlayerControlsButtonArray;
		for (int i = 0; i < playerControlsButtonArray.Length; i++)
		{
			playerControlsButtonArray[i].Draw();
		}
		_buttonWarnings.Draw();
		_buttonHeroColisoid.Draw();
		_buttonHUDMode.Draw();
		_buttonFontSize.Draw();
	}
}
