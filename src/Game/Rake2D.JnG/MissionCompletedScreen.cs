using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Font.BMFont;
using Rake2D.Menu;
using Rake2D.Script;

namespace Rake2D.JnG;

public class MissionCompletedScreen
{
	public GameLevel GUILevel;

	public Layer MainLayer;

	public GameObject MainObject;

	public GameObject FrameObject;

	private FaceTV _faceTV1;

	private FaceTV _faceTV2;

	private TextField _heroNameTextField1;

	private TextField _heroNameTextField2;

	private GameCore _gameCore;

	private float _alpha;

	private float _fadeAlpha;

	private float _textAlphaFactor;

	private float _startSoundVolumeFactor;

	private float _startMusicVolumeFactor;

	private BMFont _font;

	private SpriteAtlas _atlasBgr;

	private SpriteBatch _spriteBatch;

	private bool _exitToReloadLevel;

	private PartData _mainPartData;

	private PartData _framePartData;

	private GameObject _partRecord;

	private GameObject _partTitle;

	private GameObject _partMedals;

	private GameObject _partScore;

	private GameObject _partLevelBest;

	private GameObject _partProfit;

	private GameObject _partAirEnemy;

	private GameObject _partGroundEnemy;

	private GameObject _partMan;

	private GameObject _partBuildings;

	private GameObject _partDamageCaused;

	private GameObject _partDamageAbsorbed;

	private GameObject _partShots;

	private GameObject _partSecrets;

	private GameObject _partBonuses;

	private long _profit;

	private LevelCompletedStat[] _statArray;

	private readonly Color COLOR_LABEL = new Color(255, 255, 255);

	private readonly Color COLOR_VALUE = new Color(255, 255, 0);

	private const float FONT_SCALE = 0.75f;

	public MissionScreenStateEnum State { get; private set; }

	public MissionCompletedScreen(GameCore gameCore)
	{
		_gameCore = gameCore;
		_font = GameCore.GetBMFont("shop_font");
		_atlasBgr = GameCore.GetSpriteAtlas("mission_completed_bgr");
		_spriteBatch = new SpriteBatch(GameCore.GraphicsDeviceStatic);
		State = MissionScreenStateEnum.Hidden;
		_statArray = new LevelCompletedStat[12];
		for (int i = 0; i < _statArray.Length; i++)
		{
			_statArray[i] = new LevelCompletedStat();
		}
		_mainPartData = GameCore.GetPartData("debriefing_main");
		_framePartData = GameCore.GetPartData("debriefing_frame");
		_faceTV1 = new FaceTV(GameJnG.ShopMenu);
		_faceTV2 = new FaceTV(GameJnG.ShopMenu);
		_faceTV1.Position = new Vector2(170f, 275f);
		_faceTV2.Position = new Vector2(170f, 575f);
		_heroNameTextField1 = new TextField(GameJnG.ShopMenu);
		_heroNameTextField1.Position = new Vector2(170f, 385f);
		_heroNameTextField1.TextScale = ShopMenu.FONT_SCALE_BUTTON;
		_heroNameTextField1.LabelScale = _heroNameTextField1.TextScale;
		_heroNameTextField2 = new TextField(GameJnG.ShopMenu);
		_heroNameTextField2.Position = new Vector2(170f, 385f);
		_heroNameTextField2.TextScale = ShopMenu.FONT_SCALE_BUTTON;
		_heroNameTextField2.LabelScale = _heroNameTextField2.TextScale;
		SetTexts();
	}

	public void PrepareStats()
	{
		_profit = 0L;
		for (LevelCompletedStatEnum levelCompletedStatEnum = LevelCompletedStatEnum.TopScore; levelCompletedStatEnum < LevelCompletedStatEnum.MaxStat; levelCompletedStatEnum++)
		{
			int num = (int)levelCompletedStatEnum;
			LevelCompletedStat levelCompletedStat = _statArray[num];
			levelCompletedStat.CurrValue = 0f;
			switch (levelCompletedStatEnum)
			{
			case LevelCompletedStatEnum.TopScore:
				levelCompletedStat.CurrValue = GameJnG.OldLevelTopScore;
				levelCompletedStat.TargetValue = GameJnG.OldLevelTopScore;
				levelCompletedStat.MaxValue = GameJnG.OldLevelTopScore;
				break;
			case LevelCompletedStatEnum.Score:
				levelCompletedStat.TargetValue = GameJnG.LevelScore;
				levelCompletedStat.MaxValue = levelCompletedStat.TargetValue;
				levelCompletedStat.CompareStat = _statArray[0];
				break;
			case LevelCompletedStatEnum.Cash:
				levelCompletedStat.TargetValue = GameJnG.AchievementManager.LevelAchievedCountArray[0];
				levelCompletedStat.MaxValue = levelCompletedStat.TargetValue;
				break;
			case LevelCompletedStatEnum.ManKill:
				levelCompletedStat.TargetValue = GameJnG.AchievementManager.LevelAchievedCountArray[5];
				levelCompletedStat.MaxValue = GameJnG.AchievementManager.LevelAvailableCountArray[5];
				break;
			case LevelCompletedStatEnum.DamageCaused:
				levelCompletedStat.TargetValue = GameJnG.AchievementManager.DamageCaused;
				levelCompletedStat.MaxValue = GameJnG.AchievementManager.DamageCaused;
				break;
			case LevelCompletedStatEnum.DamageAbsorbed:
				levelCompletedStat.TargetValue = GameJnG.AchievementManager.HeroDamageAbsorbed;
				levelCompletedStat.MaxValue = GameJnG.AchievementManager.HeroDamageAbsorbed;
				break;
			case LevelCompletedStatEnum.EnemyAirKill:
				levelCompletedStat.TargetValue = GameJnG.AchievementManager.LevelAchievedCountArray[2];
				levelCompletedStat.MaxValue = GameJnG.AchievementManager.LevelAvailableCountArray[2];
				levelCompletedStat.ShowPercent = true;
				break;
			case LevelCompletedStatEnum.EnemyGroundKill:
				levelCompletedStat.TargetValue = GameJnG.AchievementManager.LevelAchievedCountArray[3];
				levelCompletedStat.MaxValue = GameJnG.AchievementManager.LevelAvailableCountArray[3];
				levelCompletedStat.ShowPercent = true;
				break;
			case LevelCompletedStatEnum.BuildingKill:
				levelCompletedStat.TargetValue = GameJnG.AchievementManager.LevelAchievedCountArray[4];
				levelCompletedStat.MaxValue = GameJnG.AchievementManager.LevelAvailableCountArray[4];
				break;
			case LevelCompletedStatEnum.FiredShots:
				levelCompletedStat.TargetValue = GameJnG.AchievementManager.LevelAchievedCountArray[6];
				levelCompletedStat.MaxValue = GameJnG.AchievementManager.LevelAchievedCountArray[6];
				break;
			case LevelCompletedStatEnum.BonusCount:
				levelCompletedStat.TargetValue = GameJnG.AchievementManager.LevelAchievedCountArray[1];
				levelCompletedStat.MaxValue = GameJnG.AchievementManager.LevelAvailableCountArray[1];
				break;
			case LevelCompletedStatEnum.SecretCount:
				if (GameJnG.AchievementManager.LevelAchievedFindDictionary != null && GameJnG.AchievementManager.LevelAchievedFindDictionary.ContainsKey("secret"))
				{
					levelCompletedStat.TargetValue = GameJnG.AchievementManager.LevelAchievedFindDictionary["secret"].Count;
				}
				else
				{
					levelCompletedStat.TargetValue = 0f;
				}
				if (GameCore.GameLevel.LevelData != null && GameCore.GameLevel.LevelData.AvailableFindDictionary.ContainsKey("secret"))
				{
					levelCompletedStat.MaxValue = GameCore.GameLevel.LevelData.AvailableFindDictionary["secret"].Count;
				}
				else
				{
					levelCompletedStat.MaxValue = 0f;
				}
				levelCompletedStat.ShowPercent = true;
				break;
			}
		}
	}

	public void SetTexts()
	{
		for (int i = 0; i < _statArray.Length; i++)
		{
			switch ((LevelCompletedStatEnum)i)
			{
			case LevelCompletedStatEnum.TopScore:
				_statArray[i].Caption = TextBundle.GetText("mission_stat_top_score");
				break;
			case LevelCompletedStatEnum.Score:
				_statArray[i].Caption = TextBundle.GetText("mission_stat_score");
				break;
			case LevelCompletedStatEnum.Cash:
				_statArray[i].Caption = TextBundle.GetText("mission_stat_cash");
				_statArray[i].Unit = "✐";
				break;
			case LevelCompletedStatEnum.EnemyAirKill:
				_statArray[i].Caption = TextBundle.GetText("mission_stat_air_enemy_kill");
				break;
			case LevelCompletedStatEnum.EnemyGroundKill:
				_statArray[i].Caption = TextBundle.GetText("mission_stat_ground_enemy_kill");
				break;
			case LevelCompletedStatEnum.ManKill:
				_statArray[i].Caption = TextBundle.GetText("mission_stat_man_kill");
				break;
			case LevelCompletedStatEnum.DamageCaused:
				_statArray[i].Caption = TextBundle.GetText("mission_stat_damage_causing");
				break;
			case LevelCompletedStatEnum.BuildingKill:
				_statArray[i].Caption = TextBundle.GetText("mission_stat_building_kill");
				break;
			case LevelCompletedStatEnum.SecretCount:
				_statArray[i].Caption = TextBundle.GetText("mission_stat_secret");
				break;
			case LevelCompletedStatEnum.FiredShots:
				_statArray[i].Caption = TextBundle.GetText("mission_stat_fired_shots");
				break;
			}
		}
	}

	public void SetHidden()
	{
		State = MissionScreenStateEnum.Hidden;
	}

	public void SetState(MissionScreenStateEnum state)
	{
		switch (state)
		{
		case MissionScreenStateEnum.FlashToWhite:
			_alpha = 0f;
			_fadeAlpha = 0f;
			_startSoundVolumeFactor = GameCore.SoundSystem.SoundVolumeFactor;
			_startMusicVolumeFactor = GameCore.SoundSystem.MusicVolumeFactor;
			break;
		case MissionScreenStateEnum.Showing:
			SaveGame.Save(Options.SaveGameNumber);
			GameCore.UpdateHeroes = false;
			GameCore.GameLevel.Clear();
			_alpha = 0f;
			GameCore.SoundSystem.StopAllSounds();
			PrepareGUILevel();
			GameCore.SoundSystem.LoadMusic(GameJnG.MusicFilenameMissionCompleted);
			GameCore.SoundSystem.MusicVolumeFactor = 1f;
			GameCore.SoundSystem.SoundVolumeFactor = 1f;
			GameCore.SoundSystem.PlayMusic(isLooped: false);
			break;
		case MissionScreenStateEnum.Hidding:
			_fadeAlpha = 0f;
			break;
		case MissionScreenStateEnum.Hidden:
		{
			GameCore.SoundSystem.StopMusic();
			GameCore.SoundSystem.SoundVolumeFactor = 1f;
			GameCore.SoundSystem.MusicVolumeFactor = 1f;
			GUILevel?.Clear();
			GUILevel = null;
			MainLayer = null;
			if (_exitToReloadLevel)
			{
				GameCore.GameLevel.Reload(null, bUseChecpoint: true);
				GameCore.UpdateHeroes = true;
				break;
			}
			LevelData levelData = null;
			if (GameCore.GameLevel.LevelData != null && GameCore.GameLevel.LevelData.WinToLevelId != null && !GameJnG.Mia)
			{
				levelData = GameJnG.Campaign.GetLevelData(GameCore.GameLevel.LevelData.WinToLevelId);
			}
			if (levelData != null)
			{
				GameJnG.LoadingScreen.Show(levelData, fadeToShop: false);
			}
			else
			{
				GameJnG.LoadingScreen.Show(null, fadeToShop: true);
			}
			break;
		}
		}
		State = state;
	}

	public void Update(float elapsedTime)
	{
		switch (State)
		{
		case MissionScreenStateEnum.FlashToWhite:
		{
			if (Utils.ChangeValueToTarget(ref _fadeAlpha, 255f, 400f * elapsedTime))
			{
				SetState(MissionScreenStateEnum.Showing);
				break;
			}
			float num = (255f - _fadeAlpha) / 255f;
			GameCore.SoundSystem.SoundVolumeFactor = _startSoundVolumeFactor * num;
			GameCore.SoundSystem.MusicVolumeFactor = _startMusicVolumeFactor * num;
			break;
		}
		case MissionScreenStateEnum.Showing:
			if (Utils.ChangeValueToTarget(ref _alpha, 255f, 400f * elapsedTime))
			{
				SetState(MissionScreenStateEnum.Visible);
			}
			break;
		case MissionScreenStateEnum.Hidding:
		{
			if (Utils.ChangeValueToTarget(ref _fadeAlpha, 255f, 400f * elapsedTime))
			{
				SetState(MissionScreenStateEnum.Hidden);
				break;
			}
			float num = (255f - _fadeAlpha) / 255f;
			GameCore.SoundSystem.SoundVolumeFactor = _startSoundVolumeFactor * num;
			GameCore.SoundSystem.MusicVolumeFactor = _startMusicVolumeFactor * num;
			break;
		}
		case MissionScreenStateEnum.Visible:
			_alpha = 255f;
			_fadeAlpha = 0f;
			_textAlphaFactor += elapsedTime * 5f;
			if (!GameCore.Console.IsActive && !GameCore.Console.IsActive && (GameInput.AnySystemKeyPressed() || GameInput.AnyGamepadButtonPressedAll() || GameInput.AnyMouseButtonPressed()))
			{
				_exitToReloadLevel = false;
				Hide();
			}
			break;
		case MissionScreenStateEnum.Hidden:
			return;
		default:
			_alpha = 0f;
			_fadeAlpha = 0f;
			_textAlphaFactor = 0f;
			break;
		}
		for (int i = 0; i < _statArray.Length; i++)
		{
			_statArray[i].Update(elapsedTime);
		}
		if (State == MissionScreenStateEnum.Visible || State == MissionScreenStateEnum.Hidding)
		{
			GUILevel?.Update(elapsedTime);
		}
		else
		{
			GUILevel?.Update(0f);
		}
		if (GameJnG.PlayerCount > 1)
		{
			_faceTV1.Position = new Vector2(182f, 245f);
			_heroNameTextField1.Position = new Vector2(182f, 405f);
			_faceTV2.Position = new Vector2(182f, 550f);
			_heroNameTextField2.Position = new Vector2(182f, 705f);
			_faceTV2.Update(elapsedTime);
			_heroNameTextField2.Update(elapsedTime);
		}
		else
		{
			_faceTV1.Position = new Vector2(182f, 305f);
			_heroNameTextField1.Position = new Vector2(182f, 465f);
		}
		_faceTV1.Update(elapsedTime);
		_heroNameTextField1.Update(elapsedTime);
		_profit = (long)_statArray[2].CurrValue;
	}

	public void Show()
	{
		PrepareStats();
		GameJnG.CancelFinishLevel();
		SetState(MissionScreenStateEnum.FlashToWhite);
		_faceTV1.FaceNumber = GameJnG.HeroList[0].FaceNumber;
		_faceTV1.GameMode = GameJnG.GameMode;
		_heroNameTextField1.Text = GameJnG.HeroList[0].PlayerName;
		_heroNameTextField1.Label = TextBundle.GetText($"hero_rank_{GameJnG.HeroList[0].GetRank()}");
		if (GameJnG.HeroList.Count > 1)
		{
			_faceTV2.FaceNumber = GameJnG.HeroList[1].FaceNumber;
			_faceTV2.GameMode = GameJnG.GameMode;
			_heroNameTextField2.Text = GameJnG.HeroList[1].PlayerName;
			_heroNameTextField2.Label = TextBundle.GetText($"hero_rank_{GameJnG.HeroList[1].GetRank()}");
		}
	}

	private void PrepareMedals()
	{
		if (GameJnG.AchievementManager.LevelAchievedMedalSet.Count <= 0)
		{
			return;
		}
		PartData partData = GameCore.GetPartData("medal_template");
		float num = 100f;
		float num2 = 960f - (float)(GameJnG.AchievementManager.LevelAchievedMedalSet.Count - 1) * num / 2f;
		int num3 = 0;
		foreach (HeroMedalEnum item in GameJnG.AchievementManager.LevelAchievedMedalSet)
		{
			Vector2 pos = new Vector2(num2 + num * (float)num3, 800f);
			GameObject gameObject = MainLayer.CreateObjectByPartData(partData, pos, new Vector2(10f, 10f), 0f, 1, bCreateRigidBody: false);
			BehaviorData behaviorData = new BehaviorData();
			behaviorData = new BehaviorData();
			behaviorData.SetTypeName("OnStateExit");
			behaviorData.SetParam("exit_delay", new float[1] { 3000 + 750 * num3 });
			behaviorData.SetParam("exit_state", new string[1] { "fade_in" });
			behaviorData.AddPartStates(new List<PartState>
			{
				new PartState("idle")
			});
			gameObject.AddBehaviorScript(behaviorData);
			gameObject.SpriteRenderer.SetAtlas(GameCore.GetSpriteAtlas($"medal_{(int)item}"));
			MainLayer.AddGameObject(gameObject);
			num3++;
		}
	}

	private void PrepareGUILevel()
	{
		GUILevel = new GameLevel();
		GUILevel.Init();
		MainLayer = GUILevel.CreateLayer("main", 0f, 0f);
		MainObject = MainLayer.CreateObjectByPartData(_mainPartData, new Vector2(959.5f, 540f), Vector2.One, 0f, -1, bCreateRigidBody: false);
		FrameObject = MainLayer.CreateObjectByPartData(_framePartData, new Vector2(959.5f, 540f), Vector2.One, 0f, 1, bCreateRigidBody: false);
		PrepareMedals();
		_partRecord = MainObject.GetChildByIdDeep("record");
		_partTitle = MainObject.GetChildByIdDeep("mc");
		_partMedals = MainObject.GetChildByIdDeep("medals");
		_partScore = MainObject.GetChildByIdDeep("stat_l1");
		_partLevelBest = MainObject.GetChildByIdDeep("stat_l2");
		_partProfit = MainObject.GetChildByIdDeep("stat_l3");
		_partShots = MainObject.GetChildByIdDeep("stat_l4");
		_partDamageCaused = MainObject.GetChildByIdDeep("stat_l5");
		_partDamageAbsorbed = MainObject.GetChildByIdDeep("stat_l6");
		_partBuildings = MainObject.GetChildByIdDeep("stat_r1");
		_partAirEnemy = MainObject.GetChildByIdDeep("stat_r2");
		_partGroundEnemy = MainObject.GetChildByIdDeep("stat_r3");
		_partMan = MainObject.GetChildByIdDeep("stat_r4");
		_partSecrets = MainObject.GetChildByIdDeep("stat_r5");
		_partBonuses = MainObject.GetChildByIdDeep("stat_r6");
		if (_statArray[1].MaxValue > _statArray[0].MaxValue)
		{
			_partRecord.ChangePartState("active");
		}
		else
		{
			_partRecord.ChangePartState("idle");
		}
		if (GameJnG.Mia)
		{
			MainObject.ChangePartState("idle_mia");
		}
		MainObject.OnReRegisterEvents += OnMainReRegister;
		MainLayer.Init();
		GUILevel.MainLayer = MainLayer;
		GUILevel.Begin();
		GUILevel.AfterLoad(resetCamera: true, mainLevel: false);
		GUILevel.Camera.Zoom = 1f;
		GUILevel.Camera.Position = Vector2.Zero;
		_gameCore.RefreshView();
	}

	public void Hide()
	{
		SetState(MissionScreenStateEnum.Hidding);
	}

	private void OnMainReRegister(GameObject go)
	{
		go.OnDraw += OnMainDraw;
	}

	private void OnMainDraw(SpriteBatch spriteBatch)
	{
		GameCore.SetScissorRectangle();
		if (GameJnG.Mia)
		{
			_font.Write(spriteBatch, TextBundle.GetText("mission_stat_title_mia"), _partTitle.VirtualScreenPosition.X, _partTitle.VirtualScreenPosition.Y - 22f, _partTitle.WorldScale * 1.3f, _partTitle.Color, TextAlign.Center, 500);
		}
		else
		{
			_font.Write(spriteBatch, TextBundle.GetText("mission_stat_title"), _partTitle.VirtualScreenPosition.X, _partTitle.VirtualScreenPosition.Y - 22f, _partTitle.WorldScale * 1.3f, _partTitle.Color, TextAlign.Center, 500);
		}
		_font.Write(spriteBatch, TextBundle.GetText("mission_stat_medals"), _partMedals.VirtualScreenPosition.X, _partMedals.VirtualScreenPosition.Y - 22f, _partMedals.WorldScale * 0.75f, COLOR_LABEL, TextAlign.Center, 500);
		float num = -14f;
		_font.Write(spriteBatch, string.Format("{0}: ", TextBundle.GetText("mission_stat_score")), _partScore.VirtualScreenPosition.X, _partScore.VirtualScreenPosition.Y + num, _partScore.WorldScale * 0.75f, COLOR_LABEL, TextAlign.Right, 500);
		_font.Write(spriteBatch, $"{Utils.FormatMoney((long)_statArray[1].CurrValue)}", _partScore.VirtualScreenPosition.X, _partScore.VirtualScreenPosition.Y + num, _partScore.WorldScale * 0.75f, COLOR_VALUE, TextAlign.Left, 500);
		_font.Write(spriteBatch, string.Format("{0}: ", TextBundle.GetText("mission_stat_top_score")), _partLevelBest.VirtualScreenPosition.X, _partLevelBest.VirtualScreenPosition.Y + num, _partLevelBest.WorldScale * 0.75f, COLOR_LABEL, TextAlign.Right, 500);
		_font.Write(spriteBatch, $"{Utils.FormatMoney((long)_statArray[0].CurrValue)}", _partLevelBest.VirtualScreenPosition.X, _partLevelBest.VirtualScreenPosition.Y + num, _partLevelBest.WorldScale * 0.75f, COLOR_VALUE, TextAlign.Left, 500);
		_font.Write(spriteBatch, string.Format("{0}: ", TextBundle.GetText("mission_stat_profit")), _partProfit.VirtualScreenPosition.X, _partProfit.VirtualScreenPosition.Y + num, _partProfit.WorldScale * 0.75f, COLOR_LABEL, TextAlign.Right, 500);
		_font.Write(spriteBatch, $"✐ {Utils.FormatMoney(_profit)}", _partProfit.VirtualScreenPosition.X, _partProfit.VirtualScreenPosition.Y + num, _partAirEnemy.WorldScale * 0.75f, COLOR_VALUE, TextAlign.Left, 500);
		_font.Write(spriteBatch, string.Format("{0}: ", TextBundle.GetText("mission_stat_shots_fired")), _partShots.VirtualScreenPosition.X, _partShots.VirtualScreenPosition.Y + num, _partShots.WorldScale * 0.75f, COLOR_LABEL, TextAlign.Right, 500);
		_font.Write(spriteBatch, $"{Utils.FormatMoney((long)_statArray[11].CurrValue)}", _partShots.VirtualScreenPosition.X, _partShots.VirtualScreenPosition.Y + num, _partShots.WorldScale * 0.75f, COLOR_VALUE, TextAlign.Left, 500);
		_font.Write(spriteBatch, string.Format("{0}: ", TextBundle.GetText("mission_stat_damage_caused")), _partDamageCaused.VirtualScreenPosition.X, _partDamageCaused.VirtualScreenPosition.Y + num, _partDamageCaused.WorldScale * 0.75f, COLOR_LABEL, TextAlign.Right, 500);
		_font.Write(spriteBatch, $"{Utils.FormatMoney((long)_statArray[9].CurrValue)}", _partDamageCaused.VirtualScreenPosition.X, _partDamageCaused.VirtualScreenPosition.Y + num, _partDamageCaused.WorldScale * 0.75f, COLOR_VALUE, TextAlign.Left, 500);
		_font.Write(spriteBatch, string.Format("{0}: ", TextBundle.GetText("mission_stat_damage_absorbed")), _partDamageAbsorbed.VirtualScreenPosition.X, _partDamageAbsorbed.VirtualScreenPosition.Y + num, _partDamageAbsorbed.WorldScale * 0.75f, COLOR_LABEL, TextAlign.Right, 500);
		_font.Write(spriteBatch, $"{Utils.FormatMoney((long)_statArray[10].CurrValue)}", _partDamageAbsorbed.VirtualScreenPosition.X, _partDamageAbsorbed.VirtualScreenPosition.Y + num, _partDamageCaused.WorldScale * 0.75f, COLOR_VALUE, TextAlign.Left, 500);
		_font.Write(spriteBatch, string.Format("{0}: ", TextBundle.GetText("mission_stat_air_enemy_kill")), _partAirEnemy.VirtualScreenPosition.X, _partAirEnemy.VirtualScreenPosition.Y + num, _partAirEnemy.WorldScale * 0.75f, COLOR_LABEL, TextAlign.Right, 500);
		_font.Write(spriteBatch, $"{Utils.FormatMoney((long)_statArray[3].CurrValue)} / {Utils.FormatMoney((long)_statArray[3].MaxValue)} ({_statArray[3].GetPercent()}%)", _partAirEnemy.VirtualScreenPosition.X, _partAirEnemy.VirtualScreenPosition.Y + num, _partAirEnemy.WorldScale * 0.75f, COLOR_VALUE, TextAlign.Left, 500);
		_font.Write(spriteBatch, string.Format("{0}: ", TextBundle.GetText("mission_stat_ground_enemy_kill")), _partGroundEnemy.VirtualScreenPosition.X, _partGroundEnemy.VirtualScreenPosition.Y + num, _partGroundEnemy.WorldScale * 0.75f, COLOR_LABEL, TextAlign.Right, 500);
		_font.Write(spriteBatch, $"{Utils.FormatMoney((long)_statArray[4].CurrValue)} / {Utils.FormatMoney((long)_statArray[4].MaxValue)} ({_statArray[4].GetPercent()}%)", _partGroundEnemy.VirtualScreenPosition.X, _partGroundEnemy.VirtualScreenPosition.Y + num, _partGroundEnemy.WorldScale * 0.75f, COLOR_VALUE, TextAlign.Left, 500);
		_font.Write(spriteBatch, string.Format("{0}: ", TextBundle.GetText("mission_stat_man_kill")), _partMan.VirtualScreenPosition.X, _partMan.VirtualScreenPosition.Y + num, _partMan.WorldScale * 0.75f, COLOR_LABEL, TextAlign.Right, 500);
		_font.Write(spriteBatch, $"{Utils.FormatMoney((long)_statArray[8].CurrValue)} / {Utils.FormatMoney((long)_statArray[8].MaxValue)} ({_statArray[8].GetPercent()}%)", _partMan.VirtualScreenPosition.X, _partMan.VirtualScreenPosition.Y + num, _partMan.WorldScale * 0.75f, COLOR_VALUE, TextAlign.Left, 500);
		_font.Write(spriteBatch, string.Format("{0}: ", TextBundle.GetText("mission_stat_building_kill")), _partBuildings.VirtualScreenPosition.X, _partBuildings.VirtualScreenPosition.Y + num, _partBuildings.WorldScale * 0.75f, COLOR_LABEL, TextAlign.Right, 500);
		_font.Write(spriteBatch, $"{Utils.FormatMoney((long)_statArray[7].CurrValue)}", _partBuildings.VirtualScreenPosition.X, _partBuildings.VirtualScreenPosition.Y + num, _partBuildings.WorldScale * 0.75f, COLOR_VALUE, TextAlign.Left, 500);
		_font.Write(spriteBatch, string.Format("{0}: ", TextBundle.GetText("mission_stat_secrets")), _partSecrets.VirtualScreenPosition.X, _partSecrets.VirtualScreenPosition.Y + num, _partSecrets.WorldScale * 0.75f, COLOR_LABEL, TextAlign.Right, 500);
		_font.Write(spriteBatch, $"{Utils.FormatMoney((long)_statArray[5].CurrValue)} / {Utils.FormatMoney((long)_statArray[5].MaxValue)} ({_statArray[5].GetPercent()}%)", _partSecrets.VirtualScreenPosition.X, _partSecrets.VirtualScreenPosition.Y + num, _partSecrets.WorldScale * 0.75f, COLOR_VALUE, TextAlign.Left, 500);
		_font.Write(spriteBatch, string.Format("{0}: ", TextBundle.GetText("mission_stat_bonuses")), _partBonuses.VirtualScreenPosition.X, _partBonuses.VirtualScreenPosition.Y + num, _partBonuses.WorldScale * 0.75f, COLOR_LABEL, TextAlign.Right, 500);
		_font.Write(spriteBatch, $"{Utils.FormatMoney((long)_statArray[6].CurrValue)} / {Utils.FormatMoney((long)_statArray[6].MaxValue)} ({_statArray[6].GetPercent()}%)", _partBonuses.VirtualScreenPosition.X, _partBonuses.VirtualScreenPosition.Y + num, _partBonuses.WorldScale * 0.75f, COLOR_VALUE, TextAlign.Left, 500);
	}

	private void DrawFadeRect()
	{
		if (_fadeAlpha > 0f)
		{
			GameCore.TriangleBatch.Begin(BlendState.NonPremultiplied, GameCore.ViewPortMatrix);
			_ = _fadeAlpha;
			GameCore.TriangleBatch.DrawRect(new Vector2(0f, 0f), new Vector2(1920f, 1080f), new Color(0, 0, 0, (int)_fadeAlpha));
			GameCore.TriangleBatch.End();
		}
	}

	public void Draw()
	{
		if (State == MissionScreenStateEnum.FlashToWhite || State == MissionScreenStateEnum.Showing)
		{
			DrawFadeRect();
		}
		if (_alpha > 0f)
		{
			MainLayer?.Draw();
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, GameCore.ViewPortMatrix);
			if (State == MissionScreenStateEnum.Visible)
			{
				float alphaFactor = 0.6f - Utils.Cos(_textAlphaFactor) * 0.4f;
				_font.Write(_spriteBatch, TextBundle.GetText("press_any_key_to_continue"), 960f, 943f, new Vector2(0.9f), new Color(255, 255, 255), TextAlign.Center, 0, alphaFactor);
			}
			_spriteBatch.End();
			_faceTV1.Draw();
			GameJnG.ShopMenu.SpriteBatchBegin(bLight: true);
			_heroNameTextField1.Draw();
			GameJnG.ShopMenu.SpriteBatchEnd(bLight: true);
			if (GameJnG.PlayerCount > 1)
			{
				_faceTV2.Draw();
				GameJnG.ShopMenu.SpriteBatchBegin(bLight: true);
				_heroNameTextField2.Draw();
				GameJnG.ShopMenu.SpriteBatchEnd(bLight: true);
			}
			GameCore.TriangleBatch.Begin(BlendState.NonPremultiplied, GameCore.ViewPortMatrix);
			GameCore.TriangleBatch.DrawRect(new Vector2(-1f, 0f), new Vector2(0f, 1080f), Color.Black);
			GameCore.TriangleBatch.DrawRect(new Vector2(1920f, 0f), new Vector2(1922f, 1080f), Color.Black);
			GameCore.TriangleBatch.End();
		}
		if (State == MissionScreenStateEnum.Hidding)
		{
			DrawFadeRect();
		}
	}
}
