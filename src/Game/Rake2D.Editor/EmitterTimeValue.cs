using Microsoft.Xna.Framework;

namespace Rake2D.Editor;

public abstract class EmitterTimeValue : Transformation
{
	public bool IsDestroyed;

	public bool IsSelected;

	public Color LineColor = new Color(255, 0, 0, 150);

	public int ValueHeight = 20;

	protected PanelEmitterTime _panel;

	public abstract TimeFunctionValue TimeFunctionValue { get; }

	public Vector2 ScreenPosition => _panel.Position + Position;

	public Editor Editor => _panel.Editor;

	protected EmitterTimeValue(PanelEmitterTime panel)
	{
		_panel = panel;
	}

	public virtual void Update(float elapsedTime)
	{
		Position.X = (float)_panel.TimeAreaRect.X + (float)_panel.TimeAreaRect.Width * TimeFunctionValue.StartTime;
		if (Editor.Cursor.IsInRect(ScreenPosition, 40, ValueHeight))
		{
			IsSelected = true;
		}
		else
		{
			IsSelected = false;
		}
	}

	public virtual void DrawLine()
	{
		Vector2 vector = new Vector2(ScreenPosition.X, _panel.Position.Y + (float)_panel.TimeAreaRect.Y + 20f);
		Editor.LineBatch.DrawLine(vector, new Vector2(ScreenPosition.X, ScreenPosition.Y + (float)ValueHeight), LineColor);
	}

	public virtual void Draw()
	{
	}
}
