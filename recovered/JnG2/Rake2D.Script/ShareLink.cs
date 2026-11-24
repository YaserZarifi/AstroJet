namespace Rake2D.Script;

public class ShareLink : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.OnLevelBegin += OnLevelBegin;
		MyObject.OnDestroy += OnDestroy;
	}

	private void OnLevelBegin()
	{
		if (MyObject.LinkedObjectList != null)
		{
			int count = MyObject.LinkedObjectList.Count;
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = MyObject.LinkedObjectList[i];
				gameObject.OnLink += OnSharedLink;
				gameObject.OnReRegisterEvents += OnReRegisterEvents;
			}
		}
	}

	private void OnReRegisterEvents(GameObject go)
	{
		go.OnLink += OnSharedLink;
	}

	private void OnSharedLink(GameObject senderObject, string eventName, int value)
	{
		MyObject.DoOnLink(senderObject, eventName, value, bApplyChildren: true);
	}

	private void OnDestroy()
	{
		if (MyObject.LinkedObjectList != null)
		{
			int count = MyObject.LinkedObjectList.Count;
			for (int i = 0; i < count; i++)
			{
				MyObject.LinkedObjectList[i].OnLink -= OnSharedLink;
			}
		}
	}
}
