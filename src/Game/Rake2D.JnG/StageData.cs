using System.Collections.Generic;

namespace Rake2D.JnG;

public class StageData
{
	public int Number;

	public string Id;

	public string ShopLevelFileName;

	public float TestRangeHPFactor;

	public List<LevelData> LevelDataList;

	public StageData()
	{
		LevelDataList = new List<LevelData>();
	}

	public StageData Clone()
	{
		StageData stageData = new StageData();
		stageData.Number = Number;
		stageData.ShopLevelFileName = ShopLevelFileName;
		stageData.TestRangeHPFactor = TestRangeHPFactor;
		if (LevelDataList != null)
		{
			stageData.LevelDataList = new List<LevelData>();
			foreach (LevelData levelData in LevelDataList)
			{
				stageData.LevelDataList.Add(levelData);
			}
		}
		return stageData;
	}

	public override string ToString()
	{
		return Id;
	}
}
