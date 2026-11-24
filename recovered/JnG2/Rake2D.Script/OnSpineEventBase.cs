namespace Rake2D.Script;

public abstract class OnSpineEventBase : BehaviorScript
{
	private string _eventName;

	private bool _bIgnoreKilled;

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
		_bIgnoreKilled = ParamBool("ignore_killed");
	}

	public void OnSpineAnimEvent(string eventString, int intParam, float floatParam, string stringParam)
	{
		if ((!MyObject.IsKilled || _bIgnoreKilled) && SpineAnimUtils.IsMyEvent(eventString, _eventName))
		{
			DoAction();
		}
	}

	protected abstract void DoAction();
}
