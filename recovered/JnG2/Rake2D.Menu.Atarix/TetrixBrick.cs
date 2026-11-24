namespace Rake2D.Menu.Atarix;

public class TetrixBrick
{
	public enum TetrixBrickTypeEnum
	{
		raiden,
		sub_zero,
		scorpion,
		sonya,
		johny_cage,
		liu_kang,
		kano,
		kung_lao,
		sheeva,
		baraka,
		mileena,
		sindel,
		drax,
		cyrax,
		sektor,
		goro
	}

	public int[,] ShapeArray;

	public int PivotX;

	public int PivotY;

	public int PivotType;

	public int Rotation;

	public int SquareColorNumber;

	public int[] ComboKeyArray;

	public TetrixBrickTypeEnum Type;

	public void SetType(TetrixBrickTypeEnum type)
	{
		Type = type;
		switch (Type)
		{
		case TetrixBrickTypeEnum.raiden:
			ShapeArray = new int[1, 4] { { 1, 5, 1, 1 } };
			SquareColorNumber = 2;
			ComboKeyArray = new int[6] { 3, 1, 3, 1, 3, 1 };
			break;
		case TetrixBrickTypeEnum.sub_zero:
			ShapeArray = new int[3, 2]
			{
				{ 1, 0 },
				{ 2, 0 },
				{ 1, 1 }
			};
			SquareColorNumber = 3;
			ComboKeyArray = new int[5] { 2, 3, 1, 2, 2 };
			break;
		case TetrixBrickTypeEnum.scorpion:
			ShapeArray = new int[3, 2]
			{
				{ 0, 1 },
				{ 0, 2 },
				{ 1, 1 }
			};
			SquareColorNumber = 0;
			ComboKeyArray = new int[6] { 2, 2, 3, 1, 0, 0 };
			break;
		case TetrixBrickTypeEnum.sonya:
			ShapeArray = new int[2, 3]
			{
				{ 1, 2, 1 },
				{ 0, 1, 0 }
			};
			SquareColorNumber = 1;
			ComboKeyArray = new int[7] { 2, 0, 3, 2, 0, 1, 2 };
			break;
		case TetrixBrickTypeEnum.johny_cage:
			ShapeArray = new int[2, 3]
			{
				{ 0, 1, 1 },
				{ 1, 4, 0 }
			};
			SquareColorNumber = 4;
			ComboKeyArray = new int[6] { 0, 3, 1, 2, 2, 2 };
			break;
		case TetrixBrickTypeEnum.liu_kang:
			ShapeArray = new int[2, 3]
			{
				{ 1, 1, 0 },
				{ 0, 4, 1 }
			};
			SquareColorNumber = 5;
			ComboKeyArray = new int[5] { 0, 0, 3, 1, 2 };
			break;
		case TetrixBrickTypeEnum.kano:
			ShapeArray = new int[2, 2]
			{
				{ 3, 1 },
				{ 1, 1 }
			};
			SquareColorNumber = 8;
			ComboKeyArray = new int[5] { 2, 3, 0, 1, 2 };
			break;
		case TetrixBrickTypeEnum.kung_lao:
			ShapeArray = new int[2, 2]
			{
				{ 3, 1 },
				{ 1, 0 }
			};
			SquareColorNumber = 10;
			ComboKeyArray = new int[4] { 0, 2, 0, 2 };
			break;
		case TetrixBrickTypeEnum.sheeva:
			ShapeArray = new int[3, 3]
			{
				{ 1, 0, 0 },
				{ 1, 2, 0 },
				{ 1, 1, 1 }
			};
			SquareColorNumber = 0;
			ComboKeyArray = new int[7] { 3, 3, 1, 1, 0, 2, 0 };
			break;
		case TetrixBrickTypeEnum.baraka:
			ShapeArray = new int[3, 3]
			{
				{ 1, 0, 0 },
				{ 1, 2, 1 },
				{ 1, 0, 0 }
			};
			SquareColorNumber = 8;
			ComboKeyArray = new int[6] { 0, 2, 3, 0, 2, 1 };
			break;
		case TetrixBrickTypeEnum.mileena:
			ShapeArray = new int[3, 3]
			{
				{ 0, 0, 1 },
				{ 0, 2, 0 },
				{ 1, 0, 0 }
			};
			SquareColorNumber = 4;
			ComboKeyArray = new int[6] { 0, 3, 0, 1, 0, 2 };
			break;
		case TetrixBrickTypeEnum.sindel:
			ShapeArray = new int[1, 2] { { 2, 1 } };
			SquareColorNumber = 6;
			ComboKeyArray = new int[7] { 3, 0, 1, 2, 3, 3, 3 };
			break;
		case TetrixBrickTypeEnum.drax:
			ShapeArray = new int[2, 3]
			{
				{ 1, 6, 1 },
				{ 1, 0, 1 }
			};
			SquareColorNumber = 12;
			ComboKeyArray = new int[4] { 2, 0, 3, 1 };
			break;
		case TetrixBrickTypeEnum.cyrax:
			ShapeArray = new int[2, 3]
			{
				{ 1, 0, 0 },
				{ 0, 2, 1 }
			};
			SquareColorNumber = 0;
			ComboKeyArray = new int[7] { 1, 1, 3, 3, 0, 0, 2 };
			break;
		case TetrixBrickTypeEnum.sektor:
			ShapeArray = new int[2, 3]
			{
				{ 0, 0, 1 },
				{ 1, 2, 0 }
			};
			SquareColorNumber = 13;
			ComboKeyArray = new int[4] { 0, 2, 3, 1 };
			break;
		case TetrixBrickTypeEnum.goro:
			ShapeArray = new int[3, 3]
			{
				{ 0, 1, 0 },
				{ 1, 2, 1 },
				{ 0, 1, 0 }
			};
			SquareColorNumber = 0;
			ComboKeyArray = new int[8] { 0, 3, 2, 0, 1, 2, 0, 2 };
			break;
		}
		GetPivot(out PivotX, out PivotY, out PivotType);
	}

	public void GetPivot(out int pivotX, out int pivotY, out int pivotType)
	{
		pivotX = 0;
		pivotY = 0;
		pivotType = 0;
		int upperBound = ShapeArray.GetUpperBound(0);
		int upperBound2 = ShapeArray.GetUpperBound(1);
		for (int i = 0; i <= upperBound; i++)
		{
			for (int j = 0; j <= upperBound2; j++)
			{
				if (ShapeArray[i, j] > 1)
				{
					pivotX = j;
					pivotY = i;
					pivotType = ShapeArray[i, j];
					break;
				}
			}
		}
	}
}
