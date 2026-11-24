namespace Rake2D.Script;

public class OnKillSetTargetPart : SetTargetPartBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	private void OnKill()
	{
		SetTargetPart();
		MyObject.OnKill -= OnKill;
		IsDone = true;
	}
}
