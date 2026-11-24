using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Rake2D.Menu;

namespace Rake2D.JnG;

public static class GameJnG
{
	public const int MAX_HERO_PROFILE_NUMBER = 5;

	public const int MAX_LEVEL_OF_DETAILS = 3;

	public static MissionStateEnum MissionState;

	public static Dictionary<string, LevelStats> LevelStatsDictionary;

	public static Dictionary<string, LevelData> LevelDataDictionary;

	public static HashSet<string> BountyVillainSet;

	public static bool[] IsControllerConnected;

	public static bool Mia;

	public static GameModeEnum GameMode;

	public static AchievementManager AchievementManager;

	public static int SaveGameVersion;

	public static int StageNumber;

	public static int Cash;

	public static int GlobalManKills;

	public static int AtarixAutoHackChance;

	public static int OldLevelTopScore;

	public static int LevelScore;

	public static int ZogonInsuranceCount;

	public static int HeroDeathCountInLevel;

	public static bool WasEarlyAccessBuy;

	public static Dictionary<string, string[]> ItemSetDictionary;

	public static float PlayTime;

	public static bool IsDrunkenEffectEnabled;

	public static bool IsBountyHunterEnabled;

	public static bool IsTimeCompressorEnabled;

	public static ShopOffer ShopOffer;

	public static Campaign Campaign;

	public static MainMenu MainMenu;

	public static ShopMenu ShopMenu;

	public static PopupMenuOptions PopupMenuOptions;

	public static PopupMenuInLevel PopupMenuInLevel;

	public static PopupMenuFinishCampaign PopupMenuFinishCampaign;

	public static List<Hero> HeroList;

	public static List<string> HeroCamouflageIdList;

	public static Dictionary<string, HeroPartChoice> HeroPartChoices;

	public static HeroCargo Cargo;

	public static LocationText LocationText;

	public static CheckpointInfo CheckpointInfo;

	public static LoadingScreen LoadingScreen;

	public static MissionFailedScreen MissionFailedScreen;

	public static MissionCompletedScreen MissionCompletedScreen;

	public static List<BossMeter> BossMeterList;

	public static CarnageTV CarnageTV;

	public static ScoreHUD ScoreHUD;

	public static string MusicFilenameMissionCompleted;

	public static bool IsTestRangeEnabled;

	public static bool IsFirstLevelTry;

	public static int SaveGameNumber;

	private static float _cameraSpeedFactor;

	private static FinishLevelModeEnum _finishLevelMode;

	private static float _finisLevelTimer;

	public static int Difficulty { get; private set; }

	public static bool IsCheater { get; private set; }

	public static bool IsAnyMissionScreenVisible
	{
		get
		{
			if (MissionFailedScreen.State == MissionScreenStateEnum.Hidden && MissionCompletedScreen.State == MissionScreenStateEnum.Hidden)
			{
				return LoadingScreen.State != MissionScreenStateEnum.Hidden;
			}
			return true;
		}
	}

	public static bool IsAnyPopupMenuVisible
	{
		get
		{
			if (PopupMenuOptions.State == ShopMenuState.Hidden && PopupMenuInLevel.State == ShopMenuState.Hidden)
			{
				return PopupMenuFinishCampaign.State != ShopMenuState.Hidden;
			}
			return true;
		}
	}

	public static int PlayerCount
	{
		get
		{
			if (HeroList == null)
			{
				return 1;
			}
			return HeroList.Count;
		}
	}

	public static bool IsAnyHeroAlive
	{
		get
		{
			foreach (Hero hero in HeroList)
			{
				if (!hero.IsDestroyed || (hero.IsDestroyed && hero.IsJetPackActive))
				{
					return true;
				}
			}
			return false;
		}
	}

	public static bool IsMouseControlEnabled
	{
		get
		{
			if (Options.PlayerController[0] != GameInput.PlayerControllerEnum.Mouse)
			{
				if (PlayerCount > 1)
				{
					return Options.PlayerController[1] == GameInput.PlayerControllerEnum.Mouse;
				}
				return false;
			}
			return true;
		}
	}

	public static void SetGameModeByNumber(int mode)
	{
		if (Enum.IsDefined(typeof(GameModeEnum), mode))
		{
			GameMode = (GameModeEnum)mode;
		}
		else
		{
			GameMode = GameModeEnum.Standard;
		}
	}

	static GameJnG()
	{
		Mia = false;
		AtarixAutoHackChance = 10;
		HeroList = new List<Hero>();
		HeroPartChoices = new Dictionary<string, HeroPartChoice>();
		HeroCamouflageIdList = new List<string>();
		AchievementManager = new AchievementManager();
		ShopOffer = new ShopOffer();
		Cargo = new HeroCargo();
		Campaign = new Campaign();
		CheckpointInfo = new CheckpointInfo();
		_cameraSpeedFactor = 3.2f;
		IsControllerConnected = new bool[2];
		BossMeterList = new List<BossMeter>();
		BountyVillainSet = new HashSet<string>();
	}

	public static bool Init(GameCore gameCore)
	{
		DataLoader.LoadHeroPartChoiceFile("ui/hero_part_choice.txt");
		LocationText = new LocationText();
		WarningText.Init();
		Remcon.Init();
		ShopMenu = new ShopMenu(gameCore);
		ShopMenu.Init();
		BossMeter.Init();
		LoadingScreen = new LoadingScreen(gameCore);
		MissionFailedScreen = new MissionFailedScreen(gameCore);
		MissionCompletedScreen = new MissionCompletedScreen(gameCore);
		MainMenu = new MainMenu(ShopMenu);
		PopupMenuOptions = new PopupMenuOptions(ShopMenu);
		PopupMenuInLevel = new PopupMenuInLevel(ShopMenu);
		PopupMenuFinishCampaign = new PopupMenuFinishCampaign(ShopMenu);
		ScoreHUD = new ScoreHUD();
		SetTexts();
		GameCore.Log("JnG game class initialized.");
		return true;
	}

	public static void SetTexts()
	{
		ShopMenu.SetTexts();
		MainMenu.SetTexts();
		MissionCompletedScreen.SetTexts();
		HUD.SetTexts();
		Hero.SetTexts();
		ScoreHUD.SetTexts();
		CarnageTV?.SetTexts();
		PopupMenuOptions.SetTexts();
	}

	public static int GetFreeLoadoutNumber()
	{
		int num = 1;
		List<HeroItem> list = Cargo.Items.Values.Where((HeroItem o) => o.Loadout != null).ToList();
		foreach (Hero hero in HeroList)
		{
			if (hero.CurrentLoadoutItem != null && hero.CurrentLoadoutItem.Loadout != null)
			{
				list.Add(hero.CurrentLoadoutItem);
			}
		}
		list.AddRange(ShopOffer.ItemList.Where((HeroItem o) => o.Loadout != null));
		list = list.OrderBy((HeroItem o) => o.Loadout.Number).ToList();
		foreach (HeroItem item in list)
		{
			if (item.Loadout.Number > num)
			{
				break;
			}
			num = item.Loadout.Number + 1;
		}
		return num;
	}

	public static void SetCheater(bool isCheater = true)
	{
		IsCheater = isCheater;
	}

	public static void SetStage(int stageNumber)
	{
		if (Campaign.GetStageData(stageNumber) != null)
		{
			StageNumber = stageNumber;
			ShopMenu?.ActualizeCampaign();
		}
		else
		{
			GameCore.LogErr("Unable set to undefined stage {0}", stageNumber);
		}
	}

	public static void SetDifficulty(int difficulty)
	{
		Difficulty = difficulty;
		Campaign = Campaign.LoadCampaign(Difficulty);
		ShopMenu?.ActualizeCampaign();
	}

	public static void RaiseDifficulty()
	{
		SetDifficulty(Difficulty + 1);
		Campaign.HideAllSecretLevels();
		foreach (LevelStats value in LevelStatsDictionary.Values)
		{
			value.FinishCountInCampaign = 0;
			value.TopScore = 0;
		}
		SetStage(0);
		ShopOffer.Build(StageNumber);
		ShopMenu.ScreenMission.SelectUnfinishedStageLevel(StageNumber);
		TotalStats.Recalculate();
	}

	public static void RaiseStageByLevel(GameLevel level)
	{
		if (level.LevelData == null)
		{
			return;
		}
		for (int i = 0; i < Campaign.StageDataList.Count; i++)
		{
			StageData stageData = Campaign.StageDataList[i];
			if (stageData.LevelDataList.Contains(level.LevelData))
			{
				if (stageData.Number >= StageNumber)
				{
					SetStage(stageData.Number);
				}
				break;
			}
		}
	}

	public static void RestartHeroProfiles()
	{
		TotalStats.Clear();
		SetDifficulty(1);
		Campaign.LoadCampaign(Difficulty);
		SetStage(0);
		Mia = false;
		IsCheater = false;
		SaveGameVersion = SaveGame.SAVE_GAME_VERSION;
		Cash = SaveGame.GetDefaultCash(GameMode);
		AtarixAutoHackChance = 0;
		PlayTime = 0f;
		LevelStatsDictionary?.Clear();
		BountyVillainSet.Clear();
		Cargo.Clear();
		ShopOffer.Build(StageNumber);
		ShopMenu.ScreenMission.SelectUnfinishedStageLevel(StageNumber);
		foreach (Hero hero in HeroList)
		{
			foreach (HeroSlot value in hero.MyShip.Slots.Values)
			{
				value.SetItem(null);
			}
			hero.StatDeathCount = 0;
			hero.CreateDefaultSlotItems();
		}
	}

	public static LevelStats GetLevelStatsForLevel(GameLevel level)
	{
		if (level == null || level.LevelData == null || level.Id == null || LevelStatsDictionary == null || !LevelStatsDictionary.ContainsKey(level.Id))
		{
			return null;
		}
		return LevelStatsDictionary[level.Id];
	}

	public static void CheckMissionState()
	{
		if (MissionState == MissionStateEnum.CompletedToDebriefing)
		{
			if (MissionCompletedScreen.State == MissionScreenStateEnum.Hidden)
			{
				MissionCompletedScreen.Show();
			}
			return;
		}
		bool flag = true;
		foreach (Hero hero in HeroList)
		{
			if (hero.IsJetPackActive)
			{
				if (!hero.JetPackObject.IsKilled)
				{
					flag = false;
					break;
				}
			}
			else if (!hero.IsDestroyed)
			{
				flag = false;
				break;
			}
		}
		if (flag && MissionState != MissionStateEnum.Failed && !IsAnyMissionScreenVisible)
		{
			if (IsTestRangeEnabled)
			{
				foreach (Hero hero2 in HeroList)
				{
					hero2.SpawnInLevel(hero2.MyObject.MyLevel);
				}
			}
			else
			{
				SetFinishLevel(HeroList[0].MyObject.MyLevel, FinishLevelModeEnum.Failed, 0.7f);
			}
		}
		if (MissionState == MissionStateEnum.Failed && MissionFailedScreen.State == MissionScreenStateEnum.Hidden)
		{
			if (Mia)
			{
				MissionCompletedScreen.Show();
			}
			else
			{
				MissionFailedScreen.Show();
			}
		}
	}

	public static void LevelBegin(GameLevel level)
	{
		ClearLevelAchievedValues();
		RaiseStageByLevel(level);
		if (level != ShopMenu.HeroPreviewLevel)
		{
			LocationText.Reset(level);
		}
	}

	public static Hero CreateHero(int playerNumber)
	{
		Hero hero = new Hero(playerNumber);
		HeroList.Add(hero);
		return hero;
	}

	public static HeroPartChoice GetHeroPartChoiceById(string id)
	{
		if (HeroPartChoices.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	public static HeroPartChoiceItem GetHeroPartChoiceByNumId(string choiceId, int numId)
	{
		if (HeroPartChoices.ContainsKey(choiceId))
		{
			foreach (HeroPartChoiceItem item in HeroPartChoices[choiceId].ItemList)
			{
				if (item.NumId == numId)
				{
					return item;
				}
			}
		}
		return null;
	}

	public static void AddBossMeter(GameObject go, string name)
	{
		float y = ((BossMeterList.Count <= 0) ? (-50f) : Math.Min(-50f, BossMeterList[0].Position.Y - 40f));
		BossMeter bossMeter = new BossMeter(go, name);
		bossMeter.Position = new Vector2(960f - BossMeter.Size.X / 2f, y);
		bossMeter.TargetPositionY = bossMeter.Position.Y + 100f;
		BossMeterList.Insert(0, bossMeter);
	}

	public static void SetupHeroPartChoicesByPartMap(PartMap partMap)
	{
		foreach (KeyValuePair<string, string> mapId in partMap.MapIds)
		{
			HeroPartChoice heroPartChoiceById = GetHeroPartChoiceById(mapId.Key);
			if (heroPartChoiceById != null && Utils.Equals(heroPartChoiceById.Id, mapId.Key) && !heroPartChoiceById.SetIndexesByPartId(mapId.Value))
			{
				GameCore.LogErr("Unable to set choice {0} / {1}", mapId.Key, mapId.Value);
			}
		}
	}

	public static void ApplyHeroPartChoicesToPartMap(PartMap partMap)
	{
		foreach (HeroPartChoice value in HeroPartChoices.Values)
		{
			value.ApplyCurrentChoiceIndex();
		}
		foreach (HeroPartChoice value2 in HeroPartChoices.Values)
		{
			if (partMap.MapIds.ContainsKey(value2.Id))
			{
				partMap.MapIds[value2.Id] = value2.GetCurrentId();
			}
		}
	}

	public static void ScrollCameraToHero(float elapsedTime)
	{
		Camera camera = GameCore.CurrentLevel.Camera;
		if (HeroList[0].MyObject == null)
		{
			return;
		}
		int num = 0;
		float num2;
		if (PlayerCount > 1)
		{
			GameObject controlledObject = HeroList[1].GetControlledObject();
			if (HeroList[0].GetControlledObject().IsAlive)
			{
				num2 = HeroList[0].GetControlledObject().Position.Y - 540f;
				if (controlledObject != null && controlledObject.IsAlive)
				{
					num2 += controlledObject.Position.Y - 540f;
					num2 /= 2f;
				}
			}
			else
			{
				num2 = controlledObject.Position.Y - 540f;
			}
		}
		else
		{
			num2 = HeroList[0].GetControlledObject().Position.Y - 540f;
		}
		float num3 = (camera.Position.Y - num2) / 540f * HeroList[0].MaxSpeed * _cameraSpeedFactor;
		if (camera.Position.Y > num2 + (float)num)
		{
			camera.Position.Y -= num3 * elapsedTime;
			if (camera.Position.Y < num2)
			{
				camera.Position.Y = num2;
			}
		}
		else if (camera.Position.Y < num2 - (float)num)
		{
			camera.Position.Y -= num3 * elapsedTime;
			if (camera.Position.Y > num2)
			{
				camera.Position.Y = num2;
			}
		}
		if (camera.Position.X < 0f)
		{
			camera.Position.X = 0f;
		}
		else if (camera.Position.X >= GameCore.CurrentLevel.Size.X - 1920f)
		{
			camera.Position.X = GameCore.CurrentLevel.Size.X - 1920f - 1f;
		}
		if (camera.Position.Y < 0f)
		{
			camera.Position.Y = 0f;
		}
		else if (camera.Position.Y > GameCore.CurrentLevel.Size.Y - 1080f)
		{
			camera.Position.Y = GameCore.CurrentLevel.Size.Y - 1080f;
		}
	}

	public static void DrawHUD()
	{
		if (GameCore.UpdateHeroes)
		{
			foreach (Hero hero in HeroList)
			{
				if (hero.MyObject != null)
				{
					hero.Remcon.Draw();
					if (GameCore.GameLevel.CutsceneMode == 0)
					{
						hero.MouseGun?.Draw();
						hero.Hud.Draw();
					}
				}
			}
		}
		if (GameCore.GameLevel.CutsceneMode != 0)
		{
			return;
		}
		foreach (BossMeter bossMeter in BossMeterList)
		{
			bossMeter.Draw();
		}
		ScoreHUD.Draw();
	}

	public static void DrawLocationText()
	{
		LocationText.Draw();
	}

	public static void SellItem(HeroItem item)
	{
		Cash += item.Price;
		item.Level = 0;
		item.RecalculateBehaviorParams();
		switch (item.Place)
		{
		case ItemPlaceEnum.InCargo:
			Cargo.RemoveItem(item);
			break;
		case ItemPlaceEnum.InHeroSlot:
			item.MySlot.SetItem(null);
			break;
		}
		item.IsBlockedBySameItem = false;
		ShopOffer.AddItem(item);
		ShopOffer.Order();
	}

	public static List<HeroItem> GetAllItemList()
	{
		List<HeroItem> list = new List<HeroItem>();
		list.AddRange(Cargo.Items.Values);
		list.AddRange(ShopOffer.ItemList);
		foreach (Hero hero in HeroList)
		{
			foreach (HeroSlot value in hero.MyShip.Slots.Values)
			{
				if (value.Item != null)
				{
					list.Add(value.Item);
				}
			}
		}
		return list;
	}

	public static bool IsItemIdInCargo(string itemId)
	{
		return Cargo.Items.Values.FirstOrDefault((HeroItem item) => Utils.Equals(item.Data.Id, itemId)) != null;
	}

	public static void RecalculateItemParams(Hero hero)
	{
		foreach (HeroItem allItem in GetAllItemList())
		{
			allItem.Params.Clear();
			HeroShip heroShip = ((allItem.Place == ItemPlaceEnum.InHeroSlot) ? allItem.MyShip : null);
			if (heroShip != null)
			{
				foreach (KeyValuePair<string, HeroItemDataParam> item in allItem.Data.Params)
				{
					allItem.AddParam(item.Key, heroShip.GetModifyiedParamValue(item.Key, item.Value.GetValue(allItem.Level), allItem));
				}
			}
			else
			{
				foreach (KeyValuePair<string, HeroItemDataParam> item2 in allItem.Data.Params)
				{
					allItem.AddParam(item2.Key, item2.Value.GetValue(allItem.Level));
				}
			}
			if (hero == null || allItem.MyShip == null || allItem.MyShip.MyHero == hero)
			{
				allItem.RecalculateBehaviorParams();
			}
		}
	}

	public static bool TryGetLevelStats(LevelData levelData, out LevelStats levelStats)
	{
		if (levelData != null && LevelStatsDictionary != null && LevelStatsDictionary.ContainsKey(levelData.Id))
		{
			levelStats = LevelStatsDictionary[levelData.Id];
			return true;
		}
		levelStats = null;
		return false;
	}

	public static void CancelFinishLevel()
	{
		MissionState = MissionStateEnum.Normal;
		_finishLevelMode = FinishLevelModeEnum.None;
	}

	public static void SetFinishLevel(GameLevel level, FinishLevelModeEnum mode, float fadeDelay)
	{
		if (_finishLevelMode != FinishLevelModeEnum.None || MissionState == MissionStateEnum.CompletedToDebriefing || MissionState == MissionStateEnum.CompletedToShop)
		{
			return;
		}
		switch (mode)
		{
		case FinishLevelModeEnum.WinAndFlyOut:
			level.AddWarningText(TextBundle.GetText("hud_warning_mission_completed"), new Color(255, 255, 0));
			foreach (Hero hero in HeroList)
			{
				hero.StarAutopilotLevelFlyOut();
			}
			break;
		case FinishLevelModeEnum.WinAndBlackHole:
			foreach (Hero hero2 in HeroList)
			{
				hero2.StarAutopilotBlackHole();
			}
			break;
		case FinishLevelModeEnum.WinToShop:
			level.AddWarningText(TextBundle.GetText("hud_warning_mission_completed"), new Color(255, 255, 0));
			foreach (Hero hero3 in HeroList)
			{
				hero3.StarAutopilotLevelFlyOut();
			}
			break;
		case FinishLevelModeEnum.Failed:
			level.AddWarningText(TextBundle.GetText("hud_warning_mission_failed"), new Color(255, 0, 0));
			break;
		}
		if (fadeDelay <= 0f)
		{
			DoFinishLevel(mode);
			return;
		}
		_finisLevelTimer = fadeDelay;
		_finishLevelMode = mode;
	}

	public static StageData GetLastUnFinishedStage()
	{
		StageData result = Campaign.StageDataList[0];
		StageData stageData = Campaign.StageDataList[Campaign.StageDataList.Count - 1];
		for (int num = Campaign.StageDataList.Count - 1; num >= 0; num--)
		{
			StageData stageData2 = Campaign.StageDataList[num];
			bool flag = false;
			foreach (LevelData levelData in stageData2.LevelDataList)
			{
				if (levelData.IsVisible)
				{
					if (LevelStatsDictionary == null || !LevelStatsDictionary.ContainsKey(levelData.Id))
					{
						flag = true;
						break;
					}
					LevelStats levelStats = LevelStatsDictionary[levelData.Id];
					if (levelData.IsMandatory && levelStats.FinishCountInCampaign <= 0)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				result = stageData;
				break;
			}
			stageData = stageData2;
		}
		return result;
	}

	private static void DoFinishLevel(FinishLevelModeEnum mode)
	{
		switch (mode)
		{
		case FinishLevelModeEnum.WinAndFlyOut:
		case FinishLevelModeEnum.WinToShop:
		case FinishLevelModeEnum.WinAndBlackHole:
			if (MissionState == MissionStateEnum.CompletedToDebriefing)
			{
				break;
			}
			if (!GameCore.GameLevel.IgnoreTakenValues)
			{
				ConfirmTakenLevelValues();
			}
			else if (GameCore.GameLevel.LevelData != null)
			{
				if (LevelStatsDictionary == null)
				{
					LevelStatsDictionary = new Dictionary<string, LevelStats>();
				}
				LevelStats levelStats;
				if (LevelStatsDictionary.Keys.Contains(GameCore.GameLevel.LevelData.Id))
				{
					levelStats = LevelStatsDictionary[GameCore.GameLevel.LevelData.Id];
				}
				else
				{
					levelStats = new LevelStats(GameCore.GameLevel.LevelData.Id);
					LevelStatsDictionary.Add(GameCore.GameLevel.LevelData.Id, levelStats);
				}
				if (!Mia)
				{
					levelStats.FinishCountInCampaign++;
					levelStats.TotalFinishCount++;
				}
			}
			SetStage(GetLastUnFinishedStage().Number);
			ShopOffer.Build(StageNumber, 999999);
			ShopMenu.ScreenMission.SelectUnfinishedStageLevel(StageNumber);
			if (mode == FinishLevelModeEnum.WinAndFlyOut || mode == FinishLevelModeEnum.WinAndBlackHole)
			{
				MissionState = MissionStateEnum.CompletedToDebriefing;
			}
			else
			{
				LoadingScreen.Show(null, fadeToShop: true);
			}
			break;
		default:
			if (MissionState != MissionStateEnum.Failed)
			{
				MissionState = MissionStateEnum.Failed;
				if (GameMode == GameModeEnum.HardCore)
				{
					Mia = true;
					ConfirmTakenLevelValues();
				}
			}
			break;
		}
		IsFirstLevelTry = GetIsFirstLevelTry();
	}

	public static void UpdateFinishLevel(float elapsedTime)
	{
		if (MissionFailedScreen.State != MissionScreenStateEnum.Hidden)
		{
			MissionFailedScreen.Update(elapsedTime);
		}
		else if (MissionCompletedScreen.State != MissionScreenStateEnum.Hidden)
		{
			MissionCompletedScreen.Update(elapsedTime);
		}
		else if (_finishLevelMode != FinishLevelModeEnum.None && Utils.ChangeValueToTarget(ref _finisLevelTimer, 0f, elapsedTime))
		{
			DoFinishLevel(_finishLevelMode);
			_finishLevelMode = FinishLevelModeEnum.None;
		}
	}

	private static void ConfirmMedals()
	{
		if (TotalStats.LevelWinCount >= 100 && TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_LEVEL_WIN_1) == 0)
		{
			AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_LEVEL_WIN_1);
		}
		if (AchievementManager.LevelAchievedCountArray[7] > 0)
		{
			AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_BOSS_ANY);
		}
		if (TotalStats.KillCountBoss >= 100 && TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_KILL_BOSS_1) == 0)
		{
			AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_BOSS_1);
		}
		if (TotalStats.DamageCaused >= 1000000.0 && TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_DAMAGE_CAUSED_1) == 0)
		{
			AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_DAMAGE_CAUSED_1);
		}
		if (TotalStats.DamageCaused >= 10000000.0 && TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_DAMAGE_CAUSED_2) == 0)
		{
			AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_DAMAGE_CAUSED_2);
		}
		if (TotalStats.Score >= 1000000 && TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_SCORE_1) == 0)
		{
			AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_SCORE_1);
		}
		if (TotalStats.Score >= 10000000 && TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_SCORE_2) == 0)
		{
			AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_SCORE_2);
		}
		if (TotalStats.KillCountGroundEnemy + TotalStats.KillCountAirEnemy >= 1000 && TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_KILL_ENEMY_1) == 0)
		{
			AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_ENEMY_1);
		}
		if (TotalStats.KillCountGroundEnemy + TotalStats.KillCountAirEnemy >= 5000 && TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_KILL_ENEMY_2) == 0)
		{
			AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_ENEMY_2);
		}
		if (TotalStats.KillCountGroundEnemy + TotalStats.KillCountAirEnemy >= 10000 && TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_KILL_ENEMY_3) == 0)
		{
			AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_ENEMY_3);
		}
		if (AchievementManager.LevelAvailableCountArray[2] > 0)
		{
			if (AchievementManager.GetLevelCountPercent(AchievementCountTypeEnum.KillCountAirEnemy) >= 90)
			{
				AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_AIR_ENEMY_LEVEL_3);
				if (TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_KILL_AIR_ENEMY_LEVEL_2) == 0)
				{
					AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_AIR_ENEMY_LEVEL_2);
				}
				if (TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_KILL_AIR_ENEMY_LEVEL_1) == 0)
				{
					AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_AIR_ENEMY_LEVEL_1);
				}
			}
			else if (AchievementManager.GetLevelCountPercent(AchievementCountTypeEnum.KillCountAirEnemy) >= 75)
			{
				AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_AIR_ENEMY_LEVEL_2);
				if (TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_KILL_AIR_ENEMY_LEVEL_1) == 0)
				{
					AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_AIR_ENEMY_LEVEL_1);
				}
			}
			else if (AchievementManager.GetLevelCountPercent(AchievementCountTypeEnum.KillCountAirEnemy) >= 50)
			{
				AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_AIR_ENEMY_LEVEL_1);
			}
		}
		if (AchievementManager.LevelAvailableCountArray[3] > 0)
		{
			if (AchievementManager.GetLevelCountPercent(AchievementCountTypeEnum.KillCountGroundEnemy) >= 90)
			{
				AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_GROUND_ENEMY_LEVEL_3);
				if (TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_KILL_GROUND_ENEMY_LEVEL_2) == 0)
				{
					AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_GROUND_ENEMY_LEVEL_2);
				}
				if (TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_KILL_GROUND_ENEMY_LEVEL_1) == 0)
				{
					AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_GROUND_ENEMY_LEVEL_1);
				}
			}
			else if (AchievementManager.GetLevelCountPercent(AchievementCountTypeEnum.KillCountGroundEnemy) >= 75)
			{
				AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_GROUND_ENEMY_LEVEL_2);
				if (TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_KILL_GROUND_ENEMY_LEVEL_1) == 0)
				{
					AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_GROUND_ENEMY_LEVEL_1);
				}
			}
			else if (AchievementManager.GetLevelCountPercent(AchievementCountTypeEnum.KillCountGroundEnemy) >= 50)
			{
				AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_GROUND_ENEMY_LEVEL_1);
			}
		}
		if (AchievementManager.LevelAchievedCountArray[5] >= 100)
		{
			AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_MAN_LEVEL);
		}
		if (TotalStats.KillCountMan >= 1000 && TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_KILL_MAN_1) == 0)
		{
			AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_MAN_1);
		}
		if (TotalStats.KillCountMan >= 10000 && TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_KILL_MAN_2) == 0)
		{
			AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_KILL_MAN_2);
		}
		int num = 0;
		if (GameCore.GameLevel.LevelData != null && GameCore.GameLevel.LevelData.AvailableFindDictionary.ContainsKey("secret"))
		{
			num = GameCore.GameLevel.LevelData.AvailableFindDictionary["secret"].Count;
		}
		if (num > 0)
		{
			int num2 = 0;
			if (AchievementManager.LevelAchievedFindDictionary != null && AchievementManager.LevelAchievedFindDictionary.ContainsKey("secret"))
			{
				num2 = AchievementManager.LevelAchievedFindDictionary["secret"].Count;
			}
			if (num2 >= num)
			{
				AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_SECRET_ALL_LEVEL);
			}
			else if (num2 > 0)
			{
				AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_SECRET_ANY);
			}
		}
		if (AchievementManager.HeroDamageAbsorbed >= 500f)
		{
			AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_DAMAGE_ABSORBED_LEVEL);
		}
		if (AchievementManager.GetLevelCountPercent(AchievementCountTypeEnum.BonusCount) >= 75 && AchievementManager.LevelAvailableCountArray[1] > 0)
		{
			AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_BONUS_LEVEL_1);
		}
		if (HeroDeathCountInLevel == 0)
		{
			AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_NO_DEATH_WIN);
		}
		if (GameCore.GameLevel.LevelData != null && GameCore.GameLevel.LevelData.IsFinalLevel)
		{
			if (Difficulty == 1)
			{
				if (TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_WIN_CAMPAIGN_1) == 0)
				{
					AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_WIN_CAMPAIGN_1);
				}
			}
			else if (Difficulty == 10 && TotalStats.GetMedalCount(HeroMedalEnum.MEDAL_WIN_CAMPAIGN_ULTIMATE) == 0)
			{
				AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_WIN_CAMPAIGN_ULTIMATE);
			}
			else if (Difficulty > 1)
			{
				AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_WIN_CAMPAIGN_1_PLUS);
			}
			if (GameMode == GameModeEnum.Standard)
			{
				UnlockAchievement(GameAchievementEnum.ACH_WIN_CAMPAIGN);
			}
			else if (GameMode == GameModeEnum.HardCore && !Mia)
			{
				UnlockAchievement(GameAchievementEnum.ACH_WIN_CAMPAIGN_HC);
			}
		}
		if (TotalStats.HasAllMedals())
		{
			UnlockAchievement(GameAchievementEnum.ACH_ALL_MEDALS);
		}
	}

	public static void ConfirmTakenLevelValues()
	{
		foreach (HeroItemData levelTakenItemData in AchievementManager.LevelTakenItemDataList)
		{
			HeroItem item = new HeroItem(levelTakenItemData);
			Cargo.SetItem(-1, item);
		}
		foreach (string item3 in AchievementManager.LevelTakenBountyVillainSet)
		{
			if (!BountyVillainSet.Contains(item3))
			{
				BountyVillainSet.Add(item3);
				string text = $"villain/{item3}";
				HeroItemData heroItemData = GameCore.GetHeroItemData(text);
				if (heroItemData != null)
				{
					HeroItem item2 = new HeroItem(heroItemData);
					Cargo.SetItem(-1, item2);
				}
				else
				{
					GameCore.LogErr("Undefined [item = {0}] for villain", text);
				}
			}
		}
		bool flag = false;
		LevelStats value;
		if (GameCore.GameLevel.LevelData != null)
		{
			if (LevelStatsDictionary == null)
			{
				LevelStatsDictionary = new Dictionary<string, LevelStats>();
			}
			if (LevelStatsDictionary.Keys.Contains(GameCore.GameLevel.LevelData.Id))
			{
				value = LevelStatsDictionary[GameCore.GameLevel.LevelData.Id];
			}
			else
			{
				value = new LevelStats(GameCore.GameLevel.LevelData.Id);
				LevelStatsDictionary.Add(GameCore.GameLevel.LevelData.Id, value);
			}
			if (GameMode == GameModeEnum.Casual || value.FinishCountInCampaign == 0)
			{
				flag = true;
			}
			if (!Mia)
			{
				value.FinishCountInCampaign++;
				value.TotalFinishCount++;
			}
			if (Difficulty > value.HighestFinishDifficulty)
			{
				value.HighestFinishDifficulty = Difficulty;
			}
			OldLevelTopScore = value.TopScore;
			if (LevelScore > OldLevelTopScore)
			{
				if (OldLevelTopScore > 0)
				{
					AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_LEVEL_RECORD);
				}
				value.TopScore = LevelScore;
			}
			if (AchievementManager.LevelAchievedFindDictionary != null)
			{
				if (value.FindDictionary == null)
				{
					value.FindDictionary = new Dictionary<string, HashSet<int>>();
				}
				foreach (KeyValuePair<string, HashSet<int>> item4 in AchievementManager.LevelAchievedFindDictionary)
				{
					if (!value.FindDictionary.ContainsKey(item4.Key))
					{
						value.FindDictionary[item4.Key] = new HashSet<int>(item4.Value);
						continue;
					}
					foreach (int item5 in item4.Value)
					{
						value.FindDictionary[item4.Key].Add(item5);
					}
				}
			}
		}
		else
		{
			OldLevelTopScore = 0;
		}
		int num = ((!flag) ? Math.Max(0, LevelScore - OldLevelTopScore) : LevelScore);
		Cash += num;
		TotalStats.Score += num;
		AchievementManager.LevelAchievedCountArray[0] = num;
		foreach (string levelUnhide in AchievementManager.LevelUnhideList)
		{
			if (Campaign.GetLevelData(levelUnhide) == null)
			{
				GameCore.LogErr("Unable to unhide non campaign level_id '{0}'", levelUnhide);
				continue;
			}
			if (!LevelStatsDictionary.TryGetValue(levelUnhide, out value))
			{
				value = new LevelStats(levelUnhide);
				LevelStatsDictionary[levelUnhide] = value;
			}
			value.IsVisible = true;
			GameAchievementEnum achievementIdByString = GetAchievementIdByString($"ACH_SECRET_LEVEL_{levelUnhide.ToUpper()}");
			if (achievementIdByString != GameAchievementEnum.UNKNOWN)
			{
				UnlockAchievement(achievementIdByString);
				continue;
			}
			GameCore.LogErr("Undefined achievement for secret level '{0}'", levelUnhide);
		}
		if (IsDrunkenEffectEnabled)
		{
			UnlockAchievement(GameAchievementEnum.ACH_DRUNKEN_MASTER);
		}
		TotalStats.KillCountAirEnemy += AchievementManager.LevelAchievedCountArray[2];
		TotalStats.KillCountGroundEnemy += AchievementManager.LevelAchievedCountArray[3];
		TotalStats.KillCountBoss += AchievementManager.LevelAchievedCountArray[7];
		TotalStats.KillCountMan += AchievementManager.LevelAchievedCountArray[5];
		TotalStats.BonusTakenCount += AchievementManager.LevelAchievedCountArray[1];
		TotalStats.FiredShots += AchievementManager.LevelAchievedCountArray[6];
		TotalStats.DamageCaused += AchievementManager.DamageCaused;
		TotalStats.HeroDamageAbsorbed += AchievementManager.HeroDamageAbsorbed;
		TotalStats.LevelWinCount++;
		TotalStats.Recalculate();
		GlobalManKills += AchievementManager.LevelAchievedCountArray[5];
		if (AchievementManager.LevelAchievedCountArray[2] == 0 && AchievementManager.LevelAchievedCountArray[3] == 0 && AchievementManager.LevelAchievedCountArray[7] == 0 && AchievementManager.LevelAchievedCountArray[5] == 0 && AchievementManager.LevelAchievedCountArray[4] == 0)
		{
			UnlockAchievement(GameAchievementEnum.ACH_NO_KILL_WIN);
		}
		if (AchievementManager.HeroDamageAbsorbed == 0f)
		{
			UnlockAchievement(GameAchievementEnum.ACH_WIN_NO_DAMAGE_ABSORBED);
			AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_WIN_NO_DAMAGE_ABSORBED);
		}
		if (AchievementManager.HeroDamageAbsorbed >= 1000f)
		{
			UnlockAchievement(GameAchievementEnum.ACH_DAMAGE_ABSORBED_LEVEL);
		}
		ConfirmMedals();
		GameCore.SteamManager.SetAndStoreAll();
	}

	public static void ClearLevelAchievedValues()
	{
		LevelScore = 0;
		ZogonInsuranceCount = 0;
		OldLevelTopScore = 0;
		AchievementManager.ClearLevelAchievedValues();
	}

	public static bool GetIsFirstLevelTry()
	{
		if (LevelStatsDictionary == null)
		{
			return true;
		}
		if (Difficulty > 1)
		{
			return false;
		}
		foreach (LevelStats value in LevelStatsDictionary.Values)
		{
			if (value.FinishCountInCampaign > 0)
			{
				return false;
			}
		}
		return true;
	}

	public static bool LoadLevelData(string fileName)
	{
		LevelDataDictionary = new Dictionary<string, LevelData>();
		LevelData levelData = null;
		using (IniFile iniFile = IniFile.OpenFromContent(fileName))
		{
			if (iniFile == null)
			{
				return false;
			}
			while (iniFile.NextLine())
			{
				if (iniFile.ValueType == IniFileValueType.MainSection && iniFile.KeyEquals("level"))
				{
					levelData = new LevelData();
					levelData.Id = iniFile.Value.ToLowerLazy();
					if (LevelDataDictionary.ContainsKey(levelData.Id))
					{
						iniFile.LogErr("[Level = {0}] duplicity", levelData.Id);
					}
					LevelDataDictionary[levelData.Id] = levelData;
				}
				else if (levelData != null)
				{
					int[] resultArray;
					if (iniFile.KeyEquals("id"))
					{
						levelData.Id = iniFile.Value.ToLowerLazy();
					}
					else if (iniFile.KeyEquals("text_id"))
					{
						levelData.TextId = iniFile.Value;
					}
					else if (iniFile.KeyEquals("mandatory"))
					{
						levelData.IsMandatory = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("hidden"))
					{
						levelData.IsHidden = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("loading_atlas"))
					{
						levelData.LoadingAtlasId = iniFile.Value;
					}
					else if (iniFile.KeyEquals("win_to_level"))
					{
						levelData.WinToLevelId = iniFile.Value;
					}
					else if (iniFile.KeyEquals("file"))
					{
						levelData.FileName = iniFile.Value;
					}
					else if (iniFile.KeyEquals("debriefing"))
					{
						levelData.DebriefingMode = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("map_planet") && iniFile.TryGetIntArray(3, out resultArray))
					{
						levelData.PlanetNumber = resultArray[0];
						levelData.PlanetMapPosition = new Vector2(resultArray[1], resultArray[2]);
					}
					else if (iniFile.KeyEquals("preview_number"))
					{
						levelData.PreviewNumber = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("map_planet_info_offset"))
					{
						levelData.PlanetInfoOffset = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("final"))
					{
						levelData.IsFinalLevel = iniFile.GetBoolValue();
					}
					else if (iniFile.Key.StartsWith("find_") && iniFile.TryGetIntArray(1, 999, out resultArray))
					{
						string key = iniFile.Key.Substring(5);
						levelData.AvailableFindDictionary[key] = new HashSet<int>(resultArray);
					}
				}
			}
			foreach (LevelData value in LevelDataDictionary.Values)
			{
				if (value.FileName == null)
				{
					value.FileName = "level/" + value.Id + "/level.txt";
				}
				if (!GameContent.FileExits(value.FileName))
				{
					GameCore.LogErr("Level file '{0}' not found in file '{1}'.", value.FileName, iniFile.FileName);
					value.FileExists = false;
				}
				else
				{
					value.FileExists = true;
				}
			}
			GameCore.Log("Loaded {0} level datas from file '{1}'", LevelDataDictionary.Count, iniFile.FileName);
		}
		return true;
	}

	public static void CheckNoSaveGame()
	{
		for (int i = 1; i <= 5; i++)
		{
			if (SaveGame.LoadSaveGameInfo(i) != null)
			{
				return;
			}
		}
		SaveGame.CreateDefaultValues();
		Options.SaveGameNumber = 1;
		SaveGameNumber = Options.SaveGameNumber;
	}

	public static GameAchievementEnum GetAchievementIdByString(string idStr)
	{
		for (GameAchievementEnum gameAchievementEnum = GameAchievementEnum.ACH_BOSS_ROXTEDY; gameAchievementEnum < GameAchievementEnum.MAX_ACH; gameAchievementEnum++)
		{
			if (Utils.Equals(idStr, gameAchievementEnum.ToString()))
			{
				return gameAchievementEnum;
			}
		}
		return GameAchievementEnum.UNKNOWN;
	}

	public static void UnlockAchievement(GameAchievementEnum id)
	{
		if (Options.UseSteam && GameCore.SteamManager.IsInitialized)
		{
			GameCore.SteamManager.UnlockAchievement(id);
			return;
		}
		if (TotalStats.GameAchievementDictionary.TryGetValue(id, out var value))
		{
			value.IsAchieved = true;
			return;
		}
		GameCore.LogErr("Achievement '{0}' to unlock is not in dictionary", id);
	}

	public static void LockAchievement(GameAchievementEnum id)
	{
		if (Options.UseSteam && GameCore.SteamManager.IsInitialized)
		{
			GameCore.SteamManager.LockAchievement(id);
			return;
		}
		if (TotalStats.GameAchievementDictionary.TryGetValue(id, out var value))
		{
			value.IsAchieved = false;
			return;
		}
		GameCore.LogErr("Achievement '{0}' to lock is not in dictionary", id);
	}

	public static void UnlockAllAchievements()
	{
		foreach (GameAchievementEnum value in Enum.GetValues(typeof(GameAchievementEnum)))
		{
			if (value > GameAchievementEnum.UNKNOWN && value < GameAchievementEnum.MAX_ACH)
			{
				UnlockAchievement(value);
			}
		}
	}

	public static void LockAllAchievements()
	{
		foreach (GameAchievementEnum value in Enum.GetValues(typeof(GameAchievementEnum)))
		{
			if (value > GameAchievementEnum.UNKNOWN && value < GameAchievementEnum.MAX_ACH)
			{
				LockAchievement(value);
			}
		}
	}

	public static void CheckControlerUnplugged(bool bInformPlayer)
	{
		for (int i = 0; i < 2; i++)
		{
			int num = Options.GamePadNumber[i];
			if (Options.PlayerController[i] == GameInput.PlayerControllerEnum.Gamepad && num >= 0)
			{
				bool flag = GameInput.IsGamepadConnected(num);
				if (bInformPlayer && IsControllerConnected[i] != flag && !flag)
				{
					if (PopupMenuOptions.State == ShopMenuState.Hidden)
					{
						PopupMenuOptions.Show();
					}
					PopupMenuOptions.SetControllerUnplugged();
				}
				IsControllerConnected[i] = flag;
			}
			else
			{
				IsControllerConnected[i] = true;
			}
		}
	}

	public static void FinishCampaign()
	{
		RaiseDifficulty();
	}
}
