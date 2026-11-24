namespace Rake2D.Script;

public class OnLinkDestroy : OnLinkBehavior
{
	protected override void DoAction(GameObject senderObject, string linkIn, int linkValue)
	{
		if (ParamBool("main"))
		{
			MyObject.GetMainPart().Destroy();
		}
		else
		{
			MyObject.Destroy();
		}
	}
}
