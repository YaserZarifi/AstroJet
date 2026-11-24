namespace Rake2D.Script;

public class OnStateSpeed : SpeedKeyValueBehavior
{
	public override void RegisterEvents()
	{
		base.RegisterEvents();
		SetSpeedKey();
	}
}
