namespace Rake2D.Script;

public class OnInitDamageFromMain : BehaviorScript
{
	public override void RegisterEvents()
	{
	}

	public override void Init()
	{
		base.Init();
		MyObject.Damage = MyObject.GetMainPart().Damage;
		IsDone = true;
	}
}
