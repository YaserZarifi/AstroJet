using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Editor;

public class PanelGameObject : Panel
{
	private const int LINE_H = 21;

	private TextField _textFieldPositionX;

	private TextField _textFieldPositionY;

	private TextField _textFieldScaleX;

	private TextField _textFieldScaleY;

	private TextField _textFieldRotation;

	private TextField _textFieldColorR;

	private TextField _textFieldColorG;

	private TextField _textFieldColorB;

	private TextField _textFieldColorA;

	private TextField _textFieldZOrder;

	private TextField _textFieldDifficultyMin;

	private TextField _textFieldDifficultyMax;

	private TextField _textFieldGameMode;

	private ComboBox _comboTriggerMode;

	private ComboBox _comboPlayerCount;

	private ColorShifter _colorShifter;

	public bool IsGroupSelected => Editor.IsGroupSelected;

	public PanelGameObject(Editor editor, int x, int y, int width, int height)
		: base(editor, x, y, width, height)
	{
		int num = 27;
		int num2 = 8;
		_textFieldPositionX = new TextField(this);
		_textFieldPositionX.Position.X = num2;
		_textFieldPositionX.Position.Y = num;
		_textFieldPositionX.Label = "Pos:";
		_textFieldPositionX.LabelWidth = 55;
		_textFieldPositionX.Width = 65;
		_textFieldPositionX.Tooltip = "Position X";
		_textFieldPositionX.OnValueChanged += OnPositionXChanged;
		_textFieldPositionY = new TextField(this);
		_textFieldPositionY.Position.X = 83f;
		_textFieldPositionY.Position.Y = num;
		_textFieldPositionY.Width = 65;
		_textFieldPositionY.Tooltip = "Position Y";
		_textFieldPositionY.OnValueChanged += OnPositionYChanged;
		num += 21;
		_textFieldScaleX = new TextField(this);
		_textFieldScaleX.Position.X = num2;
		_textFieldScaleX.Position.Y = num;
		_textFieldScaleX.Label = "Scale:";
		_textFieldScaleX.LabelWidth = 55;
		_textFieldScaleX.Width = 65;
		_textFieldScaleX.SetLimits(-999f, null, 10f, 100f);
		_textFieldScaleX.Tooltip = "Scale X";
		_textFieldScaleX.OnValueChanged += OnScaleXChanged;
		_textFieldScaleY = new TextField(this);
		_textFieldScaleY.Position.X = 83f;
		_textFieldScaleY.Position.Y = num;
		_textFieldScaleY.Width = 65;
		_textFieldScaleY.SetLimits(-999f, null, 10f, 100f);
		_textFieldScaleY.Tooltip = "Scale Y";
		_textFieldScaleY.OnValueChanged += OnScaleYChanged;
		_textFieldScaleX.AddSharedField(_textFieldScaleY);
		_textFieldScaleY.AddSharedField(_textFieldScaleX);
		num += 21;
		_textFieldRotation = new TextField(this);
		_textFieldRotation.Position.X = num2;
		_textFieldRotation.Position.Y = num;
		_textFieldRotation.Label = "Angle:";
		_textFieldRotation.LabelWidth = 55;
		_textFieldRotation.Width = 65;
		_textFieldRotation.SetLimits(-360f, 360f, 0f, 0f);
		_textFieldRotation.Tooltip = "Rotation";
		_textFieldRotation.OnValueChanged += OnRotationChanged;
		num += 21;
		_textFieldColorR = new TextField(this);
		_textFieldColorR.Position.X = num2;
		_textFieldColorR.Position.Y = num;
		_textFieldColorR.Label = "RGB:";
		_textFieldColorR.LabelWidth = 55;
		_textFieldColorR.Width = 35;
		_textFieldColorR.SetLimits(0f, 255f);
		_textFieldColorR.Tooltip = "Color red";
		_textFieldColorR.OnValueChanged += OnColorRChanged;
		num2 += _textFieldColorR.WidthWithLabel + 4;
		_textFieldColorG = new TextField(this);
		_textFieldColorG.Position.X = num2;
		_textFieldColorG.Position.Y = num;
		_textFieldColorG.Width = 35;
		_textFieldColorG.LabelWidth = 0;
		_textFieldColorG.SetLimits(0f, 255f);
		_textFieldColorG.Tooltip = "Color green";
		_textFieldColorG.OnValueChanged += OnColorGChanged;
		num2 += _textFieldColorG.WidthWithLabel + 4;
		_textFieldColorB = new TextField(this);
		_textFieldColorB.Position.X = num2;
		_textFieldColorB.Position.Y = num;
		_textFieldColorB.Width = 35;
		_textFieldColorB.LabelWidth = 0;
		_textFieldColorB.SetLimits(0f, 255f);
		_textFieldColorB.Tooltip = "Color blue";
		_textFieldColorB.OnValueChanged += OnColorBChanged;
		_textFieldColorR.AddSharedField(_textFieldColorG);
		_textFieldColorR.AddSharedField(_textFieldColorB);
		_textFieldColorG.AddSharedField(_textFieldColorR);
		_textFieldColorG.AddSharedField(_textFieldColorB);
		_textFieldColorB.AddSharedField(_textFieldColorR);
		_textFieldColorB.AddSharedField(_textFieldColorG);
		_colorShifter = new ColorShifter(this);
		_colorShifter.Position = _textFieldColorB.Position;
		_colorShifter.Position.X += _textFieldColorB.WidthWithLabel + 2;
		_colorShifter.OnValueChanged += OnColorShifterChanged;
		num += 21;
		num2 = 8;
		_textFieldColorA = new TextField(this);
		_textFieldColorA.Position.X = num2;
		_textFieldColorA.Position.Y = num;
		_textFieldColorA.Label = "Alpha:";
		_textFieldColorA.LabelWidth = 55;
		_textFieldColorA.Width = 35;
		_textFieldColorA.SetLimits(0f, 255f);
		_textFieldColorA.Tooltip = "Color alpha";
		_textFieldColorA.OnValueChanged += OnColorAChanged;
		num += 21;
		num2 = 8;
		_textFieldZOrder = new TextField(this);
		_textFieldZOrder.Position.X = num2;
		_textFieldZOrder.Position.Y = num;
		_textFieldZOrder.Label = "Z-ord:";
		_textFieldZOrder.LabelWidth = 55;
		_textFieldZOrder.Width = 50;
		_textFieldZOrder.SetLimits(-100f, 100f, 0f);
		_textFieldZOrder.Tooltip = "Z order";
		_textFieldZOrder.OnValueChanged += OnZorderChanged;
		_comboTriggerMode = new ComboBox(this);
		_comboTriggerMode.Position.X = num2 + 110;
		_comboTriggerMode.Position.Y = num;
		_comboTriggerMode.Label = "TM:";
		_comboTriggerMode.LabelWidth = 30;
		_comboTriggerMode.Width = 50;
		_comboTriggerMode.Tooltip = "Trigger mode";
		_comboTriggerMode.AddItem(0, "Normal");
		_comboTriggerMode.AddItem(1, "Hidden");
		_comboTriggerMode.OnValueChanged += OnTriggerModeChanged;
		num2 = 8;
		num += 21;
		_textFieldDifficultyMin = new TextField(this);
		_textFieldDifficultyMin.Position.X = num2;
		_textFieldDifficultyMin.Position.Y = num;
		_textFieldDifficultyMin.Label = "Diff:";
		_textFieldDifficultyMin.LabelWidth = 55;
		_textFieldDifficultyMin.Width = 50;
		_textFieldDifficultyMin.SetLimits(0f, 999f, 0f);
		_textFieldDifficultyMin.Tooltip = "Difficulty (min)";
		_textFieldDifficultyMin.OnValueChanged += OnDifficultyChanged;
		num2 += _textFieldDifficultyMin.WidthWithLabel + 4;
		_textFieldDifficultyMax = new TextField(this);
		_textFieldDifficultyMax.Position.X = num2;
		_textFieldDifficultyMax.Position.Y = num;
		_textFieldDifficultyMax.LabelWidth = 0;
		_textFieldDifficultyMax.Width = 50;
		_textFieldDifficultyMax.SetLimits(0f, 999f, 0f);
		_textFieldDifficultyMax.Tooltip = "Difficulty (max)";
		_textFieldDifficultyMax.OnValueChanged += OnDifficultyChanged;
		num2 = 8;
		num += 21;
		_textFieldGameMode = new TextField(this);
		_textFieldGameMode.Position.X = num2;
		_textFieldGameMode.Position.Y = num;
		_textFieldGameMode.VariableType = VariableTypeEnum.String;
		_textFieldGameMode.Label = "GM:";
		_textFieldGameMode.LabelWidth = 55;
		_textFieldGameMode.Width = 134;
		_textFieldGameMode.SetLimits(0f, 999f, 0f);
		_textFieldGameMode.Tooltip = "Game mode (list or empty)";
		_textFieldGameMode.OnValueChanged += OnGameModeChanged;
		num2 = 8;
		num += 21;
		_comboPlayerCount = new ComboBox(this);
		_comboPlayerCount.Position.X = num2;
		_comboPlayerCount.Position.Y = num;
		_comboPlayerCount.Label = "Playrs:";
		_comboPlayerCount.LabelWidth = 55;
		_comboPlayerCount.Width = 50;
		_comboPlayerCount.Tooltip = "For which player count is object active";
		_comboPlayerCount.AddItem(0, "Both");
		_comboPlayerCount.AddItem(1, "One");
		_comboPlayerCount.AddItem(2, "Two");
		_comboPlayerCount.OnValueChanged += OnPlayerCountChanged;
	}

	private void OnScaleXChanged(TextField field, float delta)
	{
		Editor.CurrentObject.SetScale(field.GetFloatValue() / 100f, Editor.CurrentObject.Scale.Y);
	}

	private void OnScaleYChanged(TextField field, float delta)
	{
		Editor.CurrentObject.SetScale(Editor.CurrentObject.Scale.X, field.GetFloatValue() / 100f);
	}

	private void OnPositionXChanged(TextField field, float delta)
	{
		Editor.MoveObject(Editor.CurrentObject, new Vector2(field.GetFloatValue(), Editor.CurrentObject.Position.Y));
	}

	private void OnPositionYChanged(TextField field, float delta)
	{
		Editor.MoveObject(Editor.CurrentObject, new Vector2(Editor.CurrentObject.Position.X, field.GetFloatValue()));
	}

	private void OnRotationChanged(TextField field, float delta)
	{
		Editor.CurrentObject.SetRotation(Utils.DegToRad(field.GetFloatValue()));
	}

	private void OnZorderChanged(TextField field, float delta)
	{
		Editor.CurrentObject.ZOrder = (int)field.GetFloatValue();
		if (Editor.CurrentObject.ZOrder > 1000)
		{
			Editor.CurrentObject.ZOrder = 1000;
		}
		else if (Editor.CurrentObject.ZOrder < -1000)
		{
			Editor.CurrentObject.ZOrder = -1000;
		}
		if (!IsGroupSelected)
		{
			return;
		}
		foreach (GameObject item in Editor.CurrentObject.ChildObjectListFront)
		{
			item.ZOrder = Editor.CurrentObject.ZOrder;
		}
	}

	private void OnTriggerModeChanged(ComboBox combo)
	{
		Editor.CurrentObject.TriggerPointMode = (TriggerPointModeEnum)combo.GetIntValue();
		if (!IsGroupSelected)
		{
			return;
		}
		foreach (GameObject item in Editor.CurrentObject.ChildObjectListFront)
		{
			item.TriggerPointMode = Editor.CurrentObject.TriggerPointMode;
		}
	}

	private void OnDifficultyChanged(TextField field, float delta)
	{
		Editor.CurrentObject.DifficultyRange.From = (int)_textFieldDifficultyMin.GetFloatValue();
		if (IsGroupSelected)
		{
			foreach (GameObject item in Editor.CurrentObject.ChildObjectListFront)
			{
				item.DifficultyRange.From = Editor.CurrentObject.DifficultyRange.From;
			}
		}
		Editor.CurrentObject.DifficultyRange.To = (int)_textFieldDifficultyMax.GetFloatValue();
		if (!IsGroupSelected)
		{
			return;
		}
		foreach (GameObject item2 in Editor.CurrentObject.ChildObjectListFront)
		{
			item2.DifficultyRange.To = Editor.CurrentObject.DifficultyRange.To;
		}
	}

	private void OnGameModeChanged(TextField field, float delta)
	{
		if (_textFieldGameMode.ValueString.Length > 0)
		{
			Utils.TryGetIntArray(_textFieldGameMode.ValueString, 0, 99, out var resultArray);
			Editor.CurrentObject.GameModeList = new List<int>(resultArray);
			if (!IsGroupSelected)
			{
				return;
			}
			{
				foreach (GameObject item in Editor.CurrentObject.ChildObjectListFront)
				{
					item.GameModeList = new List<int>(resultArray);
				}
				return;
			}
		}
		Editor.CurrentObject.GameModeList = null;
		if (!IsGroupSelected)
		{
			return;
		}
		foreach (GameObject item2 in Editor.CurrentObject.ChildObjectListFront)
		{
			item2.GameModeList = null;
		}
	}

	private void OnPlayerCountChanged(ComboBox combo)
	{
		Editor.CurrentObject.ForPlayerCount = combo.GetIntValue();
		if (!IsGroupSelected)
		{
			return;
		}
		foreach (GameObject item in Editor.CurrentObject.ChildObjectListFront)
		{
			item.ForPlayerCount = Editor.CurrentObject.ForPlayerCount;
		}
	}

	private void OnColorRChanged(TextField field, float delta)
	{
		if (delta == 0f)
		{
			Editor.CurrentObject.SetColor((byte)MathHelper.Clamp(field.GetFloatValue(), 0f, 255f), null, null);
			if (!IsGroupSelected)
			{
				return;
			}
			{
				foreach (GameObject item in Editor.CurrentObject.ChildObjectListFront)
				{
					item.SetColor(Editor.CurrentObject.Color.R, null, null);
				}
				return;
			}
		}
		Editor.CurrentObject.ChangeColor((int)delta, 0, 0, 0);
		if (!IsGroupSelected)
		{
			return;
		}
		foreach (GameObject item2 in Editor.CurrentObject.ChildObjectListFront)
		{
			item2.ChangeColor((int)delta, 0, 0, 0);
		}
	}

	private void OnColorGChanged(TextField field, float delta)
	{
		if (delta == 0f)
		{
			Editor.CurrentObject.SetColor(null, (byte)MathHelper.Clamp(field.GetFloatValue(), 0f, 255f), null);
			if (!IsGroupSelected)
			{
				return;
			}
			{
				foreach (GameObject item in Editor.CurrentObject.ChildObjectListFront)
				{
					item.SetColor(null, Editor.CurrentObject.Color.G, null);
				}
				return;
			}
		}
		Editor.CurrentObject.ChangeColor(0, (int)delta, 0, 0);
		if (!IsGroupSelected)
		{
			return;
		}
		foreach (GameObject item2 in Editor.CurrentObject.ChildObjectListFront)
		{
			item2.ChangeColor(0, (int)delta, 0, 0);
		}
	}

	private void OnColorBChanged(TextField field, float delta)
	{
		if (delta == 0f)
		{
			Editor.CurrentObject.SetColor(null, null, (byte)MathHelper.Clamp(field.GetFloatValue(), 0f, 255f));
			if (!IsGroupSelected)
			{
				return;
			}
			{
				foreach (GameObject item in Editor.CurrentObject.ChildObjectListFront)
				{
					item.SetColor(null, null, Editor.CurrentObject.Color.B);
				}
				return;
			}
		}
		Editor.CurrentObject.ChangeColor(0, 0, (int)delta, 0);
		if (!IsGroupSelected)
		{
			return;
		}
		foreach (GameObject item2 in Editor.CurrentObject.ChildObjectListFront)
		{
			item2.ChangeColor(0, 0, (int)delta, 0);
		}
	}

	private void OnColorAChanged(TextField field, float delta)
	{
		if (delta == 0f)
		{
			Editor.CurrentObject.SetColor(null, null, null, (byte)MathHelper.Clamp(field.GetFloatValue(), 0f, 255f));
			if (!IsGroupSelected)
			{
				return;
			}
			{
				foreach (GameObject item in Editor.CurrentObject.ChildObjectListFront)
				{
					item.SetColor(null, null, null, Editor.CurrentObject.Color.A);
				}
				return;
			}
		}
		Editor.CurrentObject.ChangeColor(0, 0, 0, (int)delta);
		if (!IsGroupSelected)
		{
			return;
		}
		foreach (GameObject item2 in Editor.CurrentObject.ChildObjectListFront)
		{
			item2.ChangeColor(0, 0, 0, (int)delta);
		}
	}

	private void OnColorShifterChanged(ColorShifter shifter)
	{
		if (Editor.CurrentObject == null)
		{
			return;
		}
		Editor.CurrentObject.SetColor(shifter.Value, Editor.CurrentObject.Color.A);
		if (!Editor.IsGroupSelected)
		{
			return;
		}
		foreach (GameObject item in Editor.CurrentObject.ChildObjectListFront)
		{
			item.SetColor(shifter.Value, item.Color.A);
		}
	}

	public override void Update(float elapsedTime)
	{
		if (!IsEnabled || IsOff)
		{
			return;
		}
		if (IsGroupSelected)
		{
			BackgroundColor = new Color(10, 40, 80, 230);
		}
		else
		{
			BackgroundColor = new Color(60, 10, 80, 230);
		}
		base.Update(elapsedTime);
		if (IsMinimized)
		{
			return;
		}
		if (Editor.CurrentObject != null)
		{
			GameObject currentObject = Editor.CurrentObject;
			if (!_textFieldPositionX.HasFocus)
			{
				_textFieldPositionX.UpdateValue(currentObject.Position.X);
			}
			if (!_textFieldPositionY.HasFocus)
			{
				_textFieldPositionY.UpdateValue(currentObject.Position.Y);
			}
			if (!_textFieldScaleX.HasFocus)
			{
				_textFieldScaleX.UpdateValue(currentObject.Scale.X * 100f);
			}
			if (!_textFieldScaleY.HasFocus)
			{
				_textFieldScaleY.UpdateValue(currentObject.Scale.Y * 100f);
			}
			if (!_textFieldRotation.HasFocus)
			{
				_textFieldRotation.UpdateValue(Utils.RadToDeg(currentObject.Rotation));
			}
			if (!_textFieldColorR.HasFocus)
			{
				_textFieldColorR.UpdateValue((int)currentObject.Color.R);
			}
			if (!_textFieldColorG.HasFocus)
			{
				_textFieldColorG.UpdateValue((int)currentObject.Color.G);
			}
			if (!_textFieldColorB.HasFocus)
			{
				_textFieldColorB.UpdateValue((int)currentObject.Color.B);
			}
			if (!_textFieldColorA.HasFocus)
			{
				_textFieldColorA.UpdateValue((int)currentObject.Color.A);
			}
			_colorShifter.Value = currentObject.Color;
			if (!_textFieldZOrder.HasFocus)
			{
				_textFieldZOrder.UpdateValue(currentObject.ZOrder);
			}
			_comboTriggerMode.ChooseIntValue((int)currentObject.TriggerPointMode);
			if (!_textFieldDifficultyMin.HasFocus)
			{
				_textFieldDifficultyMin.UpdateValue(currentObject.DifficultyRange.From);
			}
			if (!_textFieldDifficultyMax.HasFocus)
			{
				_textFieldDifficultyMax.UpdateValue(currentObject.DifficultyRange.To);
			}
			string text = "";
			if (!_textFieldGameMode.HasFocus)
			{
				if (!currentObject.GameModeList.IsNullOrEmpty())
				{
					for (int i = 0; i < currentObject.GameModeList.Count; i++)
					{
						if (i > 0)
						{
							text += ",";
						}
						text += currentObject.GameModeList[i];
					}
				}
				_textFieldGameMode.SetStringValue(text);
			}
			_comboPlayerCount.ChooseIntValue(currentObject.ForPlayerCount);
			if (currentObject != null)
			{
				TitleText = currentObject.Id;
			}
		}
		else
		{
			TitleText = "";
		}
	}

	public override void Draw()
	{
		if (IsEnabled && !IsOff)
		{
			base.Draw();
		}
	}
}
