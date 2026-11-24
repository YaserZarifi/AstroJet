using System.Collections.Generic;

namespace Rake2D.JnG;

public class LevelStats
{
	public int FinishCountInCampaign;

	public int TotalFinishCount;

	public int TopScore;

	public int HighestFinishDifficulty;

	public bool IsVisible;

	public Dictionary<string, HashSet<int>> FindDictionary;

	private string _levelId;

	public LevelStats(string levelId)
	{
		_levelId = levelId;
	}

	public int GetValidFindCount(string findId)
	{
		int num = 0;
		LevelData levelData = GameJnG.Campaign.GetLevelData(_levelId);
		if (FindDictionary != null && FindDictionary.ContainsKey(findId) && levelData != null && levelData.AvailableFindDictionary.ContainsKey(findId))
		{
			foreach (int item in FindDictionary[findId])
			{
				if (levelData.AvailableFindDictionary[findId].Contains(item))
				{
					num++;
				}
			}
		}
		return num;
	}
}
