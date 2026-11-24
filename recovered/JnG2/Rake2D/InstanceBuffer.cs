namespace Rake2D;

public class InstanceBuffer<T> where T : class
{
	private T[] _array;

	public int Capacity => _array.Length;

	public int Position { get; protected set; }

	public InstanceBuffer(int bufferSize)
	{
		_array = new T[bufferSize];
		Position = -1;
	}

	public T Get()
	{
		if (Position >= 0)
		{
			T result = _array[Position];
			_array[Position] = null;
			Position--;
			return result;
		}
		return null;
	}

	public void Put(T item)
	{
		if (item != null && Position < _array.Length - 1)
		{
			_array[++Position] = item;
		}
	}

	public void Report()
	{
	}
}
