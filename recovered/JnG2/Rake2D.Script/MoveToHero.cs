using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class MoveToHero : BehaviorScript
{
	private int _speedMode;

	private float _startRadius;

	private FloatRange _stopRadiusRange;

	private float _speedUp;

	private float _slowDown;

	private float _maxSpeed;

	private float _currSpeed;

	private bool _keepDistance;

	private int _jetPackMode;

	private PartState _moveState;

	private PartState _stayState;

	private Vector2 _targetOffset;

	private bool _compensateScroll;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		_currSpeed = MyObject.Velocity.Length();
	}

	public override void Init()
	{
		base.Init();
		_startRadius = Param("start_radius", 100f);
		_stopRadiusRange = ParamRange("stop_radius");
		_maxSpeed = Param("max_speed", 500f);
		_speedUp = Param("speed_up", 1000f);
		_slowDown = Param("slow_down", _speedUp);
		_keepDistance = ParamBool("keep_distance");
		_targetOffset = ParamRandomVector2Range("offset");
		_jetPackMode = ParamInt("jet_pack_mode");
		_compensateScroll = ParamBool("compensate_scroll");
		string text = ParamString("move_state");
		if (text != null)
		{
			_moveState = MyObject.PartData.FindPartState(text);
		}
		text = ParamString("stay_state");
		if (text != null)
		{
			_stayState = MyObject.PartData.FindPartState(text);
		}
	}

	private void OnUpdate(float elapsedTime)
	{
		GameObject gameObject = MyObject.FindNearestHeroObject(_jetPackMode);
		if (gameObject == null)
		{
			return;
		}
		Vector2 vector = (_compensateScroll ? MyLevel.ScrollSpeed : Vector2.Zero);
		Vector2 vector2 = ((gameObject != gameObject.MyHero.MyObject) ? Vector2.Zero : (_targetOffset * gameObject.Scale));
		float distance = Utils.GetDistance(MyObject.WorldPosition, gameObject.WorldPosition);
		if (_keepDistance)
		{
			if (distance < _stopRadiusRange.From)
			{
				_speedMode = 1;
				Utils.ChangeValueToTarget(ref _currSpeed, _maxSpeed * -1f, _speedUp * elapsedTime);
			}
			else if (distance < _stopRadiusRange.To)
			{
				_speedMode = -1;
				Utils.ChangeValueToTarget(ref _currSpeed, 0f, _slowDown * elapsedTime);
			}
			else if (distance < _startRadius)
			{
				_speedMode = 1;
				Utils.ChangeValueToTarget(ref _currSpeed, _maxSpeed, _speedUp * elapsedTime);
			}
			MyObject.SetRotationVelocity(_currSpeed, Utils.GetRotationTo(MyObject.WorldPosition, gameObject.WorldPosition + vector2), vector);
			return;
		}
		if (_speedMode == 1 && (distance > _startRadius || distance < _stopRadiusRange.To))
		{
			_speedMode = -1;
		}
		else if (_speedMode != 1 && distance <= _startRadius)
		{
			_speedMode = 1;
			if (_moveState != null)
			{
				MyObject.ChangePartState(_moveState);
			}
		}
		if (_speedMode == 1)
		{
			Utils.ChangeValueToTarget(ref _currSpeed, _maxSpeed, _speedUp * elapsedTime);
			MyObject.SetRotationVelocity(_currSpeed, Utils.GetRotationTo(MyObject.WorldPosition, gameObject.WorldPosition + vector2), vector);
		}
		else
		{
			if (_speedMode != -1)
			{
				return;
			}
			if (Utils.ChangeValueToTarget(ref _currSpeed, 0f, _slowDown * elapsedTime))
			{
				_speedMode = 0;
				MyObject.SetVelocity(vector);
				if (_stayState != null)
				{
					MyObject.ChangePartState(_stayState);
				}
			}
			else
			{
				MyObject.SetRotationVelocity(_currSpeed, Utils.GetRotationTo(MyObject.WorldPosition, gameObject.WorldPosition + _targetOffset), vector);
			}
		}
	}
}
