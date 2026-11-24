namespace Rake2D.Script.HeroDrone;

public class DroneBehavior : BehaviorScript
{
	protected float _hitTimer;

	protected float _hitDelay;

	protected bool _rotateWithHero;

	protected PartState _initState;

	public override void Init()
	{
		base.Init();
		_rotateWithHero = ParamBool("hero_rotation");
		_initState = MyObject.CurrentState;
	}

	public override void RegisterEvents()
	{
		if (MyObject.MyHero != null && MyObject.MyHero.IsBuzzsawEnabled)
		{
			MyObject.GetChildByIdDeep("buzzsaw").ChangePartState("active");
		}
	}

	protected virtual void Update(float elapsedTime)
	{
		if (!IsHeroInLevelAutopilotMode)
		{
			if (MyObject.VirtualScreenPosition.X < -50f)
			{
				MyObject.Kill();
			}
			else if (MyObject.WorldPosition.X > MyLevel.Camera.Position.X + 1920f)
			{
				MyObject.SetPosition(MyLevel.Camera.Position.X + 1920f, MyObject.WorldPosition.Y);
			}
		}
	}
}
