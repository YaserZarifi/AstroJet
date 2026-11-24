using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class RotateToNormal : BehaviorScript
{
	private Vector2 _rayOffset;

	private int _rayLength;

	private float _rotationSpeed;

	public override void Init()
	{
		GameObject childByIdDeep = MyObject.GetChildByIdDeep("contact_point");
		_rayLength = ParamInt("contact_ray_length", -1);
		if (childByIdDeep != null)
		{
			_rayOffset = childByIdDeep.Position;
			if (_rayLength < 0)
			{
				_rayLength = (int)childByIdDeep.Scale.Y;
			}
		}
		else if (_rayLength < 0)
		{
			_rayLength = 5;
		}
		_rotationSpeed = Utils.DegToRad(Param("rotation_speed", 180f));
	}

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
	}

	public void OnUpdate(float elapsedTime)
	{
		Vector2 start = Vector2.Transform(_rayOffset, MyObject.WorldMatrix);
		RayCastResult rayCastResult = MyLevel.GetRayCastResult(start, (float)Math.PI, _rayLength, (PhysClass)5120);
		if (rayCastResult.TargetObject != null)
		{
			float currentValue = MyObject.WorldRotation;
			Utils.ChangeValueToTarget(ref currentValue, Utils.GetRotationFromZero(rayCastResult.CollisionNormal), _rotationSpeed * elapsedTime);
			MyObject.SetRotation(currentValue);
		}
	}
}
