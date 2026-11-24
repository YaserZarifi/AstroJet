using Microsoft.Xna.Framework;

namespace Rake2D.JnG;

public class RemconConnection
{
	private static PartData _lockData;

	private static PartState _lockDoneState;

	public GameObject TargetObject;

	public GameObject LockObject;

	public Hero MyHero;

	public RemconStateEnum State;

	public float HackSpeed;

	public float HackPercent;

	protected Particle _startParticle;

	protected Particle _endParticle;

	public RemconConnection(Hero hero, GameObject targetObject)
	{
		MyHero = hero;
		TargetObject = targetObject;
		LockObject = TargetObject.MyLevel.LastMainLayer.CreateObjectByPartData(_lockData, TargetObject.GetTargetWorldPosition(), Vector2.One, 0f, TargetObject.ZOrder + 50, bCreateRigidBody: false);
		State = RemconStateEnum.Hacking;
		LockObject.Emitter.IsEmitedByBehavior = true;
		LockObject.Emitter.Reset();
	}

	public static void Init()
	{
		_lockData = GameCore.GetPartData("remcon_lock");
		if (_lockData != null)
		{
			_lockData.PartStates.TryGetValue("hack_done", out _lockDoneState);
		}
	}

	private void UpdateBeam()
	{
		if (LockObject.Emitter != null && (LockObject.Emitter.StripLine == null || LockObject.Emitter.StripLine.ParticleList == null || LockObject.Emitter.StripLine.ParticleList.Count < 2))
		{
			LockObject.Emitter.Emit(bIgnoreLoopCount: true);
			LockObject.Emitter.StripLine.ParentTransformation = null;
			_startParticle = LockObject.Emitter.StripLine.ParticleList[0];
			_endParticle = LockObject.Emitter.StripLine.ParticleList[1];
		}
		_startParticle.Position = LockObject.WorldPosition;
		_endParticle.Position = MyHero.MyObject.WorldPosition;
		switch (State)
		{
		case RemconStateEnum.Hacking:
			_startParticle.SpriteColor = new Color(0, 255, 0, 150);
			_startParticle.Scale.X = 0.9f;
			_endParticle.SpriteColor = new Color(100, 255, 100, 255);
			_endParticle.Scale.X = 1f;
			break;
		case RemconStateEnum.HackDone:
			_startParticle.SpriteColor = new Color(255, 255, 0, 100);
			_startParticle.Scale.X = 0.1f;
			_endParticle.SpriteColor = new Color(100, 255, 0, 150);
			_endParticle.Scale.X = 0.2f;
			break;
		}
	}

	public void Update(float elapsedTime)
	{
		if (LockObject != null)
		{
			LockObject.Position = TargetObject.GetTargetWorldPosition();
			UpdateBeam();
		}
		if (State != RemconStateEnum.Hacking)
		{
			_ = 1;
			return;
		}
		HackPercent += HackSpeed * elapsedTime * TargetObject.PartData.RemconHackSpeedFactor;
		if (HackPercent >= 1f)
		{
			HackPercent = 1f;
			State = RemconStateEnum.HackDone;
			TargetObject.DoOnRemcon(MyHero);
			LockObject.ChangePartState(_lockDoneState);
		}
	}

	public void Destroy()
	{
		if (State == RemconStateEnum.HackDone && TargetObject.IsRemconable)
		{
			TargetObject.DoOnRemconCancel(MyHero);
		}
		_startParticle?.Destroy();
		_endParticle?.Destroy();
		LockObject?.Destroy();
	}
}
