using Rake2D.JnG;

namespace Rake2D.Script;

public class BountyPart : BehaviorScript
{
	public override void RegisterEvents()
	{
	}

	public override void Init()
	{
		if (!GameCore.IsEditorEnabled && (!GameJnG.IsBountyHunterEnabled || Utils.GetRandom(99) >= ParamInt("probability", 100)))
		{
			MyObject.Destroy();
		}
	}
}
