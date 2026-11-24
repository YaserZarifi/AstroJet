using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Rake2D.Editor;

public class Panel : IEditorPanel
{
	public Color FrameColor;

	public Color BackgroundColor;

	public Color TitleBarBackgroundColor;

	public Color TitleBarTextColor;

	public static Color ShadowColor = new Color(0, 0, 0, 75);

	public int ShadowSize = 5;

	public Vector2 Position;

	public int Width;

	public int Height;

	public int TitleBarHeight;

	public string TitleText;

	public bool IsEnabled = true;

	public bool IsOff;

	public bool IsDocked;

	public bool IsSizeable;

	public bool IsMinimized;

	public bool IsCursorOnTitleBar;

	public int MinWidth = 100;

	public int MinHeight = 20;

	public Editor Editor;

	private const float _borderWidth = 2f;

	protected bool _cursorOver;

	protected List<PanelComponent> _componentList;

	public Panel(Editor editor, int x, int y, int width, int height)
	{
		Editor = editor;
		Position = new Vector2(x, y);
		Width = width;
		Height = height;
		_componentList = new List<PanelComponent>();
	}

	public virtual void Init()
	{
	}

	public void ChangeTab(int direction)
	{
		List<PanelComponent> list = _componentList.FindAll((PanelComponent o) => o.IsFocusable);
		if (list.Contains(Editor.FocusComponent))
		{
			int num = list.IndexOf(Editor.FocusComponent) + direction;
			if (num < 0 || num >= list.Count)
			{
				num = ((direction <= 0) ? (list.Count - 1) : 0);
			}
			list[num].SetFocus(bIgnoreFirstInput: true);
		}
	}

	public virtual void ParseInput()
	{
		if (IsEnabled && !IsOff && Editor.FocusComponent != null && GameInput.SystemKeyPressed(Keys.Tab))
		{
			if (GameInput.ShiftDown)
			{
				ChangeTab(-1);
			}
			else
			{
				ChangeTab(1);
			}
		}
	}

	public void ToggleEnabled()
	{
		IsEnabled = !IsEnabled;
	}

	public void UpdateComponents(float elapsedTime)
	{
		if (!IsEnabled || IsOff)
		{
			return;
		}
		if (!IsMinimized)
		{
			foreach (PanelComponent component in _componentList)
			{
				component.UpdateTop(elapsedTime);
			}
			foreach (PanelComponent component2 in _componentList)
			{
				component2.Update(elapsedTime);
			}
		}
		if (!IsDocked)
		{
			UpdateDragMove(elapsedTime);
		}
		if (IsCursorOnTitleBar && GameInput.MousePressed(GameInput.MouseButton.Left) && GameInput.DoubleClick)
		{
			IsMinimized = !IsMinimized;
		}
	}

	public virtual void Update(float elapsedTime)
	{
		IsCursorOnTitleBar = false;
		if (!IsEnabled || IsOff)
		{
			return;
		}
		int height = Math.Max(10, IsMinimized ? (TitleBarHeight + 3) : Height);
		if ((Editor.SelectedComponent == null || Editor.SelectedComponent.Panel == this) && Editor.SelectedDragAction == DragAction.None && Editor.SelectedPanel == null && Editor.Cursor.IsInRect(Position, Width, height))
		{
			_cursorOver = true;
			Editor.SelectedDragAction = DragAction.OnPanel;
			if (Editor.SelectedPanel == null)
			{
				Editor.SelectedPanel = this;
			}
			if (Editor.Cursor.IsInRect(Position, Width, TitleBarHeight))
			{
				IsCursorOnTitleBar = true;
			}
		}
		else
		{
			_cursorOver = false;
		}
		if (!IsMinimized)
		{
			ParseInput();
		}
		if (IsSizeable && !IsMinimized)
		{
			UpdateResize(elapsedTime);
		}
		if (Editor.DraggedPanel == this)
		{
			Editor.SelectedPanel = this;
		}
	}

	public void UpdateResize(float elapsedTime)
	{
		if (Editor.DragAction == DragAction.None && (Editor.SelectedDragAction == DragAction.None || Editor.SelectedDragAction == DragAction.OnPanel) && Editor.SelectedPanel == this)
		{
			if (Editor.Cursor.IsInRect(Position.X, Position.Y - 4f, Width, 8))
			{
				Editor.SelectedDragAction = DragAction.PanelResizeTop;
				Editor.Cursor.SetType(CursorType.ResizeVertical);
				Editor.Cursor.Text = "Resize panel";
				Editor.SelectedPanel = this;
				if (GameInput.MousePressed(GameInput.MouseButton.Left))
				{
					Editor.DraggedPanel = this;
					Editor.SetFocusComponent(null);
					Editor.DragAction = DragAction.PanelResizeTop;
					Editor.DragOffset = Editor.Cursor.Position - Position;
					Editor.DragCenter = Position + new Vector2(0f, Height);
				}
			}
			else if (Editor.Cursor.IsInRect(Position.X, Position.Y - 4f + (float)Height, Width, 8))
			{
				Editor.SelectedDragAction = DragAction.PanelResizeTop;
				Editor.Cursor.SetType(CursorType.ResizeVertical);
				Editor.Cursor.Text = "Resize panel";
				Editor.SelectedPanel = this;
				if (GameInput.MousePressed(GameInput.MouseButton.Left))
				{
					Editor.DraggedPanel = this;
					Editor.SetFocusComponent(null);
					Editor.DragAction = DragAction.PanelResizeBottom;
					Editor.DragOffset = Editor.Cursor.Position - Position - new Vector2(0f, Height);
				}
			}
			if (Editor.Cursor.IsInRect(Position.X - 4f, Position.Y, 8, Height))
			{
				Editor.SelectedDragAction = DragAction.PanelResizeLeft;
				Editor.Cursor.SetType(CursorType.ResizeVertical);
				Editor.Cursor.Rotation += (float)Math.PI / 2f;
				Editor.Cursor.Text = "Resize panel";
				Editor.SelectedPanel = this;
				if (GameInput.MousePressed(GameInput.MouseButton.Left))
				{
					Editor.DraggedPanel = this;
					Editor.SetFocusComponent(null);
					Editor.DragAction = DragAction.PanelResizeLeft;
					Editor.DragOffset = Editor.Cursor.Position - Position;
					Editor.DragCenter = Position + new Vector2(Width, Height);
				}
			}
			else if (Editor.Cursor.IsInRect(Position.X + (float)Width - 4f, Position.Y, 8, Height))
			{
				Editor.SelectedDragAction = DragAction.PanelResizeRight;
				Editor.Cursor.Rotation += (float)Math.PI / 2f;
				Editor.Cursor.SetType(CursorType.ResizeVertical);
				Editor.Cursor.Text = "Resize panel";
				Editor.SelectedPanel = this;
				if (GameInput.MousePressed(GameInput.MouseButton.Left))
				{
					Editor.DraggedPanel = this;
					Editor.SetFocusComponent(null);
					Editor.DragAction = DragAction.PanelResizeRight;
					Editor.DragOffset = Editor.Cursor.Position - Position - new Vector2(Width, Height);
				}
			}
		}
		if (Editor.DraggedPanel == this && (Editor.DragAction == DragAction.PanelResizeLeft || Editor.DragAction == DragAction.PanelResizeRight || Editor.DragAction == DragAction.PanelResizeTop || Editor.DragAction == DragAction.PanelResizeBottom))
		{
			Editor.Cursor.Text = "Resize panel";
			if (Editor.DragAction == DragAction.PanelResizeTop)
			{
				Editor.Cursor.SetType(CursorType.ResizeVertical);
				if (Height > MinHeight || Editor.Cursor.Position.Y - Editor.DragOffset.Y < Position.Y)
				{
					Height = Math.Max(MinHeight, (int)(Editor.DragCenter.Y - Editor.Cursor.Position.Y - Editor.DragOffset.Y));
					Position.Y = Editor.DragCenter.Y - (float)Height;
					CheckOutOfScreen();
				}
			}
			else if (Editor.DragAction == DragAction.PanelResizeBottom)
			{
				Editor.Cursor.SetType(CursorType.ResizeVertical);
				Height = (int)(Editor.Cursor.Position.Y - Position.Y - Editor.DragOffset.Y);
			}
			else if (Editor.DragAction == DragAction.PanelResizeLeft)
			{
				Editor.Cursor.SetType(CursorType.ResizeVertical);
				Editor.Cursor.Rotation = (float)Math.PI / 2f;
				if (Width > MinWidth || Editor.Cursor.Position.X - Editor.DragOffset.X < Position.X)
				{
					Width = Math.Max(MinWidth, (int)(Editor.DragCenter.X - Editor.Cursor.Position.X - Editor.DragOffset.X));
					Position.X = Editor.DragCenter.X - (float)Width;
					CheckOutOfScreen();
				}
			}
			else if (Editor.DragAction == DragAction.PanelResizeRight)
			{
				Editor.Cursor.SetType(CursorType.ResizeVertical);
				Editor.Cursor.Rotation = (float)Math.PI / 2f;
				Width = (int)(Editor.Cursor.Position.X - Position.X - Editor.DragOffset.X);
			}
		}
		Width = Math.Max(MinWidth, Width);
		Height = Math.Max(MinHeight, Height);
	}

	public void UpdateDragMove(float elapsedTime)
	{
		if (Editor.SelectedPanel == this && Editor.SelectedComponent == null && Editor.DragAction == DragAction.None && (Editor.IsDragAvailable || Editor.SelectedDragAction == DragAction.OnPanel) && Editor.DragAction != DragAction.OnPanelComponent)
		{
			Editor.Cursor.Text = "Move panel";
			Editor.SelectedDragAction = DragAction.MovePanel;
			if (GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				Editor.DragOffset = Position - Editor.Cursor.Position;
				Editor.DragAction = DragAction.MovePanel;
				Editor.DraggedPanel = this;
				Editor.SetFocusComponent(null);
			}
		}
		if (Editor.DraggedPanel == this && Editor.DragAction == DragAction.MovePanel)
		{
			Position = Editor.Cursor.Position + Editor.DragOffset;
			CheckOutOfScreen();
		}
	}

	public void CheckOutOfScreen()
	{
		if (GameCore.IsMainViewPortInitialized)
		{
			Position = Utils.Clamp(Position, new Vector2(20 - Width, 20 - Height), new Vector2(GameCore.MainViewPort.Width - 20, GameCore.MainViewPort.Height - 20));
		}
	}

	public void DrawBegin()
	{
		Editor.SpriteDrawBegin();
		Editor.LineBatch.Begin(BlendState.AlphaBlend, Matrix.Identity);
		Editor.TriangleBatch.Begin(BlendState.AlphaBlend, Matrix.Identity);
	}

	public void DrawEnd()
	{
		Editor.TriangleBatch.End();
		Editor.LineBatch.End();
		Editor.SpriteDrawEnd();
	}

	public virtual void Draw()
	{
		if (!IsEnabled || IsOff)
		{
			return;
		}
		DrawBegin();
		int num = Math.Max(10, IsMinimized ? (TitleBarHeight + 3) : Height);
		if (ShadowSize > 0)
		{
			Editor.TriangleBatch.DrawRect(new Vector2(Position.X + (float)Width, Position.Y + (float)ShadowSize), new Vector2(Position.X + (float)Width + (float)ShadowSize, Position.Y + (float)num + (float)ShadowSize), ShadowColor);
			Editor.TriangleBatch.DrawRect(new Vector2(Position.X + (float)ShadowSize, Position.Y + (float)num), new Vector2(Position.X + (float)Width, Position.Y + (float)num + (float)ShadowSize), ShadowColor);
		}
		if (TitleBarHeight > 0)
		{
			Editor.TriangleBatch.DrawRect(Position, new Vector2(Position.X + (float)Width - 2f, Position.Y + (float)TitleBarHeight), TitleBarBackgroundColor);
			Editor.Font.Write(Editor.SpriteBatch, TitleText, Position.X + 4f, Position.Y + 3f, TitleBarTextColor, TextAlign.Left, Width - 4);
		}
		Editor.TriangleBatch.DrawRect(new Vector2(Position.X, Position.Y + (float)TitleBarHeight), new Vector2(Position.X + (float)Width, Position.Y + (float)num), BackgroundColor);
		Editor.LineBatch.DrawLine(Position.X, Position.Y, Position.X + (float)Width - 1f, Position.Y, BackgroundColor * 1.5f);
		Editor.LineBatch.DrawLine(Position.X + 1f, Position.Y + 1f, Position.X + (float)Width - 2f, Position.Y + 1f, BackgroundColor * 1.25f);
		Editor.LineBatch.DrawLine(Position.X, Position.Y + 1f, Position.X, Position.Y + (float)num - 1f, BackgroundColor * 1.5f);
		Editor.LineBatch.DrawLine(Position.X + 1f, Position.Y + 2f, Position.X + 1f, Position.Y + (float)num - 2f, BackgroundColor * 1.25f);
		Editor.LineBatch.DrawLine(Position.X, Position.Y + (float)num - 1f, Position.X + (float)Width - 1f, Position.Y + (float)num - 1f, Utils.MultiplyColor(BackgroundColor, 0.5f));
		Editor.LineBatch.DrawLine(Position.X + 1f, Position.Y + (float)num - 2f, Position.X + (float)Width - 2f, Position.Y + (float)num - 2f, Utils.MultiplyColor(BackgroundColor, 0.75f));
		Editor.LineBatch.DrawLine(Position.X + (float)Width - 1f, Position.Y, Position.X + (float)Width - 1f, Position.Y + (float)num, Utils.MultiplyColor(BackgroundColor, 0.5f));
		Editor.LineBatch.DrawLine(Position.X + (float)Width - 2f, Position.Y + 1f, Position.X + (float)Width - 2f, Position.Y + (float)num - 1f, Utils.MultiplyColor(BackgroundColor, 0.75f));
		if (!IsMinimized)
		{
			foreach (PanelComponent component in _componentList)
			{
				component.Draw();
			}
			foreach (PanelComponent component2 in _componentList)
			{
				component2.DrawTop();
			}
		}
		DrawEnd();
	}

	public void AddComponent(PanelComponent component)
	{
		_componentList.Add(component);
	}

	public void RemoveComponent(PanelComponent component)
	{
		_componentList.Remove(component);
	}

	public void RemoveAllComponents()
	{
		_componentList.Clear();
	}
}
