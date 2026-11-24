namespace Rake2D.Script;

public class OnKillDestroyChildren : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	public void OnKill()
	{
		if (MyObject.ChildObjectListBack != null)
		{
			int count = MyObject.ChildObjectListBack.Count;
			for (int i = 0; i < count; i++)
			{
				MyObject.ChildObjectListBack[i].Destroy();
			}
		}
		if (MyObject.ChildObjectListFront != null)
		{
			int count2 = MyObject.ChildObjectListFront.Count;
			for (int j = 0; j < count2; j++)
			{
				MyObject.ChildObjectListFront[j].Destroy();
			}
		}
	}
}
