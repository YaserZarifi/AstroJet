namespace Rake2D.Script;

public class AsBomb : BehaviorScript
{
	public override void RegisterEvents()
	{
	}

	public override void Init()
	{
		if (MyObject.MyHero != null)
		{
			MyObject.MyHero.DoOnBombStart(MyObject);
		}
	}
}
