namespace Rake2D.Script;

public class OnKillSendLink : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	protected void OnKill()
	{
		string text = ParamString("link_out");
		if (text != null)
		{
			MyObject.SendLinkToAll(MyObject, text, ParamRandomIntRange("link_value", 1));
		}
	}
}
