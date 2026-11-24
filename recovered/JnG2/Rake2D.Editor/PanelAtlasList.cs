using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D.Editor;

public class PanelAtlasList : Panel
{
	public delegate void OnAtlasSelectedHandler(SpriteAtlas atlas, int frame);

	private List<SpriteAtlas> _atlasList;

	private PanelSlider _slider;

	private Rectangle _listRect;

	private float _frameW = 100f;

	private float _frameH = 100f;

	private int _maxVisibleCount;

	private int _selectedIndex;

	private TextField _textFieldSearch;

	public event OnAtlasSelectedHandler OnAtlasSelected;

	public PanelAtlasList(Editor editor, int x, int y, int width, int height)
		: base(editor, x, y, width, height)
	{
		MinWidth = (int)(_frameW + 10f);
		MinHeight = (int)(_frameH + 30f);
		_slider = new PanelSlider(this);
		_textFieldSearch = new TextField(this);
		_textFieldSearch.Position.X = 7f;
		_textFieldSearch.Position.Y = TitleBarHeight + 24;
		_textFieldSearch.Label = null;
		_textFieldSearch.LabelWidth = 0;
		_textFieldSearch.VariableType = VariableTypeEnum.String;
		_textFieldSearch.DefaultTextValue = "";
		_textFieldSearch.FrameColor = new Color(150, 150, 160, 200);
		_textFieldSearch.Tooltip = "Search filter";
		_textFieldSearch.OnValueChanged += OnSearchFieldChanged;
	}

	private void OnSearchFieldChanged(TextField field, float deltaValue)
	{
		BuildAtlasList();
	}

	public void SetSearchValue(string value)
	{
		_textFieldSearch.SetStringValue(value);
		_textFieldSearch.SetFocus(bIgnoreFirstInput: false);
	}

	public void BuildAtlasList()
	{
		_atlasList = (from a in Enumerable.Concat(second: (Editor.CurrentLevel == null) ? new List<SpriteAtlas>() : (from a in Editor.CurrentLevel.Content.GetAtlasList()
				where a.IsForEmitter
				select a).ToList(), first: from a in GameCore.GlobalContent.GetAtlasList()
				where a.IsForEmitter
				select a)
			orderby a.Id
			select a).ToList();
		if (_textFieldSearch.ValueString == null || _textFieldSearch.ValueString.Trim().Length <= 0)
		{
			return;
		}
		string searchValue = _textFieldSearch.ValueString.Trim().ToLower();
		if (searchValue.StartsWith("*"))
		{
			_atlasList = (from obj in _atlasList
				where obj.Id.ToLower().Contains(searchValue.Substring(1))
				orderby obj.Id
				select obj).ToList();
		}
		else
		{
			_atlasList = (from obj in _atlasList
				where obj.Id.ToLower().StartsWith(searchValue)
				orderby obj.Id
				select obj).ToList();
		}
	}

	public override void Update(float elapsedTime)
	{
		if (!IsEnabled || IsOff)
		{
			return;
		}
		base.Update(elapsedTime);
		if (IsMinimized)
		{
			return;
		}
		_listRect = new Rectangle(5, TitleBarHeight + 30, Width - 10, Height - 10 - TitleBarHeight);
		_maxVisibleCount = (int)((float)_listRect.Height / _frameH);
		_slider.MaxIndex = _atlasList.Count - _maxVisibleCount;
		_slider.Update(elapsedTime);
		_selectedIndex = -1;
		if (Editor.SelectedPanel == this)
		{
			for (int i = _slider.CurrIndex; i < _slider.CurrIndex + _maxVisibleCount; i++)
			{
				Vector2 vector = Position + GetRowPosition(i - _slider.CurrIndex);
				if (Editor.Cursor.IsInRect(Position.X + (float)_listRect.X, vector.Y - _frameH / 2f, Width - 10, (int)_frameH))
				{
					_selectedIndex = i;
					Editor.SelectedDragAction = DragAction.OnPanelComponent;
					break;
				}
			}
			if (_selectedIndex >= 0 && GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				Editor.DragAction = DragAction.OnPanelComponent;
			}
			if (_selectedIndex >= 0 && _selectedIndex < _atlasList.Count && GameInput.MouseReleased(GameInput.MouseButton.Left) && Editor.DragAction == DragAction.OnPanelComponent)
			{
				OnAtlasSelected?.Invoke(_atlasList[_selectedIndex], 0);
			}
		}
		TitleText = $"Atlas list ({_slider.CurrIndex + 1} - {_slider.CurrIndex + _maxVisibleCount} / {_atlasList.Count})";
		_textFieldSearch.Width = Width - 12;
	}

	private Vector2 GetRowPosition(int number)
	{
		return new Vector2(5f + _frameW / 2f, (float)_listRect.Top + _frameH / 2f + _frameH * (float)number);
	}

	public override void Draw()
	{
		if (!IsEnabled || IsOff)
		{
			return;
		}
		base.Draw();
		if (IsMinimized)
		{
			return;
		}
		DrawBegin();
		if (!_atlasList.IsNullOrEmpty())
		{
			for (int i = _slider.CurrIndex; i < _slider.CurrIndex + _maxVisibleCount && i < _atlasList.Count; i++)
			{
				SpriteAtlas spriteAtlas = _atlasList[i];
				Texture2D texture = spriteAtlas.Texture ?? GameCore.DefaultTexture;
				Transformation transformation = Transformation.CreateByPosition(GetRowPosition(i - _slider.CurrIndex));
				for (int j = 0; j < spriteAtlas.FrameCount; j++)
				{
					Rectangle frameRectangle = spriteAtlas.GetFrameRectangle(j);
					transformation.Scale = new Vector2(Math.Min(1f / ((float)Math.Max(frameRectangle.Width, frameRectangle.Height) / 100f), 1f));
					Editor.SpriteBatch.Draw(texture, Position + transformation.Position, frameRectangle, Color.White, transformation.Rotation, new Vector2(frameRectangle.Width / 2, frameRectangle.Height / 2), transformation.Scale, SpriteEffects.None, 0f);
					transformation.Position.X += _frameW + 2f;
					if (transformation.Position.X > (float)Width - _frameW / 2f - 5f)
					{
						break;
					}
				}
				Editor.Font.Write(Editor.SpriteBatch, spriteAtlas.Id + $" ({spriteAtlas.FileName})", Position + new Vector2(5f, transformation.Position.Y - _frameH / 2f + 1f), (_selectedIndex == i) ? Color.Yellow : Color.White, TextAlign.Left, Width - 10);
				transformation.Position.Y += _frameH + 2f;
			}
		}
		_slider.Draw();
		DrawEnd();
	}
}
