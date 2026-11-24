namespace Rake2D.Script;

public class OnKillQuake : BehaviorQuakeBase
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += Quake;
	}
}
