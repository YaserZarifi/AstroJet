using System;

namespace Rake2D.Script.Man;

public class Drown : BehaviorManBase
{
	private float _swimTimer;

	private PartData _splashData;

	private PartData _bubbleData;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		MyObject.OnEnvironmentChanged += OnEvironmentChanged;
	}

	public override void Init()
	{
		MyObject.AwakePhysics(resetDynamics: false);
		_splashData = GameCore.GetPartData("water_splash_man");
		_bubbleData = GameCore.GetPartData("water_bubble_man");
	}

	private void OnUpdate(float elapsedTime)
	{
		switch (MyObject.Environment)
		{
		case EnvironmentEnum.Water:
		{
			_swimTimer -= elapsedTime;
			if (_swimTimer <= 0f && MyObject.VelocityY > -150f)
			{
				MyObject.ApplyForce(0f, -400f);
			}
			float currentValue = MyObject.Velocity.X;
			Utils.ChangeValueToTarget(ref currentValue, 0f, elapsedTime * 200f);
			MyObject.SetVelocity(currentValue, MyObject.VelocityY);
			break;
		}
		case EnvironmentEnum.Air:
			MyObject.SetVelocity(MyObject.Velocity.X, Math.Max(20f, MyObject.VelocityY) + elapsedTime * 20f);
			break;
		}
	}

	private void OnEvironmentChanged(EnvironmentEnum env)
	{
		MyObject.CreateChildByPartData(_splashData, createPhysBody: true);
		if (env == EnvironmentEnum.Air)
		{
			if (Utils.GetRandom(20) == 0)
			{
				_swimTimer = Utils.GetRandom(0.5f, 1f);
				MyObject.CreateChildByPartData(_bubbleData, createPhysBody: true).Position.Y -= 20f;
			}
			else
			{
				_swimTimer = Utils.GetRandom(0.1f, 0.2f);
			}
		}
	}
}
