using System.Collections.Generic;

namespace Rake2D.JnG;

public class AchievementManager
{
	public int[] LevelAvailableCountArray;

	public int[] LevelAchievedCountArray;

	public HashSet<HeroMedalEnum> LevelAchievedMedalSet;

	public float DamageCaused;

	public float HeroDamageAbsorbed;

	public Dictionary<string, HashSet<int>> LevelAchievedFindDictionary;

	public List<HeroItemData> LevelTakenItemDataList;

	public HashSet<string> LevelTakenBountyVillainSet;

	public List<string> LevelUnhideList;

	public AchievementManager()
	{
		int num = 8;
		LevelAvailableCountArray = new int[num];
		LevelAchievedCountArray = new int[num];
		LevelUnhideList = new List<string>();
		LevelAchievedFindDictionary = new Dictionary<string, HashSet<int>>();
		LevelTakenItemDataList = new List<HeroItemData>();
		LevelTakenBountyVillainSet = new HashSet<string>();
		LevelAchievedMedalSet = new HashSet<HeroMedalEnum>();
	}

	public void RaiseLevelAchievedCount(AchievementCountTypeEnum type)
	{
		ChangeLevelAchievedCount(type, 1);
	}

	public void ChangeLevelAchievedCount(AchievementCountTypeEnum type, int delta)
	{
		LevelAchievedCountArray[(int)type] += delta;
	}

	public void RaiseLevelAvailableCount(AchievementCountTypeEnum type)
	{
		LevelAvailableCountArray[(int)type]++;
	}

	public void AchieveMedal(HeroMedalEnum medal)
	{
		LevelAchievedMedalSet.Add(medal);
		TotalStats.AddMedal(medal);
		if (TotalStats.GetMedalCount(medal) == 1)
		{
			HeroItemData heroItemData = GameCore.GetHeroItemData($"medal/medal_{(int)medal}");
			if (heroItemData != null)
			{
				HeroItem item = new HeroItem(heroItemData);
				GameJnG.Cargo.SetItem(-1, item);
			}
		}
	}

	public int GetLevelCountPercent(AchievementCountTypeEnum type)
	{
		long num = LevelAvailableCountArray[(int)type];
		if (num <= 0)
		{
			return 100;
		}
		return (int)((float)LevelAchievedCountArray[(int)type] / (float)num * 100f);
	}

	public void ClearLevelAchievedValues()
	{
		for (int num = LevelAchievedCountArray.Length - 1; num >= 0; num--)
		{
			LevelAchievedCountArray[num] = 0;
		}
		DamageCaused = 0f;
		HeroDamageAbsorbed = 0f;
		LevelUnhideList.Clear();
		LevelAchievedFindDictionary.Clear();
		LevelTakenItemDataList.Clear();
		LevelTakenBountyVillainSet.Clear();
		LevelAchievedMedalSet.Clear();
	}

	public void ClearLevelAvailableValues()
	{
		for (int num = LevelAvailableCountArray.Length - 1; num >= 0; num--)
		{
			LevelAvailableCountArray[num] = 0;
		}
	}

	public static AchievementCountTypeEnum GetAchievementKillCountType(GameObject go)
	{
		if (go != null)
		{
			return GetAchievementKillCountType(go.TargetClass);
		}
		return AchievementCountTypeEnum.Unknown;
	}

	public void TakeVilalin(string id)
	{
		LevelTakenBountyVillainSet.Add(id);
	}

	public static AchievementCountTypeEnum GetAchievementKillCountType(TargetClass targetClass)
	{
		AchievementCountTypeEnum result = AchievementCountTypeEnum.Unknown;
		switch (targetClass)
		{
		case TargetClass.EnemyAir:
			result = AchievementCountTypeEnum.KillCountAirEnemy;
			break;
		case TargetClass.EnemyGround:
			result = AchievementCountTypeEnum.KillCountGroundEnemy;
			break;
		case TargetClass.Boss:
			result = AchievementCountTypeEnum.KillCountBoss;
			break;
		case TargetClass.Building:
			result = AchievementCountTypeEnum.KillCountBuilding;
			break;
		case TargetClass.Man:
			result = AchievementCountTypeEnum.KillCountMan;
			break;
		}
		return result;
	}
}
