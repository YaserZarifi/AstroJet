using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public abstract class TextBehavior : BehaviorScript
{
	protected int _lineCount;

	protected Color _color;

	public override void Init()
	{
		base.Init();
		_lineCount = ParamInt("line_count", 1);
	}

	public virtual void SetText()
	{
		for (int i = 0; i < _lineCount; i++)
		{
			string text = TextBundle.GetText(ParamString($"text_{i}", "@#! WTF?"));
			WarningText warningText = MyObject.MyLevel.AddWarningText(text, ParamColor($"color_{i}"), i == 0);
			if (warningText != null)
			{
				warningText.Offset = new Vector2(0f, -20 * _lineCount + i * 40);
				warningText.StartDelay = Param($"delay_{i}") / 1000f;
			}
		}
	}
}
