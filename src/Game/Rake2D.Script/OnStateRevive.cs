namespace Rake2D.Script;

public class OnStateRevive : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject?.Revive();
	}
}
