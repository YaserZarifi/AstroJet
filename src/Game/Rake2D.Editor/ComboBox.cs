using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Rake2D.Script;

namespace Rake2D.Editor;

public class ComboBox : FieldComponent
{
	public delegate void OnValueChangedHandler(ComboBox component);

	public VariableTypeEnum VariableType;

	private List<ComboItem> ItemList;

	private Vector2 SelectionPosition;

	private int SelectionHeight;

	private bool _showSelection;

	public string Value { get; private set; }

	public int WidthWithLabel
	{
		get
		{
			if (LabelPosition == LabelPosition.Left)
			{
				return Width + LabelWidth;
			}
			return Width;
		}
	}

	public event OnValueChangedHandler OnValueChanged;

	public override void SetFocus(bool bIgnoreFirstInput)
	{
		base.SetFocus(bIgnoreFirstInput);
	}

	public ComboBox(Panel panel)
		: base(panel)
	{
		LabelColor = Color.White;
		ValueColor = Color.LightSteelBlue;
		LabelWidth = 50;
		Width = 50;
		Height = 17;
		LabelHeight = 16;
		Label = "";
		IsFocusable = true;
		ItemList = new List<ComboItem>();
	}

	public string GetValue()
	{
		string text = "";
		foreach (ComboItem item in ItemList)
		{
			if (item.IsChoosen)
			{
				if (text.Length > 0)
				{
					text += ", ";
				}
				text += item.Id;
			}
		}
		return text;
	}

	public string GetText()
	{
		string text = "";
		foreach (ComboItem item in ItemList)
		{
			if (item.IsChoosen)
			{
				if (text.Length > 0)
				{
					text += ", ";
				}
				text += item.Text;
			}
		}
		return text;
	}

	public int GetChoosenCount()
	{
		int num = 0;
		foreach (ComboItem item in ItemList)
		{
			if (item.IsChoosen)
			{
				num++;
			}
		}
		return num;
	}

	public void AddItem(string id, string text)
	{
		ComboItem item = new ComboItem(id, text);
		ItemList.Add(item);
	}

	public void AddItem(int id, string text)
	{
		ComboItem item = new ComboItem(id.ToString(), text);
		ItemList.Add(item);
	}

	private void DrawFrame()
	{
		Vector2 vector = Panel.Position + Position;
		if (LabelPosition == LabelPosition.Left)
		{
			vector.X += LabelWidth;
		}
		else
		{
			vector.Y += LabelHeight + 1;
		}
		Editor.LineBatch.DrawRect(vector, Width, Height, FrameColor, RectangleFrameType.Lowered);
		Vector2 bottomRight = new Vector2(vector.X + (float)Width - 1f, vector.Y + (float)Height - 1f);
		if (HasFocus)
		{
			Editor.TriangleBatch.DrawRect(vector + Vector2.One, bottomRight, FrameColor * 0.6f);
		}
		else
		{
			Editor.TriangleBatch.DrawRect(vector + Vector2.One, bottomRight, FrameColor * 0.2f);
		}
	}

	private void ParseInput()
	{
		if (HasFocus && GameInput.SystemKeyPressed(Keys.Escape))
		{
			Editor.SetFocusComponent(null);
		}
	}

	public void UnchooseAll()
	{
		foreach (ComboItem item in ItemList)
		{
			item.IsChoosen = false;
		}
	}

	public void ChooseByValue(string value)
	{
		if (value.IsNullOrEmpty())
		{
			UnchooseAll();
			return;
		}
		string[] array = value.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			foreach (ComboItem item in ItemList)
			{
				if (Utils.Equals(item.Id, text.Trim()))
				{
					item.IsChoosen = true;
					break;
				}
			}
		}
	}

	public void ChooseValue(string value)
	{
		UnchooseAll();
		foreach (ComboItem item in ItemList)
		{
			if (Utils.Equals(value, item.Id))
			{
				Value = value;
				item.IsChoosen = true;
				break;
			}
		}
	}

	public void ChooseIntValue(int value)
	{
		UnchooseAll();
		string text = value.ToString();
		foreach (ComboItem item in ItemList)
		{
			if (Utils.Equals(text, item.Id))
			{
				Value = text;
				item.IsChoosen = true;
				break;
			}
		}
	}

	private void UpdateSelection()
	{
		SelectionPosition = Panel.Position + Position;
		if (LabelPosition == LabelPosition.Left)
		{
			SelectionPosition.X += LabelWidth;
		}
		SelectionHeight = ItemList.Count * 20 + 8;
		SelectionPosition.Y += Height;
		if (Editor.Cursor.IsInRect(SelectionPosition, Width, SelectionHeight))
		{
			Editor.SelectedDragAction = DragAction.OnPanel;
			Editor.SelectedComponent = this;
		}
		ComboItem comboItem = null;
		int num = 0;
		foreach (ComboItem item in ItemList)
		{
			item.Position = new Vector2(SelectionPosition.X + 4f, SelectionPosition.Y + (float)(num * 20) + 4f);
			if (Editor.Cursor.IsInRect(item.Position, Width, 20))
			{
				item.IsSelected = true;
				comboItem = item;
				Editor.SelectedDragAction = DragAction.OnPanelComponent;
				Editor.SelectedComponent = this;
				Editor.Cursor.Text = "";
			}
			else
			{
				item.IsSelected = false;
			}
			num++;
		}
		if (GameInput.MousePressed(GameInput.MouseButton.Right))
		{
			Editor.SetFocusComponent(null);
			GameInput.CancelMousePressed();
		}
		else
		{
			if (!HasFocus || (!GameInput.SystemKeyPressed(Keys.Enter) && !GameInput.MouseReleased(GameInput.MouseButton.Left)))
			{
				return;
			}
			Editor.DragAction = DragAction.OnPanelComponent;
			UnchooseAll();
			if (comboItem != null)
			{
				if (!comboItem.Id.IsNullOrEmpty())
				{
					comboItem.IsChoosen = true;
				}
				OnFieldSet();
			}
			Editor.SetFocusComponent(null);
			GameInput.CancelMousePressed();
			_showSelection = false;
		}
	}

	public override void Update(float elapsedTime)
	{
		IsSelected = false;
		if (!HasFocus)
		{
			_showSelection = false;
		}
		if ((Editor.SelectedComponent == null || Editor.SelectedComponent == this) && Editor.SelectedPanel == Panel)
		{
			if (!_showSelection)
			{
				if (LabelPosition == LabelPosition.Left)
				{
					IsSelected = Editor.Cursor.IsInRect(ScreenX + LabelWidth, ScreenY, Width, Height);
				}
				else
				{
					IsSelected = Editor.Cursor.IsInRect(ScreenX, ScreenY + LabelHeight, Width, Height);
				}
			}
			if (IsSelected)
			{
				Editor.SelectedDragAction = DragAction.OnPanelComponent;
				Editor.Cursor.Text = Tooltip ?? "Choose value";
				if (GameInput.MousePressed(GameInput.MouseButton.Left))
				{
					Editor.DragAction = DragAction.OnPanelComponent;
					Editor.DragCenter = Editor.Cursor.Position;
					SetFocus(bIgnoreFirstInput: false);
				}
			}
		}
		if (HasFocus && (GameInput.MouseReleased(GameInput.MouseButton.Left) || GameInput.SystemKeyPressed(Keys.Enter)))
		{
			_showSelection = true;
			GameInput.CancelKeyPressed();
		}
		if (!HasFocus && IsSelected && Editor.KeyPressed(Keys.Delete))
		{
			UnchooseAll();
			OnFieldSet();
		}
		ParseInput();
	}

	public override void UpdateTop(float elapsedTime)
	{
		if (_showSelection)
		{
			UpdateSelection();
		}
	}

	private void OnFieldSet()
	{
		Value = GetValue();
		if (BindedData != null)
		{
			if (GetChoosenCount() == 0)
			{
				OnFieldClear();
				if (Editor.CurrentObject != null)
				{
					Editor.CurrentObject.DoZeroEvents();
				}
			}
			else
			{
				BehaviorEvent behaviorEvent;
				if (BindedData.GetType() == typeof(BehaviorEventNode))
				{
					BehaviorEventNode behaviorEventNode = (BehaviorEventNode)BindedData;
					string[] array = Label.Split('/');
					behaviorEvent = ((array.Length != 3) ? behaviorEventNode.AddEvent(null, array[0], array[1], 0) : behaviorEventNode.AddEvent(array[0], array[1], array[2], 0));
					behaviorEvent.SetObject(Editor.CurrentObject);
					BindedData = behaviorEvent;
				}
				else
				{
					behaviorEvent = (BehaviorEvent)BindedData;
				}
				behaviorEvent.ValueString = Value;
				if (behaviorEvent != null && behaviorEvent.Time <= 0f)
				{
					behaviorEvent.MyObject.DoZeroEvents();
				}
			}
		}
		OnValueChanged?.Invoke(this);
	}

	public void OnFieldClear()
	{
		if (BindedData != null)
		{
			if (BindedData.GetType() == typeof(BehaviorEvent))
			{
				BehaviorEvent behaviorEvent = (BehaviorEvent)BindedData;
				behaviorEvent.EventNode.EventList.Remove(behaviorEvent);
			}
			BindedData = null;
		}
	}

	public void ClearValue()
	{
	}

	public int GetIntValue()
	{
		if (!int.TryParse(Value, out var result))
		{
			return 0;
		}
		return result;
	}

	private void DrawSelection()
	{
		Color color = new Color(80, 40, 40, 255);
		Editor.TriangleBatch.Flush();
		Editor.LineBatch.Flush();
		Editor.SpriteBatch.End();
		Editor.TriangleBatch.DrawRect(SelectionPosition.X + 8f, SelectionPosition.Y + 8f, Width, SelectionHeight, new Color(0, 0, 0, 100));
		Editor.TriangleBatch.DrawRect(SelectionPosition.X, SelectionPosition.Y, Width, SelectionHeight, color);
		Editor.LineBatch.DrawRect(SelectionPosition, Width, SelectionHeight, color, RectangleFrameType.Raised);
		Editor.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
		int num = 0;
		foreach (ComboItem item in ItemList)
		{
			Color color2;
			if (item.IsSelected)
			{
				color2 = new Color(255, 255, 20);
			}
			else
			{
				color2 = ((!item.IsChoosen) ? new Color(200, 220, 200) : new Color(20, 255, 20));
			}
			Editor.Font.Write(Editor.SpriteBatch, item.Text, item.Position, color2, TextAlign.Left, Width - 8);
			num++;
		}
	}

	public override void Draw()
	{
		base.Draw();
		DrawFrame();
		Color color;
		if (IsSelected)
		{
			color = Color.Yellow;
		}
		else
		{
			color = ((!HasFocus) ? ValueColor : new Color(100, 255, 100));
		}
		if (LabelPosition == LabelPosition.Left)
		{
			Editor.Font.Write(Editor.SpriteBatch, Label, ScreenX, ScreenY, LabelColor);
			Editor.Font.Write(Editor.SpriteBatch, GetText(), ScreenX + LabelWidth + 2, ScreenY, color);
		}
		else
		{
			Editor.Font.Write(Editor.SpriteBatch, Label, ScreenX, ScreenY, LabelColor, TextAlign.Left, Panel.Width - 10);
			Editor.Font.Write(Editor.SpriteBatch, GetText(), ScreenX + 2, ScreenY + LabelHeight + 1, color, TextAlign.Left, Width - 8);
		}
	}

	public override void DrawTop()
	{
		base.DrawTop();
		if (_showSelection)
		{
			DrawSelection();
		}
	}
}
