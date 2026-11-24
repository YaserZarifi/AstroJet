namespace Rake2D.Script;

public class OnStateKillMain : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.GetMainPart().Kill();
	}
}
