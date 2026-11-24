using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Rake2D.Editor;

public class PanelEmitterTime : Panel
{
	private static int LINE_RGB_Y = 50;

	private static int LINE_ALPHA_Y = LINE_RGB_Y + 64;

	private static int LINE_SCALE_Y = LINE_ALPHA_Y + 24;

	private static int LINE_VELOCITY_Y = LINE_SCALE_Y + 44;

	private static Color COLOR_TIME_LINE_CAPTION = new Color(150, 150, 150, 200);

	private static Color COLOR_LABEL = new Color(200, 255, 255);

	private static Color COLOR_LABEL_DISABLED = new Color(100, 155, 155, 200);

	public Rectangle TimeAreaRect;

	public Vector2 DragOffset;

	private List<EmitterTimeFloatValue> _valueListAlpha;

	private List<EmitterTimeRGBValue> _valueListRGB;

	private List<EmitterTimeVector2Value> _valueListScale;

	private List<EmitterTimeVector2Value> _valueListVelocity;

	private float _cursorTime;

	private ParticleEmitter _emitter;

	private ParticleEmitterData _emitterData;

	private TimeFunctionValue _draggedTimeFunction;

	private TimeFunctionValue _selectedTimeFunctionValue;

	private EmitterTimeFunctionEnum _insertFunction;

	public bool CanInsert
	{
		get
		{
			if (this == Editor.SelectedPanel && _draggedTimeFunction == null && _selectedTimeFunctionValue == null)
			{
				return _insertFunction != EmitterTimeFunctionEnum.None;
			}
			return false;
		}
	}

	public PanelEmitterTime(Editor editor, int x, int y, int width, int height)
		: base(editor, x, y, width, height)
	{
		TimeAreaRect = new Rectangle(80, 24, 850, 198);
	}

	private void DestroyAllValues()
	{
		if (_valueListRGB != null)
		{
			foreach (EmitterTimeRGBValue item in _valueListRGB)
			{
				item.Destroy();
			}
			_valueListRGB.Clear();
		}
		else
		{
			_valueListRGB = new List<EmitterTimeRGBValue>();
		}
		if (_valueListAlpha != null)
		{
			foreach (EmitterTimeFloatValue item2 in _valueListAlpha)
			{
				item2.Destroy();
			}
			_valueListAlpha.Clear();
		}
		else
		{
			_valueListAlpha = new List<EmitterTimeFloatValue>();
		}
		if (_valueListScale != null)
		{
			foreach (EmitterTimeVector2Value item3 in _valueListScale)
			{
				item3.Destroy();
			}
			_valueListScale.Clear();
		}
		else
		{
			_valueListScale = new List<EmitterTimeVector2Value>();
		}
		if (_valueListVelocity != null)
		{
			foreach (EmitterTimeVector2Value item4 in _valueListVelocity)
			{
				item4.Destroy();
			}
			_valueListVelocity.Clear();
		}
		else
		{
			_valueListVelocity = new List<EmitterTimeVector2Value>();
		}
	}

	public void OnFunctionMousePresed(TimeFunctionValue tf)
	{
		_draggedTimeFunction = tf;
		Editor.DragAction = DragAction.MovePanelComponent;
		Editor.SetFocusComponent(null);
	}

	private void BuildFunctionLines()
	{
		DestroyAllValues();
		if (_emitterData == null)
		{
			return;
		}
		if (_emitterData.TimeFunctionColorList != null)
		{
			foreach (TimeFunctionColor timeFunctionColor in _emitterData.TimeFunctionColorList)
			{
				EmitterTimeRGBValue emitterTimeRGBValue = new EmitterTimeRGBValue(this, timeFunctionColor);
				emitterTimeRGBValue.Position.Y = LINE_RGB_Y;
				_valueListRGB.Add(emitterTimeRGBValue);
			}
		}
		if (_emitterData.TimeFunctionAlphaList != null)
		{
			foreach (TimeFunctionFloat timeFunctionAlpha in _emitterData.TimeFunctionAlphaList)
			{
				EmitterTimeFloatValue emitterTimeFloatValue = new EmitterTimeFloatValue(this, timeFunctionAlpha);
				emitterTimeFloatValue.Position.Y = LINE_ALPHA_Y;
				_valueListAlpha.Add(emitterTimeFloatValue);
			}
		}
		if (_emitterData.TimeFunctionScaleList != null)
		{
			foreach (TimeFunctionVector2 timeFunctionScale in _emitterData.TimeFunctionScaleList)
			{
				EmitterTimeVector2Value emitterTimeVector2Value = new EmitterTimeVector2Value(this, timeFunctionScale);
				emitterTimeVector2Value.Position.Y = LINE_SCALE_Y;
				emitterTimeVector2Value.LineColor = new Color(0, 255, 0, 150);
				emitterTimeVector2Value.SetToolTip("Scale");
				_valueListScale.Add(emitterTimeVector2Value);
			}
		}
		if (_emitterData.TimeFunctionVelocityList == null)
		{
			return;
		}
		foreach (TimeFunctionVector2 timeFunctionVelocity in _emitterData.TimeFunctionVelocityList)
		{
			EmitterTimeVector2Value emitterTimeVector2Value2 = new EmitterTimeVector2Value(this, timeFunctionVelocity);
			emitterTimeVector2Value2.Position.Y = LINE_VELOCITY_Y;
			emitterTimeVector2Value2.LineColor = new Color(0, 100, 255, 150);
			emitterTimeVector2Value2.SetLimits(-999f, 999f, 0f, 100f);
			emitterTimeVector2Value2.SetToolTip("Velocity");
			_valueListScale.Add(emitterTimeVector2Value2);
		}
	}

	private void SetEmmiterData(ParticleEmitterData emitterData)
	{
		if (emitterData != _emitterData)
		{
			_emitterData = emitterData;
			BuildFunctionLines();
		}
	}

	public void BuildAlphaValues()
	{
		if (_emitterData != null)
		{
			ParticleEmitterData.BuildTimeFunctionFloat(ref _emitterData.TimeFunctionAlphaList);
		}
	}

	public void BuildRGBValues()
	{
		if (_emitterData != null)
		{
			ParticleEmitterData.BuildTimeFunctionColor(ref _emitterData.TimeFunctionColorList);
		}
	}

	public void BuildScaleValues()
	{
		if (_emitterData != null)
		{
			ParticleEmitterData.BuildTimeFunctionVector2(ref _emitterData.TimeFunctionScaleList);
		}
	}

	public void BuildVelocityValues()
	{
		if (_emitterData != null)
		{
			ParticleEmitterData.BuildTimeFunctionVector2(ref _emitterData.TimeFunctionVelocityList);
		}
	}

	private void UpdateDragTimeFunction(float elapsedTime)
	{
		float min = 0f;
		float max = 1f;
		float cursorTime = _cursorTime;
		if (_draggedTimeFunction.GetType() == typeof(TimeFunctionFloat))
		{
			TimeFunctionFloat timeFunctionFloat = _draggedTimeFunction as TimeFunctionFloat;
			int num = timeFunctionFloat.MyList.IndexOf(timeFunctionFloat);
			if (num > 0)
			{
				min = Math.Max(0f, timeFunctionFloat.MyList[num - 1].StartTime + 0.05f);
			}
			if (num < timeFunctionFloat.MyList.Count - 1)
			{
				max = Math.Min(1f, timeFunctionFloat.MyList[num + 1].StartTime - 0.05f);
			}
			_draggedTimeFunction.StartTime = Utils.Clamp(cursorTime, min, max);
			BuildAlphaValues();
		}
		if (_draggedTimeFunction.GetType() == typeof(TimeFunctionVector2))
		{
			TimeFunctionVector2 timeFunctionVector = _draggedTimeFunction as TimeFunctionVector2;
			int num2 = timeFunctionVector.MyList.IndexOf(timeFunctionVector);
			if (num2 > 0)
			{
				min = Math.Max(0f, timeFunctionVector.MyList[num2 - 1].StartTime + 0.05f);
			}
			if (num2 < timeFunctionVector.MyList.Count - 1)
			{
				max = Math.Min(1f, timeFunctionVector.MyList[num2 + 1].StartTime - 0.05f);
			}
			_draggedTimeFunction.StartTime = Utils.Clamp(cursorTime, min, max);
			BuildScaleValues();
			BuildVelocityValues();
		}
		if (_draggedTimeFunction.GetType() == typeof(TimeFunctionColor))
		{
			TimeFunctionColor timeFunctionColor = _draggedTimeFunction as TimeFunctionColor;
			int num3 = timeFunctionColor.MyList.IndexOf(timeFunctionColor);
			if (num3 > 0)
			{
				min = Math.Max(0f, timeFunctionColor.MyList[num3 - 1].StartTime + 0.05f);
			}
			if (num3 < timeFunctionColor.MyList.Count - 1)
			{
				max = Math.Min(1f, timeFunctionColor.MyList[num3 + 1].StartTime - 0.05f);
			}
			_draggedTimeFunction.StartTime = Utils.Clamp(cursorTime, min, max);
			BuildRGBValues();
		}
	}

	public override void ParseInput()
	{
		base.ParseInput();
		if (_emitterData != null && CanInsert && GameInput.SystemKeyPressed(Keys.Insert))
		{
			Editor.SetFocusComponent(null);
			switch (_insertFunction)
			{
			case EmitterTimeFunctionEnum.RGB:
				_emitterData.AddTimeFunctionRGB(_cursorTime, 255, 255, 255);
				BuildRGBValues();
				BuildFunctionLines();
				break;
			case EmitterTimeFunctionEnum.Alpha:
				_emitterData.AddTimeFunctionAlpha(_cursorTime, 1f);
				BuildAlphaValues();
				BuildFunctionLines();
				break;
			case EmitterTimeFunctionEnum.Scale:
				_emitterData.AddTimeFunctionScale(_cursorTime, 1f, 1f);
				BuildScaleValues();
				BuildFunctionLines();
				break;
			case EmitterTimeFunctionEnum.Velocity:
				_emitterData.AddTimeFunctionVelocity(_cursorTime, 1f, 1f);
				BuildVelocityValues();
				BuildFunctionLines();
				break;
			}
		}
	}

	public override void Update(float elapsedTime)
	{
		if (!IsEnabled || IsOff)
		{
			return;
		}
		base.Update(elapsedTime);
		if (Editor.CurrentObject != null && Editor.CurrentObject.Emitter != null)
		{
			TitleText = $"{Editor.CurrentObject.Emitter.Data.Id} ({Editor.CurrentObject.Emitter.Data.FileName})";
		}
		else
		{
			TitleText = null;
		}
		if (IsMinimized)
		{
			return;
		}
		_cursorTime = Utils.Clamp((Editor.Cursor.Position.X - Position.X - (float)TimeAreaRect.X - DragOffset.X) / (float)TimeAreaRect.Width, 0f, 1f);
		_selectedTimeFunctionValue = null;
		if (!GameInput.MouseDown(GameInput.MouseButton.Left))
		{
			_draggedTimeFunction = null;
		}
		if (_draggedTimeFunction != null)
		{
			UpdateDragTimeFunction(elapsedTime);
		}
		if (Editor.CurrentObject != null && Editor.CurrentObject.Emitter != null)
		{
			_emitter = Editor.CurrentObject.Emitter;
			SetEmmiterData(_emitter.Data);
			if (_valueListAlpha != null)
			{
				_valueListAlpha.RemoveAll((EmitterTimeFloatValue v) => v.IsDestroyed);
				foreach (EmitterTimeFloatValue item in _valueListAlpha)
				{
					item.Update(elapsedTime);
					if (item.IsSelected)
					{
						_selectedTimeFunctionValue = item.TimeFunctionValue;
					}
				}
			}
			if (_valueListRGB != null)
			{
				_valueListRGB.RemoveAll((EmitterTimeRGBValue v) => v.IsDestroyed);
				foreach (EmitterTimeRGBValue item2 in _valueListRGB)
				{
					item2.Update(elapsedTime);
					if (item2.IsSelected)
					{
						_selectedTimeFunctionValue = item2.TimeFunctionValue;
					}
				}
			}
			if (_valueListScale != null)
			{
				_valueListScale.RemoveAll((EmitterTimeVector2Value v) => v.IsDestroyed);
				foreach (EmitterTimeVector2Value item3 in _valueListScale)
				{
					item3.Update(elapsedTime);
					if (item3.IsSelected)
					{
						_selectedTimeFunctionValue = item3.TimeFunctionValue;
					}
				}
			}
			if (_valueListVelocity != null)
			{
				_valueListVelocity.RemoveAll((EmitterTimeVector2Value v) => v.IsDestroyed);
				foreach (EmitterTimeVector2Value item4 in _valueListVelocity)
				{
					item4.Update(elapsedTime);
					if (item4.IsSelected)
					{
						_selectedTimeFunctionValue = item4.TimeFunctionValue;
					}
				}
			}
		}
		else
		{
			SetEmmiterData(null);
		}
		Vector2 vector = new Vector2(Position.X + (float)TimeAreaRect.X, Position.Y + (float)TimeAreaRect.Y);
		if (Editor.Cursor.IsInRect(vector.X, Position.Y + (float)LINE_RGB_Y, TimeAreaRect.Width, LINE_ALPHA_Y - LINE_RGB_Y))
		{
			_insertFunction = EmitterTimeFunctionEnum.RGB;
		}
		else if (Editor.Cursor.IsInRect(vector.X, Position.Y + (float)LINE_ALPHA_Y, TimeAreaRect.Width, LINE_SCALE_Y - LINE_ALPHA_Y))
		{
			_insertFunction = EmitterTimeFunctionEnum.Alpha;
		}
		else if (Editor.Cursor.IsInRect(vector.X, Position.Y + (float)LINE_SCALE_Y, TimeAreaRect.Width, LINE_VELOCITY_Y - LINE_SCALE_Y))
		{
			_insertFunction = EmitterTimeFunctionEnum.Scale;
		}
		else if (Editor.Cursor.IsInRect(vector.X, Position.Y + (float)LINE_VELOCITY_Y, TimeAreaRect.Width, TimeAreaRect.Y + TimeAreaRect.Height - LINE_VELOCITY_Y))
		{
			_insertFunction = EmitterTimeFunctionEnum.Velocity;
		}
		else
		{
			_insertFunction = EmitterTimeFunctionEnum.None;
		}
		if (CanInsert)
		{
			Editor.Cursor.Text = $"Insert at {(int)(_cursorTime * 100f)} %";
		}
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
		Editor.TriangleBatch.Flush();
		Editor.LineBatch.Flush();
		float y = Position.Y + (float)TimeAreaRect.Y + 20f;
		Editor.LineBatch.Begin(BlendState.NonPremultiplied, Matrix.Identity);
		for (int i = 0; i < 5; i++)
		{
			Vector2 vector = new Vector2(Position.X + (float)TimeAreaRect.X + (float)(i * (TimeAreaRect.Width / 4)), y);
			Editor.LineBatch.DrawLine(vector, new Vector2(vector.X, vector.Y + (float)TimeAreaRect.Height - 20f), new Color(60, 60, 70, 100));
		}
		Editor.LineBatch.Flush();
		Color color;
		if (_insertFunction != EmitterTimeFunctionEnum.RGB)
		{
			color = ((_emitterData != null && !_emitterData.TimeFunctionColorList.IsNullOrEmpty()) ? COLOR_LABEL : COLOR_LABEL_DISABLED);
		}
		else
		{
			color = new Color(255, 255, 0);
		}
		Editor.Font.Write(Editor.SpriteBatch, "      R:", Position.X + 6f, Position.Y + (float)LINE_RGB_Y, color, TextAlign.Left, 100);
		Editor.Font.Write(Editor.SpriteBatch, "Color G:", Position.X + 6f, Position.Y + (float)LINE_RGB_Y + 20f, color, TextAlign.Left, 100);
		Editor.Font.Write(Editor.SpriteBatch, "      B:", Position.X + 6f, Position.Y + (float)LINE_RGB_Y + 40f, color, TextAlign.Left, 100);
		if (_insertFunction != EmitterTimeFunctionEnum.Alpha)
		{
			color = ((_emitterData != null && !_emitterData.TimeFunctionAlphaList.IsNullOrEmpty()) ? COLOR_LABEL : COLOR_LABEL_DISABLED);
		}
		else
		{
			color = new Color(255, 255, 0);
		}
		Editor.Font.Write(Editor.SpriteBatch, "Alpha  :", Position.X + 6f, Position.Y + (float)LINE_ALPHA_Y, color, TextAlign.Left, 100);
		if (_insertFunction != EmitterTimeFunctionEnum.Scale)
		{
			color = ((_emitterData != null && !_emitterData.TimeFunctionScaleList.IsNullOrEmpty()) ? COLOR_LABEL : COLOR_LABEL_DISABLED);
		}
		else
		{
			color = new Color(255, 255, 0);
		}
		Editor.Font.Write(Editor.SpriteBatch, "      X:", Position.X + 6f, Position.Y + (float)LINE_SCALE_Y, color, TextAlign.Left, 100);
		Editor.Font.Write(Editor.SpriteBatch, "Scale", Position.X + 6f, Position.Y + (float)LINE_SCALE_Y + 10f, color, TextAlign.Left, 100);
		Editor.Font.Write(Editor.SpriteBatch, "      Y:", Position.X + 6f, Position.Y + (float)LINE_SCALE_Y + 20f, color, TextAlign.Left, 100);
		if (_insertFunction != EmitterTimeFunctionEnum.Velocity)
		{
			color = ((_emitterData != null && !_emitterData.TimeFunctionVelocityList.IsNullOrEmpty()) ? COLOR_LABEL : COLOR_LABEL_DISABLED);
		}
		else
		{
			color = new Color(255, 255, 0);
		}
		Editor.Font.Write(Editor.SpriteBatch, "      X:", Position.X + 6f, Position.Y + (float)LINE_VELOCITY_Y, color, TextAlign.Left, 100);
		Editor.Font.Write(Editor.SpriteBatch, "Speed", Position.X + 6f, Position.Y + (float)LINE_VELOCITY_Y + 10f, color, TextAlign.Left, 100);
		Editor.Font.Write(Editor.SpriteBatch, "      Y:", Position.X + 6f, Position.Y + (float)LINE_VELOCITY_Y + 20f, color, TextAlign.Left, 100);
		Vector2 vector2 = new Vector2(Position.X + 2f, Position.Y + (float)LINE_RGB_Y - 4f);
		Color color2 = new Color(50, 50, 50, 150);
		Editor.LineBatch.DrawLine(vector2, new Vector2(Position.X + (float)TimeAreaRect.X + (float)TimeAreaRect.Width, vector2.Y), color2);
		vector2.Y = Position.Y + (float)LINE_ALPHA_Y - 4f;
		Editor.LineBatch.DrawLine(vector2, new Vector2(Position.X + (float)TimeAreaRect.X + (float)TimeAreaRect.Width, vector2.Y), color2);
		vector2.Y = Position.Y + (float)LINE_SCALE_Y - 4f;
		Editor.LineBatch.DrawLine(vector2, new Vector2(Position.X + (float)TimeAreaRect.X + (float)TimeAreaRect.Width, vector2.Y), color2);
		vector2.Y = Position.Y + (float)LINE_VELOCITY_Y - 4f;
		Editor.LineBatch.DrawLine(vector2, new Vector2(Position.X + (float)TimeAreaRect.X + (float)TimeAreaRect.Width, vector2.Y), color2);
		vector2.Y = Position.Y + (float)TimeAreaRect.Y + (float)TimeAreaRect.Height;
		Editor.LineBatch.DrawLine(vector2, new Vector2(Position.X + (float)TimeAreaRect.X + (float)TimeAreaRect.Width, vector2.Y), color2);
		if (_valueListVelocity != null)
		{
			foreach (EmitterTimeVector2Value item in _valueListVelocity)
			{
				item.Draw();
			}
		}
		if (_valueListScale != null)
		{
			foreach (EmitterTimeVector2Value item2 in _valueListScale)
			{
				item2.Draw();
			}
		}
		if (_valueListAlpha != null)
		{
			foreach (EmitterTimeFloatValue item3 in _valueListAlpha)
			{
				item3.Draw();
			}
		}
		if (_valueListRGB != null)
		{
			foreach (EmitterTimeRGBValue item4 in _valueListRGB)
			{
				item4.Draw();
			}
		}
		float y2 = Position.Y + (float)TimeAreaRect.Y;
		Editor.Font.Write(Editor.SpriteBatch, "0", Position.X + (float)TimeAreaRect.X, y2, COLOR_TIME_LINE_CAPTION, TextAlign.Center);
		Editor.Font.Write(Editor.SpriteBatch, "25", Position.X + (float)TimeAreaRect.X + (float)(TimeAreaRect.Width / 4), y2, COLOR_TIME_LINE_CAPTION, TextAlign.Center);
		Editor.Font.Write(Editor.SpriteBatch, "50", Position.X + (float)TimeAreaRect.X + (float)(TimeAreaRect.Width / 2), y2, COLOR_TIME_LINE_CAPTION, TextAlign.Center);
		Editor.Font.Write(Editor.SpriteBatch, "75", Position.X + (float)TimeAreaRect.X + (float)TimeAreaRect.Width * 0.75f, y2, COLOR_TIME_LINE_CAPTION, TextAlign.Center);
		Editor.Font.Write(Editor.SpriteBatch, "100", Position.X + (float)TimeAreaRect.X + (float)TimeAreaRect.Width, y2, COLOR_TIME_LINE_CAPTION, TextAlign.Center);
		if (CanInsert)
		{
			switch (_insertFunction)
			{
			case EmitterTimeFunctionEnum.RGB:
				Editor.LineBatch.DrawLine(new Vector2(Editor.Cursor.Position.X, y), new Vector2(Editor.Cursor.Position.X, Position.Y + (float)LINE_ALPHA_Y - 4f), new Color(255, 255, 0, 100));
				break;
			case EmitterTimeFunctionEnum.Alpha:
				Editor.LineBatch.DrawLine(new Vector2(Editor.Cursor.Position.X, y), new Vector2(Editor.Cursor.Position.X, Position.Y + (float)LINE_SCALE_Y - 4f), new Color(255, 255, 0, 100));
				break;
			case EmitterTimeFunctionEnum.Scale:
				Editor.LineBatch.DrawLine(new Vector2(Editor.Cursor.Position.X, y), new Vector2(Editor.Cursor.Position.X, Position.Y + (float)LINE_VELOCITY_Y - 4f), new Color(255, 255, 0, 100));
				break;
			case EmitterTimeFunctionEnum.Velocity:
				Editor.LineBatch.DrawLine(new Vector2(Editor.Cursor.Position.X, y), new Vector2(Editor.Cursor.Position.X, Position.Y + (float)TimeAreaRect.Y + (float)TimeAreaRect.Height), new Color(255, 255, 0, 100));
				break;
			}
		}
		DrawEnd();
	}
}
