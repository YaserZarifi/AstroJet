namespace Rake2D.Script;

public class OnKillThrow : ThrowObjectBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	private void OnKill()
	{
		StartThrow();
	}
}
