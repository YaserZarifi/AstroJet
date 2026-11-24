using System.Linq;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnSquadronCreate : CreateObjectBehavior
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
						CreateObjectList(gameObject, gameObject.PartData.TargetOffset);
					}
					else
					{
						CreateObjectList(null, Vector2.Zero);
					}
					MyObject.Destroy();
				}
			}
		}
	}
}
