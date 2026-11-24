using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public abstract class CreateObjectBehavior : BehaviorScript
{
	protected int _zorder;

	protected int _layer;

	protected int _countToDestroy;

	protected int _createCount;

	protected bool _useParentScale;

	protected FloatRange _rotationRange;

	protected FloatRange _rotationSpeedRange;

	protected Vector2Range _scaleRange;

	protected bool _scaleXY;

	protected Vector2 _offsetPivot;

	protected Vector2 _offsetSize;

	protected Vector2Range _velocityRange;

	protected IntRange _countRange;

	protected FloatRange _speedRange;

	protected float _parentVelocityFactor;

	protected bool _parentArc;

	protected float _scrollFactor;

	protected bool _parentAngle;

	protected Color _color;

	protected int _damage;

	protected bool _useDamageFactor;

	protected ColorMode _colorMode;

	public override void Init()
	{
		base.Init();
		_layer = ParamInt("layer", 1);
		_zorder = ParamInt("zorder", MyObject.ZOrder);
		_countToDestroy = ParamInt("destroy", int.MaxValue);
		_useParentScale = ParamBool("parent_scale");
		_rotationRange = ParamRange("angle") * ((float)Math.PI / 180f);
		_rotationSpeedRange = ParamRange("rotation_speed", 0f, 0f);
		_scaleRange = ParamVector2Range("scale_xy", 0f, 0f) / 100f;
		if (_scaleRange.IsZero)
		{
			_scaleRange = ParamVector2Range("scale", 100f, 100f) / 100f;
			_scaleXY = false;
		}
		else
		{
			_scaleXY = true;
		}
		_countRange = ParamIntRange("count", 1, 1);
		_velocityRange = ParamVector2Range("velocity");
		_speedRange = ParamRange("speed", 0f, 0f);
		_parentVelocityFactor = Param("parent_velocity") / 100f;
		_scrollFactor = Param("scroll_factor") / 100f;
		_parentAngle = ParamBool("parent_angle", defaultValue: true);
		_parentArc = ParamBool("parent_arc", _parentArc);
		_color = ParamColor("color", Color.TransparentBlack);
		_damage = ParamInt("damage", -999);
		_useDamageFactor = ParamBool("use_damage_factor");
		_colorMode = DataUtils.GetColorModeByString(ParamString("color_mode"));
		float[] array = ParamArray("offset");
		if (array.IsNullOrEmpty())
		{
			_offsetPivot = Vector2.Zero;
			_offsetSize = Vector2.Zero;
			return;
		}
		if (array.Length == 1)
		{
			_offsetSize = new Vector2(array[0], array[0]);
			_offsetPivot = Vector2.Zero;
			return;
		}
		_offsetSize = new Vector2(array[0], array[1]);
		if (array.Length == 3)
		{
			_offsetPivot = new Vector2(array[2], array[2]);
		}
		else if (array.Length > 3)
		{
			_offsetPivot = new Vector2(array[2], array[3]);
		}
		else
		{
			_offsetPivot = Vector2.Zero;
		}
	}

	protected virtual GameObject CreateObject(GameObject targetObject, Vector2 offset, Layer targetLayer)
	{
		GameObject gameObject = null;
		if (targetObject == null)
		{
			targetObject = ((_layer != 10) ? MyObject : MyObject.GetMainPart());
		}
		string text = ParamRandomString("object_id");
		if (Utils.Equals(text, "none"))
		{
			return null;
		}
		PartData partData = GameCore.GetPartData(text);
		if (partData == null)
		{
			return null;
		}
		if (_layer == 0)
		{
			gameObject = targetObject.CreateChildByPartData(partData, createPhysBody: true);
			gameObject.SetPosition(offset);
		}
		else if (_layer == 10)
		{
			GameObject childByIdFlat = targetObject.GetChildByIdFlat(text);
			if (childByIdFlat != null)
			{
				targetObject.RemoveChildImediately(childByIdFlat);
			}
			gameObject = targetObject.CreateChildByPartData(partData, createPhysBody: true);
			gameObject.SetPosition(offset);
		}
		else
		{
			Layer layer;
			Vector2 vector;
			switch ((_layer == -1) ? partData.DefaultLayerNumber : _layer)
			{
			case -3:
				layer = targetLayer ?? targetObject.MyLevel.FirstMainLayer;
				vector = targetObject.GetMainLayerPosition(targetObject.WorldPosition);
				break;
			case 2:
				layer = targetLayer ?? targetObject.MyLevel.MainLayer;
				vector = targetObject.GetMainLayerPosition(targetObject.WorldPosition);
				break;
			case 3:
				layer = targetLayer ?? targetObject.MyLevel.LastMainLayer;
				vector = targetObject.GetMainLayerPosition(targetObject.WorldPosition);
				break;
			default:
				layer = targetLayer ?? targetObject.MyLayer;
				vector = targetObject.WorldPosition;
				break;
			}
			gameObject = layer.CreateObjectByPartData(partData, vector + offset, _useParentScale ? targetObject.WorldScale : Vector2.One, targetObject.WorldRotation, targetObject.WorldZOrder + _zorder);
		}
		if (gameObject == null)
		{
			LogErr("ObjectCreator: Unable to create object '{0}'", partData.Id);
		}
		return gameObject;
	}

	protected List<GameObject> CreateAndReturnObjectList(GameObject targetObject, Vector2 offset, Layer layer = null, float rotation = -999f, Vector2? createVelocity = null)
	{
		List<GameObject> gameObjectList = GameCore.Allocator.GetGameObjectList();
		for (int num = _countRange.GetRandom(); num > 0; num--)
		{
			Vector2 vector = offset + _offsetPivot + new Vector2(Utils.GetRandom((0f - _offsetSize.X) / 2f, _offsetSize.X / 2f), Utils.GetRandom((0f - _offsetSize.Y) / 2f, _offsetSize.Y / 2f));
			if (_layer != 0)
			{
				if (targetObject != null)
				{
					vector = Vector2.Transform(vector, Matrix.CreateRotationZ(targetObject.WorldRotation));
				}
				else if (_parentArc)
				{
					vector = Vector2.Transform(vector, Matrix.CreateRotationZ(MyObject.WorldRotation));
				}
			}
			GameObject gameObject = CreateObject(targetObject, vector, layer);
			if (gameObject != null)
			{
				if (_color != Color.TransparentBlack)
				{
					gameObject.SetColor(_color);
					if (_color.A == 0)
					{
						gameObject.Alpha = MyObject.Alpha;
					}
				}
				else
				{
					switch (_colorMode)
					{
					case ColorMode.ParentAlpha:
						gameObject.Alpha = MyObject.Alpha;
						break;
					case ColorMode.ParentRGB:
						gameObject.SetColor(MyObject.Color, 255);
						break;
					case ColorMode.ParentRGBA:
						gameObject.SetColor(MyObject.Color);
						break;
					}
				}
				if (rotation == -999f)
				{
					rotation = ((gameObject.PartData.BehaviorRotation != -999f) ? gameObject.PartData.BehaviorRotation : ((!_parentAngle) ? _rotationRange.GetRandom() : (gameObject.Rotation + _rotationRange.GetRandom())));
				}
				if (_layer != 0 && gameObject.Scale.X < 0f)
				{
					rotation *= -1f;
				}
				gameObject.SetRotation(rotation);
				if (_scaleXY)
				{
					gameObject.SetScale(gameObject.Scale * new Vector2(Utils.GetRandom(_scaleRange.FromX, _scaleRange.FromY)));
				}
				else
				{
					gameObject.SetScale(gameObject.Scale * _scaleRange.GetRandom());
				}
				Vector2 random = _velocityRange.GetRandom();
				random += MyLevel.ScrollSpeed * _scrollFactor;
				if (createVelocity.HasValue)
				{
					random += createVelocity.Value;
				}
				if (_parentVelocityFactor != 0f)
				{
					if (_scrollFactor != 0f)
					{
						random += (targetObject ?? MyObject).VelocityNoScroll * _parentVelocityFactor;
					}
					else
					{
						random += (targetObject ?? MyObject).Velocity * _parentVelocityFactor;
					}
				}
				if (!_speedRange.IsZero)
				{
					random += Utils.GetVelocityVector2(_speedRange.GetRandom(), gameObject.WorldRotation);
				}
				if (_damage >= 0)
				{
					gameObject.Damage = _damage;
				}
				else if (_damage != -999)
				{
					gameObject.Damage = MyObject.Damage * (float)_damage / -100f;
				}
				if (gameObject.IsMainPart)
				{
					gameObject.Init();
					gameObject.AwakePhysics(resetDynamics: true);
					gameObject.ParentThrowVelocity = MyObject.Velocity * _parentVelocityFactor;
					gameObject.SetVelocity(random);
					gameObject.SetRotationSpeedInDeg(_rotationSpeedRange.GetRandom());
					gameObject.LevelInit();
					gameObject.Activate();
				}
				else
				{
					gameObject.Update(0f);
				}
				if (_useDamageFactor)
				{
					gameObject.Damage *= FactorManager.DamageFactor;
				}
				MyObject.GetMainPart().MyHero?.ProcessObject(gameObject);
				gameObjectList.Add(gameObject);
			}
		}
		if (++_createCount >= _countToDestroy)
		{
			MyObject.Destroy();
		}
		return gameObjectList;
	}

	protected void CreateObjectList(GameObject targetObject, Vector2 offset, Layer layer = null, float rotation = -999f, Vector2? createVelocity = null)
	{
		List<GameObject> l = CreateAndReturnObjectList(targetObject, offset, layer, rotation, createVelocity);
		GameCore.Allocator.DeleteAliasingGameObjectList(l);
	}
}
