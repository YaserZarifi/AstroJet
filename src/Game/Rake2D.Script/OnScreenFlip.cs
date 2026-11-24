namespace Rake2D.Script;

public class OnScreenFlip : OnScreenBase
{
	private FloatRange _flipRange;

	private float _flipVelocity;

	public override void Init()
	{
		base.Init();
		_flipRange = ParamRange("flip", -1f, 1f);
		_flipVelocity = Param("flip_velocity") / 100f;
	}

	protected override void DoAction(int direction)
	{
		base.DoAction(direction);
		switch (direction)
		{
		case 1:
			if (MyObject.Velocity.X >= 0f && ((_flipRange.To < 0f && MyObject.Scale.X > 0f) || (_flipRange.To > 0f && MyObject.Scale.X < 0f)))
			{
				MyObject.SetScale(MyObject.Scale.X * -1f, MyObject.Scale.Y);
				if (_flipVelocity != 0f && MyObject.Velocity.X > 0f)
				{
					MyObject.SetVelocity(MyObject.Velocity.X * _flipVelocity, MyObject.Velocity.Y);
				}
			}
			break;
		case 3:
			if (MyObject.Velocity.X <= 0f && ((_flipRange.From < 0f && MyObject.Scale.X > 0f) || (_flipRange.From > 0f && MyObject.Scale.X < 0f)))
			{
				MyObject.SetScale(MyObject.Scale.X * -1f, MyObject.Scale.Y);
				if (_flipVelocity != 0f && MyObject.Velocity.X < 0f)
				{
					MyObject.SetVelocity(MyObject.Velocity.X * _flipVelocity, MyObject.Velocity.Y);
				}
			}
			break;
		}
		StartExit();
	}
}
