using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class BehaviorBeamPearcingBase : BehaviorScript
{
	protected float _hitTimer;

	protected float _widthHalf;

	protected float _hitDelay;

	protected PhysClass _hitTo;

	protected GameObject _lightPart;

	protected Particle _startParticle;

	protected Particle _midParticle;

	protected Particle _endParticle;

	protected bool _isHitReady;

	protected bool _isLifeTimeHit;

	protected int _currHitCount;

	protected int _wantedHitCount;

	protected float _hitDamage;

	protected GameObject _lastChanceMainPart;

	protected byte _startupAlpha;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		MyObject.OnEditorUpdate += OnUpdate;
		MyObject.OnDestroy += OnDestroy;
		MyObject.OnDisable += OnDestroy;
		MyObject.OnDeactivate += OnDestroy;
		_currHitCount = 0;
		_hitTimer = 0f;
		_isHitReady = true;
		if (MyObject.LifeTime < 9999f)
		{
			_wantedHitCount = (int)(MyObject.LifeTime / _hitDelay) + 1;
			_hitDamage = MyObject.Damage / (float)_wantedHitCount;
			_isLifeTimeHit = true;
		}
		else
		{
			_wantedHitCount = 0;
			_hitDamage = MyObject.Damage * _hitDelay;
			_isLifeTimeHit = false;
		}
	}

	public override void Init()
	{
		if (MyObject.Emitter != null)
		{
			MyObject.Emitter.IsEmitedByBehavior = true;
		}
		_widthHalf = Param("width", 10f) / 2f * MyObject.Scale.X;
		_hitDelay = Param("hit_delay", 1000f) / 1000f;
		_lightPart = MyObject.GetChildByIdDeep("beam_light");
	}

	protected void OnDestroy()
	{
		_startParticle?.Destroy();
		_endParticle?.Destroy();
		_midParticle?.Destroy();
	}

	protected float GetRotation()
	{
		return Utils.GetRotationTo(_startParticle.WorldPosition, _endParticle.WorldPosition);
	}

	protected virtual void OnUpdate(float elapsedTime)
	{
		if (!MyObject.IsVisible || MyObject.IsDestroyed)
		{
			OnDestroy();
			return;
		}
		if (MyObject.Emitter != null)
		{
			if (MyObject.Emitter.StripLine == null || MyObject.Emitter.StripLine.ParticleList.Count < 2)
			{
				MyObject.Emitter.Reset();
				MyObject.Emitter.Emit(bIgnoreLoopCount: true);
				MyObject.Emitter.StripLine.ParentTransformation = null;
				_startParticle = MyObject.Emitter.StripLine.ParticleList[0];
				_endParticle = MyObject.Emitter.StripLine.ParticleList[MyObject.Emitter.StripLine.ParticleList.Count - 1];
				if (MyObject.Emitter.StripLine.ParticleList.Count > 2)
				{
					_midParticle = MyObject.Emitter.StripLine.ParticleList[MyObject.Emitter.StripLine.ParticleList.Count - 2];
				}
				_startupAlpha = _startParticle.Color.A;
			}
		}
		else
		{
			_startParticle = new Particle();
			_endParticle = new Particle();
		}
		_startParticle.Position = MyObject.WorldPosition;
		Color spriteColor = _endParticle.SpriteColor;
		_startParticle.SpriteColor = new Color(spriteColor.R, spriteColor.G, spriteColor.B, (byte)((float)(_startupAlpha * MyObject.Color.A) / 255f));
		if (_midParticle != null)
		{
			_midParticle.SpriteColor = _startParticle.SpriteColor;
			_endParticle.SpriteColor = new Color(_startParticle.SpriteColor, 0);
		}
		else
		{
			_endParticle.SpriteColor = _startParticle.SpriteColor;
		}
		_endParticle.Position = MyObject.WorldPosition + Utils.GetVelocityVector2(2000f, MyObject.WorldRotation);
		if (_endParticle.Position.X > MyLevel.Camera.ViewBottomRightInWorld.X + _widthHalf)
		{
			if (_startParticle.Position.X < MyLevel.Camera.ViewBottomRightInWorld.X + _widthHalf)
			{
				float num = MyLevel.Camera.ViewBottomRightInWorld.X + _widthHalf;
				_endParticle.Position.Y = _endParticle.Position.Y - (_endParticle.Position.Y - _startParticle.Position.Y) * (_endParticle.Position.X - num) / (_endParticle.Position.X - _startParticle.Position.X);
				_endParticle.Position.X = num;
			}
			else
			{
				_endParticle.Position = _startParticle.Position;
			}
		}
		else if (_endParticle.Position.X < MyLevel.Camera.ViewTopLeftInWorld.X - _widthHalf)
		{
			if (_startParticle.Position.X > MyLevel.Camera.ViewTopLeftInWorld.X - _widthHalf)
			{
				float num2 = MyLevel.Camera.ViewTopLeftInWorld.X - _widthHalf;
				_endParticle.Position.Y = _endParticle.Position.Y - (_endParticle.Position.Y - _startParticle.Position.Y) * (_endParticle.Position.X - num2) / (_endParticle.Position.X - _startParticle.Position.X);
				_endParticle.Position.X = num2;
			}
			else
			{
				_endParticle.Position = _startParticle.Position;
			}
		}
		if (_endParticle.Position.Y > MyLevel.Size.Y + _widthHalf)
		{
			if (_startParticle.Position.Y < MyLevel.Size.Y + _widthHalf)
			{
				float num3 = MyLevel.Size.Y + _widthHalf;
				_endParticle.Position.X = _endParticle.Position.X - (_endParticle.Position.X - _startParticle.Position.X) * (_endParticle.Position.Y - num3) / (_endParticle.Position.Y - _startParticle.Position.Y);
				_endParticle.Position.Y = num3;
			}
			else
			{
				_endParticle.Position = _startParticle.Position;
			}
		}
		else if (_endParticle.Position.Y < 0f - _widthHalf)
		{
			if (_startParticle.Position.Y > 0f - _widthHalf)
			{
				float num4 = 0f - _widthHalf;
				_endParticle.Position.X = _endParticle.Position.X - (_endParticle.Position.X - _startParticle.Position.X) * (_endParticle.Position.Y - num4) / (_endParticle.Position.Y - _startParticle.Position.Y);
				_endParticle.Position.Y = num4;
			}
			else
			{
				_endParticle.Position = _startParticle.Position;
			}
		}
		if (_midParticle != null)
		{
			_midParticle.Position = _endParticle.Position - Utils.GetVelocityVector2(50f, Utils.GetRotationTo(_startParticle.WorldPosition, _endParticle.WorldPosition));
		}
		if (_lightPart != null)
		{
			float num5 = (_endParticle.WorldPosition - _startParticle.WorldPosition).Length();
			_lightPart.SetPosition(0f, (0f - num5) / 2f / MyObject.WorldScale.Y);
			_lightPart.SetScale(_lightPart.Scale.X, num5 / (float)_lightPart.FrameRectangle.Height / MyObject.WorldScale.Y);
		}
		int num6;
		if (_isLifeTimeHit)
		{
			num6 = _wantedHitCount - _currHitCount;
			if (num6 > 0)
			{
				_hitTimer -= elapsedTime;
				if (_hitTimer <= 0f)
				{
					_currHitCount++;
					_hitTimer = MyObject.LifeTime / (float)num6;
					_isHitReady = true;
				}
			}
		}
		else
		{
			num6 = 1;
			_hitTimer -= elapsedTime;
			if (_hitTimer <= 0f)
			{
				_hitTimer = _hitDelay;
				_isHitReady = true;
			}
		}
		if (!_isHitReady)
		{
			return;
		}
		List<RayCastResult> list = new List<RayCastResult>();
		for (float num7 = 0f - _widthHalf; num7 < _widthHalf; num7 += Math.Min(_widthHalf, 10f))
		{
			foreach (RayCastResult r in MyObject.MyLevel.GetRayCastResultList(Utils.GetParallelPoint(_startParticle.Position, MyObject.WorldRotation, num7), Utils.GetParallelPoint(_endParticle.Position, MyObject.WorldRotation, num7), _hitTo))
			{
				if (list.Where((RayCastResult rr) => rr.TargetObject == r.TargetObject).Count() == 0)
				{
					list.Add(r);
				}
			}
		}
		foreach (RayCastResult item in list)
		{
			GetRotation();
			GameObject targetObject = item.TargetObject;
			float num8 = _hitDamage;
			_isHitReady = false;
			if (MyObject.LifeTime <= elapsedTime)
			{
				num8 *= (float)num6;
			}
			if (targetObject.HitOnCollision && !targetObject.IsImmortal)
			{
				targetObject.Hit(num8, MyObject.DamageType, MyObject);
			}
			if (targetObject.IsAlive)
			{
				targetObject.DoOnCollision(MyObject, item.CollisionPosition, item.CollisionNormal);
			}
		}
	}
}
