using System;
using System.Collections.Generic;
using FarseerPhysics.Common.ConvexHull;

namespace FarseerPhysics.Common.Decomposition;

public static class Triangulate
{
	public static List<Vertices> ConvexPartition(Vertices vertices, TriangulationAlgorithm algorithm, bool discardAndFixInvalid = true, float tolerance = 0.001f)
	{
		if (vertices.Count <= 3)
		{
			return new List<Vertices> { vertices };
		}
		List<Vertices> list;
		switch (algorithm)
		{
		case TriangulationAlgorithm.Earclip:
			if (vertices.IsCounterClockWise())
			{
				Vertices vertices4 = new Vertices(vertices);
				vertices4.Reverse();
				list = EarclipDecomposer.ConvexPartition(vertices4, tolerance);
			}
			else
			{
				list = EarclipDecomposer.ConvexPartition(vertices, tolerance);
			}
			break;
		case TriangulationAlgorithm.Bayazit:
			if (!vertices.IsCounterClockWise())
			{
				Vertices vertices3 = new Vertices(vertices);
				vertices3.Reverse();
				list = BayazitDecomposer.ConvexPartition(vertices3);
			}
			else
			{
				list = BayazitDecomposer.ConvexPartition(vertices);
			}
			break;
		case TriangulationAlgorithm.Flipcode:
			if (!vertices.IsCounterClockWise())
			{
				Vertices vertices2 = new Vertices(vertices);
				vertices2.Reverse();
				list = FlipcodeDecomposer.ConvexPartition(vertices2);
			}
			else
			{
				list = FlipcodeDecomposer.ConvexPartition(vertices);
			}
			break;
		case TriangulationAlgorithm.Seidel:
			list = SeidelDecomposer.ConvexPartition(vertices, tolerance);
			break;
		case TriangulationAlgorithm.SeidelTrapezoids:
			list = SeidelDecomposer.ConvexPartitionTrapezoid(vertices, tolerance);
			break;
		case TriangulationAlgorithm.Delauny:
			list = CDTDecomposer.ConvexPartition(vertices);
			break;
		default:
			throw new ArgumentOutOfRangeException("algorithm");
		}
		if (discardAndFixInvalid)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (!ValidatePolygon(list[num]))
				{
					list.RemoveAt(num);
				}
			}
		}
		return list;
	}

	private static bool ValidatePolygon(Vertices polygon)
	{
		PolygonError polygonError = polygon.CheckPolygon();
		switch (polygonError)
		{
		case PolygonError.InvalidAmountOfVertices:
		case PolygonError.NotSimple:
		case PolygonError.AreaTooSmall:
		case PolygonError.SideTooSmall:
			return false;
		case PolygonError.NotCounterClockWise:
			polygon.Reverse();
			break;
		}
		if (polygonError == PolygonError.NotConvex)
		{
			polygon = GiftWrap.GetConvexHull(polygon);
			return ValidatePolygon(polygon);
		}
		return true;
	}
}
