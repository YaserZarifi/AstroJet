namespace Rake2D.Script;

public class OnEnvKill : OnEnvChangedBase
{
	protected override void DoAction()
	{
		MyObject.Kill();
	}
}
