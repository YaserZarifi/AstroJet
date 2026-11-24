namespace Rake2D.Script;

public static class SpineAnimUtils
{
	public static string ParseEventString(string eventString)
	{
		if (eventString == null)
		{
			return null;
		}
		return eventString.Split('/')[0];
	}

	public static void ParseEventString(GameObject myObject, int rootMode, string eventString, out string eventName, out GameObject targetPart)
	{
		ParseEventString(eventString, out eventName, out var targetObjectId);
		if (targetObjectId != null)
		{
			switch (rootMode)
			{
			case 1:
				targetPart = myObject.GetMainPart().GetChildByIdDeep(targetObjectId);
				return;
			case 2:
				if (myObject.ParentObject != null)
				{
					targetPart = myObject.ParentObject.GetChildByIdDeep(targetObjectId);
					return;
				}
				break;
			}
			targetPart = myObject.GetChildByIdDeep(targetObjectId);
		}
		else
		{
			targetPart = null;
		}
	}

	public static void ParseEventString(string eventString, out string eventName, out string targetObjectId)
	{
		if (eventString == null)
		{
			eventName = null;
			targetObjectId = null;
			return;
		}
		string[] array = eventString.Split('/');
		eventName = array[0];
		if (array.Length > 1)
		{
			targetObjectId = array[1];
		}
		else
		{
			targetObjectId = null;
		}
	}

	public static bool IsMyEvent(string eventString, string eventName)
	{
		if (eventName != null)
		{
			return Utils.Equals(eventName, ParseEventString(eventString));
		}
		return true;
	}
}
