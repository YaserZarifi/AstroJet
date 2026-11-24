namespace Rake2D.Script;

public class OnKillRaiseAchievement : AchievementCountBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += RaiseAchievement;
	}
}
