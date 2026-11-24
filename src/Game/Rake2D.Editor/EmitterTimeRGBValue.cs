using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Rake2D.Editor;

public class EmitterTimeRGBValue : EmitterTimeValue
{
	private TimeFunctionColor _timeFunctionValue;

	private EmitterTimeField _fieldR;

	private EmitterTimeField _fieldG;

	private EmitterTimeField _fieldB;

	public override TimeFunctionValue TimeFunctionValue => _timeFunctionValue;

	public EmitterTimeRGBValue(PanelEmitterTime panel, TimeFunctionColor timeFunction)
		: base(panel)
	{
		LineColor = new Color(255, 0, 0, 150);
		ValueHeight = 60;
		_timeFunctionValue = timeFunction;
		_fieldR = new EmitterTimeField(_panel);
		_fieldR.SetLimits(0f, 255f);
		_fieldR.VariableType = VariableTypeEnum.Int;
		_fieldR.Tooltip = "Color red";
		_fieldR.OnValueChanged += OnValueChanged;
		_fieldR.OnMousePressed += OnMousePressed;
		_fieldG = new EmitterTimeField(_panel);
		_fieldG.SetLimits(0f, 255f);
		_fieldG.VariableType = VariableTypeEnum.Int;
		_fieldG.OnValueChanged += OnValueChanged;
		_fieldG.Tooltip = "Color green";
		_fieldG.OnMousePressed += OnMousePressed;
		_fieldB = new EmitterTimeField(_panel);
		_fieldB.SetLimits(0f, 255f);
		_fieldB.VariableType = VariableTypeEnum.Int;
		_fieldB.Tooltip = "Color blue";
		_fieldB.OnValueChanged += OnValueChanged;
		_fieldB.OnMousePressed += OnMousePressed;
		_fieldR.AddSharedField(_fieldG);
		_fieldR.AddSharedField(_fieldB);
		_fieldG.AddSharedField(_fieldR);
		_fieldG.AddSharedField(_fieldB);
		_fieldB.AddSharedField(_fieldG);
		_fieldB.AddSharedField(_fieldR);
	}

	private void OnValueChanged(TextField field, float delta)
	{
		_timeFunctionValue.StartR = (int)_fieldR.GetFloatValue();
		_timeFunctionValue.StartG = (int)_fieldG.GetFloatValue();
		_timeFunctionValue.StartB = (int)_fieldB.GetFloatValue();
		_panel.BuildRGBValues();
	}

	private void OnMousePressed(TextField field, GameInput.MouseButton button)
	{
		_panel.DragOffset = Editor.Cursor.Position - ScreenPosition;
		_panel.OnFunctionMousePresed(_timeFunctionValue);
	}

	public void Destroy()
	{
		_fieldR.Destroy();
		_fieldG.Destroy();
		_fieldB.Destroy();
		IsDestroyed = true;
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		_fieldR.Position = Position;
		_fieldR.UpdateValue(_timeFunctionValue.StartR);
		_fieldG.Position = Position;
		_fieldG.Position.Y += 20f;
		_fieldG.UpdateValue(_timeFunctionValue.StartG);
		_fieldB.Position = Position;
		_fieldB.Position.Y += 40f;
		_fieldB.UpdateValue(_timeFunctionValue.StartB);
		if ((_fieldR.IsSelected || _fieldG.IsSelected || _fieldB.IsSelected) && Editor.KeyPressed(Keys.Delete) && Editor.FocusComponent == null)
		{
			Destroy();
			_timeFunctionValue.MyList.Remove(_timeFunctionValue);
			_panel.BuildRGBValues();
		}
	}

	public override void Draw()
	{
		DrawLine();
	}
}
