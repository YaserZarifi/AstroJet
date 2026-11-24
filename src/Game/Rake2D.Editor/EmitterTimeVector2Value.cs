using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Rake2D.Editor;

public class EmitterTimeVector2Value : EmitterTimeValue
{
	private TimeFunctionVector2 _timeFunctionValue;

	private EmitterTimeField _fieldX;

	private EmitterTimeField _fieldY;

	public override TimeFunctionValue TimeFunctionValue => _timeFunctionValue;

	public void SetLimits(float min, float max, float defaultMin, float defaultMax)
	{
		_fieldX.MinValue = min;
		_fieldX.MaxValue = max;
		_fieldX.DefaultValueMin = defaultMin;
		_fieldX.DefaultValueMax = defaultMax;
		_fieldY.MinValue = min;
		_fieldY.MaxValue = max;
		_fieldY.DefaultValueMin = defaultMin;
		_fieldY.DefaultValueMax = defaultMax;
	}

	public EmitterTimeVector2Value(PanelEmitterTime panel, TimeFunctionVector2 timeFunction)
		: base(panel)
	{
		ValueHeight = 40;
		_timeFunctionValue = timeFunction;
		_fieldX = new EmitterTimeField(_panel);
		_fieldX.SetLimits(0f, 999f, 0f, 100f);
		_fieldX.VariableType = VariableTypeEnum.Int;
		_fieldX.OnValueChanged += OnValueChanged;
		_fieldX.OnMousePressed += OnMousePressed;
		_fieldY = new EmitterTimeField(_panel);
		_fieldY.SetLimits(0f, 999f, 0f, 100f);
		_fieldY.VariableType = VariableTypeEnum.Int;
		_fieldY.OnValueChanged += OnValueChanged;
		_fieldY.OnMousePressed += OnMousePressed;
		_fieldX.AddSharedField(_fieldY);
		_fieldY.AddSharedField(_fieldX);
	}

	public void SetToolTip(string tooltip)
	{
		_fieldX.Tooltip = $"{tooltip} X";
		_fieldY.Tooltip = $"{tooltip} Y";
	}

	private void OnValueChanged(TextField field, float delta)
	{
		_timeFunctionValue.StartValue = new Vector2(_fieldX.GetFloatValue() / 100f, _fieldY.GetFloatValue() / 100f);
		_panel.BuildScaleValues();
		_panel.BuildVelocityValues();
	}

	private void OnMousePressed(TextField field, GameInput.MouseButton button)
	{
		_panel.DragOffset = Editor.Cursor.Position - ScreenPosition;
		_panel.OnFunctionMousePresed(_timeFunctionValue);
	}

	public void Destroy()
	{
		_fieldX.Destroy();
		_fieldY.Destroy();
		IsDestroyed = true;
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		_fieldX.Position = Position;
		_fieldX.UpdateValue(_timeFunctionValue.StartValue.X * 100f);
		_fieldY.Position = Position;
		_fieldY.Position.Y += 20f;
		_fieldY.UpdateValue(_timeFunctionValue.StartValue.Y * 100f);
		if ((_fieldX.IsSelected || _fieldY.IsSelected) && Editor.KeyPressed(Keys.Delete) && Editor.FocusComponent == null)
		{
			Destroy();
			_timeFunctionValue.MyList.Remove(_timeFunctionValue);
			_panel.BuildScaleValues();
			_panel.BuildVelocityValues();
		}
	}

	public override void Draw()
	{
		DrawLine();
	}
}
