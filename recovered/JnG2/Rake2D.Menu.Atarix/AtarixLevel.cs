namespace Rake2D.Menu.Atarix;

public class AtarixLevel
{
	private int[,] _pit;

	public AtarixLevel()
	{
		_pit = new int[AtarixTetrix.PIT_HEIGHT, AtarixTetrix.PIT_WIDTH];
		for (int num = _pit.GetUpperBound(0); num >= 0; num--)
		{
			for (int num2 = _pit.GetUpperBound(1); num2 >= 0; num2--)
			{
				_pit[num, num2] = -1;
			}
		}
	}

	private int GetPitHeight()
	{
		return _pit.GetLength(0);
	}

	private int GetPitWidth()
	{
		return _pit.GetLength(1);
	}

	public int GetBrick(int row, int column)
	{
		if (row < 0 || row > _pit.GetUpperBound(0) || column < 0 || column > _pit.GetUpperBound(1))
		{
			return -1;
		}
		return _pit[row, column];
	}

	public bool SetRowByStr(int row, string rowStr)
	{
		if (row >= 0 && row < GetPitHeight())
		{
			for (int i = 0; i < GetPitWidth(); i++)
			{
				if (i >= rowStr.Length)
				{
					_pit[row, i] = -1;
					continue;
				}
				char c = char.ToUpper(rowStr[i]);
				int num;
				switch (c)
				{
				default:
					_pit[row, i] = -1;
					continue;
				case 'A':
				case 'B':
				case 'C':
				case 'D':
				case 'E':
				case 'F':
					num = 10 + (c - 65);
					break;
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				case ':':
				case ';':
				case '<':
				case '=':
				case '>':
				case '?':
				case '@':
					num = c - 48;
					break;
				}
				int num2 = num;
				_pit[row, i] = num2;
			}
			return true;
		}
		GameCore.LogErr("Wrong Tetrix row number {0} <> [0, {1}]", row, GetPitHeight() - 1);
		return false;
	}
}
