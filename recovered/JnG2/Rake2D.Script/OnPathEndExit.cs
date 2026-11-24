namespace Rake2D.Script;

public class OnPathEndExit : ExitStateBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnPathEnd += OnPathEnd;
	}

	private void OnPathEnd()
	{
		MyObject.OnPathEnd -= OnPathEnd;
		StartExit();
	}
}
