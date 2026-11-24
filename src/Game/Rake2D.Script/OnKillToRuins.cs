namespace Rake2D.Script;

public class OnKillToRuins : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	public void OnKill()
	{
		MyObject.SetHitAndTargetClass(PhysClass.Ruins, TargetClass.None);
	}
}
