namespace Rake2D.Script;

public class OnKillExit : ExitStateBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	public void OnKill()
	{
		MyObject.OnKill -= OnKill;
		StartExit();
	}
}
