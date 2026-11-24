namespace Rake2D.Script;

public class OnSpineEventThrow : ThrowObjectBehavior
{
	private string _eventName;

	private bool _ignoreKilled;

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
		_rootMode = ParamInt("root_mode", 1);
		_ignoreKilled = ParamBool("ignore_killed");
	}

	public void OnSpineAnimEvent(string eventString, int intParam, float floatParam, string stringParam)
	{
		if (_ignoreKilled || !MyObject.IsKilled)
		{
			SpineAnimUtils.ParseEventString(MyObject, _rootMode, eventString, out var eventName, out var targetPart);
			if ((targetPart == null || _ignoreKilled || !targetPart.IsKilled) && SpineAnimUtils.IsMyEvent(eventName, _eventName))
			{
				StartThrow(targetPart);
			}
		}
	}
}
