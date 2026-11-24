namespace Rake2D.Script;

public class OnInitVelocity : BehaviorScript
{
	public override void RegisterEvents()
	{
	}

	public override void Init()
	{
		base.Init();
		MyObject.SetVelocity(ParamVector2("velocity"));
		IsDone = true;
	}
}
