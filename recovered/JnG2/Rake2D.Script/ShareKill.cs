namespace Rake2D.Script;

public class ShareKill : BehaviorScript
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
				gameObject.OnKill += OnSharedKill;
				gameObject.OnReRegisterEvents += OnReRegisterEvents;
			}
		}
	}

	private void OnReRegisterEvents(GameObject go)
	{
		go.OnKill += OnSharedKill;
	}

	private void OnSharedKill()
	{
		float num = Param("delay") / 1000f;
		if (MyObject.IsUpdatable && !MyObject.IsKilled && MyObject.KillTimer > num)
		{
			if (ParamBool("main"))
			{
				MyObject.GetMainPart().KillTimer = num;
			}
			else
			{
				MyObject.KillTimer = num;
			}
		}
	}

	private void OnDestroy()
	{
		if (MyObject.LinkedObjectList != null)
		{
			int count = MyObject.LinkedObjectList.Count;
			for (int i = 0; i < count; i++)
			{
				MyObject.LinkedObjectList[i].OnKill -= OnSharedKill;
			}
		}
	}
}
