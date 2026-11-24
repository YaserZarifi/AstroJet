using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.JnG;

public class CheckpointInfo
{
	public bool IsSaved;

	public Vector2 Position;

	public int[] AchievedCountArray;

	public int[] AvailableCountArray;

	public float DamageCausing;

	public float HeroDamageAbsorbed;

	public Dictionary<string, HashSet<int>> AchievedFindDictionary;

	public List<string> LevelUnhideList;

	public List<HeroCheckPointInfo> HeroInfoList;

	public int LevelScore;

	public List<HUDCounter> HUDCounterList;

	public List<HeroItemData> TakenItemDataList;

	public HashSet<string> TakenBountyVillainSet;

	public void Clear()
	{
		IsSaved = false;
		AchievedCountArray = null;
		AvailableCountArray = null;
		HUDCounterList = null;
		DamageCausing = 0f;
		HeroDamageAbsorbed = 0f;
	}

	public void Save(Vector2 position)
	{
		Position = position;
		AchievedCountArray = new int[GameJnG.AchievementManager.LevelAchievedCountArray.Length];
		GameJnG.AchievementManager.LevelAchievedCountArray.CopyTo(AchievedCountArray, 0);
		AvailableCountArray = new int[GameJnG.AchievementManager.LevelAvailableCountArray.Length];
		GameJnG.AchievementManager.LevelAvailableCountArray.CopyTo(AvailableCountArray, 0);
		DamageCausing = GameJnG.AchievementManager.DamageCaused;
		HeroDamageAbsorbed = GameJnG.AchievementManager.HeroDamageAbsorbed;
		AchievedFindDictionary = new Dictionary<string, HashSet<int>>();
		foreach (KeyValuePair<string, HashSet<int>> item2 in GameJnG.AchievementManager.LevelAchievedFindDictionary)
		{
			if (!AchievedFindDictionary.ContainsKey(item2.Key))
			{
				AchievedFindDictionary[item2.Key] = new HashSet<int>(item2.Value);
				continue;
			}
			foreach (int item3 in item2.Value)
			{
				AchievedFindDictionary[item2.Key].Add(item3);
			}
		}
		TakenItemDataList = new List<HeroItemData>(GameJnG.AchievementManager.LevelTakenItemDataList);
		TakenBountyVillainSet = new HashSet<string>(GameJnG.AchievementManager.LevelTakenBountyVillainSet);
		if (GameJnG.AchievementManager.LevelUnhideList != null)
		{
			LevelUnhideList = new List<string>(GameJnG.AchievementManager.LevelUnhideList);
		}
		else
		{
			LevelUnhideList = null;
		}
		LevelScore = GameJnG.LevelScore;
		HeroInfoList = new List<HeroCheckPointInfo>();
		foreach (Hero hero in GameJnG.HeroList)
		{
			new HeroCheckPointInfo(hero);
			HeroInfoList.Add(new HeroCheckPointInfo(hero));
		}
		List<HUDCounter> counterList = GameJnG.HeroList[0].Hud.GetCounterList();
		if (counterList != null)
		{
			HUDCounterList = new List<HUDCounter>();
			foreach (HUDCounter item4 in counterList)
			{
				HUDCounter item = item4.Clone();
				HUDCounterList.Add(item);
			}
		}
		else
		{
			HUDCounterList = null;
		}
		IsSaved = true;
	}

	public void ApplyPosition(GameLevel level)
	{
		level.Camera.Position = new Vector2(Position.X - 960f, 0f);
	}

	public void AplyValuesBeforeBeginLevel()
	{
		GameJnG.HeroList[0].Hud.SetCounterList(HUDCounterList);
	}

	public void ApplyValues()
	{
		AchievedCountArray.CopyTo(GameJnG.AchievementManager.LevelAchievedCountArray, 0);
		AvailableCountArray.CopyTo(GameJnG.AchievementManager.LevelAvailableCountArray, 0);
		GameJnG.AchievementManager.DamageCaused = DamageCausing;
		GameJnG.AchievementManager.HeroDamageAbsorbed = HeroDamageAbsorbed;
		GameJnG.AchievementManager.LevelAchievedFindDictionary = new Dictionary<string, HashSet<int>>();
		if (AchievedFindDictionary != null)
		{
			foreach (KeyValuePair<string, HashSet<int>> item in AchievedFindDictionary)
			{
				if (!GameJnG.AchievementManager.LevelAchievedFindDictionary.ContainsKey(item.Key))
				{
					GameJnG.AchievementManager.LevelAchievedFindDictionary[item.Key] = new HashSet<int>(item.Value);
					continue;
				}
				foreach (int item2 in item.Value)
				{
					GameJnG.AchievementManager.LevelAchievedFindDictionary[item.Key].Add(item2);
				}
			}
		}
		GameJnG.AchievementManager.LevelTakenItemDataList = new List<HeroItemData>(TakenItemDataList);
		GameJnG.AchievementManager.LevelTakenBountyVillainSet = new HashSet<string>(TakenBountyVillainSet);
		if (LevelUnhideList != null)
		{
			GameJnG.AchievementManager.LevelUnhideList = new List<string>(LevelUnhideList);
		}
		GameJnG.LevelScore = LevelScore;
		for (int i = 0; i < GameJnG.HeroList.Count && i < HeroInfoList.Count; i++)
		{
			Hero hero = GameJnG.HeroList[i];
			HeroCheckPointInfo heroCheckPointInfo = HeroInfoList[i];
			hero.SetShipHP(Math.Max(heroCheckPointInfo.HP, 50f));
			hero.SetMana(heroCheckPointInfo.Mana);
			hero.SetShield(heroCheckPointInfo.Shield);
		}
	}
}
