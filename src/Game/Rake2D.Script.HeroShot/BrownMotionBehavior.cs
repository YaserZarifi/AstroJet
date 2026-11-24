namespace Rake2D.Script.HeroShot;

public class BrownMotionBehavior : BehaviorScript
{
	private float moveTimer;

	private bool bFirstEmitt;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
	}

	public override void Init()
	{
		moveTimer = Utils.GetRandom(0.3f, 0.5f);
		if (MyObject.Emitter != null)
		{
			MyObject.Emitter.Emit();
		}
	}

	public void Update(float elapsedTime)
	{
		if (!bFirstEmitt && MyObject.Timer >= 0.05f && MyObject.Emitter != null)
		{
			MyObject.Emitter.Emit();
			bFirstEmitt = true;
		}
		moveTimer -= elapsedTime;
		if (moveTimer <= 0f)
		{
			if (MyObject.Emitter != null)
			{
				MyObject.Emitter.Emit();
			}
			moveTimer = Utils.GetRandom(0.05f, 0.15f);
			MyObject.SetVelocity(Utils.GetRandom(200, 1300, bRandonNegative: true), Utils.GetRandom(200, 1300, bRandonNegative: true));
		}
	}
}
