namespace Rake2D.Script;

public class BehaviorQuakeBase : BehaviorScript
{
	public override void RegisterEvents()
	{
	}

	public void Quake()
	{
		MyLevel.Quake = Param("quake", 100f);
	}
}
