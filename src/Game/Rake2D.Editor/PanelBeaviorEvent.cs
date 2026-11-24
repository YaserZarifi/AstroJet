using System;
using Microsoft.Xna.Framework;
using Rake2D.Script;

namespace Rake2D.Editor;

public class PanelBeaviorEvent : Panel
{
	private const int LINE_H = 36;

	private const int TEXT_X = 4;

	private const int TEXT_Y = 4;

	public BehaviorEventNode CurrentEventNode;

	public BehaviorEventNode SelectedEventNode;

	private TextField _textFieldDifficultyFrom;

	private TextField _textFieldDifficultyTo;

	public PanelBeaviorEvent(Editor editor, int x, int y, int width, int height)
		: base(editor, x, y, width, height)
	{
	}

	public override void Update(float elapsedTime)
	{
		IsOff = CurrentEventNode == null && SelectedEventNode == null;
		if (IsEnabled && !IsOff)
		{
			UpdateValues();
			base.Update(elapsedTime);
		}
	}

	private void SetWidthByEventList(GameObject go)
	{
		if (go.PartData != null && go.PartData.AvailableEventTypes != null)
		{
			foreach (BehaviorEventType value in go.PartData.AvailableEventTypes.Values)
			{
				Width = Math.Max(Width, Editor.Font.GetTextWidth(value.Name) + 12);
			}
		}
		go.ChildObjectListFront?.ForEach((GameObject child) =>
		{
			SetWidthByEventList(child);
		});
		go.ChildObjectListBack?.ForEach((GameObject child) =>
		{
			SetWidthByEventList(child);
		});
	}

	private void SetEventsByObject(GameObject go, ref int lineNumber)
	{
		if (go.PartData.AvailableEventTypes != null)
		{
			foreach (BehaviorEventType value in go.PartData.AvailableEventTypes.Values)
			{
				Vector2 position = new Vector2(4f, 4 + lineNumber * 36 + TitleBarHeight);
				if (value.Type == BehaviorEventTypeEnum.State)
				{
					ComboBox comboBox = new ComboBox(this);
					comboBox.Width = Width - 8;
					comboBox.Position = position;
					comboBox.Label = value.Name;
					comboBox.LabelPosition = LabelPosition.Top;
					comboBox.AddItem("", "(none)");
					if (go.PartData != null && !go.PartData.PartStates.IsNullOrEmpty())
					{
						foreach (PartState value2 in go.PartData.PartStates.Values)
						{
							if (go.PartData.ListedStateSet == null || go.PartData.ListedStateSet.Contains(value2.Id))
							{
								comboBox.AddItem(text: (go.PartData.DefaultStateName == null || !(value2.Id == "idle")) ? value2.Id : go.PartData.DefaultStateName, id: value2.Id);
							}
						}
					}
				}
				else
				{
					new TextField(this)
					{
						Width = Width - 8,
						Position = position,
						Label = value.Name,
						LabelPosition = LabelPosition.Top
					};
				}
				lineNumber++;
			}
		}
		if (!go.ChildObjectListFront.IsNullOrEmpty())
		{
			foreach (GameObject item in go.ChildObjectListFront)
			{
				SetEventsByObject(item, ref lineNumber);
			}
		}
		if (!go.ChildObjectListBack.IsNullOrEmpty())
		{
			foreach (GameObject item2 in go.ChildObjectListBack)
			{
				SetEventsByObject(item2, ref lineNumber);
			}
		}
		if (go.IsMainPart)
		{
			int num = (Width - 24) / 2;
			_textFieldDifficultyFrom = new TextField(this);
			_textFieldDifficultyFrom.Width = num;
			_textFieldDifficultyFrom.Position = new Vector2(4f, 4 + lineNumber * 36 + TitleBarHeight);
			_textFieldDifficultyFrom.Label = "Difficulty";
			_textFieldDifficultyFrom.LabelPosition = LabelPosition.Top;
			_textFieldDifficultyFrom.SetLimits(0f, 999f, 0f, 0f);
			_textFieldDifficultyFrom.VariableType = VariableTypeEnum.Int;
			_textFieldDifficultyFrom.OnValueChanged += OnDifficultyFromChanged;
			_textFieldDifficultyTo = new TextField(this);
			_textFieldDifficultyTo.Width = num;
			_textFieldDifficultyTo.Position = new Vector2(4 + num + 4, 4 + lineNumber * 36 + TitleBarHeight + _textFieldDifficultyTo.LabelHeight);
			_textFieldDifficultyTo.LabelWidth = 12;
			_textFieldDifficultyTo.Label = "-";
			_textFieldDifficultyTo.SetLimits(0f, 999f, 999f, 999f);
			_textFieldDifficultyTo.VariableType = VariableTypeEnum.Int;
			_textFieldDifficultyTo.OnValueChanged += OnDifficultyToChanged;
			lineNumber++;
		}
	}

	private void OnDifficultyFromChanged(TextField component, float delta)
	{
		if (CurrentEventNode != null)
		{
			CurrentEventNode.DifficultyRange.From = (int)component.GetFloatValue();
		}
	}

	private void OnDifficultyToChanged(TextField component, float delta)
	{
		if (CurrentEventNode != null)
		{
			CurrentEventNode.DifficultyRange.To = (int)component.GetFloatValue();
		}
	}

	public void SetCurrentEventNode(GameObject go, BehaviorEventNode eventNode)
	{
		CurrentEventNode = eventNode;
		SetEventNode(go, eventNode);
	}

	public void SetSelectedEventNode(GameObject go, BehaviorEventNode eventNode)
	{
		if (eventNode != SelectedEventNode)
		{
			SelectedEventNode = eventNode;
			SetEventNode(go, eventNode ?? CurrentEventNode);
		}
	}

	private void SetEventNode(GameObject go, BehaviorEventNode eventNode)
	{
		_componentList.Clear();
		BehaviorTimeLine behaviorTimeLine = ((eventNode == null || eventNode.MyObject == null || eventNode.MyObject.CurrentTimeLine == null) ? (Editor.ToolTimeLine.CurrentTimeLine ?? Editor.ToolTimeLine.SelectedTimeLine) : eventNode.MyObject.CurrentTimeLine);
		if (eventNode != null)
		{
			if (eventNode.IsOnTimeLine && behaviorTimeLine != null)
			{
				TitleBarTextColor = new Color(100, 200, 255);
				if (behaviorTimeLine.EventNodeList != null)
				{
					TitleText = $"Time events [{behaviorTimeLine.EventNodeList.IndexOf(eventNode) + 1}/{behaviorTimeLine.EventNodeList.Count}]";
				}
				else
				{
					TitleText = "Time events [0/0]";
				}
			}
			else if (go != null && go.MyPath != null)
			{
				TitleBarTextColor = new Color(100, 255, 100);
				TitleText = $"Path events [{go.MyPath.EventNodeList.IndexOf(eventNode) + 1}/{go.MyPath.EventNodeList.Count}]";
			}
			else
			{
				TitleBarTextColor = new Color(255, 100, 100);
				TitleText = "No node";
			}
		}
		int lineNumber = 0;
		if (go != null && eventNode != null)
		{
			SetWidthByEventList(go);
			SetEventsByObject(go, ref lineNumber);
			Height = 20 + lineNumber * 36 + TitleBarHeight;
		}
		UpdateValues();
	}

	public void UpdateValues()
	{
		BehaviorEventNode behaviorEventNode = SelectedEventNode ?? CurrentEventNode;
		if (behaviorEventNode == null || behaviorEventNode.MyObject == null)
		{
			return;
		}
		foreach (PanelComponent component in _componentList)
		{
			if (!(component is FieldComponent fieldComponent) || Editor.FocusComponent == fieldComponent || fieldComponent == _textFieldDifficultyFrom || fieldComponent == _textFieldDifficultyTo)
			{
				continue;
			}
			string[] array = fieldComponent.Label.Split('/');
			string eventName;
			GameObject eventObject;
			if (array.Length == 3)
			{
				eventName = array[2];
				eventObject = behaviorEventNode.MyObject.GetChildByIdDeep(array[0]);
			}
			else
			{
				if (array.Length <= 1)
				{
					continue;
				}
				eventName = array[1];
				eventObject = behaviorEventNode.MyObject;
			}
			BehaviorEvent behaviorEvent = behaviorEventNode.EventList.Find((BehaviorEvent o) => o.Name == eventName && o.MyObject == eventObject);
			if (behaviorEvent != null)
			{
				if (fieldComponent.GetType() == typeof(TextField))
				{
					((TextField)fieldComponent).VariableType = VariableTypeEnum.Int;
					((TextField)fieldComponent).UpdateValue(behaviorEvent.ValueInt);
				}
				else
				{
					((ComboBox)fieldComponent).ChooseByValue(behaviorEvent.ValueString);
				}
				fieldComponent.LabelColor = new Color(100, 230, 255);
				fieldComponent.ValueColor = new Color(120, 255, 100);
				fieldComponent.BindedData = behaviorEvent;
				continue;
			}
			if (fieldComponent.GetType() == typeof(TextField))
			{
				((TextField)fieldComponent).VariableType = VariableTypeEnum.String;
			}
			else if (fieldComponent.GetType() == typeof(ComboBox))
			{
				((ComboBox)fieldComponent).UnchooseAll();
			}
			fieldComponent.LabelColor = new Color(160, 160, 160);
			fieldComponent.ValueColor = new Color(180, 180, 180, 200);
			fieldComponent.BindedData = behaviorEventNode;
		}
		if (Editor.FocusComponent == null)
		{
			_textFieldDifficultyFrom.SetValue(behaviorEventNode.DifficultyRange.From, bCallEvents: false);
			_textFieldDifficultyTo.SetValue(behaviorEventNode.DifficultyRange.To, bCallEvents: false);
		}
	}

	public override void Draw()
	{
		if (!IsEnabled || IsOff)
		{
			return;
		}
		if (SelectedEventNode != null && SelectedEventNode != CurrentEventNode)
		{
			BackgroundColor = new Color(40, 80, 80, 200);
			TitleBarBackgroundColor = new Color(20, 60, 60, 220);
		}
		else
		{
			BackgroundColor = new Color(40, 100, 50, 220);
			TitleBarBackgroundColor = new Color(30, 80, 40, 220);
		}
		base.Draw();
		if (!IsMinimized)
		{
			DrawBegin();
			if (_componentList.IsNullOrEmpty())
			{
				Editor.Font.Write(Editor.SpriteBatch, "- No events -", Position.X + (float)(Width / 2), Position.Y + (float)TitleBarHeight + 1f, new Color(200, 255, 200, 255), TextAlign.Center, Width - 10);
			}
			DrawEnd();
		}
	}
}
