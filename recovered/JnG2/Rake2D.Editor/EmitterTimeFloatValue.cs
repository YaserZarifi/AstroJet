using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Rake2D.Editor;

public class EmitterTimeFloatValue : EmitterTimeValue
{
	private TimeFunctionFloat _timeFunctionValue;

	private EmitterTimeField _fieldAlpha;

	public override TimeFunctionValue TimeFunctionValue => _timeFunctionValue;

	public EmitterTimeFloatValue(PanelEmitterTime panel, TimeFunctionFloat timeFunction)
		: base(panel)
	{
		LineColor = new Color(150, 150, 150, 150);
		_timeFunctionValue = timeFunction;
		_fieldAlpha = new EmitterTimeField(_panel);
		_fieldAlpha.SetLimits(0f, 999f, 0f, 100f);
		_fieldAlpha.Tooltip = "Alpha";
		_fieldAlpha.OnValueChanged += OnValueChanged;
		_fieldAlpha.OnMousePressed += OnMousePressed;
	}

	private void OnValueChanged(TextField field, float delta)
	{
		_timeFunctionValue.StartValue = field.GetFloatValue() / 100f;
		_panel.BuildAlphaValues();
	}

	private void OnMousePressed(TextField field, GameInput.MouseButton button)
	{
		_panel.DragOffset = Editor.Cursor.Position - ScreenPosition;
		_panel.OnFunctionMousePresed(_timeFunctionValue);
	}

	public void Destroy()
	{
		_fieldAlpha.Destroy();
		IsDestroyed = true;
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		_fieldAlpha.Position = Position;
		_fieldAlpha.UpdateValue(_timeFunctionValue.StartValue * 100f);
		if (_fieldAlpha.IsSelected && Editor.KeyPressed(Keys.Delete) && Editor.FocusComponent == null)
		{
			Destroy();
			_timeFunctionValue.MyList.Remove(_timeFunctionValue);
			_panel.BuildAlphaValues();
		}
	}

	public override void Draw()
	{
		DrawLine();
	}
}
