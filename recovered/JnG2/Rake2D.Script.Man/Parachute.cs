using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script.Man;

public class Parachute : BehaviorManParachuteBase
{
	private float _currRotation;

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyObject.OnUpdate += OnUpdate;
		MyObject.OnCollision += OnCollision;
		_currRotation = MyObject.WorldRotation;
	}

	public override void Init()
	{
		base.Init();
		SetDirectionByFlip();
		_switchOffset = new Vector2(0f, 70f);
	}

	protected void OnUpdate(float elapsedTime)
	{
		if (!_isInitialized)
		{
			_isInitialized = true;
		}
		if (Utils.ChangeValueToTarget(ref _currRotation, 0f, 2f * elapsedTime))
		{
			_stateTimer += elapsedTime;
			MyObject.SetRotation((float)(Math.Sin(_stateTimer * 5f) * 3.1415927410125732 / 30.0));
		}
		else
		{
			MyObject.SetRotation(_currRotation);
		}
		if (MyObject.VelocityY == 0f)
		{
			MyObject.SwitchToObject("man", bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Destroy);
			DropParachute();
		}
	}

	protected void DropParachute()
	{
		GameObject gameObject = MyObject.MyLayer.CreateObjectByPartData("man_parachute_drop", MyObject.WorldPosition, MyObject.WorldScale, MyObject.Rotation, MyObject.ZOrder - 1);
		gameObject.SetVelocity(MyObject.Velocity.X * 0.8f, Math.Min(90f, MyObject.VelocityY));
		gameObject.RotationSpeed = Utils.GetRandom(-0.5f, 0.5f);
	}

	protected void OnCollision(GameObject shotObject, GameObject targetObject, Vector2 contactPoint, Vector2 normal)
	{
		float rotationFromZero = Utils.GetRotationFromZero(normal);
		if (rotationFromZero > -1f && rotationFromZero < 1f)
		{
			MyObject.SwitchToObject("man", bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Destroy);
			DropParachute();
		}
	}

	protected override void KillMe(Vector2 physForce)
	{
		base.KillMe(physForce);
		DropParachute();
	}
}
