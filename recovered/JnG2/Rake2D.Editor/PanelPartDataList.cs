using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Rake2D.Editor;

internal class PanelPartDataList : PanelList
{
	private List<PartData> _itemList;

	private TextField _textFieldSearch;

	private int _selectedEditorSet;

	public bool _isDarkEffectSelected;

	private const int CLASS_BUTTON_WIDTH = 15;

	public PartData SelectedPartData { get; private set; }

	public string SearchValue => _textFieldSearch.ValueString;

	public PanelPartDataList(Editor editor, int x, int y, int width, int height)
		: base(editor, x, y, width, height)
	{
		_itemList = new List<PartData>();
	}

	public override void Init()
	{
		base.Init();
		ItemListY = 41;
		_textFieldSearch = new TextField(this);
		_textFieldSearch.Position.X = 7f;
		_textFieldSearch.Position.Y = TitleBarHeight + 20;
		_textFieldSearch.Label = null;
		_textFieldSearch.LabelWidth = 0;
		_textFieldSearch.VariableType = VariableTypeEnum.String;
		_textFieldSearch.DefaultTextValue = "";
		_textFieldSearch.FrameColor = new Color(150, 150, 160, 200);
		_textFieldSearch.Tooltip = "Search filter";
		_textFieldSearch.Panel = this;
		_textFieldSearch.OnValueChanged += OnSerarchFieldChanged;
	}

	public void SetSearchFocus(bool bIgnoreFirstInput)
	{
		_textFieldSearch.SetFocus(bIgnoreFirstInput);
	}

	public void SetSearchValue(string value)
	{
		_textFieldSearch.SetStringValue(value);
		_textFieldSearch.SetFocus(bIgnoreFirstInput: false);
	}

	public void SetItems(List<PartData> list)
	{
		_itemList.Clear();
		_itemList.AddRange(list);
		_firstVisibleItem = 0;
		_currentItemIndex = -1;
		_itemCount = _itemList.Count;
	}

	private void OnSerarchFieldChanged(TextField textField, float delta)
	{
		Editor.RefreshPartDataList();
	}

	protected override void OnItemClick(int itemIndex, GameInput.MouseButton mouseButton, bool doubleClick)
	{
		base.OnItemClick(itemIndex, mouseButton, doubleClick);
		Editor.SetCurrentPartData(_itemList[itemIndex]);
	}

	protected override void OnItemSelect(int itemIndex)
	{
		if (GameCore.IsApplicationActive)
		{
			if (itemIndex < 0)
			{
				SelectedPartData = null;
				Editor.CreatePreviewObject(null);
			}
			else
			{
				SelectedPartData = _itemList[itemIndex];
				Editor.CreatePreviewObject(SelectedPartData);
			}
		}
	}

	public void SelectPartData()
	{
		if (Editor.CurrentPartData == null)
		{
			_currentItemIndex = -1;
		}
		else
		{
			_currentItemIndex = _itemList.IndexOf(Editor.CurrentPartData);
		}
		base.ShowSelectedItem();
	}

	private void UpdateEditorSetBar()
	{
		int num = 0;
		int num2 = 0;
		_selectedEditorSet = -1;
		for (int i = 0; i < Editor.EditorSetEnabledArray.Length; i++)
		{
			num = (int)Position.X + 5 + i * 15;
			num2 = (int)Position.Y + TitleBarHeight + 1;
			if (Editor.SelectedPanel != this || !Editor.Cursor.IsInRect(num, num2, 15, 16))
			{
				continue;
			}
			Editor.Cursor.Text = $"Select group #{i}";
			Editor.SelectedDragAction = DragAction.OnPanelComponent;
			_selectedEditorSet = i;
			if (GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				Editor.DragAction = DragAction.OnPanelComponent;
				Editor.SetPanelFocus(this);
				if (GameInput.DoubleClick)
				{
					int enabledEditorSetCount = Editor.GetEnabledEditorSetCount();
					if ((Editor.EditorSetEnabledArray[i] && enabledEditorSetCount == 1) || enabledEditorSetCount == 0)
					{
						for (int j = 0; j < Editor.EditorSetEnabledArray.Length; j++)
						{
							if (!Editor.EditorSetManualyDisabledArray[j])
							{
								Editor.EditorSetEnabledArray[j] = true;
							}
						}
					}
					else
					{
						for (int k = 0; k < Editor.EditorSetEnabledArray.Length; k++)
						{
							Editor.EditorSetEnabledArray[k] = false;
						}
						Editor.EditorSetEnabledArray[i] = true;
					}
				}
				else
				{
					for (int l = 0; l < Editor.EditorSetEnabledArray.Length; l++)
					{
						Editor.EditorSetEnabledArray[l] = false;
					}
					Editor.EditorSetEnabledArray[i] = true;
				}
				Editor.RefreshObjectList();
				Editor.RefreshPartDataList();
			}
			else if (GameInput.MousePressed(GameInput.MouseButton.Right))
			{
				Editor.EditorSetEnabledArray[i] = !Editor.EditorSetEnabledArray[i];
				Editor.EditorSetManualyDisabledArray[i] = !Editor.EditorSetEnabledArray[i];
				Editor.RefreshObjectList();
				Editor.RefreshPartDataList();
			}
		}
		num += 17;
		if (Editor.SelectedPanel == this && Editor.Cursor.IsInRect(num, num2, 15, 16))
		{
			_isDarkEffectSelected = true;
			Editor.SelectedDragAction = DragAction.OnPanelComponent;
			Editor.Cursor.Text = "Toggle dark unselected groups";
			if (GameInput.MousePressed(GameInput.MouseButton.Left) || GameInput.MousePressed(GameInput.MouseButton.Right))
			{
				Editor.DragAction = DragAction.OnPanelComponent;
				Editor.DarkDisabledEditorSets = !Editor.DarkDisabledEditorSets;
			}
		}
		else
		{
			_isDarkEffectSelected = false;
		}
		_textFieldSearch.Width = Width - 14;
	}

	public override void ParseInput()
	{
		base.ParseInput();
		if (SelectedPartData != null && Editor.CurrentTool != EditorToolEnum.Emiter && Editor.CurrentObject != null && Editor.CurrentSelectionGroup == null && Editor.KeyPressed(Keys.R) && GameInput.ControlDown)
		{
			GameObject gameObject = GameObject.CreateSimilarObject(Editor.CurrentObject, SelectedPartData, Editor.CurrentLayer, Editor.CurrentObject.Position);
			if (gameObject != null)
			{
				gameObject.Init();
				gameObject.LevelInit();
				gameObject.MyPath = Editor.CurrentObject.MyPath;
				Dictionary<GameObject, GameObject> dictionary = new Dictionary<GameObject, GameObject>();
				dictionary.Add(Editor.CurrentObject, gameObject);
				Editor.RecreateLinks(dictionary, bRecreateIncomingLinks: true);
				Editor.CurrentObject.Destroy();
			}
			Editor.SetCurrentObject(gameObject);
			Editor.RefreshObjectList();
		}
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (!IsMinimized)
		{
			UpdateEditorSetBar();
		}
	}

	public void AfterUpdate()
	{
		if (Editor.SelectedComponent == null)
		{
			if (Editor.KeyPressed(Keys.PageDown))
			{
				_firstVisibleItem += _maxVisibleItems;
				_firstVisibleItem = Utils.Clamp(_firstVisibleItem, 0, _itemCount - _maxVisibleItems);
			}
			else if (Editor.KeyPressed(Keys.PageUp))
			{
				_firstVisibleItem -= _maxVisibleItems;
				_firstVisibleItem = Math.Max(0, _firstVisibleItem);
			}
			else if (!GameInput.ControlDown && Editor.KeyPressed(Keys.Home))
			{
				_firstVisibleItem = 0;
			}
			else if (Editor.KeyPressed(Keys.End))
			{
				_firstVisibleItem = Math.Max(0, _itemCount - _maxVisibleItems);
			}
		}
	}

	public override void Draw()
	{
		base.Draw();
		if (IsMinimized)
		{
			return;
		}
		DrawBegin();
		for (int i = _firstVisibleItem; i >= 0 && i < _itemCount && i < _firstVisibleItem + _maxVisibleItems; i++)
		{
			PartData partData = _itemList[i];
			Color color = ((i != _currentSelectedItem) ? partData.EditorColor : Color.Yellow);
			Rectangle itemRectangle = GetItemRectangle(i - _firstVisibleItem);
			if (i == _currentItemIndex)
			{
				Editor.TriangleBatch.DrawRect(itemRectangle, new Color(98, 93, 131, 240));
			}
			string text = ((partData.EditorName == null) ? partData.Id : partData.EditorName);
			Editor.Font.Write(Editor.SpriteBatch, text, itemRectangle.X + 5, itemRectangle.Y, color, TextAlign.Left, Width - 10);
		}
		int num = 0;
		int num2 = 0;
		Color color2;
		for (int j = 0; j < Editor.EditorSetEnabledArray.Length; j++)
		{
			num = (int)Position.X + 5 + j * 15;
			num2 = (int)Position.Y + TitleBarHeight + 1;
			if (_selectedEditorSet != j)
			{
				if (Editor.EditorSetEnabledArray[j])
				{
					color2 = new Color(120, 255, 240);
				}
				else
				{
					color2 = ((!Editor.EditorSetManualyDisabledArray[j]) ? new Color(20, 20, 20, 180) : new Color(200, 0, 0, 255));
				}
			}
			else
			{
				color2 = (Editor.EditorSetEnabledArray[j] ? Color.Yellow : new Color(200, 200, 0, 200));
			}
			Editor.Font.Write(Editor.SpriteBatch, j.ToString(), num + 8, num2, color2, TextAlign.Center);
		}
		num += 17;
		if (_isDarkEffectSelected)
		{
			color2 = ((!Editor.DarkDisabledEditorSets) ? Color.Yellow : new Color(80, 200, 80, 220));
		}
		else
		{
			color2 = ((!Editor.DarkDisabledEditorSets) ? new Color(120, 255, 120, 200) : new Color(20, 20, 20, 180));
		}
		Editor.Font.Write(Editor.SpriteBatch, "*", num + 8, num2, color2, TextAlign.Center);
		DrawEnd();
		DrawSlider();
	}
}
