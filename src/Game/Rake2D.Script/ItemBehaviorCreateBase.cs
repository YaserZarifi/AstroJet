using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public abstract class ItemBehaviorCreateBase : ItemBehaviorScript
{
	protected int _zorder;

	protected int _createMode;

	protected float _lifeTime;

	protected int _probability;

	protected FloatRange _rotationRange;

	protected float _damage;

	protected FloatRange _scaleRange;

	private Vector2Range _offsetRange;

	protected FloatRange _speedRange;

	protected PartData _objectData;

	protected bool _isRandomObject;

	protected bool _turnOffsetByHeroPosition;

	protected bool _useParentRotation;

	protected bool _useParentScale;

	public override void Init()
	{
		_createMode = ParamInt("mode", 1);
		_zorder = ParamInt("zorder");
		_lifeTime = (float)ParamInt("life_time") / 1000f;
		_rotationRange = ParamRange("angle") * ((float)Math.PI / 180f);
		_useParentRotation = ParamBool("parent_angle");
		_useParentScale = ParamBool("parent_scale");
		_scaleRange = ParamRange("scale", 100f, 100f) / 100f;
		_speedRange = ParamRange("speed", 0f, 0f);
		_offsetRange = ParamVector2Range("offset");
		_damage = Param("damage", -1f);
		_turnOffsetByHeroPosition = ParamBool("offset_by_hero");
		string[] array = ParamStringArray("object_id");
		if (!array.IsNullOrEmpty())
		{
			if (array.Length == 1)
			{
				_objectData = GameCore.GetPartData(ParamRandomString("object_id"));
				_isRandomObject = false;
			}
			else
			{
				_isRandomObject = true;
			}
		}
	}

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		_probability = ParamInt("probability", 100);
	}

	public virtual GameObject CreateObject(Vector2 position, GameObject parentObject = null)
	{
		GameObject gameObject = null;
		if (_probability <= Utils.GetRandom(0, 99))
		{
			return null;
		}
		if (_isRandomObject)
		{
			_objectData = GameCore.GetPartData(ParamRandomString("object_id"));
		}
		if (_objectData == null)
		{
			return null;
		}
		float random = _scaleRange.GetRandom();
		if (_createMode == 0)
		{
			if (parentObject != null)
			{
				gameObject = parentObject.CreateChildByPartData(_objectData, createPhysBody: true);
				gameObject.SetScale(random / parentObject.WorldScale.X, random / parentObject.WorldScale.Y);
				gameObject.SetPosition(parentObject.GetRelativePosition(position + _offsetRange.GetRandom()));
			}
			else
			{
				gameObject = MyObject.CreateChildByPartData(_objectData, createPhysBody: true);
				gameObject.SetScale(random, random);
				gameObject.SetPosition(_offsetRange.GetRandom());
			}
		}
		else if (_createMode == 1 && parentObject != null)
		{
			gameObject = parentObject.MyLayer.CreateObjectByPartData(_objectData, parentObject.WorldPosition + _offsetRange.GetRandom(), _useParentScale ? (parentObject.WorldScale * random) : new Vector2(random), _useParentRotation ? (parentObject.WorldRotation + _rotationRange.GetRandom()) : _rotationRange.GetRandom(), parentObject.ZOrder + _zorder);
		}
		else
		{
			float num = _rotationRange.GetRandom();
			if (_useParentRotation && parentObject != null)
			{
				num += parentObject.WorldRotation;
			}
			Vector2 pos = position;
			if (_turnOffsetByHeroPosition)
			{
				pos += Utils.RotateVector2(_offsetRange.GetRandom(), Utils.GetRotationTo(position, MyHero.MyObject.WorldPosition));
			}
			else
			{
				pos += _offsetRange.GetRandom();
			}
			gameObject = MyObject.MyLayer.CreateObjectByPartData(_objectData, pos, new Vector2(random), num, _zorder);
		}
		if (gameObject != null)
		{
			gameObject.AwakePhysics(resetDynamics: true);
			if (_damage >= 0f)
			{
				gameObject.Damage = _damage;
			}
			if (!_speedRange.IsZero)
			{
				Vector2 velocity = ((!_turnOffsetByHeroPosition) ? Utils.GetVelocityVector2(_speedRange.GetRandom(), gameObject.WorldRotation) : Utils.GetVelocityVector2(_speedRange.GetRandom(), gameObject.WorldRotation + Utils.GetRotationTo(position, MyHero.MyObject.WorldPosition)));
				gameObject.SetVelocity(velocity);
			}
		}
		else
		{
			LogErr("ItemBehaviorObjectCreator: Unable to create oobject '{0}'", _objectData.Id);
		}
		return gameObject;
	}
}
