using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D.Editor;

internal class PanelList : Panel
{
	public BitmapFont Font;

	public Color SliderColor = Color.Gray;

	public Color SliderSelectedColor = Color.Yellow;

	public Color SliderDragColor = Color.Red;

	public Color SliderDragSelectedColor = Color.White;

	public int SliderAreaOfsetTop;

	public int SliderAreaOfsetBottom;

	public int SliderAreaWidth = 6;

	public int ItemListY;

	protected const int LINE_H = 16;

	protected int _firstVisibleItem;

	protected int _itemCount;

	protected int _currentItemIndex;

	protected int _currentSelectedItem;

	protected int _lastSelectedItem;

	protected int _maxVisibleItems;

	protected bool _isSliderSelected;

	protected bool _isDragSelected;

	protected bool _isDragDragged;

	protected Rectangle _sliderAreaRect;

	protected Rectangle _dragRect;

	protected int _dragStartVisibleItemNumber;

	private Vector2 _dragPosition;

	public PanelList(Editor editor, int x, int y, int width, int height)
		: base(editor, x, y, width, height)
	{
		Font = null;
		_firstVisibleItem = 10;
		_currentItemIndex = -1;
	}

	public Rectangle GetItemRectangle(int i)
	{
		return new Rectangle((int)Position.X + 2, 1 + (int)Position.Y + i * 16 + TitleBarHeight + ItemListY, Width - 4, 16);
	}

	protected virtual void OnItemClick(int itemIndex, GameInput.MouseButton mouseButton, bool doubleClick)
	{
		Editor.SetPanelFocus(this);
	}

	protected virtual void OnItemSelect(int itemIndex)
	{
	}

	public override void ParseInput()
	{
		base.ParseInput();
		if (_cursorOver)
		{
			int num = (int)((float)GameInput.GetMouseWheelDelta() / 100f);
			if (num != 0)
			{
				_firstVisibleItem -= num;
				_firstVisibleItem = Utils.Clamp(_firstVisibleItem, 0, _itemCount - 1);
			}
		}
	}

	public override void Draw()
	{
		base.Draw();
	}

	public virtual void ShowSelectedItem()
	{
		if (_currentItemIndex >= 0 && _currentItemIndex < _firstVisibleItem)
		{
			_firstVisibleItem = _currentItemIndex;
		}
		else if (_currentItemIndex >= _firstVisibleItem + _maxVisibleItems)
		{
			_firstVisibleItem = _currentItemIndex - _maxVisibleItems + 1;
		}
	}

	public override void Update(float elapsedTime)
	{
		if (!IsEnabled)
		{
			return;
		}
		UpdateSlider();
		base.Update(elapsedTime);
		_maxVisibleItems = (Height - 1 - TitleBarHeight - ItemListY) / 16;
		if (_itemCount - _firstVisibleItem < _maxVisibleItems)
		{
			_firstVisibleItem = _itemCount - _maxVisibleItems;
		}
		if (_firstVisibleItem < 0)
		{
			_firstVisibleItem = 0;
		}
		_currentSelectedItem = -1;
		if (Editor.SelectedPanel == this && (Editor.DragAction == DragAction.None || Editor.DragAction == DragAction.OnPanel || Editor.DragAction == DragAction.MovePanel) && Editor.Cursor.IsInRect(Position.X, Position.Y, Width, Height))
		{
			bool flag = false;
			for (int i = _firstVisibleItem; i < _firstVisibleItem + _maxVisibleItems && i < _itemCount; i++)
			{
				Rectangle itemRectangle = GetItemRectangle(i - _firstVisibleItem);
				if (Editor.Cursor.IsInRect(itemRectangle))
				{
					Editor.SelectedDragAction = DragAction.OnPanelComponent;
					Editor.Cursor.Text = "";
					_currentSelectedItem = i;
					if (_lastSelectedItem != i)
					{
						_lastSelectedItem = i;
						OnItemSelect(_lastSelectedItem);
					}
					Editor.Cursor.Text = $"Item: {_currentSelectedItem}";
					if (GameInput.MousePressed(GameInput.MouseButton.Left))
					{
						Editor.DragAction = DragAction.OnPanelComponent;
						OnItemClick(i, GameInput.MouseButton.Left, GameInput.DoubleClick);
						flag = true;
					}
					if (flag)
					{
						break;
					}
				}
			}
		}
		if (_currentSelectedItem == -1)
		{
			_lastSelectedItem = -1;
			OnItemSelect(_lastSelectedItem);
		}
	}

	private int GetSliderLength()
	{
		return Height - SliderAreaOfsetTop + SliderAreaOfsetBottom;
	}

	public void UpdateSlider()
	{
		_isSliderSelected = false;
		_isDragSelected = false;
		if (_itemCount > _maxVisibleItems)
		{
			float num = (float)_firstVisibleItem / (float)(_itemCount - _maxVisibleItems);
			_sliderAreaRect = new Rectangle((int)Position.X + Width - SliderAreaWidth - 2, (int)Position.Y + SliderAreaOfsetTop, SliderAreaWidth, GetSliderLength());
			_dragPosition = new Vector2(_sliderAreaRect.Left + SliderAreaWidth / 2, (float)_sliderAreaRect.Top + (float)_sliderAreaRect.Height * num);
			float num2 = 0f;
			if (_itemCount > 0)
			{
				num2 = (float)_maxVisibleItems / (float)_itemCount;
			}
			int num3 = Math.Max(20, (int)(num2 * (float)_sliderAreaRect.Height));
			_dragRect = new Rectangle(_sliderAreaRect.Left, (int)((float)_sliderAreaRect.Top + (float)(_sliderAreaRect.Height - num3) / (float)(_itemCount - _maxVisibleItems) * (float)_firstVisibleItem), SliderAreaWidth, num3);
			if (!GameInput.MouseDown(GameInput.MouseButton.Left) && _isDragDragged)
			{
				_isDragDragged = false;
				Editor.DragAction = DragAction.None;
			}
			if (Editor.DragAction == DragAction.None)
			{
				if (Editor.Cursor.IsInRect(_dragRect))
				{
					_isSliderSelected = true;
					_isDragSelected = true;
					Editor.SelectedDragAction = DragAction.ListSlider;
					if (GameInput.MousePressed(GameInput.MouseButton.Left))
					{
						Editor.DragAction = DragAction.ListSlider;
						_dragStartVisibleItemNumber = _firstVisibleItem;
						Editor.DragCenter = Editor.Cursor.Position;
						_isDragDragged = true;
					}
				}
				else if (Editor.Cursor.IsInRect(_sliderAreaRect))
				{
					_isSliderSelected = true;
					Editor.SelectedDragAction = DragAction.ListSlider;
					if (GameInput.MousePressed(GameInput.MouseButton.Left))
					{
						if (Editor.Cursor.Position.Y > (float)_dragRect.Bottom)
						{
							_firstVisibleItem = MathHelper.Min(_itemCount - _maxVisibleItems, _firstVisibleItem + _maxVisibleItems);
						}
						else
						{
							_firstVisibleItem = MathHelper.Max(0, _firstVisibleItem - _maxVisibleItems);
						}
					}
				}
			}
			if (_isDragDragged)
			{
				float num4 = (float)(_sliderAreaRect.Height - _dragRect.Height) / (float)(_itemCount - _maxVisibleItems);
				if (num4 > 0f)
				{
					_firstVisibleItem = (int)Utils.Clamp((float)_dragStartVisibleItemNumber + (Editor.Cursor.Position.Y - Editor.DragCenter.Y) / num4, 0f, _itemCount - _maxVisibleItems);
				}
				else
				{
					_firstVisibleItem = _dragStartVisibleItemNumber;
				}
			}
		}
		else
		{
			_firstVisibleItem = 0;
		}
	}

	public void DrawSlider()
	{
		if (_itemCount > _maxVisibleItems)
		{
			Editor.TriangleBatch.Begin(BlendState.AlphaBlend, Matrix.Identity);
			Editor.TriangleBatch.DrawRect(_sliderAreaRect, (_isSliderSelected || _isDragDragged || (Editor.DragAction == DragAction.ListSlider && Editor.SelectedPanel == this)) ? SliderSelectedColor : SliderColor);
			Editor.TriangleBatch.DrawRect(_dragRect, (_isDragSelected || _isDragDragged) ? SliderDragSelectedColor : SliderDragColor);
			Editor.TriangleBatch.End();
		}
	}
}
