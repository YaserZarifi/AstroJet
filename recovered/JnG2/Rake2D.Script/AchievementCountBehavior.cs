using Rake2D.JnG;

namespace Rake2D.Script;

public abstract class AchievementCountBehavior : BehaviorScript
{
	protected AchievementCountTypeEnum _achievement;

	public override void Init()
	{
		base.Init();
		int achievement = ParamInt("achievement", -1);
		_achievement = (AchievementCountTypeEnum)achievement;
	}

	public virtual void RaiseAchievement()
	{
		if (_achievement != AchievementCountTypeEnum.Unknown)
		{
			GameJnG.AchievementManager.RaiseLevelAchievedCount(_achievement);
		}
	}
}
