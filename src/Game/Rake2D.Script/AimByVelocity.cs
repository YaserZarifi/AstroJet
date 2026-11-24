namespace Rake2D.Script;

public class AimByVelocity : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
	}

	public void Update(float elapsedTime)
	{
		MyObject.SetRotation(Utils.GetRotationFromZero(MyObject.Velocity - MyLevel.ScrollSpeed * MyLayer.ScrollFactor));
	}
}
