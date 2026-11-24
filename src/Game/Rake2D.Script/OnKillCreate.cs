using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnKillCreate : CreateObjectBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	public void OnKill()
	{
		List<GameObject> list = CreateAndReturnObjectList(null, Vector2.Zero);
		if (_layer == 0 && list != null)
		{
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				list[i].SetDestroyOnKill(value: false, bOnlyUndefined: false);
			}
		}
		GameCore.Allocator.DeleteAliasingGameObjectList(list);
	}
}
