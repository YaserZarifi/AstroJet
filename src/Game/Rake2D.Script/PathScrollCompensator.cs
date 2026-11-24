namespace Rake2D.Script;

public class PathScrollCompensator : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
	}

	private void OnUpdate(float elapsedTime)
	{
		MyObject.PathOscilation += MyLevel.ScrollSpeed * elapsedTime;
	}
}
