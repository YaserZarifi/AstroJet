namespace Rake2D.Script;

public class OnScreenExit : OnScreenBase
{
	protected override void DoAction(int direction)
	{
		base.DoAction(direction);
		StartExit();
	}
}
