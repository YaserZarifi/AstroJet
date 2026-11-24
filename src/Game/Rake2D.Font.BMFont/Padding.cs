namespace Rake2D.Font.BMFont;

public struct Padding
{
	public int Bottom { get; set; }

	public int Left { get; set; }

	public int Right { get; set; }

	public int Top { get; set; }

	public Padding(int left, int top, int right, int bottom)
	{
		this = default;
		Top = top;
		Left = left;
		Right = right;
		Bottom = bottom;
	}

	public override string ToString()
	{
		return $"{Left}, {Top}, {Right}, {Bottom}";
	}
}
