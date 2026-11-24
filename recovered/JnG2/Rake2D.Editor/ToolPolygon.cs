using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Rake2D.Editor;

public class ToolPolygon
{
	public int SelectedVertexIndex;

	public int CurrentVertexIndex;

	private GameObject _currentObject;

	private GameObject _selectedObject;

	private Editor _editor;

	private LineBatch _lineBatch;

	public ToolPolygon(Editor editor)
	{
		_editor = editor;
		_lineBatch = new LineBatch(GameCore.GraphicsDeviceStatic);
	}

	public void Begin()
	{
		SelectedVertexIndex = -1;
		_selectedObject = null;
	}

	public void Update(GameObject go, float elapsedTime)
	{
		if (go == null || go.PolygonRenderer == null)
		{
			return;
		}
		for (int num = go.PolygonRenderer.VertexList.Count - 1; num >= 0; num--)
		{
			if (num % 2 == 0 && num > 0 && num != go.PolygonRenderer.VertexList.Count - 2 && go.PolygonRenderer.IsTopEdgeOnly)
			{
				go.PolygonRenderer.VertexList[num].Position = go.PolygonRenderer.TopVertexList[num + 1];
			}
			else
			{
				Vector2 value = _editor.CurrentLayer.MyLevel.Camera.CurrentLayerPosToScreen(Vector2.Transform(go.PolygonRenderer.VertexList[num].Position, go.WorldMatrix));
				if (_editor.IsDragAvailable && Vector2.Distance(value, _editor.Cursor.Position) < 8f)
				{
					SelectedVertexIndex = num;
					_selectedObject = go;
					_editor.Cursor.Text = $"Select vertex {num}";
					_editor.SelectedDragAction = DragAction.MoveVertex;
					break;
				}
			}
		}
		if (go == _selectedObject)
		{
			if (GameInput.SystemKeyPressed(Keys.E))
			{
				if (SelectedVertexIndex % 2 == 0)
				{
					go.PolygonRenderer.IsBottomEnabled = !go.PolygonRenderer.IsBottomEnabled;
					if (go.PolygonRenderer.IsBottomEnabled)
					{
						go.PolygonRenderer.RecalculateData();
					}
				}
				else
				{
					go.PolygonRenderer.IsTopEnabled = !go.PolygonRenderer.IsTopEnabled;
					if (go.PolygonRenderer.IsTopEnabled)
					{
						go.PolygonRenderer.RecalculateData();
					}
				}
			}
			if (SelectedVertexIndex >= 0)
			{
				if ((GameInput.SystemKeyPressed(Keys.Delete) || (GameInput.SystemKeyPressed(Keys.V) && GameInput.ControlDown)) && go.PolygonRenderer.VertexList.Count > 4)
				{
					int num2 = SelectedVertexIndex;
					if (num2 % 2 == 0)
					{
						num2++;
					}
					if (num2 > 0 && num2 <= go.PolygonRenderer.VertexList.Count - 1)
					{
						go.PolygonRenderer.RemoveVertex(num2);
						go.PolygonRenderer.RemoveVertex(num2 - 1);
						go.PolygonRenderer.RecalculateData();
					}
				}
				else if (GameInput.SystemKeyPressed(Keys.Insert) || (GameInput.SystemKeyPressed(Keys.V) && !GameInput.ControlDown))
				{
					int num3 = SelectedVertexIndex;
					if (num3 % 2 == 0)
					{
						num3++;
					}
					if (num3 > 0 && num3 < go.PolygonRenderer.VertexList.Count - 1)
					{
						Vector2 position = go.PolygonRenderer.VertexList[num3].Position + (go.PolygonRenderer.VertexList[num3 + 2].Position - go.PolygonRenderer.VertexList[num3].Position) / 2f;
						Vector2 position2 = go.PolygonRenderer.VertexList[num3 - 1].Position + (go.PolygonRenderer.VertexList[num3 + 1].Position - go.PolygonRenderer.VertexList[num3 - 1].Position) / 2f;
						go.PolygonRenderer.AddVertex(position, num3 + 1);
						go.PolygonRenderer.AddVertex(position2, num3 + 1);
						go.PolygonRenderer.RecalculateData();
					}
					else if (num3 >= go.PolygonRenderer.VertexList.Count - 1)
					{
						if (go.PolygonRenderer.VertexList.Count >= 4)
						{
							Vector2 position3 = go.PolygonRenderer.VertexList[num3].Position + (go.PolygonRenderer.VertexList[num3].Position - go.PolygonRenderer.VertexList[num3 - 2].Position);
							Vector2 position4 = go.PolygonRenderer.VertexList[num3 - 1].Position + (go.PolygonRenderer.VertexList[num3 - 1].Position - go.PolygonRenderer.VertexList[num3 - 3].Position);
							go.PolygonRenderer.AddVertex(position4);
							go.PolygonRenderer.AddVertex(position3);
						}
						else
						{
							go.PolygonRenderer.AddVertex(go.PolygonRenderer.VertexList[num3 - 1].Position + new Vector2(100f, 0f));
							go.PolygonRenderer.AddVertex(go.PolygonRenderer.VertexList[num3].Position + new Vector2(100f, 0f));
						}
						go.PolygonRenderer.RecalculateData();
					}
				}
			}
		}
		if (GameInput.MouseDown(GameInput.MouseButton.Left))
		{
			if (go == _selectedObject && SelectedVertexIndex >= 0 && GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				CurrentVertexIndex = SelectedVertexIndex;
				_currentObject = go;
			}
		}
		else
		{
			CurrentVertexIndex = -1;
		}
		if (CurrentVertexIndex >= 0 && go == _currentObject)
		{
			_editor.DragAction = DragAction.MoveVertex;
			Vector2 deltaPos = _currentObject.GetRelativePosition(_editor.Cursor.GetCurrentLayerPosition()) - _currentObject.PolygonRenderer.VertexList[CurrentVertexIndex].Position;
			if (GameInput.ShiftDown)
			{
				if (CurrentVertexIndex % 2 == 0)
				{
					_editor.Cursor.Text = $"Move top line by vertex {CurrentVertexIndex}";
					for (int i = 0; i < _currentObject.PolygonRenderer.VertexList.Count; i += 2)
					{
						_currentObject.PolygonRenderer.VertexList[i].ChangePosition(deltaPos);
					}
				}
				else
				{
					_editor.Cursor.Text = $"Move bottom line by vertex {CurrentVertexIndex}";
					for (int j = 1; j < _currentObject.PolygonRenderer.VertexList.Count; j += 2)
					{
						_currentObject.PolygonRenderer.VertexList[j].ChangePosition(deltaPos);
					}
				}
			}
			else if (GameInput.ControlDown)
			{
				_editor.Cursor.Text = $"Move vertex pair {CurrentVertexIndex}";
				if (CurrentVertexIndex % 2 == 0)
				{
					_currentObject.PolygonRenderer.VertexList[CurrentVertexIndex + 1].ChangePosition(deltaPos);
				}
				else
				{
					_currentObject.PolygonRenderer.VertexList[CurrentVertexIndex - 1].ChangePosition(deltaPos);
				}
				_currentObject.PolygonRenderer.VertexList[CurrentVertexIndex].ChangePosition(deltaPos);
			}
			else
			{
				_editor.Cursor.Text = $"Move vertex {CurrentVertexIndex}";
				_currentObject.PolygonRenderer.VertexList[CurrentVertexIndex].ChangePosition(deltaPos);
			}
			go.PolygonRenderer.RecalculateData();
			go.UpdateVisibilityRect();
			go.CreateRigidBodyByPoygonRenderer();
			go.MyLayer.MyLevel.PhysWorld.ProcessChanges();
		}
		_ = go.PolygonRenderer.IsTopEdgeOnly;
	}

	public void Draw(GameObject go)
	{
		if (go == null || go.PolygonRenderer == null)
		{
			return;
		}
		_editor.ObjectTransformGizmoRenderer.Frame = 5;
		if (!_editor.MinimalGizmoMode)
		{
			_lineBatch.Begin(BlendState.Additive, Matrix.Identity);
			for (int i = 0; i < go.PolygonRenderer.VertexList.Count; i++)
			{
				if (i % 2 == 0 && i > 0 && i != go.PolygonRenderer.VertexList.Count - 2 && go.PolygonRenderer.IsTopEdgeOnly)
				{
					continue;
				}
				Vector2 position = go.PolygonRenderer.VertexList[i].Position;
				Vector2 position2 = ((i >= go.PolygonRenderer.VertexList.Count - 1) ? position : go.PolygonRenderer.VertexList[i + 1].Position);
				Vector2 vector = _editor.CurrentLayer.MyLevel.Camera.CurrentLayerPosToScreen(Vector2.Transform(position, go.WorldMatrix));
				Vector2 to = _editor.CurrentLayer.MyLevel.Camera.CurrentLayerPosToScreen(Vector2.Transform(position2, go.WorldMatrix));
				Color color;
				if (i % 2 != 0)
				{
					color = new Color(200, 200, 200, 50);
				}
				else if (i != 0)
				{
					if (i == go.PolygonRenderer.VertexList.Count - 2)
					{
						color = Color.Red;
					}
					else
					{
						color = ((go.PolygonRenderer.TextureMappingMode != PolygonTextureMappingModeEnum.RelativeToPivot) ? new Color(255, 220, 50, 200) : new Color(100, 100, 255, 200));
					}
				}
				else
				{
					color = Color.Green;
				}
				if (!go.PolygonRenderer.IsTopEdgeOnly || i == 0 || i == go.PolygonRenderer.VertexList.Count - 2)
				{
					_lineBatch.DrawLine(vector, to, color);
				}
				position2 = ((i >= go.PolygonRenderer.VertexList.Count - 2) ? position : go.PolygonRenderer.VertexList[i + 2].Position);
				to = _editor.CurrentLayer.MyLevel.Camera.CurrentLayerPosToScreen(Vector2.Transform(position2, go.WorldMatrix));
				if (go.PolygonRenderer.IsTopEdgeOnly)
				{
					if (i == 0)
					{
						continue;
					}
					color = new Color(255, 100, 0, 150);
				}
				else
				{
					color = new Color(200, 200, 200, 50);
				}
				_lineBatch.DrawLine(vector, to, color);
			}
			_lineBatch.End();
		}
		_editor.SpriteDrawEnd();
		_editor.SpriteDrawBegin();
		for (int j = 0; j < go.PolygonRenderer.VertexList.Count; j++)
		{
			if (j % 2 != 0 || j <= 0 || j == go.PolygonRenderer.VertexList.Count - 2 || !go.PolygonRenderer.IsTopEdgeOnly)
			{
				Vector2 position3 = go.PolygonRenderer.VertexList[j].Position;
				Vector2 pos = _editor.CurrentLayer.MyLevel.Camera.CurrentLayerPosToScreen(Vector2.Transform(position3, go.WorldMatrix));
				if (_currentObject == go && CurrentVertexIndex == j)
				{
					_editor.ObjectTransformGizmoRenderer.Color = Color.Green;
				}
				else if (_selectedObject == go && SelectedVertexIndex == j)
				{
					_editor.ObjectTransformGizmoRenderer.Color = Color.Yellow;
				}
				else if (j == 0)
				{
					_editor.ObjectTransformGizmoRenderer.Color = new Color(0, 200, 0);
				}
				else if (j == go.PolygonRenderer.VertexList.Count - 2)
				{
					_editor.ObjectTransformGizmoRenderer.Color = new Color(200, 0, 0);
				}
				else
				{
					_editor.ObjectTransformGizmoRenderer.Color = Color.Gray;
				}
				_editor.ObjectTransformGizmoRenderer.Draw(Transformation.CreateByPosition(pos), _editor.SpriteBatch, BlendModeEnum.Additive);
			}
		}
	}
}
