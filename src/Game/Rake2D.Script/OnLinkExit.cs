namespace Rake2D.Script;

public class OnLinkExit : ExitStateBehavior
{
	private string[] _eventNameArray;

	public override void Init()
	{
		base.Init();
		_eventNameArray = ParamStringArray("link_in");
	}

	public override void RegisterEvents()
	{
		MyObject.OnLink += OnLink;
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

	private void OnLink(GameObject senderObject, string linkInEvent, int value)
	{
		if (IsMyEvent(linkInEvent))
		{
			StartExit();
		}
	}
}
