using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class PopupMenuInLevel : PopupDialog
{
	public Button ButtonResume;

	public Button ButtonRestart;

	public Button ButtonOptions;

	public Button ButtonEndMission;

	private Button _buttonYes;

	private Button _buttonNo;

	private string _yesNoQuestionText;

	private bool _drawYesNo;

	private bool IsAnyButtonSelected
	{
		get
		{
			if (!ButtonResume.IsSelected && !ButtonRestart.IsSelected && !ButtonOptions.IsSelected)
			{
				return ButtonEndMission.IsSelected;
			}
			return true;
		}
	}

	public PopupMenuInLevel(ShopMenu shopMenu)
		: base(shopMenu)
	{
		Init();
	}

	public override void Init()
	{
		base.Init();
		_atlasBgr = _shopMenu.AtlasYesNoDialog;
		ButtonResume = new Button(_shopMenu, ShopButtonType.Big);
		ButtonResume.Position = new Vector2(0f, -90f);
		ButtonResume.ParentTransformation = this;
		ButtonResume.OnClick += OnClickButtonResume;
		ButtonRestart = new Button(_shopMenu, ShopButtonType.Big);
		ButtonRestart.Position = new Vector2(0f, ButtonResume.Position.Y + 65f);
		ButtonRestart.ParentTransformation = this;
		ButtonRestart.OnClick += OnClickButtonRestart;
		ButtonOptions = new Button(_shopMenu, ShopButtonType.Big);
		ButtonOptions.Position = new Vector2(0f, ButtonRestart.Position.Y + 65f);
		ButtonOptions.ParentTransformation = this;
		ButtonOptions.OnClick += OnClickButtonOptions;
		ButtonEndMission = new Button(_shopMenu, ShopButtonType.Big);
		ButtonEndMission.Position = new Vector2(0f, ButtonOptions.Position.Y + 65f);
		ButtonEndMission.ParentTransformation = this;
		ButtonEndMission.OnClick += OnClickButtonEndMission;
		_buttonYes = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonYes.Position = new Vector2(-_buttonYes.Width / 2, 110f);
		_buttonYes.ParentTransformation = this;
		_buttonYes.OnClick += OnClickButtonYes;
		_buttonNo = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonNo.Position = new Vector2(_buttonNo.Width / 2, 110f);
		_buttonNo.ParentTransformation = this;
		_buttonNo.OnClick += OnClickButtonNo;
		SetTexts();
	}

	public void SetTexts()
	{
		_yesNoQuestionText = TextBundle.GetText("yes_no_end_mision");
		ButtonResume.Caption = TextBundle.GetText("game_menu_resume_level");
		ButtonRestart.Caption = TextBundle.GetText("game_menu_restart_level");
		ButtonOptions.Caption = TextBundle.GetText("game_menu_options");
		ButtonEndMission.Caption = TextBundle.GetText("game_menu_end_mission");
		_buttonYes.Caption = TextBundle.GetText("game_menu_button_yes");
		_buttonNo.Caption = TextBundle.GetText("game_menu_button_no");
	}

	private void OnClickButtonResume(Button button)
	{
		Hide();
		if (!GameCore.IsPaused)
		{
			GameCore.SoundSystem.ResumeAllSounds();
			GameCore.SoundSystem.ResumeMusic();
		}
		GameCore.PlaySound("menu_yesno_hide");
	}

	private void OnClickButtonRestart(Button button)
	{
		SetState(ShopMenuState.Hidden);
		if (!GameCore.IsPaused)
		{
			GameCore.SoundSystem.ResumeAllSounds();
			GameCore.SoundSystem.ResumeMusic();
		}
		GameCore.PlaySound("menu_yesno_hide");
		GameJnG.LoadingScreen.Show(GameCore.GameLevel.LevelData, fadeToShop: false);
	}

	private void OnClickButtonOptions(Button button)
	{
		Hide();
		State = ShopMenuState.Hidden;
		GameJnG.PopupMenuOptions.Show();
		GameJnG.PopupMenuOptions.SetState(ShopMenuState.Visible);
	}

	private void OnClickButtonEndMission(Button button)
	{
		ButtonEndMission.Unselect();
		if (_shopMenu.CurrentController != GameInput.ControllerTypeEnum.Mouse)
		{
			_buttonYes.SetSelected();
		}
		else
		{
			_buttonYes.SetUnselected();
		}
		_buttonNo.SetUnselected();
		GameCore.PlaySound("menu_yesno_show");
		GameJnG.CancelFinishLevel();
		GameJnG.MissionCompletedScreen.SetHidden();
		GameJnG.MissionFailedScreen.SetHidden();
		GameJnG.IsDrunkenEffectEnabled = false;
		_drawYesNo = true;
	}

	private void OnClickButtonYes(Button button)
	{
		SetState(ShopMenuState.Hidden);
		if (GameJnG.IsFirstLevelTry)
		{
			GameJnG.LoadingScreen.ShowToMainMenu();
		}
		else
		{
			GameJnG.LoadingScreen.Show(null, fadeToShop: true);
		}
	}

	private void OnClickButtonNo(Button button)
	{
		_drawYesNo = false;
		if (_shopMenu.CurrentController != GameInput.ControllerTypeEnum.Mouse)
		{
			ButtonEndMission.SetSelected();
		}
		else
		{
			ButtonEndMission.SetUnselected();
		}
		GameCore.PlaySound("menu_yesno_hide");
	}

	private void ParseInput()
	{
		if (GameCore.Console.IsActive)
		{
			return;
		}
		if ((!GameInput.ControlDown && GameInput.SystemKeyPressed(Keys.Tab)) || _shopMenu.KeyPressed(ShopMenuKeys.CancelAll) || GameInput.MousePressed(GameInput.MouseButton.Right))
		{
			OnClickButtonResume(null);
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp, bOnlyOnce: false))
		{
			if (ButtonEndMission.IsSelected)
			{
				ButtonEndMission.Unselect();
				ButtonOptions.Select();
			}
			else if (ButtonRestart.IsSelected)
			{
				ButtonRestart.Unselect();
				ButtonResume.Select();
			}
			else if (ButtonOptions.IsSelected)
			{
				ButtonOptions.Unselect();
				ButtonRestart.Select();
			}
			else if (ButtonResume.IsSelected)
			{
				ButtonResume.Unselect();
				ButtonEndMission.Select();
			}
			else
			{
				ButtonEndMission.Select();
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveDown, bOnlyOnce: false))
		{
			if (ButtonResume.IsSelected)
			{
				ButtonResume.Unselect();
				ButtonRestart.Select();
			}
			else if (ButtonRestart.IsSelected)
			{
				ButtonRestart.Unselect();
				ButtonOptions.Select();
			}
			else if (ButtonOptions.IsSelected)
			{
				ButtonOptions.Unselect();
				ButtonEndMission.Select();
			}
			else if (ButtonEndMission.IsSelected)
			{
				ButtonEndMission.Unselect();
				ButtonResume.Select();
			}
			else
			{
				ButtonResume.Select();
			}
		}
	}

	private void ParseInputYesNo()
	{
		if (GameCore.Console.IsActive)
		{
			return;
		}
		if ((!GameInput.ControlDown && GameInput.SystemKeyPressed(Keys.Tab)) || _shopMenu.KeyPressed(ShopMenuKeys.CancelAll) || GameInput.MousePressed(GameInput.MouseButton.Right))
		{
			_buttonNo.Click();
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeft, bOnlyOnce: false))
		{
			if (_buttonNo.IsSelected)
			{
				_buttonNo.Unselect();
				_buttonYes.Select();
			}
			else if (_buttonYes.IsSelected)
			{
				_buttonYes.Unselect();
				_buttonNo.Select();
			}
			else
			{
				_buttonNo.Select();
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRight, bOnlyOnce: false))
		{
			if (_buttonNo.IsSelected)
			{
				_buttonNo.Unselect();
				_buttonYes.Select();
			}
			else if (_buttonYes.IsSelected)
			{
				_buttonYes.Unselect();
				_buttonNo.Select();
			}
			else
			{
				_buttonYes.Select();
			}
		}
	}

	public override void Show()
	{
		base.Show();
		SetTexts();
		_shopMenu.Cursor.UpdatePosition();
		_shopMenu.Cursor.IsPositionChanged = false;
		_shopMenu.ShopInput.CancelKeyPressed();
		Position.X = 960f;
		Position.Y = 540f;
		if (!ButtonOptions.IsSelected)
		{
			ButtonResume.SetSelected();
			ButtonRestart.SetUnselected();
			ButtonOptions.SetUnselected();
			ButtonEndMission.SetUnselected();
		}
		_drawYesNo = false;
	}

	public override void Hide()
	{
		base.Hide();
	}

	private void UpdateYesNo(float elapsedTime)
	{
		ParseInputYesNo();
		_buttonYes.Update(elapsedTime);
		_buttonNo.Update(elapsedTime);
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (_drawYesNo)
		{
			UpdateYesNo(elapsedTime);
			return;
		}
		ParseInput();
		ButtonResume.Update(elapsedTime);
		ButtonRestart.Update(elapsedTime);
		ButtonOptions.Update(elapsedTime);
		ButtonEndMission.Update(elapsedTime);
	}

	private void DrawYesNo()
	{
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, _yesNoQuestionText, WorldPosition.X, WorldPosition.Y - 90f, new Vector2(0.8f), new Color(ShopMenu.FONT_COLOR_YES_NO_QUESTION, Color.A), TextAlign.Center, 500, 1f, 40f);
		_buttonYes.Draw();
		_buttonNo.Draw();
	}

	public override void Draw()
	{
		_shopMenu.SpriteBatchBegin(bLight: true);
		base.Draw();
		if (_drawYesNo)
		{
			DrawYesNo();
		}
		else
		{
			ButtonResume.Draw();
			ButtonRestart.Draw();
			ButtonOptions.Draw();
			ButtonEndMission.Draw();
		}
		_shopMenu.SpriteBatchEnd(bLight: true);
		GameCore.EffectLightBasic.SetLightMaps(bUseShopMaps: false);
		GameCore.EffectLight.SetLightMaps(bUseShopMaps: false);
	}
}
