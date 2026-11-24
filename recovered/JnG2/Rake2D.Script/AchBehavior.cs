using Rake2D.JnG;

namespace Rake2D.Script;

public class AchBehavior : BehaviorScript
{
	protected GameAchievementEnum _achievementId;

	protected IntRange _difficultyRange;

	public override void RegisterEvents()
	{
	}

	public override void Init()
	{
		base.Init();
		string text = ParamString("ach");
		_achievementId = GameJnG.GetAchievementIdByString(text);
		if (text == null)
		{
			LogErr("Undefined param $ach");
		}
		else if (_achievementId == GameAchievementEnum.UNKNOWN)
		{
			LogErr("Wrong param '$ach = {0}' - unknown achievement id", text);
		}
		_difficultyRange = ParamIntRange("diff", 0, 9999);
	}

	public void UnlockAch()
	{
		if (_achievementId != GameAchievementEnum.UNKNOWN && _difficultyRange.IsInRange(GameJnG.Difficulty))
		{
			GameJnG.UnlockAchievement(_achievementId);
		}
	}
}
