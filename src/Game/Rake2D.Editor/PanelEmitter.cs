using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Editor;

public class PanelEmitter : Panel
{
	public const int TEXT_WIDTH_1 = 200;

	public const int TEXT_WIDTH_2 = 89;

	public const int TEXT_WIDTH_4 = 42;

	public const int LABEL_WIDTH = 100;

	public const int LABEL_WIDTH_SHORT = 15;

	public static readonly int[] TEXT_X_2 = new int[2] { 8, 204 };

	public static readonly int[] TEXT_X_4 = new int[4] { 8, 155, 204, 266 };

	private ParticleEmitter _emitter;

	private TextField _textFieldFrameList;

	private TextField _textFieldAnimDelayFrom;

	private TextField _textFieldAnimDelayTo;

	private TextField _textFieldLifeTimeFrom;

	private TextField _textFieldLifeTimeTo;

	private TextField _textFieldCountFrom;

	private TextField _textFieldCountTo;

	private TextField _textFieldDelayFrom;

	private TextField _textFieldDelayTo;

	private TextField _textFieldBeginDelayFrom;

	private TextField _textFieldBeginDelayTo;

	private TextField _textFieldLoopFrom;

	private TextField _textFieldLoopTo;

	private TextField _textFieldAngleFrom;

	private TextField _textFieldAngleTo;

	private TextField _textFieldRotationSpeedFrom;

	private TextField _textFieldRotationSpeedTo;

	private TextField _textFieldAlphaFrom;

	private TextField _textFieldAlphaTo;

	private TextField _textFieldLightFactor;

	private TextField _textFieldBlendFactor;

	private TextField _textFieldScaleFromX;

	private TextField _textFieldScaleFromY;

	private TextField _textFieldScaleToX;

	private TextField _textFieldScaleToY;

	private TextField _textFieldOffsetFromX;

	private TextField _textFieldOffsetFromY;

	private TextField _textFieldOffsetToX;

	private TextField _textFieldOffsetToY;

	private TextField _textFieldVelocityFromX;

	private TextField _textFieldVelocityFromY;

	private TextField _textFieldVelocityToX;

	private TextField _textFieldVelocityToY;

	private TextField _textFieldForceFromX;

	private TextField _textFieldForceFromY;

	private TextField _textFieldForceToX;

	private TextField _textFieldForceToY;

	private TextField _textFieldPrewarmRepeatCount;

	private TextField _textFieldPrewarmTime;

	private TextField _textParentVelocityFactor;

	private ComboBox _comboStrip;

	private ComboBox _comboSpace;

	private ComboBox _comboOffsetShape;

	private ComboBox _comboTransformMode;

	private ComboBox _comboSameScaleXY;

	private ComboBox _comboScaleMode;

	private ComboBox _comboVelocityMode;

	private ComboBox _comboRotationMode;

	private ComboBox _comboUseVeloToRotation;

	private ComboBox _comboColorMode;

	private ComboBox _comboSortMode;

	private ComboBox _comboFlipModeH;

	private ComboBox _comboFlipModeV;

	private ComboBox _comboBlendMode;

	private ComboBox _comboDestroyOnLeft;

	private ComboBox _comboDestroyMode;

	private ComboBox _comboCompensateLevelScrolling;

	private ComboBox _comboSpreadModeX;

	private ComboBox _comboSpreadModeY;

	private Button _buttonSaveEmitter;

	private Button _buttonAtlas;

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

	public PanelEmitter(Editor editor, int x, int y, int width, int height)
		: base(editor, x, y, width, height)
	{
		int num = 27;
		_buttonAtlas = new Button(null, this);
		_buttonAtlas.Position.X = TEXT_X_2[0];
		_buttonAtlas.Position.Y = num;
		_buttonAtlas.Label = "Atlas:";
		_buttonAtlas.LabelWidth = 100;
		_buttonAtlas.Width = 200;
		_buttonAtlas.Height = 21;
		_buttonAtlas.ForegroundColor = new Color(255, 255, 100);
		_buttonAtlas.CaptionAlign = TextAlign.Left;
		num += 25;
		_comboStrip = new ComboBox(this);
		_comboStrip.Position.X = TEXT_X_2[0];
		_comboStrip.Position.Y = num;
		_comboStrip.Label = "Is strip:";
		_comboStrip.LabelWidth = 100;
		_comboStrip.LabelPosition = LabelPosition.Left;
		_comboStrip.Width = 200;
		_comboStrip.ValueColor = _colorComboValue;
		_comboStrip.AddItem(0, "No");
		_comboStrip.AddItem(1, "Yes");
		_comboStrip.Tooltip = "Is strip line or normal emitter";
		_comboStrip.OnValueChanged += OnComboStripValueChanged;
		num += 25;
		_textFieldFrameList = new TextField(this);
		_textFieldFrameList.Position.X = TEXT_X_2[0];
		_textFieldFrameList.Position.Y = num;
		_textFieldFrameList.Label = "Frames:";
		_textFieldFrameList.LabelWidth = 100;
		_textFieldFrameList.VariableType = VariableTypeEnum.String;
		_textFieldFrameList.Width = 200;
		_textFieldFrameList.Tooltip = "Atlas frames (comma separated list)";
		_textFieldFrameList.OnValueChanged += OnFrameListChanged;
		num += 21;
		_textFieldAnimDelayFrom = new TextField(this);
		_textFieldAnimDelayFrom.Position.X = TEXT_X_2[0];
		_textFieldAnimDelayFrom.Position.Y = num;
		_textFieldAnimDelayFrom.Label = "Anim delay:";
		_textFieldAnimDelayFrom.LabelWidth = 100;
		_textFieldAnimDelayFrom.SetLimits(-1f, 999f, 0f, 1000f);
		_textFieldAnimDelayFrom.SetSteps(1f, 10f, 100f);
		_textFieldAnimDelayFrom.Width = 89;
		_textFieldAnimDelayFrom.Tooltip = "Delay between animation frames (0 = no anim - random selection)";
		_textFieldAnimDelayFrom.OnValueChanged += OnValueChanged;
		_textFieldAnimDelayTo = new TextField(this);
		_textFieldAnimDelayTo.Position.X = TEXT_X_2[1];
		_textFieldAnimDelayTo.Position.Y = num;
		_textFieldAnimDelayTo.Label = "-";
		_textFieldAnimDelayTo.LabelWidth = 15;
		_textFieldAnimDelayTo.SetLimits(0f, 999f, 0f, 1000f);
		_textFieldAnimDelayTo.SetSteps(1f, 10f, 100f);
		_textFieldAnimDelayTo.Width = 89;
		_textFieldAnimDelayTo.Tooltip = "Delay between animation frames (0 = no anim - random selection)";
		_textFieldAnimDelayTo.OnValueChanged += OnValueChanged;
		_textFieldAnimDelayFrom.AddSharedField(_textFieldAnimDelayTo);
		_textFieldAnimDelayTo.AddSharedField(_textFieldAnimDelayFrom);
		num += 25;
		_comboSpace = new ComboBox(this);
		_comboSpace.Position.X = TEXT_X_2[0];
		_comboSpace.Position.Y = num;
		_comboSpace.Label = "Space:";
		_comboSpace.LabelWidth = 100;
		_comboSpace.LabelPosition = LabelPosition.Left;
		_comboSpace.Width = 200;
		_comboSpace.ValueColor = _colorComboValue;
		_comboSpace.AddItem(7, "My layer front");
		_comboSpace.AddItem(6, "My layer back");
		_comboSpace.AddItem(12, "My layer random");
		_comboSpace.AddItem(8, "Last particle layer front");
		_comboSpace.AddItem(9, "First main layer back");
		_comboSpace.AddItem(1, "My part front");
		_comboSpace.AddItem(0, "My part back");
		_comboSpace.AddItem(3, "Parent part front");
		_comboSpace.AddItem(2, "Parent part back");
		_comboSpace.AddItem(5, "Main part front");
		_comboSpace.AddItem(4, "Main part back");
		_comboSpace.AddItem(10, "Level front");
		_comboSpace.AddItem(11, "Distortion");
		_comboSpace.Tooltip = "Where particles will be created";
		_comboSpace.OnValueChanged += OnComboValueChanged;
		num += 21;
		_comboSortMode = new ComboBox(this);
		_comboSortMode.Position.X = TEXT_X_2[0];
		_comboSortMode.Position.Y = num;
		_comboSortMode.Label = "Sort:";
		_comboSortMode.LabelWidth = 100;
		_comboSortMode.LabelPosition = LabelPosition.Left;
		_comboSortMode.Width = 200;
		_comboSortMode.ValueColor = _colorComboValue;
		_comboSortMode.AddItem("0", "As last");
		_comboSortMode.AddItem("1", "As first");
		_comboSortMode.AddItem("2", "Random");
		_comboSortMode.Tooltip = "Position in list of particles";
		_comboSortMode.OnValueChanged += OnComboValueChanged;
		num += 21;
		_textFieldLifeTimeFrom = new TextField(this);
		_textFieldLifeTimeFrom.Position.X = TEXT_X_2[0];
		_textFieldLifeTimeFrom.Position.Y = num;
		_textFieldLifeTimeFrom.Label = "Life time:";
		_textFieldLifeTimeFrom.LabelWidth = 100;
		_textFieldLifeTimeFrom.SetLimits(0.02f, 999f, 0.02f, 0.02f);
		_textFieldLifeTimeFrom.SetSteps(0.001f, 0.01f, 0.05f);
		_textFieldLifeTimeFrom.ValueRoundFactor = 1000f;
		_textFieldLifeTimeFrom.Width = 89;
		_textFieldLifeTimeFrom.Tooltip = "Life time (from)";
		_textFieldLifeTimeFrom.OnValueChanged += OnValueChanged;
		_textFieldLifeTimeTo = new TextField(this);
		_textFieldLifeTimeTo.Position.X = TEXT_X_2[1];
		_textFieldLifeTimeTo.Position.Y = num;
		_textFieldLifeTimeTo.Label = "-";
		_textFieldLifeTimeTo.LabelWidth = 15;
		_textFieldLifeTimeTo.SetLimits(0.02f, 999f, 0.02f, 1f);
		_textFieldLifeTimeTo.SetSteps(0.001f, 0.01f, 0.05f);
		_textFieldLifeTimeTo.ValueRoundFactor = 1000f;
		_textFieldLifeTimeTo.Width = 89;
		_textFieldLifeTimeTo.Tooltip = "Life time (to)";
		_textFieldLifeTimeTo.OnValueChanged += OnValueChanged;
		_textFieldLifeTimeFrom.AddSharedField(_textFieldLifeTimeTo);
		_textFieldLifeTimeTo.AddSharedField(_textFieldLifeTimeFrom);
		num += 21;
		_textFieldCountFrom = new TextField(this);
		_textFieldCountFrom.Position.X = TEXT_X_2[0];
		_textFieldCountFrom.Position.Y = num;
		_textFieldCountFrom.Label = "Count:";
		_textFieldCountFrom.LabelWidth = 100;
		_textFieldCountFrom.SetLimits(0f, 100f, 1f, 1f);
		_textFieldCountFrom.Width = 89;
		_textFieldCountFrom.Tooltip = "Count of emitted particles (from)";
		_textFieldCountFrom.OnValueChanged += OnValueChanged;
		_textFieldCountTo = new TextField(this);
		_textFieldCountTo.Position.X = TEXT_X_2[1];
		_textFieldCountTo.Position.Y = num;
		_textFieldCountTo.Label = "-";
		_textFieldCountTo.LabelWidth = 15;
		_textFieldCountTo.SetLimits(1f, 100f, 1f, 100f);
		_textFieldCountTo.Width = 89;
		_textFieldCountTo.Tooltip = "Count of emitted particles (to)";
		_textFieldCountTo.OnValueChanged += OnValueChanged;
		_textFieldCountFrom.AddSharedField(_textFieldCountTo);
		_textFieldCountTo.AddSharedField(_textFieldCountFrom);
		num += 21;
		_textFieldBeginDelayFrom = new TextField(this);
		_textFieldBeginDelayFrom.Position.X = TEXT_X_2[0];
		_textFieldBeginDelayFrom.Position.Y = num;
		_textFieldBeginDelayFrom.Label = "Begin delay:";
		_textFieldBeginDelayFrom.LabelWidth = 100;
		_textFieldBeginDelayFrom.SetLimits(0f, 99f);
		_textFieldBeginDelayFrom.SetSteps(0.001f, 0.01f, 0.05f);
		_textFieldBeginDelayFrom.ValueRoundFactor = 1000f;
		_textFieldBeginDelayFrom.Width = 89;
		_textFieldBeginDelayFrom.Tooltip = "Delay on start of emmiting (from)";
		_textFieldBeginDelayFrom.OnValueChanged += OnDelayChanged;
		_textFieldBeginDelayTo = new TextField(this);
		_textFieldBeginDelayTo.Position.X = TEXT_X_2[1];
		_textFieldBeginDelayTo.Position.Y = num;
		_textFieldBeginDelayTo.Label = "-";
		_textFieldBeginDelayTo.LabelWidth = 15;
		_textFieldBeginDelayTo.SetLimits(0f, 99f);
		_textFieldBeginDelayTo.SetSteps(0.001f, 0.01f, 0.05f);
		_textFieldBeginDelayTo.ValueRoundFactor = 1000f;
		_textFieldBeginDelayTo.Width = 89;
		_textFieldBeginDelayTo.Tooltip = "Delay on start of emmiting (to)";
		_textFieldBeginDelayTo.OnValueChanged += OnDelayChanged;
		_textFieldBeginDelayFrom.AddSharedField(_textFieldBeginDelayTo);
		_textFieldBeginDelayTo.AddSharedField(_textFieldBeginDelayFrom);
		num += 21;
		_textFieldDelayFrom = new TextField(this);
		_textFieldDelayFrom.Position.X = TEXT_X_2[0];
		_textFieldDelayFrom.Position.Y = num;
		_textFieldDelayFrom.Label = "Delay:";
		_textFieldDelayFrom.LabelWidth = 100;
		_textFieldDelayFrom.SetLimits(0.02f, 99f);
		_textFieldDelayFrom.SetSteps(0.001f, 0.01f, 0.05f);
		_textFieldDelayFrom.ValueRoundFactor = 1000f;
		_textFieldDelayFrom.Width = 89;
		_textFieldDelayFrom.Tooltip = "Delay between two emits (from)";
		_textFieldDelayFrom.OnValueChanged += OnDelayChanged;
		_textFieldDelayTo = new TextField(this);
		_textFieldDelayTo.Position.X = TEXT_X_2[1];
		_textFieldDelayTo.Position.Y = num;
		_textFieldDelayTo.Label = "-";
		_textFieldDelayTo.LabelWidth = 15;
		_textFieldDelayTo.SetLimits(0.02f, 99f);
		_textFieldDelayTo.SetSteps(0.001f, 0.01f, 0.05f);
		_textFieldDelayTo.ValueRoundFactor = 1000f;
		_textFieldDelayTo.Width = 89;
		_textFieldDelayTo.Tooltip = "Delay between two emits (to)";
		_textFieldDelayTo.OnValueChanged += OnDelayChanged;
		_textFieldDelayFrom.AddSharedField(_textFieldDelayTo);
		_textFieldDelayTo.AddSharedField(_textFieldDelayFrom);
		num += 21;
		_textFieldLoopFrom = new TextField(this);
		_textFieldLoopFrom.Position.X = TEXT_X_2[0];
		_textFieldLoopFrom.Position.Y = num;
		_textFieldLoopFrom.Label = "Loop:";
		_textFieldLoopFrom.LabelWidth = 100;
		_textFieldLoopFrom.SetLimits(1f, 9999f);
		_textFieldLoopFrom.Width = 89;
		_textFieldLoopFrom.Tooltip = "Count of loops (from)   [9999 = neverending loop]";
		_textFieldLoopFrom.OnValueChanged += OnValueChanged;
		_textFieldLoopTo = new TextField(this);
		_textFieldLoopTo.Position.X = TEXT_X_2[1];
		_textFieldLoopTo.Position.Y = num;
		_textFieldLoopTo.Label = "-";
		_textFieldLoopTo.LabelWidth = 15;
		_textFieldLoopTo.SetLimits(1f, 9999f);
		_textFieldLoopTo.Width = 89;
		_textFieldLoopTo.Tooltip = "Count of loops (to)";
		_textFieldLoopTo.OnValueChanged += OnValueChanged;
		_textFieldLoopFrom.AddSharedField(_textFieldLoopTo);
		_textFieldLoopTo.AddSharedField(_textFieldLoopFrom);
		num += 21;
		_comboDestroyOnLeft = new ComboBox(this);
		_comboDestroyOnLeft.Position.X = TEXT_X_2[0];
		_comboDestroyOnLeft.Position.Y = num;
		_comboDestroyOnLeft.Label = "Destroy:";
		_comboDestroyOnLeft.LabelWidth = 100;
		_comboDestroyOnLeft.LabelPosition = LabelPosition.Left;
		_comboDestroyOnLeft.Width = 89;
		_comboDestroyOnLeft.AddItem(0, "No");
		_comboDestroyOnLeft.AddItem(1, "On left");
		_comboDestroyOnLeft.ValueColor = _colorComboValue;
		_comboDestroyOnLeft.Tooltip = "Where particle destroys";
		_comboDestroyOnLeft.OnValueChanged += OnComboValueChanged;
		_comboDestroyMode = new ComboBox(this);
		_comboDestroyMode.Position.X = TEXT_X_2[1];
		_comboDestroyMode.Position.Y = num;
		_comboDestroyMode.Label = ",";
		_comboDestroyMode.LabelWidth = 15;
		_comboDestroyMode.LabelPosition = LabelPosition.Left;
		_comboDestroyMode.Width = 89;
		_comboDestroyMode.AddItem(0, "None");
		_comboDestroyMode.AddItem(1, "Destroy");
		_comboDestroyMode.AddItem(2, "Stop");
		_comboDestroyMode.ValueColor = _colorComboValue;
		_comboDestroyMode.Tooltip = "Particle destroy with parent mode";
		_comboDestroyMode.OnValueChanged += OnComboValueChanged;
		num += 25;
		_comboRotationMode = new ComboBox(this);
		_comboRotationMode.Position.X = TEXT_X_2[0];
		_comboRotationMode.Position.Y = num;
		_comboRotationMode.Label = "Angle mode:";
		_comboRotationMode.LabelWidth = 100;
		_comboRotationMode.LabelPosition = LabelPosition.Left;
		_comboRotationMode.Width = 200;
		_comboRotationMode.AddItem(0, "Fixed range");
		_comboRotationMode.AddItem(1, "Parent range");
		_comboRotationMode.AddItem(2, "By velocity");
		_comboRotationMode.AddItem(3, "By velocity continual");
		_comboRotationMode.AddItem(4, "By parent velocity");
		_comboRotationMode.ValueColor = _colorComboValue;
		_comboRotationMode.Tooltip = "Angle mode";
		_comboRotationMode.OnValueChanged += OnComboValueChanged;
		num += 21;
		_textFieldAngleFrom = new TextField(this);
		_textFieldAngleFrom.Position.X = TEXT_X_2[0];
		_textFieldAngleFrom.Position.Y = num;
		_textFieldAngleFrom.Label = "Angle:";
		_textFieldAngleFrom.LabelWidth = 100;
		_textFieldAngleFrom.SetLimits(0f - Utils.DegToRad(359f), Utils.DegToRad(359f), 0f, Utils.DegToRad(359f));
		_textFieldAngleFrom.SetSteps(Utils.DegToRad(1f), Utils.DegToRad(10f), Utils.DegToRad(90f));
		_textFieldAngleFrom.ValueRoundFactor = 180f / (float)Math.PI;
		_textFieldAngleFrom.Width = 89;
		_textFieldAngleFrom.Tooltip = "Sartup angle (from)";
		_textFieldAngleFrom.OnValueChanged += OnValueChanged;
		_textFieldAngleTo = new TextField(this);
		_textFieldAngleTo.Position.X = TEXT_X_2[1];
		_textFieldAngleTo.Position.Y = num;
		_textFieldAngleTo.Label = "-";
		_textFieldAngleTo.LabelWidth = 15;
		_textFieldAngleTo.SetLimits(0f - Utils.DegToRad(359f), Utils.DegToRad(359f), 0f, Utils.DegToRad(359f));
		_textFieldAngleTo.SetSteps(Utils.DegToRad(1f), Utils.DegToRad(10f), Utils.DegToRad(90f));
		_textFieldAngleTo.ValueRoundFactor = 180f / (float)Math.PI;
		_textFieldAngleTo.Width = 89;
		_textFieldAngleTo.Tooltip = "Sartup angle (to)";
		_textFieldAngleTo.OnValueChanged += OnValueChanged;
		_textFieldAngleFrom.AddSharedField(_textFieldAngleTo);
		_textFieldAngleTo.AddSharedField(_textFieldAngleFrom);
		num += 21;
		_comboUseVeloToRotation = new ComboBox(this);
		_comboUseVeloToRotation.Position.X = TEXT_X_2[0];
		_comboUseVeloToRotation.Position.Y = num;
		_comboUseVeloToRotation.Label = "Velo rot.:";
		_comboUseVeloToRotation.LabelWidth = 100;
		_comboUseVeloToRotation.LabelPosition = LabelPosition.Left;
		_comboUseVeloToRotation.Width = 200;
		_comboUseVeloToRotation.AddItem(0, "No");
		_comboUseVeloToRotation.AddItem(1, "Yes (use velocity X)");
		_comboUseVeloToRotation.ValueColor = _colorComboValue;
		_comboUseVeloToRotation.Tooltip = "Use X velocity for rotation direction";
		_comboUseVeloToRotation.OnValueChanged += OnComboValueChanged;
		num += 21;
		_textFieldRotationSpeedFrom = new TextField(this);
		_textFieldRotationSpeedFrom.Position.X = TEXT_X_2[0];
		_textFieldRotationSpeedFrom.Position.Y = num;
		_textFieldRotationSpeedFrom.Label = "Rot. speed:";
		_textFieldRotationSpeedFrom.LabelWidth = 100;
		_textFieldRotationSpeedFrom.SetLimits(0f - Utils.DegToRad(9999f), Utils.DegToRad(9999f), 0f, Utils.DegToRad(359f));
		_textFieldRotationSpeedFrom.SetSteps(Utils.DegToRad(1f), Utils.DegToRad(10f), Utils.DegToRad(90f));
		_textFieldRotationSpeedFrom.ValueRoundFactor = 180f / (float)Math.PI;
		_textFieldRotationSpeedFrom.Width = 89;
		_textFieldRotationSpeedFrom.Tooltip = "Rotation speed (from)";
		_textFieldRotationSpeedFrom.OnValueChanged += OnValueChanged;
		_textFieldRotationSpeedTo = new TextField(this);
		_textFieldRotationSpeedTo.Position.X = TEXT_X_2[1];
		_textFieldRotationSpeedTo.Position.Y = num;
		_textFieldRotationSpeedTo.Label = "-";
		_textFieldRotationSpeedTo.LabelWidth = 15;
		_textFieldRotationSpeedTo.SetLimits(0f - Utils.DegToRad(9999f), Utils.DegToRad(9999f), 0f, Utils.DegToRad(359f));
		_textFieldRotationSpeedTo.SetSteps(Utils.DegToRad(1f), Utils.DegToRad(10f), Utils.DegToRad(90f));
		_textFieldRotationSpeedTo.ValueRoundFactor = 180f / (float)Math.PI;
		_textFieldRotationSpeedTo.Width = 89;
		_textFieldRotationSpeedTo.Tooltip = "Rotation speed (to)";
		_textFieldRotationSpeedTo.OnValueChanged += OnValueChanged;
		_textFieldRotationSpeedFrom.AddSharedField(_textFieldRotationSpeedTo);
		_textFieldRotationSpeedTo.AddSharedField(_textFieldRotationSpeedFrom);
		num += 21;
		_comboFlipModeH = new ComboBox(this);
		_comboFlipModeH.Position.X = TEXT_X_2[0];
		_comboFlipModeH.Position.Y = num;
		_comboFlipModeH.Label = "Flip H/V:";
		_comboFlipModeH.LabelWidth = 100;
		_comboFlipModeH.LabelPosition = LabelPosition.Left;
		_comboFlipModeH.Width = 89;
		_comboFlipModeH.AddItem("1", "Yes");
		_comboFlipModeH.AddItem("0", "No");
		_comboFlipModeH.AddItem("2", "Random");
		_comboFlipModeH.ValueColor = _colorComboValue;
		_comboFlipModeH.Tooltip = "Flip horzontaly";
		_comboFlipModeH.OnValueChanged += OnFlipComboValueChanged;
		_comboFlipModeV = new ComboBox(this);
		_comboFlipModeV.Position.X = TEXT_X_2[1];
		_comboFlipModeV.Position.Y = num;
		_comboFlipModeV.Label = "-";
		_comboFlipModeV.LabelWidth = 15;
		_comboFlipModeV.LabelPosition = LabelPosition.Left;
		_comboFlipModeV.Width = 89;
		_comboFlipModeV.AddItem("1", "Yes");
		_comboFlipModeV.AddItem("0", "No");
		_comboFlipModeV.AddItem("2", "Random");
		_comboFlipModeV.ValueColor = _colorComboValue;
		_comboFlipModeV.Tooltip = "Flip verticaly";
		_comboFlipModeV.OnValueChanged += OnFlipComboValueChanged;
		num += 25;
		_comboColorMode = new ComboBox(this);
		_comboColorMode.Position.X = TEXT_X_2[0];
		_comboColorMode.Position.Y = num;
		_comboColorMode.Label = "Color mode:";
		_comboColorMode.LabelWidth = 100;
		_comboColorMode.LabelPosition = LabelPosition.Left;
		_comboColorMode.Width = 200;
		_comboColorMode.AddItem(0, "Local");
		_comboColorMode.AddItem(1, "Parent RGBA");
		_comboColorMode.AddItem(4, "Parent RGB");
		_comboColorMode.AddItem(3, "Parent alpha");
		_comboColorMode.ValueColor = _colorComboValue;
		_comboColorMode.Tooltip = "Color mode";
		_comboColorMode.OnValueChanged += OnComboValueChanged;
		num += 21;
		_comboBlendMode = new ComboBox(this);
		_comboBlendMode.Position.X = TEXT_X_2[0];
		_comboBlendMode.Position.Y = num;
		_comboBlendMode.Label = "Blend:";
		_comboBlendMode.LabelWidth = 100;
		_comboBlendMode.LabelPosition = LabelPosition.Left;
		_comboBlendMode.Width = 89;
		_comboBlendMode.AddItem(0, "Normal");
		_comboBlendMode.AddItem(1, "Additive");
		_comboBlendMode.AddItem(2, "Random");
		_comboBlendMode.AddItem(3, "Alternate");
		_comboBlendMode.ValueColor = _colorComboValue;
		_comboBlendMode.Tooltip = "Blend mode";
		_comboBlendMode.OnValueChanged += OnComboValueChanged;
		_textFieldBlendFactor = new TextField(this);
		_textFieldBlendFactor.Position.X = TEXT_X_2[1];
		_textFieldBlendFactor.Position.Y = num;
		_textFieldBlendFactor.Label = "x";
		_textFieldBlendFactor.LabelWidth = 15;
		_textFieldBlendFactor.SetLimits(0f, 1f);
		_textFieldBlendFactor.SetSteps(0.01f, 0.1f, 0.05f);
		_textFieldBlendFactor.ValueRoundFactor = 100f;
		_textFieldBlendFactor.Width = 89;
		_textFieldBlendFactor.Tooltip = "Additive blend factor";
		_textFieldBlendFactor.OnValueChanged += OnValueChanged;
		num += 21;
		_textFieldAlphaFrom = new TextField(this);
		_textFieldAlphaFrom.Position.X = TEXT_X_2[0];
		_textFieldAlphaFrom.Position.Y = num;
		_textFieldAlphaFrom.Label = "Alpha:";
		_textFieldAlphaFrom.LabelWidth = 100;
		_textFieldAlphaFrom.SetLimits(0f, 255f);
		_textFieldAlphaFrom.Width = 89;
		_textFieldAlphaFrom.Tooltip = "Alpha (from)";
		_textFieldAlphaFrom.OnValueChanged += OnValueChanged;
		_textFieldAlphaTo = new TextField(this);
		_textFieldAlphaTo.Position.X = TEXT_X_2[1];
		_textFieldAlphaTo.Position.Y = num;
		_textFieldAlphaTo.Label = "-";
		_textFieldAlphaTo.LabelWidth = 15;
		_textFieldAlphaTo.SetLimits(0f, 255f);
		_textFieldAlphaTo.Width = 89;
		_textFieldAlphaTo.Tooltip = "Alpha (to)";
		_textFieldAlphaTo.OnValueChanged += OnValueChanged;
		_textFieldAlphaFrom.AddSharedField(_textFieldAlphaTo);
		_textFieldAlphaTo.AddSharedField(_textFieldAlphaFrom);
		num += 21;
		_textFieldLightFactor = new TextField(this);
		_textFieldLightFactor.Position.X = TEXT_X_2[0];
		_textFieldLightFactor.Position.Y = num;
		_textFieldLightFactor.Label = "Light fact.:";
		_textFieldLightFactor.LabelWidth = 100;
		_textFieldLightFactor.SetLimits(0f, 1f, 0f, 1f);
		_textFieldLightFactor.SetSteps(0.01f, 0.1f, 0.05f);
		_textFieldLightFactor.ValueRoundFactor = 100f;
		_textFieldLightFactor.Width = 89;
		_textFieldLightFactor.Tooltip = "Light factor";
		_textFieldLightFactor.OnValueChanged += OnValueChanged;
		num += 25;
		_comboOffsetShape = new ComboBox(this);
		_comboOffsetShape.Position.X = TEXT_X_2[0];
		_comboOffsetShape.Position.Y = num;
		_comboOffsetShape.Label = "Shape:";
		_comboOffsetShape.LabelWidth = 100;
		_comboOffsetShape.LabelPosition = LabelPosition.Left;
		_comboOffsetShape.Width = 200;
		_comboOffsetShape.AddItem("0", "Rectangle");
		_comboOffsetShape.AddItem("1", "Arc");
		_comboOffsetShape.ValueColor = _colorComboValue;
		_comboOffsetShape.Tooltip = "Offset shape";
		_comboOffsetShape.OnValueChanged += OnComboValueChanged;
		num += 21;
		_textFieldOffsetFromX = new TextField(this);
		_textFieldOffsetFromX.Position.X = TEXT_X_4[0];
		_textFieldOffsetFromX.Position.Y = num;
		_textFieldOffsetFromX.Label = "Offset:";
		_textFieldOffsetFromX.LabelWidth = 100;
		_textFieldOffsetFromX.SetLimits(-9999f, 9999f, 0f);
		_textFieldOffsetFromX.Width = 42;
		_textFieldOffsetFromX.Tooltip = "Offset (from X)";
		_textFieldOffsetFromX.OnValueChanged += OnValueChanged;
		_textFieldOffsetFromY = new TextField(this);
		_textFieldOffsetFromY.Position.X = TEXT_X_4[1];
		_textFieldOffsetFromY.Position.Y = num;
		_textFieldOffsetFromY.SetLimits(-9999f, 9999f, 0f);
		_textFieldOffsetFromY.LabelWidth = 0;
		_textFieldOffsetFromY.Width = 42;
		_textFieldOffsetFromY.Tooltip = "Offset (from Y)";
		_textFieldOffsetFromY.OnValueChanged += OnValueChanged;
		_textFieldOffsetToX = new TextField(this);
		_textFieldOffsetToX.Position.X = TEXT_X_4[2];
		_textFieldOffsetToX.Position.Y = num;
		_textFieldOffsetToX.Label = "-";
		_textFieldOffsetToX.LabelWidth = 15;
		_textFieldOffsetToX.SetLimits(-9999f, 9999f, 0f);
		_textFieldOffsetToX.Width = 42;
		_textFieldOffsetToX.Tooltip = "Offset (to X)";
		_textFieldOffsetToX.OnValueChanged += OnValueChanged;
		_textFieldOffsetToY = new TextField(this);
		_textFieldOffsetToY.Position.X = TEXT_X_4[3];
		_textFieldOffsetToY.Position.Y = num;
		_textFieldOffsetToY.SetLimits(-9999f, 9999f, 0f);
		_textFieldOffsetToY.LabelWidth = 0;
		_textFieldOffsetToY.Width = 42;
		_textFieldOffsetToY.Tooltip = "Offset (to Y)";
		_textFieldOffsetToY.OnValueChanged += OnValueChanged;
		_textFieldOffsetFromX.AddSharedField(_textFieldOffsetToX);
		_textFieldOffsetToX.AddSharedField(_textFieldOffsetFromX);
		_textFieldOffsetToY.AddSharedField(_textFieldOffsetFromY);
		_textFieldOffsetFromY.AddSharedField(_textFieldOffsetToY);
		num += 21;
		_comboSpreadModeX = new ComboBox(this);
		_comboSpreadModeX.Position.X = TEXT_X_2[0];
		_comboSpreadModeX.Position.Y = num;
		_comboSpreadModeX.Label = "Spread X:";
		_comboSpreadModeX.LabelWidth = 100;
		_comboSpreadModeX.LabelPosition = LabelPosition.Left;
		_comboSpreadModeX.Width = 89;
		_comboSpreadModeX.AddItem(0, "Random");
		_comboSpreadModeX.AddItem(1, "Regular");
		_comboSpreadModeX.AddItem(2, "Semi");
		_comboSpreadModeX.ValueColor = _colorComboValue;
		_comboSpreadModeX.Tooltip = "Spread X or Arc";
		_comboSpreadModeX.OnValueChanged += OnComboValueChanged;
		_comboSpreadModeY = new ComboBox(this);
		_comboSpreadModeY.Position.X = TEXT_X_2[1];
		_comboSpreadModeY.Position.Y = num;
		_comboSpreadModeY.Label = "Y:";
		_comboSpreadModeY.LabelWidth = 15;
		_comboSpreadModeY.LabelPosition = LabelPosition.Left;
		_comboSpreadModeY.Width = 89;
		_comboSpreadModeY.AddItem(0, "Random");
		_comboSpreadModeY.AddItem(1, "Regular");
		_comboSpreadModeY.AddItem(2, "Semi");
		_comboSpreadModeY.ValueColor = _colorComboValue;
		_comboSpreadModeY.Tooltip = "Spread Y or Radius";
		_comboSpreadModeY.OnValueChanged += OnComboValueChanged;
		num += 25;
		_comboVelocityMode = new ComboBox(this);
		_comboVelocityMode.Position.X = TEXT_X_2[0];
		_comboVelocityMode.Position.Y = num;
		_comboVelocityMode.Label = "Velo. mode:";
		_comboVelocityMode.LabelWidth = 100;
		_comboVelocityMode.LabelPosition = LabelPosition.Left;
		_comboVelocityMode.Width = 200;
		_comboVelocityMode.AddItem(0, "Range");
		_comboVelocityMode.AddItem(1, "Radial");
		_comboVelocityMode.AddItem(2, "Parent range");
		_comboVelocityMode.AddItem(3, "Parent radial");
		_comboVelocityMode.AddItem(5, "Parent range x parent velocity angle");
		_comboVelocityMode.ValueColor = _colorComboValue;
		_comboVelocityMode.Tooltip = "Velocity mode";
		_comboVelocityMode.OnValueChanged += OnComboValueChanged;
		num += 21;
		_textFieldVelocityFromX = new TextField(this);
		_textFieldVelocityFromX.Position.X = TEXT_X_4[0];
		_textFieldVelocityFromX.Position.Y = num;
		_textFieldVelocityFromX.Label = "Velocity:";
		_textFieldVelocityFromX.LabelWidth = 100;
		_textFieldVelocityFromX.SetLimits(-9999f, 9999f, 0f);
		_textFieldVelocityFromX.Width = 42;
		_textFieldVelocityFromX.Tooltip = "Velocity (from X)";
		_textFieldVelocityFromX.OnValueChanged += OnValueChanged;
		_textFieldVelocityFromY = new TextField(this);
		_textFieldVelocityFromY.Position.X = TEXT_X_4[1];
		_textFieldVelocityFromY.Position.Y = num;
		_textFieldVelocityFromY.SetLimits(-9999f, 9999f, 0f);
		_textFieldVelocityFromY.LabelWidth = 0;
		_textFieldVelocityFromY.Width = 42;
		_textFieldVelocityFromY.Tooltip = "Velocity (from Y)";
		_textFieldVelocityFromY.OnValueChanged += OnValueChanged;
		_textFieldVelocityToX = new TextField(this);
		_textFieldVelocityToX.Position.X = TEXT_X_4[2];
		_textFieldVelocityToX.Position.Y = num;
		_textFieldVelocityToX.Label = "-";
		_textFieldVelocityToX.LabelWidth = 15;
		_textFieldVelocityToX.SetLimits(-9999f, 9999f, 0f);
		_textFieldVelocityToX.Width = 42;
		_textFieldVelocityToX.Tooltip = "Velocity (to X)";
		_textFieldVelocityToX.OnValueChanged += OnValueChanged;
		_textFieldVelocityToY = new TextField(this);
		_textFieldVelocityToY.Position.X = TEXT_X_4[3];
		_textFieldVelocityToY.Position.Y = num;
		_textFieldVelocityToY.SetLimits(-9999f, 9999f, 0f);
		_textFieldVelocityToY.LabelWidth = 0;
		_textFieldVelocityToY.Width = 42;
		_textFieldVelocityToY.Tooltip = "Velocity (to Y)";
		_textFieldVelocityToY.OnValueChanged += OnValueChanged;
		_textFieldVelocityFromX.AddSharedField(_textFieldVelocityToX);
		_textFieldVelocityToX.AddSharedField(_textFieldVelocityFromX);
		_textFieldVelocityFromY.AddSharedField(_textFieldVelocityToY);
		_textFieldVelocityToY.AddSharedField(_textFieldVelocityFromY);
		num += 21;
		_textFieldForceFromX = new TextField(this);
		_textFieldForceFromX.Position.X = TEXT_X_4[0];
		_textFieldForceFromX.Position.Y = num;
		_textFieldForceFromX.Label = "Force:";
		_textFieldForceFromX.LabelWidth = 100;
		_textFieldForceFromX.SetLimits(-9999f, 9999f, 0f);
		_textFieldForceFromX.Width = 42;
		_textFieldForceFromX.Tooltip = "Global force (from X)";
		_textFieldForceFromX.OnValueChanged += OnValueChanged;
		_textFieldForceFromY = new TextField(this);
		_textFieldForceFromY.Position.X = TEXT_X_4[1];
		_textFieldForceFromY.Position.Y = num;
		_textFieldForceFromY.SetLimits(-9999f, 9999f, 0f);
		_textFieldForceFromY.LabelWidth = 0;
		_textFieldForceFromY.Width = 42;
		_textFieldForceFromY.Tooltip = "Global force (from Y)";
		_textFieldForceFromY.OnValueChanged += OnValueChanged;
		_textFieldForceToX = new TextField(this);
		_textFieldForceToX.Position.X = TEXT_X_4[2];
		_textFieldForceToX.Position.Y = num;
		_textFieldForceToX.Label = "-";
		_textFieldForceToX.LabelWidth = 15;
		_textFieldForceToX.SetLimits(-9999f, 9999f, 0f);
		_textFieldForceToX.Width = 42;
		_textFieldForceToX.Tooltip = "Global force (to X)";
		_textFieldForceToX.OnValueChanged += OnValueChanged;
		_textFieldForceToY = new TextField(this);
		_textFieldForceToY.Position.X = TEXT_X_4[3];
		_textFieldForceToY.Position.Y = num;
		_textFieldForceToY.SetLimits(-9999f, 9999f, 0f);
		_textFieldForceToY.LabelWidth = 0;
		_textFieldForceToY.Width = 42;
		_textFieldForceToY.Tooltip = "Global force (to Y)";
		_textFieldForceToY.OnValueChanged += OnValueChanged;
		_textFieldForceFromX.AddSharedField(_textFieldForceToX);
		_textFieldForceToX.AddSharedField(_textFieldForceFromX);
		_textFieldForceFromY.AddSharedField(_textFieldForceToY);
		_textFieldForceToY.AddSharedField(_textFieldForceFromY);
		num += 21;
		_textParentVelocityFactor = new TextField(this);
		_textParentVelocityFactor.Position.X = TEXT_X_4[0];
		_textParentVelocityFactor.Position.Y = num;
		_textParentVelocityFactor.Label = "Parent velo:";
		_textParentVelocityFactor.LabelWidth = 100;
		_textParentVelocityFactor.SetLimits(-9f, 9f, 0f, 1f);
		_textParentVelocityFactor.SetSteps(0.01f, 0.1f, 0.5f);
		_textParentVelocityFactor.ValueRoundFactor = 100f;
		_textParentVelocityFactor.Width = 89;
		_textParentVelocityFactor.Tooltip = "Parent velocity factor";
		_textParentVelocityFactor.OnValueChanged += OnValueChanged;
		_comboCompensateLevelScrolling = new ComboBox(this);
		_comboCompensateLevelScrolling.Position.X = TEXT_X_2[1];
		_comboCompensateLevelScrolling.Position.Y = num;
		_comboCompensateLevelScrolling.Label = ",";
		_comboCompensateLevelScrolling.LabelWidth = 15;
		_comboCompensateLevelScrolling.LabelPosition = LabelPosition.Left;
		_comboCompensateLevelScrolling.Width = 89;
		_comboCompensateLevelScrolling.AddItem(0, "By level");
		_comboCompensateLevelScrolling.AddItem(1, "Compensate");
		_comboCompensateLevelScrolling.AddItem(2, "Full");
		_comboCompensateLevelScrolling.AddItem(3, "Never");
		_comboCompensateLevelScrolling.AddItem(4, "Throw");
		_comboCompensateLevelScrolling.ValueColor = _colorComboValue;
		_comboCompensateLevelScrolling.Tooltip = "Compensate level scrolling";
		_comboCompensateLevelScrolling.OnValueChanged += OnComboValueChanged;
		num += 25;
		_comboTransformMode = new ComboBox(this);
		_comboTransformMode.Position.X = TEXT_X_2[0];
		_comboTransformMode.Position.Y = num;
		_comboTransformMode.Label = "Transf mode:";
		_comboTransformMode.LabelWidth = 100;
		_comboTransformMode.LabelPosition = LabelPosition.Left;
		_comboTransformMode.Width = 200;
		_comboTransformMode.AddItem(0, "World");
		_comboTransformMode.AddItem(2, "Parent start");
		_comboTransformMode.AddItem(1, "Parent");
		_comboTransformMode.ValueColor = _colorComboValue;
		_comboTransformMode.Tooltip = "Offset transformation mode";
		_comboTransformMode.OnValueChanged += OnComboValueChanged;
		num += 25;
		_comboScaleMode = new ComboBox(this);
		_comboScaleMode.Position.X = TEXT_X_2[0];
		_comboScaleMode.Position.Y = num;
		_comboScaleMode.Label = "Scale mode:";
		_comboScaleMode.LabelWidth = 100;
		_comboScaleMode.LabelPosition = LabelPosition.Left;
		_comboScaleMode.Width = 89;
		_comboScaleMode.AddItem("0", "Fixed");
		_comboScaleMode.AddItem("1", "Parent");
		_comboScaleMode.ValueColor = _colorComboValue;
		_comboScaleMode.Tooltip = "Scale mode";
		_comboScaleMode.OnValueChanged += OnComboValueChanged;
		_comboSameScaleXY = new ComboBox(this);
		_comboSameScaleXY.Position.X = TEXT_X_2[1];
		_comboSameScaleXY.Position.Y = num;
		_comboSameScaleXY.Label = "/";
		_comboSameScaleXY.LabelWidth = 15;
		_comboSameScaleXY.LabelPosition = LabelPosition.Left;
		_comboSameScaleXY.Width = 89;
		_comboSameScaleXY.AddItem("1", "X = Y");
		_comboSameScaleXY.AddItem("0", "X <> Y");
		_comboSameScaleXY.ValueColor = _colorComboValue;
		_comboSameScaleXY.Tooltip = "Same scale X and Y";
		_comboSameScaleXY.OnValueChanged += OnComboValueChanged;
		num += 21;
		_textFieldScaleFromX = new TextField(this);
		_textFieldScaleFromX.Position.X = TEXT_X_4[0];
		_textFieldScaleFromX.Position.Y = num;
		_textFieldScaleFromX.Label = "Scale:";
		_textFieldScaleFromX.LabelWidth = 100;
		_textFieldScaleFromX.SetLimits(0f, 99.9f, 0f, 1f);
		_textFieldScaleFromX.SetSteps(0.01f, 0.1f, 0.5f);
		_textFieldScaleFromX.ValueRoundFactor = 100f;
		_textFieldScaleFromX.Width = 42;
		_textFieldScaleFromX.Tooltip = "Scale (from X)";
		_textFieldScaleFromX.OnValueChanged += OnValueChanged;
		_textFieldScaleFromY = new TextField(this);
		_textFieldScaleFromY.Position.X = TEXT_X_4[1];
		_textFieldScaleFromY.Position.Y = num;
		_textFieldScaleFromY.SetLimits(0f, 99.9f, 0f, 1f);
		_textFieldScaleFromY.SetSteps(0.01f, 0.1f, 0.5f);
		_textFieldScaleFromY.ValueRoundFactor = 100f;
		_textFieldScaleFromY.LabelWidth = 0;
		_textFieldScaleFromY.Width = 42;
		_textFieldScaleFromY.Tooltip = "Scale (from Y)";
		_textFieldScaleFromY.OnValueChanged += OnValueChanged;
		_textFieldScaleToX = new TextField(this);
		_textFieldScaleToX.Position.X = TEXT_X_4[2];
		_textFieldScaleToX.Position.Y = num;
		_textFieldScaleToX.Label = "-";
		_textFieldScaleToX.LabelWidth = 15;
		_textFieldScaleToX.SetLimits(0f, 99.9f, 0f, 1f);
		_textFieldScaleToX.SetSteps(0.01f, 0.1f, 0.5f);
		_textFieldScaleToX.ValueRoundFactor = 100f;
		_textFieldScaleToX.Width = 42;
		_textFieldScaleToX.Tooltip = "Scale (to X)";
		_textFieldScaleToX.OnValueChanged += OnValueChanged;
		_textFieldScaleToY = new TextField(this);
		_textFieldScaleToY.Position.X = TEXT_X_4[3];
		_textFieldScaleToY.Position.Y = num;
		_textFieldScaleToY.SetLimits(0f, 99.9f, 0f, 1f);
		_textFieldScaleToY.SetSteps(0.01f, 0.1f, 0.5f);
		_textFieldScaleToY.ValueRoundFactor = 100f;
		_textFieldScaleToY.LabelWidth = 0;
		_textFieldScaleToY.Width = 42;
		_textFieldScaleToY.Tooltip = "Scale (to Y)";
		_textFieldScaleToY.OnValueChanged += OnValueChanged;
		_textFieldScaleFromX.AddSharedField(_textFieldScaleToX);
		_textFieldScaleToX.AddSharedField(_textFieldScaleFromX);
		_textFieldScaleFromY.AddSharedField(_textFieldScaleToY);
		_textFieldScaleToY.AddSharedField(_textFieldScaleFromY);
		num += 25;
		_textFieldPrewarmRepeatCount = new TextField(this);
		_textFieldPrewarmRepeatCount.Position.X = TEXT_X_2[0];
		_textFieldPrewarmRepeatCount.Position.Y = num;
		_textFieldPrewarmRepeatCount.Label = "Prewarm:";
		_textFieldPrewarmRepeatCount.LabelWidth = 100;
		_textFieldPrewarmRepeatCount.SetLimits(0f, 100f, 0f, 20f);
		_textFieldPrewarmRepeatCount.SetSteps(1f, 10f, 10f);
		_textFieldPrewarmRepeatCount.Width = 89;
		_textFieldPrewarmRepeatCount.Tooltip = "Prewarm repeat count";
		_textFieldPrewarmRepeatCount.OnValueChanged += OnValueChanged;
		_textFieldPrewarmTime = new TextField(this);
		_textFieldPrewarmTime.Position.X = TEXT_X_2[1];
		_textFieldPrewarmTime.Position.Y = num;
		_textFieldPrewarmTime.Label = "x";
		_textFieldPrewarmTime.LabelWidth = 15;
		_textFieldPrewarmTime.SetLimits(0f, 1f, 0f, 1f);
		_textFieldPrewarmTime.SetSteps(0.001f, 0.01f, 0.1f);
		_textFieldPrewarmTime.ValueRoundFactor = 1000f;
		_textFieldPrewarmTime.Width = 89;
		_textFieldPrewarmTime.Tooltip = "Prewarm time";
		_textFieldPrewarmTime.OnValueChanged += OnValueChanged;
		num += 25;
		_buttonSaveEmitter = new Button(null, this);
		_buttonSaveEmitter.Width = 100;
		_buttonSaveEmitter.Height = 24;
		_buttonSaveEmitter.Position.X = Width / 2 - _buttonSaveEmitter.Width / 2;
		_buttonSaveEmitter.Position.Y = Height - _buttonSaveEmitter.Height - 8;
		_buttonSaveEmitter.ForegroundColor = new Color(255, 255, 200);
		_buttonSaveEmitter.Tooltip = "Save current emitter";
		_buttonSaveEmitter.Caption = "Save";
		_buttonSaveEmitter.OnClick += OnSaveEmitterClick;
	}

	private void OnSaveEmitterClick(Button button)
	{
		_emitterData?.Save();
	}

	private void OnComboStripValueChanged(ComboBox combo)
	{
		if (combo.GetIntValue() == 0)
		{
			_emitterData.IsStripLine = false;
		}
		else
		{
			_emitterData.IsStripLine = true;
		}
	}

	private void OnValueChanged(TextField field, float delta)
	{
		if (_emitterData != null)
		{
			_emitterData.AnimDuration.From = _textFieldAnimDelayFrom.GetFloatValue();
			_emitterData.AnimDuration.To = _textFieldAnimDelayTo.GetFloatValue();
			_emitterData.LifeTimeRange.From = _textFieldLifeTimeFrom.GetFloatValue();
			_emitterData.LifeTimeRange.To = _textFieldLifeTimeTo.GetFloatValue();
			_emitterData.CountRange.From = (int)_textFieldCountFrom.GetFloatValue();
			_emitterData.CountRange.To = (int)_textFieldCountTo.GetFloatValue();
			_emitterData.LoopCountRange.From = (int)_textFieldLoopFrom.GetFloatValue();
			_emitterData.LoopCountRange.To = (int)_textFieldLoopTo.GetFloatValue();
			_emitterData.RotationRange.From = _textFieldAngleFrom.GetFloatValue();
			_emitterData.RotationRange.To = _textFieldAngleTo.GetFloatValue();
			_emitterData.RotationSpeedRange.From = _textFieldRotationSpeedFrom.GetFloatValue();
			_emitterData.RotationSpeedRange.To = _textFieldRotationSpeedTo.GetFloatValue();
			_emitterData.StartAlphaRange.From = (byte)Utils.Clamp(_textFieldAlphaFrom.GetFloatValue(), 0f, 255f);
			_emitterData.StartAlphaRange.To = (byte)Utils.Clamp(_textFieldAlphaTo.GetFloatValue(), 0f, 255f);
			_emitterData.OffsetRange.FromX = _textFieldOffsetFromX.GetFloatValue();
			_emitterData.OffsetRange.FromY = _textFieldOffsetFromY.GetFloatValue();
			_emitterData.OffsetRange.ToX = _textFieldOffsetToX.GetFloatValue();
			_emitterData.OffsetRange.ToY = _textFieldOffsetToY.GetFloatValue();
			_emitterData.ScaleRange.FromX = _textFieldScaleFromX.GetFloatValue();
			_emitterData.ScaleRange.FromY = _textFieldScaleFromY.GetFloatValue();
			_emitterData.ScaleRange.ToX = _textFieldScaleToX.GetFloatValue();
			_emitterData.ScaleRange.ToY = _textFieldScaleToY.GetFloatValue();
			_emitterData.VelocityRange.FromX = _textFieldVelocityFromX.GetFloatValue();
			_emitterData.VelocityRange.FromY = _textFieldVelocityFromY.GetFloatValue();
			_emitterData.VelocityRange.ToX = _textFieldVelocityToX.GetFloatValue();
			_emitterData.VelocityRange.ToY = _textFieldVelocityToY.GetFloatValue();
			_emitterData.ForceRange.FromX = _textFieldForceFromX.GetFloatValue();
			_emitterData.ForceRange.FromY = _textFieldForceFromY.GetFloatValue();
			_emitterData.ForceRange.ToX = _textFieldForceToX.GetFloatValue();
			_emitterData.ForceRange.ToY = _textFieldForceToY.GetFloatValue();
			_emitterData.ParentVelocityFactor = _textParentVelocityFactor.GetFloatValue();
			_emitterData.BlendFactor = Utils.Clamp(_textFieldBlendFactor.GetFloatValue(), 0f, 1f);
			_emitterData.LightFactor = Utils.Clamp(_textFieldLightFactor.GetFloatValue(), 0f, 1f);
			_emitterData.PrewarmRepeatCount = (int)_textFieldPrewarmRepeatCount.GetFloatValue();
			_emitterData.PrewarmTime = _textFieldPrewarmTime.GetFloatValue();
		}
	}

	private void OnDelayChanged(TextField field, float delta)
	{
		if (_emitterData != null)
		{
			_emitterData.DelayRange.From = _textFieldDelayFrom.GetFloatValue();
			_emitterData.DelayRange.To = _textFieldDelayTo.GetFloatValue();
			_emitterData.BeginDelayRange.From = _textFieldBeginDelayFrom.GetFloatValue();
			_emitterData.BeginDelayRange.To = _textFieldBeginDelayTo.GetFloatValue();
			_emitter.ZeroTimer();
		}
	}

	private void OnFlipComboValueChanged(ComboBox combo)
	{
		if (_emitterData != null)
		{
			int.TryParse(_comboFlipModeH.Value, out _emitterData.FlipHorizontal);
			int.TryParse(_comboFlipModeV.Value, out _emitterData.FlipVertical);
		}
	}

	private void OnFrameListChanged(TextField field, float delta)
	{
		if (_emitterData == null)
		{
			return;
		}
		_emitterData.FrameArray = Utils.GetIntArray(_textFieldFrameList.ValueString);
		if (_emitter.StripLine != null)
		{
			if (_emitterData.FrameArray.IsNullOrEmpty())
			{
				_emitter.StripLine.SetAtlas(_emitterData.Atlas, 0);
			}
			else
			{
				_emitter.StripLine.SetAtlas(_emitterData.Atlas, _emitterData.FrameArray[0]);
			}
		}
	}

	private void OnComboValueChanged(ComboBox combo)
	{
		if (_emitterData != null)
		{
			if (_comboOffsetShape.Value == "1")
			{
				_emitterData.OffsetShape = ParticleEmitterShapeEnum.Arc;
			}
			else
			{
				_emitterData.OffsetShape = ParticleEmitterShapeEnum.Rectangle;
			}
			if (_comboSameScaleXY.Value == "1")
			{
				_emitterData.SameScaleXY = true;
				_emitterData.ScaleRange.FromY = _emitterData.ScaleRange.FromX;
				_emitterData.ScaleRange.ToY = _emitterData.ScaleRange.ToX;
			}
			else
			{
				_emitterData.SameScaleXY = false;
			}
			_emitterData.Space = (ParticleSpace)_comboSpace.GetIntValue();
			_emitter.Space = _emitterData.Space;
			_emitterData.SortMode = (SortModeEnum)_comboSortMode.GetIntValue();
			_emitterData.RotationMode = (RotationMode)_comboRotationMode.GetIntValue();
			_emitterData.TransformationMode = (TransformationMode)_comboTransformMode.GetIntValue();
			_emitterData.ColorMode = (ColorMode)_comboColorMode.GetIntValue();
			_emitterData.UseParentScale = _comboScaleMode.GetIntValue() != 0;
			_emitterData.UseVelocityXToRotationDirection = _comboUseVeloToRotation.GetIntValue() != 0;
			_emitterData.DestroyOnLeftEdge = _comboDestroyOnLeft.GetIntValue() != 0;
			_emitterData.DestroyMode = (ParticleDestroyModeEnum)_comboDestroyMode.GetIntValue();
			_emitterData.CompensateLevelScrollingMode = (CompensateScrollingModeEnum)_comboCompensateLevelScrolling.GetIntValue();
			_emitterData.VelocityMode = (VelocityMode)_comboVelocityMode.GetIntValue();
			_emitterData.SpreadModeX = _comboSpreadModeX.GetIntValue();
			_emitterData.SpreadModeY = _comboSpreadModeY.GetIntValue();
			_emitterData.BlendMode = (BlendModeEnum)_comboBlendMode.GetIntValue();
			if (_emitter.StripLine != null)
			{
				_emitter.StripLine.BlendMode1 = _emitterData.BlendMode;
			}
		}
	}

	public void SetAtlas(SpriteAtlas atlas)
	{
		if (_emitterData == null)
		{
			return;
		}
		_emitterData.Atlas = atlas;
		if (_emitter.StripLine != null)
		{
			if (_emitterData.FrameArray.IsNullOrEmpty())
			{
				_emitter.StripLine.SetAtlas(atlas, 0);
			}
			else
			{
				_emitter.StripLine.SetAtlas(atlas, _emitterData.FrameArray[0]);
			}
			_emitter.StripLine.SetFrame2(_emitterData.StripFrame2, _emitterData.StripBlendMode2);
		}
	}

	public override void Update(float elapsedTime)
	{
		if (!IsEnabled || IsOff)
		{
			return;
		}
		base.Update(elapsedTime);
		if (IsMinimized)
		{
			return;
		}
		if (Editor.CurrentObject != null && Editor.CurrentObject.Emitter != null)
		{
			_emitter = Editor.CurrentObject.Emitter;
			if (_emitterData.Atlas != null)
			{
				_buttonAtlas.Caption = _emitterData.Atlas.Id;
			}
			else
			{
				_buttonAtlas.Caption = "(none)";
			}
			string text = "";
			if (!_textFieldFrameList.HasFocus && !_emitterData.FrameArray.IsNullOrEmpty())
			{
				for (int i = 0; i < _emitterData.FrameArray.Length; i++)
				{
					if (i > 0)
					{
						text += ",";
					}
					text += _emitterData.FrameArray[i];
				}
				_textFieldFrameList.SetStringValue(text);
			}
			_textFieldAnimDelayFrom.UpdateValue((int)_emitterData.AnimDuration.From);
			_textFieldAnimDelayTo.UpdateValue((int)_emitterData.AnimDuration.To);
			_textFieldCountFrom.UpdateValue(_emitterData.CountRange.From);
			_textFieldCountTo.UpdateValue(_emitterData.CountRange.To);
			_textFieldLifeTimeFrom.UpdateValue(_emitterData.LifeTimeRange.From);
			_textFieldLifeTimeTo.UpdateValue(_emitterData.LifeTimeRange.To);
			_textFieldDelayFrom.UpdateValue(_emitterData.DelayRange.From);
			_textFieldDelayTo.UpdateValue(_emitterData.DelayRange.To);
			_textFieldBeginDelayFrom.UpdateValue(_emitterData.BeginDelayRange.From);
			_textFieldBeginDelayTo.UpdateValue(_emitterData.BeginDelayRange.To);
			_textFieldLoopFrom.UpdateValue(_emitterData.LoopCountRange.From);
			_textFieldLoopTo.UpdateValue(_emitterData.LoopCountRange.To);
			_textFieldAngleFrom.UpdateValue(_emitterData.RotationRange.From);
			_textFieldAngleTo.UpdateValue(_emitterData.RotationRange.To);
			_textFieldRotationSpeedFrom.UpdateValue(_emitterData.RotationSpeedRange.From);
			_textFieldRotationSpeedTo.UpdateValue(_emitterData.RotationSpeedRange.To);
			_textFieldAlphaFrom.UpdateValue(_emitterData.StartAlphaRange.From);
			_textFieldAlphaTo.UpdateValue(_emitterData.StartAlphaRange.To);
			_textFieldBlendFactor.UpdateValue(_emitterData.BlendFactor);
			_comboBlendMode.ChooseIntValue((int)_emitterData.BlendMode);
			_textFieldLightFactor.UpdateValue(_emitterData.LightFactor);
			if (_emitterData.OffsetShape == ParticleEmitterShapeEnum.Arc)
			{
				_comboOffsetShape.ChooseIntValue(1);
			}
			else
			{
				_comboOffsetShape.ChooseIntValue(0);
			}
			_comboStrip.ChooseIntValue(_emitterData.IsStripLine ? 1 : 0);
			_comboSpace.ChooseIntValue((int)_emitterData.Space);
			_comboSortMode.ChooseIntValue((int)_emitterData.SortMode);
			_comboRotationMode.ChooseIntValue((int)_emitterData.RotationMode);
			_comboTransformMode.ChooseIntValue((int)_emitterData.TransformationMode);
			_comboColorMode.ChooseIntValue((int)_emitterData.ColorMode);
			_comboScaleMode.ChooseIntValue(_emitterData.UseParentScale ? 1 : 0);
			_comboUseVeloToRotation.ChooseIntValue(_emitterData.UseVelocityXToRotationDirection ? 1 : 0);
			_comboDestroyOnLeft.ChooseIntValue(_emitterData.DestroyOnLeftEdge ? 1 : 0);
			_comboDestroyMode.ChooseIntValue((int)_emitterData.DestroyMode);
			_comboCompensateLevelScrolling.ChooseIntValue((int)_emitterData.CompensateLevelScrollingMode);
			_comboSpreadModeX.ChooseIntValue(_emitterData.SpreadModeX);
			_comboSpreadModeY.ChooseIntValue(_emitterData.SpreadModeY);
			_comboFlipModeH.ChooseIntValue(_emitterData.FlipHorizontal);
			_comboFlipModeV.ChooseIntValue(_emitterData.FlipVertical);
			_textFieldOffsetFromX.UpdateValue((int)_emitterData.OffsetRange.FromX);
			_textFieldOffsetFromY.UpdateValue((int)_emitterData.OffsetRange.FromY);
			_textFieldOffsetToX.UpdateValue((int)_emitterData.OffsetRange.ToX);
			_textFieldOffsetToY.UpdateValue((int)_emitterData.OffsetRange.ToY);
			if (_emitterData.SameScaleXY)
			{
				_comboSameScaleXY.ChooseValue("1");
				_textFieldScaleFromY.IsVisible = false;
				_textFieldScaleToY.IsVisible = false;
				_textFieldScaleToX.Position.X = TEXT_X_4[1];
				_textFieldScaleToX.LabelWidth = 0;
				_textFieldScaleToX.Label = null;
			}
			else
			{
				_comboSameScaleXY.ChooseValue("0");
				_textFieldScaleFromY.IsVisible = true;
				_textFieldScaleToY.IsVisible = true;
				_textFieldScaleToX.Position.X = TEXT_X_4[2];
				_textFieldScaleToX.LabelWidth = 15;
				_textFieldScaleToX.Label = "-";
			}
			_textFieldScaleFromX.UpdateValue(_emitterData.ScaleRange.FromX);
			_textFieldScaleFromY.UpdateValue(_emitterData.ScaleRange.FromY);
			_textFieldScaleToX.UpdateValue(_emitterData.ScaleRange.ToX);
			_textFieldScaleToY.UpdateValue(_emitterData.ScaleRange.ToY);
			switch (_emitterData.VelocityMode)
			{
			case VelocityMode.Range:
			case VelocityMode.ParentRange:
			case VelocityMode.ParentVelocityAngle:
				_textFieldVelocityFromY.IsVisible = true;
				_textFieldVelocityToY.IsVisible = true;
				_textFieldVelocityToX.Position.X = TEXT_X_4[2];
				_textFieldVelocityToX.LabelWidth = 15;
				_textFieldVelocityToX.Label = "-";
				break;
			case VelocityMode.Radial:
			case VelocityMode.ParentRadial:
				_textFieldVelocityFromY.IsVisible = false;
				_textFieldVelocityToY.IsVisible = false;
				_textFieldVelocityToX.Position.X = TEXT_X_4[1];
				_textFieldVelocityToX.LabelWidth = 0;
				_textFieldVelocityToX.Label = null;
				break;
			}
			_comboVelocityMode.ChooseIntValue((int)_emitterData.VelocityMode);
			_textFieldVelocityFromX.UpdateValue(_emitterData.VelocityRange.FromX);
			_textFieldVelocityFromY.UpdateValue(_emitterData.VelocityRange.FromY);
			_textFieldVelocityToX.UpdateValue(_emitterData.VelocityRange.ToX);
			_textFieldVelocityToY.UpdateValue(_emitterData.VelocityRange.ToY);
			_textFieldForceFromX.UpdateValue(_emitterData.ForceRange.FromX);
			_textFieldForceFromY.UpdateValue(_emitterData.ForceRange.FromY);
			_textFieldForceToX.UpdateValue(_emitterData.ForceRange.ToX);
			_textFieldForceToY.UpdateValue(_emitterData.ForceRange.ToY);
			_textParentVelocityFactor.UpdateValue(_emitterData.ParentVelocityFactor);
			_textFieldPrewarmRepeatCount.UpdateValue(_emitterData.PrewarmRepeatCount);
			_textFieldPrewarmTime.UpdateValue(_emitterData.PrewarmTime);
			TitleText = $"{_emitterData.Id}";
		}
		else
		{
			TitleText = "- no emitter -";
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
