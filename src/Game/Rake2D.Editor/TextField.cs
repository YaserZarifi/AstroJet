using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Rake2D.Script;

namespace Rake2D.Editor;

public class TextField : FieldComponent
{
	public delegate void OnValueChangedHandler(TextField component, float delta);

	public delegate void OnMousePressedHandler(TextField component, GameInput.MouseButton mouseButton);

	public delegate void OnMouseOverHandler(TextField component);

	public VariableTypeEnum VariableType;

	public float ValueStepSmall = 1f;

	public float ValueStepMedium = 10f;

	public float ValueStepBig = 50f;

	public float ValueRoundFactor = 1f;

	public float? MinValue;

	public float? MaxValue;

	public float? DefaultValueMin;

	public float? DefaultValueMax;

	public string DefaultTextValue;

	private List<TextField> _shareFieldList;

	private bool _bCanClickSetFocus = true;

	private bool _ignoreFirstInput;

	private bool _clearValueOnInput;

	private float _cursorTimer;

	private int _cursorPos;

	public string ValueString { get; protected set; }

	public float FloatValue { get; protected set; }

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

	public event OnMousePressedHandler OnMousePressed;

	public event OnMouseOverHandler OnMouseOver;

	public override void SetFocus(bool bIgnoreFirstInput)
	{
		ValueString = GetFormattedValue();
		base.SetFocus(bIgnoreFirstInput);
		_cursorPos = ValueString.Length;
		_ignoreFirstInput = bIgnoreFirstInput;
		if (_ignoreFirstInput)
		{
			_clearValueOnInput = true;
		}
	}

	public override void CancelFocus()
	{
		if (ValueString.Length == 0)
		{
			OnFieldClear();
		}
	}

	public void SetLimits(float? min, float? max, float? defaultValueMin = null, float? defaultValueMax = null)
	{
		MinValue = min;
		MaxValue = max;
		DefaultValueMin = defaultValueMin;
		DefaultValueMax = defaultValueMax;
	}

	public void SetSteps(float smallStep, float mediumStep, float bigStep)
	{
		ValueStepSmall = smallStep;
		ValueStepMedium = mediumStep;
		ValueStepBig = bigStep;
	}

	public TextField(Panel panel)
		: base(panel)
	{
		LabelColor = Color.White;
		LabelWidth = 50;
		Width = 50;
		Height = 17;
		LabelHeight = 16;
		Label = "";
		ValueString = "";
		IsFocusable = true;
	}

	public void AddSharedField(TextField textField)
	{
		if (_shareFieldList == null)
		{
			_shareFieldList = new List<TextField>();
		}
		_shareFieldList.Add(textField);
	}

	private void ParseInput()
	{
		float num = 0f;
		if (IsSelected && VariableType != VariableTypeEnum.String)
		{
			if (GameInput.SystemKeyPressed(Keys.Add) || GameInput.SystemKeyPressed(Keys.OemPlus))
			{
				if (GameInput.ShiftDown)
				{
					num = ValueStepMedium;
				}
				else
				{
					num = (GameInput.ControlDown ? ValueStepBig : ValueStepSmall);
				}
			}
			else if (Editor.KeyPressed(Keys.Subtract) || Editor.KeyPressed(Keys.OemMinus))
			{
				num = ((!GameInput.ShiftDown) ? (0f - (GameInput.ControlDown ? ValueStepBig : ValueStepSmall)) : (0f - ValueStepMedium));
			}
			else if (GameInput.SystemKeyPressed(Keys.PageUp))
			{
				num = (GameInput.ControlDown ? ValueStepBig : ValueStepMedium);
			}
			else if (GameInput.SystemKeyPressed(Keys.PageDown))
			{
				num = 0f - (GameInput.ControlDown ? ValueStepBig : ValueStepMedium);
			}
			else if (GameInput.SystemKeyPressed(Keys.Home))
			{
				if (DefaultValueMin.HasValue)
				{
					SetValue(DefaultValueMin.Value);
				}
				else if (MinValue.HasValue)
				{
					SetValue(MinValue.Value);
				}
			}
			else if (GameInput.SystemKeyPressed(Keys.End))
			{
				if (DefaultValueMax.HasValue)
				{
					SetValue(DefaultValueMax.Value);
				}
				else if (MaxValue.HasValue)
				{
					SetValue(MaxValue.Value);
				}
			}
			else if (GameInput.GetMouseWheelDelta() > 0)
			{
				if (GameInput.ShiftDown)
				{
					num = ValueStepMedium;
				}
				else
				{
					num = (GameInput.ControlDown ? ValueStepBig : ValueStepSmall);
				}
			}
			else if (GameInput.GetMouseWheelDelta() < 0)
			{
				num = ((!GameInput.ShiftDown) ? (0f - (GameInput.ControlDown ? ValueStepBig : ValueStepSmall)) : (0f - ValueStepMedium));
			}
			if (GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				if (GameInput.DoubleClick)
				{
					Editor.SetFocusComponent(null);
					_bCanClickSetFocus = false;
					if (DefaultTextValue != null)
					{
						SetStringValue(DefaultTextValue);
					}
					else
					{
						_bCanClickSetFocus = true;
						if (DefaultValueMax.HasValue && GetFloatValue() < DefaultValueMax)
						{
							SetValue(DefaultValueMax.Value);
						}
						else if (DefaultValueMin.HasValue && GetFloatValue() > DefaultValueMin)
						{
							SetValue(DefaultValueMin.Value);
						}
						else
						{
							float floatValue = GetFloatValue();
							float? maxValue = MaxValue;
							if (floatValue >= maxValue.GetValueOrDefault() && maxValue.HasValue && MinValue.HasValue)
							{
								SetValue(MinValue.Value);
							}
							else if (MaxValue.HasValue)
							{
								SetValue(MaxValue.Value);
							}
						}
					}
				}
				else
				{
					OnMousePressed?.Invoke(this, GameInput.MouseButton.Left);
				}
			}
			if (num != 0f)
			{
				IncreaseValue(num);
				Editor.SetFocusComponent(null);
			}
		}
		if (_cursorPos > ValueString.Length)
		{
			_cursorPos = ValueString.Length;
		}
		if (!HasFocus)
		{
			return;
		}
		string text = null;
		if (GameInput.ControlDown)
		{
			return;
		}
		if (GameInput.SystemKeyPressed(Keys.Escape) || GameInput.SystemKeyPressed(Keys.F1) || GameInput.SystemKeyPressed(Keys.F2) || GameInput.SystemKeyPressed(Keys.F3) || GameInput.SystemKeyPressed(Keys.F4) || GameInput.SystemKeyPressed(Keys.F5) || GameInput.SystemKeyPressed(Keys.F6) || GameInput.SystemKeyPressed(Keys.F7) || GameInput.SystemKeyPressed(Keys.F8) || GameInput.SystemKeyPressed(Keys.F9) || GameInput.SystemKeyPressed(Keys.F10) || GameInput.SystemKeyPressed(Keys.F11) || GameInput.SystemKeyPressed(Keys.F12))
		{
			Editor.SetFocusComponent(null);
			return;
		}
		if (_clearValueOnInput && GameInput.SystemGetPressedKey() != Keys.None)
		{
			if (!GameInput.SystemKeyPressed(Keys.Left) && !GameInput.SystemKeyPressed(Keys.Right) && !GameInput.SystemKeyPressed(Keys.Up) && !GameInput.SystemKeyPressed(Keys.Down) && !GameInput.SystemKeyPressed(Keys.Back) && !GameInput.SystemKeyPressed(Keys.Home) && !GameInput.SystemKeyPressed(Keys.End) && !GameInput.SystemKeyPressed(Keys.Enter) && !GameInput.SystemKeyPressed(Keys.LeftShift) && !GameInput.SystemKeyPressed(Keys.RightShift) && !GameInput.SystemKeyPressed(Keys.LeftAlt) && !GameInput.SystemKeyPressed(Keys.RightAlt))
			{
				SetStringValue("", bCallEvents: false);
				_cursorPos = 0;
			}
			_clearValueOnInput = false;
		}
		for (int i = 0; i < 10; i++)
		{
			if (GameInput.SystemKeyPressed((Keys)(96 + i)) || GameInput.SystemKeyPressed((Keys)(48 + i)))
			{
				text = i.ToString();
			}
		}
		if (VariableType == VariableTypeEnum.String)
		{
			for (Keys keys = Keys.A; keys <= Keys.Z; keys++)
			{
				if (GameInput.SystemKeyPressed(keys))
				{
					text = ((!GameInput.ShiftDown) ? keys.ToString().ToLower() : keys.ToString());
				}
			}
			if (GameInput.SystemKeyPressed(Keys.Multiply))
			{
				text = "*";
			}
			else if (GameInput.SystemKeyPressed(Keys.OemComma))
			{
				text = ",";
			}
			else if (GameInput.SystemKeyPressed(Keys.Decimal) || GameInput.SystemKeyPressed(Keys.OemPeriod))
			{
				text = ".";
			}
		}
		if (GameInput.SystemKeyPressed(Keys.Decimal) || GameInput.SystemKeyPressed(Keys.OemComma) || GameInput.SystemKeyPressed(Keys.OemPeriod))
		{
			if (VariableType == VariableTypeEnum.Float && !ValueString.Contains("."))
			{
				text = ".";
			}
		}
		else if ((GameInput.SystemKeyPressed(Keys.OemMinus) || GameInput.SystemKeyPressed(Keys.Subtract)) && (VariableType == VariableTypeEnum.String || !ValueString.Contains("-")))
		{
			text = ((!GameInput.ShiftDown) ? "-" : "_");
		}
		if (text != null)
		{
			if (_cursorPos < ValueString.Length)
			{
				ValueString = ValueString.Insert(_cursorPos, text);
			}
			else
			{
				ValueString += text;
			}
			_cursorPos++;
			OnFieldSet();
		}
		if (GameInput.SystemKeyPressed(Keys.Back))
		{
			if (_cursorPos > 0)
			{
				ValueString = ValueString.Remove(_cursorPos - 1, 1);
				_cursorPos--;
				OnFieldSet();
			}
		}
		else if (GameInput.SystemKeyPressed(Keys.Delete))
		{
			if (_cursorPos < ValueString.Length)
			{
				ValueString = ValueString.Remove(_cursorPos, 1);
				OnFieldSet();
			}
		}
		else if (GameInput.SystemKeyPressed(Keys.Left))
		{
			if (_cursorPos > 0)
			{
				_cursorPos--;
			}
		}
		else if (GameInput.SystemKeyPressed(Keys.Right) && _cursorPos < ValueString.Length)
		{
			_cursorPos++;
		}
		if (GameInput.SystemKeyPressed(Keys.Home))
		{
			_cursorPos = 0;
		}
		else if (GameInput.SystemKeyPressed(Keys.End))
		{
			_cursorPos = ValueString.Length;
		}
		else if (GameInput.SystemKeyPressed(Keys.Enter))
		{
			OnFieldEnter();
			Editor.SetFocusComponent(null);
		}
	}

	private BehaviorEvent GetBindedBehaviorEvent()
	{
		BehaviorEvent behaviorEvent = null;
		if (BindedData != null && BindedData.GetType() == typeof(BehaviorEventNode))
		{
			BehaviorEventNode behaviorEventNode = (BehaviorEventNode)BindedData;
			string[] array = Label.Split('/');
			behaviorEvent = (BehaviorEvent)(BindedData = ((array.Length != 3) ? behaviorEventNode.AddEvent(null, array[0], array[1], (int)GetFloatValue()) : behaviorEventNode.AddEvent(array[0], array[1], array[2], (int)GetFloatValue())));
			behaviorEvent.SetObject(Editor.CurrentObject);
		}
		else
		{
			behaviorEvent = (BehaviorEvent)BindedData;
		}
		return behaviorEvent;
	}

	private void CheckValueRange()
	{
		bool flag = false;
		if (MinValue.HasValue && GetFloatValue() < MinValue)
		{
			SetValue(MinValue.Value, bCallEvents: false);
			flag = true;
		}
		if (MaxValue.HasValue && GetFloatValue() > MaxValue)
		{
			SetValue(MaxValue.Value, bCallEvents: false);
			flag = true;
		}
		if (flag)
		{
			_cursorPos = ValueString.Length;
			OnValueChanged?.Invoke(this, 0f);
		}
	}

	private void OnFieldSet()
	{
		if (HasFocus)
		{
			FloatValue = Utils.StringToFloat(ValueString) / ValueRoundFactor;
		}
		BehaviorEvent bindedBehaviorEvent = GetBindedBehaviorEvent();
		if (bindedBehaviorEvent != null)
		{
			bindedBehaviorEvent.ValueInt = (int)GetFloatValue();
		}
		OnValueChanged?.Invoke(this, 0f);
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
			VariableType = VariableTypeEnum.String;
			BindedData = null;
		}
	}

	public void OnFieldEnter()
	{
		if (ValueString.Length == 0)
		{
			OnFieldClear();
		}
		SetStringValue(ValueString, bCallEvents: false);
		CheckValueRange();
		BehaviorEvent bindedBehaviorEvent = GetBindedBehaviorEvent();
		if (bindedBehaviorEvent != null && bindedBehaviorEvent.Time <= 0f)
		{
			bindedBehaviorEvent.MyObject.DoZeroEvents();
		}
		OnValueChanged?.Invoke(this, 0f);
	}

	public void OnFieldIncrease(float delta)
	{
		if (BindedData != null)
		{
			BehaviorEvent bindedBehaviorEvent = GetBindedBehaviorEvent();
			if (bindedBehaviorEvent != null)
			{
				bindedBehaviorEvent.ValueInt = (int)GetFloatValue();
			}
		}
		OnValueChanged?.Invoke(this, delta);
	}

	public override void Update(float elapsedTime)
	{
		if (!IsVisible)
		{
			return;
		}
		base.Update(elapsedTime);
		if ((Editor.SelectedComponent == null || Editor.SelectedComponent == this) && Editor.SelectedPanel == Panel)
		{
			bool isSelected = IsSelected;
			if (LabelPosition == LabelPosition.Left)
			{
				IsSelected = Editor.Cursor.IsInRect(ScreenX + LabelWidth, ScreenY, Width, Height);
			}
			else
			{
				IsSelected = Editor.Cursor.IsInRect(ScreenX, ScreenY + LabelHeight, Width, Height);
			}
			if (IsSelected)
			{
				Editor.SelectedDragAction = DragAction.OnPanelComponent;
				Editor.SelectedComponent = this;
				Editor.SelectedPanel = Panel;
				Editor.Cursor.Text = Tooltip ?? ("Change value " + GetFormattedValue());
				if (GameInput.MousePressed(GameInput.MouseButton.Left))
				{
					Editor.DragAction = DragAction.OnPanelComponent;
					Editor.DragCenter = Editor.Cursor.Position;
				}
				if (GameInput.MouseReleased(GameInput.MouseButton.Left))
				{
					if (!IsMoved && _bCanClickSetFocus)
					{
						SetFocus(bIgnoreFirstInput: true);
						GameInput.CancelMousePressed();
					}
					else
					{
						Editor.SetFocusComponent(null);
					}
				}
				if (!isSelected)
				{
					OnMouseOver?.Invoke(this);
				}
				if (!HasFocus && Editor.KeyPressed(Keys.Delete))
				{
					ValueString = "";
					OnFieldEnter();
				}
			}
		}
		if (HasFocus && GameInput.MouseReleased(GameInput.MouseButton.Left))
		{
			Editor.SetFocusComponent(null);
		}
		if (HasFocus && GameInput.MousePressed(GameInput.MouseButton.Right))
		{
			Editor.SetFocusComponent(null);
			GameInput.CancelMousePressed();
		}
		if (!_ignoreFirstInput)
		{
			ParseInput();
		}
		_ignoreFirstInput = false;
	}

	public float GetFloatValue(float defaultValue = 0f, bool bClamp = true)
	{
		float num = FloatValue;
		if (bClamp)
		{
			if (MinValue.HasValue && num < MinValue)
			{
				num = MinValue.Value;
			}
			else if (MaxValue.HasValue && num > MaxValue)
			{
				num = MaxValue.Value;
			}
		}
		return num;
	}

	public void IncreaseValue(float delta, bool bCallEvents = true)
	{
		float floatValue = GetFloatValue();
		float num = floatValue + delta;
		FloatValue = Utils.Clamp(num, MinValue.HasValue ? MinValue.Value : num, MaxValue.HasValue ? MaxValue.Value : num);
		ValueString = FloatValue.ToString(Utils.FloatFI);
		if (!bCallEvents)
		{
			return;
		}
		OnFieldIncrease(num - floatValue);
		if (!GameInput.AltDown || _shareFieldList.IsNullOrEmpty())
		{
			return;
		}
		foreach (TextField shareField in _shareFieldList)
		{
			float floatValue2 = shareField.GetFloatValue();
			float num2 = floatValue2 + delta;
			shareField.IncreaseValue(delta, bCallEvents: false);
			shareField.OnFieldIncrease(num2 - floatValue2);
		}
	}

	public void SetValue(float value, bool bCallEvents = true)
	{
		FloatValue = value;
		ValueString = value.ToString(Utils.FloatFI);
		if (!bCallEvents)
		{
			return;
		}
		OnFieldSet();
		if (!GameInput.ControlDown || _shareFieldList.IsNullOrEmpty())
		{
			return;
		}
		foreach (TextField shareField in _shareFieldList)
		{
			shareField.SetValue(value, bCallEvents: false);
			shareField.OnFieldSet();
		}
	}

	public void SetStringValue(string value, bool bCallEvents = true)
	{
		ValueString = value;
		FloatValue = Utils.StringToFloat(ValueString) / ValueRoundFactor;
		if (bCallEvents)
		{
			OnFieldSet();
		}
	}

	public void UpdateValue(float value)
	{
		if (!HasFocus)
		{
			SetValue(value, bCallEvents: false);
		}
	}

	public void ClearValue()
	{
		OnFieldClear();
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
		Editor.TriangleBatch.End();
	}

	private void DrawCursor()
	{
		_cursorTimer += GameCore.FrameTime * 4f;
		Vector2 vector = Panel.Position + Position;
		vector.X += _cursorPos * Editor.Font.GetCharWidth(32) + 2;
		vector.Y += 2f;
		if (LabelPosition == LabelPosition.Top)
		{
			vector.Y += LabelHeight;
		}
		else
		{
			vector.X += LabelWidth;
		}
		Color valueColorSelected = ValueColorSelected;
		Editor.LineBatch.Begin(BlendState.AlphaBlend, Matrix.Identity);
		valueColorSelected.A = (byte)(155.0 + Math.Sin(_cursorTimer) * 100.0);
		Editor.LineBatch.DrawLine(vector, vector + new Vector2(0f, Height - 4), valueColorSelected);
		Editor.LineBatch.End();
	}

	public string GetFormattedValue()
	{
		if (VariableType == VariableTypeEnum.String || HasFocus)
		{
			return ValueString;
		}
		if (VariableType == VariableTypeEnum.Int)
		{
			return ((int)(FloatValue * ValueRoundFactor)).ToString();
		}
		return Utils.FormatFloat2(FloatValue * ValueRoundFactor);
	}

	public override void Draw()
	{
		if (IsVisible)
		{
			base.Draw();
			DrawFrame();
			Color color;
			if ((MinValue.HasValue && GetFloatValue(0f, bClamp: false) < MinValue) || (MaxValue.HasValue && GetFloatValue(0f, bClamp: false) > MaxValue))
			{
				color = new Color(255, 0, 0);
			}
			else if (HasFocus)
			{
				color = ((!_clearValueOnInput) ? new Color(100, 255, 100) : new Color(60, 120, 255));
			}
			else
			{
				color = ((!IsSelected) ? ValueColor : ValueColorSelected);
			}
			if (LabelPosition == LabelPosition.Left)
			{
				Editor.Font.Write(Editor.SpriteBatch, Label, ScreenX, ScreenY, LabelColor);
				Editor.Font.Write(Editor.SpriteBatch, GetFormattedValue(), ScreenX + LabelWidth + 2, ScreenY, color);
			}
			else
			{
				Editor.Font.Write(Editor.SpriteBatch, Label, ScreenX, ScreenY, LabelColor, TextAlign.Left, Panel.Width - 10);
				Editor.Font.Write(Editor.SpriteBatch, GetFormattedValue(), ScreenX + 2, ScreenY + LabelHeight + 1, color, TextAlign.Left, Width - 8);
			}
			if (HasFocus)
			{
				DrawCursor();
			}
		}
	}
}
