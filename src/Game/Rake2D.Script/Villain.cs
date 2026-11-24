using Rake2D.JnG;

namespace Rake2D.Script;

public class Villain : BehaviorScript
{
	private string _villainId;

	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	public override void Init()
	{
		base.Init();
		_villainId = ParamString("villain_id") ?? MyObject.GetMainPart().Id;
		if (GameCore.IsEditorEnabled)
		{
			return;
		}
		if (GameJnG.IsBountyHunterEnabled && !GameJnG.BountyVillainSet.Contains(_villainId))
		{
			foreach (GameObject linkedObject in MyObject.GetMainPart().LinkedObjectList)
			{
				linkedObject.Destroy();
			}
			return;
		}
		MyObject.GetMainPart().Destroy();
	}

	private void OnKill()
	{
		GameJnG.AchievementManager.TakeVilalin(_villainId);
	}
}
