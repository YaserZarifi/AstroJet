using System;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision.Shapes;

public struct MassData : IEquatable<MassData>
{
	public float Area { get; internal set; }

	public Vector2 Centroid { get; internal set; }

	public float Inertia { get; internal set; }

	public float Mass { get; internal set; }

	public static bool operator ==(MassData left, MassData right)
	{
		if (left.Area == right.Area && left.Mass == right.Mass && left.Centroid == right.Centroid)
		{
			return left.Inertia == right.Inertia;
		}
		return false;
	}

	public static bool operator !=(MassData left, MassData right)
	{
		return !(left == right);
	}

	public bool Equals(MassData other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.GetType() != typeof(MassData))
		{
			return false;
		}
		return Equals((MassData)obj);
	}

	public override int GetHashCode()
	{
		return (((((Area.GetHashCode() * 397) ^ Centroid.GetHashCode()) * 397) ^ Inertia.GetHashCode()) * 397) ^ Mass.GetHashCode();
	}
}
