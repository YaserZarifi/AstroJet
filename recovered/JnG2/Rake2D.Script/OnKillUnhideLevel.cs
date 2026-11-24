using Rake2D.JnG;

namespace Rake2D.Script;

public class OnKillUnhideLevel : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	private void OnKill()
	{
		string text = ParamString("level_id");
		if (text == null)
		{
			LogErr("Undefined level_id");
		}
		else if (GameJnG.Campaign.GetLevelData(text) == null)
		{
			LogErr("Unable to unhide non campaign level_id '{0}'", text);
		}
		else
		{
			GameJnG.AchievementManager.LevelUnhideList.Add(ParamString("level_id"));
		}
	}
}
