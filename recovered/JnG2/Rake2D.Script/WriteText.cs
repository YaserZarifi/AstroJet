using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D.Script;

public class WriteText : BehaviorScript
{
	protected Color _color;

	protected Vector2 _scale;

	protected string _text;

	public override void RegisterEvents()
	{
		MyObject.OnDraw += OnDraw;
	}

	public override void Init()
	{
		base.Init();
		_text = TextBundle.GetText(ParamString("text", "undefined_text"));
	}

	protected void OnDraw(SpriteBatch spriteBatch)
	{
		string text = _text;
		if (_text.Contains("@"))
		{
			double value = Math.Max(1f, MyObject.GetMainPart().CurrentHP);
			text = _text.Replace("@main_hp", Utils.FormatBigNumber(value));
		}
		GameCore.FontNoBorder.Write(spriteBatch, text, MyObject.WorldPosition.X, MyObject.WorldPosition.Y - (float)GameCore.FontNoBorder.LineHeight * MyObject.WorldScale.Y / 2f, MyObject.WorldScale, MyObject.Color, TextAlign.Center, 0, 1f, -1f, MyObject.PartData.BlendMode);
	}
}
