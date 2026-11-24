using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class MainMenu
{
	public MainMenuStateEnum State;

	public GameLevel Level;

	public YesNoDialog YesNoDialogQuitGame;

	public NewGameWizard NewGameWizard;

	public ProfileSelector ProfileSelector;

	public ScreenLeaderboard ScreenLeaderboard;

	public SpriteAtlas AtlasRigLogo;

	public SpriteAtlas AtlasProfileSlot;

	public Hero[] MyHeroArray;

	public Credits Credits;

	public SaveGameInfo SwitchToSaveGameInfo;

	private ShopMenu _shopMenu;

	private Button _buttonContinue;

	private Button _buttonChooseProfile;

	private Button _buttonOptions;

	private Button _buttonQuit;

	private Button _buttonCredits;

	private Button _buttonLeaderboards;

	private float _fadeEffectAlpha;

	private float _targetFadeEffectAlpha;

	private float _hintLinetAlphaFactor;

	private float _targetHintLineAlphaFactor;

	private string _hintText;

	private float _buttonFadeTimer;

	private float _buttonFadeAlpha;

	private float _buttonTargetFadeAlpha;

	private float _rigLogoScaleFactor;

	private float _rigLogoRotatiomFactor;

	private GameObject _logoObject;

	private List<Button> _buttonList;

	private bool _isIntroStarted;

	private bool _isHeroStarted;

	private float _heroStartTimer;

	private bool _wasDefaultButtonSelected;

	private bool _wasInitialized;

	private Button _selectAfterFadeInButton;

	private WelcomeScreen _welcomeScreen;

	public Hero ReloadProfileByHero;

	public Cursor Cursor { get; private set; }

	private bool IsWelcomeScreeVisible
	{
		get
		{
			if (_welcomeScreen != null)
			{
				return _welcomeScreen.State != WelcomeScreen.StateEnum.Hidden;
			}
			return false;
		}
	}

	public bool IsAnyMainMenuScreenVisible
	{
		get
		{
			if (NewGameWizard.State == NewGameWizard.WizardStateEnum.Hidden && ProfileSelector.State == ProfileSelectorStateEnum.Hidden)
			{
				return ScreenLeaderboard.State != ScreenLeaderBoardStateEnum.Hidden;
			}
			return true;
		}
	}

	public MainMenu(ShopMenu shopMenu)
	{
		_shopMenu = shopMenu;
		Cursor = _shopMenu.Cursor;
		_buttonList = new List<Button>();
		_buttonContinue = new Button(_shopMenu, ShopButtonType.Big);
		_buttonContinue.CaptionScale = ShopMenu.FONT_SCALE_BUTTON_BIG;
		_buttonContinue.OnClick += OnContinueButtonClick;
		_buttonList.Add(_buttonContinue);
		_buttonChooseProfile = new Button(_shopMenu, ShopButtonType.Big);
		_buttonChooseProfile.CaptionScale = ShopMenu.FONT_SCALE_BUTTON_BIG;
		_buttonChooseProfile.OnClick += OnChooseProfileButtonClick;
		_buttonList.Add(_buttonChooseProfile);
		_buttonOptions = new Button(_shopMenu, ShopButtonType.Big);
		_buttonOptions.CaptionScale = ShopMenu.FONT_SCALE_BUTTON_BIG;
		_buttonOptions.OnClick += OnOptionsButtonClick;
		_buttonList.Add(_buttonOptions);
		_buttonLeaderboards = new Button(_shopMenu, ShopButtonType.Big);
		_buttonLeaderboards.CaptionScale = ShopMenu.FONT_SCALE_BUTTON_BIG;
		_buttonLeaderboards.OnClick += OnLeaderboardsButtonClick;
		_buttonLeaderboards.HasPressEffectEnabled = true;
		_buttonList.Add(_buttonLeaderboards);
		_buttonCredits = new Button(_shopMenu, ShopButtonType.Big);
		_buttonCredits.CaptionScale = ShopMenu.FONT_SCALE_BUTTON_BIG;
		_buttonCredits.OnClick += OnCreditsButtonClick;
		_buttonCredits.HasPressEffectEnabled = true;
		_buttonList.Add(_buttonCredits);
		_buttonQuit = new Button(_shopMenu, ShopButtonType.Big);
		_buttonQuit.CaptionScale = ShopMenu.FONT_SCALE_BUTTON_BIG;
		_buttonQuit.OnClick += OnQuitButtonClick;
		_buttonQuit.HasPressEffectEnabled = true;
		_buttonList.Add(_buttonQuit);
		YesNoDialogQuitGame = new YesNoDialog(_shopMenu, "yes_no_quit_game");
		YesNoDialogQuitGame.OnYesButtonPressed += OnQuitYesButtonPressed;
		YesNoDialogQuitGame.OnNoButtonPressed += OnQuitNoButtonPressed;
		NewGameWizard = new NewGameWizard(_shopMenu, this);
		ProfileSelector = new ProfileSelector(_shopMenu, this);
		ScreenLeaderboard = new ScreenLeaderboard(_shopMenu, this);
		AtlasRigLogo = GameCore.GetSpriteAtlas("rig_logo", logWhenNotFound: true);
		AtlasProfileSlot = GameCore.GetSpriteAtlas("profile_slot", logWhenNotFound: true);
		Credits = new Credits(this, _shopMenu.FontDefault);
	}

	private void OnContinueButtonClick(Button button)
	{
		if (!button.IsDisabled)
		{
			Hide();
		}
	}

	private void OnChooseProfileButtonClick(Button button)
	{
		ProfileSelector.Show();
		Hero[] myHeroArray = MyHeroArray;
		for (int i = 0; i < myHeroArray.Length; i++)
		{
			myHeroArray[i].StartAutopilotFlyOutMainMenu();
		}
		button.Unselect();
	}

	private void OnModsButtonClick(Button button)
	{
	}

	private void OnCreditsButtonClick(Button button)
	{
		Credits.Show();
		_buttonCredits.Unselect();
		_selectAfterFadeInButton = _buttonCredits;
		Hero[] myHeroArray = MyHeroArray;
		for (int i = 0; i < myHeroArray.Length; i++)
		{
			myHeroArray[i].StartAutopilotFlyOutMainMenu();
		}
		GameCore.PlaySound("menu_yesno_show");
	}

	private void OnOptionsButtonClick(Button button)
	{
		button.Unselect();
		GameJnG.PopupMenuOptions.Show();
	}

	private void OnQuitButtonClick(Button button)
	{
		button.Unselect();
		YesNoDialogQuitGame.Show();
	}

	private void OnWebPageButtonClick(Button button)
	{
		GameCore.SteamManager.ShowWebPage("https://steamcommunity.com/app/830820/discussions/", "http://www.rakeingrass.com/contact.php");
	}

	private void OnQuitYesButtonPressed()
	{
		_shopMenu.GameCore.GameExit();
	}

	private void OnQuitNoButtonPressed()
	{
		if (_shopMenu.CurrentController != GameInput.ControllerTypeEnum.Mouse)
		{
			SelectButton(5);
		}
	}

	private void OnLeaderboardsButtonClick(Button button)
	{
		Hero[] myHeroArray = MyHeroArray;
		for (int i = 0; i < myHeroArray.Length; i++)
		{
			myHeroArray[i].StartAutopilotFlyOutMainMenu();
		}
		GameCore.PlaySound("menu_yesno_show");
		ScreenLeaderboard.Show(GameJnG.GameMode, GameJnG.PlayerCount, bFadeAll: true);
		_selectAfterFadeInButton = _buttonLeaderboards;
	}

	public void SetTexts()
	{
		if (!GameJnG.Mia && (GameJnG.IsFirstLevelTry || GameJnG.SaveGameNumber <= 0))
		{
			_buttonContinue.Caption = TextBundle.GetText("button_new_game");
		}
		else
		{
			_buttonContinue.Caption = TextBundle.GetText("button_continue_game");
		}
		_buttonChooseProfile.Caption = TextBundle.GetText("button_choose_profile");
		_buttonOptions.Caption = TextBundle.GetText("button_options");
		_buttonQuit.Caption = TextBundle.GetText("button_quit_game");
		_buttonCredits.Caption = TextBundle.GetText("button_credits");
		_buttonLeaderboards.Caption = TextBundle.GetText("button_leaderboards");
		YesNoDialogQuitGame.SetTexts();
		NewGameWizard.SetTexts();
		ProfileSelector.SetTexts();
		ScreenLeaderboard.SetTexts();
	}

	public void SetHintText(string textId)
	{
		_hintText = TextBundle.GetText(textId);
		_targetHintLineAlphaFactor = 1f;
	}

	public void Show()
	{
		GameCore.SoundSystem.StopAll();
		GameCore.SetGCPhase(GCPhase.Loading);
		Level = new GameLevel();
		Level.Init();
		GameCore.GameLevel = Level;
		if (!Level.LoadFromFile("level/shop/main_menu/level.txt"))
		{
			GameCore.LogErr("Unable to load main menu level");
		}
		GameCore.CurrentLevel = Level;
		Level.HeroList = GameJnG.HeroList;
		if (GameJnG.HeroList != null && GameJnG.HeroList.Count >= 1)
		{
			MyHeroArray = new Hero[GameJnG.HeroList.Count];
			GameJnG.HeroList.CopyTo(MyHeroArray);
		}
		Level.Begin();
		GameCore.UpdateHeroes = true;
		GameCore.SetGCPhase(GCPhase.Menu);
		Layer layerById = Level.GetLayerById("logo");
		_logoObject = layerById.GameObjectList.FirstOrDefault((GameObject go) => Utils.Equals(go.Id, "jng2_logo"));
		State = MainMenuStateEnum.Visible;
		ScreenLeaderboard.State = ScreenLeaderBoardStateEnum.Hidden;
		_targetFadeEffectAlpha = 0f;
		_fadeEffectAlpha = 0f;
		_hintLinetAlphaFactor = 0f;
		_buttonFadeTimer = 1f;
		_buttonTargetFadeAlpha = 255f;
		_hintText = null;
		SwitchToSaveGameInfo = null;
		ReloadProfileByHero = null;
		_isIntroStarted = false;
		_isHeroStarted = false;
		_heroStartTimer = 2f;
		_shopMenu.GameCore.Pause(paused: false);
		UnselectAll();
		_wasDefaultButtonSelected = false;
		_wasInitialized = false;
		GameJnG.IsFirstLevelTry = GameJnG.GetIsFirstLevelTry();
		GameJnG.IsDrunkenEffectEnabled = false;
		SetTexts();
	}

	public void ShowWelcomeScreen()
	{
		if (_welcomeScreen == null)
		{
			_welcomeScreen = new WelcomeScreen(_shopMenu);
		}
		_welcomeScreen.Show();
	}

	private void UnselectAll()
	{
		foreach (Button button in _buttonList)
		{
			button.Unselect();
		}
	}

	public void Hide()
	{
		SetState(MainMenuStateEnum.Hiding);
		_targetFadeEffectAlpha = 1f;
		_buttonFadeTimer = 0f;
		_buttonTargetFadeAlpha = 0f;
		Hero[] myHeroArray = MyHeroArray;
		for (int i = 0; i < myHeroArray.Length; i++)
		{
			myHeroArray[i].StartAutopilotFlyOutMainMenu();
		}
	}

	public void SelectButton(int index, bool bPlaySound = true)
	{
		for (int i = 0; i < _buttonList.Count; i++)
		{
			if (i == index)
			{
				_buttonList[i].Select(bPlaySound);
			}
			else
			{
				_buttonList[i].Unselect();
			}
		}
	}

	private void ParseInput()
	{
		bool flag = false;
		GameJnG.CheckControlerUnplugged(bInformPlayer: true);
		if (_shopMenu.KeyPressed(ShopMenuKeys.CancelAll))
		{
			_buttonOptions.Click();
		}
		if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp))
		{
			for (int i = 0; i < _buttonList.Count; i++)
			{
				Button button = _buttonList[i];
				if (button.IsSelected)
				{
					button.Unselect();
					if (i == 0)
					{
						_buttonList[_buttonList.Count - 1].Select();
					}
					else
					{
						_buttonList[i - 1].Select();
					}
					flag = true;
					break;
				}
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveDown))
		{
			for (int j = 0; j < _buttonList.Count; j++)
			{
				Button button2 = _buttonList[j];
				if (button2.IsSelected)
				{
					button2.Unselect();
					if (j < _buttonList.Count - 1)
					{
						_buttonList[j + 1].Select();
					}
					else
					{
						_buttonList[0].Select();
					}
					flag = true;
					break;
				}
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRight))
		{
			for (int k = 0; k < 3; k++)
			{
				Button button3 = _buttonList[k];
				if (button3.IsSelected)
				{
					button3.Unselect();
					_buttonList[k + 3].Select();
					flag = true;
					break;
				}
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeft))
		{
			for (int l = 3; l < _buttonList.Count; l++)
			{
				Button button4 = _buttonList[l];
				if (button4.IsSelected)
				{
					button4.Unselect();
					_buttonList[l - 3].Select();
					flag = true;
					break;
				}
			}
		}
		if (!flag && !IsAnyItemSelected())
		{
			if (_shopMenu.KeyPressed(ShopMenuKeys.MoveDown, bOnlyOnce: false) || _shopMenu.KeyPressed(ShopMenuKeys.MoveLeft, bOnlyOnce: false) || _shopMenu.KeyPressed(ShopMenuKeys.MoveRight, bOnlyOnce: false))
			{
				_buttonContinue.Select();
			}
			else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp, bOnlyOnce: false))
			{
				_buttonQuit.Select();
			}
		}
	}

	public bool IsAnyItemSelected()
	{
		foreach (Button button in _buttonList)
		{
			if (button.IsSelected)
			{
				return true;
			}
		}
		return false;
	}

	public void ReloadHeroProfile(int number)
	{
		Vector2[] array = new Vector2[GameJnG.HeroList.Count];
		for (int i = 0; i < GameJnG.HeroList.Count; i++)
		{
			array[i] = ((GameJnG.HeroList[i].MyObject != null) ? GameJnG.HeroList[i].MyObject.Position : new Vector2(0f, 0f));
		}
		SaveGame.Load(number, bFromCloud: true);
		MyHeroArray = new Hero[GameJnG.HeroList.Count];
		for (int j = 0; j < GameJnG.HeroList.Count; j++)
		{
			Hero hero = GameJnG.HeroList[j];
			MyHeroArray[j] = hero;
			hero.RebuildShipConfiguration();
			Vector2 position = ((array.Length > j) ? array[j] : new Vector2(0f, 0f));
			hero.CreateSmallShip(Level.MainLayer, position);
			hero.StartAutopilotMainMenu();
		}
	}

	public void SetState(MainMenuStateEnum state)
	{
		if (State != state)
		{
			State = state;
			if (state == MainMenuStateEnum.Hidden && !GameCore.IsEditorEnabled)
			{
				Level.ClearAll();
				Level = null;
			}
		}
	}

	public void Update(float elapsedTime)
	{
		_shopMenu.CursorType = CursorTypeEnum.NoAction;
		_targetHintLineAlphaFactor = 0f;
		if (SwitchToSaveGameInfo != null)
		{
			ReloadHeroProfile(SwitchToSaveGameInfo.Number);
			SwitchToSaveGameInfo = null;
		}
		if (ReloadProfileByHero != null)
		{
			ReloadHeroProfile(GameJnG.SaveGameNumber);
			ReloadProfileByHero = null;
		}
		else
		{
			MyHeroArray = new Hero[GameJnG.HeroList.Count];
			GameJnG.HeroList.CopyTo(MyHeroArray);
		}
		if (IsWelcomeScreeVisible)
		{
			Cursor.Update(elapsedTime);
			_shopMenu.ShopInput.Update(elapsedTime);
			_welcomeScreen.Update(elapsedTime);
			Hero[] myHeroArray = MyHeroArray;
			for (int i = 0; i < myHeroArray.Length; i++)
			{
				myHeroArray[i].MyObject?.SetPosition(-300f, 540f);
			}
			return;
		}
		GameLevel gameLevel = GameCore.GameLevel;
		GameCore.GameLevel = Level;
		Level.Camera.SetViewPort(GameCore.MainViewPort);
		Level.Update(elapsedTime);
		GameCore.GameLevel = gameLevel;
		GameCore.UseShopLightMaps = true;
		_shopMenu.ShopInput.Update(elapsedTime);
		_shopMenu.OnlyOneKeyPressedProtection = false;
		if (GameJnG.PopupMenuOptions.State != ShopMenuState.Hidden)
		{
			GameJnG.PopupMenuOptions.Update(elapsedTime);
		}
		Cursor.Update(elapsedTime);
		if (!_wasInitialized)
		{
			Cursor.IsPositionChanged = false;
			_shopMenu.CurrentController = GameInput.ControllerTypeEnum.GamePad;
			_wasInitialized = true;
		}
		if (Credits.State == CreditsStateEnum.Visible || ProfileSelector.State != ProfileSelectorStateEnum.Hidden || ScreenLeaderboard.State != ScreenLeaderBoardStateEnum.Hidden)
		{
			Utils.ChangeValueToTarget(ref _buttonFadeAlpha, 0f, 700f * elapsedTime);
		}
		else if (Utils.ChangeValueToTarget(ref _buttonFadeTimer, 0f, elapsedTime) && _isHeroStarted && Utils.ChangeValueToTarget(ref _buttonFadeAlpha, _buttonTargetFadeAlpha, 500f * elapsedTime))
		{
			if (_selectAfterFadeInButton != null)
			{
				_selectAfterFadeInButton.Select(bPlaySound: false);
				_selectAfterFadeInButton = null;
			}
			else if (!_wasDefaultButtonSelected && _buttonTargetFadeAlpha > 0f)
			{
				if (!_buttonContinue.IsDisabled)
				{
					_buttonContinue.Select(bPlaySound: false);
				}
				else
				{
					_buttonChooseProfile.Select(bPlaySound: false);
				}
				_wasDefaultButtonSelected = true;
			}
		}
		if (_buttonFadeAlpha > 0f && !GameCore.Console.IsActive && YesNoDialogQuitGame.State == ShopMenuState.Hidden && !GameJnG.ShopMenu.IsAnyPopupMenuVisible && !IsAnyMainMenuScreenVisible)
		{
			ParseInput();
		}
		for (int j = 0; j < _buttonList.Count; j++)
		{
			Button button = _buttonList[j];
			button.Alpha = (byte)_buttonFadeAlpha;
			if (button.Alpha < byte.MaxValue || _shopMenu.IsAnyPopupMenuVisible || IsAnyMainMenuScreenVisible)
			{
				button.IsInputEnabled = false;
			}
			else
			{
				button.IsInputEnabled = true;
			}
			if (j == 6)
			{
				button.Position = new Vector2(960f, 770 + (j - 3) * 55);
			}
			else if (j < 3)
			{
				button.Position = new Vector2(838f, 770 + j * 55);
			}
			else
			{
				button.Position = new Vector2(1082f, 770 + (j - 3) * 55);
			}
			button.Update(elapsedTime);
		}
		_buttonContinue.IsDisabled = GameJnG.SaveGameNumber <= 0;
		if (Utils.ChangeValueToTarget(ref _fadeEffectAlpha, _targetFadeEffectAlpha, 3f * elapsedTime) && State == MainMenuStateEnum.Hiding)
		{
			SetState(MainMenuStateEnum.Hidden);
			_buttonFadeAlpha = 0f;
			if (!GameJnG.Mia && GameJnG.IsFirstLevelTry)
			{
				if (GameJnG.GameMode == GameModeEnum.HardCore)
				{
					GameJnG.ShopOffer.Build(GameJnG.StageNumber, 999999);
					GameJnG.LoadingScreen.Show(null, fadeToShop: true);
				}
				else
				{
					GameJnG.LoadingScreen.Show(GameJnG.Campaign.StageDataList[0].LevelDataList[0], fadeToShop: false);
				}
			}
			else
			{
				GameJnG.LoadingScreen.Show(null, fadeToShop: true);
			}
		}
		if (YesNoDialogQuitGame.State != ShopMenuState.Hidden)
		{
			YesNoDialogQuitGame.Update(elapsedTime);
		}
		if (NewGameWizard.State != NewGameWizard.WizardStateEnum.Hidden)
		{
			NewGameWizard.Update(elapsedTime);
		}
		if (ProfileSelector.State != ProfileSelectorStateEnum.Hidden)
		{
			ProfileSelector.Update(elapsedTime);
		}
		if (ScreenLeaderboard.State != ScreenLeaderBoardStateEnum.Hidden)
		{
			ScreenLeaderboard.Update(elapsedTime);
		}
		if (Credits.State != CreditsStateEnum.Hidden)
		{
			Cursor.SetCursorType(CursorTypeEnum.Unselect);
		}
		else
		{
			Cursor.SetCursorType(_shopMenu.CursorType);
		}
		if (Credits.State != CreditsStateEnum.Hidden)
		{
			_targetHintLineAlphaFactor = 0f;
		}
		else if (_buttonContinue.IsSelected)
		{
			if (GameJnG.SaveGameNumber <= 0)
			{
				SetHintText("hint_main_new_game_disabled");
			}
			else if (!GameJnG.Mia && GameJnG.IsFirstLevelTry)
			{
				SetHintText("hint_main_new_game");
			}
			else
			{
				SetHintText("hint_main_continue");
			}
		}
		else if (_buttonChooseProfile.IsSelected)
		{
			SetHintText("hint_main_choose_profile");
		}
		else if (_buttonOptions.IsSelected)
		{
			SetHintText("hint_main_options");
		}
		else if (_buttonQuit.IsSelected)
		{
			SetHintText("hint_main_quit");
		}
		else if (_buttonCredits.IsSelected)
		{
			SetHintText("hint_main_credits");
		}
		else if (_buttonLeaderboards.IsSelected && ScreenLeaderboard.State == ScreenLeaderBoardStateEnum.Hidden)
		{
			SetHintText("hint_main_leaderboards");
		}
		if (State != MainMenuStateEnum.Hidden)
		{
			Utils.ChangeValueToTarget(ref _hintLinetAlphaFactor, _targetHintLineAlphaFactor, 4f * elapsedTime);
		}
		_rigLogoScaleFactor += elapsedTime;
		_rigLogoRotatiomFactor += 1.5f * elapsedTime;
		if (Credits.State != CreditsStateEnum.Hidden)
		{
			Credits.Update(elapsedTime);
		}
		else if (GameJnG.SaveGameNumber <= 0)
		{
			Hero[] myHeroArray = MyHeroArray;
			for (int i = 0; i < myHeroArray.Length; i++)
			{
				myHeroArray[i].DestroySmallShip();
			}
		}
		else
		{
			Hero[] myHeroArray = MyHeroArray;
			foreach (Hero hero in myHeroArray)
			{
				if (hero.MyObject == null || hero.MyObject.IsDestroyed)
				{
					hero.RecreateSmallShip = true;
				}
			}
		}
		_shopMenu.RenderLightMaps();
		if (GameCore.IsFadeEffectDone && !_isIntroStarted)
		{
			_isIntroStarted = true;
			_logoObject.ChangePartState("intro");
			_logoObject.Update(0f);
		}
		if (ScreenLeaderboard.State != ScreenLeaderBoardStateEnum.Hidden && (Utils.Equals(_logoObject.CurrentState.Id, "fade_in") || Utils.Equals(_logoObject.CurrentState.Id, "idle")))
		{
			_logoObject.ChangePartState("fade_out");
			_logoObject.Update(0f);
		}
		else if (ScreenLeaderboard.State == ScreenLeaderBoardStateEnum.Hidden && (Utils.Equals(_logoObject.CurrentState.Id, "fade_out") || Utils.Equals(_logoObject.CurrentState.Id, "disabled")))
		{
			_logoObject.ChangePartState("fade_in");
			_logoObject.Update(0f);
		}
		if (_isHeroStarted)
		{
			return;
		}
		if (GameCore.IsFadeEffectDone && Utils.ChangeValueToTarget(ref _heroStartTimer, 0f, elapsedTime))
		{
			Hero[] myHeroArray = MyHeroArray;
			for (int i = 0; i < myHeroArray.Length; i++)
			{
				myHeroArray[i].StartAutopilotMainMenu();
			}
			_isHeroStarted = true;
		}
		else
		{
			Hero[] myHeroArray = MyHeroArray;
			for (int i = 0; i < myHeroArray.Length; i++)
			{
				myHeroArray[i].MyObject?.SetPosition(-250f, 540f);
			}
		}
	}

	private void DrawHintLine()
	{
		if (_hintLinetAlphaFactor > 0f)
		{
			float y = 1020f;
			_shopMenu.SpriteBatchBegin(bLight: false);
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, _hintText, 960f, y, ShopMenu.FONT_SCALE_TEXT_HINT, null, TextAlign.Center, 0, _hintLinetAlphaFactor);
			_shopMenu.SpriteBatchEnd(bLight: false);
		}
	}

	private void DrawRigLogo()
	{
		Color color = new Color(255, 255, 255, (int)_buttonFadeAlpha);
		_shopMenu.SpriteBatchBegin(bLight: false);
		string text = TextBundle.GetText("main_menu_created_by") + " ";
		float x = (_shopMenu.FontDefault.MeasureFont(text) * ShopMenu.FONT_SCALE_TEXT_NORMAL).X;
		float num = AtlasRigLogo.GetFrameRectangle(0).Width;
		Transformation transformation = Transformation.CreateByPosition(1920f - x - num - 150f, 930f);
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, text, transformation.Position.X, transformation.Position.Y, ShopMenu.FONT_SCALE_TEXT_NORMAL, null, TextAlign.Left, 0, (float)(int)color.A / 255f);
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, $"\\4Version {GameCore.APP_VERSION}", transformation.Position.X + (x + num) / 2f, transformation.Position.Y + 30f, ShopMenu.FONT_SCALE_TEXT_NORMAL, null, TextAlign.Center, 0, (float)(int)color.A / 255f);
		transformation.Position += new Vector2(x + num / 2f, 7f);
		transformation.Scale = new Vector2(1f - 0.05f * Utils.Sin(_rigLogoScaleFactor));
		transformation.Rotation = 0.05f * Utils.Sin(_rigLogoRotatiomFactor);
		AtlasRigLogo.DrawFrame(_shopMenu.SpriteBatchNoLight, 0, color, transformation, BlendModeEnum.Normal);
		_shopMenu.SpriteBatchEnd(bLight: false);
	}

	public void Draw()
	{
		if (IsWelcomeScreeVisible)
		{
			GameCore.GraphicsDeviceStatic.SetRenderTarget(GameCore.ScreenRenderTarget);
			_shopMenu.SpriteBatchBegin(bLight: false);
			_welcomeScreen.Draw();
			_shopMenu.SpriteBatchEnd(bLight: false);
			if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
			{
				Cursor.Draw();
				Cursor.DrawSymbol(Cursor.Position + new Vector2(40f, 38f));
			}
			return;
		}
		GameLevel gameLevel = GameCore.GameLevel;
		GameCore.GameLevel = Level;
		GameCore.GraphicsDeviceStatic.SetRenderTarget(GameCore.LevelRenderTarget);
		Level.Draw();
		GameCore.EffectLight.SetLightMaps(bUseShopMaps: true);
		GameCore.GraphicsDeviceStatic.SetRenderTarget(GameCore.ScreenRenderTarget);
		GameCore.EffectLight.LayerBeginDraw(_shopMenu.LayerShop);
		if (MyHeroArray != null && MyHeroArray.Length != 0 && MyHeroArray[0] != null && MyHeroArray[0].MyObject != null)
		{
			string text = null;
			text = ((MyHeroArray.Length != 1) ? string.Format("\0{0}: \\3{1} & {2}, \\0{3}: \\3{4} - {5}, \\0{6}: \\3{7}", TextBundle.GetText("main_menu_players"), MyHeroArray[0].PlayerName, MyHeroArray[1].PlayerName, TextBundle.GetDifficultyLabel(GameJnG.GameMode), GameJnG.Difficulty, TextBundle.GetDifficultyName(GameJnG.Difficulty), TextBundle.GetText("hero_stat_score"), Utils.FormatMoney(TotalStats.Score)) : string.Format("\0{0}: \\3{1}, \\0{2}: \\3{3} - {4}, \\0{5}: \\3{6}", TextBundle.GetText("main_menu_player"), MyHeroArray[0].PlayerName, TextBundle.GetDifficultyLabel(GameJnG.GameMode), GameJnG.Difficulty, TextBundle.GetDifficultyName(GameJnG.Difficulty), TextBundle.GetText("hero_stat_score"), Utils.FormatMoney(TotalStats.Score)));
			_shopMenu.SpriteBatchBegin(bLight: false);
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, text, 960f, _buttonContinue.Position.Y - 100f, ShopMenu.FONT_SCALE_TEXT_HINT, null, TextAlign.Center, 0, _buttonFadeAlpha / 255f);
			if (GameJnG.Mia)
			{
				Transformation transformation = Transformation.CreateByPosition(960f, _buttonContinue.Position.Y - 124f);
				_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("{0}", TextBundle.GetText("hero_profile_mia")), transformation.Position.X, transformation.Position.Y - 9.5f * Options.FontScale, ShopMenu.FONT_SCALE_TEXT_HINT, new Color(255, 0, 0), TextAlign.Center, 0, _buttonFadeAlpha / 255f, -1f, BlendModeEnum.Additive);
				ProfileSelector.AtlasFrame.DrawFrame(_shopMenu.SpriteBatchNoLight, 1, new Color(255f, 255f, 255f, _buttonFadeAlpha / 255f), transformation, BlendModeEnum.Additive);
			}
			_shopMenu.SpriteBatchEnd(bLight: false);
		}
		_shopMenu.SpriteBatchBegin(bLight: true);
		GameCore.EffectLight.SetLightFactor(0f, 1f, 0f);
		foreach (Button button in _buttonList)
		{
			button.Draw();
		}
		_shopMenu.SpriteBatchEnd(bLight: true);
		DrawRigLogo();
		if (ProfileSelector.State != ProfileSelectorStateEnum.Hidden)
		{
			ProfileSelector.Draw();
		}
		if (ScreenLeaderboard.State != ScreenLeaderBoardStateEnum.Hidden)
		{
			ScreenLeaderboard.Draw();
		}
		DrawHintLine();
		if (YesNoDialogQuitGame.State != ShopMenuState.Hidden)
		{
			_shopMenu.GameCore.DrawDarkBackground((float)(int)YesNoDialogQuitGame.Alpha / 350f);
			YesNoDialogQuitGame.Draw();
		}
		if (NewGameWizard.State != NewGameWizard.WizardStateEnum.Hidden)
		{
			_shopMenu.GameCore.DrawDarkBackground((float)(int)NewGameWizard.Alpha / 350f);
			NewGameWizard.Draw();
		}
		if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
		{
			Cursor.Draw();
			Cursor.DrawSymbol(Cursor.Position + new Vector2(40f, 38f));
		}
		if (_fadeEffectAlpha > 0f)
		{
			GameCore.TriangleBatch.Begin(BlendState.NonPremultiplied, GameCore.ViewPortMatrix);
			GameCore.TriangleBatch.DrawRect(new Vector2(0f, 0f), new Vector2(1920f, 1080f), new Color(0f, 0f, 0f, _fadeEffectAlpha));
			GameCore.TriangleBatch.End();
		}
		GameCore.GameLevel = gameLevel;
		GameCore.EffectLightBasic.SetLightMaps(bUseShopMaps: false);
		GameCore.EffectLight.SetLightMaps(bUseShopMaps: false);
	}
}
