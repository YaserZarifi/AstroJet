namespace Rake2D.Script;

public class RandomFrame : BehaviorScript
{
	public override void RegisterEvents()
	{
	}

	public override void Init()
	{
		MyObject.Frame = ParamRandomInt("frame");
		IsDone = true;
	}
}
