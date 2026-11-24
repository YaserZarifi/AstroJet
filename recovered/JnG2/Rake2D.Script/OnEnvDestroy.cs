namespace Rake2D.Script;

public class OnEnvDestroy : OnEnvChangedBase
{
	protected override void DoAction()
	{
		MyObject.Destroy();
	}
}
