using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Editor;

internal class PanelGameObjectList : PanelList
{
	private List<GameObject> _itemList;

	private TextField _textFieldSearch;

	public string SearchValue => _textFieldSearch.ValueString;

	public PanelGameObjectList(Editor editor, int x, int y, int width, int height)
		: base(editor, x, y, width, height)
	{
		Font = null;
		_itemList = new List<GameObject>();
		ItemListY = 20;
		_textFieldSearch = new TextField(this);
		_textFieldSearch.Position.X = 7f;
		_textFieldSearch.Position.Y = TitleBarHeight + 23;
		_textFieldSearch.Label = null;
		_textFieldSearch.LabelWidth = 0;
		_textFieldSearch.VariableType = VariableTypeEnum.String;
		_textFieldSearch.DefaultTextValue = "";
		_textFieldSearch.FrameColor = new Color(150, 150, 160, 200);
		_textFieldSearch.Tooltip = "Search filter";
		_textFieldSearch.Panel = this;
		_textFieldSearch.OnValueChanged += OnSerarchFieldChanged;
	}

	private void OnSerarchFieldChanged(TextField textField, float delta)
	{
		Editor.RefreshObjectList();
	}

	public void SetItems(List<GameObject> list, bool reset = true)
	{
		_itemList = list;
		if (reset)
		{
			_firstVisibleItem = 0;
			_currentItemIndex = -1;
		}
	}

	public bool Contains(GameObject gameObject)
	{
		return _itemList.Contains(gameObject);
	}

	protected override void OnItemClick(int itemIndex, GameInput.MouseButton mouseButton, bool doubleClick)
	{
		base.OnItemClick(itemIndex, mouseButton, doubleClick);
		Editor.SetCurrentObject(_itemList[itemIndex]);
		if (GameInput.DoubleClick)
		{
			GameCore.CurrentLevel.Camera.PanTo((Editor.CurrentObject.Position - GameCore.CurrentLevel.Camera.Origin) / Editor.CurrentLayer.ScrollFactor, 100000f, useFrameTime: true);
		}
	}

	public void SelectObject()
	{
		if (Editor.CurrentObject == null)
		{
			_currentItemIndex = -1;
		}
		else
		{
			_currentItemIndex = _itemList.IndexOf(Editor.CurrentObject);
		}
		base.ShowSelectedItem();
	}

	public override void Update(float elapsedTime)
	{
		_itemCount = _itemList.Count;
		base.Update(elapsedTime);
		_textFieldSearch.Width = Width - 14;
	}

	public override void Draw()
	{
		_itemCount = _itemList.Count;
		base.Draw();
		if (IsMinimized || IsOff || !IsEnabled)
		{
			return;
		}
		DrawBegin();
		for (int i = _firstVisibleItem; i < _itemCount && i < _firstVisibleItem + _maxVisibleItems; i++)
		{
			GameObject gameObject = _itemList[i];
			Color color;
			if (i == _currentSelectedItem)
			{
				color = Color.Yellow;
			}
			else
			{
				color = ((gameObject.PartData == null) ? Color.Pink : gameObject.PartData.EditorColor);
			}
			if (!gameObject.IsLevelObject)
			{
				color.A = 100;
			}
			Rectangle itemRectangle = GetItemRectangle(i - _firstVisibleItem);
			if (gameObject == Editor.CurrentObject)
			{
				Editor.TriangleBatch.DrawRect(itemRectangle, new Color(98, 93, 131, 240));
			}
			Editor.Font.Write(Editor.SpriteBatch, gameObject.Id, itemRectangle.X + 5, itemRectangle.Y, color, TextAlign.Left, Width - 10);
		}
		DrawEnd();
		DrawSlider();
	}
}
