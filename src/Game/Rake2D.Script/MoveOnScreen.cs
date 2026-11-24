using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class MoveOnScreen : ExitStateBehavior
{
	private float _currSpeed;

	private float _maxSpeed;

	private float _acceleration;

	private int _xyMode = 3;

	private Vector2 _targetScreenPos;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		_currSpeed = MyObject.Velocity.Length();
	}

	public override void Init()
	{
		base.Init();
		_maxSpeed = Param("max_speed", 500f);
		_acceleration = Param("speed_up", 500f);
		_currSpeed = _maxSpeed;
		_targetScreenPos = ParamRandomVector2Range("screen_pos");
		_xyMode = ParamInt("xy_mode", 3);
	}

	private void OnUpdate(float elapsedTime)
	{
		Vector2 virtualScreenPosition = MyObject.VirtualScreenPosition;
		if (_xyMode == 1)
		{
			_targetScreenPos.Y = virtualScreenPosition.Y;
		}
		if (_xyMode == 2)
		{
			_targetScreenPos.X = virtualScreenPosition.X;
		}
		float num = Vector2.Distance(virtualScreenPosition, _targetScreenPos);
		float num2;
		if (num < 100f)
		{
			num2 = num / 100f;
		}
		else
		{
			num2 = 1f;
			Utils.ChangeValueToTarget(ref _currSpeed, _maxSpeed, _acceleration * elapsedTime);
		}
		float rotationTo = Utils.GetRotationTo(virtualScreenPosition, _targetScreenPos);
		Vector2 velocity = Utils.GetVelocityVector2(_currSpeed, rotationTo) * num2;
		velocity += MyLevel.ScrollSpeed * MyLayer.ScrollFactor;
		MyObject.SetVelocity(velocity);
		if (num < 1f)
		{
			MyObject.SetVelocity(Vector2.Zero);
			StartExit();
		}
	}
}
