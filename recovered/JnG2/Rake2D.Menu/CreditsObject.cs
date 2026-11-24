using Microsoft.Xna.Framework.Graphics;
using Rake2D.Font.BMFont;

namespace Rake2D.Menu;

public class CreditsObject : GameObject
{
	private string _text;

	private BMFont _font;

	public CreditsObject(BMFont font, string text)
	{
		_font = font;
		_text = text;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_text != null)
		{
			_font.Write(spriteBatch, _text, Position.X, Position.Y, Scale * Options.FontScale, null, TextAlign.Center, 0, (float)(int)Color.A / 255f);
		}
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
	}
}
