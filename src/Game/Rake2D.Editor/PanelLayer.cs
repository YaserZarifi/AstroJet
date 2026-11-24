namespace Rake2D.Editor;

public class PanelLayer : Panel
{
	private const int LINE_H = 21;

	private TextField _textFieldColorR;

	private TextField _textFieldColorG;

	private TextField _textFieldColorB;

	private TextField _textFieldLightFactor1;

	private TextField _textFieldLightFactor2;

	private TextField _textFieldLightFactor3;

	private TextField _textFieldlShadowFactor;

	private TextField _textFieldLightTo;

	private TextField _textFieldlShadowTo;

	private TextField _textFieldlShadowFrom;

	private TextField _textFieldLoopX;

	private TextField _textFieldLoopOffsetX;

	private ColorShifter _colorShifter;

	private Layer _myLayer => Editor.CurrentLayer;

	public PanelLayer(Editor editor, int x, int y, int width, int height)
		: base(editor, x, y, width, height)
	{
		int num = 27;
		int num2 = 8;
		_textFieldColorR = new TextField(this);
		_textFieldColorR.Position.X = num2;
		_textFieldColorR.Position.Y = num;
		_textFieldColorR.Label = "Ambient:";
		_textFieldColorR.LabelWidth = 85;
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
		_textFieldLightFactor1 = new TextField(this);
		_textFieldLightFactor1.Position.X = num2;
		_textFieldLightFactor1.Position.Y = num;
		_textFieldLightFactor1.Label = "Light fct:";
		_textFieldLightFactor1.LabelWidth = 85;
		_textFieldLightFactor1.Width = 35;
		_textFieldLightFactor1.SetLimits(0f, 100f);
		_textFieldLightFactor1.Tooltip = "Factor for light map 1";
		_textFieldLightFactor1.OnValueChanged += OnLightFactor1Changed;
		num2 += _textFieldLightFactor1.WidthWithLabel + 4;
		_textFieldLightFactor2 = new TextField(this);
		_textFieldLightFactor2.Position.X = num2;
		_textFieldLightFactor2.Position.Y = num;
		_textFieldLightFactor2.Width = 35;
		_textFieldLightFactor2.LabelWidth = 0;
		_textFieldLightFactor2.SetLimits(0f, 100f);
		_textFieldLightFactor2.Tooltip = "Factor for light map 2";
		_textFieldLightFactor2.OnValueChanged += OnLightFactor2Changed;
		num2 += _textFieldLightFactor2.WidthWithLabel + 4;
		_textFieldLightFactor3 = new TextField(this);
		_textFieldLightFactor3.Position.X = num2;
		_textFieldLightFactor3.Position.Y = num;
		_textFieldLightFactor3.Width = 35;
		_textFieldLightFactor3.LabelWidth = 0;
		_textFieldLightFactor3.SetLimits(0f, 100f);
		_textFieldLightFactor3.Tooltip = "Factor for light map 3";
		_textFieldLightFactor3.OnValueChanged += OnLightFactor3Changed;
		num += 21;
		num2 = 8;
		_textFieldLightTo = new TextField(this);
		_textFieldLightTo.Position.X = num2;
		_textFieldLightTo.Position.Y = num;
		_textFieldLightTo.Label = "Light to:";
		_textFieldLightTo.LabelWidth = 85;
		_textFieldLightTo.Width = 35;
		_textFieldLightTo.SetLimits(-1f, 2f, 0f, 2f);
		_textFieldLightTo.Tooltip = "Light to (light map number 0 - 2 or -1 as none)";
		_textFieldLightTo.OnValueChanged += OnLightToChanged;
		num += 21;
		num2 = 8;
		_textFieldlShadowFactor = new TextField(this);
		_textFieldlShadowFactor.Position.X = num2;
		_textFieldlShadowFactor.Position.Y = num;
		_textFieldlShadowFactor.Label = "Shadow:";
		_textFieldlShadowFactor.LabelWidth = 85;
		_textFieldlShadowFactor.Width = 35;
		_textFieldlShadowFactor.SetLimits(0f, 100f);
		_textFieldlShadowFactor.SetSteps(1f, 10f, 25f);
		_textFieldlShadowFactor.Tooltip = "Factor for shadowmap";
		_textFieldlShadowFactor.OnValueChanged += OnShadowFactorChanged;
		num2 += _textFieldlShadowFactor.WidthWithLabel + 4;
		_textFieldlShadowFrom = new TextField(this);
		_textFieldlShadowFrom.Position.X = num2;
		_textFieldlShadowFrom.Position.Y = num;
		_textFieldlShadowFrom.Width = 35;
		_textFieldlShadowFrom.LabelWidth = 0;
		_textFieldlShadowFrom.SetLimits(-1f, 2f, 0f, 2f);
		_textFieldlShadowFrom.Tooltip = "Shadow from (shadow map number 0 - 2 or -1 as none)";
		_textFieldlShadowFrom.OnValueChanged += OnShadowFromChanged;
		num2 += _textFieldlShadowFrom.WidthWithLabel + 4;
		_textFieldlShadowTo = new TextField(this);
		_textFieldlShadowTo.Position.X = num2;
		_textFieldlShadowTo.Position.Y = num;
		_textFieldlShadowTo.Width = 35;
		_textFieldlShadowTo.LabelWidth = 0;
		_textFieldlShadowTo.SetLimits(-1f, 2f, 0f, 2f);
		_textFieldlShadowTo.Tooltip = "Shadow to (shadow map number 0 - 2 or -1 as none)";
		_textFieldlShadowTo.OnValueChanged += OnShadowToChanged;
		num += 21;
		num2 = 8;
		_textFieldLoopX = new TextField(this);
		_textFieldLoopX.Position.X = num2;
		_textFieldLoopX.Position.Y = num;
		_textFieldLoopX.Label = "Loop:";
		_textFieldLoopX.LabelWidth = 85;
		_textFieldLoopX.Width = 55;
		_textFieldLoopX.SetLimits(-1f, 999999f, 0f);
		_textFieldLoopX.SetSteps(1f, 10f, 100f);
		_textFieldLoopX.Tooltip = "Loop line pos X";
		_textFieldLoopX.OnValueChanged += OnLoopXChanged;
		num2 += _textFieldLoopX.WidthWithLabel + 4;
		_textFieldLoopOffsetX = new TextField(this);
		_textFieldLoopOffsetX.Position.X = num2;
		_textFieldLoopOffsetX.Position.Y = num;
		_textFieldLoopOffsetX.Width = 55;
		_textFieldLoopOffsetX.LabelWidth = 0;
		_textFieldLoopOffsetX.SetLimits(0f, 999999f);
		_textFieldLoopOffsetX.SetSteps(1f, 10f, 100f);
		_textFieldLoopOffsetX.Tooltip = "Loop line offset";
		_textFieldLoopOffsetX.OnValueChanged += OnLightLoopOffsetChanged;
	}

	private void OnColorRChanged(TextField field, float delta)
	{
		_myLayer.AmbientLightColor.R = (byte)field.FloatValue;
	}

	private void OnColorGChanged(TextField field, float delta)
	{
		_myLayer.AmbientLightColor.G = (byte)field.FloatValue;
	}

	private void OnColorBChanged(TextField field, float delta)
	{
		_myLayer.AmbientLightColor.B = (byte)field.FloatValue;
	}

	private void OnColorShifterChanged(ColorShifter shifter)
	{
		_myLayer.AmbientLightColor = shifter.Value;
	}

	private void OnLightFactor1Changed(TextField field, float delta)
	{
		_myLayer.LightMapFactorArray[0] = field.FloatValue / 100f;
	}

	private void OnLightFactor2Changed(TextField field, float delta)
	{
		_myLayer.LightMapFactorArray[1] = field.FloatValue / 100f;
	}

	private void OnLightFactor3Changed(TextField field, float delta)
	{
		_myLayer.LightMapFactorArray[2] = field.FloatValue / 100f;
	}

	private void OnShadowFactorChanged(TextField field, float delta)
	{
		_myLayer.ShadowMapFactor = field.FloatValue / 100f;
	}

	private void OnShadowFromChanged(TextField field, float delta)
	{
		_myLayer.ShadowFromMapNumber = (int)field.FloatValue;
	}

	private void OnShadowToChanged(TextField field, float delta)
	{
		_myLayer.ShadowToMapNumber = (int)field.FloatValue;
	}

	private void OnLightToChanged(TextField field, float delta)
	{
		_myLayer.LightToMapNumber = (int)field.FloatValue;
	}

	private void OnLoopXChanged(TextField field, float delta)
	{
		_myLayer.LoopX = (int)field.FloatValue;
	}

	private void OnLightLoopOffsetChanged(TextField field, float delta)
	{
		_myLayer.LoopOffsetX = (int)field.FloatValue;
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		TitleText = "Layer: " + _myLayer.Id;
		if (!_textFieldColorR.HasFocus)
		{
			_textFieldColorR.UpdateValue((int)_myLayer.AmbientLightColor.R);
		}
		if (!_textFieldColorG.HasFocus)
		{
			_textFieldColorG.UpdateValue((int)_myLayer.AmbientLightColor.G);
		}
		if (!_textFieldColorB.HasFocus)
		{
			_textFieldColorB.UpdateValue((int)_myLayer.AmbientLightColor.B);
		}
		if (!_textFieldLightFactor1.HasFocus)
		{
			_textFieldLightFactor1.UpdateValue(_myLayer.LightMapFactorArray[0] * 100f);
		}
		if (!_textFieldLightFactor2.HasFocus)
		{
			_textFieldLightFactor2.UpdateValue(_myLayer.LightMapFactorArray[1] * 100f);
		}
		if (!_textFieldLightFactor3.HasFocus)
		{
			_textFieldLightFactor3.UpdateValue(_myLayer.LightMapFactorArray[2] * 100f);
		}
		if (!_textFieldlShadowFactor.HasFocus)
		{
			_textFieldlShadowFactor.UpdateValue(_myLayer.ShadowMapFactor * 100f);
		}
		if (!_textFieldLightTo.HasFocus)
		{
			_textFieldLightTo.UpdateValue(_myLayer.LightToMapNumber);
		}
		if (!_textFieldlShadowFrom.HasFocus)
		{
			_textFieldlShadowFrom.UpdateValue(_myLayer.ShadowFromMapNumber);
		}
		if (!_textFieldlShadowTo.HasFocus)
		{
			_textFieldlShadowTo.UpdateValue(_myLayer.ShadowToMapNumber);
		}
		if (!_textFieldLoopX.HasFocus)
		{
			_textFieldLoopX.UpdateValue(_myLayer.LoopX);
		}
		if (!_textFieldLoopOffsetX.HasFocus)
		{
			_textFieldLoopOffsetX.UpdateValue(_myLayer.LoopOffsetX);
		}
		_colorShifter.Value = _myLayer.AmbientLightColor;
	}
}
