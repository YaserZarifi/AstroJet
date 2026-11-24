using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rake2D.JnG;

public class Campaign
{
	public List<StageData> StageDataList;

	public Campaign()
	{
		StageDataList = new List<StageData>();
	}

	public string GetShopLevelFileName(int stageNumber)
	{
		return GetStageData(stageNumber)?.ShopLevelFileName;
	}

	public List<LevelData> GetOrderedLevelDataList()
	{
		List<LevelData> list = new List<LevelData>();
		foreach (StageData item in StageDataList.OrderBy((StageData s) => s.Number))
		{
			list.AddRange(item.LevelDataList);
		}
		return list;
	}

	public LevelData GetLevelData(string id)
	{
		new List<LevelData>();
		foreach (StageData stageData in StageDataList)
		{
			LevelData levelData = stageData.LevelDataList.FirstOrDefault((LevelData d) => Utils.Equals(d.Id, id));
			if (levelData != null)
			{
				return levelData;
			}
		}
		return null;
	}

	public LevelData GetLevelDataByFileName(string fileName)
	{
		new List<LevelData>();
		foreach (StageData stageData in StageDataList)
		{
			LevelData levelData = stageData.LevelDataList.FirstOrDefault((LevelData d) => Utils.Equals(d.FileName, fileName));
			if (levelData != null)
			{
				return levelData;
			}
		}
		return null;
	}

	public Campaign Clone()
	{
		Campaign campaign = new Campaign();
		if (StageDataList != null)
		{
			campaign.StageDataList = new List<StageData>();
			foreach (StageData stageData in StageDataList)
			{
				campaign.StageDataList.Add(stageData.Clone());
			}
		}
		return campaign;
	}

	public int GetStageNumberByLevelData(LevelData levelData)
	{
		if (levelData != null)
		{
			for (int i = 0; i < StageDataList.Count; i++)
			{
				StageData stageData = StageDataList[i];
				foreach (LevelData levelData2 in stageData.LevelDataList)
				{
					if (Utils.Equals(levelData2.Id, levelData.Id))
					{
						return stageData.Number;
					}
				}
			}
		}
		return 0;
	}

	public StageData GetStageData(int stageNumber)
	{
		return StageDataList.FirstOrDefault((StageData s) => s.Number == stageNumber);
	}

	public static Campaign LoadCampaign(int difficulty)
	{
		return LoadCampaign("campaign.txt", difficulty);
	}

	public void ShowAllSecretLevels()
	{
		foreach (StageData stageData in StageDataList)
		{
			foreach (LevelData levelData in stageData.LevelDataList)
			{
				if (levelData.IsHidden)
				{
					if (!GameJnG.LevelStatsDictionary.TryGetValue(levelData.Id, out var value))
					{
						value = new LevelStats(levelData.Id);
						GameJnG.LevelStatsDictionary[levelData.Id] = value;
					}
					value.IsVisible = true;
				}
			}
		}
	}

	public static void HideAllSecretLevels()
	{
		if (GameJnG.LevelStatsDictionary == null)
		{
			return;
		}
		foreach (LevelStats value in GameJnG.LevelStatsDictionary.Values)
		{
			value.IsVisible = false;
		}
	}

	private static Campaign LoadCampaign(string fileName, int difficulty)
	{
		GameJnG.LoadLevelData("level_set.txt");
		Campaign campaign = null;
		Campaign campaign2 = null;
		using (IniFile iniFile = IniFile.OpenFromContent(fileName))
		{
			while (iniFile.NextLine())
			{
				if (iniFile.ValueType != IniFileValueType.MainSection || !iniFile.KeyEquals("campaign"))
				{
					continue;
				}
				if (iniFile.ValueEquals("default"))
				{
					campaign2 = new Campaign();
					campaign2.Load(iniFile);
					continue;
				}
				if (campaign2 == null)
				{
					iniFile.LogErr("Undefined [campaign = default] - unable to load [campagn = {0}]", difficulty);
					return null;
				}
				if (iniFile.GetIntValue() != difficulty)
				{
					continue;
				}
				campaign = campaign2.Clone();
				campaign.Load(iniFile);
				break;
			}
		}
		return campaign ?? campaign2;
	}

	public bool Load(IniFile iniFile)
	{
		StageData stageData = null;
		LevelData levelData = null;
		while (iniFile.NextLine() && (iniFile.ValueType != IniFileValueType.MainSection || (!iniFile.KeyEquals("campaign") && !iniFile.KeyEquals("end"))))
		{
			if (iniFile.ValueType == IniFileValueType.MainSection && iniFile.KeyEquals("stage"))
			{
				int number = iniFile.GetIntValue();
				stageData = StageDataList.FirstOrDefault((StageData s) => s.Number == number);
				if (stageData != null)
				{
					stageData.LevelDataList.Clear();
				}
				else
				{
					stageData = new StageData();
					stageData.Id = iniFile.Value.ToLowerLazy();
					stageData.Number = number;
					StageDataList.Add(stageData);
				}
				levelData = null;
				continue;
			}
			if (stageData != null)
			{
				if (iniFile.ValueType == IniFileValueType.SubSection && iniFile.KeyEquals("level"))
				{
					string key = iniFile.Value.ToLowerLazy();
					if (GameJnG.LevelDataDictionary.ContainsKey(key))
					{
						levelData = GameJnG.LevelDataDictionary[key];
						stageData.LevelDataList.Add(levelData);
					}
				}
				else
				{
					if (iniFile.KeyEquals("shop_level"))
					{
						stageData.ShopLevelFileName = iniFile.Value;
						continue;
					}
					if (iniFile.KeyEquals("test_range_hp_factor"))
					{
						stageData.TestRangeHPFactor = iniFile.GetFloatValue();
						continue;
					}
				}
			}
			if (levelData != null)
			{
				if (iniFile.KeyEquals("hp_factor"))
				{
					levelData.HPFactor = iniFile.GetFloatValue();
				}
				else if (iniFile.KeyEquals("damage_to_hero_factor"))
				{
					levelData.DamageToHeroFactor = iniFile.GetFloatValue();
				}
				else if (iniFile.KeyEquals("score_factor"))
				{
					levelData.ScoreFactor = iniFile.GetFloatValue();
				}
			}
		}
		foreach (StageData stageData2 in StageDataList)
		{
			if (stageData2.ShopLevelFileName == null)
			{
				stageData2.ShopLevelFileName = Path.Combine("level", "shop", "shop_0", "level.txt");
			}
			if (!GameContent.FileExits(stageData2.ShopLevelFileName))
			{
				GameCore.LogErr("Shop level file '{0}' not found.", stageData2.ShopLevelFileName);
			}
		}
		return true;
	}

	public void DebugPrint()
	{
		GameCore.Log("--- CAMPAIGN ---");
		foreach (StageData stageData in GameJnG.Campaign.StageDataList)
		{
			GameCore.Log($"[stage = {stageData.Number}]");
			foreach (LevelData levelData in stageData.LevelDataList)
			{
				LevelStats levelStats = ((!GameJnG.LevelStatsDictionary.Keys.Contains(levelData.Id)) ? new LevelStats(levelData.Id) : GameJnG.LevelStatsDictionary[levelData.Id]);
				GameCore.Log($"  {levelData.Id} ('{levelData.FileName}'), Finishes: {levelStats.FinishCountInCampaign} / {levelStats.TotalFinishCount}, Best: {levelStats.TopScore}");
			}
		}
	}
}
