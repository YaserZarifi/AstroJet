namespace Rake2D;

public struct FloatRange
{
	public float From;

	public float To;

	public bool IsZero
	{
		get
		{
			if (From == 0f)
			{
				return To == 0f;
			}
			return false;
		}
	}

	public FloatRange(float[] valueArray)
	{
		if (valueArray.Length != 0)
		{
			From = valueArray[0];
		}
		else
		{
			From = 0f;
		}
		if (valueArray.Length > 1)
		{
			To = valueArray[1];
		}
		else
		{
			To = From;
		}
	}

	public FloatRange(int[] valueArray)
	{
		if (valueArray.Length != 0)
		{
			From = valueArray[0];
		}
		else
		{
			From = 0f;
		}
		if (valueArray.Length > 1)
		{
			To = valueArray[1];
		}
		else
		{
			To = From;
		}
	}

	public FloatRange(float from, float to)
	{
		From = from;
		To = to;
	}

	public FloatRange(FloatRange range)
	{
		From = range.From;
		To = range.To;
	}

	public FloatRange(float fromTo)
	{
		From = fromTo;
		To = fromTo;
	}

	public float GetRandom()
	{
		return Utils.GetRandom(this);
	}

	public float GetAverage()
	{
		return (From + To) / 2f;
	}

	public bool IsInRange(float value)
	{
		if (value > From)
		{
			return value < To;
		}
		return false;
	}

	public static FloatRange operator +(FloatRange range, float value)
	{
		return new FloatRange(range.From + value, range.To + value);
	}

	public static FloatRange operator -(FloatRange range, float value)
	{
		return new FloatRange(range.From - value, range.To - value);
	}

	public static FloatRange operator *(FloatRange range, float value)
	{
		return new FloatRange(range.From * value, range.To * value);
	}

	public static FloatRange operator /(FloatRange range, float value)
	{
		return new FloatRange(range.From / value, range.To / value);
	}
}
