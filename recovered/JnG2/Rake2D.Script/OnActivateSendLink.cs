namespace Rake2D.Script;

public class OnActivateSendLink : ExitStateBehavior
{
	private string _eventName;

	private int _eventValue;

	public override void RegisterEvents()
	{
		if (DoInEditor)
		{
			MyObject.OnActivate += OnActivate;
		}
	}

	public override void Init()
	{
		base.Init();
		_eventName = ParamString("link_out");
		_eventValue = ParamInt("link_value");
	}

	private void OnActivate()
	{
		MyObject.GetMainPart().SendLinkToAll(MyObject, _eventName, _eventValue);
		StartExit();
	}
}
