namespace Rake2D.Script;

public class OnStateDestroyMain : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.GetMainPart().Destroy();
	}
}
