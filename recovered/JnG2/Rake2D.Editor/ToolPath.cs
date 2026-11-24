using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Rake2D.Script;

namespace Rake2D.Editor;

public class ToolPath
{
	public ToolPathModeEnum Mode;

	public BezierPath CurrentPath;

	public BehaviorEventNode DraggedEventNode;

	public BehaviorEventNode SelectedEventNode;

	public BehaviorEvent SelectedAngleEvent;

	public BehaviorEvent DraggedAngleEvent;

	private float _nearestNodeTime;

	private int _selectedPathControlPoint;

	private int _draggedPathControlPoint;

	private Editor _editor;

	private Vector2 _cursorWorldPos;

	private SpriteRenderer _pathSpriteRenderer;

	private SpriteBatch _spriteBatch;

	public BezierPath SelectedPath { get; private set; }

	public ToolPath(Editor editor)
	{
		_editor = editor;
		_spriteBatch = new SpriteBatch(GameCore.GraphicsDeviceStatic);
		SpriteAtlas spriteAtlas = GameCore.GlobalContent.GetSpriteAtlas("editor_bezier_path");
		_pathSpriteRenderer = new SpriteRenderer(spriteAtlas);
	}

	public void ToggleMode()
	{
		if (Mode == ToolPathModeEnum.ControlPoints)
		{
			Mode = ToolPathModeEnum.Events;
		}
		else
		{
			Mode = ToolPathModeEnum.ControlPoints;
		}
	}

	private void GetPathSelected(BezierPath path)
	{
		_selectedPathControlPoint = -1;
		if (Mode == ToolPathModeEnum.Events || Mode == ToolPathModeEnum.ControlPointsAndEvents)
		{
			foreach (BehaviorEventNode eventNode in path.EventNodeList)
			{
				Vector2 position = path.GetPosition(eventNode.Time);
				Vector2 vector = GameCore.CurrentLevel.Camera.CurrentLayerPosToScreen(position);
				if (eventNode.TimeLine != null)
				{
					_editor.ToolTimeLine.UpdateTimeLine(vector, eventNode.TimeLine, 0f, Vector2.Zero);
				}
				UpdateNodeAngleControl(eventNode, vector);
				if (_editor.SelectedDragAction != DragAction.MoveTimeNode && _editor.DragAction != DragAction.MoveTimeNode && _editor.Cursor.IsInCircle(vector, 12))
				{
					SelectedEventNode = eventNode;
					_editor.SetSelectedEventNode(eventNode);
					SelectedPath = path;
					_editor.Cursor.Text = $"Select node ({Utils.FormatFloat2(SelectedEventNode.Time * 100f)} %)";
					_selectedPathControlPoint = -1;
					_editor.SelectedDragAction = DragAction.MovePathNode;
					if (GameInput.MousePressed(GameInput.MouseButton.Left))
					{
						SetCurrentPathEventNode(path, eventNode);
						_editor.DragOffset = position - _editor.CursorWorldPos;
						_editor.DragAction = DragAction.MovePathNode;
					}
				}
			}
			if (SelectedAngleEvent != null || DraggedAngleEvent != null)
			{
				BehaviorEvent behaviorEvent = SelectedAngleEvent ?? DraggedAngleEvent;
				_editor.Cursor.Text = $"Angle: {behaviorEvent.ValueInt}";
			}
		}
		if (Mode != ToolPathModeEnum.ControlPoints && Mode != ToolPathModeEnum.ControlPointsAndEvents)
		{
			return;
		}
		for (int i = 0; i < path.ControlPointList.Count; i++)
		{
			PathPointType pointType = path.GetPointType(i);
			if (!_editor.IsDragAvailable || !_editor.Cursor.IsInCircle(GameCore.CurrentLevel.Camera.CurrentLayerPosToScreen(path.ControlPointList[i]), 24) || (pointType != PathPointType.EndPoint && path != CurrentPath))
			{
				continue;
			}
			SelectedPath = path;
			_selectedPathControlPoint = i;
			_editor.Cursor.Text = $"Move[{i}] {Utils.FormatFloat2(path.ControlPointList[i].X)}, {Utils.FormatFloat2(path.ControlPointList[i].Y)}";
			if (GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				_editor.SetDragAction(DragAction.MovePathPoint);
				SetCurrentPathEventNode(path, null);
				_editor.DragOffset = path.ControlPointList[i] - _cursorWorldPos;
				_draggedPathControlPoint = i;
			}
			if (GameInput.SystemKeyPressed(Keys.Delete))
			{
				if (GameInput.ControlDown)
				{
					path.Clear();
				}
				else
				{
					path.RemoveCurve(i / 3);
				}
				_editor.DeleteObjectAllowed = false;
			}
			_editor.SelectedDragAction = DragAction.MovePathPoint;
		}
	}

	public bool ParseLinkInput()
	{
		if (_editor.CurrentEventNode == null || _editor.SelectedObject == null)
		{
			return false;
		}
		if (_editor.CurrentEventNode.LinkedObjectList != null && _editor.CurrentEventNode.LinkedObjectList.Contains(_editor.SelectedObject))
		{
			_editor.CurrentEventNode.RemoveLinkedbject(_editor.SelectedObject);
		}
		else
		{
			_editor.CurrentEventNode.AddLinkedbject(_editor.SelectedObject);
		}
		return true;
	}

	public void SetCurrentPathEventNode(BezierPath path, BehaviorEventNode node)
	{
		CurrentPath = path;
		DraggedEventNode = node;
		if (CurrentPath != null)
		{
			_editor.SetCurrentObject(CurrentPath.MyObject);
		}
		_editor.SetCurretEventNode(node);
	}

	private void MoveEndPoint(BezierPath path, int pointNumber, Vector2 offset)
	{
		for (int i = pointNumber - 1; i <= pointNumber + 1; i++)
		{
			if (i >= 0)
			{
				if (i >= path.ControlPointList.Count)
				{
					break;
				}
				path.ControlPointList[i] = Vector2.Transform(path.ControlPointList[i], Matrix.CreateTranslation(offset.X, offset.Y, 0f));
			}
		}
		path.CalculateLength();
	}

	private void UpdatePaths()
	{
		SelectedPath = null;
		SelectedEventNode = null;
		SelectedAngleEvent = null;
		if (_editor.CurrentLayer == null || _editor.SelectedDragAction == DragAction.OnPanel || _editor.SelectedDragAction == DragAction.MoveTimeNode)
		{
			return;
		}
		if (_editor.IsShowPathEnabled)
		{
			foreach (GameObject gameObject in _editor.CurrentLayer.GameObjectList)
			{
				if (gameObject.MyPath == null)
				{
					continue;
				}
				GetPathSelected(gameObject.MyPath);
				if (SelectedPath == gameObject.MyPath)
				{
					if (GameInput.MousePressed(GameInput.MouseButton.Left))
					{
						_editor.SetCurrentObject(gameObject);
					}
					break;
				}
			}
		}
		else if (_editor.CurrentObject != null && _editor.CurrentObject.MyPath != null)
		{
			GetPathSelected(_editor.CurrentObject.MyPath);
			if (SelectedPath == _editor.CurrentObject.MyPath && GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				_editor.SetCurrentObject(_editor.CurrentObject);
			}
		}
		if (SelectedPath != null && SelectedPath.ControlPointList.Count == 0)
		{
			if (SelectedPath == CurrentPath)
			{
				CurrentPath = null;
			}
			if (SelectedPath.MyObject != null)
			{
				SelectedPath.MyObject.MyPath = null;
			}
			SelectedPath = null;
		}
		if (CurrentPath != null && CurrentPath.ControlPointList.Count == 0)
		{
			if (CurrentPath.MyObject != null)
			{
				CurrentPath.MyObject.MyPath = null;
			}
			CurrentPath = null;
		}
		if (GameInput.MousePressed(GameInput.MouseButton.Right) && _editor.CurrentEventNode != null)
		{
			SetCurrentPathEventNode(CurrentPath, null);
			GameInput.CancelMousePressed();
		}
		if (_editor.CurrentObject != null && _editor.IsDragAvailable)
		{
			if (_editor.IsDragAvailable && _editor.ToolTimeLine.CurrentTimeLine == null && SelectedAngleEvent == null)
			{
				_editor.Cursor.Text = "<INS> to add path node";
			}
			if (GameInput.SystemKeyPressed(Keys.Insert))
			{
				if (Mode == ToolPathModeEnum.Events)
				{
					Mode = ToolPathModeEnum.ControlPoints;
				}
				if (_editor.CurrentObject.MyPath == null)
				{
					CurrentPath = _editor.CurrentObject.CreatePath();
					_editor.CurrentObject.MyPath = CurrentPath;
				}
				if (_nearestNodeTime > 0f)
				{
					CurrentPath.SplitAt(_nearestNodeTime);
				}
				else
				{
					CurrentPath.AddCurve(_cursorWorldPos, recalculate: true, _editor.CurrentObject.Rotation);
				}
			}
		}
		if (GameInput.MouseDown(GameInput.MouseButton.Left))
		{
			if (CurrentPath != null && _editor.DragAction == DragAction.MovePathPoint)
			{
				Vector2 vector = _cursorWorldPos - CurrentPath.ControlPointList[_draggedPathControlPoint] + _editor.DragOffset;
				if (GameInput.ShiftDown)
				{
					if (vector != Vector2.Zero)
					{
						CurrentPath.Move(vector);
					}
				}
				else
				{
					PathPointType pointType = CurrentPath.GetPointType(_draggedPathControlPoint);
					if (pointType == PathPointType.EndPoint && !GameInput.ControlDown)
					{
						MoveEndPoint(CurrentPath, _draggedPathControlPoint, vector);
					}
					else if (!GameInput.AltDown)
					{
						int num = -1;
						Vector2 vector2;
						if (pointType == PathPointType.ControlPoint1 && _draggedPathControlPoint >= 2)
						{
							num = _draggedPathControlPoint - 2;
							vector2 = CurrentPath.ControlPointList[_draggedPathControlPoint - 1];
						}
						else if (pointType == PathPointType.ControlPoint2 && _draggedPathControlPoint < CurrentPath.ControlPointList.Count - 2)
						{
							num = _draggedPathControlPoint + 2;
							vector2 = CurrentPath.ControlPointList[_draggedPathControlPoint + 1];
						}
						else
						{
							vector2 = Vector2.Zero;
							CurrentPath.ControlPointList[_draggedPathControlPoint] = _cursorWorldPos + _editor.DragOffset;
						}
						if (num >= 0)
						{
							Vector2 vector3 = vector2 - CurrentPath.ControlPointList[_draggedPathControlPoint];
							float num2 = (vector2 - CurrentPath.ControlPointList[num]).Length() / vector3.Length();
							CurrentPath.ControlPointList[_draggedPathControlPoint] = _cursorWorldPos + _editor.DragOffset;
							if (CurrentPath.ControlPointList[_draggedPathControlPoint] == vector2)
							{
								CurrentPath.ControlPointList[_draggedPathControlPoint] += new Vector2(0.01f, 0.01f);
							}
							vector3 = vector2 - CurrentPath.ControlPointList[_draggedPathControlPoint];
							if (GameInput.ControlDown)
							{
								float num3 = (vector2 - CurrentPath.ControlPointList[num]).Length();
								vector3.Normalize();
								Vector2 value = vector2 + vector3 * num3;
								CurrentPath.ControlPointList[num] = value;
							}
							else
							{
								CurrentPath.ControlPointList[num] = vector2 + vector3 * num2;
							}
						}
					}
					else
					{
						CurrentPath.ControlPointList[_draggedPathControlPoint] = _cursorWorldPos + _editor.DragOffset;
					}
				}
			}
		}
		else
		{
			_draggedPathControlPoint = -1;
		}
		if (SelectedEventNode == null && _editor.ToolTimeLine.SelectedEventNode == null)
		{
			_editor.SetSelectedEventNode(null);
		}
	}

	public void ParseInput()
	{
		if (SelectedEventNode != null && GameInput.SystemKeyPressed(Keys.Delete))
		{
			(SelectedPath ?? CurrentPath).EventNodeList.Remove(SelectedEventNode);
			if (_editor.CurrentEventNode == SelectedEventNode)
			{
				SetCurrentPathEventNode(CurrentPath, null);
			}
			SelectedEventNode = null;
			_editor.SetSelectedEventNode(null);
			_editor.DeleteObjectAllowed = false;
		}
		if (CurrentPath == null || CurrentPath.EventNodeList.IsNullOrEmpty() || (_editor.CurrentEventNode != null && _editor.CurrentEventNode.IsOnTimeLine))
		{
			return;
		}
		if (_editor.KeyPressed(Keys.OemComma))
		{
			_editor.ToolTimeLine.SetCurrentEventNode(null);
			if (_editor.CurrentEventNode == null)
			{
				_editor.SetCurretEventNode(CurrentPath.EventNodeList[CurrentPath.EventNodeList.Count - 1]);
			}
			else
			{
				int num = CurrentPath.EventNodeList.IndexOf(_editor.CurrentEventNode);
				if (num <= 0)
				{
					_editor.SetCurretEventNode(CurrentPath.EventNodeList[CurrentPath.EventNodeList.Count - 1]);
				}
				else
				{
					_editor.SetCurretEventNode(CurrentPath.EventNodeList[num - 1]);
				}
			}
		}
		if (!_editor.KeyPressed(Keys.OemPeriod))
		{
			return;
		}
		_editor.ToolTimeLine.SetCurrentEventNode(null);
		if (_editor.CurrentEventNode == null)
		{
			_editor.SetCurretEventNode(CurrentPath.EventNodeList[0]);
			return;
		}
		int num2 = CurrentPath.EventNodeList.IndexOf(_editor.CurrentEventNode);
		if (num2 >= CurrentPath.EventNodeList.Count - 1)
		{
			_editor.SetCurretEventNode(CurrentPath.EventNodeList[0]);
		}
		else
		{
			_editor.SetCurretEventNode(CurrentPath.EventNodeList[num2 + 1]);
		}
	}

	public void Update(float elapsedTime)
	{
		_cursorWorldPos = _editor.Cursor.GetCurrentLayerPosition();
		UpdatePaths();
		if (CurrentPath != null && _nearestNodeTime >= 0f)
		{
			_editor.Cursor.Text = $"Path pos: {Utils.FormatDecimal1(_nearestNodeTime * 100f)} %";
			if (DraggedEventNode != null)
			{
				DraggedEventNode.Time = _nearestNodeTime;
			}
			if ((_editor.IsDragAvailable || _editor.SelectedDragAction == DragAction.MovePathPoint) && GameInput.SystemKeyPressed(Keys.E))
			{
				if (Mode == ToolPathModeEnum.ControlPoints)
				{
					Mode = ToolPathModeEnum.Events;
				}
				BehaviorEventNode behaviorEventNode = CurrentPath.AddEventNode(_nearestNodeTime);
				behaviorEventNode.SetObject(_editor.CurrentObject);
				CurrentPath.RefreshEventNodes();
				SetCurrentPathEventNode(CurrentPath, behaviorEventNode);
			}
		}
		if (!GameInput.MouseDown(GameInput.MouseButton.Left))
		{
			DraggedEventNode = null;
			DraggedAngleEvent = null;
		}
		if (DraggedEventNode != null)
		{
			_editor.Cursor.Text = $"Move node ({Utils.FormatDecimal1(DraggedEventNode.Time * 100f)} %)";
		}
		if (_editor.SelectedDragAction != DragAction.OnPanel && _editor.SelectedDragAction != DragAction.OnPanelComponent && _editor.SelectedDragAction != DragAction.MoveTimeNode && _editor.DragAction != DragAction.MoveTimeNode && _nearestNodeTime >= 0f)
		{
			_editor.SelectedDragAction = DragAction.PathManipulation;
		}
		_nearestNodeTime = -1f;
	}

	public void DrawTriggerPoint(GameObject go)
	{
		if (!(go.TriggerPoint == Vector2.Zero) && (GameCore.IsEditorEnabled || !go.IsTriggerFree))
		{
			_editor.LineBatch.Begin(BlendState.AlphaBlend, Matrix.Identity);
			Vector2 vector = go.MyLevel.Camera.CurrentLayerPosToScreen(go.TriggerPoint);
			_editor.LineBatch.DrawLine(go.MyLevel.Camera.CurrentLayerPosToScreen(go.WorldTransformation.Position), vector, new Color(0, 200, 0, 100));
			_editor.LineBatch.DrawLine(vector + new Vector2(0f, 50f), vector + new Vector2(0f, -50f), new Color(0, 200, 0, 90));
			Transformation transformation = Transformation.CreateByPosition(vector);
			if (go == _editor.SelectedTriggerPointObject && _editor.DragAction == DragAction.MoveTriggerPoint)
			{
				_pathSpriteRenderer.Color = new Color(255, 50, 240);
				transformation.Scale *= 1.25f;
			}
			else if (go == _editor.SelectedTriggerPointObject && _editor.SelectedDragAction == DragAction.MoveTriggerPoint)
			{
				_pathSpriteRenderer.Color = new Color(255, 200, 0);
				transformation.Scale *= 1.5f;
			}
			else
			{
				_pathSpriteRenderer.Color = new Color(0, 255, 20);
			}
			_pathSpriteRenderer.Frame = 0;
			_editor.LineBatch.End();
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
			_pathSpriteRenderer.Draw(transformation, _spriteBatch, BlendModeEnum.Normal);
			_spriteBatch.End();
		}
	}

	private void UpdateNodeAngleControl(BehaviorEventNode node, Vector2 nodePos)
	{
		BehaviorEvent behaviorEvent = node.EventList.FindAll((BehaviorEvent o) => Utils.Equals(o.BehaviorId, "PathRider")).Find((BehaviorEvent p) => Utils.Equals(p.Name, "angle") && p.ValueInt != -999);
		if (behaviorEvent == null)
		{
			return;
		}
		float rotation = Utils.DegToRad(behaviorEvent.ValueInt);
		Vector2 pos = nodePos + Utils.GetVelocityVector2(100f * GameCore.CurrentLevel.Camera.Zoom, rotation);
		if (_editor.Cursor.IsInCircle(pos, 20))
		{
			SelectedAngleEvent = behaviorEvent;
			if (GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				DraggedAngleEvent = behaviorEvent;
			}
		}
		if (DraggedAngleEvent == behaviorEvent)
		{
			behaviorEvent.ValueInt = (int)Utils.RadToDeg(Utils.GetRotationTo(nodePos, _editor.Cursor.Position));
		}
	}

	private void DrawNodeAngleControl(BehaviorEventNode node, Vector2 nodePos)
	{
		BehaviorEvent behaviorEvent = node.EventList.FindAll((BehaviorEvent o) => Utils.Equals(o.BehaviorId, "PathRider")).Find((BehaviorEvent p) => Utils.Equals(p.Name, "angle") && p.ValueInt != -999);
		if (behaviorEvent != null)
		{
			float rotation = Utils.DegToRad(behaviorEvent.ValueInt);
			_editor.LineBatch.Begin(BlendState.NonPremultiplied, Matrix.Identity);
			Vector2 vector = nodePos + Utils.GetVelocityVector2(100f * GameCore.CurrentLevel.Camera.Zoom, rotation);
			Color color;
			Color color2;
			if (DraggedAngleEvent == behaviorEvent)
			{
				color = new Color(255, 100, 0, 200);
				color2 = new Color(255, 100, 0);
			}
			else if (SelectedAngleEvent == behaviorEvent)
			{
				color = new Color(255, 255, 50, 200);
				color2 = new Color(255, 255, 50);
			}
			else
			{
				color = new Color(50, 90, 200, 200);
				color2 = new Color(100, 200, 255);
			}
			_editor.LineBatch.DrawLine(nodePos, vector, color);
			_editor.LineBatch.End();
			Transformation transformation = Transformation.CreateByPosition(vector);
			transformation.Rotation = rotation;
			_pathSpriteRenderer.Frame = 5;
			_pathSpriteRenderer.Color = color2;
			_pathSpriteRenderer.Draw(transformation, _spriteBatch, BlendModeEnum.Normal);
		}
	}

	private void DrawEventNodes(BezierPath path)
	{
		foreach (BehaviorEventNode eventNode in path.EventNodeList)
		{
			Vector2 position = path.GetPosition(eventNode.Time);
			position += path.MyObject.PathOscilation;
			if (!path.MyObject.MyCamera.IsPointOnScreen(position))
			{
				continue;
			}
			Vector2 vector = GameCore.CurrentLevel.Camera.CurrentLayerPosToScreen(position);
			Matrix matrix = Matrix.CreateTranslation(vector.X, vector.Y, 0f);
			if (eventNode.ErrorCount > 0)
			{
				_pathSpriteRenderer.Color = new Color(255, 0, 0, 255);
				matrix = Matrix.CreateScale(1.2f) * matrix;
			}
			else if (DraggedEventNode == eventNode)
			{
				if (eventNode.Time == 0f)
				{
					_pathSpriteRenderer.Color = new Color(240, 255, 180, 255);
					matrix = Matrix.CreateScale(1.75f) * matrix;
				}
				else
				{
					_pathSpriteRenderer.Color = new Color(70, 255, 180, 255);
					matrix = Matrix.CreateScale(1.5f) * matrix;
				}
			}
			else if (SelectedEventNode == eventNode)
			{
				if (eventNode.Time == 0f)
				{
					_pathSpriteRenderer.Color = new Color(255, 255, 150, 255);
				}
				else
				{
					_pathSpriteRenderer.Color = new Color(0, 255, 255, 230);
				}
				matrix = Matrix.CreateScale(1.5f) * matrix;
			}
			else if (eventNode == _editor.CurrentEventNode)
			{
				if (eventNode.Time == 0f)
				{
					_pathSpriteRenderer.Color = new Color(255, 240, 160, 255);
					matrix = Matrix.CreateScale(1.4f) * matrix;
				}
				else
				{
					_pathSpriteRenderer.Color = new Color(70, 240, 200, 255);
					matrix = Matrix.CreateScale(1.25f) * matrix;
				}
			}
			else if (eventNode.Time <= path.MyObject.PathTime && !GameCore.IsEditorEnabled)
			{
				_pathSpriteRenderer.Color = new Color(100, 100, 100, 150);
			}
			else if (eventNode.Time == 0f)
			{
				_pathSpriteRenderer.Color = new Color(255, 255, 0, 255);
				matrix = Matrix.CreateScale(1.25f) * matrix;
			}
			else if (eventNode.EventList.IsNullOrEmpty())
			{
				_pathSpriteRenderer.Color = new Color(150, 150, 150, 200);
			}
			else
			{
				_pathSpriteRenderer.Color = new Color(0, 255, 0, 200);
			}
			if (!GameCore.IsEditorEnabled || Mode == ToolPathModeEnum.ControlPoints)
			{
				matrix = Matrix.CreateScale(0.6f) * matrix;
			}
			_pathSpriteRenderer.Frame = 2;
			_pathSpriteRenderer.Draw(matrix, _spriteBatch, 0f, BlendModeEnum.Normal);
			int eventNodeCount = path.GetEventNodeCount(eventNode.Time);
			if (eventNodeCount > 1)
			{
				Editor.Font.Write(_spriteBatch, $"{eventNodeCount}", vector.X + 12f, vector.Y - 6f, Color.Yellow, TextAlign.Center);
			}
			if (_editor.CurrentObject != null && _editor.CurrentObject.MyPath != null)
			{
				DrawNodeAngleControl(eventNode, vector);
			}
			if (eventNode.TimeLine != null && (eventNode.Time >= path.MyObject.PathTime || GameCore.IsEditorEnabled))
			{
				_editor.ToolTimeLine.Draw(vector, eventNode.TimeLine, 0f, Vector2.Zero);
			}
			if (eventNode.LinkedObjectList == null)
			{
				continue;
			}
			_editor.LineBatch.Begin(BlendState.NonPremultiplied, Matrix.Identity);
			foreach (GameObject linkedObject in eventNode.LinkedObjectList)
			{
				Vector2 to = GameCore.CurrentLevel.Camera.CurrentLayerPosToScreen(linkedObject.WorldPosition);
				_editor.LineBatch.DrawLine(vector, to, new Color(50, 190, 255, 150));
			}
			_editor.LineBatch.End();
		}
	}

	public void DrawPath(BezierPath path, GameObject relativeToObject)
	{
		if (path.MyObject == null)
		{
			return;
		}
		_editor.LineBatch.Begin(BlendState.AlphaBlend, GameCore.LevelMatrix);
		Color color;
		if (GameCore.IsEditorEnabled)
		{
			if (_editor.CurrentTool == EditorToolEnum.Path)
			{
				if (path == CurrentPath)
				{
					color = new Color(200, 0, 200, 230);
				}
				else if (path == SelectedPath)
				{
					color = new Color(255, 10, 10, 220);
				}
				else
				{
					color = (_editor.IsShowPathEnabled ? new Color(190, 0, 0, 180) : new Color(110, 100, 100, 130));
				}
			}
			else if (_editor.IsShowPathEnabled)
			{
				color = ((_editor.CurrentObject != path.MyObject) ? new Color(120, 100, 100, 150) : new Color(150, 0, 150, 100));
			}
			else
			{
				color = ((_editor.CurrentObject != path.MyObject) ? new Color(100, 80, 80, 50) : new Color(150, 0, 150, 80));
			}
		}
		else
		{
			color = ((path.MyObject.PathFollowMode == PathFollowModeEnum.NoMove) ? new Color(100, 100, 100, 150) : new Color(200, 0, 200, 150));
		}
		float num = 0f;
		Vector2 vector = Vector2.Zero;
		bool flag = false;
		float num2 = float.MaxValue;
		float num3 = 0f;
		float num4 = ((CurrentPath != path) ? 20f : 1f);
		for (int i = 0; i < 99999; i++)
		{
			if (num >= 1f)
			{
				flag = true;
				num = 1f;
			}
			_ = path.CurveCount;
			Vector2 position = path.GetPosition(num);
			position += path.MyObject.PathOscilation;
			num3 += num4;
			if (i == 0)
			{
				vector = position;
			}
			else if ((num3 >= 20f) | flag)
			{
				num3 = 0f;
				if (path.MyObject != null && path.MyObject.MyCamera.IsPointOnScreen(position))
				{
					_editor.LineBatch.DrawLine(vector, position, color);
				}
				vector = position;
			}
			if (_editor.SelectedDragAction != DragAction.OnPanel && _editor.SelectedDragAction != DragAction.OnPanelComponent && _editor.SelectedDragAction != DragAction.MoveTimeNode && _editor.DragAction != DragAction.MoveTimeNode && path == CurrentPath && DraggedAngleEvent == null)
			{
				if (_selectedPathControlPoint >= 0)
				{
					_nearestNodeTime = path.GetEndPointTime(_selectedPathControlPoint / 3);
				}
				else
				{
					float num5 = ((_editor.DragAction != DragAction.MovePathNode || DraggedEventNode == null) ? Vector2.Distance(_cursorWorldPos, position) : Vector2.Distance(_cursorWorldPos + _editor.DragOffset, position));
					if (num2 > num5)
					{
						num2 = num5;
						if (num2 < 20f || DraggedEventNode != null)
						{
							_nearestNodeTime = num;
						}
					}
				}
			}
			else if (path == CurrentPath)
			{
				_nearestNodeTime = -1f;
			}
			num = path.GetNextPositionTime(num, num4);
			if (flag)
			{
				break;
			}
		}
		_editor.LineBatch.End();
		if (!_editor.IsShowPathEnabled && relativeToObject != _editor.CurrentObject && (GameCore.IsEditorEnabled || !Options.DebugPath))
		{
			return;
		}
		_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
		DrawEventNodes(path);
		if (GameCore.IsEditorEnabled && (Mode == ToolPathModeEnum.ControlPoints || Mode == ToolPathModeEnum.ControlPointsAndEvents))
		{
			_editor.LineBatch.Begin(BlendState.NonPremultiplied, Matrix.Identity);
			for (int j = 0; j < path.ControlPointList.Count; j++)
			{
				Vector2 vector2 = path.ControlPointList[j];
				vector2 += path.MyObject.PathOscilation;
				if (path.MyObject != null && !path.MyObject.MyCamera.IsPointOnScreen(vector2))
				{
					vector = vector2;
					continue;
				}
				Vector2 vector3 = GameCore.CurrentLevel.Camera.CurrentLayerPosToScreen(vector2);
				Vector2 to = GameCore.CurrentLevel.Camera.CurrentLayerPosToScreen(vector);
				PathPointType pointType = path.GetPointType(j);
				if (j > 0 && pointType != PathPointType.ControlPoint2 && path == CurrentPath)
				{
					_editor.LineBatch.DrawLine(vector3, to, new Color(180, 170, 150, 150));
				}
				if (relativeToObject != path.MyObject)
				{
					_pathSpriteRenderer.Frame = 1;
					_pathSpriteRenderer.Color = new Color(160, 160, 160, 200);
					if (pointType != PathPointType.EndPoint)
					{
						continue;
					}
				}
				else if (pointType == PathPointType.EndPoint)
				{
					if (path == CurrentPath)
					{
						_pathSpriteRenderer.Color = new Color(255, 0, 255);
					}
					else
					{
						_pathSpriteRenderer.Color = new Color(255, 0, 0);
					}
					_pathSpriteRenderer.Frame = 1;
				}
				else
				{
					if (path != CurrentPath)
					{
						continue;
					}
					_pathSpriteRenderer.Frame = 0;
					_pathSpriteRenderer.Color = new Color(255, 240, 0);
				}
				Matrix matrix = Matrix.CreateTranslation(vector3.X, vector3.Y, 0f);
				if (pointType == PathPointType.EndPoint)
				{
					matrix = Matrix.CreateRotationZ(path.GetDirection(path.GetEndPointTime(j / 3))) * matrix;
				}
				if (path == SelectedPath && _selectedPathControlPoint == j)
				{
					matrix = Matrix.CreateScale(1.5f) * matrix;
				}
				_pathSpriteRenderer.Draw(matrix, _spriteBatch, 0f, BlendModeEnum.Normal);
				vector = vector2;
			}
			_editor.LineBatch.End();
		}
		if (GameCore.IsEditorEnabled && _nearestNodeTime >= 0f && SelectedEventNode == null && DraggedEventNode == null && _draggedPathControlPoint < 0 && _selectedPathControlPoint < 0 && path == CurrentPath && _nearestNodeTime >= 0f)
		{
			Vector2 position2 = path.GetPosition(_nearestNodeTime);
			position2 += path.MyObject.PathOscilation;
			Vector2 vector3 = GameCore.CurrentLevel.Camera.CurrentLayerPosToScreen(position2);
			Matrix matrix2 = Matrix.CreateTranslation(vector3.X, vector3.Y, 0f);
			_pathSpriteRenderer.Color = new Color(0, 255, 255, 220);
			_pathSpriteRenderer.Frame = 0;
			_pathSpriteRenderer.Draw(matrix2, _spriteBatch, 0f, BlendModeEnum.Normal);
		}
		if (!GameCore.IsEditorEnabled)
		{
			Vector2 position3 = path.GetPosition(path.MyObject.PathTime);
			position3 += path.MyObject.PathOscilation;
			Vector2 vector3 = GameCore.CurrentLevel.Camera.CurrentLayerPosToScreen(position3);
			Matrix matrix3 = Matrix.CreateTranslation(vector3.X, vector3.Y, 0f);
			if (path.MyObject.PathFollowMode == PathFollowModeEnum.NoMove)
			{
				_pathSpriteRenderer.Color = new Color(150, 150, 150, 150);
			}
			else
			{
				_pathSpriteRenderer.Color = new Color(0, 255, 255, 220);
			}
			_pathSpriteRenderer.Frame = 0;
			_pathSpriteRenderer.Draw(matrix3, _spriteBatch, 0f, BlendModeEnum.Normal);
		}
		_spriteBatch.End();
	}
}
