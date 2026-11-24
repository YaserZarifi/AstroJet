using Microsoft.Xna.Framework;

namespace Rake2D.Editor;

public class ComboItem
{
	public const int Height = 20;

	public string Id;

	public string Text;

	public bool IsSelected;

	public bool IsChoosen;

	public Vector2 Position;

	public ComboItem(string id, string text)
	{
		Id = id;
		Text = text;
		IsSelected = false;
		IsChoosen = false;
		Position = Vector2.Zero;
	}
}
