namespace Rake2D.Script;

public class OnKillFind : FindBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	private void OnKill()
	{
		SetFind();
	}
}
