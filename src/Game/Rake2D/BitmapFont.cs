using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D;

public class BitmapFont
{
	public string Id;

	public int FirstCharNumber;

	private Texture2D _texture;

	private Rectangle[] _charRect;

	public BitmapFont(string id)
	{
		Id = id;
		FirstCharNumber = 32;
	}

	public bool LoadTexture(string filename)
	{
		_texture = GameCore.GlobalContent.LoadTexture(filename, filename);
		return true;
	}

	public bool SetGrid(int gridWidth, int gridHeight, int gridCharWidth, int gridCharHeight, int defaultCharWidth)
	{
		_charRect = new Rectangle[gridWidth * gridHeight];
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < _charRect.Length; i++)
		{
			_charRect[i] = new Rectangle(num * gridCharWidth, num2 * gridCharHeight, defaultCharWidth, gridCharHeight);
			if (++num >= gridWidth)
			{
				num = 0;
				num2++;
			}
		}
		return true;
	}

	public int GetCharWidth(byte asciByte)
	{
		return _charRect[asciByte - FirstCharNumber].Width;
	}

	public int GetTextWidth(string text)
	{
		int num = 0;
		byte[] bytes = Encoding.ASCII.GetBytes(text);
		foreach (byte asciByte in bytes)
		{
			num += GetCharWidth(asciByte);
		}
		return num;
	}

	public void Write(SpriteBatch spriteBatch, string text, Vector2 pos, Color color, TextAlign align = TextAlign.Left, int maxWidth = 0)
	{
		Write(spriteBatch, text, pos.X, pos.Y, color, align, maxWidth);
	}

	public void Write(SpriteBatch spriteBatch, string text, float x, float y, Color color, TextAlign align = TextAlign.Left, int maxWidth = 0)
	{
		if (text == null || _texture == null)
		{
			return;
		}
		byte[] bytes = Encoding.ASCII.GetBytes(text);
		Vector2 position = new Vector2(x, y);
		switch (align)
		{
		case TextAlign.Left:
		{
			byte[] array = bytes;
			foreach (byte b2 in array)
			{
				int charWidth = GetCharWidth(b2);
				if (maxWidth <= 0 || !(position.X - x >= (float)(maxWidth - charWidth)))
				{
					position = new Vector2((int)position.X, (int)position.Y);
					spriteBatch.Draw(_texture, position, _charRect[b2 - FirstCharNumber], color);
					position.X += charWidth;
					continue;
				}
				break;
			}
			break;
		}
		case TextAlign.Right:
		{
			int num2 = bytes.Length - 1;
			while (num2 >= 0)
			{
				int charWidth = GetCharWidth(bytes[num2]);
				if (maxWidth <= 0 || !(x - position.X >= (float)(maxWidth - charWidth)))
				{
					position.X -= charWidth;
					position = new Vector2((int)position.X, (int)position.Y);
					spriteBatch.Draw(_texture, position, _charRect[bytes[num2] - FirstCharNumber], color);
					num2--;
					continue;
				}
				break;
			}
			break;
		}
		case TextAlign.Center:
		{
			int num = 0;
			byte[] array = bytes;
			foreach (byte asciByte in array)
			{
				num += GetCharWidth(asciByte);
			}
			position.X -= num / 2;
			position = new Vector2((int)position.X, (int)position.Y);
			array = bytes;
			foreach (byte b in array)
			{
				spriteBatch.Draw(_texture, position, _charRect[b - FirstCharNumber], color);
				position.X += GetCharWidth(b);
			}
			break;
		}
		}
	}
}
