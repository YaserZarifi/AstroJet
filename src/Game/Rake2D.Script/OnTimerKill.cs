namespace Rake2D.Script;

public class OnTimerKill : OnTimerBehavior
{
	protected override void DoAction()
	{
		if (ParamBool("main"))
		{
			MyObject.GetMainPart().Kill();
		}
		else
		{
			MyObject.Kill();
		}
	}
}
