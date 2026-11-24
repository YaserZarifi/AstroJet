using Rake2D.JnG;

namespace Rake2D.Script;

public class OnStateCheckPoint : BehaviorScript
{
	public override void RegisterEvents()
	{
		GameJnG.CheckpointInfo.Save(MyObject.WorldPosition);
	}
}
