namespace Rake2D.Script;

public class DamageByMainPart : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.Damage = MyObject.GetMainPart().Damage;
	}
}
