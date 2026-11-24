using System.Linq;

namespace Rake2D.Script;

public class OnSquadronThrow : ThrowObjectBehavior
{
	private int _place;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
	}

	public override void Init()
	{
		base.Init();
		_place = ParamInt("place");
	}

	private void OnUpdate(float elapsedTime)
	{
		if (MyObject.LinkedObjectList == null)
		{
			return;
		}
		for (int num = Enumerable.Count(MyObject.LinkedObjectList) - 1; num >= 0; num--)
		{
			GameObject gameObject = MyObject.LinkedObjectList[num];
			if (gameObject.IsKilled)
			{
				MyObject.LinkedObjectList.RemoveAt(num);
				if (Enumerable.Count(MyObject.LinkedObjectList) == 0)
				{
					if (_place == 0)
					{
						StartThrow(gameObject);
					}
					else
					{
						StartThrow();
					}
				}
			}
		}
	}
}
