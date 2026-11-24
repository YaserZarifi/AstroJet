namespace Rake2D.Script;

public class OnStateCreateFxArea : CreateFxAreaBehavior
{
	public override void RegisterEvents()
	{
		if (MyObject.IsVisible)
		{
			StartCreate();
		}
	}
}
