namespace Rake2D.Script;

public class OnKillAch : AchBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	private void OnKill()
	{
		UnlockAch();
	}
}
