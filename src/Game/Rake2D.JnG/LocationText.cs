using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Font.BMFont;

namespace Rake2D.JnG;

public class LocationText
{
	public Vector2 Position;

	private List<string> _lineList;

	private BMFont _font;

	private int _currLine;

	private int _currChar;

	private float _charTimer;

	private float _fadeStartTimer;

	private Vector2 _fontScale;

	private SpriteBatch _spriteBatch;

	private const int _lineHeight = 25;

	private float _alpha;

	private bool _updateCursor;

	public LocationText()
	{
		_lineList = new List<string>();
		_font = GameCore.GetBMFont("level_font");
		_spriteBatch = new SpriteBatch(GameCore.GraphicsDeviceStatic);
		_fontScale = new Vector2(0.6f, 0.7f);
		Position = new Vector2(48f, 980f);
		Reset(null);
	}

	public void SetLevel(GameLevel level)
	{
		_lineList.Clear();
		if (level == null)
		{
			return;
		}
		foreach (string locationText in level.LocationTextList)
		{
			_lineList.Add(locationText);
		}
	}

	public void Reset(GameLevel level)
	{
		_currLine = 0;
		_currChar = 0;
		_charTimer = 0f;
		_alpha = 1f;
		_fadeStartTimer = 3f;
		_updateCursor = true;
		SetLevel(level);
	}

	public void Update(float elapsedTime)
	{
		if (_currLine == _lineList.Count - 1)
		{
			_fadeStartTimer -= elapsedTime;
			if (_fadeStartTimer <= 0f)
			{
				Utils.ChangeValueToTarget(ref _alpha, 0f, elapsedTime);
			}
		}
		if (!_updateCursor)
		{
			return;
		}
		_charTimer -= elapsedTime;
		if (_charTimer <= 0f)
		{
			if (_currChar > 0)
			{
				GameCore.PlaySound("loc_char");
			}
			_charTimer += 0.04f;
			_currChar++;
		}
		if (_currLine < _lineList.Count && _currChar >= _lineList[_currLine].Length)
		{
			if (_lineList[_currLine].Length > 0)
			{
				GameCore.PlaySound("loc_line");
			}
			_currLine++;
			_currChar = 0;
		}
		if (_currLine >= _lineList.Count)
		{
			_currLine = _lineList.Count - 1;
			_updateCursor = false;
		}
	}

	public void Draw()
	{
		if (_alpha <= 0f || _lineList.IsNullOrEmpty())
		{
			return;
		}
		_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, GameCore.ViewPortMatrix);
		Color value = new Color(255, 255, 255, (int)(255f * _alpha));
		float num = Position.Y - (float)(_currLine * 25);
		for (int i = 0; i <= _currLine; i++)
		{
			if (_updateCursor && _currLine == i && _currChar < _lineList[i].Length)
			{
				_font.Write(_spriteBatch, _lineList[i].Substring(0, _currChar) + "✦", Position.X, num, _fontScale, value);
			}
			else
			{
				_font.Write(_spriteBatch, _lineList[i], Position.X, num, _fontScale, value);
			}
			num += 25f;
		}
		_spriteBatch.End();
	}
}
