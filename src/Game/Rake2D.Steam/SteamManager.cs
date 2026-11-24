using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Rake2D.JnG;
using Steamworks;

namespace Rake2D.Steam;

public class SteamManager
{
	public AppId_t STEAM_APP_ID = (AppId_t)830820u;

	public CSteamID UserId;

	public string UserName;

	public bool IsInitialized;

	public bool IsStatsStateValid;

	private List<LeaderboardEntry> _topScoreEntryList;

	private LeaderboardEntry _topScoreMyEntry;

	private Callback<GameOverlayActivated_t> _callbackGameOverlayActivated;

	private Callback<UserStatsReceived_t> _callbackUserStatsRecived;

	private Callback<UserStatsStored_t> _callbackUserStatsStored;

	private Callback<UserAchievementStored_t> _callbackAchievementStored;

	public SteamLeaderboard_t LeaderboardTopScoreStandardP1;

	public SteamLeaderboard_t LeaderboardTopScoreStandardP2;

	public SteamLeaderboard_t LeaderboardTopScoreHardcoreP1;

	public SteamLeaderboard_t LeaderboardTopScoreHardcoreP2;

	public SteamLeaderboard_t LeaderboardTetrixScore;

	public SteamLeaderboardEntries_t LeaderboardTopScoreEntries;

	public SteamLeaderboardEntries_t LeaderboardTetrixScoreEntries;

	private CallResult<LeaderboardFindResult_t> _onLeaderboardTopScoreStandardP1FindResultCallResult;

	private CallResult<LeaderboardFindResult_t> _onLeaderboardTopScoreStandardP2FindResultCallResult;

	private CallResult<LeaderboardFindResult_t> _onLeaderboardTopScoreHardcoreP1FindResultCallResult;

	private CallResult<LeaderboardFindResult_t> _onLeaderboardTopScoreHardcoreP2FindResultCallResult;

	private CallResult<LeaderboardScoresDownloaded_t> _onLeaderboardTopScoreDownloadedCallResult;

	private CallResult<LeaderboardScoresDownloaded_t> _onLeaderboardTopScoreForMyUserDownloadedCallResult;

	private CallResult<LeaderboardScoresDownloaded_t> _onLeaderboardTetrixScoreDownloadedCallResult;

	private CallResult<LeaderboardScoresDownloaded_t> _onLeaderboardTetrixScoreForMyUserDownloadedCallResult;

	private CallResult<LeaderboardFindResult_t> _onLeaderboardTetrixScoreFindResultCallResult;

	private CallResult<LeaderboardScoreUploaded_t> _onLeaderboardTopScoreUploadedCallResult;

	private CallResult<LeaderboardScoreUploaded_t> _onLeaderboardTetrixScoreUploadedCallResult;

	private SteamAPIWarningMessageHook_t _steamAPIWarningMessageHook;

	protected GameCore _gameCore;

	private bool _wasPaused;

	private bool _wasStatsRequested;

	private bool _storeStats;

	public bool IsSteamOverlayActive { get; private set; }

	public SteamManager(GameCore gameCore)
	{
		_gameCore = gameCore;
	}

	public void Init()
	{
		IsInitialized = false;
		if (!DllCheck.Test())
		{
			GameCore.LogErr("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
		}
		try
		{
			if (SteamAPI.RestartAppIfNecessary(STEAM_APP_ID))
			{
				GameCore.LogErr("Game will be restarted throw Steam. This instance shutting down...");
				_gameCore.GameExit(-1);
				return;
			}
		}
		catch (DllNotFoundException ex)
		{
			GameCore.LogErr("[Steamworks.NET] Could not load [lib]steam_api.dll.\nException:\n" + ex);
			return;
		}
		if (SteamAPI.Init())
		{
			_callbackGameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
			_callbackUserStatsRecived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
			_callbackUserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
			_callbackAchievementStored = Callback<UserAchievementStored_t>.Create(OnUserAchievementStored);
			_onLeaderboardTopScoreStandardP1FindResultCallResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardTopScoreStandardP1FindResult);
			_onLeaderboardTopScoreStandardP2FindResultCallResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardTopScoreStandardP2FindResult);
			_onLeaderboardTopScoreHardcoreP1FindResultCallResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardTopScoreHardcoreP1FindResult);
			_onLeaderboardTopScoreHardcoreP2FindResultCallResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardTopScoreHardcoreP2FindResult);
			_onLeaderboardTetrixScoreFindResultCallResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardTetrixScoreFindResult);
			_onLeaderboardTopScoreDownloadedCallResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardTopScoresDownloaded);
			_onLeaderboardTopScoreForMyUserDownloadedCallResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardTopScoresForMyUserDownloaded);
			_onLeaderboardTopScoreUploadedCallResult = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardTopScoreUploaded);
			_onLeaderboardTetrixScoreDownloadedCallResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardTetrixScoreDownloaded);
			_onLeaderboardTetrixScoreForMyUserDownloadedCallResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardTetrixScoreForMyUserDownloaded);
			_onLeaderboardTetrixScoreUploadedCallResult = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardTetrixScoreUploaded);
			_steamAPIWarningMessageHook = SteamAPIDebugTextHook;
			SteamClient.SetWarningMessageHook(_steamAPIWarningMessageHook);
			UserName = SteamFriends.GetPersonaName();
			UserId = SteamUser.GetSteamID();
			SteamAPICall_t hAPICall = SteamUserStats.FindLeaderboard("JNG2_TOP_SCORE_STANDARD_P1");
			_onLeaderboardTopScoreStandardP1FindResultCallResult.Set(hAPICall);
			hAPICall = SteamUserStats.FindLeaderboard("JNG2_TOP_SCORE_STANDARD_P2");
			_onLeaderboardTopScoreStandardP2FindResultCallResult.Set(hAPICall);
			hAPICall = SteamUserStats.FindLeaderboard("JNG2_TOP_SCORE_HARDCORE_P1");
			_onLeaderboardTopScoreHardcoreP1FindResultCallResult.Set(hAPICall);
			hAPICall = SteamUserStats.FindLeaderboard("JNG2_TOP_SCORE_HARDCORE_P2");
			_onLeaderboardTopScoreHardcoreP2FindResultCallResult.Set(hAPICall);
			hAPICall = SteamUserStats.FindLeaderboard("JNG2_TETRIX_SCORE");
			_onLeaderboardTetrixScoreFindResultCallResult.Set(hAPICall);
			GameCore.Log("Steam is initialized.");
			GameCore.Log("Steam name: {0} (Id: {1})", UserName, UserId);
			IsInitialized = true;
			CheckStatVersion();
		}
		else
		{
			GameCore.Log("No Steam.");
			IsInitialized = false;
		}
	}

	private void CheckStatVersion()
	{
		int pData = 0;
		if (SteamUserStats.GetStat(SteamStatEnum.STAT_EARLY_ACCESS.ToString(), out pData) && pData < 1)
		{
			ResetAllStats(bAchievementsToo: true);
			GameCore.Log("Steam achs and stats was reseted due to STAT_VERSION = {0}", pData);
		}
	}

	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		GameCore.LogErr(pchDebugText.ToString());
	}

	private void OnGameOverlayActivated(GameOverlayActivated_t callback)
	{
		if (callback.m_bActive != 0)
		{
			_wasPaused = GameCore.IsPaused;
			_gameCore.Pause(paused: true);
			IsSteamOverlayActive = true;
		}
		else
		{
			_gameCore.Pause(_wasPaused);
			IsSteamOverlayActive = false;
		}
	}

	private void OnUserStatsReceived(UserStatsReceived_t callback)
	{
		if (!((AppId_t)(uint)callback.m_nGameID == STEAM_APP_ID))
		{
			return;
		}
		GameCore.Log("Received stats and achievements from Steam");
		if (callback.m_eResult == EResult.k_EResultOK)
		{
			IsStatsStateValid = true;
			SteamUserStats.GetStat(SteamStatEnum.STAT_EARLY_ACCESS.ToString(), out int pData);
			if (pData > 0)
			{
				GameJnG.WasEarlyAccessBuy = true;
			}
			TotalStats.ClearGameAchievements();
			{
				foreach (GameAchievement value in TotalStats.GameAchievementDictionary.Values)
				{
					if (SteamUserStats.GetAchievement(value.IdStr, out value.IsAchieved))
					{
						value.Name = SteamUserStats.GetAchievementDisplayAttribute(value.IdStr, "name");
						value.Description = SteamUserStats.GetAchievementDisplayAttribute(value.IdStr, "desc");
					}
					else
					{
						GameCore.Log("SteamUserStats.GetAchievement failed for Achievement {0}", value.Id);
					}
				}
				return;
			}
		}
		GameCore.LogErr("RequestStats failed: {0}", callback.m_eResult);
	}

	private void OnUserStatsStored(UserStatsStored_t callback)
	{
		if ((uint)STEAM_APP_ID == callback.m_nGameID)
		{
			if (callback.m_eResult == EResult.k_EResultOK)
			{
				GameCore.Log("Steam stats stored.");
			}
			else
			{
				GameCore.LogErr("Steam stats storing failed, {0}", callback.m_eResult);
			}
		}
		_wasStatsRequested = false;
	}

	private void OnUserAchievementStored(UserAchievementStored_t callback)
	{
		if ((uint)STEAM_APP_ID == callback.m_nGameID)
		{
			GameCore.Log("Steam achievement '{0}' stored.", callback.m_rgchAchievementName);
		}
	}

	public void Update()
	{
		if (IsInitialized)
		{
			if (!_wasStatsRequested)
			{
				_wasStatsRequested = SteamUserStats.RequestCurrentStats();
			}
			if (_storeStats)
			{
				_storeStats = false;
				SteamUserStats.StoreStats();
			}
			SteamAPI.RunCallbacks();
		}
	}

	public void UnlockAchievement(GameAchievementEnum id)
	{
		if (TotalStats.GameAchievementDictionary.TryGetValue(id, out var value))
		{
			if (!value.IsAchieved)
			{
				value.IsAchieved = true;
				if (!SteamUserStats.SetAchievement(value.IdStr))
				{
					GameCore.LogErr("Unable to unlock Steam achievement '{0}'. SetAchievement failed.", value.IdStr);
				}
				_storeStats = true;
			}
		}
		else
		{
			GameCore.LogErr("Unable to unlock Steam achievement '{0}'. Does not exist.", id);
		}
	}

	public void LockAchievement(GameAchievementEnum id)
	{
		if (TotalStats.GameAchievementDictionary.TryGetValue(id, out var value))
		{
			if (value.IsAchieved)
			{
				value.IsAchieved = false;
				SteamUserStats.ClearAchievement(value.Id.ToString());
				_storeStats = true;
			}
		}
		else
		{
			GameCore.LogErr("Unable to clear Steam achievement '{0}'. Does not exist.", id);
		}
	}

	public void SetStat(SteamStatEnum statId, int value)
	{
		SetStat(statId.ToString(), value);
	}

	public void SetStat(string statId, int value)
	{
		if (IsInitialized && SteamUserStats.GetStat(statId, out int pData) && pData < value)
		{
			SteamUserStats.SetStat(statId, value);
			_storeStats = true;
		}
	}

	public void SetStat(SteamStatEnum statId, float value)
	{
		SetStat(statId.ToString(), value);
	}

	public void SetStat(string statId, float value)
	{
		if (IsInitialized && SteamUserStats.GetStat(statId, out float pData) && pData < value)
		{
			SteamUserStats.SetStat(statId.ToString(), value);
			_storeStats = true;
		}
	}

	public void ResetAllStats(bool bAchievementsToo)
	{
		if (!IsInitialized)
		{
			return;
		}
		if (SteamUserStats.ResetAllStats(bAchievementsToo))
		{
			if (bAchievementsToo)
			{
				GameCore.Log("All Steam stats and achs rested.");
			}
			else
			{
				GameCore.Log("All Steam stats rested. Achs stay unchanged.");
			}
		}
		else
		{
			GameCore.LogErr("Unable to reset Steam stats.");
		}
		SetStat(SteamStatEnum.STAT_EARLY_ACCESS, 1);
		SetStat(SteamStatEnum.STAT_VERSION, 1);
		SteamUserStats.RequestCurrentStats();
	}

	private void ProcessTopScoreLeaderBoard(ref SteamLeaderboard_t leaderBoard, LeaderboardFindResult_t callback)
	{
		GameCore.Log(string.Concat("[", 1104, " - LeaderboardFindResult] - ", callback.m_hSteamLeaderboard, " -- ", callback.m_bLeaderboardFound));
		if (callback.m_bLeaderboardFound != 0)
		{
			leaderBoard = callback.m_hSteamLeaderboard;
		}
	}

	private void OnLeaderboardTopScoreStandardP1FindResult(LeaderboardFindResult_t callback, bool bIOFailure)
	{
		ProcessTopScoreLeaderBoard(ref LeaderboardTopScoreStandardP1, callback);
	}

	private void OnLeaderboardTopScoreStandardP2FindResult(LeaderboardFindResult_t callback, bool bIOFailure)
	{
		ProcessTopScoreLeaderBoard(ref LeaderboardTopScoreStandardP2, callback);
	}

	private void OnLeaderboardTopScoreHardcoreP1FindResult(LeaderboardFindResult_t callback, bool bIOFailure)
	{
		ProcessTopScoreLeaderBoard(ref LeaderboardTopScoreHardcoreP1, callback);
	}

	private void OnLeaderboardTopScoreHardcoreP2FindResult(LeaderboardFindResult_t callback, bool bIOFailure)
	{
		ProcessTopScoreLeaderBoard(ref LeaderboardTopScoreHardcoreP2, callback);
	}

	private void OnLeaderboardTetrixScoreFindResult(LeaderboardFindResult_t pCallback, bool bIOFailure)
	{
		GameCore.Log(string.Concat("[", 1104, " - LeaderboardFindResult] - ", pCallback.m_hSteamLeaderboard, " -- ", pCallback.m_bLeaderboardFound));
		if (pCallback.m_bLeaderboardFound != 0)
		{
			LeaderboardTetrixScore = pCallback.m_hSteamLeaderboard;
		}
	}

	private void OnLeaderboardTopScoresDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
	{
		GameCore.Log(string.Concat("[", 1105, " - LeaderboardScoreDownloaded] - ", pCallback.m_hSteamLeaderboard, " -- ", pCallback.m_hSteamLeaderboardEntries, " -- ", pCallback.m_cEntryCount));
		LeaderboardTopScoreEntries = pCallback.m_hSteamLeaderboardEntries;
		try
		{
			if (_topScoreEntryList != null)
			{
				for (int i = 0; i < pCallback.m_cEntryCount; i++)
				{
					LeaderboardEntry leaderboardEntry = new LeaderboardEntry();
					leaderboardEntry.DetailsArray = new int[64];
					SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, i, out var pLeaderboardEntry, leaderboardEntry.DetailsArray, 64);
					leaderboardEntry.UserName = SteamFriends.GetFriendPersonaName(pLeaderboardEntry.m_steamIDUser);
					leaderboardEntry.Score = pLeaderboardEntry.m_nScore;
					leaderboardEntry.Rank = pLeaderboardEntry.m_nGlobalRank;
					leaderboardEntry.IsMyUser = pLeaderboardEntry.m_steamIDUser == UserId;
					leaderboardEntry.ParseDetails();
					_topScoreEntryList.Add(leaderboardEntry);
				}
			}
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Steam GetDownloadedLeaderboardEntry failed. Ex: {0}", ex);
		}
		GameJnG.MainMenu.ScreenLeaderboard.DoOnLeaderboardDownloaded();
	}

	private void OnLeaderboardTopScoresForMyUserDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
	{
		try
		{
			LeaderboardTopScoreEntries = pCallback.m_hSteamLeaderboardEntries;
			if (_topScoreMyEntry != null && pCallback.m_cEntryCount > 0)
			{
				_topScoreMyEntry.DetailsArray = new int[64];
				SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, 0, out var pLeaderboardEntry, _topScoreMyEntry.DetailsArray, 64);
				_topScoreMyEntry.UserName = SteamFriends.GetFriendPersonaName(pLeaderboardEntry.m_steamIDUser);
				_topScoreMyEntry.Score = pLeaderboardEntry.m_nScore;
				_topScoreMyEntry.Rank = pLeaderboardEntry.m_nGlobalRank;
				_topScoreMyEntry.IsMyUser = pLeaderboardEntry.m_steamIDUser == UserId;
				_topScoreMyEntry.ParseDetails();
			}
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Steam GetDownloadedLeaderboardEntry failed. Ex: {0}", ex);
		}
		GameJnG.MainMenu.ScreenLeaderboard.DoOnLeaderboardForMyUserDownloaded();
	}

	private void OnLeaderboardTetrixScoreDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
	{
		try
		{
			GameJnG.ShopMenu.Atarix.ContentTetrix.LeaderBoardEntryList = new List<LeaderboardEntry>();
			for (int i = 0; i < pCallback.m_cEntryCount; i++)
			{
				LeaderboardEntry leaderboardEntry = new LeaderboardEntry();
				SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, i, out var pLeaderboardEntry, leaderboardEntry.DetailsArray, 1);
				leaderboardEntry.UserName = SteamFriends.GetFriendPersonaName(pLeaderboardEntry.m_steamIDUser);
				leaderboardEntry.Score = pLeaderboardEntry.m_nScore;
				leaderboardEntry.Rank = pLeaderboardEntry.m_nGlobalRank;
				leaderboardEntry.IsMyUser = pLeaderboardEntry.m_steamIDUser == UserId;
				GameJnG.ShopMenu.Atarix.ContentTetrix.LeaderBoardEntryList.Add(leaderboardEntry);
			}
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Steam GetDownloadedLeaderboardEntry failed. Ex: {0}", ex);
		}
	}

	private void OnLeaderboardTetrixScoreForMyUserDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
	{
		_ = new int[1];
		try
		{
			if (pCallback.m_cEntryCount > 0)
			{
				GameJnG.ShopMenu.Atarix.ContentTetrix.LeaderBoardEntryMyUser = new LeaderboardEntry();
				SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, 0, out var pLeaderboardEntry, GameJnG.ShopMenu.Atarix.ContentTetrix.LeaderBoardEntryMyUser.DetailsArray, 1);
				GameJnG.ShopMenu.Atarix.ContentTetrix.LeaderBoardEntryMyUser.UserName = SteamFriends.GetFriendPersonaName(pLeaderboardEntry.m_steamIDUser);
				GameJnG.ShopMenu.Atarix.ContentTetrix.LeaderBoardEntryMyUser.Score = pLeaderboardEntry.m_nScore;
				GameJnG.ShopMenu.Atarix.ContentTetrix.LeaderBoardEntryMyUser.Rank = pLeaderboardEntry.m_nGlobalRank;
				GameJnG.ShopMenu.Atarix.ContentTetrix.LeaderBoardEntryMyUser.IsMyUser = pLeaderboardEntry.m_steamIDUser == UserId;
			}
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Steam GetDownloadedLeaderboardEntry failed. Ex: {0}", ex);
		}
	}

	private void OnLeaderboardTopScoreUploaded(LeaderboardScoreUploaded_t pCallback, bool bIOFailure)
	{
		GameCore.Log(string.Concat("[", 1106, " - LeaderboardScoreUploaded] - ", pCallback.m_bSuccess, " -- ", pCallback.m_hSteamLeaderboard, " -- ", pCallback.m_nScore, " -- ", pCallback.m_bScoreChanged, " -- ", pCallback.m_nGlobalRankNew, " -- ", pCallback.m_nGlobalRankPrevious));
	}

	private void OnLeaderboardTetrixScoreUploaded(LeaderboardScoreUploaded_t pCallback, bool bIOFailure)
	{
		GameCore.Log(string.Concat("[", 1106, " - LeaderboardTetrixScoreUploaded] - ", pCallback.m_bSuccess, " -- ", pCallback.m_hSteamLeaderboard, " -- ", pCallback.m_nScore, " -- ", pCallback.m_bScoreChanged, " -- ", pCallback.m_nGlobalRankNew, " -- ", pCallback.m_nGlobalRankPrevious));
	}

	private void StoreShipChoiceIndex(List<int> valueList, Hero hero, string partMapId)
	{
		string value = hero.MyShip.PartMap.MapIds[partMapId];
		HeroPartChoice heroPartChoiceById = GameJnG.GetHeroPartChoiceById(partMapId);
		if (heroPartChoiceById == null)
		{
			return;
		}
		bool flag = false;
		foreach (HeroPartChoiceItem item in heroPartChoiceById.ItemList)
		{
			for (int i = 0; i < item.IdList.Count; i++)
			{
				if (Utils.Equals(item.IdList[i], value))
				{
					valueList.Add(item.NumId);
					valueList.Add(i);
					flag = true;
					break;
				}
				if (flag)
				{
					break;
				}
			}
		}
		if (!flag)
		{
			valueList.Add(0);
			valueList.Add(0);
		}
	}

	private void StoreHeroLeaderBoardDetail(List<int> valueList, Hero hero)
	{
		valueList.Add(hero.FaceNumber);
		valueList.Add(hero.GetCamouflageIndex());
		StoreShipChoiceIndex(valueList, hero, "body");
		StoreShipChoiceIndex(valueList, hero, "body/nose");
		StoreShipChoiceIndex(valueList, hero, "body/side_front");
		StoreShipChoiceIndex(valueList, hero, "body/side_front/wing");
		StoreShipChoiceIndex(valueList, hero, "body/rudder");
		StoreShipChoiceIndex(valueList, hero, "body/jet");
		if (hero.MyShip.PartMap.PartOffsetXPercentages.TryGetValue("side_front", out var value))
		{
			valueList.Add((int)(Math.Round(value, 2) * 100.0));
		}
		else
		{
			valueList.Add(30);
		}
	}

	private void StoreHeroNames(string name1, string name2, List<int> valueList)
	{
		for (int i = 0; i < 16; i++)
		{
			byte[] first = ((name1.Length <= i) ? new byte[2] : BitConverter.GetBytes(name1[i]));
			byte[] second = ((name2.Length <= i) ? new byte[2] : BitConverter.GetBytes(name2[i]));
			byte[] value = first.Concat(second).ToArray();
			valueList.Add(BitConverter.ToInt32(value, 0));
		}
	}

	private int[] GetTopScoreLeaderboardDetail()
	{
		List<int> list = new List<int>();
		list.Add(GameJnG.PlayerCount);
		list.Add(GameJnG.Difficulty);
		list.Add(GameJnG.StageNumber);
		StoreHeroLeaderBoardDetail(list, GameJnG.HeroList[0]);
		if (GameJnG.PlayerCount > 1)
		{
			StoreHeroLeaderBoardDetail(list, GameJnG.HeroList[1]);
		}
		for (int i = list.Count; i < 48; i++)
		{
			list.Add(-1);
		}
		StoreHeroNames(GameJnG.HeroList[0].PlayerName, (GameJnG.PlayerCount > 1) ? GameJnG.HeroList[1].PlayerName : "", list);
		return list.ToArray();
	}

	private SteamLeaderboard_t GetTopScoreLeaderboard(GameModeEnum gameMode, int playerCount)
	{
		switch (gameMode)
		{
		case GameModeEnum.Standard:
			if (playerCount < 2)
			{
				return LeaderboardTopScoreStandardP1;
			}
			return LeaderboardTopScoreStandardP2;
		case GameModeEnum.HardCore:
			if (playerCount < 2)
			{
				return LeaderboardTopScoreHardcoreP1;
			}
			return LeaderboardTopScoreHardcoreP2;
		default:
			GameCore.LogErr("Wrong game mode ({0}) for top score leadeboard", gameMode);
			return default;
		}
	}

	public void SetAndStoreAll()
	{
		SetStat(SteamStatEnum.STAT_DIFF, GameJnG.Difficulty);
		SetStat(SteamStatEnum.STAT_BONUS_COUNT, TotalStats.BonusTakenCount);
		SetStat(SteamStatEnum.STAT_CARGO_UNLOCK, TotalStats.CargoUnlockCount);
		SetStat(SteamStatEnum.STAT_DAMAGE_ABSORBED, TotalStats.HeroDamageAbsorbed);
		SetStat(SteamStatEnum.STAT_DAMAGE_CAUSED, (float)TotalStats.DamageCaused);
		SetStat(SteamStatEnum.STAT_FIRED_SHOTS, TotalStats.FiredShots);
		SetStat(SteamStatEnum.STAT_KILL_BOSS, TotalStats.KillCountBoss);
		SetStat(SteamStatEnum.STAT_KILL_MAN, TotalStats.KillCountMan);
		SetStat(SteamStatEnum.STAT_KILL_ENEMY, TotalStats.KillCountGroundEnemy + TotalStats.KillCountAirEnemy);
		SetStat(SteamStatEnum.STAT_LEVEL_WIN, TotalStats.LevelWinCount);
		SetStat(SteamStatEnum.STAT_SCORE, TotalStats.Score);
		UploadTopScoreLeaderboard();
	}

	public void DownloadTopScoreLeaderboard(List<LeaderboardEntry> entryList, LeaderboardEntry topScoreMyEntry, GameModeEnum gameMode, int playerCount)
	{
		if (IsInitialized)
		{
			_topScoreEntryList = entryList;
			_topScoreMyEntry = topScoreMyEntry;
			SteamAPICall_t hAPICall = SteamUserStats.DownloadLeaderboardEntries(GetTopScoreLeaderboard(gameMode, playerCount), ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 0, 10);
			_onLeaderboardTopScoreDownloadedCallResult.Set(hAPICall);
			CSteamID[] prgUsers = new CSteamID[1] { UserId };
			hAPICall = SteamUserStats.DownloadLeaderboardEntriesForUsers(GetTopScoreLeaderboard(gameMode, playerCount), prgUsers, 1);
			_onLeaderboardTopScoreForMyUserDownloadedCallResult.Set(hAPICall);
		}
	}

	public void UploadTopScoreLeaderboard()
	{
		if (IsInitialized && GameJnG.GameMode != GameModeEnum.Casual && GameJnG.SaveGameVersion > 1 && !GameJnG.IsCheater)
		{
			try
			{
				int[] topScoreLeaderboardDetail = GetTopScoreLeaderboardDetail();
				SteamAPICall_t hAPICall = SteamUserStats.UploadLeaderboardScore(GetTopScoreLeaderboard(GameJnG.GameMode, GameJnG.PlayerCount), ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, Math.Min(TotalStats.Score, int.MaxValue), topScoreLeaderboardDetail, topScoreLeaderboardDetail.Length);
				_onLeaderboardTopScoreUploadedCallResult.Set(hAPICall);
			}
			catch (Exception ex)
			{
				GameCore.LogErr("Steam UploadLeaderboardScore failed. Ex: {0}", ex);
			}
		}
	}

	public void UploadTetrixLeaderboards(int score, int level)
	{
		if (IsInitialized && GameJnG.SaveGameVersion > 1 && !GameJnG.IsCheater)
		{
			try
			{
				int[] array = new int[1] { level };
				SteamAPICall_t hAPICall = SteamUserStats.UploadLeaderboardScore(LeaderboardTetrixScore, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, score, array, array.Length);
				_onLeaderboardTetrixScoreUploadedCallResult.Set(hAPICall);
			}
			catch (Exception ex)
			{
				GameCore.LogErr("Steam UploadLeaderboardScore failed. Ex: {0}", ex);
			}
		}
	}

	public void DownloadTetrixLeaderboard()
	{
		if (IsInitialized)
		{
			SteamAPICall_t hAPICall = SteamUserStats.DownloadLeaderboardEntries(LeaderboardTetrixScore, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 0, 10);
			_onLeaderboardTetrixScoreDownloadedCallResult.Set(hAPICall);
			CSteamID[] prgUsers = new CSteamID[1] { UserId };
			hAPICall = SteamUserStats.DownloadLeaderboardEntriesForUsers(LeaderboardTetrixScore, prgUsers, 1);
			_onLeaderboardTetrixScoreForMyUserDownloadedCallResult.Set(hAPICall);
		}
	}

	public void Dump()
	{
		if (IsInitialized)
		{
			GameCore.Log("STEAM DUMP (AppId: {0}, User: {1}, Id :{2})", STEAM_APP_ID, UserName, UserId);
			GameCore.Log(" State: {0}, {1}", IsInitialized ? "OK" : "Disabled", IsStatsStateValid ? "Valid" : "Invalid");
			GameCore.Log(" Stats ({0}):", 13);
			for (SteamStatEnum steamStatEnum = SteamStatEnum.STAT_EARLY_ACCESS; steamStatEnum < SteamStatEnum.MAX_STAT; steamStatEnum++)
			{
				SteamUserStats.GetStat(steamStatEnum.ToString(), out int pData);
				GameCore.Log("  - {0}: {1}", steamStatEnum, pData);
			}
			GameCore.Log(" Leader boards:");
			object[] array = new object[1];
			_ = LeaderboardTopScoreStandardP1;
			array[0] = "Founded";
			GameCore.Log("  - Top score: {0}", array);
			object[] array2 = new object[1];
			_ = LeaderboardTetrixScore;
			array2[0] = "Founded";
			GameCore.Log("  - Tetrix score: {0}", array2);
		}
		else
		{
			GameCore.LogErr("Steam not initialized");
		}
		GameCore.Log(" Achievements ({0}):", TotalStats.GameAchievementDictionary.Count);
		foreach (GameAchievement value in TotalStats.GameAchievementDictionary.Values)
		{
			GameCore.Log("  - ({0}) {1} ('{2}'): {3}", (int)value.Id, value.Id, value.Name, value.IsAchieved ? "Achieved" : "Not achieved");
		}
	}

	public void Destroy()
	{
		if (IsInitialized)
		{
			GameCore.Log("Steam shutting down.");
			SteamAPI.Shutdown();
			IsInitialized = false;
		}
	}

	public void ShowWebPage(string urlSteam, string urlWeb)
	{
		if (IsInitialized)
		{
			SteamFriends.ActivateGameOverlayToWebPage(urlSteam);
		}
		else
		{
			Process.Start(urlWeb);
		}
	}
}
