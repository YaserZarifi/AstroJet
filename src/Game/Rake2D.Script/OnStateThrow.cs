namespace Rake2D.Script;

public class OnStateThrow : ThrowObjectBehavior
{
	public override void RegisterEvents()
	{
		if (DoInEditor)
		{
			StartThrow();
		}
	}
}
