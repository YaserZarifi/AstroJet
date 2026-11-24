namespace Rake2D;

public struct IntRange
{
	public int From;

	public int To;

	public bool IsZero
	{
		get
		{
			if (From == 0)
			{
				return To == 0;
			}
			return false;
		}
	}

	public IntRange(float[] valueArray)
	{
		if (valueArray.Length != 0)
		{
			From = (int)valueArray[0];
		}
		else
		{
			From = 0;
		}
		if (valueArray.Length > 1)
		{
			To = (int)valueArray[1];
		}
		else
		{
			To = From;
		}
	}

	public IntRange(int[] valueArray)
	{
		if (valueArray.Length != 0)
		{
			From = valueArray[0];
		}
		else
		{
			From = 0;
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

	public bool IsInRange(int value)
	{
		if (value >= From)
		{
			return value <= To;
		}
		return false;
	}

	public IntRange(int fromTo)
	{
		From = fromTo;
		To = fromTo;
	}

	public IntRange(int from, int to)
	{
		From = from;
		To = to;
	}

	public void SwapFromTo()
	{
		int to = To;
		To = From;
		From = to;
	}

	public int GetMidValue()
	{
		return From + (To - From) / 2;
	}

	public int GetRandom()
	{
		return Utils.GetRandom(From, To);
	}

	public static IntRange operator +(IntRange range, float value)
	{
		return new IntRange((int)((float)range.From + value), (int)((float)range.To + value));
	}

	public static IntRange operator -(IntRange range, float value)
	{
		return new IntRange((int)((float)range.From - value), (int)((float)range.To - value));
	}

	public static IntRange operator *(IntRange range, float value)
	{
		return new IntRange((int)((float)range.From * value), (int)((float)range.To * value));
	}

	public static IntRange operator /(IntRange range, float value)
	{
		return new IntRange((int)((float)range.From / value), (int)((float)range.To / value));
	}
}
