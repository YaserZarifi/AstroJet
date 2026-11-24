using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Rake2D;

public class ParticleEmitterData
{
	public string Id;

	public string TemplateId;

	public string FileName;

	public SpriteAtlas Atlas;

	public ParticleSpace Space = ParticleSpace.LastParticleLayerFront;

	public FloatRange LifeTimeRange;

	public FloatRange DelayRange;

	public FloatRange BeginDelayRange;

	public ParticleEmitterShapeEnum OffsetShape;

	public Vector2Range OffsetRange;

	public FloatRange AnimDuration;

	public bool IsAnimDurationAsDelay;

	public bool IsLooped;

	public SortModeEnum SortMode;

	public IntRange CountRange;

	public FloatRange RotationSpeedRange;

	public FloatRange RotationRange;

	public float TimeFactorMin;

	public float TimeFactorMax;

	public Vector2Range ScaleRange;

	public Vector2Range VelocityRange;

	public Vector2Range ForceRange;

	public IntRange StartAlphaRange;

	public float LightFactor;

	public IntRange LoopCountRange;

	public CompensateScrollingModeEnum CompensateLevelScrollingMode;

	public bool SameScaleXY;

	public bool UseParentScale;

	public ParticleDestroyModeEnum DestroyMode;

	public bool DestroyOnLeftEdge;

	public int FlipVertical;

	public int FlipHorizontal;

	public int PrewarmRepeatCount;

	public int SpreadModeX;

	public int SpreadModeY;

	public float PrewarmTime;

	public bool IsStripLine;

	public float StripTextureFlowSpeed1;

	public float StripTextureFlowSpeed2;

	public float StripBlendFactor2;

	public int StripFrame2 = -1;

	public BlendModeEnum StripBlendMode2 = BlendModeEnum.Additive;

	public bool StripLastZeroAlpha;

	public float BlendFactor = 1f;

	public BlendModeEnum BlendMode;

	public ColorMode ColorMode;

	public VelocityMode VelocityMode;

	public RotationMode RotationMode;

	public bool UseVelocityXToRotationDirection;

	public float ParentVelocityFactor;

	public TransformationMode TransformationMode;

	public EnvironmentEnum Environment;

	public List<TimeFunctionFloat> TimeFunctionAlphaList;

	public List<TimeFunctionVector2> TimeFunctionScaleList;

	public List<TimeFunctionVector2> TimeFunctionVelocityList;

	public List<TimeFunctionColor> TimeFunctionColorList;

	public int[] FrameArray;

	public ParticleEmitterData(string id)
	{
		Id = id;
		DelayRange = new FloatRange(1f);
		LoopCountRange = new IntRange(9999);
		CountRange = new IntRange(1);
		TimeFactorMin = (TimeFactorMax = 1f);
		UseParentScale = false;
		ScaleRange = new Vector2Range(1f, 1f, 1f, 1f);
		SameScaleXY = true;
		IsLooped = true;
		LifeTimeRange = new FloatRange(1f, 1f);
		StartAlphaRange = new IntRange(255);
		VelocityMode = VelocityMode.Range;
		Environment = EnvironmentEnum.Air;
		RotationMode = RotationMode.Fixed;
		ColorMode = ColorMode.Local;
		TransformationMode = TransformationMode.World;
		LightFactor = 0f;
	}

	public void SetCount(int min, int max)
	{
		CountRange = new IntRange(min, max);
	}

	public void SetLifeTime(float min, float max)
	{
		LifeTimeRange = new FloatRange(min, max);
	}

	public void SetLoopCount(int min, int max)
	{
		LoopCountRange = new IntRange(min, max);
	}

	public void SetRotation(float min, float max)
	{
		RotationRange = new FloatRange(min, max);
	}

	public void SetRotationSpeed(float min, float max)
	{
		RotationSpeedRange = new FloatRange(min, max);
	}

	public void SetAlpha(byte startAlphaMin, byte startAlphaMax)
	{
		StartAlphaRange = new IntRange(startAlphaMin, startAlphaMax);
	}

	private void AddTimeFunctionVector2(float startTime, float startX, float startY, ref List<TimeFunctionVector2> list)
	{
		if (list == null)
		{
			list = new List<TimeFunctionVector2>();
		}
		TimeFunctionVector2 timeFunctionVector = new TimeFunctionVector2();
		timeFunctionVector.StartTime = startTime;
		timeFunctionVector.StartValue = new Vector2(startX, startY);
		list.Add(timeFunctionVector);
	}

	public void AddTimeFunctionScale(float startTime, float startX, float startY)
	{
		AddTimeFunctionVector2(startTime, startX, startY, ref TimeFunctionScaleList);
	}

	public void AddTimeFunctionVelocity(float startTime, float startX, float startY)
	{
		AddTimeFunctionVector2(startTime, startX, startY, ref TimeFunctionVelocityList);
	}

	public void AddTimeFunctionAlpha(float startTime, float startValue)
	{
		if (TimeFunctionAlphaList == null)
		{
			TimeFunctionAlphaList = new List<TimeFunctionFloat>();
		}
		TimeFunctionFloat timeFunctionFloat = new TimeFunctionFloat();
		timeFunctionFloat.StartTime = startTime;
		timeFunctionFloat.StartValue = startValue;
		TimeFunctionAlphaList.Add(timeFunctionFloat);
	}

	public void AddTimeFunctionRGB(float startTime, int r, int g, int b)
	{
		if (TimeFunctionColorList == null)
		{
			TimeFunctionColorList = new List<TimeFunctionColor>();
		}
		TimeFunctionColor timeFunctionColor = new TimeFunctionColor();
		timeFunctionColor.StartTime = startTime;
		timeFunctionColor.StartR = r;
		timeFunctionColor.StartG = g;
		timeFunctionColor.StartB = b;
		TimeFunctionColorList.Add(timeFunctionColor);
	}

	public void CopyValues(ParticleEmitterData emitterData)
	{
		DestroyMode = emitterData.DestroyMode;
		Atlas = emitterData.Atlas;
		DelayRange = emitterData.DelayRange;
		BeginDelayRange = emitterData.BeginDelayRange;
		AnimDuration = emitterData.AnimDuration;
		IsAnimDurationAsDelay = emitterData.IsAnimDurationAsDelay;
		IsLooped = emitterData.IsLooped;
		SortMode = emitterData.SortMode;
		LifeTimeRange = emitterData.LifeTimeRange;
		ScaleRange = emitterData.ScaleRange;
		CountRange = emitterData.CountRange;
		RotationSpeedRange = emitterData.RotationSpeedRange;
		RotationRange = emitterData.RotationRange;
		TimeFactorMin = emitterData.TimeFactorMin;
		TimeFactorMax = emitterData.TimeFactorMax;
		OffsetRange = emitterData.OffsetRange;
		OffsetShape = emitterData.OffsetShape;
		VelocityRange = emitterData.VelocityRange;
		ForceRange = emitterData.ForceRange;
		StartAlphaRange = emitterData.StartAlphaRange;
		LightFactor = emitterData.LightFactor;
		LoopCountRange = emitterData.LoopCountRange;
		SameScaleXY = emitterData.SameScaleXY;
		UseParentScale = emitterData.UseParentScale;
		IsStripLine = emitterData.IsStripLine;
		DestroyOnLeftEdge = emitterData.DestroyOnLeftEdge;
		StripTextureFlowSpeed1 = emitterData.StripTextureFlowSpeed1;
		StripTextureFlowSpeed2 = emitterData.StripTextureFlowSpeed2;
		StripLastZeroAlpha = emitterData.StripLastZeroAlpha;
		StripBlendMode2 = emitterData.StripBlendMode2;
		BlendFactor = emitterData.BlendFactor;
		StripFrame2 = emitterData.StripFrame2;
		UseVelocityXToRotationDirection = emitterData.UseVelocityXToRotationDirection;
		BlendMode = emitterData.BlendMode;
		VelocityMode = emitterData.VelocityMode;
		ParentVelocityFactor = emitterData.ParentVelocityFactor;
		RotationMode = emitterData.RotationMode;
		ColorMode = emitterData.ColorMode;
		TransformationMode = emitterData.TransformationMode;
		Environment = emitterData.Environment;
		if (emitterData.TimeFunctionAlphaList != null)
		{
			TimeFunctionAlphaList = new List<TimeFunctionFloat>();
			emitterData.TimeFunctionAlphaList.ForEach((TimeFunctionFloat tf) =>
			{
				TimeFunctionFloat timeFunctionFloat = tf.Clone();
				timeFunctionFloat.MyList = TimeFunctionAlphaList;
				TimeFunctionAlphaList.Add(timeFunctionFloat);
			});
		}
		else
		{
			TimeFunctionAlphaList = null;
		}
		if (emitterData.TimeFunctionScaleList != null)
		{
			TimeFunctionScaleList = new List<TimeFunctionVector2>();
			emitterData.TimeFunctionScaleList.ForEach((TimeFunctionVector2 tf) =>
			{
				TimeFunctionVector2 timeFunctionVector = tf.Clone();
				timeFunctionVector.MyList = TimeFunctionScaleList;
				TimeFunctionScaleList.Add(timeFunctionVector);
			});
		}
		else
		{
			TimeFunctionScaleList = null;
		}
		if (emitterData.TimeFunctionVelocityList != null)
		{
			TimeFunctionVelocityList = new List<TimeFunctionVector2>();
			emitterData.TimeFunctionVelocityList.ForEach((TimeFunctionVector2 tf) =>
			{
				TimeFunctionVector2 timeFunctionVector = tf.Clone();
				timeFunctionVector.MyList = TimeFunctionVelocityList;
				TimeFunctionVelocityList.Add(timeFunctionVector);
			});
		}
		else
		{
			TimeFunctionVelocityList = null;
		}
		if (emitterData.TimeFunctionColorList != null)
		{
			TimeFunctionColorList = new List<TimeFunctionColor>();
			emitterData.TimeFunctionColorList.ForEach((TimeFunctionColor tf) =>
			{
				TimeFunctionColor timeFunctionColor = tf.Clone();
				timeFunctionColor.MyList = TimeFunctionColorList;
				TimeFunctionColorList.Add(timeFunctionColor);
			});
		}
		else
		{
			TimeFunctionColorList = null;
		}
		PrewarmRepeatCount = emitterData.PrewarmRepeatCount;
		PrewarmTime = emitterData.PrewarmTime;
		if (emitterData.FrameArray != null)
		{
			FrameArray = new int[emitterData.FrameArray.Length];
			emitterData.FrameArray.CopyTo(FrameArray, 0);
		}
	}

	private bool TemplateByIniFile(IniFile iniFile, bool fromId)
	{
		string text;
		if (fromId)
		{
			if (iniFile.Id == null)
			{
				return false;
			}
			text = iniFile.Id;
		}
		else
		{
			text = iniFile.Value;
		}
		ParticleEmitterData particleEmitterData = GameCore.GetParticleEmitterData(text);
		if (particleEmitterData == null)
		{
			iniFile.LogErr("Unable to find [Emitter = {0}] for template of '{1}'", text, Id);
			return false;
		}
		CopyValues(particleEmitterData);
		TemplateId = particleEmitterData.Id;
		return true;
	}

	internal void LoadFromIniFile(IniFile iniFile, GameContent content)
	{
		FileName = iniFile.FileName;
		TemplateByIniFile(iniFile, fromId: true);
		while (iniFile.NextLine())
		{
			if (DataLoader.CheckParams(iniFile, content))
			{
				continue;
			}
			if (iniFile.ValueType == IniFileValueType.MainSection)
			{
				break;
			}
			int[] resultArray;
			if (iniFile.KeyEquals("template"))
			{
				TemplateByIniFile(iniFile, fromId: false);
			}
			else if (iniFile.KeyEquals("env"))
			{
				Environment = DataUtils.GetEnvironmentByString(iniFile.Value);
			}
			else if (iniFile.KeyEquals("atlas"))
			{
				Atlas = GameCore.GetSpriteAtlas(iniFile.Value);
				if (Atlas == null)
				{
					iniFile.LogErr("Unable to find atlas '{0}'", iniFile.Value);
				}
			}
			else if (iniFile.KeyEquals("blend"))
			{
				if (iniFile.GetIntValue() == 1)
				{
					BlendMode = BlendModeEnum.Additive;
				}
				else if (iniFile.GetIntValue() == 2)
				{
					BlendMode = BlendModeEnum.Random;
				}
				else
				{
					BlendMode = BlendModeEnum.Normal;
				}
			}
			else if (iniFile.KeyEquals("alpha") && iniFile.TryGetIntArray(1, 2, out resultArray))
			{
				if (resultArray.Length > 1)
				{
					SetAlpha((byte)Utils.Clamp(resultArray[0], 0, 255), (byte)Utils.Clamp(resultArray[1], 0, 255));
				}
				else
				{
					SetAlpha((byte)Utils.Clamp(resultArray[0], 0, 255), (byte)Utils.Clamp(resultArray[0], 0, 255));
				}
			}
			else if (iniFile.KeyEquals("alpha_func") && iniFile.TryGetIntArray(2, out resultArray))
			{
				AddTimeFunctionAlpha((float)resultArray[0] / 100f, (float)resultArray[1] / 100f);
			}
			else if (iniFile.KeyEquals("color_mode"))
			{
				ColorMode = DataUtils.GetColorModeByString(iniFile.Value);
				if (ColorMode == ColorMode.Unknown)
				{
					ColorMode = ColorMode.Local;
					iniFile.LogErr("Unknown value '{0}'", iniFile.Value);
				}
			}
			else if (iniFile.KeyEquals("color_func") && iniFile.TryGetIntArray(4, out resultArray))
			{
				AddTimeFunctionRGB((float)resultArray[0] / 100f, resultArray[1], resultArray[2], resultArray[3]);
			}
			else if (iniFile.KeyEquals("count") && iniFile.TryGetIntArray(1, 2, out resultArray))
			{
				if (resultArray.Length == 2)
				{
					SetCount(resultArray[0], resultArray[1]);
				}
				else
				{
					SetCount(resultArray[0], resultArray[0]);
				}
			}
			else if (iniFile.KeyEquals("time_factor") && iniFile.TryGetIntArray(1, 2, out resultArray))
			{
				TimeFactorMin = (float)resultArray[0] / 100f;
				if (resultArray.Length > 1)
				{
					TimeFactorMax = (float)resultArray[1] / 100f;
				}
				else
				{
					TimeFactorMax = TimeFactorMin;
				}
			}
			else if (iniFile.KeyEquals("sort"))
			{
				try
				{
					SortMode = (SortModeEnum)iniFile.GetIntValue();
				}
				catch
				{
				}
			}
			else if (iniFile.KeyEquals("delay") && iniFile.TryGetFloatRange(ref DelayRange))
			{
				DelayRange /= 1000f;
			}
			else if (iniFile.KeyEquals("begin_delay") && iniFile.TryGetFloatRange(ref BeginDelayRange))
			{
				BeginDelayRange /= 1000f;
			}
			else if (iniFile.KeyEquals("anim_duration") && iniFile.TryGetIntArray(1, 2, out resultArray))
			{
				if (resultArray.Length > 1)
				{
					AnimDuration = new FloatRange((float)resultArray[0] / 1000f, (float)resultArray[1] / 1000f);
				}
				else
				{
					AnimDuration = new FloatRange((float)resultArray[0] / 1000f, (float)resultArray[0] / 1000f);
				}
				IsAnimDurationAsDelay = false;
			}
			else if (iniFile.KeyEquals("anim_delay") && iniFile.TryGetIntArray(1, 2, out resultArray))
			{
				if (resultArray.Length > 1)
				{
					AnimDuration = new FloatRange((float)resultArray[0] / 1000f, (float)resultArray[1] / 1000f);
				}
				else
				{
					AnimDuration = new FloatRange((float)resultArray[0] / 1000f, (float)resultArray[0] / 1000f);
				}
				IsAnimDurationAsDelay = true;
			}
			else if (iniFile.KeyEquals("anim_loop"))
			{
				IsLooped = iniFile.GetBoolValue();
			}
			else if (iniFile.KeyEquals("force") && iniFile.TryGetIntArray(2, 4, out resultArray))
			{
				if (resultArray.Length == 4)
				{
					ForceRange = new Vector2Range(resultArray[0], resultArray[1], resultArray[2], resultArray[3]);
				}
				else
				{
					ForceRange = new Vector2Range(resultArray[0], resultArray[1], resultArray[0], resultArray[1]);
				}
			}
			else if (iniFile.KeyEquals("life_time") && iniFile.TryGetIntArray(1, 2, out resultArray))
			{
				if (resultArray.Length > 1)
				{
					SetLifeTime((float)resultArray[0] / 1000f, (float)resultArray[1] / 1000f);
				}
				else
				{
					SetLifeTime((float)resultArray[0] / 1000f, (float)resultArray[0] / 1000f);
				}
			}
			else if (iniFile.KeyEquals("light_factor"))
			{
				LightFactor = (float)iniFile.GetIntValue() / 100f;
			}
			else if (iniFile.KeyEquals("offset") && iniFile.TryGetIntArray(2, 4, out resultArray))
			{
				if (resultArray.Length == 4)
				{
					OffsetRange = new Vector2Range(resultArray[0], resultArray[1], resultArray[2], resultArray[3]);
				}
				else
				{
					OffsetRange = new Vector2Range(resultArray[0], resultArray[1], resultArray[0], resultArray[1]);
				}
			}
			else if (iniFile.KeyEquals("offset_shape"))
			{
				if (iniFile.GetIdIntValue() == 1)
				{
					OffsetShape = ParticleEmitterShapeEnum.Arc;
				}
				else
				{
					OffsetShape = ParticleEmitterShapeEnum.Rectangle;
				}
			}
			else if (iniFile.KeyEquals("angle") && iniFile.TryGetIntArray(1, 2, out resultArray))
			{
				if (resultArray.Length > 1)
				{
					SetRotation(Utils.DegToRad(resultArray[0]), Utils.DegToRad(resultArray[1]));
				}
				else
				{
					SetRotation(Utils.DegToRad(resultArray[0]), Utils.DegToRad(resultArray[0]));
				}
			}
			else if (iniFile.KeyEquals("loop") && iniFile.TryGetIntArray(1, 2, out resultArray))
			{
				if (resultArray.Length > 1)
				{
					SetLoopCount(resultArray[0], resultArray[1]);
				}
				else
				{
					SetLoopCount(resultArray[0], resultArray[0]);
				}
			}
			else if (iniFile.KeyEquals("flip") && iniFile.TryGetIntArray(2, out resultArray))
			{
				FlipHorizontal = resultArray[0];
				FlipVertical = resultArray[1];
			}
			else if (iniFile.KeyEquals("velocity_rotation"))
			{
				UseVelocityXToRotationDirection = iniFile.GetBoolValue();
			}
			else if (iniFile.KeyEquals("rotation_speed") && iniFile.TryGetIntArray(1, 2, out resultArray))
			{
				if (resultArray.Length > 1)
				{
					SetRotationSpeed(Utils.DegToRad(resultArray[0]), Utils.DegToRad(resultArray[1]));
				}
				else
				{
					SetRotationSpeed(Utils.DegToRad(resultArray[0]), Utils.DegToRad(resultArray[0]));
				}
			}
			else if (iniFile.KeyEquals("rotation_mode"))
			{
				if (iniFile.ValueEquals("velocity"))
				{
					RotationMode = RotationMode.ByVelocity;
				}
				else if (iniFile.ValueEquals("velocity_continual"))
				{
					RotationMode = RotationMode.ByVelocityContinual;
				}
				else if (iniFile.ValueEquals("parent"))
				{
					RotationMode = RotationMode.Parent;
				}
				else if (iniFile.ValueEquals("parent_velocity"))
				{
					RotationMode = RotationMode.ByParentVelocity;
				}
				else if (iniFile.ValueEquals("fixed"))
				{
					RotationMode = RotationMode.Fixed;
				}
				else
				{
					iniFile.LogErr("Wrong value");
				}
			}
			else if (iniFile.KeyEquals("scale_mode"))
			{
				if (iniFile.ValueEquals("parent"))
				{
					UseParentScale = true;
					continue;
				}
				if (iniFile.ValueEquals("normal"))
				{
					UseParentScale = false;
					continue;
				}
				iniFile.LogErr("Unknown value '{0}'", iniFile.Value);
			}
			else if (iniFile.KeyEquals("scale") && iniFile.TryGetIntArray(1, 4, out resultArray))
			{
				if (resultArray.Length == 1)
				{
					ScaleRange = new Vector2Range((float)resultArray[0] / 100f, (float)resultArray[0] / 100f, (float)resultArray[0] / 100f, (float)resultArray[0] / 100f);
					SameScaleXY = true;
				}
				else if (resultArray.Length == 4)
				{
					ScaleRange = new Vector2Range((float)resultArray[0] / 100f, (float)resultArray[1] / 100f, (float)resultArray[2] / 100f, (float)resultArray[3] / 100f);
					SameScaleXY = false;
				}
				else
				{
					ScaleRange = new Vector2Range((float)resultArray[0] / 100f, (float)resultArray[0] / 100f, (float)resultArray[1] / 100f, (float)resultArray[1] / 100f);
					SameScaleXY = true;
				}
			}
			else if (iniFile.KeyEquals("scale_func") && iniFile.TryGetIntArray(2, 3, out resultArray))
			{
				if (resultArray.Length == 3)
				{
					AddTimeFunctionScale((float)resultArray[0] / 100f, (float)resultArray[1] / 100f, (float)resultArray[2] / 100f);
				}
				else
				{
					AddTimeFunctionScale((float)resultArray[0] / 100f, (float)resultArray[1] / 100f, (float)resultArray[1] / 100f);
				}
			}
			else if (iniFile.KeyEquals("speed_func") && iniFile.TryGetIntArray(2, 3, out resultArray))
			{
				if (resultArray.Length == 3)
				{
					AddTimeFunctionVelocity((float)resultArray[0] / 100f, (float)resultArray[1] / 100f, (float)resultArray[2] / 100f);
				}
				else
				{
					AddTimeFunctionVelocity((float)resultArray[0] / 100f, (float)resultArray[1] / 100f, (float)resultArray[1] / 100f);
				}
			}
			else if (iniFile.KeyEquals("transform_mode"))
			{
				if (iniFile.ValueEquals("parent"))
				{
					TransformationMode = TransformationMode.Parent;
					continue;
				}
				if (iniFile.ValueEquals("parent_position"))
				{
					TransformationMode = TransformationMode.ParentStart;
					continue;
				}
				if (iniFile.ValueEquals("parent_start"))
				{
					TransformationMode = TransformationMode.ParentStart;
					continue;
				}
				iniFile.LogErr("Unknown transform_mode '{0}'", iniFile.Value);
				TransformationMode = TransformationMode.World;
			}
			else if (iniFile.KeyEquals("velocity") && iniFile.TryGetIntArray(1, 4, out resultArray))
			{
				if (resultArray.Length == 1)
				{
					VelocityRange = new Vector2Range(resultArray[0], resultArray[0], resultArray[0], resultArray[0]);
				}
				else if (resultArray.Length == 4)
				{
					VelocityRange = new Vector2Range(resultArray[0], resultArray[1], resultArray[2], resultArray[3]);
				}
				else
				{
					VelocityRange = new Vector2Range(resultArray[0], resultArray[0], resultArray[1], resultArray[1]);
				}
			}
			else if (iniFile.KeyEquals("parent_velocity"))
			{
				ParentVelocityFactor = iniFile.GetFloatValue() / 100f;
			}
			else if (iniFile.KeyEquals("velocity_mode"))
			{
				if (iniFile.ValueEquals("radial"))
				{
					VelocityMode = VelocityMode.Radial;
				}
				else if (iniFile.ValueEquals("parent_range"))
				{
					VelocityMode = VelocityMode.ParentRange;
				}
				else if (iniFile.ValueEquals("parent_radial"))
				{
					VelocityMode = VelocityMode.ParentRadial;
				}
				else if (iniFile.ValueEquals("parent_velocity"))
				{
					VelocityMode = VelocityMode.ParentVelocity;
				}
				else if (iniFile.ValueEquals("parent_velocity_angle"))
				{
					VelocityMode = VelocityMode.ParentVelocityAngle;
				}
				else if (iniFile.ValueEquals("range"))
				{
					VelocityMode = VelocityMode.Range;
				}
				else
				{
					iniFile.LogErr($"Unknown velocity_mode = '{iniFile.Value}'");
				}
			}
			else
			{
				if (iniFile.KeyEquals("anim") && iniFile.TryGetStringArray(1, 99, out var _))
				{
					continue;
				}
				if (iniFile.KeyEquals("frame") && iniFile.TryGetIntArray(1, 999, out resultArray))
				{
					FrameArray = resultArray;
				}
				else if (iniFile.KeyEquals("strip"))
				{
					if (iniFile.ValueEquals("0"))
					{
						IsStripLine = false;
					}
					else
					{
						IsStripLine = true;
					}
				}
				else if (iniFile.KeyEquals("strip_flow"))
				{
					StripTextureFlowSpeed1 = iniFile.GetFloatValue() / 100f;
				}
				else if (iniFile.KeyEquals("strip_flow_2"))
				{
					StripTextureFlowSpeed2 = (float)iniFile.GetIntValue() / 100f;
				}
				else if (iniFile.KeyEquals("strip_frame_2"))
				{
					StripFrame2 = iniFile.GetIntValue();
				}
				else if (iniFile.KeyEquals("blend_factor"))
				{
					BlendFactor = MathHelper.Clamp(iniFile.GetFloatValue() / 100f, 0f, 1f);
				}
				else if (iniFile.KeyEquals("strip_blend_2"))
				{
					if (iniFile.GetIntValue() == 1)
					{
						StripBlendMode2 = BlendModeEnum.Additive;
					}
					else
					{
						StripBlendMode2 = BlendModeEnum.Normal;
					}
				}
				else if (iniFile.KeyEquals("destroy_on_left"))
				{
					DestroyOnLeftEdge = iniFile.GetBoolValue();
				}
				else if (iniFile.KeyEquals("destroy_mode"))
				{
					DestroyMode = (ParticleDestroyModeEnum)iniFile.GetIntValue();
				}
				else if (iniFile.KeyEquals("prewarm") && iniFile.TryGetIntArray(2, out resultArray))
				{
					PrewarmRepeatCount = resultArray[0];
					PrewarmTime = (float)resultArray[1] / 1000f;
				}
				else
				{
					iniFile.LogErr("Unknown emitter command");
				}
			}
		}
		BuildTimeFunctionFloat(ref TimeFunctionAlphaList);
		BuildTimeFunctionVector2(ref TimeFunctionScaleList);
		BuildTimeFunctionVector2(ref TimeFunctionVelocityList);
		BuildTimeFunctionColor(ref TimeFunctionColorList);
	}

	public static void BuildTimeFunctionFloat(ref List<TimeFunctionFloat> tfList)
	{
		if (tfList == null)
		{
			return;
		}
		tfList = tfList.OrderBy((TimeFunctionFloat tf) => tf.StartTime).ToList();
		for (int num = tfList.Count - 1; num >= 0; num--)
		{
			TimeFunctionFloat timeFunctionFloat = tfList[num];
			timeFunctionFloat.MyList = tfList;
			if (num == tfList.Count - 1)
			{
				timeFunctionFloat.Duration = 1f - timeFunctionFloat.StartTime;
				timeFunctionFloat.DeltaValue = 0f;
			}
			else if (timeFunctionFloat.StartTime >= tfList[num + 1].StartTime || timeFunctionFloat.StartTime < 0f)
			{
				tfList.RemoveAt(num);
			}
			else
			{
				timeFunctionFloat.Duration = tfList[num + 1].StartTime - timeFunctionFloat.StartTime;
				timeFunctionFloat.DeltaValue = tfList[num + 1].StartValue - timeFunctionFloat.StartValue;
			}
		}
		if (tfList.Count < 1)
		{
			tfList = null;
		}
	}

	public static void BuildTimeFunctionVector2(ref List<TimeFunctionVector2> tfList)
	{
		if (tfList == null)
		{
			return;
		}
		tfList = tfList.OrderBy((TimeFunctionVector2 tf) => tf.StartTime).ToList();
		for (int num = tfList.Count - 1; num >= 0; num--)
		{
			TimeFunctionVector2 timeFunctionVector = tfList[num];
			timeFunctionVector.MyList = tfList;
			if (num == tfList.Count - 1)
			{
				timeFunctionVector.Duration = 1f - timeFunctionVector.StartTime;
				timeFunctionVector.DeltaValue = Vector2.Zero;
			}
			else if (timeFunctionVector.StartTime >= tfList[num + 1].StartTime || timeFunctionVector.StartTime < 0f)
			{
				tfList.RemoveAt(num);
			}
			else
			{
				timeFunctionVector.Duration = tfList[num + 1].StartTime - timeFunctionVector.StartTime;
				timeFunctionVector.DeltaValue = tfList[num + 1].StartValue - timeFunctionVector.StartValue;
			}
		}
		if (tfList.Count < 1)
		{
			tfList = null;
		}
	}

	public static void BuildTimeFunctionColor(ref List<TimeFunctionColor> tfList)
	{
		if (tfList == null)
		{
			return;
		}
		tfList = tfList.OrderBy((TimeFunctionColor tf) => tf.StartTime).ToList();
		for (int num = tfList.Count - 1; num >= 0; num--)
		{
			TimeFunctionColor timeFunctionColor = tfList[num];
			timeFunctionColor.MyList = tfList;
			if (num == tfList.Count - 1)
			{
				timeFunctionColor.Duration = 1f - timeFunctionColor.StartTime;
				timeFunctionColor.DeltaR = (timeFunctionColor.DeltaG = (timeFunctionColor.DeltaB = 0));
			}
			else if (timeFunctionColor.StartTime >= tfList[num + 1].StartTime || timeFunctionColor.StartTime < 0f)
			{
				tfList.RemoveAt(num);
			}
			else
			{
				timeFunctionColor.Duration = tfList[num + 1].StartTime - timeFunctionColor.StartTime;
				timeFunctionColor.DeltaR = tfList[num + 1].StartR - timeFunctionColor.StartR;
				timeFunctionColor.DeltaG = tfList[num + 1].StartG - timeFunctionColor.StartG;
				timeFunctionColor.DeltaB = tfList[num + 1].StartB - timeFunctionColor.StartB;
			}
		}
		if (tfList.Count < 1)
		{
			tfList = null;
		}
	}

	internal void LoadFromIniFileEmt(IniFile iniFile, GameContent content)
	{
		FileName = iniFile.FileName;
		TemplateByIniFile(iniFile, fromId: true);
		while (iniFile.NextLine())
		{
			if (DataLoader.CheckParams(iniFile, content))
			{
				continue;
			}
			if (iniFile.ValueType == IniFileValueType.MainSection)
			{
				break;
			}
			float[] resultArray2;
			if (iniFile.KeyEquals("frm") && iniFile.TryGetIntArray(1, 999, out var resultArray))
			{
				FrameArray = resultArray;
			}
			else if (iniFile.KeyEquals("env"))
			{
				Environment = DataUtils.GetEnvironmentByString(iniFile.Value);
			}
			else if (iniFile.KeyEquals("atl"))
			{
				Atlas = GameCore.GetSpriteAtlas(iniFile.Value);
				if (Atlas == null)
				{
					iniFile.LogErr("Unable to find atlas '{0}'", iniFile.Value);
				}
			}
			else if (iniFile.KeyEquals("lft") && iniFile.TryGetFloatArray(2, out resultArray2))
			{
				LifeTimeRange = new FloatRange(resultArray2);
			}
			else if (iniFile.KeyEquals("cnt") && iniFile.TryGetFloatArray(6, out resultArray2))
			{
				CountRange = new IntRange((int)resultArray2[0], (int)resultArray2[1]);
				DelayRange = new FloatRange(resultArray2[2], resultArray2[3]);
				LoopCountRange = new IntRange((int)resultArray2[4], (int)resultArray2[5]);
			}
			else if (iniFile.KeyEquals("bdl") && iniFile.TryGetFloatArray(2, out resultArray2))
			{
				BeginDelayRange = new FloatRange(resultArray2[0], resultArray2[1]);
			}
			else if (iniFile.KeyEquals("flp") && iniFile.TryGetIntArray(3, out resultArray))
			{
				FlipHorizontal = resultArray[0];
				FlipVertical = resultArray[1];
				try
				{
					SortMode = (SortModeEnum)resultArray[2];
				}
				catch
				{
					SortMode = SortModeEnum.AsLast;
				}
			}
			else if (iniFile.KeyEquals("vel") && iniFile.TryGetIntArray(5, out resultArray))
			{
				try
				{
					VelocityMode = (VelocityMode)resultArray[0];
				}
				catch
				{
					VelocityMode = VelocityMode.Range;
				}
				VelocityRange = new Vector2Range(resultArray[1], resultArray[2], resultArray[3], resultArray[4]);
			}
			else if (iniFile.KeyEquals("pvf"))
			{
				ParentVelocityFactor = iniFile.GetFloatValue();
			}
			else if (iniFile.KeyEquals("spc"))
			{
				try
				{
					Space = (ParticleSpace)iniFile.GetIntValue();
				}
				catch
				{
				}
			}
			else if (iniFile.KeyEquals("frc") && iniFile.TryGetIntArray(4, out resultArray))
			{
				ForceRange = new Vector2Range(resultArray[0], resultArray[1], resultArray[2], resultArray[3]);
			}
			else if (iniFile.KeyEquals("rot") && iniFile.TryGetIntArray(5, out resultArray))
			{
				try
				{
					RotationMode = (RotationMode)resultArray[0];
				}
				catch
				{
					RotationMode = RotationMode.Fixed;
				}
				RotationRange = new FloatRange(Utils.DegToRad(resultArray[1]), Utils.DegToRad(resultArray[2]));
				RotationSpeedRange = new FloatRange(Utils.DegToRad(resultArray[3]), Utils.DegToRad(resultArray[4]));
			}
			else if (iniFile.KeyEquals("uvr"))
			{
				UseVelocityXToRotationDirection = iniFile.GetBoolValue();
			}
			else if (iniFile.KeyEquals("dst") && iniFile.TryGetIntArray(2, out resultArray))
			{
				DestroyOnLeftEdge = resultArray[0] != 0;
				DestroyMode = (ParticleDestroyModeEnum)resultArray[1];
			}
			else if (iniFile.KeyEquals("off") && iniFile.TryGetIntArray(6, out resultArray))
			{
				TransformationMode = (TransformationMode)resultArray[0];
				try
				{
					OffsetShape = (ParticleEmitterShapeEnum)resultArray[1];
				}
				catch
				{
					OffsetShape = ParticleEmitterShapeEnum.Rectangle;
				}
				OffsetRange = new Vector2Range(resultArray[2], resultArray[3], resultArray[4], resultArray[5]);
			}
			else if (iniFile.KeyEquals("spr") && iniFile.TryGetIntArray(2, out resultArray))
			{
				SpreadModeX = resultArray[0];
				SpreadModeY = resultArray[1];
			}
			else if (iniFile.KeyEquals("scl") && iniFile.TryGetFloatArray(3, 5, out resultArray2))
			{
				UseParentScale = resultArray2[0] != 0f;
				if (resultArray2.Length == 3)
				{
					SameScaleXY = true;
					ScaleRange = new Vector2Range(resultArray2[1], resultArray2[1], resultArray2[2], resultArray2[2]);
				}
				else
				{
					SameScaleXY = false;
					ScaleRange = new Vector2Range(resultArray2[1], resultArray2[2], resultArray2[3], resultArray2[4]);
				}
			}
			else if (iniFile.KeyEquals("clm") && iniFile.TryGetIntArray(4, out resultArray))
			{
				try
				{
					ColorMode = (ColorMode)resultArray[0];
				}
				catch
				{
					ColorMode = ColorMode.Local;
				}
				StartAlphaRange = new IntRange(resultArray[1], resultArray[2]);
				LightFactor = (float)resultArray[3] / 100f;
			}
			else if (iniFile.KeyEquals("blm") && iniFile.TryGetFloatArray(2, out resultArray2))
			{
				try
				{
					BlendMode = (BlendModeEnum)resultArray2[0];
				}
				catch
				{
					BlendMode = BlendModeEnum.Normal;
				}
				BlendFactor = resultArray2[1];
			}
			else if (iniFile.KeyEquals("prw") && iniFile.TryGetFloatArray(2, out resultArray2))
			{
				PrewarmRepeatCount = (int)resultArray2[0];
				PrewarmTime = resultArray2[1];
			}
			else if (iniFile.KeyEquals("str") && iniFile.TryGetFloatArray(5, out resultArray2))
			{
				StripTextureFlowSpeed1 = resultArray2[0];
				StripTextureFlowSpeed2 = resultArray2[1];
				StripFrame2 = (int)resultArray2[2];
				StripBlendMode2 = (BlendModeEnum)resultArray2[3];
				StripLastZeroAlpha = resultArray2[4] != 0f;
				IsStripLine = true;
			}
			else if (iniFile.KeyEquals("adr") && iniFile.TryGetFloatArray(3, out resultArray2))
			{
				AnimDuration = new FloatRange(resultArray2[0], resultArray2[1]);
				IsAnimDurationAsDelay = resultArray2[0] != 0f;
			}
			else if (iniFile.KeyEquals("com"))
			{
				CompensateLevelScrollingMode = (CompensateScrollingModeEnum)iniFile.GetIntValue();
			}
			else if (iniFile.KeyEquals("a_f") && iniFile.TryGetFloatArray(2, 999, out resultArray2))
			{
				TimeFunctionAlphaList = new List<TimeFunctionFloat>(resultArray2.Length / 2);
				for (int i = 0; i < resultArray2.Length; i += 2)
				{
					TimeFunctionFloat timeFunctionFloat = new TimeFunctionFloat();
					timeFunctionFloat.StartTime = resultArray2[i];
					timeFunctionFloat.StartValue = resultArray2[i + 1];
					timeFunctionFloat.MyList = TimeFunctionAlphaList;
					TimeFunctionAlphaList.Add(timeFunctionFloat);
				}
				BuildTimeFunctionFloat(ref TimeFunctionAlphaList);
			}
			else if (iniFile.KeyEquals("s_f") && iniFile.TryGetFloatArray(3, 999, out resultArray2))
			{
				TimeFunctionScaleList = new List<TimeFunctionVector2>(resultArray2.Length / 3);
				for (int j = 0; j < resultArray2.Length; j += 3)
				{
					TimeFunctionVector2 timeFunctionVector = new TimeFunctionVector2();
					timeFunctionVector.StartTime = resultArray2[j];
					timeFunctionVector.StartValue = new Vector2(resultArray2[j + 1], resultArray2[j + 2]);
					timeFunctionVector.MyList = TimeFunctionScaleList;
					TimeFunctionScaleList.Add(timeFunctionVector);
				}
				BuildTimeFunctionVector2(ref TimeFunctionScaleList);
			}
			else if (iniFile.KeyEquals("v_f") && iniFile.TryGetFloatArray(3, 999, out resultArray2))
			{
				TimeFunctionVelocityList = new List<TimeFunctionVector2>(resultArray2.Length / 3);
				for (int k = 0; k < resultArray2.Length; k += 3)
				{
					TimeFunctionVector2 timeFunctionVector2 = new TimeFunctionVector2();
					timeFunctionVector2.StartTime = resultArray2[k];
					timeFunctionVector2.StartValue = new Vector2(resultArray2[k + 1], resultArray2[k + 2]);
					timeFunctionVector2.MyList = TimeFunctionVelocityList;
					TimeFunctionVelocityList.Add(timeFunctionVector2);
				}
				BuildTimeFunctionVector2(ref TimeFunctionVelocityList);
			}
			else if (iniFile.KeyEquals("c_f") && iniFile.TryGetFloatArray(4, 999, out resultArray2))
			{
				TimeFunctionColorList = new List<TimeFunctionColor>(resultArray2.Length / 4);
				for (int l = 0; l < resultArray2.Length; l += 4)
				{
					TimeFunctionColor timeFunctionColor = new TimeFunctionColor();
					timeFunctionColor.StartTime = resultArray2[l];
					timeFunctionColor.StartR = (int)resultArray2[l + 1];
					timeFunctionColor.StartG = (int)resultArray2[l + 2];
					timeFunctionColor.StartB = (int)resultArray2[l + 3];
					timeFunctionColor.MyList = TimeFunctionColorList;
					TimeFunctionColorList.Add(timeFunctionColor);
				}
				BuildTimeFunctionColor(ref TimeFunctionColorList);
			}
			else
			{
				iniFile.LogErr("Unknown param");
			}
		}
		AfterLoad();
	}

	private void AfterLoad()
	{
		if (FrameArray == null)
		{
			FrameArray = new int[1];
		}
	}

	public List<string> SaveToLines()
	{
		List<string> list = new List<string>();
		list.Add($"[emt={Id}]");
		if (Atlas != null)
		{
			list.Add($"\tatl={Atlas.Id}");
		}
		if (FrameArray != null && FrameArray.Length != 0)
		{
			int num = 0;
			if (FrameArray.Length > 1 || FrameArray[0] != 0)
			{
				string text = "\tfrm=";
				int[] frameArray = FrameArray;
				foreach (int num2 in frameArray)
				{
					if (num > 0)
					{
						text += ",";
					}
					text += num2;
					num++;
				}
				list.Add(text);
			}
		}
		list.Add($"\tspc={(int)Space}");
		list.Add($"\tlft={Utils.FormatFloat4(LifeTimeRange.From)},{Utils.FormatFloat4(LifeTimeRange.To)}");
		list.Add($"\tcnt={CountRange.From},{CountRange.To},{Utils.FormatFloat4(DelayRange.From)},{Utils.FormatFloat4(DelayRange.To)},{LoopCountRange.From},{LoopCountRange.To}");
		if (!BeginDelayRange.IsZero)
		{
			list.Add($"\tbdl={Utils.FormatFloat4(BeginDelayRange.From)},{Utils.FormatFloat4(BeginDelayRange.To)}");
		}
		list.Add($"\tflp={FlipHorizontal},{FlipVertical},{(int)SortMode}");
		list.Add($"\tvel={(int)VelocityMode},{VelocityRange.FromX},{VelocityRange.FromY},{VelocityRange.ToX},{VelocityRange.ToY}");
		if (ParentVelocityFactor != 0f)
		{
			list.Add($"\tpvf={Utils.FormatFloat4(ParentVelocityFactor)}");
		}
		if (!ForceRange.IsZero)
		{
			list.Add($"\tfrc={(int)ForceRange.FromX},{(int)ForceRange.FromY},{(int)ForceRange.ToX},{(int)ForceRange.ToY}");
		}
		list.Add($"\trot={(int)RotationMode},{(int)Utils.RadToDeg(RotationRange.From)},{(int)Utils.RadToDeg(RotationRange.To)},{(int)Utils.RadToDeg(RotationSpeedRange.From)},{(int)Utils.RadToDeg(RotationSpeedRange.To)}");
		if (UseVelocityXToRotationDirection)
		{
			list.Add("\tuvr=1");
		}
		ParticleEmitterShapeEnum offsetShape = OffsetShape;
		int num3 = ((offsetShape == ParticleEmitterShapeEnum.Arc) ? 1 : 0);
		list.Add($"\toff={(int)TransformationMode},{num3},{OffsetRange.FromX},{OffsetRange.FromY},{OffsetRange.ToX},{OffsetRange.ToY}");
		if (SpreadModeX != 0 || SpreadModeY != 0)
		{
			list.Add($"\tspr={SpreadModeX},{SpreadModeY}");
		}
		if (SameScaleXY)
		{
			list.Add($"\tscl={(UseParentScale ? 1 : 0)},{Utils.FormatFloat4(ScaleRange.FromX)},{Utils.FormatFloat4(ScaleRange.ToX)}");
		}
		else
		{
			list.Add($"\tscl={(UseParentScale ? 1 : 0)},{Utils.FormatFloat4(ScaleRange.FromX)},{Utils.FormatFloat4(ScaleRange.FromY)},{Utils.FormatFloat4(ScaleRange.ToX)},{Utils.FormatFloat4(ScaleRange.ToY)}");
		}
		list.Add($"\tclm={(int)ColorMode},{StartAlphaRange.From},{StartAlphaRange.To},{(int)(LightFactor * 100f)}");
		list.Add($"\tblm={(int)BlendMode},{Utils.FormatFloat4(BlendFactor)}");
		if (DestroyOnLeftEdge || DestroyMode != ParticleDestroyModeEnum.None)
		{
			list.Add($"\tdst={(DestroyOnLeftEdge ? 1 : 0)},{(int)DestroyMode}");
		}
		if (PrewarmRepeatCount > 0 || PrewarmTime > 0f)
		{
			list.Add($"\tprw={PrewarmRepeatCount},{Utils.FormatFloat4(PrewarmTime)}");
		}
		if (IsStripLine)
		{
			list.Add($"\tstr={Utils.FormatFloat4(StripTextureFlowSpeed1)},{Utils.FormatFloat4(StripTextureFlowSpeed2)},{StripFrame2},{(int)StripBlendMode2},{(StripLastZeroAlpha ? 1 : 0)}");
		}
		if (!AnimDuration.IsZero)
		{
			list.Add($"\tadr={Utils.FormatFloat4(AnimDuration.From)},{Utils.FormatFloat4(AnimDuration.To)},{(IsAnimDurationAsDelay ? 1 : 0)}");
		}
		if (CompensateLevelScrollingMode != CompensateScrollingModeEnum.ByLevel)
		{
			list.Add($"\tcom={(int)CompensateLevelScrollingMode}");
		}
		if (!TimeFunctionAlphaList.IsNullOrEmpty())
		{
			string text = "\ta_f=";
			for (int j = 0; j < TimeFunctionAlphaList.Count; j++)
			{
				if (j > 0)
				{
					text += ", ";
				}
				text += $"{Utils.FormatFloat4(TimeFunctionAlphaList[j].StartTime)},{Utils.FormatFloat4(TimeFunctionAlphaList[j].StartValue)}";
			}
			list.Add(text);
		}
		if (!TimeFunctionScaleList.IsNullOrEmpty())
		{
			string text = "\ts_f=";
			for (int k = 0; k < TimeFunctionScaleList.Count; k++)
			{
				if (k > 0)
				{
					text += ", ";
				}
				text += $"{Utils.FormatFloat4(TimeFunctionScaleList[k].StartTime)},{Utils.FormatFloat4(TimeFunctionScaleList[k].StartValue.X)},{Utils.FormatFloat4(TimeFunctionScaleList[k].StartValue.Y)}";
			}
			list.Add(text);
		}
		if (!TimeFunctionVelocityList.IsNullOrEmpty())
		{
			string text = "\tv_f=";
			for (int l = 0; l < TimeFunctionVelocityList.Count; l++)
			{
				if (l > 0)
				{
					text += ", ";
				}
				text += $"{Utils.FormatFloat4(TimeFunctionVelocityList[l].StartTime)},{Utils.FormatFloat4(TimeFunctionVelocityList[l].StartValue.X)},{Utils.FormatFloat4(TimeFunctionVelocityList[l].StartValue.Y)}";
			}
			list.Add(text);
		}
		if (!TimeFunctionColorList.IsNullOrEmpty())
		{
			string text = "\tc_f=";
			for (int m = 0; m < TimeFunctionColorList.Count; m++)
			{
				if (m > 0)
				{
					text += ", ";
				}
				text += $"{Utils.FormatFloat4(TimeFunctionColorList[m].StartTime)},{TimeFunctionColorList[m].StartR},{TimeFunctionColorList[m].StartG},{TimeFunctionColorList[m].StartB}";
			}
			list.Add(text);
		}
		list.Add("");
		return list;
	}

	public void Save()
	{
		using IniFile iniFile = IniFile.OpenFromContent(FileName);
		iniFile.ReplaceMainSection(new string[2] { "emitter", "emt" }, null, Id, SaveToLines());
		iniFile.SaveToFile(FileName);
	}

	public override string ToString()
	{
		return Id;
	}
}
