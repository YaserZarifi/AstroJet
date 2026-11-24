namespace Rake2D.Script;

public class FlipByVelocityX : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
	}

	private void Update(float elapsedTime)
	{
		if ((MyObject.Scale.X > 0f && MyObject.Velocity.X > 0f) || (MyObject.Scale.X < 0f && MyObject.Velocity.X < 0f))
		{
			MyObject.SetScale(MyObject.Scale.X * -1f, MyObject.Scale.Y);
		}
	}
}
