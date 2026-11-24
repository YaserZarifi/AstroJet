using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D.Menu;

public class CursorMessage
{
	public string Text;

	public Vector2 Position;

	public Color Color;

	public bool IsDestroyed;

	private float _lifeTime;

	private float _velocity;

	private ShopMenu _shopMenu;

	private Vector2 _scale;

	public CursorMessage(string text, Vector2 position, Color color, ShopMenu shopMenu)
	{
		Text = text;
		Position = position;
		Color = color;
		_shopMenu = shopMenu;
		_lifeTime = 1.1f;
		_velocity = 150f;
		_scale = new Vector2(0.3f);
	}

	public void Update(float elapsedTime)
	{
		IsDestroyed = Utils.ChangeValueToTarget(ref _lifeTime, 0f, elapsedTime);
		Position.Y -= _velocity * elapsedTime;
		Utils.ChangeValueToTarget(ref _velocity, 0f, 160f * elapsedTime);
		Color.A = (byte)Utils.Clamp(_lifeTime * 2f * 255f, 0f, 255f);
		Utils.ChangeValueToTarget(ref _scale, ShopMenu.FONT_SCALE_LABEL, 0.9f * elapsedTime);
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		_shopMenu.FontDefault.Write(spriteBatch, Text, Position.X, Position.Y - (float)(_shopMenu.FontDefault.BaseHeight / 2) * _scale.Y, _scale, Color, TextAlign.Center);
	}
}
