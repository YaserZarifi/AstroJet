using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnCollisionThrow : ThrowObjectBehavior
{
	private float _minSpeed;

	private float _repeatDelay;

	private float _lastTime;

	private int _bounceCount;

	private int _maxBounceCount;

	private Vector2 _impactVelocity;

	public override void RegisterEvents()
	{
		if (_bounceCount <= _maxBounceCount)
		{
			MyObject.OnCollision += OnCollision;
			if (_minSpeed > 0f)
			{
				MyObject.OnBeforeCollision += OnBeforeCollision;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		_minSpeed = Math.Max(0f, Param("min_speed"));
		_repeatDelay = Param("repeat_delay") / 1000f;
		_maxBounceCount = ParamInt("max_bounce", 999);
		_lastTime = -99999f;
	}

	protected void OnBeforeCollision(GameObject go)
	{
		_impactVelocity = MyObject.Velocity;
	}

	protected void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		_bounceCount++;
		if (_bounceCount > _maxBounceCount)
		{
			MyObject.OnCollision -= OnCollision;
			MyObject.OnBeforeCollision -= OnBeforeCollision;
		}
		else if (MyObject.IsActivated && !(_impactVelocity.Length() < _minSpeed) && !(MyObject.Timer - _lastTime < _repeatDelay))
		{
			StartThrow();
			_lastTime = MyObject.Timer;
		}
	}
}
