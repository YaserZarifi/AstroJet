using Microsoft.Xna.Framework;

namespace Rake2D;

public class SpriteAtlasFrame
{
	public string Id;

	public Rectangle Rectangle;

	public Vector2 Pivot;

	public SpriteAtlasFrame(int x, int y, int width, int height)
	{
		Rectangle = new Rectangle(x, y, width, height);
		PivotAlign(Align.Center);
	}

	public void PivotAlign(Align align)
	{
		switch (align)
		{
		case Align.Center:
			Pivot = new Vector2(Rectangle.Width / 2, Rectangle.Height / 2);
			break;
		case Align.TopLeft:
			Pivot = Vector2.Zero;
			break;
		case Align.TopCenter:
			SetPivot(Rectangle.Width / 2, 0);
			break;
		case Align.TopRight:
			SetPivot(Rectangle.Width / 2, Rectangle.Height);
			break;
		case Align.RightCenter:
			SetPivot(Rectangle.Width, Rectangle.Height / 2);
			break;
		case Align.BottomRight:
			SetPivot(Rectangle.Width, Rectangle.Height);
			break;
		case Align.BottomCenter:
			SetPivot(Rectangle.Width / 2, Rectangle.Height);
			break;
		case Align.BottomLeft:
			SetPivot(0, Rectangle.Height);
			break;
		case Align.LeftCenter:
			SetPivot(0, Rectangle.Height / 2);
			break;
		}
	}

	public void SetPivot(int x, int y)
	{
		Pivot = new Vector2(x, y);
	}
}
