using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script.HeroBomb;

public class BombArcaball : BehaviorScript
{
	private PartData _bounceEffectData;

	private PartData _hitEnemyData;

	private const int MAX_SPEED = 1600;

	private const int BOUNCE_RADIUS = 10;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		MyObject.OnEditorUpdate += OnUpdate;
		MyObject.RigidBody.SetCollisionCategories((PhysClass)34616443, PhysClass.ShotHero);
		MyObject.OnCollision += OnCollision;
		MyObject.RotationSpeed = 20f;
	}

	public override void Init()
	{
		base.Init();
		_bounceEffectData = GameCore.GetPartData("arcaball_bounce");
		_hitEnemyData = GameCore.GetPartData("explo_arcaball");
	}

	private void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (collisionObject.PartData == MyObject.PartData)
		{
			return;
		}
		if (collisionObject.TargetClass == TargetClass.ShotHero)
		{
			Vector2 vector = collisionObject.Velocity;
			if (vector != Vector2.Zero)
			{
				vector.Normalize();
			}
			else
			{
				vector = Utils.GetVelocityVector2(1f, Utils.GetRandom(0f, (float)Math.PI * 2f));
			}
			vector = Vector2.Transform(vector, Matrix.CreateRotationZ(Utils.GetRandom(-0.3f, 0.3f)));
			MyObject.SetVelocity(vector * 1600f);
			Bounce();
		}
		else
		{
			GameObject gameObject = collisionObject.MyLayer.CreateObjectByPartData(_hitEnemyData, contactPoint, Vector2.One, 0f);
			gameObject.Damage = MyObject.Damage;
			gameObject.MyHero = MyObject.MyHero;
		}
	}

	private void Bounce()
	{
		MyObject.CreateChildByPartData(_bounceEffectData, createPhysBody: false);
		MyLayer.CreateObjectByPartData(_bounceEffectData, MyObject.WorldPosition, Vector2.One, 0f);
	}

	private void OnUpdate(float elapsedTime)
	{
		if ((double)MyObject.Timer > 0.1 && MyObject.RigidBody != null)
		{
			MyObject.RigidBody.SetCollisionCategories(MyObject.RigidBody.HitTo, (PhysClass)192);
		}
		float num = MyObject.Velocity.Length();
		if (num == 0f)
		{
			MyObject.SetVelocity(Utils.GetVelocityVector2(1f, Utils.GetRandom(0f, (float)Math.PI * 2f)) * 1600f);
		}
		else if (num != 1600f)
		{
			MyObject.SetVelocity(Vector2.Normalize(MyObject.Velocity) * 1600f);
		}
		if (MyObject.WorldPosition.X >= MyObject.MyCamera.ViewBottomRightInWorld.X - 10f)
		{
			MyObject.SetPosition(MyObject.MyCamera.ViewBottomRightInWorld.X - 10f, MyObject.WorldPosition.Y);
			if (MyObject.Velocity.X > 0f)
			{
				MyObject.SetVelocity(MyObject.Velocity.X * -1f, MyObject.Velocity.Y);
				Bounce();
			}
		}
		else if (MyObject.WorldPosition.X <= MyObject.MyCamera.ViewTopLeftInWorld.X + 10f)
		{
			MyObject.SetPosition(MyObject.MyCamera.ViewTopLeftInWorld.X + 10f, MyObject.WorldPosition.Y);
			if (MyObject.Velocity.X < 0f)
			{
				MyObject.SetVelocity(MyObject.Velocity.X * -1f, MyObject.Velocity.Y);
				Bounce();
			}
		}
		else if (MyObject.WorldPosition.Y <= MyObject.MyCamera.ViewTopLeftInWorld.Y + 10f)
		{
			MyObject.SetPosition(MyObject.WorldPosition.X, MyObject.MyCamera.ViewTopLeftInWorld.Y + 10f);
			if (MyObject.Velocity.Y < 0f)
			{
				MyObject.SetVelocity(MyObject.Velocity.X, MyObject.Velocity.Y * -1f);
				Bounce();
			}
		}
		else if (MyObject.WorldPosition.Y >= MyObject.MyCamera.ViewBottomRightInWorld.Y - 10f)
		{
			MyObject.SetPosition(MyObject.WorldPosition.X, MyObject.MyCamera.ViewBottomRightInWorld.Y - 10f);
			if (MyObject.Velocity.Y > 0f)
			{
				MyObject.SetVelocity(MyObject.Velocity.X, MyObject.Velocity.Y * -1f);
				Bounce();
			}
		}
	}
}
