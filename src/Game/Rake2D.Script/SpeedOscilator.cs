using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class SpeedOscilator : BehaviorScript
{
	private Vector2 _startVelocity;

	private float _startRotation;

	private float _startOffset;

	private float _amplitude;

	private float _changeSpeed;

	private bool _isInitialized;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		if (_doInEditor)
		{
			MyObject.OnEditorUpdate += OnUpdate;
		}
	}

	public override void Init()
	{
		base.Init();
		_amplitude = Param("amplitude", 100f);
		_changeSpeed = Param("change_speed", 200f);
		_startOffset = Utils.DegToRad(Param("start_offset"));
	}

	public void OnUpdate(float elapsedTime)
	{
		if (!_isInitialized)
		{
			_startVelocity = MyObject.Velocity;
			_startRotation = Utils.GetRotationFromZero(MyObject.Velocity) + Utils.DegToRad(-90f);
			_isInitialized = true;
		}
		_startOffset += elapsedTime * _changeSpeed;
		MyObject.SetVelocity(_startVelocity + Utils.GetVelocityVector2(Utils.Sin(_startOffset) * _amplitude, _startRotation));
		MyObject.SetRotation(Utils.GetRotationFromZero(MyObject.Velocity));
	}
}
