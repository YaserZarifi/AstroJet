namespace Rake2D.JnG;

public class HeroItemDataParam
{
	private float[] _valueArray;

	public HeroItemDataParam(float[] valueArray)
	{
		_valueArray = valueArray;
	}

	public float GetValue(int level)
	{
		if (_valueArray.IsNullOrEmpty() || level < 0)
		{
			return 0f;
		}
		if (level >= _valueArray.Length)
		{
			return _valueArray[_valueArray.Length - 1];
		}
		return _valueArray[level];
	}
}
