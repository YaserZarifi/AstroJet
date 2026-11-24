using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class BehaviorDataDrone
{
	public DroneTypeEnum DroneType;

	public int Placement;

	public float MaxSpeed;

	public float Acceleration;

	public Vector2 OffsetToHero;

	public BehaviorDataDrone(DroneTypeEnum droneType)
	{
		DroneType = droneType;
	}
}
