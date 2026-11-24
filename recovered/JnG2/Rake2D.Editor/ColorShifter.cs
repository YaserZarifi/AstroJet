using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Rake2D.Editor;

public class ColorShifter : PanelComponent
{
	public delegate void OnValueChangedHandler(ColorShifter shifter);

	public bool IsSelected;

	public Color Value;

	public event OnValueChangedHandler OnValueChanged;

	public ColorShifter(Panel panel)
		: base(null, panel)
	{
		Width = 16;
		Height = 16;
	}

	public override void Draw()
	{
		base.Draw();
		if (IsSelected)
		{
			Editor.ComponentRenderer.Color = Color.Yellow;
		}
		else
		{
			Editor.ComponentRenderer.Color = Color.White;
		}
		Editor.ComponentRenderer.Frame = 0;
		Editor.ComponentRenderer.Draw(Transformation.CreateByPosition(ScreenPosition), Editor.SpriteBatch);
	}

	private void ParseInput()
	{
		if (!IsSelected)
		{
			return;
		}
		int num = 0;
		if (GameInput.SystemKeyPressed(Keys.Add) || GameInput.SystemKeyPressed(Keys.OemPlus))
		{
			num = 1;
		}
		else if (Editor.KeyPressed(Keys.Subtract) || Editor.KeyPressed(Keys.OemMinus))
		{
			num = -1;
		}
		else if (GameInput.SystemKeyPressed(Keys.PageUp))
		{
			num = ((!GameInput.ControlDown) ? 10 : 50);
		}
		else if (GameInput.SystemKeyPressed(Keys.PageDown))
		{
			num = ((!GameInput.ControlDown) ? (-10) : (-50));
		}
		if (GameInput.SystemKeyPressed(Keys.Home))
		{
			num = -1000;
		}
		else if (GameInput.SystemKeyPressed(Keys.End))
		{
			num = 1000;
		}
		else if (GameInput.GetMouseWheelDelta() > 0)
		{
			num = ((!GameInput.ShiftDown) ? 1 : 10);
		}
		else if (GameInput.GetMouseWheelDelta() < 0)
		{
			num = ((!GameInput.ShiftDown) ? (-1) : (-10));
		}
		if (num != 0)
		{
			Editor.FocusComponent = null;
			IncreaseValue(num);
		}
		if (!GameInput.MousePressed(GameInput.MouseButton.Left))
		{
			return;
		}
		SetFocus(bIgnoreFirstInput: false);
		if (GameInput.DoubleClick)
		{
			if (Value.R == byte.MaxValue)
			{
				SetValue(new Color(0, 0, 0));
			}
			else
			{
				SetValue(new Color(255, 255, 255));
			}
		}
	}

	public override void Update(float elapsedTime)
	{
		IsSelected = false;
		IsSelected = Editor.Cursor.IsInRect(ScreenX, ScreenY, Width, Height) && Editor.SelectedPanel == Panel;
		if (IsSelected)
		{
			Editor.SelectedComponent = this;
			Editor.SelectedDragAction = DragAction.OnPanelComponent;
			Editor.Cursor.Text = "Shift color";
			GameInput.MousePressed(GameInput.MouseButton.Left);
		}
		ParseInput();
	}

	public void IncreaseValue(int delta)
	{
		SetValue(new Color(Value.R + delta, Value.G + delta, Value.B + delta));
	}

	public void SetValue(Color color)
	{
		Value = color;
		OnValueChanged?.Invoke(this);
	}
}
