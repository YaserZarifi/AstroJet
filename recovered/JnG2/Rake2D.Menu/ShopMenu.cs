using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Rake2D.Font.BMFont;
using Rake2D.JnG;
using Rake2D.Menu.Atarix;

namespace Rake2D.Menu;

public class ShopMenu : Transformation
{
	public static readonly Vector2 FONT_SCALE_BUTTON = new Vector2(0.5f);

	public static readonly Vector2 FONT_SCALE_BUTTON_BIG = new Vector2(0.6f);

	public static readonly Vector2 FONT_SCALE_TAB_BUTTON = new Vector2(0.5f);

	public static readonly Color COLOR_EMPTY_SLOT_SELECTION = new Color(150, 150, 150);

	public static readonly Color FONT_COLOR_LABEL = new Color(247, 145, 32);

	public static readonly Color FONT_COLOR_PARAM_VALUE = new Color(255, 250, 20);

	public static readonly Color FONT_COLOR_YES_NO_QUESTION = new Color(220, 200, 80);

	public static readonly Vector2 SLOT_CENTER_OFFSET = new Vector2(37f, 28f);

	public const int SLOT_WIDTH = 82;

	public const int SLOT_HEIGHT = 65;

	public GameCore GameCore;

	public ShopScreenEnum CurrentScreen;

	public bool IsEnabled;

	public ShopMenuState State;

	public float ShowHidePercentage;

	public Cursor Cursor;

	public LightMap[] LightMapArray;

	public SpriteAtlas AtlasSlot;

	public SpriteAtlas AtlasGemSlot;

	public SpriteAtlas AtlasSlotLevel;

	public SpriteAtlas AtlasSlider;

	public SpriteAtlas AtlasHorizontalSlider;

	public SpriteAtlas AtlasWheel;

	public SpriteAtlas AtlasButton;

	public SpriteAtlas AtlasButtonPriceGlow;

	public SpriteAtlas AtlasTabButton;

	public SpriteAtlas AtlasTabButtonSmall;

	public SpriteAtlas AtlasBigButton;

	public SpriteAtlas AtlasSlotTypeIcon;

	public SpriteAtlas AtlasSymbol;

	public SpriteAtlas AtlasOptionsElement;

	public SpriteAtlas AtlasTV;

	public SpriteAtlas AtlasPopupDialog;

	public SpriteAtlas AtlasYesNoDialog;

	public SpriteAtlas AtlasFace;

	public SpriteAtlas AtlasSummaryStatsBgr;

	public SpriteAtlas AtlasPanelLabel;

	public SpriteAtlas AtlasItemSetIcon;

	public SpriteAtlas AtlasLoadout;

	public ShopBackground ShopBackground;

	public PanelCargo PanelCargo;

	public PanelInfo PanelInfo;

	public PanelShop PanelShop;

	public PanelHeroStats PanelHeroStats;

	public PanelShipParams PanelShipParams;

	public PanelMia PanelMia;

	public PanelFaceSelector PanelFaceSelector;

	public PanelTopButtons PanelTopButtons;

	public YesNoDialog YesNoEndCampaign;

	public YesNoDialog YesNoResetHeroProfiles;

	public BigShipSlotSet BigShipSlotSet;

	public BigShip BigShip;

	public HeroItem SelectedItem;

	public HeroItem CursorItem;

	public ItemFlySet ItemFlySet;

	public Hero MyHero;

	public Hero SwapToHero;

	public string HintText;

	public ShopMenuSection FocusSection;

	public GameInput.ControllerTypeEnum CurrentController;

	public BMFont FontDefault;

	public BMFont FontNoBorder;

	public bool IsCursorOnSlot;

	public GameLevel GUILevel;

	public GameLevel HeroPreviewLevel;

	public Layer LayerShopBackground;

	public Layer LayerShopFront;

	public Layer LayerShop;

	public Layer LayerTV;

	public Layer LayerMissionLight;

	public Layer LayerMissionStarfield;

	public RenderTarget2D HeroPreviewRenderTarget;

	public SpriteBatch SpriteBatchLight;

	public SpriteBatch SpriteBatchNoLight;

	public ShopInput ShopInput;

	public ItemView ItemView;

	public Rake2D.Menu.Atarix.Atarix Atarix;

	public bool IsAtarixInstalled;

	public float FadeFromlevelPercentage;

	public CursorTypeEnum CursorType;

	public ScreenMission ScreenMission;

	public TriangleBatch TriangleBatch;

	public bool OnlyOneKeyPressedProtection;

	public Vector2 BigShipMoveFactor;

	public Button ButtonTestRange;

	private ShopObject _objBottomFillerLeft;

	private ShopObject _objBottomFillerRight;

	private float _targetHintLineAlphaFactor;

	private float _hintLinetAlphaFactor;

	private ShopMenuSection _newFocusSection;

	private float _originalCameraZoom;

	private float _cameraZoom;

	private float _targetCameraZoom;

	private float _selectionAlpha;

	private float _targetAlpha;

	private List<CursorMessage> _cursorMessageList;

	private List<ShopObject> _diodeList;

	private Viewport _levelPreviewViewPort;

	private LevelData _hideToLoadingLevelData;

	private bool _lastHideFrameDrawed;

	private float _shipStatsLabelWidth;

	private float _slotBlinkAlpha;

	private float _slotBlinkTimer;

	private int _heroPreviewLevelNumber = -1;

	public static Vector2 FONT_SCALE_CASH => new Vector2(0.75f * Options.FontScale);

	public static Vector2 FONT_SCALE_TEXT_NORMAL => new Vector2(0.45f * Options.FontScale);

	public static Vector2 FONT_SCALE_PARAMS => new Vector2(0.45f * Options.FontScale);

	public static Vector2 FONT_SCALE_PARAM_TAG => new Vector2(0.45f * Options.FontScale);

	public static Vector2 FONT_SCALE_TV_SMALL => new Vector2(0.4f * Options.FontScale);

	public static Vector2 FONT_SCALE_LABEL => new Vector2(0.5f * Options.FontScale);

	public static Vector2 FONT_SCALE_LABEL_SMALL => new Vector2(0.4f * Options.FontScale);

	public static Vector2 FONT_SCALE_PRICE_TV => new Vector2(0.6f * Options.FontScale);

	public static Vector2 FONT_SCALE_ITEM_NAME => new Vector2(0.6f * Options.FontScale);

	public static Vector2 FONT_SCALE_ITEM_TYPE => new Vector2(0.4f * Options.FontScale);

	public static Vector2 FONT_SCALE_TEXT_HINT => new Vector2(0.5f * Options.FontScale);

	public SelectionModeEnum SelectionMode { get; private set; }

	public bool WasFirstUpdateDone { get; private set; }

	public bool IsHideReady
	{
		get
		{
			if (PanelCargo.State != ShopMenuState.Hidden || PanelShop.State != ShopMenuState.Hidden || PanelInfo.State != ShopMenuState.Hidden)
			{
				return false;
			}
			if (ShowHidePercentage <= 0f)
			{
				return _lastHideFrameDrawed;
			}
			return false;
		}
	}

	public bool IsKeyInputFree
	{
		get
		{
			if (!GameCore.Console.IsActive && (State == ShopMenuState.Hidden || !PanelHeroStats.IsHeroNameEdited))
			{
				if (GameJnG.MainMenu.State != MainMenuStateEnum.Hidden)
				{
					return GameJnG.MainMenu.NewGameWizard.IsKeyInputFree;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsAnyPopupMenuVisible
	{
		get
		{
			if (!GameJnG.IsAnyPopupMenuVisible && YesNoEndCampaign.State == ShopMenuState.Hidden && YesNoResetHeroProfiles.State == ShopMenuState.Hidden && PanelFaceSelector.State == ShopMenuState.Hidden && ItemView.State == ItemViewStateEnum.Hidden)
			{
				return Atarix.State != AtarixStateEnum.Hidden;
			}
			return true;
		}
	}

	public ShopMenu(GameCore gameCore)
	{
		GameCore = gameCore;
	}

	public void CalcShowHideAlphaFactor(float elapsedTime)
	{
		switch (State)
		{
		case ShopMenuState.Hidden:
			ShowHidePercentage = 0f;
			break;
		case ShopMenuState.Visible:
			ShowHidePercentage = 1f;
			break;
		default:
			Utils.ChangeValueToTarget(ref ShowHidePercentage, _targetAlpha, 3f * elapsedTime);
			ShowHidePercentage = Utils.Clamp(ShowHidePercentage, 0f, 1f);
			break;
		}
	}

	private void LoadHeroPreviewLevel(int number)
	{
		GameLevel gameLevel = GameCore.GameLevel;
		if (_heroPreviewLevelNumber != number)
		{
			if (HeroPreviewLevel != null)
			{
				HeroPreviewLevel.ClearAll();
			}
			HeroPreviewLevel = new GameLevel();
			HeroPreviewLevel.Init();
			GameCore.GameLevel = HeroPreviewLevel;
			HeroPreviewLevel.LoadFromFile($"level/shop/hero_preview_{number}/level.txt");
			HeroPreviewLevel.SpawnHeroes(GameJnG.HeroList);
			HeroPreviewLevel.Begin();
			_heroPreviewLevelNumber = number;
		}
		else
		{
			GameCore.GameLevel = HeroPreviewLevel;
			HeroPreviewLevel.Camera.Position = Vector2.Zero;
			HeroPreviewLevel.Reload(null);
		}
		GameCore.GameLevel = gameLevel;
	}

	private void InitShopMenuLevel()
	{
		GUILevel = new GameLevel();
		GUILevel.Init();
		GUILevel.Begin();
		LayerShopBackground = GUILevel.CreateLayer("shop_bgr_back");
		LayerShopBackground.LightMapFactorArray[0] = 1f;
		LayerShopBackground.LightMapFactorArray[1] = 0f;
		LayerShopBackground.LightMapFactorArray[2] = 0f;
		LayerShopBackground.LightToMapNumber = 0;
		LayerShopFront = GUILevel.CreateLayer("shop_bgr_front");
		LayerShopFront.LightMapFactorArray[0] = 1f;
		LayerShopFront.LightMapFactorArray[1] = 1f;
		LayerShopFront.LightMapFactorArray[2] = 0f;
		LayerShopFront.LightToMapNumber = 1;
		LayerShop = GUILevel.CreateLayer("shop");
		LayerShop.LightMapFactorArray[0] = 0f;
		LayerShop.LightMapFactorArray[1] = 1f;
		LayerShop.LightMapFactorArray[2] = 0f;
		LayerShop.LightToMapNumber = 1;
		LayerTV = GUILevel.CreateLayer("tv1");
		LayerTV.LightMapFactorArray[0] = 0f;
		LayerTV.LightMapFactorArray[1] = 1f;
		LayerTV.LightMapFactorArray[2] = 0f;
		LayerTV.LightToMapNumber = 1;
		LayerMissionStarfield = GUILevel.CreateLayer("starfield");
		LayerMissionStarfield.LightMapFactorArray[0] = 0f;
		LayerMissionStarfield.LightMapFactorArray[1] = 0f;
		LayerMissionStarfield.LightMapFactorArray[2] = 0f;
		LayerMissionStarfield.LightToMapNumber = 0;
		LayerMissionLight = GUILevel.CreateLayer("top_light");
		LayerMissionLight.LightMapFactorArray[0] = 0f;
		LayerMissionLight.LightMapFactorArray[1] = 0f;
		LayerMissionLight.LightMapFactorArray[2] = 1f;
		LayerMissionLight.LightToMapNumber = 2;
		GUILevel.MainLayer = LayerShop;
		GUILevel.AfterLoad(resetCamera: true, mainLevel: false);
	}

	public void Init()
	{
		CurrentScreen = ShopScreenEnum.Shop;
		LightMapArray = new LightMap[3];
		for (int i = 0; i < 3; i++)
		{
			LightMapArray[i] = new LightMap();
			LightMapArray[i].Init();
		}
		AtlasSlot = GameCore.GetSpriteAtlas("shop_slot", logWhenNotFound: true);
		AtlasGemSlot = GameCore.GetSpriteAtlas("shop_gem_slot", logWhenNotFound: true);
		AtlasSlotLevel = GameCore.GetSpriteAtlas("shop_slot_level", logWhenNotFound: true);
		AtlasSlider = GameCore.GetSpriteAtlas("shop_slider", logWhenNotFound: true);
		AtlasHorizontalSlider = GameCore.GetSpriteAtlas("shop_h_slider", logWhenNotFound: true);
		AtlasWheel = GameCore.GetSpriteAtlas("shop_wheel", logWhenNotFound: true);
		AtlasButton = GameCore.GetSpriteAtlas("shop_button", logWhenNotFound: true);
		AtlasButtonPriceGlow = GameCore.GetSpriteAtlas("shop_button_price_glow", logWhenNotFound: true);
		AtlasBigButton = GameCore.GetSpriteAtlas("shop_big_button", logWhenNotFound: true);
		AtlasTabButton = GameCore.GetSpriteAtlas("shop_tab_button", logWhenNotFound: true);
		AtlasTabButtonSmall = GameCore.GetSpriteAtlas("shop_tab_button_small", logWhenNotFound: true);
		AtlasSlotTypeIcon = GameCore.GetSpriteAtlas("shop_slot_type_icon", logWhenNotFound: true);
		AtlasSymbol = GameCore.GetSpriteAtlas("menu_symbol", logWhenNotFound: true);
		AtlasOptionsElement = GameCore.GetSpriteAtlas("option_element", logWhenNotFound: true);
		AtlasTV = GameCore.GetSpriteAtlas("shop_tv", logWhenNotFound: true);
		AtlasYesNoDialog = GameCore.GetSpriteAtlas("yes_no_dialog", logWhenNotFound: true);
		AtlasPopupDialog = GameCore.GetSpriteAtlas("popup_dialog");
		AtlasFace = GameCore.GetSpriteAtlas("profile_face", logWhenNotFound: true);
		AtlasSummaryStatsBgr = GameCore.GetSpriteAtlas("shop_summary_stats_bgr", logWhenNotFound: true);
		AtlasPanelLabel = GameCore.GetSpriteAtlas("shop_panel_label", logWhenNotFound: true);
		AtlasItemSetIcon = GameCore.GetSpriteAtlas("shop_item_set_icon", logWhenNotFound: true);
		AtlasLoadout = GameCore.GetSpriteAtlas("shop_loadout", logWhenNotFound: true);
		ShopInput = new ShopInput(this);
		ItemFlySet = new ItemFlySet(this);
		InitShopMenuLevel();
		ItemView = new ItemView(this);
		Atarix = new Rake2D.Menu.Atarix.Atarix(this);
		PartData partData = GameCore.GetPartData("shop_bottom_filler_left");
		_objBottomFillerLeft = new ShopObject(this, LayerShop, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
		partData = GameCore.GetPartData("shop_bottom_filler_right");
		_objBottomFillerRight = new ShopObject(this, LayerShop, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
		ShopBackground = new ShopBackground(this);
		PanelCargo = new PanelCargo(this);
		PanelShop = new PanelShop(this);
		PanelInfo = new PanelInfo(this);
		PanelHeroStats = new PanelHeroStats(this);
		PanelShipParams = new PanelShipParams(this);
		PanelMia = new PanelMia(this);
		PanelTopButtons = new PanelTopButtons(this);
		PanelFaceSelector = new PanelFaceSelector(this);
		PanelFaceSelector.OnFaceChanged += OnFaceChanged;
		PanelFaceSelector.OnHide += OnFaceSelectorHide;
		YesNoEndCampaign = new YesNoDialog(this, "yes_no_end_campaign");
		YesNoEndCampaign.OnYesButtonPressed += OnEndCampaignYesButtonPressed;
		YesNoEndCampaign.OnNoButtonPressed += OnEndCampaignNoButtonPressed;
		YesNoResetHeroProfiles = new YesNoDialog(this, "yes_no_reset_hero_profiles");
		YesNoResetHeroProfiles.OnYesButtonPressed += OnResetHeroProfilesYesButtonPressed;
		YesNoResetHeroProfiles.OnNoButtonPressed += OnResetHeroProfilesNoButtonPressed;
		ScreenMission = new ScreenMission(this);
		FontDefault = GameCore.GlobalContent.GetBMFont("shop_font");
		if (FontDefault == null)
		{
			GameCore.LogErr("UI font not found");
		}
		FontNoBorder = GameCore.GlobalContent.GetBMFont("level_font");
		if (FontNoBorder == null)
		{
			GameCore.LogErr("UI font no border not found");
		}
		Cursor = new Cursor(this);
		TriangleBatch = new TriangleBatch(GameCore.GraphicsDeviceStatic);
		BigShip = new BigShip(this);
		BigShipSlotSet = new BigShipSlotSet(this);
		GameCore.Log("Shop menu initialized.");
		SetHintText("");
		SetMode(SelectionModeEnum.Hover);
		HeroPreviewRenderTarget = new RenderTarget2D(GameCore.GraphicsDeviceStatic, 675, 380, mipMap: false, GameCore.GraphicsDeviceStatic.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
		_levelPreviewViewPort = new Viewport(0, 0, HeroPreviewRenderTarget.Width, HeroPreviewRenderTarget.Height);
		SpriteBatchLight = new SpriteBatch(GameCore.GraphicsDeviceStatic);
		SpriteBatchNoLight = new SpriteBatch(GameCore.GraphicsDeviceStatic);
		ButtonTestRange = new Button(this, ShopButtonType.Normal);
		ButtonTestRange.OnClick += OnTestRangeButtonClick;
		ButtonTestRange.Position = new Vector2(1282f, 508f);
		ButtonTestRange.HasPressEffectEnabled = true;
		_cursorMessageList = new List<CursorMessage>();
		_diodeList = new List<ShopObject>();
		partData = GameCore.GetPartData("shop_diode_dot");
		ShopObject shopObject = new ShopObject(this, LayerShopFront, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
		shopObject.MyObject.SetPosition(-318f, 846f);
		shopObject.MyObject.ParentTransformation = this;
		_diodeList.Add(shopObject);
		partData = GameCore.GetPartData("shop_diode_power_box");
		shopObject = new ShopObject(this, LayerShopFront, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
		shopObject.MyObject.SetPosition(-317f, 257f);
		_diodeList.Add(shopObject);
		SetTexts();
	}

	private void OnFaceSelectorHide(PanelFaceSelector faceSelector)
	{
		if (CurrentController != GameInput.ControllerTypeEnum.Mouse && CurrentScreen == ShopScreenEnum.HeroProfile)
		{
			PanelHeroStats.SelectFaceTV(bPlaySound: false);
		}
	}

	private void OnFaceChanged(int faceNumber)
	{
		MyHero.FaceNumber = faceNumber;
		PanelHeroStats.SwapHeroFace(bAutoSwap: true);
	}

	private void OnEndCampaignYesButtonPressed()
	{
		Hide(toMainMenu: true, null);
	}

	private void OnEndCampaignNoButtonPressed()
	{
		if (CurrentController != GameInput.ControllerTypeEnum.Mouse)
		{
			PanelTopButtons.SelectButton(4, bPlaySound: false);
		}
	}

	private void OnResetHeroProfilesYesButtonPressed()
	{
		GameJnG.RestartHeroProfiles();
		CancelCursorSelection();
		SelectedItem = null;
		PanelInfo.SetTVItem(null);
		SwitchScreen(ShopScreenEnum.Shop);
	}

	private void OnResetHeroProfilesNoButtonPressed()
	{
	}

	public void CancelSelection()
	{
		SelectedItem = null;
		ShopBackground.CancelSelection();
		PanelCargo.CancelSelection();
		BigShipSlotSet.CancelSelection();
		PanelShop.CancelSelection();
		PanelInfo.CancelSelection();
		PanelTopButtons.CancelSelection();
		PanelHeroStats.CancelSelection();
		PanelMia.CancelSelection();
		SetCursorItem(null);
		PanelInfo.SetTVItem(null);
		SetMode(SelectionModeEnum.Hover);
	}

	public void SetTexts()
	{
		ButtonTestRange.Caption = TextBundle.GetText("button_test_range");
		PanelShipParams.SetTexts();
		PanelMia.SetTexts();
		BigShipSlotSet.SetTexts();
		ScreenMission.SetTexts();
		PanelTopButtons.SetTexts();
		PanelInfo.SetTexts();
		PanelHeroStats.SetTexts();
		YesNoEndCampaign.SetTexts();
		YesNoResetHeroProfiles.SetTexts();
		Atarix.SetTexts();
		_shipStatsLabelWidth = FontDefault.MeasureFont(TextBundle.GetText("shop_summary_hp")).X;
		_shipStatsLabelWidth = Math.Max(_shipStatsLabelWidth, FontDefault.MeasureFont(TextBundle.GetText("shop_summary_heat")).X);
		_shipStatsLabelWidth = Math.Max(_shipStatsLabelWidth, FontDefault.MeasureFont(TextBundle.GetText("shop_summary_total_dps")).X);
		_shipStatsLabelWidth = Math.Max(_shipStatsLabelWidth, FontDefault.MeasureFont(TextBundle.GetText("shop_summary_burn_dps")).X);
		_shipStatsLabelWidth = Math.Max(_shipStatsLabelWidth, FontDefault.MeasureFont(TextBundle.GetText("shop_summary_acid_dps")).X);
	}

	public CursorMessage AddCursorMessage(string text, Vector2 position, Color color)
	{
		CursorMessage cursorMessage = new CursorMessage(text, position, color, this);
		_cursorMessageList.Add(cursorMessage);
		return cursorMessage;
	}

	public void CancelCursorSelection()
	{
		SetCursorItem(null);
		PanelShop.CancelCursorSelection();
		PanelCargo.CancelCursorSelection();
		ScreenMission.CancelCursorSelection();
		IsCursorOnSlot = false;
	}

	public void SetSelectedItem(HeroItem item)
	{
		if (SelectedItem != item)
		{
			SelectedItem = item;
			if (item != null || PanelHeroStats.SelectedMedalIndex < 0)
			{
				PanelInfo.SetTVItem(item);
			}
			if (item != null)
			{
				SetMode(SelectionModeEnum.Selected);
				PanelHeroStats.SelectMedal(-1);
			}
		}
	}

	public void SetCursorItem(HeroItem item)
	{
		CursorItem = item;
	}

	public void SetHintText(string textId, HeroItem item)
	{
		string text = TextBundle.GetText(textId);
		if (item != null)
		{
			text = text.Replace("|item_name|", item.GetShopName());
			text = text.Replace("|item_price|", $"✐ {Utils.FormatMoney(item.Price)}");
			text = text.Replace("|item_up_price|", $"✐ {Utils.FormatMoney(item.GetUpgradePrice(item.Level + 1))}");
			text = text.Replace("|item_down_price|", $"✐ {Utils.FormatMoney(item.GetUpgradePrice(item.Level - 1))}");
			text = text.Replace("|item_up_level|", $"{item.Level + 2}");
			text = text.Replace("|item_down_level|", $"{item.Level}");
		}
		SetHintText(text);
	}

	public void SetHintText(string textId)
	{
		HintText = TextBundle.GetControlText(textId, (CurrentController == GameInput.ControllerTypeEnum.GamePad) ? GameInput.ControllerTypeEnum.GamePad : GameInput.ControllerTypeEnum.Mouse);
		_targetHintLineAlphaFactor = 1f;
	}

	public bool SetHero(Hero hero)
	{
		if (hero != null && MyHero != hero)
		{
			MyHero = hero;
			GameJnG.SetupHeroPartChoicesByPartMap(MyHero.MyShip.PartMap);
			BigShip.Init(MyHero);
			BigShipSlotSet.Init(MyHero);
			BigShipSlotSet.SetTexts();
			BigShipMoveFactor = Vector2.Zero;
			return true;
		}
		return false;
	}

	private void OnTestRangeButtonClick(Button button)
	{
		if (CurrentScreen != ShopScreenEnum.Mia)
		{
			GameJnG.IsTestRangeEnabled = true;
			Hide(toMainMenu: false, null);
		}
	}

	private void OnMissionButtonClick(Button button)
	{
		SwitchScreen(ShopScreenEnum.Mission);
	}

	public bool KeyPressed(ShopMenuKeys menuKey, bool bOnlyOnce = true)
	{
		if (GameCore.Console.IsActive)
		{
			return false;
		}
		if (ShopInput.KeyPressed(menuKey) && (!OnlyOneKeyPressedProtection || !bOnlyOnce))
		{
			OnlyOneKeyPressedProtection = true;
			return true;
		}
		return false;
	}

	public void Show()
	{
		WasFirstUpdateDone = false;
		if (!GameJnG.IsTestRangeEnabled)
		{
			GameJnG.IsDrunkenEffectEnabled = false;
		}
		if (State == ShopMenuState.Hidden)
		{
			_originalCameraZoom = GameCore.GameLevel.Camera.Zoom;
		}
		_targetCameraZoom = 1f;
		_cameraZoom = 1f;
		_lastHideFrameDrawed = false;
		GameCore.ChangeTimeFactor(1f);
		_cursorMessageList.Clear();
		CancelCursorSelection();
		CancelSelection();
		ButtonTestRange.CancelPressEffect();
		foreach (Hero hero in GameJnG.HeroList)
		{
			if (hero.MyObject != null)
			{
				hero.AutopilotStartPos = hero.MyObject.WorldPosition;
			}
		}
		LoadHeroPreviewLevel(0);
		GameJnG.PopupMenuInLevel.State = ShopMenuState.Hidden;
		PanelFaceSelector.State = ShopMenuState.Hidden;
		ScreenMission.State = ShopMenuState.Hidden;
		ScreenMission.SelectUnfinishedStageLevel(GameJnG.GetLastUnFinishedStage().Number);
		PanelShipParams.State = ShopMenuState.Hidden;
		PanelMia.State = ShopMenuState.Hidden;
		PanelHeroStats.State = ShopMenuState.Hidden;
		State = ShopMenuState.Showing;
		CurrentController = GameInput.ControllerTypeEnum.GamePad;
		ItemView.SetState(ItemViewStateEnum.Hidden);
		Atarix.SetState(AtarixStateEnum.Hidden);
		MyHero = null;
		SetHero(GameJnG.HeroList[0]);
		MyHero.RebuildShipConfiguration();
		BigShip.Show(0.3f);
		BigShipSlotSet.Show();
		MyHero.StartAutopilotShopMenu();
		PanelCargo.ResetToDefault();
		if (GameJnG.Mia)
		{
			CurrentScreen = ShopScreenEnum.HeroProfile;
			SwitchScreen(ShopScreenEnum.Mia);
			PanelShop.PrepareSlots();
		}
		else
		{
			PanelCargo.Show(0.25f);
			PanelShop.Show(0.25f);
			PanelInfo.Show(0.25f);
			SelectTopButton(0);
			PanelTopButtons.Show(0f);
		}
		ShopBackground.Show();
		_hintLinetAlphaFactor = -1f;
		_targetHintLineAlphaFactor = -1f;
		_targetAlpha = 1f;
		GameCore.Editor.CurrentLayer = null;
		_hideToLoadingLevelData = null;
		IsEnabled = true;
		if (GameJnG.Mia)
		{
			CurrentScreen = ShopScreenEnum.Mia;
		}
		else
		{
			CurrentScreen = ShopScreenEnum.Shop;
		}
		GameCore.Pause(paused: false);
		GameJnG.IsTestRangeEnabled = false;
	}

	public void ShowFromLevel()
	{
		State = ShopMenuState.ShowingFromLevel;
		FadeFromlevelPercentage = 0f;
	}

	public void Hide(bool toMainMenu, LevelData levelDataToLoad)
	{
		if (State == ShopMenuState.Visible || State == ShopMenuState.Showing)
		{
			State = (toMainMenu ? ShopMenuState.HidingToMainMenu : ShopMenuState.Hiding);
			PanelCargo.Hide();
			PanelShop.Hide();
			PanelInfo.Hide();
			PanelShipParams.Hide();
			PanelMia.Hide();
			PanelFaceSelector.Hide();
			PanelHeroStats.Hide();
			BigShip.Hide();
			BigShipSlotSet.Hide();
			PanelTopButtons.Hide();
			CancelSelection();
			CancelCursorSelection();
			ShopBackground.CloseGate();
			ScreenMission.Hide();
			_targetAlpha = 0f;
			_hideToLoadingLevelData = levelDataToLoad;
			GameCore.Editor.CurrentLayer = GameCore.GameLevel.MainLayer;
			SaveGame.Save(Options.SaveGameNumber);
		}
	}

	public void ToggleState()
	{
		if (State == ShopMenuState.Hidden || State == ShopMenuState.HidingToMainMenu || State == ShopMenuState.Hiding)
		{
			Show();
		}
		else
		{
			Hide(toMainMenu: false, null);
		}
	}

	public void ChangeFocus(ShopMenuSection menuSection)
	{
		_newFocusSection = menuSection;
	}

	private void UpdateFocus()
	{
		if (_newFocusSection != null)
		{
			if (FocusSection != null && _newFocusSection != FocusSection)
			{
				FocusSection.SetFocus(focus: false);
				FocusSection.CancelCursorSelection();
			}
			FocusSection = _newFocusSection;
			FocusSection.SetFocus(focus: true);
			_newFocusSection = null;
		}
	}

	public void SelectTopButton(int buttonIndex)
	{
		CancelCursorSelection();
		ShopBackground.CancelSelection();
		ButtonTestRange.Unselect();
		BigShipSlotSet.CancelCursorSelection();
		BigShipSlotSet.UnselectPlayerButton();
		PanelTopButtons.SelectButton(buttonIndex, WasFirstUpdateDone);
		PanelCargo.UnselectSortButton();
	}

	public void SelectTestRangeButton()
	{
		CancelCursorSelection();
		ButtonTestRange.Select();
		ShopBackground.CancelSelection();
		BigShipSlotSet.CancelCursorSelection();
		BigShipSlotSet.UnselectPlayerButton();
		PanelShipParams.CancelCursorSelection();
		PanelMia.CancelCursorSelection();
	}

	public void SelectPlayerButton()
	{
		CancelCursorSelection();
		ButtonTestRange.Unselect();
		BigShipSlotSet.SelectPlayerButton();
	}

	private void DebugParseInput()
	{
		if (!GameCore.Console.IsActive && IsKeyInputFree && GameInput.SystemKeyPressed(Keys.U))
		{
			GameCore.TakeScreenshot();
		}
	}

	public void SwapHero()
	{
		if (MyHero == GameJnG.HeroList[0] && GameJnG.PlayerCount > 1)
		{
			SwapToHero = GameJnG.HeroList[1];
		}
		else
		{
			SwapToHero = GameJnG.HeroList[0];
		}
		if (SwapToHero != MyHero)
		{
			GameCore.PlaySound("menu_item_move");
			BigShip.Hide();
			BigShipSlotSet.Hide();
			if (SelectedItem != null && SelectedItem.Place == ItemPlaceEnum.InHeroSlot)
			{
				CancelSelection();
			}
			PanelHeroStats.SwapHeroFace(bAutoSwap: false);
		}
	}

	private void UpdateSwapHero()
	{
		if (BigShip.State == ShopMenuState.Hidden && BigShipSlotSet.State == ShopMenuState.Hidden)
		{
			SetHero(SwapToHero);
			BigShip.Show(0f);
			BigShipSlotSet.Show();
			SwapToHero = null;
			PanelShipParams.OnSwapHero();
			PanelHeroStats.OnSwapHero();
			GameCore.PlaySound("menu_item_place");
		}
	}

	public void ParseInput()
	{
		if (Options.MasterOfTheUniverse)
		{
			DebugParseInput();
		}
		GameJnG.CheckControlerUnplugged(!IsAnyPopupMenuVisible);
		if (State != ShopMenuState.Visible || IsAnyPopupMenuVisible || GameCore.Console.IsActive)
		{
			return;
		}
		if (CurrentScreen != ShopScreenEnum.Mission && !PanelHeroStats.IsHeroNameEdited)
		{
			if (GameInput.SystemKeyPressed(Keys.G))
			{
				ShopBackground.ToggleGate();
			}
			if (GameInput.SystemKeyPressed(Keys.T))
			{
				OnTestRangeButtonClick(null);
			}
		}
		if (KeyPressed(ShopMenuKeys.CancelAll) || KeyPressed(ShopMenuKeys.CancelSelection))
		{
			if (SelectionMode == SelectionModeEnum.Selected)
			{
				CancelSelection();
				GameCore.PlaySound("menu_item_tv_hide");
			}
			else if (GameJnG.PopupMenuOptions.State == ShopMenuState.Hidden && !PanelHeroStats.IsHeroNameEdited && PanelHeroStats.SelectedMedalIndex < 0)
			{
				GameJnG.PopupMenuOptions.Show();
			}
		}
		if (ButtonTestRange.IsSelected)
		{
			if (KeyPressed(ShopMenuKeys.MoveLeft))
			{
				CancelCursorSelection();
				ButtonTestRange.Unselect();
				BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.front_gun);
			}
			else if (KeyPressed(ShopMenuKeys.MoveDown))
			{
				CancelCursorSelection();
				ButtonTestRange.Unselect();
				BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.equipment_3);
			}
			else if (KeyPressed(ShopMenuKeys.MoveUp))
			{
				ShopInput.CancelKeyPressed();
				ShopBackground.SelectGateTV();
				ButtonTestRange.Unselect();
			}
			else if (KeyPressed(ShopMenuKeys.MoveRight))
			{
				CancelCursorSelection();
				ButtonTestRange.Unselect();
				if (CurrentScreen == ShopScreenEnum.Shop)
				{
					PanelShop.SetCursorSlotByY(ButtonTestRange.Position.Y - 50f);
				}
				else if (CurrentScreen == ShopScreenEnum.HeroProfile)
				{
					PanelShipParams.SelectSliderByY(ButtonTestRange.Position.Y);
				}
			}
		}
		else if (BigShipSlotSet.IsPlayerButtonSelected)
		{
			if (KeyPressed(ShopMenuKeys.MoveUp))
			{
				CancelCursorSelection();
				BigShipSlotSet.UnselectPlayerButton();
				BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.rear_gun);
			}
			else if (KeyPressed(ShopMenuKeys.MoveLeft))
			{
				CancelCursorSelection();
				BigShipSlotSet.UnselectPlayerButton();
				if (CurrentScreen == ShopScreenEnum.HeroProfile)
				{
					PanelHeroStats.SelectFaceTV();
				}
				else
				{
					PanelCargo.SetVisibleCursorSlot(7, 2);
				}
			}
			else if (KeyPressed(ShopMenuKeys.MoveRight))
			{
				CancelCursorSelection();
				BigShipSlotSet.UnselectPlayerButton();
				BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.bomb);
			}
			else if (KeyPressed(ShopMenuKeys.MoveDown) && PanelInfo.SelectDefaultButton())
			{
				CancelCursorSelection();
				BigShipSlotSet.UnselectPlayerButton();
			}
		}
		else if (CursorItem == null && SelectedItem == null && CurrentScreen != ShopScreenEnum.Mission && BigShipSlotSet.CursorSlot == null && (PanelShop.CursorSlotIndex < 0 || CurrentScreen != ShopScreenEnum.Shop) && (PanelCargo.CursorSlotIndex < 0 || CurrentScreen != ShopScreenEnum.Shop) && (PanelShipParams.CursorSliderIndex < 0 || PanelHeroStats.State == ShopMenuState.Hidden) && (!PanelHeroStats.IsAnyItemHighlited || PanelHeroStats.State == ShopMenuState.Hidden) && (PanelHeroStats.SelectedMedalIndex < 0 || PanelHeroStats.State == ShopMenuState.Hidden) && (!PanelMia.IsAnyItemHighlited || CurrentScreen != ShopScreenEnum.Mia) && !ButtonTestRange.IsSelected && PanelTopButtons.SelectedButtonIndex < 0 && !ShopBackground.IsGateTVSelected && !PanelCargo.IsSortButtonSelected && (KeyPressed(ShopMenuKeys.MoveLeft) || KeyPressed(ShopMenuKeys.MoveRight) || KeyPressed(ShopMenuKeys.MoveUp) || KeyPressed(ShopMenuKeys.MoveDown)))
		{
			SelectTopButton(0);
		}
	}

	public void UpdateShowHide(float elapsedTime)
	{
		CalcShowHideAlphaFactor(elapsedTime);
		switch (State)
		{
		case ShopMenuState.ShowingFromLevel:
			if (Utils.ChangeValueToTarget(ref FadeFromlevelPercentage, 1f, 5f * elapsedTime))
			{
				GameCore.SoundSystem.StopAllSounds();
				GameCore.SoundSystem.SoundVolumeFactor = 1f;
				Show();
			}
			else
			{
				GameCore.SoundSystem.SoundVolumeFactor = 1f - FadeFromlevelPercentage;
			}
			break;
		case ShopMenuState.Showing:
			GameCore.SoundSystem.SoundVolumeFactor = 1f - ShowHidePercentage;
			if (ShowHidePercentage >= 1f)
			{
				GameCore.SoundSystem.StopAllSounds();
				GameCore.SoundSystem.SoundVolumeFactor = 1f;
				State = ShopMenuState.Visible;
			}
			break;
		case ShopMenuState.Hiding:
		case ShopMenuState.HidingToMainMenu:
			if (!IsHideReady)
			{
				break;
			}
			IsEnabled = false;
			if (_hideToLoadingLevelData != null)
			{
				GameJnG.IsTestRangeEnabled = false;
				GameJnG.LoadingScreen.Show(_hideToLoadingLevelData, fadeToShop: false);
			}
			else if (State == ShopMenuState.HidingToMainMenu)
			{
				GameCore.GameLevel.ClearAll();
				GameJnG.MainMenu.Show();
				GameJnG.IsTestRangeEnabled = false;
			}
			else
			{
				GameCore.SetFadeEffect(bFadeIn: true);
				GameCore.GameLevel.Camera.SetZoom(_originalCameraZoom);
				if (GameJnG.IsTestRangeEnabled)
				{
					GameCore.GameLevel.Camera.Position = Vector2.Zero;
					GameCore.GameLevel.Reload(null);
				}
				else
				{
					GameJnG.LocationText.SetLevel(GameCore.GameLevel);
					foreach (Hero hero in GameJnG.HeroList)
					{
						hero.SpawnInLevel(GameCore.GameLevel);
						hero.MyObject.SetPosition(hero.AutopilotStartPos);
						hero.CancelAutopilot();
					}
				}
			}
			State = ShopMenuState.Hidden;
			GameCore.SoundSystem.SoundVolumeFactor = 1f;
			_cursorMessageList.Clear();
			PanelFaceSelector.State = ShopMenuState.Hidden;
			GameCore.UpdateViewScaleMatrix();
			GameCore.EffectLightBasic.Update();
			break;
		case ShopMenuState.HidingToLevelLoading:
			break;
		}
	}

	private void UpdateExtraButtons(float elapsedTime)
	{
		Color color = new Color(1f, 1f, 1f, ShowHidePercentage);
		ButtonTestRange.IsInputEnabled = !IsAnyPopupMenuVisible && ScreenMission.State == ShopMenuState.Hidden && CurrentScreen != ShopScreenEnum.Mia;
		ButtonTestRange.Color = color;
		ButtonTestRange.Update(elapsedTime);
	}

	public void SetMode(SelectionModeEnum mode)
	{
		SelectionMode = mode;
	}

	public void UpdateCursorType()
	{
		if (ButtonTestRange.IsSelected)
		{
			CursorType = CursorTypeEnum.Select;
			SetHintText("hint_test_range");
		}
		else
		{
			if ((GameInput.ControlDown || GameInput.SystemGPButtonDown(Buttons.RightTrigger)) && CursorItem != null && CursorItem.Place != ItemPlaceEnum.InShop && CurrentScreen != ShopScreenEnum.Mia)
			{
				CursorType = CursorTypeEnum.Buy;
				SetHintText("hint_sell", CursorItem);
				return;
			}
			if ((GameInput.ControlDown || GameInput.SystemGPButtonDown(Buttons.RightTrigger)) && CursorItem != null && CursorItem.Place == ItemPlaceEnum.InShop)
			{
				if (CursorItem.IsTooExpensive)
				{
					CursorType = CursorTypeEnum.TooExpensive;
					SetHintText("hint_too_expensive", CursorItem);
				}
				else
				{
					CursorType = CursorTypeEnum.Buy;
					SetHintText("hint_buy", CursorItem);
				}
				return;
			}
		}
		if ((GameInput.ShiftDown || GameInput.SystemGPButtonDown(Buttons.LeftTrigger)) && CursorItem != null && CursorItem.Place == ItemPlaceEnum.InShop)
		{
			if (BigShipSlotSet.IsAnySlotFreeForItem(CursorItem))
			{
				if (CursorItem.IsTooExpensive)
				{
					CursorType = CursorTypeEnum.NoRoom;
					SetHintText("hint_too_expensive", CursorItem);
				}
				else
				{
					CursorType = CursorTypeEnum.Install;
					SetHintText("hint_buy_and_install", CursorItem);
				}
			}
			else
			{
				CursorType = CursorTypeEnum.NoRoom;
				SetHintText("hint_no_room");
			}
			return;
		}
		if ((GameInput.ShiftDown || GameInput.SystemGPButtonDown(Buttons.LeftTrigger)) && CursorItem != null && CurrentScreen != ShopScreenEnum.Mia)
		{
			if (CursorItem.Place == ItemPlaceEnum.InHeroSlot)
			{
				CursorType = CursorTypeEnum.Install;
				SetHintText("hint_remove", CursorItem);
			}
			else if (BigShipSlotSet.IsAnySlotFreeForItem(CursorItem))
			{
				CursorType = CursorTypeEnum.Install;
				SetHintText("hint_install", CursorItem);
			}
			else
			{
				CursorType = CursorTypeEnum.NoRoom;
				SetHintText("hint_no_room");
			}
			return;
		}
		switch (SelectionMode)
		{
		case SelectionModeEnum.Hover:
			if (CursorItem != null)
			{
				CursorType = CursorTypeEnum.Select;
				if (CursorItem.Place == ItemPlaceEnum.InShop)
				{
					SetHintText("hint_select_shop");
				}
				else
				{
					SetHintText("hint_select");
				}
			}
			break;
		case SelectionModeEnum.Selected:
			if (BigShipSlotSet.CursorSlot != null && !BigShipSlotSet.CursorSlot.IsCompatible(SelectedItem))
			{
				CursorType = CursorTypeEnum.Denied;
				SetHintText("hint_incompatible_slot", SelectedItem);
			}
			else if (CursorItem != null && CursorItem != SelectedItem)
			{
				if (SelectedItem == null || (SelectedItem != null && CursorItem.Place == ItemPlaceEnum.InShop))
				{
					CursorType = CursorTypeEnum.Select;
					if (CurrentController == GameInput.ControllerTypeEnum.GamePad)
					{
						SetHintText("hint_select_shop_gp");
					}
					else
					{
						SetHintText("hint_select_shop");
					}
				}
				else if (SelectedItem.Place != ItemPlaceEnum.InShop && (SelectedItem.MySlot == null || SelectedItem.MySlot.IsCompatible(CursorItem)))
				{
					CursorType = CursorTypeEnum.Swap;
					SetHintText("hint_swap");
				}
				else if (SelectedItem.Place == ItemPlaceEnum.InShop && CursorItem.Place == ItemPlaceEnum.InHeroSlot && CursorItem.MySlot.IsCompatible(SelectedItem))
				{
					if (SelectedItem.IsTooExpensive)
					{
						CursorType = CursorTypeEnum.TooExpensive;
						SetHintText("hint_too_expensive", SelectedItem);
					}
					else
					{
						CursorType = CursorTypeEnum.Buy;
						SetHintText("hint_buy_and_replace", SelectedItem);
					}
				}
				else if (SelectedItem.Place == ItemPlaceEnum.InShop && CursorItem.Place == ItemPlaceEnum.InCargo)
				{
					CursorType = CursorTypeEnum.Denied;
					SetHintText("hint_buy_to_occupied_slot");
				}
				else
				{
					CursorType = CursorTypeEnum.Denied;
					SetHintText("hint_incompatible_items", SelectedItem);
				}
			}
			else if (IsCursorOnSlot && CursorItem != SelectedItem)
			{
				if (SelectedItem.Place == ItemPlaceEnum.InShop)
				{
					if (SelectedItem.Price > GameJnG.Cash)
					{
						CursorType = CursorTypeEnum.TooExpensive;
						SetHintText("hint_too_expensive", SelectedItem);
					}
					else
					{
						CursorType = CursorTypeEnum.Buy;
						SetHintText("hint_buy", SelectedItem);
					}
				}
				else if (PanelCargo.CursorSlotIndex >= 0)
				{
					CursorType = CursorTypeEnum.Place;
					SetHintText("hint_place_to_cargo", SelectedItem);
				}
				else if (BigShipSlotSet.CursorSlot != null)
				{
					CursorType = CursorTypeEnum.Place;
					SetHintText("hint_place_to_slot", SelectedItem);
				}
				else
				{
					CursorType = CursorTypeEnum.Unselect;
					SetHintText("hint_cancel");
				}
			}
			else if (ItemView.State != ItemViewStateEnum.Hidden)
			{
				CursorType = CursorTypeEnum.Unselect;
				SetHintText("hint_hide_item_view");
			}
			else if (CursorType == CursorTypeEnum.NoAction && !PanelInfo.IsAnyButtonSelected && Atarix.State == AtarixStateEnum.Hidden)
			{
				if (CurrentScreen != ShopScreenEnum.Mission)
				{
					CursorType = CursorTypeEnum.Unselect;
				}
				SetHintText("hint_cancel");
			}
			break;
		}
	}

	public void SwitchScreen(ShopScreenEnum screen)
	{
		if (screen == CurrentScreen)
		{
			return;
		}
		GameCore.PlaySound("menu_screen_switch");
		switch (screen)
		{
		case ShopScreenEnum.Mission:
			PanelHeroStats.Hide();
			PanelShipParams.Hide();
			PanelShop.Hide();
			PanelCargo.Hide();
			PanelFaceSelector.Hide();
			BigShipSlotSet.Hide();
			ScreenMission.Show();
			break;
		case ShopScreenEnum.Shop:
			foreach (Hero hero in GameJnG.HeroList)
			{
				hero.AutopilotMode = HeroAutopilotMode.ShopMenuPreview;
			}
			PanelHeroStats.Hide();
			PanelShipParams.Hide();
			PanelFaceSelector.Hide();
			PanelTopButtons.Show(0f);
			PanelCargo.Show(0.25f);
			PanelShop.Show(0.25f);
			PanelInfo.Show(0.25f);
			ScreenMission.Hide();
			_targetCameraZoom = 1f;
			break;
		case ShopScreenEnum.HeroProfile:
			foreach (Hero hero2 in GameJnG.HeroList)
			{
				hero2.AutopilotMode = HeroAutopilotMode.ShopMenuProfile;
			}
			PanelShop.Hide();
			PanelCargo.Hide();
			PanelFaceSelector.Hide();
			PanelHeroStats.Show(0.25f);
			PanelShipParams.Show(0.25f);
			ScreenMission.Hide();
			_targetCameraZoom = 2.7f;
			break;
		case ShopScreenEnum.Mia:
			foreach (Hero hero3 in GameJnG.HeroList)
			{
				hero3.AutopilotMode = HeroAutopilotMode.ShopMenuProfile;
			}
			PanelShop.Hide();
			PanelCargo.Hide();
			PanelFaceSelector.Hide();
			ScreenMission.Hide();
			PanelTopButtons.Hide();
			PanelHeroStats.Show(0.25f);
			PanelMia.Show(0.25f);
			PanelInfo.Show(0.25f);
			_targetCameraZoom = 2.7f;
			break;
		}
		if (CurrentScreen == ShopScreenEnum.Mission)
		{
			BigShipSlotSet.Show();
		}
		CurrentScreen = screen;
	}

	public void EnableLayersByScreen()
	{
		if (ScreenMission.State != ShopMenuState.Hidden)
		{
			LayerMissionStarfield.IsEnabled = true;
			LayerMissionLight.IsEnabled = true;
		}
		else
		{
			LayerMissionStarfield.IsEnabled = false;
			LayerMissionLight.IsEnabled = false;
		}
		if (ScreenMission.State == ShopMenuState.Visible)
		{
			LayerShop.IsEnabled = false;
			LayerShopBackground.IsEnabled = false;
			LayerShopFront.IsEnabled = false;
			LayerTV.IsEnabled = false;
		}
		else
		{
			LayerShop.IsEnabled = true;
			LayerShopBackground.IsEnabled = true;
			LayerShopFront.IsEnabled = true;
			LayerTV.IsEnabled = true;
		}
	}

	public void ActualizeCampaign()
	{
		ScreenMission.ActualizeCampaign();
	}

	private void RefreshCursorItem()
	{
		if (CursorItem != null)
		{
			return;
		}
		HeroItem slotItem = PanelCargo.GetSlotItem(PanelCargo.CursorSlotIndex);
		if (slotItem != null)
		{
			SetCursorItem(slotItem);
			return;
		}
		if (BigShipSlotSet.CursorSlot != null && BigShipSlotSet.CursorSlot.HeroSlot.Item != null)
		{
			SetCursorItem(BigShipSlotSet.CursorSlot.HeroSlot.Item);
			return;
		}
		slotItem = PanelShop.GetSlotItem(PanelShop.CursorSlotIndex);
		if (slotItem != null)
		{
			SetCursorItem(slotItem);
		}
	}

	public void Update(float elapsedTime)
	{
		WasFirstUpdateDone = true;
		_slotBlinkTimer += elapsedTime * 6f;
		_slotBlinkAlpha = 0.75f + Utils.Sin(_slotBlinkTimer) * 0.25f;
		if (ItemView.State != ItemViewStateEnum.Hidden)
		{
			ItemView.Update(elapsedTime);
			GameCore.CurrentLevel = GameCore.GameLevel;
		}
		OnlyOneKeyPressedProtection = false;
		CursorType = CursorTypeEnum.NoAction;
		if (Atarix.State != AtarixStateEnum.Hidden)
		{
			Atarix.Update(elapsedTime);
		}
		if (CurrentScreen == ShopScreenEnum.Shop && (MyHero.MyObject == null || MyHero.MyObject.MyLevel != HeroPreviewLevel))
		{
			GameCore.CurrentLevel = HeroPreviewLevel;
			MyHero.RecreateSmallShip = true;
		}
		if (ScreenMission.State != ShopMenuState.Visible && ShopBackground.GateState != GateStateEnum.Closed)
		{
			RenderHeroPreviewLevel(GameCore.ElapsedTime);
		}
		RefreshCursorItem();
		GameCore.UseShopLightMaps = true;
		GameCore.CurrentLevel = GUILevel;
		GameCore.CurrentLevel.CurrentLayer = GameCore.CurrentLevel.MainLayer;
		ShopInput.Update(elapsedTime);
		ParseInput();
		_targetHintLineAlphaFactor = -1f;
		if (_hintLinetAlphaFactor <= -1f)
		{
			HintText = null;
		}
		if (ScreenMission.State != ShopMenuState.Visible)
		{
			ShopBackground.Update(elapsedTime);
		}
		_selectionAlpha += elapsedTime * 4f;
		Cursor.Update(elapsedTime);
		if (SwapToHero != null)
		{
			UpdateSwapHero();
		}
		UpdateFocus();
		UpdateShowHide(elapsedTime);
		if (State != ShopMenuState.Hidden)
		{
			if (BigShip.State == ShopMenuState.Visible)
			{
				BigShipMoveFactor.X += 0.9f * elapsedTime;
				BigShipMoveFactor.Y += 1.5f * elapsedTime;
				Vector2 vector = new Vector2(Utils.Sin(BigShipMoveFactor.X) * 6f, Utils.Sin(BigShipMoveFactor.Y) * 10f);
				BigShip.MyObject.SetPosition(BigShip.TargetPosition + vector);
			}
			else
			{
				BigShipMoveFactor = Vector2.Zero;
			}
			BigShip.Update(elapsedTime);
			BigShipSlotSet.Update(elapsedTime);
			PanelCargo.Update(elapsedTime);
			PanelShop.Update(elapsedTime);
			PanelInfo.Update(elapsedTime);
			PanelHeroStats.Update(elapsedTime);
			PanelShipParams.Update(elapsedTime);
			PanelMia.Update(elapsedTime);
			PanelFaceSelector.Update(elapsedTime);
			PanelTopButtons.Update(elapsedTime);
			_objBottomFillerLeft.SetPosition(new Vector2(0f, PanelInfo.Position.Y + 340f));
			_objBottomFillerLeft.Update(elapsedTime);
			_objBottomFillerRight.SetPosition(new Vector2(1920f, PanelInfo.Position.Y + 340f));
			_objBottomFillerRight.Update(elapsedTime);
			if (ScreenMission.State != ShopMenuState.Hidden)
			{
				ScreenMission.Update(elapsedTime);
			}
			else
			{
				Utils.ChangeValueToTarget(ref _cameraZoom, _targetCameraZoom, 5f * elapsedTime);
				HeroPreviewLevel.Camera.SetZoom(_cameraZoom);
			}
			UpdateExtraButtons(elapsedTime);
			GUILevel.Camera.SetZoom(1f);
			if (ScreenMission.State != ShopMenuState.Visible)
			{
				GUILevel.Camera.Position = new Vector2(0f, 0f);
			}
			EnableLayersByScreen();
			GUILevel.Update(elapsedTime);
			if (State != ShopMenuState.Hidden)
			{
				if (CurrentController == GameInput.ControllerTypeEnum.Mouse && !IsCursorOnSlot)
				{
					CancelCursorSelection();
				}
				ItemFlySet.Update(elapsedTime);
				UpdateCursorType();
				Utils.ChangeValueToTarget(ref _hintLinetAlphaFactor, _targetHintLineAlphaFactor, 10f * elapsedTime);
			}
		}
		if (GameJnG.PopupMenuOptions.State != ShopMenuState.Hidden)
		{
			GameJnG.PopupMenuOptions.Update(elapsedTime);
		}
		else if (YesNoEndCampaign.State != ShopMenuState.Hidden)
		{
			YesNoEndCampaign.Update(elapsedTime);
		}
		else if (YesNoResetHeroProfiles.State != ShopMenuState.Hidden)
		{
			YesNoResetHeroProfiles.Update(elapsedTime);
		}
		if (!IsAnyPopupMenuVisible && Cursor.CursorType == CursorTypeEnum.Unselect && CursorItem == null && (KeyPressed(ShopMenuKeys.Select) || KeyPressed(ShopMenuKeys.Action) || KeyPressed(ShopMenuKeys.CancelSelection)))
		{
			if (PanelInfo.TVItem != null)
			{
				GameCore.PlaySound("menu_item_tv_hide");
			}
			CancelSelection();
		}
		Cursor.SetCursorType(CursorType);
		for (int num = _cursorMessageList.Count - 1; num >= 0; num--)
		{
			CursorMessage cursorMessage = _cursorMessageList[num];
			cursorMessage.Update(elapsedTime);
			if (cursorMessage.IsDestroyed)
			{
				_cursorMessageList.RemoveAt(num);
			}
		}
		foreach (ShopObject diode in _diodeList)
		{
			switch (CurrentScreen)
			{
			case ShopScreenEnum.Shop:
				diode.MyObject.ParentTransformation = PanelShop;
				break;
			case ShopScreenEnum.HeroProfile:
				diode.MyObject.ParentTransformation = PanelShipParams;
				break;
			case ShopScreenEnum.Mia:
				diode.MyObject.ParentTransformation = PanelMia;
				break;
			case ShopScreenEnum.Mission:
				diode.MyObject.ParentTransformation = Transformation.CreateByPosition(2220f + (1f - ScreenMission.ShowHidePercentage) * 69f + 1f, 0f);
				break;
			}
			diode.Update(elapsedTime);
		}
		RenderLightMaps();
	}

	public void RenderLightMaps()
	{
		LightMap[] lightMapArray = LightMapArray;
		for (int i = 0; i < lightMapArray.Length; i++)
		{
			lightMapArray[i].Render(GUILevel.Camera);
		}
		GameCore.GraphicsDeviceStatic.Viewport = GameCore.MainViewPort;
	}

	public Color GetSlotFrameColor(HeroItem item)
	{
		return Color.White;
	}

	public Color GetSlotSelectedColor()
	{
		int alpha = (int)(200.0 + Math.Sin(_selectionAlpha) * 55.0);
		return new Color(255, 255, 255, alpha);
	}

	public Color GetSlotCursorColor()
	{
		switch (Cursor.CursorType)
		{
		case CursorTypeEnum.Select:
			return Color.White;
		case CursorTypeEnum.NoAction:
			return Color.White;
		case CursorTypeEnum.TooExpensive:
		case CursorTypeEnum.Denied:
			return new Color(200, 0, 0);
		default:
			if (CursorItem != null && CursorItem.Place == ItemPlaceEnum.InShop)
			{
				return Color.White;
			}
			return new Color(40, 160, 255);
		}
	}

	public string GetParamText(HeroItem item, string paramName)
	{
		item.TryGetBaseParamSummary(paramName, out var value, out var finalValue);
		string text = TextBundle.GetText($"param_{paramName}");
		if (text != null && text.Contains("@"))
		{
			bool flag;
			if (value != 0f || finalValue == 0f)
			{
				string text2 = ((!float.IsNaN(value) && !float.IsInfinity(value)) ? Utils.FormatBigNumber(value) : "!#@WTF?");
				string arg = ((!(value < 0f)) ? "\\1" : "\\9");
				text = text.Replace("@val", $"{arg}{text2}");
				text = text.Replace("@sig_val", string.Format("{0}{1}{2}", arg, (value < 0f) ? "" : "+", text2));
				text = text.Replace("@total_dps_percent", $"{arg}{Utils.FormatBigNumber(MyHero.GetTotalDPS() * (value / 100f))}");
				HeroItem weaponItemForGem = item.GetWeaponItemForGem();
				if (weaponItemForGem != null)
				{
					if (!weaponItemForGem.TryGetBaseParamSummary("dps", out var value2, out var finalValue2) && !weaponItemForGem.TryGetBaseParamSummary("collision_dps", out value2, out finalValue2))
					{
						finalValue2 = 0f;
					}
					text = text.Replace("@weapon_dps_percent", string.Format("{0}{1}{2}", arg, (value < 0f) ? "" : "+", Utils.FormatBigNumber(finalValue2 * (value / 100f))));
				}
				else
				{
					text = text.Replace("@weapon_dps_percent", TextBundle.GetText("param_no_weapon_for_gem"));
				}
				arg = ((!(value > 0f)) ? "\\1" : "\\9");
				text = text.Replace("@neg_sig_val", string.Format("{0}{1}{2}", arg, (value < 0f) ? "" : "+", text2));
				float value3 = 0f;
				float finalValue3 = 0f;
				HeroSlot slot = MyHero.GetSlot(HeroSlotEnum.bomb);
				if (slot.Item != null && slot.Item.Data.ItemType == ItemTypeEnum.Bomb)
				{
					slot.Item.TryGetBaseParamSummary("bomb_damage", out value3, out finalValue3);
				}
				text = text.Replace("@bomb_damage_percent", $"\\1{Utils.FormatBigNumber(finalValue3 * (value / 100f))}");
				flag = true;
			}
			else
			{
				text = text.Replace("@val", "");
				text = text.Replace("@sig_val", "");
				text = text.Replace("@total_dps_perecent", "");
				text = text.Replace("@bomb_damage_percent", "");
				flag = false;
			}
			float num = finalValue - value;
			if (num != 0f)
			{
				int num2 = (((!(num < 0f) || HeroItem.IsParamLessBetter(paramName)) && (!(num > 0f) || !HeroItem.IsParamLessBetter(paramName))) ? 8 : 9);
				text = ((!flag) ? text.Replace("@bonus", $"\\{num2}{Utils.FormatBigNumber(num)}") : text.Replace("@bonus", string.Format(" \\{0}{1}{2}", num2, (num > 0f) ? "+ " : "- ", Utils.FormatBigNumber(Math.Abs(num)))));
			}
			else
			{
				text = text.Replace("@bonus", "");
			}
		}
		return text;
	}

	public void SellItem(HeroItem item)
	{
		if (item != null)
		{
			ItemFlySet.StartToShop(item);
			CancelSelection();
			GameCore.PlaySound("menu_money");
			AddCursorMessage($"+{Utils.FormatMoney(item.Price)}", new Vector2(Position.X + 159f, Position.Y + 995f), new Color(255, 255, 0));
			PanelCargo.SetCashEffect();
			GameJnG.SellItem(item);
			PanelShop.ScrollToSlot(PanelShop.GetSlotIndex(item));
		}
	}

	public void BuyItem(HeroItem item)
	{
		GameJnG.Cash -= item.Price;
		GameCore.PlaySound("menu_money");
		AddCursorMessage($"{Utils.FormatMoney(-item.Price)}", new Vector2(Position.X + 159f, Position.Y + 995f), Color.Red);
		PanelCargo.SetCashEffect();
	}

	public void UpgradeItem(HeroItem item)
	{
		if (item != null && item.IsUpgradable && item.Level < item.Data.MaxLevel)
		{
			int upgradePrice = item.GetUpgradePrice(item.Level + 1);
			if (upgradePrice <= GameJnG.Cash)
			{
				ChangeItemLevel(item, 1);
				GameJnG.Cash -= upgradePrice;
				GameCore.PlaySound("menu_money");
				AddCursorMessage($"-{Utils.FormatMoney(upgradePrice)}", new Vector2(Position.X + 159f, Position.Y + 995f), Color.Red);
				PanelCargo.SetCashEffect();
			}
			PanelInfo.TVItem = null;
			PanelInfo.SetTVItem(SelectedItem);
		}
	}

	public void DowngradeItem(HeroItem item)
	{
		if (item != null && item.Level > 0)
		{
			int upgradePrice = item.GetUpgradePrice(item.Level - 1);
			ChangeItemLevel(item, -1);
			GameJnG.Cash += upgradePrice;
			GameCore.PlaySound("menu_money");
			AddCursorMessage($"+{Utils.FormatMoney(upgradePrice)}", new Vector2(Position.X + 159f, Position.Y + 995f), new Color(255, 255, 0));
			PanelCargo.SetCashEffect();
		}
		PanelInfo.TVItem = null;
		PanelInfo.SetTVItem(SelectedItem);
	}

	public void ChangeItemLevel(HeroItem item, int deltaLevel)
	{
		if (item == null || !item.IsUpgradable || deltaLevel == 0)
		{
			return;
		}
		if (deltaLevel > 0)
		{
			if (item.Level + deltaLevel <= item.Data.MaxLevel)
			{
				item.Level += deltaLevel;
			}
		}
		else if (item.Level + deltaLevel >= 0)
		{
			item.Level += deltaLevel;
		}
		if (item.MySlot != null)
		{
			item.MySlot.MyHero.RebuildShipConfiguration();
		}
		else
		{
			GameJnG.RecalculateItemParams(null);
		}
	}

	public void ApplyLoadoutItem(HeroItem item)
	{
		int cargoIndex = item.CargoIndex;
		GameJnG.Cargo.RemoveItem(item);
		GameJnG.Cargo.SetItem(cargoIndex, MyHero.CurrentLoadoutItem);
		MyHero.CurrentLoadoutItem = item;
		MyHero.ApplyLoadout(item.Loadout);
		Utils.Clamp(PanelCargo.GetSlotPosition(cargoIndex), new Vector2(0f, 100f), new Vector2(300f, 1030f));
		CancelSelection();
		GameCore.PlaySound("menu_item_place");
	}

	public void SwapItems(HeroItem firstItem, HeroItem secondItem)
	{
		if (secondItem == null)
		{
			return;
		}
		if (firstItem != null)
		{
			switch (firstItem.Place)
			{
			case ItemPlaceEnum.InCargo:
				if (secondItem.Place == ItemPlaceEnum.InCargo)
				{
					GameJnG.Cargo.SwapItems(firstItem.CargoIndex, secondItem.CargoIndex);
				}
				else if (secondItem.Place == ItemPlaceEnum.InHeroSlot)
				{
					GameJnG.Cargo.SwapItemWithSlot(firstItem.CargoIndex, MyHero.MyShip, secondItem.MySlot.SlotType);
				}
				break;
			case ItemPlaceEnum.InHeroSlot:
				if (secondItem.Place == ItemPlaceEnum.InCargo)
				{
					GameJnG.Cargo.SwapItemWithSlot(secondItem.CargoIndex, MyHero.MyShip, firstItem.MySlot.SlotType);
				}
				else if (secondItem.Place == ItemPlaceEnum.InHeroSlot)
				{
					HeroSlot mySlot = secondItem.MySlot;
					firstItem.MySlot.SetItem(secondItem);
					mySlot.SetItem(firstItem);
				}
				break;
			}
		}
		CancelSelection();
	}

	private void RenderHeroPreviewLevel(float elapsedTime)
	{
		GameCore.UseRenderTarget = true;
		GameCore.CurrentLevel = HeroPreviewLevel;
		GameCore.MainViewPort = _levelPreviewViewPort;
		GameCore.UpdateViewScaleMatrix();
		HeroPreviewLevel.Update(elapsedTime);
		GameCore.RenderLightAndShadow(HeroPreviewLevel.Camera);
		GameCore.GraphicsDeviceStatic.SetRenderTarget(HeroPreviewRenderTarget);
		GameCore.UpdateViewScaleMatrix();
		GameCore.EffectLightBasic.Update();
		HeroPreviewLevel.Draw(showRenderTarget: false);
		GameCore.GraphicsDeviceStatic.SetRenderTarget(GameCore.ScreenRenderTarget);
		GameCore.MainViewPort = GameCore.GraphicsDeviceStatic.Viewport;
		GameCore.UpdateViewScaleMatrix();
		GameCore.EffectLightBasic.Update();
	}

	public void DrawHeroPreviewLevel(Vector2 position)
	{
		GameCore.UpdateViewScaleMatrix();
		GameCore.EffectLightBasic.Update();
		byte b = (byte)(255f * ShowHidePercentage);
		GameCore.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, GameCore.RasterizerState, null, GameCore.ViewPortMatrix);
		GameCore.SpriteBatch.Draw(HeroPreviewRenderTarget, position, null, new Color(b, b, b));
		GameCore.SpriteBatch.End();
	}

	private void DrawHintLine()
	{
		if (_hintLinetAlphaFactor > 0f && HintText != null)
		{
			float y = 1054f - 5f * Options.FontScale;
			SpriteBatchBegin(bLight: false);
			FontDefault.Write(SpriteBatchNoLight, HintText, 960f, y, FONT_SCALE_TEXT_HINT, new Color(255, 255, 255), TextAlign.Center, 0, _hintLinetAlphaFactor * ShowHidePercentage);
			SpriteBatchEnd(bLight: false);
		}
	}

	public void DrawSlot(SpriteAtlas atlasSlot, Transformation trans, HeroItem item, float alphaFactor, bool bAsLoadoutPreview)
	{
		Color color = new Color(1f, 1f, 1f, alphaFactor);
		atlasSlot.DrawFrame(SpriteBatchLight, bAsLoadoutPreview ? 8 : 0, color, trans);
		if (item == null || ItemFlySet.Contains(item))
		{
			return;
		}
		if (item.Data.IconAtlas != null)
		{
			if (bAsLoadoutPreview)
			{
				item.Data.IconAtlas.DrawFrame(SpriteBatchLight, item.GetIconFrame(), new Color(color, _slotBlinkAlpha), trans, BlendModeEnum.Additive);
			}
			else
			{
				item.Data.IconAtlas.DrawFrame(SpriteBatchLight, item.GetIconFrame(), color, trans, BlendModeEnum.Normal);
			}
		}
		if (item.IsMyItemSetComplete && item.Place == ItemPlaceEnum.InHeroSlot)
		{
			AtlasItemSetIcon.DrawFrame(SpriteBatchLight, 0, color, trans);
		}
		if (item.IsUpgradable && (item.Place != ItemPlaceEnum.InShop || item.Level > 0))
		{
			if (item.Level < item.Data.MaxLevel && !item.IsUpgradeTooExpensive)
			{
				AtlasSlotLevel.DrawFrame(SpriteBatchLight, item.Level + 4, color, trans);
			}
			else
			{
				AtlasSlotLevel.DrawFrame(SpriteBatchLight, item.Level, color, trans);
			}
		}
		if (item.Data.ItemType == ItemTypeEnum.Loadout && item.Loadout != null)
		{
			AtlasSlot.DrawFrame(SpriteBatchLight, 7, color, trans, BlendModeEnum.Additive);
			FontDefault.Write(SpriteBatchLight, $"{item.Loadout.Number}", trans.WorldPosition.X + 10f, trans.WorldPosition.Y + 37f, new Vector2(0.5f), color, TextAlign.Center, 0, 1f, -1f, BlendModeEnum.Additive);
		}
		if (item.Place == ItemPlaceEnum.InShop && item.Data.ShopStageRange.From == GameJnG.StageNumber + Math.Max(1, GameJnG.Difficulty) * 1000)
		{
			AtlasSlot.DrawFrame(SpriteBatchLight, 9, color, trans, BlendModeEnum.Normal);
		}
	}

	internal void DrawSummaryStats()
	{
		int num = 18;
		Transformation transformation = Transformation.CreateByPosition(1020f, 432f);
		AtlasSummaryStatsBgr.DrawFrame(SpriteBatchNoLight, 0, new Color(255, 255, 255, (int)(255f * ShowHidePercentage)), transformation, BlendModeEnum.Normal);
		Vector2 scale = new Vector2(Math.Min(FONT_SCALE_LABEL_SMALL.X, 0.5f));
		Vector2 vector = transformation.Position + new Vector2(2f, 7f);
		float num2 = vector.X + _shipStatsLabelWidth * scale.X + 12f;
		Color value = new Color(255, 255, 0, (int)(255f * ShowHidePercentage));
		Color color = new Color(90, 210, 255, (int)(255f * ShowHidePercentage));
		FontDefault.Write(SpriteBatchNoLight, TextBundle.GetText("shop_summary_hp") + ":", vector.X, vector.Y, scale, value);
		string text = ((int)((MyHero.MyObject != null) ? MyHero.MyObject.MaxHP : 0f)).ToString();
		FontDefault.Write(SpriteBatchNoLight, $"{text}", num2, vector.Y, scale, color);
		if (MyHero.MaxShield > 0f)
		{
			FontDefault.Write(SpriteBatchNoLight, $" + {(int)MyHero.MaxShield}", num2 + FontDefault.MeasureFont(text).X * scale.X, vector.Y, scale, color);
		}
		vector.Y += num;
		FontDefault.Write(SpriteBatchNoLight, TextBundle.GetText("shop_summary_heat") + ":", vector.X, vector.Y, scale, value);
		Color value2;
		if (MyHero.HeatPercent > 0.9f)
		{
			value2 = new Color(255, 0, 0, (int)(255f * ShowHidePercentage));
		}
		else
		{
			value2 = ((!(MyHero.HeatPercent > 0.75f)) ? color : new Color(255, 200, 0, (int)(255f * ShowHidePercentage)));
		}
		FontDefault.Write(SpriteBatchNoLight, $"{Utils.FormatDecimal1(MyHero.GetTotalHeatPS())} ({(int)(MyHero.HeatPercent * 100f)} %)", num2, vector.Y, scale, value2, TextAlign.Left, 80);
		vector.Y += num;
		FontDefault.Write(SpriteBatchNoLight, TextBundle.GetText("shop_summary_total_dps") + ":", vector.X, vector.Y, scale, value);
		FontDefault.Write(SpriteBatchNoLight, $"{Utils.FormatBigNumber(MyHero.GetTotalDPS())}", num2, vector.Y, scale, color);
		vector.Y += num;
	}

	private void DrawExtraButtons()
	{
		if (CurrentScreen != ShopScreenEnum.Mia)
		{
			SpriteBatchBegin(bLight: true);
			ButtonTestRange.Draw();
			SpriteBatchEnd(bLight: true);
		}
	}

	public float ChangeLightFactor(float lightFactor)
	{
		float lightFactor2 = GameCore.EffectLight.LightFactor;
		SpriteBatchLight.End();
		GameCore.EffectLight.SetLightFactor(LayerShop, lightFactor);
		SpriteBatchBegin(bLight: true);
		return lightFactor2;
	}

	public void SpriteBatchBegin(bool bLight)
	{
		GameCore.SetViewScissorRectangle();
		if (bLight)
		{
			SpriteBatchLight.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, GameCore.EffectLight.MyEffect, GameCore.ViewPortMatrix);
			if (!GameCore.IsShopMenuEnabled)
			{
				GameCore.EffectLight.SetLightFactor(GameCore.GameLevel.MainLayer, 0f);
			}
			else
			{
				GameCore.EffectLight.SetLightFactor(LayerShop, 1f);
			}
		}
		else
		{
			SpriteBatchNoLight.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, GameCore.ViewPortMatrix);
		}
	}

	public void SpriteBatchEnd(bool bLight)
	{
		if (bLight)
		{
			SpriteBatchLight.End();
		}
		else
		{
			SpriteBatchNoLight.End();
		}
	}

	public void DrawGameModeSkull(GameModeEnum gameMode, Transformation trans, int alpha)
	{
		int frameNumber;
		switch (gameMode)
		{
		case GameModeEnum.HardCore:
			frameNumber = 1;
			break;
		default:
			frameNumber = 0;
			break;
		case GameModeEnum.Casual:
			return;
		}
		trans.Position.Y += 99f;
		PanelHeroStats.AtlasBadge.DrawFrame(SpriteBatchLight, frameNumber, new Color(255, 255, 255, alpha), trans);
	}

	private void DrawBottomFiller(Color color)
	{
		SpriteBatchBegin(bLight: true);
		_objBottomFillerLeft.Draw();
		_objBottomFillerRight.Draw();
		SpriteBatchEnd(bLight: true);
	}

	private void DrawLoadoutItem()
	{
		SpriteBatchBegin(bLight: false);
		Transformation transformation = Transformation.CreateByPosition(480f, 425f);
		Color color = new Color(1f, 1f, 1f, 0.8f * BigShip.ShowHidePercentage * ShowHidePercentage);
		AtlasLoadout.DrawFrame(SpriteBatchNoLight, 0, color, transformation);
		color = new Color(1f, 1f, 1f, BigShip.ShowHidePercentage * ShowHidePercentage);
		GameCore.FontNoBorder.Write(SpriteBatchNoLight, TextBundle.GetText("shop_loadout"), transformation.Position.X - 45f, transformation.Position.Y - 5f - 6f * Options.FontScale, FONT_SCALE_LABEL, color, TextAlign.Center, 0, 1f, -1f, BlendModeEnum.Additive);
		Transformation transformation2 = new Transformation(transformation);
		transformation2.Position.X += 10f;
		transformation2.Position.Y -= 28f;
		AtlasSlot.DrawFrame(SpriteBatchNoLight, 7, color, transformation2, BlendModeEnum.Additive);
		GameCore.FontNoBorder.Write(SpriteBatchNoLight, MyHero.CurrentLoadoutItem.Loadout.Number.ToString(), transformation2.Position.X + 9f, transformation2.Position.Y + 36f, FONT_SCALE_LABEL, color, TextAlign.Center, 0, 1f, -1f, BlendModeEnum.Additive);
		if (MyHero.IsLoadoutSwapperEnabled)
		{
			transformation.Position.X += 47f;
			transformation.Position.Y -= 2f;
			AtlasLoadout.DrawFrame(SpriteBatchNoLight, 1 + MyHero.PlayerNumber, color, transformation, BlendModeEnum.Normal);
		}
		SpriteBatchEnd(bLight: false);
	}

	public void Draw()
	{
		if (!WasFirstUpdateDone)
		{
			return;
		}
		if ((State == ShopMenuState.Hiding || State == ShopMenuState.HidingToLevelLoading || State == ShopMenuState.HidingToMainMenu) && ShowHidePercentage <= 0f)
		{
			_lastHideFrameDrawed = true;
		}
		GUILevel.SetCurrentLayer(LayerShop);
		GameCore.GraphicsDeviceStatic.SetRenderTarget(GameCore.LevelRenderTarget);
		GameCore.UpdateViewScaleMatrix();
		GameCore.UseRenderTarget = true;
		GameCore.SetDefaultRenderTarget();
		if (State != ShopMenuState.Hidden && ScreenMission.State != ShopMenuState.Visible && ShowHidePercentage > 0f && (CurrentScreen != ShopScreenEnum.Mission || (State != ShopMenuState.Hiding && State != ShopMenuState.HidingToLevelLoading && State != ShopMenuState.HidingToMainMenu)))
		{
			if (ShopBackground.GateState != GateStateEnum.Closed)
			{
				DrawHeroPreviewLevel(new Vector2(1010f, 123f));
			}
			GameCore.EffectLightBasic.SetLightMaps(bUseShopMaps: true);
			GameCore.EffectLight.SetLightMaps(bUseShopMaps: true);
			ShopBackground.Draw();
			BigShipSlotSet.Alpha = (byte)(255f * ShowHidePercentage);
			BigShipSlotSet.Draw();
			if (MyHero.CurrentLoadoutItem != null && MyHero.CurrentLoadoutItem.Loadout != null && GameJnG.Cargo.LoadouCount > 0)
			{
				DrawLoadoutItem();
			}
			DrawExtraButtons();
		}
		PanelInfo.DrawHintBackground();
		if (ScreenMission.State != ShopMenuState.Hidden)
		{
			GameCore.EffectLightBasic.SetLightMaps(bUseShopMaps: true);
			GameCore.EffectLight.SetLightMaps(bUseShopMaps: true);
			ScreenMission.Draw();
		}
		DrawBottomFiller(new Color(1f, 1f, 1f, ShowHidePercentage));
		if (ScreenMission.State != ShopMenuState.Hidden)
		{
			ScreenMission.DrawFrame();
		}
		if (CurrentScreen != ShopScreenEnum.Mia)
		{
			PanelCargo.Draw();
			PanelShop.Draw();
			PanelShipParams.Draw();
		}
		else
		{
			PanelMia.Draw();
		}
		PanelHeroStats.Draw();
		PanelFaceSelector.Draw();
		PanelInfo.Draw();
		SpriteBatchBegin(bLight: true);
		foreach (ShopObject diode in _diodeList)
		{
			diode.Draw();
		}
		SpriteBatchEnd(bLight: true);
		ItemFlySet.Draw();
		PanelTopButtons.Draw();
		if (CursorItem != null && CursorItem != SelectedItem && ScreenMission.State != ShopMenuState.Visible && State == ShopMenuState.Visible)
		{
			PanelInfo.DrawParamBox(CursorItem);
		}
		if (GameJnG.PopupMenuOptions.State != ShopMenuState.Hidden)
		{
			GameCore.DrawDarkBackground((float)(int)GameJnG.PopupMenuOptions.Color.A / 400f);
			GameJnG.PopupMenuOptions.Draw();
		}
		else if (YesNoEndCampaign.State != ShopMenuState.Hidden)
		{
			GameCore.DrawDarkBackground((float)(int)YesNoEndCampaign.Color.A / 400f);
			YesNoEndCampaign.Draw();
		}
		else if (YesNoResetHeroProfiles.State != ShopMenuState.Hidden)
		{
			GameCore.DrawDarkBackground((float)(int)YesNoResetHeroProfiles.Color.A / 400f);
			YesNoResetHeroProfiles.Draw();
		}
		if (ItemView.State != ItemViewStateEnum.Hidden)
		{
			ItemView.Draw();
		}
		if (Atarix.State != AtarixStateEnum.Hidden)
		{
			GameCore.DrawDarkBackground((float)(int)Atarix.Color.A / 400f);
			Atarix.Draw();
		}
		DrawHintLine();
		if (CurrentController == GameInput.ControllerTypeEnum.Mouse)
		{
			Cursor.Draw();
			Cursor.DrawSymbol(Cursor.Position + new Vector2(40f, 38f));
		}
		else if (PanelCargo.CursorSlotIndex >= 0)
		{
			Cursor.DrawSymbol(PanelCargo.GetSlotPosition(PanelCargo.CursorSlotIndex) + SLOT_CENTER_OFFSET);
		}
		else if (BigShipSlotSet.CursorSlot != null)
		{
			Cursor.DrawSymbol(BigShipSlotSet.GetCursorSlotPosition() + SLOT_CENTER_OFFSET);
		}
		else if (PanelShop.CursorSlotIndex >= 0)
		{
			Vector2 position = PanelShop.GetCursorSlotPosition() + SLOT_CENTER_OFFSET;
			if (position.Y > 130f && position.Y < 1050f)
			{
				Cursor.DrawSymbol(position);
			}
		}
		SpriteBatchBegin(bLight: false);
		foreach (CursorMessage cursorMessage in _cursorMessageList)
		{
			cursorMessage.Draw(SpriteBatchNoLight);
		}
		SpriteBatchEnd(bLight: false);
		GameCore.DrawLevelRenderTarget(bUseDistortion: false, Color.White);
		GameCore.EffectLightBasic.SetLightMaps(bUseShopMaps: false);
		GameCore.EffectLight.SetLightMaps(bUseShopMaps: false);
	}
}
