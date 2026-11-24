namespace Rake2D.Script;

public class OnScreenSendLink : OnScreenBase
{
	protected override void DoAction(int direction)
	{
		base.DoAction(direction);
		MyObject.SendLinkToAll(MyObject, ParamRandomString("link_out"), ParamRandomIntRange("link_value"));
	}
}
