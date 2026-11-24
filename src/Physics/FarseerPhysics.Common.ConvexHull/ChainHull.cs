using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.ConvexHull;

public static class ChainHull
{
	private class PointComparer : Comparer<Vector2>
	{
		public override int Compare(Vector2 a, Vector2 b)
		{
			int num = a.X.CompareTo(b.X);
			if (num == 0)
			{
				return a.Y.CompareTo(b.Y);
			}
			return num;
		}
	}

	private static PointComparer _pointComparer = new PointComparer();

	public static Vertices GetConvexHull(Vertices vertices)
	{
		if (vertices.Count <= 3)
		{
			return vertices;
		}
		Vertices vertices2 = new Vertices(vertices);
		vertices2.Sort(_pointComparer);
		Vector2[] array = new Vector2[vertices2.Count];
		int num = -1;
		float x = vertices2[0].X;
		int i;
		for (i = 1; i < vertices2.Count && vertices2[i].X == x; i++)
		{
		}
		int num2 = i - 1;
		Vertices vertices3;
		if (num2 == vertices2.Count - 1)
		{
			array[++num] = vertices2[0];
			if (vertices2[num2].Y != vertices2[0].Y)
			{
				array[++num] = vertices2[num2];
			}
			array[++num] = vertices2[0];
			vertices3 = new Vertices(num + 1);
			for (int j = 0; j < num + 1; j++)
			{
				vertices3.Add(array[j]);
			}
			return vertices3;
		}
		num = -1;
		int num3 = vertices2.Count - 1;
		float x2 = vertices2[vertices2.Count - 1].X;
		i = vertices2.Count - 2;
		while (i >= 0 && vertices2[i].X == x2)
		{
			i--;
		}
		int num4 = i + 1;
		array[++num] = vertices2[0];
		i = num2;
		while (++i <= num4)
		{
			if (!(MathUtils.Area(vertices2[0], vertices2[num4], vertices2[i]) >= 0f) || i >= num4)
			{
				while (num > 0 && !(MathUtils.Area(array[num - 1], array[num], vertices2[i]) > 0f))
				{
					num--;
				}
				array[++num] = vertices2[i];
			}
		}
		if (num3 != num4)
		{
			array[++num] = vertices2[num3];
		}
		int num5 = num;
		i = num4;
		while (--i >= num2)
		{
			if (!(MathUtils.Area(vertices2[num3], vertices2[num2], vertices2[i]) >= 0f) || i <= num2)
			{
				while (num > num5 && !(MathUtils.Area(array[num - 1], array[num], vertices2[i]) > 0f))
				{
					num--;
				}
				array[++num] = vertices2[i];
			}
		}
		if (num2 != 0)
		{
			array[++num] = vertices2[0];
		}
		vertices3 = new Vertices(num + 1);
		for (int k = 0; k < num + 1; k++)
		{
			vertices3.Add(array[k]);
		}
		return vertices3;
	}
}
