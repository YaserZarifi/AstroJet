using Microsoft.Xna.Framework;

namespace Rake2D.Editor;

public class LevelScrollBar(Editor editor, int x, int y, int width, int height) : Panel(editor, x, y, width, height)
{
	public float SliderPercentage = 1f;

	public static int SLIDER_WIDTH = 100;

	public override void Update(float elapsedTime)
	{
		if (!IsEnabled || IsOff)
		{
			return;
		}
		SliderPercentage = Utils.Clamp(GameCore.CurrentLevel.Camera.Position.X / (GameCore.CurrentLevel.Size.X - 1920f), 0f, 1f);
		float num = Utils.Clamp((Editor.Cursor.Position.X - Position.X - (float)(SLIDER_WIDTH / 2)) / (float)(Width - SLIDER_WIDTH), 0f, 1f);
		if (Editor.DragAction == DragAction.None && Editor.Cursor.IsInRect(Position, Width, Height))
		{
			Editor.SelectedDragAction = DragAction.LevelScroll;
			if (GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				Editor.DragAction = DragAction.LevelScroll;
			}
		}
		float num2 = (GameCore.CurrentLevel.Size.X - 1920f) * num;
		if (Editor.DragAction == DragAction.LevelScroll || Editor.SelectedDragAction == DragAction.LevelScroll)
		{
			Editor.Cursor.Text = string.Format("Scroll level to {0} ({1})%", (int)num2, (num * 100f).ToString(".##"));
		}
		if (Editor.DragAction == DragAction.LevelScroll)
		{
			GameCore.CurrentLevel.Camera.Position.X = num2;
		}
	}

	public override void Draw()
	{
		if (IsEnabled && !IsOff)
		{
			DrawBegin();
			Rectangle rect = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
			Editor.TriangleBatch.DrawRect(rect, new Color(0, 50, 120, 150));
			float num = Width - SLIDER_WIDTH;
			int x = (int)(Position.X + num * SliderPercentage);
			rect = new Rectangle(x, (int)Position.Y, SLIDER_WIDTH, Height);
			Editor.TriangleBatch.DrawRect(rect, new Color(50, 100, 180, 200));
			DrawEnd();
		}
	}
}
