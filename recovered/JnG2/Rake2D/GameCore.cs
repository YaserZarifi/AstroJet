using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using FarseerPhysics;
using FarseerPhysics.DebugView;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Rake2D.Debug;
using Rake2D.Editor;
using Rake2D.Font.BMFont;
using Rake2D.JnG;
using Rake2D.Menu;
using Rake2D.Sound;
using Rake2D.Spine;
using Rake2D.Steam;

namespace Rake2D;

public class GameCore : Game
{
	public static string APP_NAME = "Jets'n'Guns 2";

	public static string APP_VERSION = "1.0";

	public static string LEVEL_DIR = "level";

	public const int MAX_PLAYERS = 2;

	public const int MAX_LIGHT_MAP = 3;

	public const string GAME_FILE_NAME = "game.txt";

	public const string GL_DX = "GL";

	public const float VIRTUAL_SCREEN_WIDTH = 1920f;

	public const float VIRTUAL_SCREEN_HEIGHT = 1080f;

	internal DebugViewXNA PhysDebugView;

	public static float ElapsedTime;

	public static Allocator Allocator;

	public static GraphicsDeviceManager GraphicsDeviceManager;

	public static SpriteBatch SpriteBatch;

	public static SpriteBatch SpriteBatchDistortion;

	public static TriangleBatch TriangleBatch;

	public static TriangleBatchTextured TriangleBatchTextured;

	public static GameContent GlobalContent;

	public static Viewport MainViewPort;

	public static Texture2D DefaultTexture;

	public static EffectLight EffectLight;

	public static EffectLight EffectParticle;

	public static EffectLightBasic EffectLightBasic;

	public static EffectDistortion EffectDistortion;

	public static Rake2D.Editor.Editor Editor;

	public static Effect CurrentEffect;

	public static RasterizerState RasterizerState;

	public static Vector2 ScreenCenterOffset;

	public static Vector2 VirtualAspectRatio;

	public static GameLevel CurrentLevel;

	public static GameLevel GameLevel;

	public static TextureCache TextureCache;

	public static ISoundSystem SoundSystem;

	public static SteamManager SteamManager;

	public static GameInput.ControllerTypeEnum CurrentController;

	public static int ErrorCount = 0;

	public static bool ForceUpdateBlendState;

	public static float FrameTime;

	public static Rake2D.Debug.Console Console;

	public static BMFont FontNoBorder;

	public static bool IsLightRenderingEnabled;

	public static bool UseRenderTarget = true;

	public static bool UseShopLightMaps;

	public static bool IsMainViewPortInitialized;

	public static bool IsEditorEnabled;

	private static bool _updateHeroes = true;

	public static bool IsPaused = false;

	public static bool IgnoreDestroyOnRightEdge;

	public static LightMap[] LightMapArray;

	public static LightMap[] ShadowMapArray;

	public static SkeletonMeshRenderer SpineSkeletonRenderer;

	public static float TimeFactor = 1f;

	public static float TargetTimeFactor = TimeFactor;

	public static float TimeFactorChangeSpeed = 2f;

	public static Matrix ViewPortMatrix;

	public static RenderTarget2D LevelRenderTarget;

	public static RenderTarget2D ScreenRenderTarget;

	public static Rectangle ScissorRectangle;

	public static int MaxParticleCountPerLayer;

	private int _coreInitState;

	private Stopwatch _loopStopwatch;

	private static Logger _logger;

	private static BitmapFont _fontDebug;

	private static Matrix _projectionMatrix;

	private static float _fadeEffectAlpha;

	private static float _targetFadeEffectAlpha;

	private static bool _fadeEffectFirstFrame;

	private static float _pressKeyAlphaFactor;

	private bool _wasPausedOnDeactivate;

	private static bool _togglingScreen;

	private int _exitCode;

	public static GraphicsDevice GraphicsDeviceStatic => GraphicsDeviceManager.GraphicsDevice;

	public static bool IsApplicationActive { get; private set; }

	public static bool IsMainMenuEnabled => GameJnG.MainMenu.State != MainMenuStateEnum.Hidden;

	public static bool IsShopMenuEnabled
	{
		get
		{
			if (GameJnG.ShopMenu != null)
			{
				return GameJnG.ShopMenu.IsEnabled;
			}
			return false;
		}
	}

	public static bool IsInLevelMenuEnabled
	{
		get
		{
			if (GameJnG.PopupMenuInLevel.State == ShopMenuState.Hidden)
			{
				if (GameJnG.PopupMenuOptions.State != ShopMenuState.Hidden)
				{
					return !IsShopMenuEnabled;
				}
				return false;
			}
			return true;
		}
	}

	public static bool UpdateHeroes
	{
		get
		{
			return _updateHeroes;
		}
		set
		{
			if (!value && _updateHeroes)
			{
				foreach (Hero hero in GameJnG.HeroList)
				{
					hero.DestroyRigidBody();
				}
				GameLevel?.PhysWorld.ProcessChanges();
			}
			_updateHeroes = value;
		}
	}

	public static bool SkipExpiredObjects
	{
		get
		{
			if (!IsEditorEnabled)
			{
				return Options.SkipExpiredObjects;
			}
			return false;
		}
	}

	public FrameCounter FpsCounter { get; private set; }

	public int MaxFPS { get; private set; }

	public static Matrix LevelMatrix
	{
		get
		{
			if (CurrentLevel.CurrentLayer != null)
			{
				return CurrentLevel.Camera.WorldMatrixCurrentLayer;
			}
			return ViewPortMatrix;
		}
	}

	public static bool IgnoreDestroyOnEdge { get; internal set; }

	public static bool IsFadeEffectDone => _fadeEffectAlpha == 0f;

	public GameCore()
	{
		_logger = new Logger();
		Log("App: {0}", APP_NAME);
		Log("Ver: {0}", APP_VERSION);
		Log("DotNet: {0}", Utils.GetDotNetVersion());
		Options.IsNewVersionInstalled = true;
		SetGCPhase(GCPhase.Construct);
		Console = new Rake2D.Debug.Console(this);
		Options.SetDefaults();
		string gameIniPath = DataUtils.GetGameIniPath();
		if (File.Exists(gameIniPath))
		{
			Options.Load(gameIniPath);
		}
		else
		{
			Log("Using default option values.");
		}
		gameIniPath = Path.Combine(DataUtils.GetAppDataDirectory(), "debug.ini");
		if (File.Exists(gameIniPath))
		{
			Options.Load(gameIniPath);
		}
		Log("Options loaded OK");
		try
		{
			GameContent.LoadContentArchiveList();
			Log("Content OK");
		}
		catch (Exception ex)
		{
			LogErr("Problem with loading content. Ex: {0}", ex);
		}
		Log("Ciphering init");
		StringCipher.SetPassword("arnold");
		SoundSystem = new SoundDataBass.SoundSystemBass();
		Log("Sound system OK");
		IsEditorEnabled = Options.StartInEditor > 0;
		GraphicsDeviceManager = new GraphicsDeviceManager(this);
		GraphicsDeviceManager.PreferMultiSampling = false;
		GraphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
		GraphicsDeviceManager.SynchronizeWithVerticalRetrace = Options.VSync;
		GraphicsDeviceManager.HardwareModeSwitch = Options.HwFullScreen;
		GraphicsDeviceManager.IsFullScreen = false;
		if (Options.ScreenResolutionWidth >= 0)
		{
			GraphicsDeviceManager.PreferredBackBufferWidth = Options.ScreenResolutionWidth;
		}
		else
		{
			GraphicsDeviceManager.PreferredBackBufferWidth = 1920;
		}
		if (Options.ScreenResolutionHeight >= 0)
		{
			GraphicsDeviceManager.PreferredBackBufferHeight = Options.ScreenResolutionHeight;
		}
		else
		{
			GraphicsDeviceManager.PreferredBackBufferHeight = 1080;
		}
		Window.AllowUserResizing = true;
		Window.IsBorderless = false;
		SetMaxFPS();
		TextureCache = new TextureCache(80);
		Log("Texture cache OK");
		ScissorRectangle = default;
		GameLevel = new GameLevel();
		CurrentLevel = GameLevel;
		FpsCounter = new FrameCounter();
		_loopStopwatch = new Stopwatch();
		Log("Core OK");
	}

	public void SetMaxFPS()
	{
		IsFixedTimeStep = false;
		TargetElapsedTime = TimeSpan.FromMilliseconds(1.0);
		if (IsPaused)
		{
			MaxFPS = ((Options.MaxFPS > 0) ? Math.Min(30, Options.MaxFPS) : 30);
		}
		else if (Options.MaxFPS > 0)
		{
			Options.MaxFPS = Math.Max(10, Options.MaxFPS);
			MaxFPS = Options.MaxFPS;
		}
		else
		{
			MaxFPS = 0;
		}
	}

	public void SetVSync(bool vsync)
	{
		Options.VSync = vsync;
		GraphicsDeviceManager.SynchronizeWithVerticalRetrace = Options.VSync;
		GraphicsDeviceManager.ApplyChanges();
		RefreshView();
	}

	protected override void Initialize()
	{
		SetGCPhase(GCPhase.Init);
		base.Initialize();
		Allocator = new Allocator();
		SteamManager = new SteamManager(this);
		if (Options.UseSteam)
		{
			SteamManager.Init();
		}
		Window.Title = $"{APP_NAME} - Ver. {APP_VERSION}";
		Window.ClientSizeChanged += OnWindowClientSizeChanged;
		Log("Preffered buffer size = {0}x{1} ", GraphicsDeviceManager.PreferredBackBufferWidth, GraphicsDeviceManager.PreferredBackBufferHeight);
		MainViewPort = new Viewport(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
		Window.Position = new Point(GraphicsDevice.DisplayMode.Width / 2 - Window.ClientBounds.Width / 2, GraphicsDevice.DisplayMode.Height / 2 - Window.ClientBounds.Height / 2);
		if (Window.Position.X == 0 && Window.Position.Y == 0)
		{
			Window.Position = new Point(0, 1);
		}
		Log("Display mode: {0} x {1}", GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height);
		RasterizerState = new RasterizerState();
		RasterizerState.FillMode = FillMode.Solid;
		RasterizerState.ScissorTestEnable = true;
		RasterizerState.CullMode = CullMode.None;
		IsLightRenderingEnabled = true;
		SpriteBatch = new SpriteBatch(GraphicsDevice);
		SpriteBatchDistortion = new SpriteBatch(GraphicsDevice);
		GlobalContent = new GameContent();
	}

	public static void SetGCPhase(GCPhase phase)
	{
	}

	private void GameCoreEventDeactivated(object sender, EventArgs e)
	{
		if (IsApplicationActive && !_togglingScreen)
		{
			_wasPausedOnDeactivate = IsPaused && !IsMainMenuEnabled && !IsShopMenuEnabled;
			Pause(paused: true);
		}
	}

	private void GameCoreEventActivated(object sender, EventArgs e)
	{
		IsApplicationActive = true;
		GameInput.Update(0f);
		GameInput.UpdateEnd();
		if (!SteamManager.IsSteamOverlayActive)
		{
			Pause(_wasPausedOnDeactivate);
		}
	}

	protected void CoreInit()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		GameInput.SetGameCore(this);
		try
		{
			TriangleBatchTextured = new TriangleBatchTextured(GraphicsDevice);
			TriangleBatch = new TriangleBatch(GraphicsDevice);
			DataConstant.LoadFromFile(DataConstant.CONST_FILE_NAME);
			SoundSystem.Init(Window.Handle);
			UpdateVolumeByOptions();
			GameLevel.Init();
			SpineSkeletonRenderer = new SkeletonMeshRenderer(GraphicsDeviceStatic);
			SpineSkeletonRenderer.PremultipliedAlpha = true;
			PhysDebugView = new DebugViewXNA(CurrentLevel.PhysWorld);
			PhysDebugView.LoadContent(GraphicsDevice, Content);
			PhysDebugView.RemoveFlags(DebugViewFlags.PerformanceGraph);
			PhysDebugView.RemoveFlags(DebugViewFlags.AABB);
			PhysDebugView.RemoveFlags(DebugViewFlags.DebugPanel);
			PhysDebugView.AppendFlags(DebugViewFlags.Shape);
			PhysDebugView.AppendFlags(DebugViewFlags.CenterOfMass);
			_fontDebug = new BitmapFont("Font GUI");
			_fontDebug.SetGrid(16, 16, 8, 16, 8);
			_fontDebug.FirstCharNumber = 0;
			_fontDebug.LoadTexture("ui/font/font_gui.png");
			DebugDraw.Init(_fontDebug);
			DebugInfo.Init(this, _fontDebug);
			DefaultTexture = DataLoader.LoadTexture("ui/editor/undefined_texture.png");
			Editor = new Rake2D.Editor.Editor(this, _fontDebug);
			CoreLoad();
			if (!GameJnG.Init(this))
			{
				GameExit();
			}
			LightMapArray = new LightMap[3];
			for (int i = 0; i < 3; i++)
			{
				LightMapArray[i] = new LightMap();
				LightMapArray[i].Init();
			}
			ShadowMapArray = new LightMap[3];
			for (int j = 0; j < 3; j++)
			{
				ShadowMapArray[j] = new LightMap();
				ShadowMapArray[j].Init();
			}
			EffectLight = new EffectLight();
			EffectLight.Init();
			EffectParticle = new EffectLight();
			EffectParticle.Init();
			EffectLightBasic = new EffectLightBasic();
			EffectLightBasic.Init();
			Log("Light & shadow effects initialized.");
			EffectDistortion = new EffectDistortion();
			EffectDistortion.Init();
			Log("Distortion effects initialized.");
			Activated += GameCoreEventActivated;
			Deactivated += GameCoreEventDeactivated;
			Window.IsBorderless = false;
			GameLevel.Camera.SetZoom((float)Options.StartCameraZoom / 100f);
			stopwatch.Stop();
			Log("Init time: {0} sec.", (float)stopwatch.ElapsedMilliseconds / 1000f);
			_coreInitState = 2;
			Console.DoBatch();
			Console.ClearBatch();
			Window.Position = new Point(GraphicsDevice.DisplayMode.Width / 2 - GraphicsDeviceManager.PreferredBackBufferWidth / 2, GraphicsDevice.DisplayMode.Height / 2 - GraphicsDeviceManager.PreferredBackBufferHeight / 2);
			if (Window.Position.Y <= 25)
			{
				Window.Position = new Point(Window.Position.X, 25);
			}
			if (Options.PlayerController[0] == GameInput.PlayerControllerEnum.Gamepad)
			{
				Options.GamePadNumber[0] = GameInput.GetFirstConnectedGamePad(new int[1] { Options.GamePadNumber[1] });
			}
			if (Options.PlayerController[1] == GameInput.PlayerControllerEnum.Gamepad)
			{
				Options.GamePadNumber[1] = GameInput.GetFirstConnectedGamePad(new int[1] { Options.GamePadNumber[0] });
			}
			GameJnG.CheckControlerUnplugged(bInformPlayer: false);
		}
		catch (Exception ex)
		{
			LogErr("Init Faied: {0}", ex);
			_coreInitState = 9;
		}
	}

	protected void CoreLoad()
	{
		try
		{
			if (new GameLoader().Load("game.txt"))
			{
				GameJnG.LoadLevelData("level_set.txt");
				GlobalContent.Init();
				GlobalContent.LogInfo("Global content: ");
				if (Options.SaveGameNumber == 0 || !SaveGame.Load(Options.SaveGameNumber, bFromCloud: true))
				{
					SaveGame.CreateDefaultValues();
				}
				if (GameJnG.HeroList.IsNullOrEmpty())
				{
					GameJnG.CreateHero(0);
				}
			}
			FontNoBorder = GetBMFont("level_font");
		}
		catch (Exception ex)
		{
			LogErr("System exception: " + ex.ToString());
			GameExit();
		}
	}

	public void ShowSplashScreen(int screenNumber)
	{
		SpriteAtlas spriteAtlas = GlobalContent.GetSpriteAtlas("Loading Screen");
		if (spriteAtlas == null)
		{
			Texture2D texture2D = DataLoader.LoadTexture($"ui/splash_screen_{screenNumber}.png");
			spriteAtlas = GlobalContent.CreateSpriteAtlas("loading_splash_screen", texture2D, texture2D?.Name);
			spriteAtlas.OneFrame();
		}
		PresentationParameters presentationParameters = GraphicsDevice.PresentationParameters;
		Matrix screenTransform = Matrix.CreateTranslation(new Vector3(presentationParameters.BackBufferWidth / 2, presentationParameters.BackBufferHeight / 2, 0f));
		Color white = Color.White;
		SpriteBatch.Begin(SpriteSortMode.Immediate);
		spriteAtlas.DrawFrame(SpriteBatch, 0, white, screenTransform, spriteAtlas.Texture);
		SpriteBatch.End();
	}

	private void InitMainViewPort()
	{
		MainViewPort = new Viewport(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
		if (_coreInitState >= 2)
		{
			IsMainViewPortInitialized = true;
		}
	}

	private void OnWindowClientSizeChanged(object sender, EventArgs e)
	{
		RefreshView();
	}

	public void RefreshView()
	{
		InitMainViewPort();
		UpdateViewScaleMatrix();
		SetupLevelRenderTarget();
	}

	public static void ToggleFullScreen()
	{
		_togglingScreen = true;
		GraphicsDeviceManager.ToggleFullScreen();
		Options.FullScreen = GraphicsDeviceManager.IsFullScreen;
		_togglingScreen = false;
	}

	private void DisableFullscreen()
	{
		if (GraphicsDeviceManager.IsFullScreen)
		{
			GraphicsDeviceManager.IsFullScreen = false;
			GraphicsDeviceManager.ApplyChanges();
		}
	}

	public void Pause(bool paused)
	{
		if (IsPaused == paused)
		{
			return;
		}
		IsPaused = paused;
		if (IsPaused)
		{
			SoundSystem.PauseAllSounds();
			SoundSystem.PauseMusic();
		}
		else if (!IsInLevelMenuEnabled || IsMainMenuEnabled)
		{
			SoundSystem.ResumeAllSounds();
			if (!IsEditorEnabled && !GameJnG.IsAnyPopupMenuVisible)
			{
				SoundSystem.ResumeMusic();
			}
		}
		SetMaxFPS();
	}

	public static bool LoadGameLevel(LevelData levelData, bool bAllowLowerStage)
	{
		int stageNumberByLevelData = GameJnG.Campaign.GetStageNumberByLevelData(levelData);
		GameJnG.HeroDeathCountInLevel = 0;
		if (bAllowLowerStage || stageNumberByLevelData > GameJnG.StageNumber)
		{
			GameJnG.StageNumber = stageNumberByLevelData;
		}
		if (LoadGameLevel(levelData.FileName, levelData))
		{
			GameLevel.LevelData = levelData;
			return true;
		}
		return false;
	}

	public static bool LoadGameLevel(string fileName, LevelData levelData, bool bResetCamera = true)
	{
		string value = null;
		if (GameLevel != null)
		{
			value = GameLevel.Id;
		}
		SetGCPhase(GCPhase.Loading);
		float zoom;
		if (GameLevel != null)
		{
			GameLevel.Clear();
			zoom = GameLevel.Camera.Zoom;
		}
		else
		{
			zoom = 1f;
		}
		GameLevel = new GameLevel();
		GameLevel.LevelData = levelData;
		GameLevel.Init();
		if (GameLevel.LoadFromFile(fileName, bResetCamera))
		{
			if (!Utils.Equals(value, GameLevel.Id))
			{
				GameLevel.Camera.Position = Vector2.Zero;
			}
			if (!bResetCamera)
			{
				GameLevel.Camera.Zoom = zoom;
			}
			if (!IsEditorEnabled)
			{
				GameLevel.Camera.Position.X = 0f;
				GameLevel.SpawnHeroes(GameJnG.HeroList);
				GameLevel.Camera.Position.Y = GameLevel.HeroList[0].MyObject.Position.Y - 540f;
			}
			GameLevel.Begin();
			GameLevel.Update(0f);
			Editor.SetLevel(GameLevel);
			SetGCPhase((GameJnG.LoadingScreen != null && GameJnG.LoadingScreen.IsFadeToShop) ? GCPhase.Menu : GCPhase.Game);
			return true;
		}
		SetGCPhase(GCPhase.Game);
		return false;
	}

	public static void ChangeTimeFactor(float targetFactor)
	{
		TimeFactorChangeSpeed = 2f;
		if (targetFactor > 1f)
		{
			if (TargetTimeFactor > 1f)
			{
				TargetTimeFactor = 1f;
			}
			else
			{
				TargetTimeFactor = targetFactor;
			}
		}
		else if (TargetTimeFactor < 1f)
		{
			TargetTimeFactor = 1f;
		}
		else
		{
			TargetTimeFactor = targetFactor;
		}
	}

	private void ParseDebugInput()
	{
		if (GameInput.SystemKeyPressed(Keys.F5))
		{
			if (GameInput.ControlDown)
			{
				if (GameInput.ShiftDown)
				{
					IsLightRenderingEnabled = !IsLightRenderingEnabled;
				}
				else if (!IsEditorEnabled)
				{
					Options.ShowLightmaps = !Options.ShowLightmaps;
				}
			}
			else if (GameInput.ShiftDown)
			{
				Options.IsShadowsOn = !Options.IsShadowsOn;
			}
			else if (!IsEditorEnabled)
			{
				Options.IsLightsOn = !Options.IsLightsOn;
			}
		}
		if (GameInput.SystemKeyPressed(Keys.F7))
		{
			Options.UseDistortionShader = !Options.UseDistortionShader;
		}
		if (!IsMainMenuEnabled && GameInput.ControlDown && GameInput.SystemKeyPressed(Keys.Tab))
		{
			if (IsEditorEnabled)
			{
				IsEditorEnabled = false;
				Editor.Hide();
			}
			SoundSystem.StopAllSounds();
			GameJnG.ShopMenu.ToggleState();
		}
		if (IsShopMenuEnabled)
		{
			return;
		}
		if (!IsEditorEnabled)
		{
			if (GameInput.SystemKeyPressed(Keys.F3))
			{
				Options.DebugPath = !Options.DebugPath;
			}
			if (GameInput.ControlDown && GameInput.SystemKeyPressed(Keys.F6))
			{
				Options.ShowDebugPhysics = !Options.ShowDebugPhysics;
			}
			if (GameInput.SystemKeyPressed(Keys.F8))
			{
				if (GameInput.ControlDown)
				{
					Options.ScissorsMode = ((Options.ScissorsMode != 1) ? 1 : 0);
				}
				else
				{
					Options.ScissorsMode = ((Options.ScissorsMode != 2) ? 2 : 0);
				}
			}
		}
		if (GameInput.ControlDown)
		{
			if (GameInput.SystemKeyPressed(Keys.OemPlus) || GameInput.SystemKeyPressed(Keys.Add))
			{
				CurrentLevel.Camera.ZoomTo(CurrentLevel.Camera.Zoom / 0.8f, CurrentLevel.Camera.Zoom * 2f, useFrameTime: true);
			}
			else if (GameInput.SystemKeyPressed(Keys.OemMinus) || GameInput.SystemKeyPressed(Keys.Subtract))
			{
				CurrentLevel.Camera.ZoomTo(CurrentLevel.Camera.Zoom * 0.8f, CurrentLevel.Camera.Zoom * 2f, useFrameTime: true);
			}
			else if (GameInput.SystemKeyPressed(Keys.NumPad0))
			{
				if (CurrentLevel.Camera.Zoom > 1f)
				{
					CurrentLevel.Camera.ZoomTo(1f, CurrentLevel.Camera.Zoom * 6f, useFrameTime: true);
				}
				else
				{
					CurrentLevel.Camera.ZoomTo(1f, 5f, useFrameTime: true);
				}
			}
		}
		if (!GameInput.ControlDown && !GameInput.SystemKeyDown(Keys.D1) && !GameInput.SystemKeyDown(Keys.D2) && !GameInput.SystemKeyDown(Keys.D3))
		{
			int scrollSpeed = 1000;
			if (GameInput.ShiftDown)
			{
				scrollSpeed *= 3;
			}
			if (!IsEditorEnabled)
			{
				if (GameInput.SystemKeyDown(Keys.OemComma))
				{
					IgnoreDestroyOnRightEdge = true;
					CurrentLevel.Camera.Position.X -= (float)scrollSpeed * FrameTime;
					if (!IsPaused)
					{
						GameJnG.HeroList.ForEach((Hero hero) =>
						{
							hero.MyObject.SetPosition(hero.MyObject.Position.X - (float)scrollSpeed * FrameTime, hero.MyObject.Position.Y);
						});
					}
				}
				else if (GameInput.SystemKeyDown(Keys.OemPeriod))
				{
					CurrentLevel.Camera.Position.X += (float)scrollSpeed * FrameTime;
					if (!IsPaused)
					{
						GameJnG.HeroList.ForEach((Hero hero) =>
						{
							hero.MyObject.SetPosition(hero.MyObject.Position.X + (float)scrollSpeed * FrameTime, hero.MyObject.Position.Y);
						});
					}
				}
			}
		}
		if (!Console.IsActive && !IsMainMenuEnabled && (Editor.FocusComponent == null || Editor.FocusComponent.GetType() != typeof(Rake2D.Editor.TextField)))
		{
			if (GameInput.SystemKeyPressed(Keys.B))
			{
				ChangeTimeFactor(0.25f);
			}
			else if (GameInput.SystemKeyPressed(Keys.N))
			{
				ChangeTimeFactor(2f);
			}
		}
		if (!Console.IsActive && GameInput.SystemKeyPressed(Keys.Q) && Editor.FocusComponent == null)
		{
			if (GameInput.ControlDown)
			{
				CurrentLevel.Camera.AddQuake(100f);
			}
			else
			{
				CurrentLevel.Camera.AddQuake(60f);
			}
		}
		if (!GameJnG.IsAnyMissionScreenVisible && (GameInput.SystemKeyPressed(Keys.F12) || (GameInput.SystemKeyPressed(Keys.E) && GameInput.ControlDown)))
		{
			IsEditorEnabled = !IsEditorEnabled;
			if (IsInLevelMenuEnabled)
			{
				GameJnG.PopupMenuOptions.Hide();
				GameJnG.PopupMenuInLevel.Hide();
			}
			if (IsEditorEnabled)
			{
				if (!GameInput.AltDown)
				{
					GameLevel.Reload(null);
					Editor.SaveLevelOnHide = true;
				}
				else
				{
					Editor.SaveLevelOnHide = false;
				}
				GameJnG.ShopMenu.Hide(toMainMenu: false, null);
				GameJnG.MainMenu.SetState(MainMenuStateEnum.Hidden);
				Editor.Popup();
			}
			else
			{
				Editor.Hide();
			}
		}
		if (!Console.IsActive && (!IsEditorEnabled || Editor.IsKeyboardFree) && GameJnG.ShopMenu.IsKeyInputFree && GameInput.SystemKeyPressed(Keys.U))
		{
			TakeScreenshot();
		}
	}

	protected void ParseInput(float elapsedTime)
	{
		if (Options.MasterOfTheUniverse)
		{
			ParseDebugInput();
		}
		if (GameInput.AnyGamepadButtonDown(0) || GameInput.AnyGamepadButtonDown(1))
		{
			CurrentController = GameInput.ControllerTypeEnum.GamePad;
		}
		else if (!Console.IsActive && GameInput.SystemGetPressedKey() != Keys.None)
		{
			CurrentController = GameInput.ControllerTypeEnum.Keyboard;
		}
		if (GameInput.AltDown && GameInput.SystemKeyPressed(Keys.Enter))
		{
			ToggleFullScreen();
		}
		if (!GameInput.ControlDown && (GameInput.SystemKeyPressed(Keys.OemTilde) || GameInput.SystemKeyPressed(Keys.F11)))
		{
			Console.Toggle();
		}
		if (IsEditorEnabled || Console.IsActive || GameJnG.IsAnyMissionScreenVisible || IsMainMenuEnabled || IsShopMenuEnabled || IsInLevelMenuEnabled)
		{
			return;
		}
		if (!GameJnG.IsAnyPopupMenuVisible && (GameInput.SystemKeyPressed(Keys.Escape) || GameInput.SystemGPButtonPressedAll(Buttons.Back) || GameInput.SystemGPButtonPressedAll(Buttons.Start)))
		{
			if (GameJnG.IsTestRangeEnabled)
			{
				GameJnG.ShopMenu.ShowFromLevel();
			}
			else if (GameLevel.CutsceneMode == 9)
			{
				GameJnG.PopupMenuFinishCampaign.Show();
			}
			else if (GameLevel.CutsceneMode != 0)
			{
				GameJnG.LoadingScreen.Show(null, fadeToShop: true);
			}
			else
			{
				SoundSystem.Resume();
				SoundSystem.PauseAllSounds();
				SoundSystem.PauseMusic();
				GameJnG.ShopMenu.CurrentController = GameInput.ControllerTypeEnum.GamePad;
				GameJnG.PopupMenuInLevel.ButtonOptions.SetUnselected();
				GameJnG.PopupMenuInLevel.ButtonEndMission.SetUnselected();
				GameJnG.PopupMenuInLevel.ButtonResume.SetSelected();
				GameJnG.PopupMenuInLevel.Show();
				PlaySound("menu_yesno_show");
			}
		}
		if (GameInput.SystemKeyPressed(Keys.P))
		{
			Pause(!IsPaused);
		}
		if (!GameInput.SystemKeyPressed(Keys.F1))
		{
			return;
		}
		if (GameInput.ControlDown)
		{
			if (Options.HUDMode == HUDModeEnum.None)
			{
				Options.HUDMode = HUDModeEnum.Both;
			}
			else
			{
				Options.HUDMode = HUDModeEnum.None;
			}
		}
		else
		{
			Options.ToggleHUDMode();
		}
		PlaySound("menu_option_set");
	}

	public static void SetFadeEffect(bool bFadeIn)
	{
		if (bFadeIn)
		{
			_fadeEffectAlpha = 1f;
			_targetFadeEffectAlpha = 0f;
		}
		else
		{
			_fadeEffectAlpha = 0f;
			_targetFadeEffectAlpha = 1f;
		}
		_fadeEffectFirstFrame = true;
	}

	public void SetTimeFactor(float timeFactor)
	{
		TimeFactor = timeFactor;
		TargetTimeFactor = timeFactor;
	}

	protected override void Update(GameTime gameTime)
	{
		Allocator.Update();
		IgnoreDestroyOnRightEdge = IsEditorEnabled;
		UseRenderTarget = Options.UseDistortionShader;
		UseShopLightMaps = false;
		if (!GameJnG.Mia)
		{
			GameJnG.PlayTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
		}
		SteamManager.Update();
		if (_coreInitState < 1)
		{
			base.Update(gameTime);
			return;
		}
		if (_coreInitState == 1)
		{
			CoreInit();
			Console.Init(_fontDebug);
			RefreshView();
			if (_coreInitState == 9)
			{
				GameExit(-1);
				return;
			}
			UpdateViewScaleMatrix();
			GameLevel.HeroList = GameJnG.HeroList;
			if (Options.StartLevelName != null)
			{
				if (GameJnG.Campaign == null)
				{
					GameJnG.SetDifficulty(1);
					Campaign.HideAllSecretLevels();
				}
				GameLevel.LevelData = GameJnG.Campaign.GetLevelDataByFileName(Options.StartLevelName + "/level.txt");
				GameLevel.LoadFromDir(Options.StartLevelName);
				GameLevel.Begin();
			}
			else if (Options.StartInEditor == 0)
			{
				GameJnG.MainMenu.Show();
			}
			else
			{
				GameLevel.LevelData = GameJnG.Campaign.StageDataList[0].LevelDataList[0];
				if (GameLevel.LevelData != null)
				{
					GameLevel.LoadFromFile(GameLevel.LevelData.FileName);
					GameLevel.Begin();
				}
			}
			if (Options.StartLevelPosition != Vector2.Zero)
			{
				CurrentLevel.Camera.Position = Options.StartLevelPosition;
				Options.StartLevelPosition = Vector2.Zero;
			}
			Editor.Init();
			Editor.SetLevel(GameLevel);
			if (IsEditorEnabled)
			{
				Editor.SaveLevelOnHide = true;
				Editor.Popup();
			}
			else
			{
				GameLevel.SpawnHeroes(GameJnG.HeroList);
			}
			if (Options.FullScreen && Options.FullScreen != GraphicsDeviceManager.IsFullScreen)
			{
				ToggleFullScreen();
			}
		}
		else
		{
			FpsCounter.Update(gameTime.ElapsedGameTime.TotalSeconds);
		}
		IsApplicationActive = IsActive;
		DebugInfo.ClearFrameInfo();
		FrameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
		if (TargetTimeFactor < TimeFactor)
		{
			TimeFactor -= FrameTime * TimeFactorChangeSpeed;
			if (TimeFactor < TargetTimeFactor)
			{
				TimeFactor = TargetTimeFactor;
			}
		}
		else if (TargetTimeFactor > TimeFactor)
		{
			TimeFactor += FrameTime * TimeFactorChangeSpeed;
			if (TimeFactor > TargetTimeFactor)
			{
				TimeFactor = TargetTimeFactor;
			}
		}
		ElapsedTime = FrameTime;
		if (IsPaused)
		{
			ElapsedTime = 0f;
		}
		else if (ElapsedTime >= 0.05f)
		{
			ElapsedTime = 0.05f;
		}
		ElapsedTime *= TimeFactor;
		SoundSystem.Update(ElapsedTime);
		ResetLights();
		if (!GameJnG.IsMouseControlEnabled || IsPaused || !IsActive || IsShopMenuEnabled || IsMainMenuEnabled || IsEditorEnabled || GameJnG.IsAnyMissionScreenVisible || GameJnG.IsAnyPopupMenuVisible)
		{
			GameInput.IsMouseFixed = false;
		}
		else
		{
			GameInput.IsMouseFixed = true;
		}
		GameInput.Update(FrameTime);
		ParseInput(FrameTime);
		if (GameJnG.LoadingScreen != null && GameJnG.LoadingScreen.State != MissionScreenStateEnum.Hidden)
		{
			GameJnG.LoadingScreen.Update(FrameTime);
			Console.Update(FrameTime);
			return;
		}
		UpdateHeroes = !IsEditorEnabled;
		if (IsMainMenuEnabled)
		{
			GameJnG.MainMenu.Update(ElapsedTime);
		}
		else if (IsShopMenuEnabled)
		{
			GameJnG.ShopMenu.Update(ElapsedTime);
		}
		else
		{
			if (GameJnG.ShopMenu.State == ShopMenuState.ShowingFromLevel)
			{
				GameJnG.ShopMenu.UpdateShowHide(ElapsedTime);
			}
			GameJnG.UpdateFinishLevel(ElapsedTime);
			if ((IsInLevelMenuEnabled || GameJnG.PopupMenuFinishCampaign.State != ShopMenuState.Hidden) && !IsMainMenuEnabled)
			{
				GameJnG.ShopMenu.OnlyOneKeyPressedProtection = false;
				GameJnG.ShopMenu.ShopInput.Update(FrameTime);
				GameJnG.ShopMenu.CursorType = CursorTypeEnum.NoAction;
				if (GameJnG.PopupMenuOptions.State != ShopMenuState.Hidden)
				{
					GameJnG.PopupMenuOptions.Update(FrameTime);
				}
				else if (GameJnG.PopupMenuInLevel.State != ShopMenuState.Hidden)
				{
					GameJnG.PopupMenuInLevel.Update(FrameTime);
				}
				else if (GameJnG.PopupMenuFinishCampaign.State != ShopMenuState.Hidden)
				{
					GameJnG.PopupMenuFinishCampaign.Update(FrameTime);
				}
				if (GameJnG.ShopMenu.IsAnyPopupMenuVisible)
				{
					GameJnG.ShopMenu.Cursor.Update(FrameTime);
					ElapsedTime = 0f;
					GameJnG.ShopMenu.Cursor.SetCursorType(GameJnG.ShopMenu.CursorType);
					GameJnG.ShopMenu.RenderLightMaps();
				}
				UseShopLightMaps = false;
			}
			else if (IsEditorEnabled)
			{
				Editor.Update(FrameTime);
			}
		}
		if (UpdateHeroes && !IsShopMenuEnabled && !IsMainMenuEnabled && !Options.ScreenShotModeEnabled && GameLevel.CutsceneMode == 0 && GameJnG.MissionFailedScreen.State == MissionScreenStateEnum.Hidden)
		{
			GameJnG.LocationText.Update(ElapsedTime);
		}
		Console.Update(FrameTime);
		if (!IsShopMenuEnabled && !IsMainMenuEnabled && GameJnG.MissionFailedScreen.State <= MissionScreenStateEnum.FlashToWhite && GameJnG.MissionCompletedScreen.State != MissionScreenStateEnum.Visible && GameJnG.LoadingScreen.State == MissionScreenStateEnum.Hidden)
		{
			GameLevel.HeroList = GameJnG.HeroList;
			GameLevel.Update(ElapsedTime);
			GameJnG.CheckControlerUnplugged(!GameJnG.IsAnyPopupMenuVisible);
			for (int num = GameJnG.BossMeterList.Count - 1; num >= 0; num--)
			{
				BossMeter bossMeter = GameJnG.BossMeterList[num];
				if (bossMeter.IsDestroyed)
				{
					GameJnG.BossMeterList.RemoveAt(num);
				}
				else
				{
					bossMeter.TargetPositionY = 50 + num * 40;
					bossMeter.Update(ElapsedTime);
				}
			}
			GameJnG.ScoreHUD.Update(ElapsedTime);
			GameJnG.CarnageTV?.Update(ElapsedTime);
			if (GameJnG.IsTestRangeEnabled || GameLevel.CutsceneMode != 0)
			{
				_pressKeyAlphaFactor += 5f * ElapsedTime;
			}
			GameJnG.CheckMissionState();
		}
		if (!_fadeEffectFirstFrame && GameJnG.MissionFailedScreen.State == MissionScreenStateEnum.Hidden)
		{
			Utils.ChangeValueToTarget(ref _fadeEffectAlpha, _targetFadeEffectAlpha, 4f * ElapsedTime);
		}
		GameInput.UpdateEnd();
		if (MaxFPS > 0 && _coreInitState >= 2 && FpsCounter.ActualizeCount > 4)
		{
			int num2 = 1000 / MaxFPS;
			while (_loopStopwatch.IsRunning && _loopStopwatch.ElapsedMilliseconds < num2)
			{
				Thread.Sleep(1);
			}
			_loopStopwatch.Restart();
		}
		base.Update(gameTime);
	}

	internal static void ResetLights()
	{
		LightMap[] lightMapArray = LightMapArray;
		for (int i = 0; i < lightMapArray.Length; i++)
		{
			lightMapArray[i].Reset();
		}
		lightMapArray = ShadowMapArray;
		for (int i = 0; i < lightMapArray.Length; i++)
		{
			lightMapArray[i].Reset();
		}
		if (IsShopMenuEnabled || IsMainMenuEnabled || GameJnG.ShopMenu.IsAnyPopupMenuVisible)
		{
			lightMapArray = GameJnG.ShopMenu.LightMapArray;
			for (int i = 0; i < lightMapArray.Length; i++)
			{
				lightMapArray[i].Reset();
			}
		}
	}

	public static void SetupLevelRenderTarget()
	{
		LevelRenderTarget = new RenderTarget2D(GraphicsDeviceStatic, MainViewPort.Width, MainViewPort.Height, mipMap: false, GraphicsDeviceStatic.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
		ScreenRenderTarget = new RenderTarget2D(GraphicsDeviceStatic, MainViewPort.Width, MainViewPort.Height, mipMap: false, GraphicsDeviceStatic.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
	}

	public static Effect GetRenderEffect(Layer layer)
	{
		if (layer != null && IsLightRenderingEnabled)
		{
			float num = layer.ShadowMapFactor * EffectLight.LightFactor;
			if (EffectLight.TotalLightFactor == 0f && EffectLight.HitFactor == 0f && num == 0f)
			{
				return null;
			}
			if (Options.IsShadowsOn && layer.ShadowFromMapNumber >= 0 && layer.ShadowFromMapNumber < 3 && num > 0f)
			{
				EffectLight.MyEffect.Parameters["shadowSampler"].SetValue(ShadowMapArray[layer.ShadowFromMapNumber].RenderTarget);
				EffectLight.SetShadowFactor(num);
			}
			else
			{
				EffectLight.SetShadowFactor(0f);
			}
			return EffectLight.MyEffect;
		}
		return null;
	}

	public static void DrawLevelRenderTarget(bool bUseDistortion, Color color)
	{
		if (LevelRenderTarget != null)
		{
			GraphicsDeviceStatic.SetRenderTarget(ScreenRenderTarget);
			if (bUseDistortion)
			{
				EffectDistortion.Begin();
				SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise, EffectDistortion.Effect, Matrix.Identity);
			}
			else
			{
				SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);
			}
			SetScissorRectangle();
			SpriteBatch.Draw(LevelRenderTarget, GraphicsDeviceStatic.ScissorRectangle, GraphicsDeviceStatic.ScissorRectangle, color);
			SpriteBatch.End();
		}
	}

	public static void RenderLightAndShadow(Camera camera)
	{
		int num = 0;
		LightMap[] lightMapArray = LightMapArray;
		foreach (LightMap lightMap in lightMapArray)
		{
			lightMap.Render(camera);
			DebugInfo.LightCount[num++] = lightMap.LightObjectList.Count;
		}
		num = 0;
		lightMapArray = ShadowMapArray;
		foreach (LightMap lightMap2 in lightMapArray)
		{
			lightMap2.Render(camera);
			DebugInfo.ShadowCount[num++] = lightMap2.LightObjectList.Count;
		}
	}

	public static void SetDefaultRenderTarget()
	{
		if (UseRenderTarget)
		{
			GraphicsDeviceStatic.SetRenderTarget(LevelRenderTarget);
		}
		else
		{
			GraphicsDeviceStatic.SetRenderTarget(ScreenRenderTarget);
		}
	}

	public void DrawDarkBackground(float alpha)
	{
		TriangleBatch.Begin(BlendState.NonPremultiplied, Matrix.Identity);
		TriangleBatch.DrawRect(new Vector2(-2f, -2f), new Vector2(Window.ClientBounds.Width + 2, Window.ClientBounds.Height + 2), new Color(0f, 0f, 0f, alpha));
		TriangleBatch.End();
	}

	public void DrawPopupMenus()
	{
		GraphicsDeviceStatic.SetRenderTarget(ScreenRenderTarget);
		UseRenderTarget = false;
		EffectLight.LayerBeginDraw(GameJnG.ShopMenu.LayerShop);
		if (GameJnG.IsAnyPopupMenuVisible)
		{
			float alpha;
			if (GameJnG.PopupMenuInLevel.State != ShopMenuState.Hidden)
			{
				alpha = ((GameJnG.PopupMenuOptions.State == ShopMenuState.Hidden) ? ((float)(int)GameJnG.PopupMenuInLevel.Color.A / 350f) : 0.425f);
			}
			else
			{
				alpha = ((GameJnG.PopupMenuFinishCampaign.State == ShopMenuState.Hidden) ? ((float)(int)GameJnG.PopupMenuOptions.Color.A / 350f) : ((float)(int)GameJnG.PopupMenuFinishCampaign.Color.A / 350f));
			}
			DrawDarkBackground(alpha);
		}
		if (GameJnG.PopupMenuOptions.State != ShopMenuState.Hidden)
		{
			GameJnG.PopupMenuOptions.Draw();
		}
		else if (GameJnG.PopupMenuFinishCampaign.State != ShopMenuState.Hidden)
		{
			GameJnG.PopupMenuFinishCampaign.Draw();
		}
		else if (GameJnG.PopupMenuInLevel.State != ShopMenuState.Hidden)
		{
			GameJnG.PopupMenuInLevel.Draw();
		}
		if (GameJnG.IsAnyPopupMenuVisible && GameJnG.ShopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
		{
			GameJnG.ShopMenu.Cursor.Draw();
		}
	}

	protected override void Draw(GameTime gameTime)
	{
		base.Draw(gameTime);
		if (_coreInitState == 0)
		{
			ShowSplashScreen(0);
			_coreInitState = 1;
			base.Draw(gameTime);
			return;
		}
		if (GameLevel != null)
		{
			GameLevel.CurrentLayer = null;
			EffectLightBasic.Update();
			RenderLightAndShadow(GameLevel.Camera);
			if (Options.UseDistortionShader)
			{
				if (IsMainMenuEnabled)
				{
					GameJnG.MainMenu.Level.RenderDistortion();
				}
				else
				{
					GameLevel.RenderDistortion();
				}
			}
		}
		GraphicsDeviceStatic.Viewport = MainViewPort;
		GraphicsDeviceStatic.SetRenderTarget(ScreenRenderTarget);
		GraphicsDeviceStatic.Clear(Color.Black);
		SetDefaultRenderTarget();
		if (IsEditorEnabled)
		{
			GraphicsDeviceStatic.Clear(Options.BgrColor);
		}
		else
		{
			GraphicsDeviceStatic.Clear(Color.Black);
		}
		if (IsMainMenuEnabled)
		{
			GameJnG.MainMenu.Draw();
		}
		else if (!IsShopMenuEnabled)
		{
			if (GameLevel != null && GameJnG.MissionFailedScreen.State <= MissionScreenStateEnum.FlashToWhite && GameJnG.MissionCompletedScreen.State != MissionScreenStateEnum.Visible && GameJnG.LoadingScreen.State == MissionScreenStateEnum.Hidden)
			{
				GameLevel.Draw();
			}
			else
			{
				GraphicsDeviceStatic.SetRenderTarget(ScreenRenderTarget);
			}
		}
		if (GameJnG.LoadingScreen.State != MissionScreenStateEnum.Hidden)
		{
			GameJnG.LoadingScreen.Draw();
		}
		else if (GameLevel != null)
		{
			if (Options.ShowDebugPhysics)
			{
				GraphicsDeviceStatic.BlendState = BlendState.AlphaBlend;
				CurrentLevel.Camera.SetLayer(CurrentLevel.MainLayer);
				Matrix view = Matrix.CreateScale(ConvertUnits.ToDisplayUnits(1)) * CurrentLevel.Camera.WorldMatrixCurrentLayer;
				PhysDebugView.SetWorld(CurrentLevel.PhysWorld);
				PhysDebugView.RenderDebugData(ref _projectionMatrix, ref view);
			}
			DebugDraw.DrawPrints();
			if (!IsMainMenuEnabled && !IsShopMenuEnabled)
			{
				if (UpdateHeroes)
				{
					GameJnG.DrawHUD();
					if (!Options.ScreenShotModeEnabled && GameLevel.CutsceneMode == 0 && GameJnG.MissionFailedScreen.State == MissionScreenStateEnum.Hidden)
					{
						GameJnG.DrawLocationText();
					}
				}
				if (!IsEditorEnabled)
				{
					if (GameJnG.MissionFailedScreen.State != MissionScreenStateEnum.Hidden)
					{
						GameJnG.MissionFailedScreen.Draw();
					}
					else if (GameJnG.MissionCompletedScreen.State != MissionScreenStateEnum.Hidden)
					{
						GameJnG.MissionCompletedScreen.Draw();
					}
					else if (GameLevel.CutsceneMode > 0 && !GameJnG.IsAnyPopupMenuVisible)
					{
						GameJnG.ShopMenu.SpriteBatchBegin(bLight: false);
						GameJnG.ShopMenu.FontDefault.Write(GameJnG.ShopMenu.SpriteBatchNoLight, TextBundle.GetControlText("press_esc_to_continue", CurrentController), 960f, 990f, Vector2.One, Color.White, TextAlign.Center, 0, 0.6f + 0.4f * Utils.Sin(_pressKeyAlphaFactor));
						GameJnG.ShopMenu.SpriteBatchEnd(bLight: false);
					}
					else if (GameJnG.IsTestRangeEnabled && !GameJnG.IsAnyPopupMenuVisible)
					{
						GameJnG.ShopMenu.SpriteBatchBegin(bLight: false);
						GameJnG.ShopMenu.FontDefault.Write(GameJnG.ShopMenu.SpriteBatchNoLight, TextBundle.GetControlText("press_esc_to_return_to_shop", CurrentController), 960f, 990f, Vector2.One, Color.White, TextAlign.Center, 0, 0.6f + 0.4f * Utils.Sin(_pressKeyAlphaFactor));
						GameJnG.ShopMenu.SpriteBatchEnd(bLight: false);
					}
				}
			}
			if (GameJnG.ShopMenu.State == ShopMenuState.ShowingFromLevel)
			{
				_fadeEffectAlpha = GameJnG.ShopMenu.FadeFromlevelPercentage;
			}
			if (_fadeEffectAlpha > 0f && GameJnG.MissionCompletedScreen.State == MissionScreenStateEnum.Hidden && GameJnG.MissionFailedScreen.State == MissionScreenStateEnum.Hidden)
			{
				DrawDarkBackground(_fadeEffectAlpha);
			}
			if (IsPaused && !Options.DebugPause)
			{
				DrawDarkBackground(0.5f);
				DrawBegin(ViewPortMatrix);
				GameJnG.ShopMenu.FontDefault.Write(SpriteBatch, TextBundle.GetText("game_paused"), 960f, 540f, Vector2.One, Color.LightCyan, TextAlign.Center);
				DrawEnd();
			}
			if (IsShopMenuEnabled && !IsMainMenuEnabled)
			{
				GameJnG.ShopMenu.Draw();
			}
			else if (IsEditorEnabled)
			{
				Editor.Draw();
			}
			else if (UpdateHeroes && GameJnG.MissionFailedScreen.State == MissionScreenStateEnum.Hidden && GameJnG.MissionCompletedScreen.State == MissionScreenStateEnum.Hidden)
			{
				DebugDraw.ShowScreenFrames();
			}
			if (!IsShopMenuEnabled)
			{
				DrawPopupMenus();
			}
			if (Options.ShowLightmaps)
			{
				DrawLighMapsPreview();
			}
		}
		if (Options.DebugInfoMode != 0 && CurrentLevel != null)
		{
			DebugInfo.Draw(gameTime);
		}
		if (Options.SoundDebugInfoMode > 0)
		{
			SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);
			SoundSystem.DrawDebugInfo(SpriteBatch, _fontDebug);
			SpriteBatch.End();
		}
		Console.Draw();
		if (_fadeEffectAlpha > 0f)
		{
			_fadeEffectFirstFrame = false;
		}
		GraphicsDevice.SetRenderTarget(null);
		SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);
		if (IsEditorEnabled)
		{
			SpriteBatch.Draw(ScreenRenderTarget, Vector2.Zero, Color.White);
		}
		else
		{
			SpriteBatch.Draw(ScreenRenderTarget, GraphicsDeviceStatic.Viewport.Bounds, Color.White);
		}
		SpriteBatch.End();
	}

	private void DrawLighMapsPreview()
	{
		int num = 0;
		Editor.SpriteDrawBegin();
		LightMap[] lightMapArray = LightMapArray;
		foreach (LightMap lightMap in lightMapArray)
		{
			Editor.SpriteBatch.Draw(lightMap.RenderTarget, new Rectangle(0, (GraphicsDevice.Viewport.Height / 4 + 2) * num, GraphicsDevice.Viewport.Width / 4, GraphicsDevice.Viewport.Height / 4), Color.White);
			num++;
		}
		num = 0;
		lightMapArray = ShadowMapArray;
		foreach (LightMap lightMap2 in lightMapArray)
		{
			Editor.SpriteBatch.Draw(lightMap2.RenderTarget, new Rectangle(GraphicsDevice.Viewport.Width / 4 + 2, (GraphicsDevice.Viewport.Height / 4 + 2) * num, GraphicsDevice.Viewport.Width / 4, GraphicsDevice.Viewport.Height / 4), Color.White);
			num++;
		}
		if (IsShopMenuEnabled || GameJnG.ShopMenu.IsAnyPopupMenuVisible || IsMainMenuEnabled)
		{
			num = 0;
			lightMapArray = GameJnG.ShopMenu.LightMapArray;
			foreach (LightMap lightMap3 in lightMapArray)
			{
				Editor.SpriteBatch.Draw(lightMap3.RenderTarget, new Rectangle(GraphicsDevice.Viewport.Width / 2 + 4, (GraphicsDevice.Viewport.Height / 4 + 2) * num, GraphicsDevice.Viewport.Width / 4, GraphicsDevice.Viewport.Height / 4), Color.White);
				num++;
			}
		}
		Editor.SpriteDrawEnd();
	}

	public static void SetViewScissorRectangle()
	{
		Vector2 vector = new Vector2(1919f, 1079f) * VirtualAspectRatio;
		ScissorRectangle = new Rectangle((int)((float)(GraphicsDeviceStatic.Viewport.Width / 2) - vector.X / 2f), (int)((float)(GraphicsDeviceStatic.Viewport.Height / 2) - vector.Y / 2f), (int)(vector.X + 2f), (int)(vector.Y + 2f));
		GraphicsDeviceStatic.ScissorRectangle = ScissorRectangle;
	}

	public static void SetScissorRectangle()
	{
		if (((!IsEditorEnabled && Options.ScissorsMode == 0) || (IsEditorEnabled && Editor.ScissorsMode == 0)) && !IsShopMenuEnabled)
		{
			GraphicsDeviceStatic.ScissorRectangle = GraphicsDeviceStatic.Viewport.Bounds;
		}
		else if (((Options.ScissorsMode == 2 && !IsEditorEnabled) || (IsEditorEnabled && Editor.ScissorsMode == 2)) && !float.IsNaN(CurrentLevel.Camera.ViewTopLeftOnScreen.X))
		{
			ScissorRectangle = new Rectangle((int)CurrentLevel.Camera.ViewTopLeftOnScreen.X, (int)CurrentLevel.Camera.ViewTopLeftOnScreen.Y, (int)(CurrentLevel.Camera.ViewBottomRightOnScreen.X - CurrentLevel.Camera.ViewTopLeftOnScreen.X + 1f), (int)(CurrentLevel.Camera.ViewBottomRightOnScreen.Y - CurrentLevel.Camera.ViewTopLeftOnScreen.Y + 1f));
			GraphicsDeviceStatic.ScissorRectangle = ScissorRectangle;
		}
		else
		{
			SetViewScissorRectangle();
		}
	}

	public static void DrawBegin()
	{
		DrawBegin(LevelMatrix);
	}

	public static void DrawBegin(Matrix matrix)
	{
		SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, RasterizerState, GetRenderEffect(CurrentLevel.CurrentLayer), matrix);
	}

	public static void DrawEnd()
	{
		SpriteBatch.End();
	}

	public static void UpdateViewScaleMatrix()
	{
		if (MainViewPort.AspectRatio > 1.7777778f)
		{
			float num = (float)MainViewPort.Height / 1080f;
			VirtualAspectRatio = new Vector2(num);
			ScreenCenterOffset = new Vector2(((float)MainViewPort.Width - 1920f * num) / 2f, 0f);
			ViewPortMatrix = Matrix.CreateScale(num);
		}
		else
		{
			float num2 = (float)MainViewPort.Width / 1920f;
			VirtualAspectRatio = new Vector2(num2);
			ScreenCenterOffset = new Vector2(0f, ((float)MainViewPort.Height - 1080f * num2) / 2f);
			ViewPortMatrix = Matrix.CreateScale(num2);
		}
		ViewPortMatrix *= Matrix.CreateTranslation(ScreenCenterOffset.X, ScreenCenterOffset.Y, 0f);
		if (CurrentLevel.Camera != null)
		{
			CurrentLevel.Camera.SetViewPort(MainViewPort);
		}
		_projectionMatrix = Matrix.CreateOrthographicOffCenter(0f, MainViewPort.Width, MainViewPort.Height, 0f, 0f, 1000f);
		SetViewScissorRectangle();
	}

	public static void Log(string message, params object[] args)
	{
		string text = string.Format("- " + message, args);
		_logger.Log(text);
		if (Console != null)
		{
			Console.AddMessage(text);
		}
	}

	public static void LogErr(string message, params object[] args)
	{
		string text = string.Format("! " + message, args);
		_logger.Log(text);
		if (Console != null)
		{
			Console.AddMessage(text, ConsoleMessageType.Error);
		}
		ErrorCount++;
	}

	public static SpriteAtlas GetSpriteAtlas(string id, bool logWhenNotFound = false)
	{
		SpriteAtlas spriteAtlas = null;
		if (GameLevel.Content != null)
		{
			spriteAtlas = GameLevel.Content.GetSpriteAtlas(id);
		}
		if (spriteAtlas == null && GlobalContent != null)
		{
			spriteAtlas = GlobalContent.GetSpriteAtlas(id);
		}
		if (logWhenNotFound && spriteAtlas == null)
		{
			LogErr("Atlas '{0}' not found in game content", id);
		}
		return spriteAtlas;
	}

	public static SpineData GetSpineData(string id)
	{
		SpineData spineData = null;
		if (GameLevel.Content != null)
		{
			spineData = GameLevel.Content.GetSpineData(id);
		}
		if (spineData == null && GlobalContent != null)
		{
			spineData = GlobalContent.GetSpineData(id);
		}
		return spineData;
	}

	public static SoundData GetSoundData(string id)
	{
		SoundData soundData = null;
		if (GameLevel.Content != null)
		{
			soundData = GameLevel.Content.GetSoundData(id);
		}
		if (soundData == null && GlobalContent != null)
		{
			soundData = GlobalContent.GetSoundData(id);
		}
		return soundData;
	}

	public static List<SoundData> GetAllSoundData()
	{
		List<SoundData> list = new List<SoundData>();
		if (GameLevel.Content != null)
		{
			list.AddRange(GameLevel.Content.GetAllSoundData());
		}
		if (GlobalContent != null)
		{
			list.AddRange(GlobalContent.GetAllSoundData());
		}
		return list;
	}

	public static ParticleEmitterData GetParticleEmitterData(string id, bool logWhenNotFound = true)
	{
		ParticleEmitterData particleEmitterData = null;
		if (GameLevel.Content != null)
		{
			particleEmitterData = GameLevel.Content.GetParticleEmitterData(id);
		}
		if (particleEmitterData == null && GlobalContent != null)
		{
			particleEmitterData = GlobalContent.GetParticleEmitterData(id);
		}
		if (logWhenNotFound && particleEmitterData == null)
		{
			LogErr("Emitter data '{0}' not found in game content", id);
		}
		return particleEmitterData;
	}

	public static BMFont GetBMFont(string id, bool logWhenNotFound = true)
	{
		BMFont bMFont = null;
		if (GameLevel.Content != null)
		{
			bMFont = GameLevel.Content.GetBMFont(id);
		}
		if (bMFont == null && GlobalContent != null)
		{
			bMFont = GlobalContent.GetBMFont(id);
		}
		if (logWhenNotFound && bMFont == null)
		{
			LogErr("Font '{0}' not found in game content", id);
		}
		return bMFont;
	}

	public static HeroItemData GetHeroItemData(string id)
	{
		HeroItemData heroItemData = null;
		if (id != null)
		{
			if (GameLevel.Content != null)
			{
				heroItemData = GameLevel.Content.GetHeroItemData(id);
			}
			if (heroItemData == null && GlobalContent != null)
			{
				heroItemData = GlobalContent.GetHeroItemData(id);
			}
		}
		return heroItemData;
	}

	public static void UpdateVolumeByOptions()
	{
		SoundSystem.SoundVolume = Options.SoundVolume;
		SoundSystem.MusicVolume = Options.MusicVolume;
	}

	public static ISound2D PlaySound(string id, float volume = 100f, float pan = 0f, bool looped = false)
	{
		ISound2D sound2D = null;
		SoundData soundData = GetSoundData(id);
		if (soundData != null)
		{
			sound2D = SoundSystem.PlaySound(soundData, volume, pan, looped);
			if (sound2D == null)
			{
				LogErr("Unable to play sound '{0}'.", id);
			}
		}
		else
		{
			LogErr("Unable to play undefined sound '{0}'.", id);
		}
		return sound2D;
	}

	public static PartData GetPartData(string id, bool logWhenNotFound = true)
	{
		if (id == null)
		{
			LogErr("Null id for GetPartData");
			return null;
		}
		PartData partData = null;
		if (GameLevel.Content != null)
		{
			partData = GameLevel.Content.GetPartData(id);
		}
		if (partData == null && GlobalContent != null)
		{
			partData = GlobalContent.GetPartData(id);
		}
		if ((partData == null) & logWhenNotFound)
		{
			LogErr("Part data '{0}' not found in game content", id);
		}
		return partData;
	}

	public static Texture2D GetTexture2D(string id, bool logWhenNotFound = true)
	{
		if (id == null)
		{
			return null;
		}
		Texture2D texture2D = null;
		if (GameLevel.Content != null)
		{
			texture2D = GameLevel.Content.GetTexture(id);
		}
		if (texture2D == null && GlobalContent != null)
		{
			texture2D = GlobalContent.GetTexture(id);
		}
		if ((texture2D == null) & logWhenNotFound)
		{
			LogErr("Texture '{0}' not found in game content", id);
		}
		return texture2D;
	}

	public static BezierPath GetBezierPath(string id)
	{
		if (id == null)
		{
			return null;
		}
		BezierPath bezierPath = null;
		if (GameLevel.Content != null)
		{
			bezierPath = GameLevel.Content.GetBezierPath(id);
		}
		if (bezierPath == null && GlobalContent != null)
		{
			bezierPath = GlobalContent.GetBezierPath(id);
		}
		if (bezierPath == null)
		{
			LogErr("Path '{0}' not found in game content", id);
		}
		return bezierPath;
	}

	public void GameExit(int exitCode = 0)
	{
		_exitCode = exitCode;
		Log("Error count: {0}", ErrorCount);
		Exit();
	}

	protected override void OnExiting(object sender, EventArgs args)
	{
		Log("Exiting with code {0}", _exitCode);
		SoundSystem?.Destroy();
		if (_exitCode == 0)
		{
			Options.Save(DataUtils.GetGameIniPath());
			GameJnG.ClearLevelAchievedValues();
			if (Options.SaveGameNumber > 0)
			{
				SaveGame.Save(Options.SaveGameNumber);
			}
			Editor.SaveOptionsToFile();
		}
		SteamManager?.Destroy();
	}

	public void TakeScreenshot()
	{
		string text = null;
		try
		{
			text = Path.Combine(Options.SShotDir ?? "", $"jng_shot_{Options.SShotNumber++:D3}.png");
			Stream stream;
			using (stream = new FileStream(text, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				ScreenRenderTarget.SaveAsPng(stream, ScreenRenderTarget.Width, ScreenRenderTarget.Height);
				stream.Close();
			}
			Log("SShot saved to '{0}'", text);
		}
		catch (Exception ex)
		{
			LogErr("Unable to save sshot to file '{0}': {1}", text, ex);
		}
	}
}
