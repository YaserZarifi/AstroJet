using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnStateVelocity : ExitStateBehavior
{
	private Vector2 _targetVelocity;

	private float _acceleration;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
	}

	public override void Init()
	{
		base.Init();
		_targetVelocity = ParamVector2("velocity");
		_acceleration = Param("acceleration", 1000f);
	}

	private void OnUpdate(float elapsedTime)
	{
		Vector2 currentValue = MyObject.Velocity;
		if (Utils.ChangeValueToTarget(ref currentValue, _targetVelocity * MyObject.Scale, _acceleration * elapsedTime))
		{
			StartExit();
		}
		MyObject.SetVelocity(currentValue);
	}
}
