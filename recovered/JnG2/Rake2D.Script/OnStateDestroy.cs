namespace Rake2D.Script;

public class OnStateDestroy : BehaviorScript
{
	public override void RegisterEvents()
	{
		if (DoInEditor)
		{
			MyObject.Destroy();
		}
	}
}
