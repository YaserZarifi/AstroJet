using Microsoft.Xna.Framework;

namespace Rake2D.Editor;

public class FieldComponent : PanelComponent
{
	public string Label;

	public bool IsVisible = true;

	public LabelPosition LabelPosition;

	public int LabelWidth;

	public int LabelHeight;

	public Color LabelColor;

	public Color ValueColor;

	public Color ValueColorSelected;

	public bool IsSelected;

	private Color _frameColor;

	public object BindedData;

	public Color FrameColor
	{
		get
		{
			if (_frameColor == Color.TransparentBlack)
			{
				Color result = Panel.BackgroundColor * 1.5f;
				result.A = 200;
				return result;
			}
			return _frameColor;
		}
		set
		{
			_frameColor = value;
		}
	}

	public FieldComponent(SpriteAtlas atlas, Panel panel)
		: base(atlas, panel)
	{
		LabelColor = Color.White;
		ValueColor = Color.LightSteelBlue;
		ValueColorSelected = Color.Yellow;
		LabelWidth = 50;
		IsFocusable = true;
	}

	public FieldComponent(Panel panel)
		: this(null, panel)
	{
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		IsSelected = false;
	}
}
