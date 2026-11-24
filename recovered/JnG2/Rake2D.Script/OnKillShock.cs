namespace Rake2D.Script;

public class OnKillShock : ShockBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	public void OnKill()
	{
		Shock(MyObject.WorldPosition, null);
	}
}
