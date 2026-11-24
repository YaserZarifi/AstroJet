namespace Rake2D.Script;

public class OnAnimEndExit : ExitStateBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnAnimEnd += OnAnimEnd;
	}

	protected void OnAnimEnd()
	{
		StartExit();
	}
}
