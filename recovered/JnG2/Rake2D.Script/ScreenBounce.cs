using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class ScreenBounce : BehaviorScript
{
	protected float _bounceRadius;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
	}

	public override void Init()
	{
		base.Init();
		_bounceRadius = Param("bounce_radius", 10f);
	}

	protected void OnUpdate(float elapsedTime)
	{
		if (MyObject.WorldPosition.X >= MyObject.MyCamera.ViewBottomRightInWorld.X - _bounceRadius)
		{
			MyObject.SetPosition(MyObject.MyCamera.ViewBottomRightInWorld.X - _bounceRadius, MyObject.WorldPosition.Y);
			if (MyObject.Velocity.X > 0f)
			{
				MyObject.SetVelocity(MyObject.Velocity.X * -1f, MyObject.Velocity.Y);
				Bounce(new Vector2(-1f, 0f));
			}
		}
		else if (MyObject.WorldPosition.X <= MyObject.MyCamera.ViewTopLeftInWorld.X - _bounceRadius)
		{
			MyObject.SetPosition(MyObject.MyCamera.ViewTopLeftInWorld.X - _bounceRadius, MyObject.WorldPosition.Y);
			if (MyObject.Velocity.X < 0f)
			{
				MyObject.SetVelocity(MyObject.Velocity.X * -1f, MyObject.Velocity.Y);
				Bounce(new Vector2(1f, 0f));
			}
		}
		else if (MyObject.WorldPosition.Y <= MyObject.MyCamera.ViewTopLeftInWorld.Y + _bounceRadius)
		{
			MyObject.SetPosition(MyObject.WorldPosition.X, MyObject.MyCamera.ViewTopLeftInWorld.Y + _bounceRadius);
			if (MyObject.Velocity.Y < 0f)
			{
				MyObject.SetVelocity(MyObject.Velocity.X, MyObject.Velocity.Y * -1f);
				Bounce(new Vector2(0f, 1f));
			}
		}
		else if (MyObject.WorldPosition.Y >= MyObject.MyCamera.ViewBottomRightInWorld.Y - _bounceRadius)
		{
			MyObject.SetPosition(MyObject.WorldPosition.X, MyObject.MyCamera.ViewBottomRightInWorld.Y - _bounceRadius);
			if (MyObject.Velocity.Y > 0f)
			{
				MyObject.SetVelocity(MyObject.Velocity.X, MyObject.Velocity.Y * -1f);
				Bounce(new Vector2(0f, -1f));
			}
		}
	}

	protected void Bounce(Vector2 normal)
	{
		MyObject.DoOnCollision(null, MyObject.WorldPosition, normal);
	}
}
