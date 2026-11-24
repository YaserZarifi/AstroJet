namespace Rake2D.Script;

public class OnKillFallAndThrow : FallAndThrowBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	private void OnKill()
	{
		FallAndThrow();
		MyObject.OnKill -= OnKill;
	}
}
