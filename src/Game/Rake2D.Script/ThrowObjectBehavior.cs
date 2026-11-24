using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public abstract class ThrowObjectBehavior : ExitStateBehavior
{
	public delegate void OnGameObjectEventHandler(GameObject go);

	public OnGameObjectEventHandler OnThrow;

	private GameObject _throwPart;

	private IntRange _countRange;

	private FloatRange _scaleRange;

	private FloatRange _arcRange;

	private FloatRange _angleRange;

	private FloatRange _speedRange;

	private FloatRange _lifeTimeRange;

	private FloatRange _startRadiusRange;

	private FloatRange _rotationSpeedRange;

	private FloatRange _beginDelayRange;

	private IntRange _loopCountRange;

	private FloatRange _loopDelayRange;

	private float _loopTimer;

	private float _scrollFactor;

	private int _loopCount;

	private int _zorder;

	private int _layer;

	private Vector2 _startImpulse;

	private Vector2Range _offsetRange;

	private Vector2 _acceleration;

	private bool _doCCWCorrection;

	private bool _parentAngle;

	private bool _parentArc;

	private bool _parentOffset;

	private bool _parentScale;

	private bool _scaleSpeed;

	private int _fxRectMode;

	private int _spreadMode;

	private float _parentVelocityFactor;

	protected int _damage;

	protected bool _useDamageFactor;

	public override void Init()
	{
		base.Init();
		_countRange = ParamIntRange("count", 1);
		_loopCountRange = ParamIntRange("loop", 1);
		_loopDelayRange = ParamRange("loop_delay", 100f) / 1000f;
		_beginDelayRange = ParamRange("begin_delay") / 1000f;
		_scaleRange = ParamRange("scale", 100f, 100f) / 100f;
		_arcRange = ParamRange("arc");
		_parentAngle = ParamBool("parent_angle", defaultValue: true);
		_parentArc = ParamBool("parent_arc", defaultValue: true);
		_parentOffset = ParamBool("parent_offset", defaultValue: true);
		_parentScale = ParamBool("parent_scale", defaultValue: true);
		_parentVelocityFactor = Param("parent_velocity") / 100f;
		_scaleSpeed = ParamBool("scale_speed", defaultValue: true);
		_scrollFactor = Param("scroll_factor") / 100f;
		_angleRange = ParamRange("angle", -999f);
		_speedRange = ParamRange("speed");
		_spreadMode = ParamInt("spread");
		_lifeTimeRange = ParamRange("life_time") / 1000f;
		_startRadiusRange = ParamRange("start_radius");
		_rotationSpeedRange = ParamRange("rotation_speed");
		_acceleration = ParamVector2("acceleration");
		_doCCWCorrection = ParamBool("ccw_correction", defaultValue: true);
		_damage = ParamInt("damage", -999);
		_useDamageFactor = ParamBool("use_damage_factor");
		_layer = ParamInt("layer", 1);
		_zorder = ParamInt("zorder");
		_startImpulse = ParamVector2("start_impulse");
		_fxRectMode = ParamInt("fx_rect");
		if (_fxRectMode == 1)
		{
			Rectangle fxRectangle = MyObject.FxRectangle;
			_offsetRange = new Vector2Range(fxRectangle.Left, fxRectangle.Top, fxRectangle.Right, fxRectangle.Bottom);
		}
		else if (_fxRectMode > 1)
		{
			Rectangle fxRectangle2 = MyObject.GetMainPart().FxRectangle;
			_offsetRange = new Vector2Range(fxRectangle2.Left, fxRectangle2.Top, fxRectangle2.Right, fxRectangle2.Bottom);
		}
		else
		{
			_offsetRange = ParamVector2Range("offset");
		}
	}

	protected void StartThrow(GameObject throwPart = null)
	{
		_throwPart = throwPart ?? MyObject;
		_loopCount = _loopCountRange.GetRandom();
		_loopTimer = _beginDelayRange.GetRandom();
		if (_loopCount == 1 && _loopTimer <= 0f)
		{
			Throw();
			StartExit();
		}
		else if (_loopCount >= 1)
		{
			MyObject.OnUpdate += UpdateLoop;
			if (_doInEditor)
			{
				MyObject.OnEditorUpdate += UpdateLoop;
			}
		}
	}

	protected void UpdateLoop(float elapsedTime)
	{
		_loopTimer -= elapsedTime;
		if (_loopTimer <= 0f)
		{
			Throw();
			_loopCount--;
			if (_loopCount <= 0)
			{
				MyObject.OnUpdate -= UpdateLoop;
				MyObject.OnEditorUpdate -= UpdateLoop;
				StartExit();
			}
			else
			{
				_loopTimer = _loopDelayRange.GetRandom();
			}
		}
	}

	private void Throw()
	{
		int random = _countRange.GetRandom();
		float num;
		Vector2 vector;
		if (random > 1)
		{
			num = (_arcRange.To - _arcRange.From) / (float)(random - 1);
			vector = new Vector2((_offsetRange.ToX - _offsetRange.FromX) / (float)(random - 1), (_offsetRange.ToY - _offsetRange.FromY) / (float)(random - 1));
		}
		else
		{
			num = 0f;
			vector = Vector2.Zero;
		}
		for (int i = 0; i < random; i++)
		{
			float num2;
			Vector2 vector2;
			switch (_spreadMode)
			{
			case 1:
				num2 = Utils.DegToRad(_arcRange.From + (float)i * num);
				vector2 = new Vector2(_offsetRange.FromX + (float)i * vector.X, _offsetRange.FromY + (float)i * vector.Y);
				break;
			case 2:
				if (i == 0)
				{
					num2 = Utils.DegToRad(_arcRange.From + Utils.GetRandom(0f, num / 2f));
					vector2 = new Vector2(_offsetRange.FromX + Utils.GetRandom(0f, vector.X / 2f), _offsetRange.FromY + Utils.GetRandom(0f, vector.Y / 2f));
				}
				else if (i == random - 1)
				{
					num2 = Utils.DegToRad(_arcRange.From + (float)i * num + Utils.GetRandom((0f - num) / 2f, 0f));
					vector2 = new Vector2(_offsetRange.FromX + (float)i * vector.X + Utils.GetRandom((0f - vector.X) / 2f, 0f), _offsetRange.FromY + (float)i * vector.Y + Utils.GetRandom((0f - vector.Y) / 2f, 0f));
				}
				else
				{
					num2 = Utils.DegToRad(_arcRange.From + (float)i * num + Utils.GetRandom((0f - num) / 2f, num / 2f));
					vector2 = new Vector2(_offsetRange.FromX + (float)i * vector.X + Utils.GetRandom((0f - vector.X) / 2f, vector.X / 2f), _offsetRange.FromY + (float)i * vector.Y + Utils.GetRandom((0f - vector.Y) / 2f, vector.Y / 2f));
				}
				break;
			default:
				num2 = Utils.DegToRad(_arcRange.GetRandom());
				vector2 = _offsetRange.GetRandom();
				break;
			}
			if (_parentArc)
			{
				num2 += _throwPart.WorldRotation;
			}
			Vector2 velocityVector = Utils.GetVelocityVector2(_startRadiusRange.GetRandom(), num2);
			if (_parentOffset)
			{
				velocityVector += Utils.RotateVector2(vector2 * _throwPart.WorldScale, _throwPart.WorldRotation);
			}
			else
			{
				velocityVector += vector2;
			}
			PartData partData = GameCore.GetPartData(ParamRandomString("object_id"));
			if (partData == null)
			{
				LogErr("Unable to create object - part data not found.");
				break;
			}
			int num3 = ((_layer == -1) ? partData.DefaultLayerNumber : _layer);
			float num4;
			if (partData.BehaviorRotation != -999f)
			{
				num4 = partData.BehaviorRotation;
			}
			else if (_angleRange.From == -999f)
			{
				num4 = num2;
			}
			else
			{
				num4 = Utils.DegToRad(_angleRange.GetRandom());
				if (_parentAngle && num3 != 0)
				{
					num4 += _throwPart.WorldRotation;
				}
			}
			Vector2 scale = new Vector2(_scaleRange.GetRandom());
			if (_parentScale && num3 != 0)
			{
				scale *= _throwPart.WorldScale;
			}
			GameObject gameObject;
			if (num3 == 0)
			{
				gameObject = _throwPart.CreateChildByPartData(partData, createPhysBody: true);
				gameObject.SetPosition(velocityVector);
				gameObject.SetScale(scale);
				gameObject.SetRotation(num4);
			}
			else
			{
				Layer layer;
				Vector2 vector3;
				switch (num3)
				{
				case -3:
					layer = _throwPart.MyLevel.FirstMainLayer;
					vector3 = _throwPart.GetMainLayerPosition(_throwPart.WorldPosition);
					break;
				case 2:
					layer = _throwPart.MyLevel.MainLayer;
					vector3 = _throwPart.GetMainLayerPosition(_throwPart.WorldPosition);
					break;
				case 3:
					layer = _throwPart.MyLevel.LastMainLayer;
					vector3 = _throwPart.GetMainLayerPosition(_throwPart.WorldPosition);
					break;
				default:
					layer = MyLayer;
					vector3 = _throwPart.WorldPosition;
					break;
				}
				gameObject = layer.CreateObjectByPartData(partData, vector3 + velocityVector, scale, num4, _throwPart.WorldZOrder + _zorder);
			}
			if (gameObject != null)
			{
				MyObject.GetMainPart().MyHero?.ProcessObject(gameObject);
				if (_damage >= 0)
				{
					gameObject.Damage = _damage;
				}
				else if (_damage != -999)
				{
					gameObject.Damage = MyObject.Damage * (float)_damage / -100f;
				}
				if (_useDamageFactor)
				{
					gameObject.Damage *= FactorManager.DamageFactor;
				}
				gameObject.Alpha = MyObject.Alpha;
				if (_throwPart.ParentThrowVelocity != Vector2.Zero)
				{
					gameObject.ParentThrowVelocity = _throwPart.ParentThrowVelocity * _parentVelocityFactor;
				}
				else
				{
					gameObject.ParentThrowVelocity = _throwPart.Velocity * _parentVelocityFactor;
				}
				if (MyObject.WorldScale.X < 0f)
				{
					num2 *= -1f;
				}
				Vector2 velocityVector2 = Utils.GetVelocityVector2(_speedRange.GetRandom(), num2);
				if (_scrollFactor != 0f)
				{
					velocityVector2 += _throwPart.VelocityNoScroll * _parentVelocityFactor + MyLevel.ScrollSpeed * _scrollFactor;
				}
				else
				{
					velocityVector2 += _throwPart.Velocity * _parentVelocityFactor;
				}
				if (_scaleSpeed)
				{
					gameObject.SetVelocity(_throwPart.WorldScale.X * (velocityVector2.X + _startImpulse.X), _throwPart.WorldScale.Y * (velocityVector2.Y + _startImpulse.Y));
				}
				else
				{
					gameObject.SetVelocity(velocityVector2.X + _startImpulse.X, velocityVector2.Y + _startImpulse.Y);
				}
				gameObject.SetAcceleration(_acceleration * _throwPart.WorldScale);
				if (!_rotationSpeedRange.IsZero)
				{
					if (_doCCWCorrection)
					{
						gameObject.SetRotationSpeedInDeg(_rotationSpeedRange.GetRandom() * (float)((!(gameObject.VelocityX < 0f)) ? 1 : (-1)));
					}
					else
					{
						gameObject.SetRotationSpeedInDeg(_rotationSpeedRange.GetRandom());
					}
				}
				if (!_lifeTimeRange.IsZero)
				{
					gameObject.LifeTime = _lifeTimeRange.GetRandom();
				}
				if (MyObject.MyItem != null && MyObject.MyItem.GetBehaviorByType(typeof(ItemWeapon)) is ItemWeapon itemWeapon)
				{
					gameObject.OnReRegisterEvents += itemWeapon.OnReRegisterEvents;
				}
				gameObject.Init();
				gameObject.LevelInit();
				gameObject.Activate();
				OnThrow?.Invoke(gameObject);
			}
			else
			{
				LogErr("Unable to create object '{0}'.", partData.Id);
			}
		}
	}
}
