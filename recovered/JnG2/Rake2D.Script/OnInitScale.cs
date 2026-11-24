namespace Rake2D.Script;

public class OnInitScale : BehaviorScript
{
	public override void RegisterEvents()
	{
	}

	public override void Init()
	{
		base.Init();
		MyObject.SetScale(ParamRandomVector2Range("scale") / 100f);
		IsDone = true;
	}
}
