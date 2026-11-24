namespace Rake2D.Script;

public class OnKillHitClass : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	public void OnKill()
	{
		MyObject.PhysClass = DataUtils.GetPhysClassByString(ParamString("hit_class"));
	}
}
