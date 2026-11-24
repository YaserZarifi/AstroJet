using System.Collections.Generic;

namespace Rake2D.JnG;

public static class TotalStats
{
	public static int Score;

	public static int KillCountAirEnemy;

	public static int KillCountGroundEnemy;

	public static int KillCountBoss;

	public static int KillCountMan;

	public static double DamageCaused;

	public static float HeroDamageAbsorbed;

	public static int FiredShots;

	public static int LevelWinCount;

	public static int CargoUnlockCount;

	public static int BonusTakenCount;

	public static Dictionary<GameAchievementEnum, GameAchievement> GameAchievementDictionary;

	public static int[] MedalArray;

	public const int MaxVillainCount = 10;

	public static int FindSecretCount { get; private set; }

	public static int FindHitchhikerCount { get; private set; }

	public static int MaxFindSecretCount { get; private set; }

	public static int MaxFindHitchhikerCount { get; private set; }

	static TotalStats()
	{
		ClearGameAchievements();
		MedalArray = new int[30];
	}

	public static void Recalculate()
	{
		FindSecretCount = 0;
		FindHitchhikerCount = 0;
		if (GameJnG.LevelStatsDictionary != null)
		{
			foreach (KeyValuePair<string, LevelStats> item in GameJnG.LevelStatsDictionary)
			{
				LevelStats value = item.Value;
				FindSecretCount += value.GetValidFindCount("secret");
				FindHitchhikerCount += value.GetValidFindCount("hitchhiker");
			}
		}
		MaxFindSecretCount = 0;
		MaxFindHitchhikerCount = 0;
		foreach (LevelData orderedLevelData in GameJnG.Campaign.GetOrderedLevelDataList())
		{
			if (orderedLevelData.AvailableFindDictionary != null)
			{
				if (orderedLevelData.AvailableFindDictionary.ContainsKey("secret"))
				{
					MaxFindSecretCount += orderedLevelData.AvailableFindDictionary["secret"].Count;
				}
				if (orderedLevelData.AvailableFindDictionary.ContainsKey("hitchhiker"))
				{
					MaxFindHitchhikerCount += orderedLevelData.AvailableFindDictionary["hitchhiker"].Count;
				}
			}
		}
		if (FindSecretCount >= MaxFindSecretCount)
		{
			GameJnG.UnlockAchievement(GameAchievementEnum.ACH_SECRET_ALL);
			GameJnG.AchievementManager.AchieveMedal(HeroMedalEnum.MEDAL_SECRET_ALL);
		}
		if (FindHitchhikerCount >= MaxFindHitchhikerCount)
		{
			GameJnG.UnlockAchievement(GameAchievementEnum.ACH_HITCHIKER_ALL);
		}
	}

	public static void Clear()
	{
		KillCountAirEnemy = 0;
		KillCountGroundEnemy = 0;
		KillCountBoss = 0;
		KillCountMan = 0;
		CargoUnlockCount = 0;
		BonusTakenCount = 0;
		DamageCaused = 0.0;
		HeroDamageAbsorbed = 0f;
		FiredShots = 0;
		Score = 0;
		LevelWinCount = 0;
		if (!GameCore.SteamManager.IsInitialized)
		{
			ClearGameAchievements();
		}
		MedalArray = new int[30];
	}

	public static void AddMedal(HeroMedalEnum medal)
	{
		MedalArray[(int)medal]++;
	}

	public static bool HasAllMedals()
	{
		int[] medalArray = MedalArray;
		for (int i = 0; i < medalArray.Length; i++)
		{
			if (medalArray[i] <= 0)
			{
				return false;
			}
		}
		return true;
	}

	public static int GetMedalCount(HeroMedalEnum medal)
	{
		return MedalArray[(int)medal];
	}

	public static void ClearGameAchievements()
	{
		GameAchievementDictionary = new Dictionary<GameAchievementEnum, GameAchievement>();
		for (GameAchievementEnum gameAchievementEnum = GameAchievementEnum.ACH_BOSS_ROXTEDY; gameAchievementEnum < GameAchievementEnum.MAX_ACH; gameAchievementEnum++)
		{
			GameAchievementDictionary.Add(gameAchievementEnum, new GameAchievement(gameAchievementEnum));
		}
	}
}
