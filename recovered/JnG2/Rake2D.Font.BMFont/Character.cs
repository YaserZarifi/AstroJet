using Microsoft.Xna.Framework;

namespace Rake2D.Font.BMFont;

public struct Character
{
	public Rectangle Bounds { get; set; }

	public int Channel { get; set; }

	public char Char { get; set; }

	public Point Offset { get; set; }

	public int TexturePage { get; set; }

	public int XAdvance { get; set; }

	public override string ToString()
	{
		return Char.ToString();
	}
}
