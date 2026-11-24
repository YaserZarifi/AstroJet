namespace Rake2D.Script;

public class OnStateShock : ShockBehavior
{
	public override void RegisterEvents()
	{
		Shock(MyObject.WorldPosition, null);
	}
}
