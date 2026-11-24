namespace Rake2D.Script;

public class OnSpineEventExit : ExitStateBehavior
{
	private string _eventName;

	private bool _useEventNameAsPartId;

	private bool _useSpinePartList;

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
		_useEventNameAsPartId = _eventName != null && Utils.Equals(_eventName, "part_id");
		_bIgnoreKilled = ParamBool("ignore_killed");
		_useSpinePartList = _exitPartListAll.IsNullOrEmpty();
		if (_useSpinePartList && _exitPartListAll != null)
		{
			_exitPartListAll = GameCore.Allocator.DeleteAliasingGameObjectList(_exitPartListAll);
		}
	}

	public void OnSpineAnimEvent(string eventString, int intParam, float floatParam, string stringParam)
	{
		if (MyObject.IsKilled && !_bIgnoreKilled)
		{
			return;
		}
		SpineAnimUtils.ParseEventString(eventString, out var eventName, out var targetObjectId);
		if (_eventName != null && (!_useEventNameAsPartId || !Utils.Equals(eventName, MyObject.PartData.Id)) && !Utils.Equals(_eventName, eventName))
		{
			return;
		}
		if (_useSpinePartList && targetObjectId != null)
		{
			string[] idArray = targetObjectId.Split(',');
			_exitPartListAll = ExitStateBehavior.GetExitPartList(idArray, MyObject, out _bExitStateToAll, ParamInt("root_mode", 1));
			if (!_bIgnoreKilled)
			{
				for (int num = _exitPartListAll.Count - 1; num >= 0; num--)
				{
					if (_exitPartListAll[num].IsKilled)
					{
						_exitPartListAll.RemoveAt(num);
					}
				}
			}
		}
		StartExit();
	}
}
