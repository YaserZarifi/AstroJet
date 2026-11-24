using Microsoft.Xna.Framework;

namespace Rake2D.Editor;

public class PanelGameObjectDebug : Panel
{
	private const int LINE_H = 12;

	private const int TEXT_X = 4;

	private const int TEXT_Y = 4;

	public PanelGameObjectDebug(Editor editor, int x, int y, int width, int height)
		: base(editor, x, y, width, height)
	{
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
	}

	private int DrawObjectTree(int lineNumber, int treeLevel, GameObject go, float x, float y, int zorder)
	{
		int num = (int)(y + 4f + (float)(lineNumber * 10));
		if (lineNumber < 0 || num > GameCore.MainViewPort.Bounds.Bottom - 30)
		{
			return lineNumber;
		}
		Color color;
		if (go.SpineConnector != null)
		{
			color = ((go.RigidBody == null) ? new Color(50, 190, 200) : new Color(50, 255, 200));
		}
		else
		{
			color = ((go.RigidBody == null) ? Color.LightGray : new Color(50, 255, 50));
		}
		if (go.PartData == null)
		{
			Editor.Font.Write(Editor.SpriteBatch, $"{go.Id} ({go.PartStateId}/{go.PartStateCount}, Ang: {(int)Utils.RadToDeg(go.ParentObject.WorldRotation)}", (int)(x + 4f + (float)(treeLevel * 12)), num, color);
		}
		else
		{
			int num2 = (int)Utils.RadToDeg(go.WorldRotation);
			char c = ((zorder >= 0) ? 'F' : 'B');
			if (go.IsMainPart)
			{
				c = 'M';
			}
			Editor.Font.Write(Editor.SpriteBatch, $"{c}: {go.Id} ({go.PartStateId}/{go.PartStateCount}), T:{go.PartData.SpineTrack}, XY: {Utils.FormatDecimal1(go.Position.X)}, {Utils.FormatDecimal1(go.Position.Y)}, Ang: {num2}, HP: {go.CurrentHP} / {go.MaxHP}", (int)(x + 4f + (float)(treeLevel * 12)), num, color);
		}
		lineNumber++;
		if (go.ChildObjectListFront != null)
		{
			foreach (GameObject item in go.ChildObjectListFront)
			{
				lineNumber = DrawObjectTree(lineNumber + 1, treeLevel + 1, item, x, y, 1);
				if (lineNumber < 0)
				{
					break;
				}
			}
		}
		if (go.ChildObjectListBack != null)
		{
			foreach (GameObject item2 in go.ChildObjectListBack)
			{
				lineNumber = DrawObjectTree(lineNumber + 1, treeLevel + 1, item2, x, y, -1);
				if (lineNumber < 0)
				{
					break;
				}
			}
		}
		return lineNumber;
	}

	public override void Draw()
	{
		base.Draw();
		if (!IsMinimized && IsEnabled && !IsOff)
		{
			DrawBegin();
			GameObject currentObject = Editor.CurrentObject;
			if (currentObject != null && (Editor.CurrentSelectionGroup == null || currentObject != Editor.CurrentSelectionGroup.SelectionObject))
			{
				DrawObjectTree(0, 0, currentObject, Position.X, Position.Y, 0);
			}
			DrawEnd();
		}
	}
}
