using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class GunLaser : BehaviorScript
{
	private float _alpha;

	private float _taretAlpha;

	private float _hitTimer;

	private float _widthHalf;

	private PhysClass _hitTo;

	private GameObject hitObject;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
	}

	public override void Init()
	{
		if (MyObject.Emitter == null)
		{
			LogErr("No emitter for Laser gun.");
		}
		else if (MyObject.Emitter.StripLine == null)
		{
			LogErr("No strip line emitter.");
		}
		else
		{
			MyObject.Emitter.Reset();
			MyObject.Emitter.StripLine.ParentTransformation = null;
		}
		_widthHalf = Param("width", 10f) / 2f;
		_hitTo = (PhysClass)1061897;
		_alpha = 160f;
	}

	public void Update(float elapsedTime)
	{
		if (MyObject.Emitter.StripLine.ParticleList == null || MyObject.Emitter.StripLine.ParticleList.Count < 2)
		{
			MyObject.Emitter.Emit(bIgnoreLoopCount: true);
		}
		Utils.ChangeValueToTarget(ref _alpha, _taretAlpha, 1200f * elapsedTime);
		if (_alpha >= 250f)
		{
			_taretAlpha = 0f;
		}
		else if (_alpha <= 160f)
		{
			_taretAlpha = 255f;
		}
		Particle particle = MyObject.Emitter.StripLine.ParticleList[0];
		Particle particle2 = MyObject.Emitter.StripLine.ParticleList[1];
		particle.Position = MyObject.WorldPosition;
		particle.Alpha = (byte)_alpha;
		GameObject gameObject = null;
		RayCastResult rayCastResult = GetRayCastResult();
		gameObject = rayCastResult.TargetObject;
		particle2.Position = rayCastResult.CollisionPosition;
		particle2.Alpha = (byte)_alpha;
		_hitTimer -= elapsedTime;
		if (_hitTimer <= 0f && gameObject != null)
		{
			_hitTimer = 0.1f;
		}
		float rotationTo = Utils.GetRotationTo(particle.WorldPosition, particle2.WorldPosition);
		if (hitObject == null || hitObject.IsDestroyed)
		{
			hitObject = MyObject.MyLayer.CreateObjectByPartData(ParamRandomString("hit_object_id"), particle2.WorldPosition, Vector2.One, rotationTo, 100);
			return;
		}
		hitObject.SetPosition(particle2.WorldPosition);
		hitObject.SetRotation(rotationTo);
		hitObject.LifeTime = 0.05f;
	}

	private RayCastResult GetRayCastResult()
	{
		float maxLength = 2000f;
		RayCastResult rayCastResult = MyLevel.GetRayCastResult(MyObject.WorldPosition, MyObject.WorldRotation, maxLength, _hitTo);
		RayCastResult rayCastResult2 = rayCastResult;
		float num = rayCastResult2.Length;
		RayCastResult rayCastResult3 = MyLevel.GetRayCastResult(Utils.GetParallelPoint(MyObject.WorldPosition, MyObject.WorldRotation, _widthHalf), MyObject.WorldRotation, maxLength, _hitTo);
		float length = rayCastResult3.Length;
		if (length < num)
		{
			num = length;
			rayCastResult2 = rayCastResult3;
		}
		rayCastResult3 = MyLevel.GetRayCastResult(Utils.GetParallelPoint(MyObject.WorldPosition, MyObject.WorldRotation, 0f - _widthHalf), MyObject.WorldRotation, maxLength, _hitTo);
		length = rayCastResult3.Length;
		if (length < num)
		{
			num = length;
			rayCastResult2 = rayCastResult3;
		}
		if (rayCastResult2 != rayCastResult)
		{
			rayCastResult.CollisionPosition = rayCastResult.StartPoint + Utils.GetVelocityVector2(num, MyObject.WorldRotation);
			rayCastResult.TargetObject = rayCastResult3.TargetObject;
		}
		return rayCastResult;
	}
}
