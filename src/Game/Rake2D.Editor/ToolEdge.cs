using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Rake2D.Editor;

public class ToolEdge
{
	public List<EditorVertex> VertexList;

	public EditorVertex SelectedVertex;

	public EditorVertex CurrentVertex;

	public GameObject MyObject;

	private Editor _editor;

	private LineBatch _lineBatch;

	public ToolEdge(Editor editor, GameObject gameObject)
	{
		_editor = editor;
		_lineBatch = new LineBatch(GameCore.GraphicsDeviceStatic);
		MyObject = gameObject;
		VertexList = new List<EditorVertex>();
	}

	public void AddVertex(Vector2 position, bool toFirst)
	{
		EditorVertex item = new EditorVertex(position);
		if (toFirst)
		{
			VertexList.Insert(0, item);
		}
		else
		{
			VertexList.Add(item);
		}
	}

	public void BuildEdeBody()
	{
		if (MyObject.RigidBody == null)
		{
			MyObject.CreateRigidBody();
		}
		List<Vector2> list = new List<Vector2>();
		foreach (EditorVertex vertex in VertexList)
		{
			list.Add(vertex.Position);
		}
		MyObject.RigidBody.BuildEdgeBody(list);
	}

	public void BuildVerticesFromEdgeBody(RigidBody body)
	{
		VertexList.Clear();
		if (body == null)
		{
			return;
		}
		List<Vector2> edgeVertexList = body.GetEdgeVertexList();
		if (edgeVertexList == null)
		{
			return;
		}
		foreach (Vector2 item in edgeVertexList)
		{
			AddVertex(MyObject.GetRelativePosition(item), toFirst: false);
		}
	}

	public void Update(float elapsedTime)
	{
		Vector2 currentLayerPosition = _editor.Cursor.GetCurrentLayerPosition();
		SelectedVertex = null;
		for (int num = VertexList.Count - 1; num >= 0; num--)
		{
			EditorVertex editorVertex = VertexList[num];
			if (Vector2.Distance(editorVertex.GetWorldPosition(MyObject.WorldMatrix), currentLayerPosition) < 6f)
			{
				SelectedVertex = editorVertex;
				_editor.Cursor.Text = "Select vertex";
				if (GameInput.SystemKeyPressed(Keys.Delete))
				{
					VertexList.RemoveAt(num);
				}
				break;
			}
		}
		if (GameInput.MouseDown(GameInput.MouseButton.Left))
		{
			if (SelectedVertex != null && GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				CurrentVertex = SelectedVertex;
			}
		}
		else
		{
			CurrentVertex = null;
		}
		if (CurrentVertex != null)
		{
			CurrentVertex.Position = MyObject.GetRelativePosition(currentLayerPosition);
			_editor.DragAction = DragAction.PaintEdge;
		}
		if (!GameInput.ControlDown && GameInput.SystemKeyPressed(Keys.Space))
		{
			_editor.SetTool(EditorToolEnum.Object);
			BuildEdeBody();
			MyObject.HasEdge = true;
			MyObject.UpdateVisibilityRect();
			_editor.CurrentObject = MyObject;
			GameCore.CurrentLevel.PhysWorld.ProcessChanges();
		}
		if (_editor.Cursor.Text == null)
		{
			_editor.Cursor.Text = "Add edge vertex";
		}
		if (_editor.IsDragAvailable && GameInput.MousePressed(GameInput.MouseButton.Left))
		{
			AddVertex(MyObject.GetRelativePosition(currentLayerPosition), GameInput.ControlDown);
			_editor.DragAction = DragAction.PaintEdge;
		}
		if (_editor.SelectedDragAction == DragAction.None)
		{
			_editor.SelectedDragAction = DragAction.PaintEdge;
		}
	}

	public void Draw()
	{
		EditorVertex editorVertex = null;
		_editor.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, GameCore.LevelMatrix);
		_editor.ObjectTransformGizmoRenderer.Frame = 1;
		_lineBatch.Begin(BlendState.Additive, GameCore.ViewPortMatrix);
		int num = 0;
		foreach (EditorVertex vertex in VertexList)
		{
			num++;
			Vector2 worldPosition = vertex.GetWorldPosition(MyObject.WorldMatrix);
			if (editorVertex != null)
			{
				_lineBatch.DrawLine(editorVertex.GetWorldPosition(MyObject.WorldMatrix), worldPosition, Color.Yellow);
			}
			if ((num == VertexList.Count && !GameInput.ControlDown) || (num == 1 && GameInput.ControlDown))
			{
				_editor.ObjectTransformGizmoRenderer.Color = Color.Green;
			}
			else if (SelectedVertex == vertex)
			{
				_editor.ObjectTransformGizmoRenderer.Color = Color.White;
			}
			else
			{
				_editor.ObjectTransformGizmoRenderer.Color = Color.Gray;
			}
			_editor.ObjectTransformGizmoRenderer.Draw(Transformation.CreateByPosition(worldPosition), _editor.SpriteBatch);
			editorVertex = vertex;
		}
		_editor.SpriteDrawEnd();
		_lineBatch.End();
	}
}
