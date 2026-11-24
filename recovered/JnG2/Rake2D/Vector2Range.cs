using Microsoft.Xna.Framework;

namespace Rake2D;

public struct Vector2Range
{
	public float FromX;

	public float ToX;

	public float FromY;

	public float ToY;

	public bool IsZero
	{
		get
		{
			if (FromX == 0f && FromX == FromY && ToX == ToY)
			{
				return FromX == ToY;
			}
			return false;
		}
	}

	public Vector2Range(float fromX, float fromY, float toX, float toY)
	{
		FromX = fromX;
		ToX = toX;
		FromY = fromY;
		ToY = toY;
	}

	public Vector2Range(float from, float to)
		: this(from, to, from, to)
	{
	}

	public Vector2Range(Vector2 from, Vector2 to)
	{
		FromX = from.X;
		ToX = to.X;
		FromY = from.Y;
		ToY = to.Y;
	}

	public Vector2 GetRandom()
	{
		return new Vector2(GetRandomX(), GetRandomY());
	}

	public Vector2 GetAverage()
	{
		return new Vector2((FromX + ToX) / 2f, (FromY + ToY) / 2f);
	}

	public float GetRandomX()
	{
		return Utils.GetRandom(FromX, ToX);
	}

	public float GetRandomY()
	{
		return Utils.GetRandom(FromY, ToY);
	}

	public static Vector2Range operator *(Vector2Range range, float value)
	{
		return new Vector2Range(range.FromX * value, range.FromY * value, range.ToX * value, range.ToY * value);
	}

	public static Vector2Range operator /(Vector2Range range, float value)
	{
		return new Vector2Range(range.FromX / value, range.FromY / value, range.ToX / value, range.ToY / value);
	}
}
