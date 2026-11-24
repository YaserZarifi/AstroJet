using System;
using System.Collections.Generic;

namespace Rake2D;

internal class VisibleObjectSorter
{
	private List<GameObject>[] _buckets;

	internal VisibleObjectSorter()
	{
	}

	internal void Sort(List<GameObject> list)
	{
		int count = list.Count;
		if (count <= 1)
		{
			return;
		}
		int num = int.MaxValue;
		int num2 = int.MinValue;
		for (int i = 0; i < count; i++)
		{
			GameObject gameObject = list[i];
			num = Math.Min(num, gameObject.ZOrder);
			num2 = Math.Max(num2, gameObject.ZOrder);
		}
		int num3 = num2 - num + 1;
		if (_buckets == null || _buckets.Length < num3)
		{
			if (_buckets != null)
			{
				for (int j = 0; j < _buckets.Length; j++)
				{
					GameCore.Allocator.DeleteAliasingGameObjectList(_buckets[j]);
				}
			}
			_buckets = new List<GameObject>[num3];
		}
		for (int k = 0; k < count; k++)
		{
			GameObject gameObject2 = list[k];
			int zOrder = gameObject2.ZOrder;
			List<GameObject> list2 = _buckets[zOrder - num];
			if (list2 == null)
			{
				list2 = GameCore.Allocator.GetGameObjectList();
				_buckets[zOrder - num] = list2;
			}
			list2.Add(gameObject2);
		}
		list.Clear();
		for (int num4 = num2; num4 >= num; num4--)
		{
			List<GameObject> list3 = _buckets[num4 - num];
			if (list3 != null)
			{
				list.AddList(list3);
			}
		}
		for (int l = 0; l < _buckets.Length; l++)
		{
			if (_buckets[l] != null)
			{
				_buckets[l].Clear();
			}
		}
	}
}
