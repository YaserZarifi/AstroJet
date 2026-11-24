using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnCollisionCreate : CreateObjectBehavior
{
	protected bool _createOnContact;

	protected int _normalMode;

	protected bool _createAsChildOfCollisionObject;

	protected float _minSpeed;

	protected float _repeatDelay;

	protected float _lastTime;

	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
	}

	public override void Init()
	{
		_createOnContact = ParamBool("on_contact");
		if (_createOnContact)
		{
			_parentArc = false;
		}
		_normalMode = ParamInt("normal");
		_createAsChildOfCollisionObject = ParamBool("coli_child");
		_minSpeed = Param("min_speed");
		_repeatDelay = Param("repeat_delay") / 1000f;
		_lastTime = -99999f;
		base.Init();
	}

	protected void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (!MyObject.IsActivated || MyObject.Timer - _lastTime < _repeatDelay || (_minSpeed != 0f && MyObject.Velocity.Length() < _minSpeed))
		{
			return;
		}
		GameObject gameObject = null;
		if (_createAsChildOfCollisionObject)
		{
			gameObject = collisionObject;
		}
		float rotation;
		switch (_normalMode)
		{
		case 1:
			rotation = Utils.GetRotationFromZero(normal);
			rotation += _rotationRange.GetRandom();
			break;
		case 2:
			rotation = Utils.GetRotationTo(MyObject.WorldPosition, contactPoint);
			break;
		case 3:
			rotation = Utils.GetRotationTo(MyObject.GetMainPart().WorldPosition, contactPoint);
			break;
		case 4:
			rotation = collisionObject.Rotation + (float)Math.PI;
			break;
		default:
			rotation = -999f;
			break;
		}
		if (_createOnContact && collisionObject != null)
		{
			Layer layer = ((_layer == 2) ? MyLevel.MainLayer : collisionObject.MyLayer);
			Vector2 vector = contactPoint - myObject.WorldPosition - collisionObject.MyCamera.Position * (Vector2.One - layer.ScrollFactor);
			if (_layer == 0)
			{
				gameObject = MyObject;
				vector = Vector2.Transform(vector, Matrix.CreateRotationZ(0f - gameObject.WorldRotation));
			}
			Vector2 value;
			if (collisionObject.IsHero || collisionObject.MyHero != null)
			{
				value = layer.ScrollSpeed;
			}
			else
			{
				value = ((!collisionObject.GetMainPart().IsTriggerFree) ? Vector2.Zero : collisionObject.Velocity);
			}
			List<GameObject> list = CreateAndReturnObjectList(gameObject, vector, layer, rotation, value);
			if (_layer == 0 && list != null && !_useParentScale)
			{
				gameObject = MyObject;
				foreach (GameObject item in list)
				{
					item.SetScale(item.Scale / gameObject.WorldScale);
				}
			}
			GameCore.Allocator.DeleteAliasingGameObjectList(list);
		}
		else if (collisionObject == null)
		{
			CreateObjectList(gameObject, Vector2.Zero, MyLayer, rotation);
		}
		else
		{
			CreateObjectList(gameObject, Vector2.Zero, (_layer == 2) ? null : collisionObject.MyLayer, rotation);
		}
		_lastTime = MyObject.Timer;
	}
}
