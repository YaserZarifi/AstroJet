using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D.Editor;

internal class Checkbox : FieldComponent
{
	public delegate void OnClickHandler(Checkbox button);

	public delegate void OnRightClickHandler(Checkbox button);

	public bool IsChecked = true;

	public event OnClickHandler OnClick;

	public event OnClickHandler OnDoubleClick;

	public event OnRightClickHandler OnRightClick;

	public Checkbox(Panel panel)
		: this(null, panel)
	{
	}

	public Checkbox(SpriteAtlas atlas, Panel panel)
		: base(atlas, panel)
	{
		LabelWidth = 0;
		Width = 20;
		Height = 20;
		FrameColor = Color.TransparentBlack;
		ForegroundColor = Color.LightBlue;
		ValueColor = Color.White;
		ValueColorSelected = Color.Yellow;
	}

	public override void Update(float elapsedTime)
	{
		if (!IsVisible)
		{
			return;
		}
		base.Update(elapsedTime);
		if (Editor.SelectedPanel != Panel || !Panel.Editor.Cursor.IsInRect(ScreenPosition, Width, Height))
		{
			return;
		}
		IsSelected = true;
		Editor.Cursor.Text = Tooltip ?? "Toggle check box";
		Editor.SelectedDragAction = DragAction.OnPanelComponent;
		if (GameInput.MousePressed(GameInput.MouseButton.Right))
		{
			OnRightClick?.Invoke(this);
		}
		else if (GameInput.MousePressed(GameInput.MouseButton.Left))
		{
			Editor.DragAction = DragAction.OnPanelComponent;
			SetFocus(bIgnoreFirstInput: true);
			if (GameInput.DoubleClick)
			{
				OnDoubleClick?.Invoke(this);
				return;
			}
			IsChecked = !IsChecked;
			OnClick?.Invoke(this);
		}
	}

	public override void Draw()
	{
		if (IsVisible)
		{
			Vector2 screenPosition = ScreenPosition;
			if (LabelPosition == LabelPosition.Left)
			{
				screenPosition.X += LabelWidth;
			}
			else
			{
				screenPosition.Y += LabelHeight + 1;
			}
			Editor.LineBatch.Begin(BlendState.AlphaBlend, Matrix.Identity);
			Editor.LineBatch.DrawRect(screenPosition, Width, Height, (FrameColor == Color.TransparentBlack) ? (BackgroundColor * 1.5f) : FrameColor, RectangleFrameType.Lowered);
			Editor.LineBatch.End();
			if (IsChecked)
			{
				Editor.TriangleBatch.Begin(BlendState.AlphaBlend, Matrix.Identity);
				Color color;
				if (IsChecked)
				{
					color = (IsSelected ? Color.Yellow : ForegroundColor);
				}
				else
				{
					color = (IsSelected ? new Color(150, 150, 0) : Color.Black);
				}
				Editor.TriangleBatch.DrawRect(screenPosition + new Vector2(4f), screenPosition + new Vector2(Width - 4, Height - 4), color);
				Editor.TriangleBatch.End();
			}
		}
	}
}
