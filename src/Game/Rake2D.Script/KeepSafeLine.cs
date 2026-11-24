namespace Rake2D.Script;

public class KeepSafeLine : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
	}

	private void OnUpdate(float elapsedTime)
	{
		MyObject.SetPosition(MyLevel.GetNearestSafePosition(MyObject.MyCamera.Position.X + 960f));
	}
}
