namespace Rake2D.Script;

public abstract class OnLinkBehavior : BehaviorScript
{
	protected string[] _eventNameArray;

	public override void Init()
	{
		base.Init();
		_eventNameArray = ParamStringArray("link_in");
	}

	public override void RegisterEvents()
	{
		if (!GameCore.IsEditorEnabled || _doInEditor)
		{
			MyObject.OnLink += OnLink;
		}
	}

	private bool IsMyEvent(string eventName)
	{
		if (_eventNameArray != null)
		{
			string[] eventNameArray = _eventNameArray;
			for (int i = 0; i < eventNameArray.Length; i++)
			{
				if (Utils.Equals(eventNameArray[i], eventName))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public void OnLink(GameObject senderObject, string linkInEvent, int value)
	{
		if (IsMyEvent(linkInEvent))
		{
			DoAction(senderObject, linkInEvent, value);
		}
	}

	protected abstract void DoAction(GameObject senderObject, string linkIn, int linkValue);
}
