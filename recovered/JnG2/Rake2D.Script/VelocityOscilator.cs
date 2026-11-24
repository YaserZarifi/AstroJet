using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class VelocityOscilator : BehaviorScript
{
	private Vector2 _max_velocity;

	private Vector2 _startAngle;

	private Vector2 _radians;

	private Vector2 _changeSpeed;

	private Vector2 _startPosition;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		if (_doInEditor)
		{
			MyObject.OnEditorUpdate += OnUpdate;
		}
		_startPosition = MyObject.WorldPosition;
		_radians = new Vector2(MathHelper.ToRadians(_startAngle.X), MathHelper.ToRadians(_startAngle.Y));
	}

	public override void Init()
	{
		base.Init();
		_max_velocity = ParamVector2("max_velocity", 100f);
		_startAngle = ParamVector2("start_angle");
		_changeSpeed = ParamVector2("change_speed", 100f);
		_changeSpeed = new Vector2(MathHelper.ToRadians(_changeSpeed.X), MathHelper.ToRadians(_changeSpeed.Y));
	}

	public void OnUpdate(float elapsedTime)
	{
		MyObject.SetVelocity(new Vector2(Utils.Sin(_radians.X), 0f - Utils.Cos(_radians.Y)) * _max_velocity);
		_radians += _changeSpeed * elapsedTime;
	}
}
