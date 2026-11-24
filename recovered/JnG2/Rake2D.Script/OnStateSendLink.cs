namespace Rake2D.Script;

public class OnStateSendLink : ExitStateBehavior
{
	private string _eventName;

	private int _eventValue;

	public override void RegisterEvents()
	{
		if (DoInEditor)
		{
			MyObject.GetMainPart().SendLinkToAll(MyObject, _eventName, _eventValue);
			StartExit();
		}
	}

	public override void Init()
	{
		base.Init();
		_eventName = ParamString("link_out");
		_eventValue = ParamInt("link_value");
	}
}
