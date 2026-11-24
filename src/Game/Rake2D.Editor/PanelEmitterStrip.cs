using Microsoft.Xna.Framework;

namespace Rake2D.Editor;

public class PanelEmitterStrip : Panel
{
	private ParticleEmitter _emitter;

	private TextField _textFieldFrame2;

	private TextField _textFieldBlendFactor2;

	private TextField _textFieldFlowSpeed1;

	private TextField _textFieldFlowSpeed2;

	private ComboBox _comboBlendMode;

	private ComboBox _comboLastZeroAlpha;

	private readonly Color _colorComboValue = new Color(150, 180, 255);

	private ParticleEmitterData _emitterData
	{
		get
		{
			if (_emitter != null)
			{
				return _emitter.Data;
			}
			return null;
		}
	}

	public PanelEmitterStrip(Editor editor, int x, int y, int width, int height)
		: base(editor, x, y, width, height)
	{
		int num = 27;
		_textFieldFrame2 = new TextField(this);
		_textFieldFrame2.Position.X = PanelEmitter.TEXT_X_2[0];
		_textFieldFrame2.Position.Y = num;
		_textFieldFrame2.Label = "Frame 2:";
		_textFieldFrame2.LabelWidth = 100;
		_textFieldFrame2.SetLimits(-1f, 99f, 0f, 0f);
		_textFieldFrame2.SetSteps(1f, 1f, 1f);
		_textFieldFrame2.Tooltip = "Frame number 2 (-1 = no second texture)";
		_textFieldFrame2.Width = 89;
		_textFieldFrame2.OnValueChanged += OnValueChanged;
		num += 21;
		_comboBlendMode = new ComboBox(this);
		_comboBlendMode.Position.X = PanelEmitter.TEXT_X_2[0];
		_comboBlendMode.Position.Y = num;
		_comboBlendMode.Label = "Blend 2:";
		_comboBlendMode.LabelWidth = 100;
		_comboBlendMode.LabelPosition = LabelPosition.Left;
		_comboBlendMode.Width = 89;
		_comboBlendMode.AddItem(0, "Normal");
		_comboBlendMode.AddItem(1, "Additive");
		_comboBlendMode.ValueColor = _colorComboValue;
		_comboBlendMode.Tooltip = "Blend mode for texture 2";
		_comboBlendMode.OnValueChanged += OnComboValueChanged;
		_textFieldBlendFactor2 = new TextField(this);
		_textFieldBlendFactor2.Position.X = PanelEmitter.TEXT_X_2[1];
		_textFieldBlendFactor2.Position.Y = num;
		_textFieldBlendFactor2.Label = "x";
		_textFieldBlendFactor2.LabelWidth = 15;
		_textFieldBlendFactor2.SetLimits(0f, 1f);
		_textFieldBlendFactor2.SetSteps(0.01f, 0.1f, 0.05f);
		_textFieldBlendFactor2.ValueRoundFactor = 100f;
		_textFieldBlendFactor2.Width = 89;
		_textFieldBlendFactor2.Tooltip = "Blend factor 2";
		_textFieldBlendFactor2.OnValueChanged += OnValueChanged;
		num += 21;
		_textFieldFlowSpeed1 = new TextField(this);
		_textFieldFlowSpeed1.Position.X = PanelEmitter.TEXT_X_2[0];
		_textFieldFlowSpeed1.Position.Y = num;
		_textFieldFlowSpeed1.Label = "Flow 1:";
		_textFieldFlowSpeed1.LabelWidth = 100;
		_textFieldFlowSpeed1.SetLimits(-10f, 10f, 0f, 0f);
		_textFieldFlowSpeed1.SetSteps(0.01f, 0.1f, 0.05f);
		_textFieldFlowSpeed1.ValueRoundFactor = 100f;
		_textFieldFlowSpeed1.Width = 89;
		_textFieldFlowSpeed1.Tooltip = "Flow speed of texture 1";
		_textFieldFlowSpeed1.OnValueChanged += OnValueChanged;
		_textFieldFlowSpeed2 = new TextField(this);
		_textFieldFlowSpeed2.Position.X = PanelEmitter.TEXT_X_2[1];
		_textFieldFlowSpeed2.Position.Y = num;
		_textFieldFlowSpeed2.Label = "2";
		_textFieldFlowSpeed2.LabelWidth = 15;
		_textFieldFlowSpeed2.SetLimits(-10f, 10f, 0f, 0f);
		_textFieldFlowSpeed2.SetSteps(0.01f, 0.1f, 0.05f);
		_textFieldFlowSpeed2.ValueRoundFactor = 100f;
		_textFieldFlowSpeed2.Width = 89;
		_textFieldFlowSpeed2.Tooltip = "Flow speed of texture 2";
		_textFieldFlowSpeed2.OnValueChanged += OnValueChanged;
		num += 21;
		_comboLastZeroAlpha = new ComboBox(this);
		_comboLastZeroAlpha.Position.X = PanelEmitter.TEXT_X_2[0];
		_comboLastZeroAlpha.Position.Y = num;
		_comboLastZeroAlpha.Label = "Last zero A:";
		_comboLastZeroAlpha.LabelWidth = 100;
		_comboLastZeroAlpha.LabelPosition = LabelPosition.Left;
		_comboLastZeroAlpha.Width = 200;
		_comboLastZeroAlpha.AddItem(1, "Yes");
		_comboLastZeroAlpha.AddItem(0, "No");
		_comboLastZeroAlpha.ValueColor = _colorComboValue;
		_comboLastZeroAlpha.Tooltip = "Set last vertices to zero alpha";
		_comboLastZeroAlpha.OnValueChanged += OnComboValueChanged;
	}

	private void OnComboValueChanged(ComboBox combo)
	{
		try
		{
			_emitterData.StripBlendMode2 = (BlendModeEnum)_comboBlendMode.GetIntValue();
		}
		catch
		{
			_emitterData.StripBlendMode2 = BlendModeEnum.Normal;
		}
		_emitterData.StripLastZeroAlpha = _comboLastZeroAlpha.GetIntValue() != 0;
		if (_emitter.StripLine != null)
		{
			_emitter.StripLine.LastZeroAlpha = _emitterData.StripLastZeroAlpha;
			_emitter.StripLine.BlendMode2 = _emitterData.StripBlendMode2;
		}
	}

	private void OnValueChanged(TextField field, float delta)
	{
		if (_emitterData != null)
		{
			_emitterData.StripFrame2 = (int)_textFieldFrame2.GetFloatValue();
			_emitterData.StripBlendFactor2 = _textFieldBlendFactor2.GetFloatValue();
			_emitterData.StripTextureFlowSpeed1 = _textFieldFlowSpeed1.GetFloatValue();
			_emitterData.StripTextureFlowSpeed2 = _textFieldFlowSpeed2.GetFloatValue();
			if (_emitter.StripLine != null)
			{
				_emitter.StripLine.SetFrame2(_emitterData.StripFrame2, _emitterData.StripBlendMode2);
				_emitter.StripLine.TextureFlowSpeed1 = _emitterData.StripTextureFlowSpeed1;
				_emitter.StripLine.TextureFlowSpeed2 = _emitterData.StripTextureFlowSpeed2;
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
		if (!IsMinimized)
		{
			if (Editor.CurrentObject != null && Editor.CurrentObject.Emitter != null)
			{
				_emitter = Editor.CurrentObject.Emitter;
				_textFieldFrame2.UpdateValue(_emitterData.StripFrame2);
				_textFieldBlendFactor2.UpdateValue(_emitterData.StripBlendFactor2);
				_textFieldFlowSpeed1.UpdateValue(_emitterData.StripTextureFlowSpeed1);
				_textFieldFlowSpeed2.UpdateValue(_emitterData.StripTextureFlowSpeed2);
				_comboBlendMode.ChooseIntValue((int)_emitterData.StripBlendMode2);
				_comboLastZeroAlpha.ChooseIntValue(_emitterData.StripLastZeroAlpha ? 1 : 0);
			}
			else
			{
				_emitter = null;
			}
		}
	}
}
