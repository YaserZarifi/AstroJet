namespace Rake2D.Script;

public class OnStateDetach : ExitStateBehavior
{
	public override void RegisterEvents()
	{
		if (MyObject.ParentObject != null)
		{
			MyObject.ParentObject.DetachChild(MyObject, MyLayer, bRestoreClass: false);
		}
		StartExit();
	}
}
