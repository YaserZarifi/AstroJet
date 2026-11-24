using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class PathOscilator : BehaviorScript
{
	private float _amplitudeY;

	private float _angle;

	private Vector2 _scrollCompensation;

	private float _scrollFactor;

	private Vector2 _changeSpeed;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
	}

	public override void Init()
	{
		_amplitudeY = ParamRandomRange("amplitude", 10f);
		_angle = Utils.DegToRad(ParamRandomRange("start_angle", 0f));
		_scrollFactor = Param("scroll_factor") / 100f;
		_changeSpeed = new Vector2(0f, 30f);
	}

	public void Update(float elapsedTime)
	{
		_angle += _changeSpeed.Y * elapsedTime;
		if (_scrollFactor == 0f)
		{
			MyObject.PathOscilation = Utils.GetVelocityVector2(Utils.Sin(_angle) * _amplitudeY, MyObject.GetPathRotation() - Utils.DegToRad(90f));
			return;
		}
		_scrollCompensation += MyLevel.ScrollSpeed * _scrollFactor * elapsedTime;
		MyObject.PathOscilation = Utils.GetVelocityVector2(Utils.Sin(_angle) * _amplitudeY, MyObject.GetPathRotation() - Utils.DegToRad(90f)) + _scrollCompensation;
	}
}
