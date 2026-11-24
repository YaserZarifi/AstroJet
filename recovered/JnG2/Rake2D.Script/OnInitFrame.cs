namespace Rake2D.Script;

public class OnInitFrame : BehaviorScript
{
	public override void RegisterEvents()
	{
	}

	public override void Init()
	{
		base.Init();
		MyObject.SpriteRenderer.Frame = ParamRandomInt("frame");
		IsDone = true;
	}
}
