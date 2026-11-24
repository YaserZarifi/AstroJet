using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnSpineEventCreate : CreateObjectBehavior
{
	private string _eventName;

	private int _rootMode;

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
	}

	public void OnSpineAnimEvent(string eventString, int intParam, float floatParam, string stringParam)
	{
		SpineAnimUtils.ParseEventString(MyObject, _rootMode, eventString, out var eventName, out var targetPart);
		if (SpineAnimUtils.IsMyEvent(eventName, _eventName))
		{
			CreateObjectList(targetPart, Vector2.Zero);
		}
	}
}
