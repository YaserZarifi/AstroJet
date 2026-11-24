using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class BehaviorBeamBase : BehaviorScript
{
	protected float _hitTimer;

	protected float _chanceTimer;

	protected float _widthHalf;

	protected float _hitDelay;

	protected PhysClass _hitTo;

	protected GameObject _hitEffectObject;

	protected GameObject _lightPart;

	protected Particle _startParticle;

	protected Particle _midParticle;

	protected Particle _endParticle;

	protected GameObject _targetObject;

	protected Vector2 _collisionNormal;

	protected bool _isHitReady;

	protected bool _isChanceReady;

	protected bool _isPenetrationReady;

	protected bool _isLifeTimeHit;

	protected int _currHitCount;

	protected int _wantedHitCount;

	protected float _hitDamage;

	protected float _chanceFactor;

	protected GameObject _lastChanceMainPart;

	protected byte _startupAlpha;

	protected GemableItemBehavior _weaponBeh;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		MyObject.OnEditorUpdate += OnUpdate;
		MyObject.OnDestroy += OnDestroy;
		MyObject.OnDisable += OnDestroy;
		MyObject.OnDeactivate += OnDestroy;
		_currHitCount = 0;
		_hitTimer = 0f;
		_chanceTimer = 0f;
		_isHitReady = true;
		_isPenetrationReady = true;
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
		if (MyObject.MyItem != null)
		{
			_weaponBeh = (GemableItemBehavior)MyObject.MyItem.GetBehaviorByType(typeof(GemableItemBehavior));
			_chanceFactor = 1f / (1f / _hitDelay * MyObject.LifeTime);
		}
		else
		{
			_weaponBeh = null;
			_chanceFactor = 1f;
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
		_hitEffectObject?.Destroy();
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
		_targetObject = null;
		RayCastResult rayCastResult = GetRayCastResult();
		_targetObject = rayCastResult.TargetObject;
		_collisionNormal = rayCastResult.CollisionNormal;
		_endParticle.Position = rayCastResult.CollisionPosition;
		if (_endParticle.Position.X > MyLevel.Camera.ViewBottomRightInWorld.X + _widthHalf)
		{
			if (_startParticle.Position.X < MyLevel.Camera.ViewBottomRightInWorld.X + _widthHalf)
			{
				float num = MyLevel.Camera.ViewBottomRightInWorld.X + _widthHalf;
				_endParticle.Position.Y = _endParticle.Position.Y - (_endParticle.Position.Y - _startParticle.Position.Y) * (_endParticle.Position.X - num) / (_endParticle.Position.X - _startParticle.Position.X);
				_endParticle.Position.X = num;
				_targetObject = null;
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
				_targetObject = null;
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
				_targetObject = null;
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
				_targetObject = null;
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
		if (Utils.ChangeValueToTarget(ref _chanceTimer, 0f, elapsedTime) || (_targetObject != null && _lastChanceMainPart != _targetObject.GetMainPart()))
		{
			_chanceTimer = 0.2f;
			_lastChanceMainPart = _targetObject?.GetMainPart();
			_isChanceReady = true;
		}
		if (_targetObject != null)
		{
			float rotation = GetRotation();
			if (_hitEffectObject == null || _hitEffectObject.IsDestroyed)
			{
				_hitEffectObject = MyObject.MyLayer.CreateObjectByPartData(ParamRandomString("hit_object_id"), _endParticle.WorldPosition, MyObject.WorldScale, rotation, 100);
			}
			else
			{
				_hitEffectObject.IsVisible = true;
				_hitEffectObject.SetPosition(_endParticle.WorldPosition);
				_hitEffectObject.SetRotation(rotation);
				if (_hitEffectObject.LifeTime < 0.05f)
				{
					_hitEffectObject.LifeTime = 0.05f;
				}
			}
			if (!_isHitReady && (_targetObject.TargetClass != TargetClass.Man || !_isPenetrationReady))
			{
				return;
			}
			float num7 = _hitDamage;
			if (_isHitReady)
			{
				_isHitReady = false;
				if (MyObject.LifeTime <= elapsedTime)
				{
					num7 *= (float)num6;
				}
				if (MyObject.MyItem != null && _isChanceReady)
				{
					MyObject.MyItem.DoOnHitCollision(MyObject, _targetObject, rayCastResult.CollisionPosition, _collisionNormal, num7);
					_isChanceReady = false;
				}
				_isPenetrationReady = true;
			}
			else
			{
				_isPenetrationReady = false;
			}
			if ((_targetObject.HitOnCollision || _targetObject.IsHero || (_targetObject.PhysClass & PhysClass.JetPack) != 0) && !_targetObject.IsImmortal)
			{
				_targetObject.Hit(num7, MyObject.DamageType, MyObject);
			}
			if (!_targetObject.IsKilled && !_targetObject.IsDestroyed)
			{
				_targetObject.DoOnCollision(MyObject, rayCastResult.CollisionPosition, _collisionNormal);
			}
		}
		else if (_hitEffectObject != null)
		{
			_hitEffectObject.Destroy();
			_hitEffectObject = null;
		}
	}

	protected RayCastResult GetRayCastResult()
	{
		float maxLength = 2000f;
		RayCastResult rayCastResult = MyObject.MyLevel.GetRayCastResult(MyObject.WorldPosition, MyObject.WorldRotation, maxLength, _hitTo);
		RayCastResult rayCastResult2 = rayCastResult;
		float num = rayCastResult2.Length;
		RayCastResult rayCastResult3 = MyObject.MyLevel.GetRayCastResult(Utils.GetParallelPoint(MyObject.WorldPosition, MyObject.WorldRotation, _widthHalf), MyObject.WorldRotation, maxLength, _hitTo);
		float length = rayCastResult3.Length;
		if (length < num)
		{
			num = length;
			rayCastResult2 = rayCastResult3;
		}
		rayCastResult3 = MyObject.MyLevel.GetRayCastResult(Utils.GetParallelPoint(MyObject.WorldPosition, MyObject.WorldRotation, 0f - _widthHalf), MyObject.WorldRotation, maxLength, _hitTo);
		length = rayCastResult3.Length;
		if (length < num)
		{
			num = length;
			rayCastResult2 = rayCastResult3;
		}
		if (rayCastResult2 != rayCastResult)
		{
			rayCastResult.CollisionPosition = rayCastResult.StartPoint + Utils.GetVelocityVector2(num, MyObject.WorldRotation);
			rayCastResult.TargetObject = rayCastResult2.TargetObject;
		}
		return rayCastResult;
	}
}
