namespace Rake2D.Script;

public class OnSpineEventQuake : BehaviorQuakeBase
{
	private string _eventName;

	public override void RegisterEvents()
	{
		if (_doInEditor || !GameCore.IsEditorEnabled)
		{
			MyObject.OnSpineAnimEvent += OnSpineAnimEvent;
		}
	}

	public override void Init()
	{
		base.Init();
		_eventName = ParamString("event");
	}

	public void OnSpineAnimEvent(string eventString, int intParam, float floatParam, string stringParam)
	{
		if (SpineAnimUtils.IsMyEvent(eventString, _eventName))
		{
			Quake();
		}
	}
}
