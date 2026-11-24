namespace Rake2D.Script;

public class OnSpineEndDestroy : OnSpineEndScript
{
	public override void DoAction()
	{
		MyObject.Destroy();
	}
}
