using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Rake2D.Script;

namespace Rake2D.Editor;

public class ToolTimeLine
{
	public BehaviorTimeLine SelectedTimeLine;

	public BehaviorTimeLine CurrentTimeLine;

	public BehaviorEventNode SelectedEventNode;

	private Editor _editor;

	private BehaviorEventNode _currentNode;

	private SpriteRenderer _spriteRenderer;

	private SpriteBatch _spriteBatch;

	private const float _oneSecondLength = 80f;

	public ToolTimeLine(Editor editor)
	{
		_editor = editor;
		SpriteAtlas spriteAtlas = GameCore.GlobalContent.GetSpriteAtlas("editor_bezier_path");
		_spriteRenderer = new SpriteRenderer(spriteAtlas);
		_spriteBatch = new SpriteBatch(GameCore.GraphicsDeviceStatic);
	}

	public void SetCurrentEventNode(BehaviorEventNode node)
	{
		if (node == null)
		{
			CurrentTimeLine = null;
			if (_currentNode != null)
			{
				_editor.SetCurretEventNode(null);
			}
		}
		else
		{
			_editor.SetCurretEventNode(node);
		}
		_currentNode = node;
		SelectedEventNode = null;
	}

	public void ParseInput()
	{
		if (CurrentTimeLine == null || CurrentTimeLine.EventNodeList.IsNullOrEmpty() || _editor.CurrentEventNode == null || !_editor.CurrentEventNode.IsOnTimeLine)
		{
			return;
		}
		if (_editor.KeyPressed(Keys.OemComma))
		{
			if (_editor.CurrentEventNode == null)
			{
				SetCurrentEventNode(CurrentTimeLine.EventNodeList[CurrentTimeLine.EventNodeList.Count - 1]);
			}
			else
			{
				int num = CurrentTimeLine.EventNodeList.IndexOf(_editor.CurrentEventNode);
				if (num <= 0)
				{
					SetCurrentEventNode(CurrentTimeLine.EventNodeList[CurrentTimeLine.EventNodeList.Count - 1]);
				}
				else
				{
					SetCurrentEventNode(CurrentTimeLine.EventNodeList[num - 1]);
				}
			}
		}
		if (!_editor.KeyPressed(Keys.OemPeriod))
		{
			return;
		}
		if (_editor.CurrentEventNode == null)
		{
			SetCurrentEventNode(CurrentTimeLine.EventNodeList[0]);
			return;
		}
		int num2 = CurrentTimeLine.EventNodeList.IndexOf(_editor.CurrentEventNode);
		if (num2 >= CurrentTimeLine.EventNodeList.Count - 1)
		{
			SetCurrentEventNode(CurrentTimeLine.EventNodeList[0]);
		}
		else
		{
			SetCurrentEventNode(CurrentTimeLine.EventNodeList[num2 + 1]);
		}
	}

	public void UpdateTimeLine(Vector2 screenPos, BehaviorTimeLine timeLine, float elapsedTime, Vector2 offset)
	{
		if (_editor.ToolPath.DraggedEventNode != null)
		{
			CurrentTimeLine = _editor.ToolPath.DraggedEventNode.TimeLine;
		}
		Vector2 vector = (timeLine.ScreenPosition = screenPos + offset);
		timeLine.CursorTime = -1f;
		if (timeLine.EventNodeList == null)
		{
			return;
		}
		float maxTime = timeLine.GetMaxTime();
		if (_editor.IsDragAvailable && _editor.Cursor.IsInRect(vector.X, vector.Y - 8f, (int)(maxTime * 80f), 16))
		{
			float cursorTime = (_editor.Cursor.Position.X - vector.X) / 80f;
			_editor.SelectedDragAction = DragAction.MoveTimeNode;
			timeLine.CursorTime = cursorTime;
			SelectedTimeLine = timeLine;
		}
		if (CurrentTimeLine != null)
		{
			float value = Math.Max(0f, (_editor.Cursor.Position.X - vector.X) / 80f);
			_editor.Cursor.Text = $"<T> to add time event at {Utils.FormatFloat2(value)} / {Utils.FormatFloat2(maxTime)}sec";
		}
		for (int num = timeLine.EventNodeList.Count - 1; num >= 0; num--)
		{
			BehaviorEventNode behaviorEventNode = timeLine.EventNodeList[num];
			Vector2 vector2 = vector + new Vector2(80f * behaviorEventNode.Time, 0f);
			if ((_editor.IsDragAvailable || _editor.SelectedDragAction == DragAction.MoveTimeNode) && _editor.Cursor.IsInRect(vector2.X - 5f, vector2.Y - 15f, 11, 30))
			{
				SelectedEventNode = behaviorEventNode;
				_editor.SetSelectedEventNode(behaviorEventNode);
				_editor.SelectedDragAction = DragAction.MoveTimeNode;
				_editor.Cursor.Text = $"Select node t = {Utils.FormatFloat2(behaviorEventNode.Time)} sec";
				if (_editor.KeyPressed(Keys.Delete))
				{
					timeLine.RemoveEventNode(behaviorEventNode);
					if (behaviorEventNode == _editor.CurrentEventNode)
					{
						_editor.SetCurretEventNode(null);
					}
				}
				else if (GameInput.MousePressed(GameInput.MouseButton.Left))
				{
					if (_editor.CurrentObject != timeLine.MyObject)
					{
						_editor.SetCurrentObject(timeLine.MyObject);
					}
					CurrentTimeLine = timeLine;
					SetCurrentEventNode(behaviorEventNode);
					_editor.DragAction = DragAction.MoveTimeNode;
					_editor.DragOffset = vector2 - _editor.Cursor.Position;
				}
			}
			if (_currentNode == behaviorEventNode && GameInput.MouseDown(GameInput.MouseButton.Left) && _editor.DragAction == DragAction.MoveTimeNode)
			{
				float time = _currentNode.Time;
				_currentNode.Time = Math.Max(0f, (_editor.Cursor.Position.X - vector.X + _editor.DragOffset.X) / 80f);
				if (GameInput.ShiftDown)
				{
					_editor.Cursor.Text = $"Move node block t = {Utils.FormatFloat2(_currentNode.Time)} sec";
					float num2 = _currentNode.Time - time;
					for (int i = num + 1; i < timeLine.EventNodeList.Count; i++)
					{
						timeLine.EventNodeList[i].Time += num2;
					}
				}
				else
				{
					_editor.Cursor.Text = $"Move node t = {Utils.FormatFloat2(_currentNode.Time)} sec";
				}
				timeLine.ReorderNodes();
			}
		}
	}

	public void Update(float elapsedTime)
	{
		SelectedEventNode = null;
		SelectedTimeLine = null;
		if (_editor.IsShowPathEnabled)
		{
			foreach (GameObject visibleGameObject in _editor.CurrentLayer.VisibleGameObjectList)
			{
				if (visibleGameObject.MyTimeLine != null)
				{
					UpdateTimeLine(visibleGameObject.GetScreenPos(), visibleGameObject.MyTimeLine, elapsedTime, new Vector2(0f, -25f));
					if (SelectedEventNode != null)
					{
						break;
					}
				}
			}
		}
		else if (_editor.CurrentObject != null && _editor.CurrentObject.MyTimeLine != null)
		{
			UpdateTimeLine(_editor.CurrentObject.GetScreenPos(), _editor.CurrentObject.MyTimeLine, elapsedTime, new Vector2(0f, -25f));
		}
		BehaviorTimeLine behaviorTimeLine = SelectedTimeLine ?? CurrentTimeLine;
		if (_editor.KeyPressed(Keys.T) && behaviorTimeLine != null)
		{
			float time = Math.Max(0f, (_editor.Cursor.Position.X - behaviorTimeLine.ScreenPosition.X) / 80f);
			BehaviorEventNode behaviorEventNode = behaviorTimeLine.AddEventNode(time);
			behaviorEventNode.SetObject(behaviorTimeLine.MyObject);
			SetCurrentEventNode(behaviorEventNode);
		}
	}

	public void Draw(Vector2 screenPos, BehaviorTimeLine timeLine, float currTime, Vector2 offset)
	{
		if (timeLine.EventNodeList == null)
		{
			return;
		}
		Vector2 vector = screenPos + offset;
		_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
		for (int num = timeLine.EventNodeList.Count - 1; num >= 0; num--)
		{
			BehaviorEventNode behaviorEventNode = timeLine.EventNodeList[num];
			Vector2 vector2 = vector + new Vector2(80f * behaviorEventNode.Time, 0f);
			if (num == timeLine.EventNodeList.Count - 1)
			{
				_editor.LineBatch.Begin(BlendState.AlphaBlend, Matrix.Identity);
				Color color = ((timeLine != CurrentTimeLine) ? new Color(80, 100, 150, 100) : new Color(50, 100, 255));
				_editor.LineBatch.DrawLine(vector, vector2, color);
				if (offset != Vector2.Zero)
				{
					_editor.LineBatch.DrawLine(screenPos, screenPos + offset, color);
				}
				_editor.LineBatch.End();
			}
			if (behaviorEventNode.LinkedObjectList != null)
			{
				_editor.LineBatch.Begin(BlendState.NonPremultiplied, Matrix.Identity);
				foreach (GameObject linkedObject in behaviorEventNode.LinkedObjectList)
				{
					Vector2 to = GameCore.CurrentLevel.Camera.CurrentLayerPosToScreen(linkedObject.WorldPosition);
					_editor.LineBatch.DrawLine(vector2, to, new Color(70, 240, 240, 150));
				}
				_editor.LineBatch.End();
			}
			_spriteRenderer.Frame = 3;
			Transformation transformation = Transformation.CreateByPosition(vector2);
			if (behaviorEventNode.ErrorCount > 0)
			{
				transformation.Scale *= 1.2f;
				_spriteRenderer.Color = new Color(255, 0, 0, 255);
			}
			else if (behaviorEventNode == SelectedEventNode)
			{
				if (behaviorEventNode.Time == 0f)
				{
					transformation.Scale *= 1.75f;
					_spriteRenderer.Color = new Color(255, 255, 150, 255);
				}
				else
				{
					transformation.Scale *= 1.5f;
					_spriteRenderer.Color = new Color(0, 255, 255, 230);
				}
			}
			else if (behaviorEventNode == _currentNode)
			{
				if (behaviorEventNode.Time == 0f)
				{
					transformation.Scale = new Vector2(1.25f, 1.5f);
					_spriteRenderer.Color = new Color(255, 255, 100, 255);
				}
				else
				{
					transformation.Scale = new Vector2(1.25f);
					_spriteRenderer.Color = new Color(70, 255, 180, 255);
				}
			}
			else if (!GameCore.IsEditorEnabled && behaviorEventNode.Time <= currTime)
			{
				_spriteRenderer.Color = new Color(100, 100, 100, 150);
			}
			else
			{
				if (behaviorEventNode.Time == 0f)
				{
					transformation.Scale = new Vector2(1.25f);
				}
				if (behaviorEventNode.EventList.IsNullOrEmpty())
				{
					_spriteRenderer.Color = new Color(150, 150, 150, 200);
				}
				else if (behaviorEventNode.Time == 0f)
				{
					_spriteRenderer.Color = new Color(255, 255, 0, 220);
				}
				else
				{
					_spriteRenderer.Color = new Color(0, 255, 0, 200);
				}
			}
			if (_editor.CurrentTool != EditorToolEnum.Path || timeLine != CurrentTimeLine)
			{
				_spriteRenderer.Color.A = 100;
			}
			_spriteRenderer.Draw(transformation, _spriteBatch, BlendModeEnum.Normal);
		}
		if (currTime > 0f)
		{
			Transformation transformation = Transformation.CreateByPosition(vector + new Vector2(currTime * 80f, 0f));
			_spriteRenderer.Color = new Color(200, 150, 255);
			_spriteRenderer.Draw(transformation, _spriteBatch);
		}
		if (timeLine.CursorTime >= 0f && _editor.SelectedDragAction == DragAction.MoveTimeNode && _editor.DragAction == DragAction.None)
		{
			Transformation transformation = Transformation.CreateByPosition(vector + new Vector2(timeLine.CursorTime * 80f, 0f));
			transformation.Scale = new Vector2(0.75f);
			_spriteRenderer.Color = new Color(255, 150, 200);
			_spriteRenderer.Draw(transformation, _spriteBatch);
		}
		_spriteBatch.End();
	}
}
