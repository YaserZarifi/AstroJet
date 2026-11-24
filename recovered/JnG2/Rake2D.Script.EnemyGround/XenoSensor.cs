using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script.EnemyGround;

public class XenoSensor : BehaviorScript
{
	private GameObject _drilledObject;

	private GameObject _hitPartDrill;

	private GameObject _drillPart;

	private GameObject _mainPart;

	private float _hitTimer;

	private bool _hitToHero;

	private TargetClass _targetClass;

	public override void Init()
	{
		base.Init();
		string[] array = ParamStringArray("target_class");
		if (array != null)
		{
			_targetClass = DataUtils.GetTargetClassByStringArray(array);
		}
		else
		{
			_targetClass = (TargetClass)147512;
		}
	}

	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
		MyObject.OnUpdate += OnUpdate;
		_mainPart = MyObject.GetMainPart();
		_hitPartDrill = _mainPart.GetChildByIdDeep("hit_part_drill");
		_drillPart = _mainPart.GetChildByIdDeep("drill");
	}

	protected void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (collisionObject.TargetClass == TargetClass.Man)
		{
			collisionObject.Hit(MyObject.Damage * FactorManager.DamageFactor, MyObject.DamageType, MyObject);
		}
		else if (_drilledObject == null && (collisionObject.PhysClass == PhysClass.Hero || (collisionObject.TargetClass & _targetClass) != 0))
		{
			_drilledObject = collisionObject;
			Vector2 position = _mainPart.WorldPosition - _drilledObject.WorldPosition;
			_mainPart.ParentObject = _drilledObject;
			Vector2 position2 = Vector2.Transform(position, Matrix.CreateRotationZ(0f - _drilledObject.Rotation) * Matrix.CreateScale(1f / _drilledObject.Scale.X, 1f / _drilledObject.Scale.Y, 1f));
			_mainPart.SetPosition(position2);
			_mainPart.SetRotation(_mainPart.Rotation - _drilledObject.Rotation);
			_mainPart.ChangePartState("drill");
			if (collisionObject.TargetClass == TargetClass.Hero || collisionObject.TargetClass == TargetClass.Drone)
			{
				_hitPartDrill.ChangePartState("drill_hero");
				_hitToHero = true;
			}
			else
			{
				_hitPartDrill.ChangePartState("drill_enemy");
				_hitToHero = false;
			}
			_drillPart?.ChangePartState("drill");
			_mainPart.SetVelocity(Vector2.Zero);
			MyObject.IgnoreCollisionWith(_drilledObject);
			_drilledObject.IgnoreCollisionWith(MyObject);
			_drilledObject.IgnoreCollisionWith(_mainPart);
		}
	}

	public void OnUpdate(float elapsedTime)
	{
		if (_drilledObject == null)
		{
			return;
		}
		if (_drilledObject.IsAlive)
		{
			float currentValue = _mainPart.Rotation;
			float rotationTo = Utils.GetRotationTo(_drilledObject.PartData.TargetOffset, _mainPart.Position);
			Utils.ChangeValueToTarget(ref currentValue, rotationTo, 10f * elapsedTime);
			_mainPart.SetRotation(currentValue);
			if (Utils.ChangeValueToTarget(ref _hitTimer, 0f, elapsedTime))
			{
				_hitTimer = 0.2f;
				if (_hitToHero)
				{
					_drilledObject.Hit(1f, MyObject.DamageType, MyObject);
				}
				else
				{
					_drilledObject.Hit(MyObject.Damage * FactorManager.DamageFactor * 0.2f, MyObject.DamageType, MyObject);
				}
			}
		}
		else
		{
			_drilledObject = null;
			Transformation worldTransformation = _mainPart.WorldTransformation;
			_mainPart.ParentObject = null;
			_mainPart.SetTransformation(worldTransformation);
			_mainPart.ChangePartState("fall");
			_drillPart?.ChangePartState("idle");
			_hitPartDrill.ChangePartState("idle");
		}
	}
}
