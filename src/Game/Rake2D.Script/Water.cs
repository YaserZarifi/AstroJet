using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class Water : BehaviorScript
{
	private PartData _splashDataBig;

	private PartData _splashDataSmall;

	private PartData _bubblesData;

	private string[] _splasSoundArrayBig;

	private float _surfaceY;

	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
		MyObject.OnSeparation += OnSeparation;
	}

	public override void Init()
	{
		base.Init();
		if (MyLevel.ScrollSpeed.X >= 500f)
		{
			_splashDataBig = GameCore.GetPartData("water_splash_fast");
		}
		else
		{
			_splashDataBig = GameCore.GetPartData("water_splash_big");
		}
		_splashDataSmall = GameCore.GetPartData("water_splash_small");
		_bubblesData = GameCore.GetPartData("water_bubble_impact");
		_splasSoundArrayBig = new string[2] { "water_splash_big_0", "water_splash_big_1" };
		_surfaceY = MyObject.WorldPosition.Y - (float)(MyObject.FrameRectangle.Height / 2) * MyObject.WorldScale.Y;
	}

	public void OnSeparation(GameObject collisionObject)
	{
		collisionObject.SetEnvironment(EnvironmentEnum.Air);
	}

	public void OnCollision(GameObject myObject, GameObject colliObject, Vector2 contactPoint, Vector2 normal)
	{
		if (colliObject.Environment == EnvironmentEnum.Water)
		{
			return;
		}
		colliObject.SetEnvironment(EnvironmentEnum.Water);
		if (colliObject.Timer > 0.01f && colliObject.PartData.FxMass > 0f)
		{
			float num = Math.Min(1.3f, Math.Max(0.075f, colliObject.PartData.FxMass / 500f * colliObject.Scale.X));
			PartData partData = (((double)num < 0.2 || colliObject.RigidBody == null || colliObject.RigidBody.Mass < 10f) ? _splashDataSmall : _splashDataBig);
			GameObject gameObject = colliObject.MyLayer.CreateObjectByPartData(partData, new Vector2(colliObject.WorldPosition.X, _surfaceY + (float)Utils.GetRandom(-5, 5)), new Vector2(num, num * Utils.Clamp(colliObject.VelocityY / 500f, 0.9f, 1.2f)), 0f, MyObject.ZOrder);
			if (partData == _splashDataBig)
			{
				gameObject.PlaySound(Utils.GetRandomItem(_splasSoundArrayBig), (int)Utils.Clamp(100f * num, 30f, 100f));
			}
		}
		if (!colliObject.PartData.IgnoreWater)
		{
			if (colliObject.RigidBody != null && !colliObject.IgnoreWater)
			{
				colliObject.SetVelocity(colliObject.Velocity * 0.75f);
				colliObject.RigidBody.LinearDamping = 0.75f;
				colliObject.RigidBody.AngularDamping = colliObject.PartData.PhysAngularDamping + 0.75f;
				colliObject.RigidBody.GravityScale = 0.25f;
			}
			colliObject.CreateChildByPartData(_bubblesData, createPhysBody: true);
		}
	}
}
