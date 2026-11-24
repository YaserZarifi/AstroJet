using Microsoft.Xna.Framework;

namespace Rake2D;

public class Motion
{
	public Vector2 Velocity;

	public Vector2 Acceleration;

	public float RotationSpeed;

	public float Damping;

	public Motion Clone()
	{
		return new Motion
		{
			Velocity = Velocity,
			RotationSpeed = RotationSpeed,
			Damping = Damping
		};
	}

	internal void Update(float elapsedTime)
	{
		Velocity += Acceleration * elapsedTime;
		if (Damping != 0f)
		{
			Utils.ChangeValueToTarget(ref Velocity, Vector2.Zero, Damping * elapsedTime);
		}
	}
}
