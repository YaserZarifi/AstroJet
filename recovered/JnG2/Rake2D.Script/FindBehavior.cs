using System.Collections.Generic;
using Rake2D.JnG;

namespace Rake2D.Script;

public class FindBehavior : BehaviorScript
{
	private string _findId;

	private int _findNumber;

	public override void RegisterEvents()
	{
	}

	public override void Init()
	{
		base.Init();
		_findId = ParamString("find_id");
		_findNumber = ParamInt("find_number");
	}

	public void SetFind()
	{
		if (MyLevel.LevelData == null || MyLevel.LevelData.AvailableFindDictionary == null || !MyLevel.LevelData.AvailableFindDictionary.ContainsKey(_findId) || !MyLevel.LevelData.AvailableFindDictionary[_findId].Contains(_findNumber))
		{
			LogErr($"find_{_findId} = {_findNumber} is not available in level {MyLevel.Id} - this find is not defined in level_set.txt");
			return;
		}
		if (!GameJnG.AchievementManager.LevelAchievedFindDictionary.ContainsKey(_findId))
		{
			GameJnG.AchievementManager.LevelAchievedFindDictionary[_findId] = new HashSet<int>();
		}
		if (!GameJnG.AchievementManager.LevelAchievedFindDictionary[_findId].Contains(_findNumber))
		{
			GameJnG.AchievementManager.LevelAchievedFindDictionary[_findId].Add(_findNumber);
		}
		else
		{
			LogErr($"Achieved find_{_findId} already contains number {_findNumber}");
		}
	}
}
