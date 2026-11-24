using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Rake2D.JnG;
using Rake2D.Sound;

namespace Rake2D.Menu;

public class PopupMenuOptions : PopupDialog
{
	public OptionsTabEnum CurrentTab;

	public HorizontalSlider SliderMusicVolume;

	public HorizontalSlider SliderSFXVolume;

	public HorizontalSlider SliderSoundPanning;

	public HorizontalSlider SliderLOD;

	public HorizontalSlider SliderQuakeMode;

	public HorizontalSlider SliderMaxFPS;

	public Button ButtonClose;

	public Button ButtonReset;

	public Button TabButtonAudioVideo;

	public Button TabButtonGame;

	public Button TabButtonControls;

	public SwitchButton ButtonFullscreen;

	public SwitchButton ButtonVsync;

	public OptionsTabControls TabControls;

	public OptionsTabGame TabGame;

	private ISound2D _testSound;

	private bool _updateMusic;

	private float _musicPos;

	private float _oldMusicPos;

	private bool _showControllerUnpluggedMessage;

	public Button CursorTabButton
	{
		get
		{
			if (TabButtonAudioVideo.IsSelected)
			{
				return TabButtonAudioVideo;
			}
			if (TabButtonGame.IsSelected)
			{
				return TabButtonGame;
			}
			if (TabButtonControls.IsSelected)
			{
				return TabButtonControls;
			}
			return null;
		}
	}

	public PopupMenuOptions(ShopMenu shopMenu)
		: base(shopMenu)
	{
		Init();
	}

	public override void Init()
	{
		base.Init();
		ButtonClose = new Button(_shopMenu, ShopButtonType.Normal);
		ButtonClose.OnClick += OnCloseButtonClick;
		ButtonClose.HasPressEffectEnabled = true;
		ButtonClose.Position = new Vector2(ButtonClose.Width / 2, 325f);
		ButtonClose.ParentTransformation = this;
		ButtonReset = new Button(_shopMenu, ShopButtonType.Normal);
		ButtonReset.OnClick += OnResetButtonClick;
		ButtonReset.Position = new Vector2(-ButtonReset.Width / 2, ButtonClose.Position.Y);
		ButtonReset.ParentTransformation = this;
		ButtonReset.HasPressEffectEnabled = true;
		TabButtonAudioVideo = new Button(_shopMenu, ShopButtonType.TabSmall);
		TabButtonAudioVideo.OnClick += OnTabButtonClick;
		TabButtonAudioVideo.CaptionScale = ShopMenu.FONT_SCALE_TAB_BUTTON;
		TabButtonAudioVideo.Position = new Vector2(-TabButtonAudioVideo.Width, -227f);
		TabButtonAudioVideo.ParentTransformation = this;
		TabButtonGame = new Button(_shopMenu, ShopButtonType.TabSmall);
		TabButtonGame.OnClick += OnTabButtonClick;
		TabButtonGame.Position = new Vector2(0f, -227f);
		TabButtonGame.CaptionScale = ShopMenu.FONT_SCALE_TAB_BUTTON;
		TabButtonGame.ParentTransformation = this;
		TabButtonControls = new Button(_shopMenu, ShopButtonType.TabSmall);
		TabButtonControls.OnClick += OnTabButtonClick;
		TabButtonControls.Position = new Vector2(TabButtonControls.Width, -227f);
		TabButtonControls.CaptionScale = ShopMenu.FONT_SCALE_TAB_BUTTON;
		TabButtonControls.ParentTransformation = this;
		TabControls = new OptionsTabControls(_shopMenu, this);
		TabGame = new OptionsTabGame(_shopMenu, this);
		Vector2 position = new Vector2(115f, -160f);
		Vector2 labelOffset = new Vector2(-325f, -13f);
		SliderSFXVolume = new HorizontalSlider(_shopMenu);
		SliderSFXVolume.MaxValue = 100;
		SliderSFXVolume.LabelValueMode = SliderLabeValueModeEnum.Percentage;
		SliderSFXVolume.LabelValueOffset = 0;
		SliderSFXVolume.LabelOffset = labelOffset;
		SliderSFXVolume.ParentTransformation = this;
		SliderSFXVolume.Position = position;
		SliderSFXVolume.OnSetValue += OnSFXVolumeSliderValue;
		SliderSFXVolume.OnActivate += OnSliderSFXVolumeActivate;
		SliderSFXVolume.OnDeactivate += OnSliderSFXVolumeDeactivate;
		position.Y += 50f;
		SliderMusicVolume = new HorizontalSlider(_shopMenu);
		SliderMusicVolume.MaxValue = 100;
		SliderMusicVolume.LabelValueOffset = 0;
		SliderMusicVolume.LabelValueMode = SliderLabeValueModeEnum.Percentage;
		SliderMusicVolume.LabelOffset = labelOffset;
		SliderMusicVolume.ParentTransformation = this;
		SliderMusicVolume.Position = position;
		SliderMusicVolume.OnSetValue += OnMusicVolumeSliderValue;
		SliderMusicVolume.OnActivate += OnSliderSFXVolumeActivate;
		SliderMusicVolume.OnDeactivate += OnSliderSFXVolumeDeactivate;
		position.Y += 50f;
		SliderSoundPanning = new HorizontalSlider(_shopMenu);
		SliderSoundPanning.MaxValue = 2;
		SliderSoundPanning.LabelValueOffset = 0;
		SliderSoundPanning.LabelValueMode = SliderLabeValueModeEnum.StringArray;
		SliderSoundPanning.LabelOffset = labelOffset;
		SliderSoundPanning.ParentTransformation = this;
		SliderSoundPanning.Position = position;
		SliderSoundPanning.OnSetValue += OnSoundPanningSliderValue;
		position.Y += 70f;
		SliderLOD = new HorizontalSlider(_shopMenu);
		SliderLOD.MaxValue = 3;
		SliderLOD.LabelValueMode = SliderLabeValueModeEnum.StringArray;
		SliderLOD.LabelValueOffset = 0;
		SliderLOD.LabelOffset = labelOffset;
		SliderLOD.ParentTransformation = this;
		SliderLOD.Position = position;
		SliderLOD.OnSetValue += OnLodSliderValue;
		position.Y += 50f;
		SliderQuakeMode = new HorizontalSlider(_shopMenu);
		SliderQuakeMode.MaxValue = 2;
		SliderQuakeMode.LabelValueMode = SliderLabeValueModeEnum.StringArray;
		SliderQuakeMode.LabelValueOffset = 0;
		SliderQuakeMode.LabelOffset = labelOffset;
		SliderQuakeMode.ParentTransformation = this;
		SliderQuakeMode.Position = position;
		SliderQuakeMode.OnSetValue += OnQuakeModeSliderValue;
		position.Y += 50f;
		SliderMaxFPS = new HorizontalSlider(_shopMenu);
		SliderMaxFPS.MaxValue = 4;
		SliderMaxFPS.LabelValueMode = SliderLabeValueModeEnum.StringArray;
		SliderMaxFPS.LabelValueOffset = 0;
		SliderMaxFPS.LabelOffset = labelOffset;
		SliderMaxFPS.ParentTransformation = this;
		SliderMaxFPS.Position = position;
		SliderMaxFPS.OnSetValue += OnMaxFPSSliderValue;
		position.Y += 70f;
		ButtonFullscreen = new SwitchButton(_shopMenu);
		ButtonFullscreen.IsClickable = true;
		ButtonFullscreen.OnClick += OnFullscreenButtonClick;
		ButtonFullscreen.LabelOffset = labelOffset;
		ButtonFullscreen.ParentTransformation = this;
		ButtonFullscreen.Position = position;
		position.Y += 50f;
		ButtonVsync = new SwitchButton(_shopMenu);
		ButtonVsync.IsClickable = true;
		ButtonVsync.OnClick += OnVsyncButtonClick;
		ButtonVsync.LabelOffset = labelOffset;
		ButtonVsync.ParentTransformation = this;
		ButtonVsync.Position = position;
		SetTexts();
	}

	public void SetTexts()
	{
		TabButtonAudioVideo.Caption = TextBundle.GetText("options_tab_audio_video");
		TabButtonGame.Caption = TextBundle.GetText("options_tab_game");
		TabButtonControls.Caption = TextBundle.GetText("options_tab_controls");
		SliderMusicVolume.Label = TextBundle.GetText("options_music_volume");
		SliderSoundPanning.Label = TextBundle.GetText("options_sound_panning");
		SliderSFXVolume.Label = TextBundle.GetText("options_sfx_volume");
		SliderLOD.Label = TextBundle.GetText("options_lod");
		SliderQuakeMode.Label = TextBundle.GetText("options_quake");
		SliderMaxFPS.Label = TextBundle.GetText("options_max_fps");
		ButtonFullscreen.Label = TextBundle.GetText("options_fullscreen");
		ButtonVsync.Label = TextBundle.GetText("options_vsync");
		ButtonClose.Caption = TextBundle.GetText("button_close");
		ButtonReset.Caption = TextBundle.GetText("button_reset");
		SliderLOD.ValueStringArray = new string[4]
		{
			TextBundle.GetText("options_lod_low"),
			TextBundle.GetText("options_lod_medium"),
			TextBundle.GetText("options_lod_high"),
			TextBundle.GetText("options_lod_full")
		};
		SliderQuakeMode.ValueStringArray = new string[3]
		{
			TextBundle.GetText("options_quake_off"),
			TextBundle.GetText("options_quake_low"),
			TextBundle.GetText("options_quake_normal")
		};
		SliderSoundPanning.ValueStringArray = new string[3]
		{
			TextBundle.GetText("options_sound_panning_off"),
			TextBundle.GetText("options_sound_panning_low"),
			TextBundle.GetText("options_sound_panning_normal")
		};
		SliderMaxFPS.ValueStringArray = new string[5]
		{
			"30",
			"60",
			"120",
			"200",
			TextBundle.GetText("options_max_fps_unlimited")
		};
		TabGame.SetTexts();
		TabControls.SetTexts();
	}

	private void OnCloseButtonClick(Button button)
	{
		Hide();
	}

	private void OnResetButtonClick(Button button)
	{
		switch (CurrentTab)
		{
		case OptionsTabEnum.AudioVideo:
			Options.SetDefaultsAudioVideo();
			GameCore.SoundSystem.MusicVolume = Options.MusicVolume;
			GameCore.SoundSystem.SoundVolume = Options.SoundVolume;
			Options.SetDefaultsGame();
			ActualizeValues();
			break;
		case OptionsTabEnum.Controls:
			TabControls.ResetCurrentControls();
			break;
		case OptionsTabEnum.Game:
			Options.SetDefaultControllers();
			Options.SetDefaultsGame();
			TabGame.ActualizeValues();
			break;
		}
		GameCore.PlaySound("menu_option_set");
	}

	private void OnMusicVolumeSliderValue(HorizontalSlider slider)
	{
		Options.MusicVolume = SoundUtils.GetExponentialVolume(slider.Value);
		GameCore.SoundSystem.MusicVolume = Options.MusicVolume;
	}

	private void OnSoundPanningSliderValue(HorizontalSlider slider)
	{
		Options.SoundPanningLevel = slider.Value;
		GameCore.PlaySound("menu_option_set");
	}

	private void OnSFXVolumeSliderValue(HorizontalSlider slider)
	{
		Options.SoundVolume = SoundUtils.GetExponentialVolume(slider.Value);
		GameCore.SoundSystem.SoundVolume = Options.SoundVolume;
	}

	private void OnSliderSFXVolumeActivate(HorizontalSlider slider)
	{
		if (_testSound == null)
		{
			_testSound = GameCore.PlaySound("menu_volume_test", 100f, 0f, looped: true);
		}
		if (_updateMusic)
		{
			GameCore.SoundSystem.PlayMusicFromPositionInSec(isLooped: true, _musicPos);
		}
	}

	private void OnSliderSFXVolumeDeactivate(HorizontalSlider slider)
	{
		if (_testSound != null)
		{
			_testSound.Stop();
			_testSound = null;
		}
		if (_updateMusic)
		{
			_musicPos = GameCore.SoundSystem.GetMusicPositionInSec();
			GameCore.SoundSystem.PauseMusic();
		}
	}

	private void OnLodSliderValue(HorizontalSlider slider)
	{
		Options.LevelOfDetails = slider.Value;
		GameCore.PlaySound("menu_option_set");
	}

	private void OnQuakeModeSliderValue(HorizontalSlider slider)
	{
		Options.QuakeMode = slider.Value;
		GameCore.PlaySound("menu_option_set");
	}

	private void OnMaxFPSSliderValue(HorizontalSlider slider)
	{
		switch (slider.Value)
		{
		case 0:
			Options.MaxFPS = 30;
			break;
		case 1:
			Options.MaxFPS = 60;
			break;
		case 2:
			Options.MaxFPS = 120;
			break;
		case 3:
			Options.MaxFPS = 200;
			break;
		default:
			Options.MaxFPS = 0;
			break;
		}
		_shopMenu.GameCore.SetMaxFPS();
		GameCore.PlaySound("menu_option_set");
	}

	private void OnFullscreenButtonClick(SwitchButton button)
	{
		GameCore.ToggleFullScreen();
	}

	private void OnVsyncButtonClick(SwitchButton button)
	{
		_shopMenu.GameCore.SetVSync(!Options.VSync);
	}

	private void SetFullscreenButtonCaption()
	{
		if (Options.FullScreen)
		{
			ButtonFullscreen.Text = TextBundle.GetText("options_value_on");
		}
		else
		{
			ButtonFullscreen.Text = TextBundle.GetText("options_value_off");
		}
	}

	private void SetVsyncButtonCaption()
	{
		if (Options.VSync)
		{
			ButtonVsync.Text = TextBundle.GetText("options_value_on");
		}
		else
		{
			ButtonVsync.Text = TextBundle.GetText("options_value_off");
		}
	}

	public void SwitchTab(OptionsTabEnum tab)
	{
		if (CurrentTab != tab)
		{
			GameCore.PlaySound("menu_tab_switch");
			CurrentTab = tab;
		}
		UnselectAll();
		switch (tab)
		{
		case OptionsTabEnum.AudioVideo:
			TabButtonAudioVideo.Select(bPlaySound: false);
			TabButtonAudioVideo.IsPressed = true;
			TabButtonGame.IsPressed = false;
			TabButtonControls.IsPressed = false;
			break;
		case OptionsTabEnum.Game:
			TabGame.Show();
			TabButtonGame.Select(bPlaySound: false);
			TabButtonAudioVideo.IsPressed = false;
			TabButtonGame.IsPressed = true;
			TabButtonControls.IsPressed = false;
			break;
		case OptionsTabEnum.Controls:
			TabControls.Show();
			TabButtonControls.Select(bPlaySound: false);
			TabButtonAudioVideo.IsPressed = false;
			TabButtonGame.IsPressed = false;
			TabButtonControls.IsPressed = true;
			break;
		}
	}

	private void OnTabButtonClick(Button button)
	{
		if (button == TabButtonGame)
		{
			SwitchTab(OptionsTabEnum.Game);
		}
		else if (button == TabButtonAudioVideo)
		{
			SwitchTab(OptionsTabEnum.AudioVideo);
		}
		else if (button == TabButtonControls)
		{
			SwitchTab(OptionsTabEnum.Controls);
		}
	}

	public void SetControllerUnplugged()
	{
		_showControllerUnpluggedMessage = true;
		SwitchTab(OptionsTabEnum.Game);
	}

	private void ActualizeValues()
	{
		SliderMusicVolume.SetValue((int)SoundUtils.GetNormalVolume(Options.MusicVolume), bCallEvents: false);
		SliderSFXVolume.SetValue((int)SoundUtils.GetNormalVolume(Options.SoundVolume), bCallEvents: false);
		SliderSoundPanning.SetValue(Options.SoundPanningLevel, bCallEvents: false);
		SliderLOD.SetValue(Options.LevelOfDetails, bCallEvents: false);
		SliderQuakeMode.SetValue(Options.QuakeMode, bCallEvents: false);
		if (Options.MaxFPS < 30 || Options.MaxFPS > 200)
		{
			SliderMaxFPS.SetValue(4, bCallEvents: false);
		}
		else if (Options.MaxFPS < 60)
		{
			SliderMaxFPS.SetValue(0, bCallEvents: false);
		}
		else if (Options.MaxFPS < 120)
		{
			SliderMaxFPS.SetValue(1, bCallEvents: false);
		}
		else if (Options.MaxFPS < 200)
		{
			SliderMaxFPS.SetValue(2, bCallEvents: false);
		}
		else
		{
			SliderMaxFPS.SetValue(3, bCallEvents: false);
		}
		SetVsyncButtonCaption();
	}

	public override void Show()
	{
		base.Show();
		_showControllerUnpluggedMessage = false;
		_updateMusic = !GameCore.IsShopMenuEnabled && !GameCore.IsMainMenuEnabled;
		_oldMusicPos = GameCore.SoundSystem.GetMusicPositionInSec();
		_musicPos = Math.Max(20f, _oldMusicPos);
		Position.X = 960f;
		Position.Y = 440f;
		ActualizeValues();
		SwitchTab(CurrentTab);
		TabButtonAudioVideo.SetUnselected();
		TabButtonGame.SetUnselected();
		TabButtonControls.SetUnselected();
		if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
		{
			ButtonClose.SetUnselected();
		}
		else
		{
			ButtonClose.SetSelected();
		}
		ButtonClose.CancelPressEffect();
		GameCore.PlaySound("menu_yesno_show");
	}

	public override void Hide()
	{
		base.Hide();
		if (_updateMusic)
		{
			GameCore.SoundSystem.SetMusicPositionInSec(_oldMusicPos);
		}
		OnSliderSFXVolumeDeactivate(SliderSFXVolume);
		SliderMaxFPS.IsDragged = false;
		SliderSFXVolume.IsDragged = false;
		SliderMusicVolume.IsDragged = false;
		SliderQuakeMode.IsDragged = false;
		if (State != ShopMenuState.Hidden)
		{
			if (GameCore.IsMainMenuEnabled)
			{
				if (_shopMenu.CurrentController != GameInput.ControllerTypeEnum.Mouse)
				{
					GameJnG.MainMenu.SelectButton(2);
				}
			}
			else if (!GameCore.IsShopMenuEnabled)
			{
				SetState(ShopMenuState.Hidden);
				GameJnG.PopupMenuInLevel.Show();
				GameJnG.PopupMenuInLevel.SetState(ShopMenuState.Visible);
				if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
				{
					GameJnG.PopupMenuInLevel.ButtonOptions.SetUnselected();
				}
				else
				{
					GameJnG.PopupMenuInLevel.ButtonOptions.SetSelected();
				}
			}
			else if (_shopMenu.CurrentController != GameInput.ControllerTypeEnum.Mouse)
			{
				_shopMenu.PanelTopButtons.SelectButton(3, bPlaySound: false);
			}
		}
		Options.Save(DataUtils.GetGameIniPath());
		GameCore.PlaySound("menu_yesno_hide");
	}

	public void UpdateAudioVideoTab(float elapsedTime)
	{
		SliderMusicVolume.Update(elapsedTime);
		SliderSFXVolume.Update(elapsedTime);
		SliderSoundPanning.Update(elapsedTime);
		SliderLOD.Update(elapsedTime);
		SliderQuakeMode.Update(elapsedTime);
		SliderMaxFPS.Update(elapsedTime);
		ButtonFullscreen.Update(elapsedTime);
		ButtonVsync.Update(elapsedTime);
	}

	private bool IsAnyItemSelected()
	{
		if (!SliderMusicVolume.IsSelected && !SliderMusicVolume.IsDragSelected && !SliderSoundPanning.IsSelected && !SliderSoundPanning.IsDragSelected && !SliderSFXVolume.IsSelected && !SliderSFXVolume.IsDragSelected && !SliderLOD.IsSelected && !SliderLOD.IsDragSelected && !SliderQuakeMode.IsSelected && !SliderQuakeMode.IsDragSelected && !SliderMaxFPS.IsSelected && !SliderMaxFPS.IsDragSelected && !ButtonFullscreen.IsSelected && !ButtonClose.IsSelected && !ButtonReset.IsSelected && !ButtonVsync.IsSelected && !TabButtonAudioVideo.IsSelected && !TabButtonGame.IsSelected && !TabButtonControls.IsSelected && !TabGame.IsAnyItemSelected())
		{
			return TabControls.IsAnyItemSelected();
		}
		return true;
	}

	private void UnselectAll()
	{
		TabButtonAudioVideo.Unselect();
		TabButtonGame.Unselect();
		TabButtonControls.Unselect();
		SliderLOD.Unselect();
		SliderLOD.Deactivate();
		SliderQuakeMode.Unselect();
		SliderQuakeMode.Deactivate();
		SliderMaxFPS.Unselect();
		SliderMaxFPS.Deactivate();
		SliderSFXVolume.Unselect();
		SliderSFXVolume.Deactivate();
		SliderMusicVolume.Unselect();
		SliderMusicVolume.Deactivate();
		SliderSoundPanning.Unselect();
		SliderSoundPanning.Deactivate();
		ButtonClose.Unselect();
		ButtonFullscreen.Unselect();
		ButtonVsync.Unselect();
		TabGame.UnselectAll();
		TabControls.UnselectAll();
	}

	private void ParseInput()
	{
		if (TabControls.WaitingToButton == null && (GameInput.MousePressed(GameInput.MouseButton.Right) || (!GameCore.Console.IsActive && (_shopMenu.KeyPressed(ShopMenuKeys.CancelAll) || _shopMenu.KeyPressed(ShopMenuKeys.CancelSelection) || GameInput.SystemGPButtonPressedAll(Buttons.B)))))
		{
			GameInput.CancelKeyPressed();
			OnCloseButtonClick(null);
			return;
		}
		if (TabControls.WaitingToButton == null)
		{
			if (_shopMenu.KeyPressed(ShopMenuKeys.NextTab))
			{
				switch (CurrentTab)
				{
				case OptionsTabEnum.AudioVideo:
					TabButtonGame.Click();
					TabButtonGame.Select(bPlaySound: false);
					break;
				case OptionsTabEnum.Game:
					TabButtonControls.Click();
					TabButtonControls.Select(bPlaySound: false);
					break;
				case OptionsTabEnum.Controls:
					TabButtonAudioVideo.Click();
					TabButtonAudioVideo.Select(bPlaySound: false);
					break;
				}
			}
			else if (_shopMenu.KeyPressed(ShopMenuKeys.PreviousTab))
			{
				switch (CurrentTab)
				{
				case OptionsTabEnum.AudioVideo:
					TabButtonControls.Click();
					TabButtonControls.Select(bPlaySound: false);
					break;
				case OptionsTabEnum.Game:
					TabButtonAudioVideo.Click();
					TabButtonAudioVideo.Select(bPlaySound: false);
					break;
				case OptionsTabEnum.Controls:
					TabButtonGame.Click();
					TabButtonGame.Select(bPlaySound: false);
					break;
				}
			}
			if (_shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveLeft))
			{
				if (TabButtonGame.IsSelected)
				{
					TabButtonGame.Unselect();
					TabButtonAudioVideo.Select();
				}
				else if (TabButtonControls.IsSelected)
				{
					TabButtonControls.Unselect();
					TabButtonGame.Select();
				}
				else if (ButtonClose.IsSelected)
				{
					ButtonClose.Unselect();
					ButtonReset.Select();
				}
				else if (ButtonReset.IsSelected)
				{
					ButtonReset.Unselect();
					ButtonClose.Select();
				}
			}
			else if (_shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveRight))
			{
				if (TabButtonAudioVideo.IsSelected)
				{
					TabButtonAudioVideo.Unselect();
					TabButtonGame.Select();
				}
				else if (TabButtonGame.IsSelected)
				{
					TabButtonGame.Unselect();
					TabButtonControls.Select();
				}
				else if (ButtonReset.IsSelected)
				{
					ButtonReset.Unselect();
					ButtonClose.Select();
				}
				else if (ButtonClose.IsSelected)
				{
					ButtonClose.Unselect();
					ButtonReset.Select();
				}
			}
			else if (CursorTabButton != null && _shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveDown))
			{
				TabButtonGame.Unselect();
				TabButtonAudioVideo.Unselect();
				TabButtonControls.Unselect();
				if (CurrentTab == OptionsTabEnum.AudioVideo)
				{
					SliderSFXVolume.Select();
				}
				else if (CurrentTab == OptionsTabEnum.Game)
				{
					TabGame.SelectPlayerControlsButton(0);
				}
				else if (CurrentTab == OptionsTabEnum.Controls)
				{
					TabControls.CurrentControllerButton.Select();
				}
				_shopMenu.ShopInput.CancelKeyPressed();
			}
			else if (CursorTabButton != null && _shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveUp))
			{
				if (CursorTabButton == TabButtonAudioVideo)
				{
					ButtonReset.Select();
				}
				else
				{
					ButtonClose.Select();
				}
				TabButtonGame.Unselect();
				TabButtonAudioVideo.Unselect();
				TabButtonControls.Unselect();
				_shopMenu.ShopInput.CancelKeyPressed();
			}
			else if (ButtonReset.IsSelected && _shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveDown))
			{
				ButtonReset.Unselect();
				TabButtonAudioVideo.Select();
			}
			else if (ButtonClose.IsSelected && _shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveDown))
			{
				ButtonClose.Unselect();
				TabButtonControls.Select();
			}
		}
		if (CurrentTab == OptionsTabEnum.AudioVideo)
		{
			if (_shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveUp))
			{
				if (ButtonClose.IsSelected || ButtonReset.IsSelected)
				{
					ButtonClose.Unselect();
					ButtonReset.Unselect();
					ButtonVsync.Select();
				}
				else if (ButtonVsync.IsSelected)
				{
					ButtonVsync.Unselect();
					ButtonFullscreen.Select();
				}
				else if (ButtonFullscreen.IsSelected)
				{
					ButtonFullscreen.Unselect();
					SliderMaxFPS.Select();
				}
				else if (!SliderLOD.IsActivated && !SliderSFXVolume.IsActivated && !SliderMusicVolume.IsActivated && !SliderQuakeMode.IsActivated && !SliderMaxFPS.IsActivated && !SliderSoundPanning.IsActivated)
				{
					if (SliderMaxFPS.IsSelected)
					{
						SliderMaxFPS.Unselect();
						SliderQuakeMode.Select();
					}
					else if (SliderQuakeMode.IsSelected)
					{
						SliderQuakeMode.Unselect();
						SliderLOD.Select();
					}
					else if (SliderLOD.IsSelected)
					{
						SliderLOD.Unselect();
						SliderSoundPanning.Select();
					}
					else if (SliderSoundPanning.IsSelected)
					{
						SliderSoundPanning.Unselect();
						SliderMusicVolume.Select();
					}
					else if (SliderMusicVolume.IsSelected)
					{
						SliderMusicVolume.Unselect();
						SliderSFXVolume.Select();
					}
					else if (SliderSFXVolume.IsSelected)
					{
						SliderSFXVolume.Unselect();
						TabButtonAudioVideo.Select();
					}
				}
			}
			else if (_shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveDown))
			{
				if (ButtonFullscreen.IsSelected)
				{
					ButtonFullscreen.Unselect();
					ButtonVsync.Select();
				}
				else if (ButtonVsync.IsSelected)
				{
					ButtonVsync.Unselect();
					ButtonClose.Select();
				}
				else if (!SliderLOD.IsActivated && !SliderQuakeMode.IsActivated && !SliderSFXVolume.IsActivated && !SliderMusicVolume.IsActivated && !SliderMaxFPS.IsActivated && !SliderSoundPanning.IsActivated)
				{
					if (SliderSFXVolume.IsSelected)
					{
						SliderSFXVolume.Unselect();
						SliderMusicVolume.Select();
					}
					else if (SliderMusicVolume.IsSelected)
					{
						SliderMusicVolume.Unselect();
						SliderSoundPanning.Select();
					}
					else if (SliderSoundPanning.IsSelected)
					{
						SliderSoundPanning.Unselect();
						SliderLOD.Select();
					}
					else if (SliderLOD.IsSelected)
					{
						SliderLOD.Unselect();
						SliderQuakeMode.Select();
					}
					else if (SliderQuakeMode.IsSelected)
					{
						SliderQuakeMode.Unselect();
						SliderMaxFPS.Select();
					}
					else if (SliderMaxFPS.IsSelected)
					{
						SliderMaxFPS.Unselect();
						ButtonFullscreen.Select();
					}
				}
			}
		}
		if ((_shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveUp) || _shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveDown) || _shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveLeft) || _shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveRight)) && !IsAnyItemSelected())
		{
			ButtonClose.Select();
		}
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (!GameCore.IsShopMenuEnabled)
		{
			if (GameJnG.PopupMenuInLevel.State == ShopMenuState.Visible)
			{
				State = ShopMenuState.Hidden;
				return;
			}
			if (GameJnG.PopupMenuInLevel.State != ShopMenuState.Hidden)
			{
				_alpha = 255f;
			}
		}
		ParseInput();
		TabButtonAudioVideo.Update(elapsedTime);
		TabButtonGame.Update(elapsedTime);
		TabButtonControls.Update(elapsedTime);
		switch (CurrentTab)
		{
		case OptionsTabEnum.AudioVideo:
			UpdateAudioVideoTab(elapsedTime);
			break;
		case OptionsTabEnum.Game:
			TabGame.Update(elapsedTime);
			break;
		case OptionsTabEnum.Controls:
			TabControls.Update(elapsedTime);
			break;
		}
		SetFullscreenButtonCaption();
		SetVsyncButtonCaption();
		ButtonClose.Color = Color;
		ButtonClose.Update(elapsedTime);
		ButtonReset.Color = Color;
		ButtonReset.Update(elapsedTime);
	}

	public void DrawAudioVideoTab()
	{
		SliderMusicVolume.Draw();
		SliderSFXVolume.Draw();
		SliderSoundPanning.Draw();
		SliderLOD.Draw();
		SliderQuakeMode.Draw();
		SliderMaxFPS.Draw();
		ButtonFullscreen.Draw();
		ButtonVsync.Draw();
	}

	public override void Draw()
	{
		_shopMenu.SpriteBatchBegin(bLight: true);
		base.Draw();
		TabButtonGame.Draw();
		TabButtonAudioVideo.Draw();
		TabButtonControls.Draw();
		ButtonClose.Draw();
		ButtonReset.Draw();
		switch (CurrentTab)
		{
		case OptionsTabEnum.AudioVideo:
			DrawAudioVideoTab();
			break;
		case OptionsTabEnum.Game:
			TabGame.Draw();
			break;
		case OptionsTabEnum.Controls:
			TabControls.Draw();
			break;
		}
		_shopMenu.SpriteBatchEnd(bLight: true);
		if (_showControllerUnpluggedMessage)
		{
			_shopMenu.SpriteBatchBegin(bLight: false);
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("controls_controller_unplugged"), 960f, Position.Y - 400f, Vector2.One, new Color(255, 0, 0), TextAlign.Center, 0, (float)(int)Color.A / 255f);
			_shopMenu.SpriteBatchEnd(bLight: false);
		}
		if (!GameCore.IsShopMenuEnabled)
		{
			GameCore.EffectLightBasic.SetLightMaps(bUseShopMaps: false);
			GameCore.EffectLight.SetLightMaps(bUseShopMaps: false);
		}
	}
}
