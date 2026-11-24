using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Debug;

namespace Rake2D;

public class Particle : Transformation
{
	private static ParticleListRenderer ParticleListRenderer;

	public float CreateTime;

	public float RotationSpeed;

	public Vector2 Velocity;

	public float StartupVelocityRotation;

	public Vector2 Force;

	public Vector2 ForceVelocity;

	public string Text;

	public float LifeTime;

	public float StripTextureCoordY;

	public float StripTextureCoordY2;

	public Color StartupColor;

	public float StartupRotation;

	public Vector2 StartupScale;

	public Vector2 StartupVelocity;

	public bool IsDestroyed;

	public ParticleEmitterData Data;

	public ParticleEmitter MyEmitter;

	public BlendModeEnum BlendMode;

	public SpriteEffects SpriteEffects;

	private Vector2 _textSize;

	private int _frame;

	private SpriteRenderer _spriteRenderer;

	protected float _timer;

	private int _alphaFunctionIndex;

	private int _scaleFunctionIndex;

	private int _speedFunctionIndex;

	private int _colorFunctionIndex;

	public Color SpriteColor
	{
		get
		{
			if (_spriteRenderer != null)
			{
				return _spriteRenderer.Color;
			}
			return Color;
		}
		set
		{
			if (_spriteRenderer != null)
			{
				_spriteRenderer.Color = value;
			}
			Color = value;
		}
	}

	public bool IsAdditive
	{
		get
		{
			if (BlendMode != BlendModeEnum.Additive)
			{
				if (BlendMode == BlendModeEnum.Default)
				{
					return Data.BlendMode == BlendModeEnum.Additive;
				}
				return false;
			}
			return true;
		}
	}

	public int Frame
	{
		get
		{
			if (_spriteRenderer != null)
			{
				return _spriteRenderer.Frame;
			}
			return _frame;
		}
		set
		{
			_frame = value;
			if (_spriteRenderer != null)
			{
				_spriteRenderer.Frame = _frame;
			}
		}
	}

	public override Matrix WorldMatrix
	{
		get
		{
			if (Data.TransformationMode == TransformationMode.ParentStart && ParentTransformation != null)
			{
				return GetLocalMatrix() * Matrix.CreateTranslation(ParentTransformation.WorldPosition.X, ParentTransformation.WorldPosition.Y, 0f);
			}
			return base.WorldMatrix;
		}
	}

	static Particle()
	{
		ParticleListRenderer = new ParticleListRenderer();
	}

	public void Init(ParticleEmitterData data, Vector2 position)
	{
		ClearTransformation(isInit: true);
		Data = data;
		_timer = 0f;
		_alphaFunctionIndex = 0;
		_scaleFunctionIndex = 0;
		_speedFunctionIndex = 0;
		_colorFunctionIndex = 0;
		Text = null;
		_textSize = Vector2.Zero;
		StartupVelocityRotation = 0f;
		StripTextureCoordY = 1f;
		StripTextureCoordY2 = 1f;
		SpriteEffects = SpriteEffects.None;
		Position = position;
		Force = Vector2.Zero;
		ForceVelocity = Vector2.Zero;
		LifeTime = Data.LifeTimeRange.GetRandom();
		if (Data.SameScaleXY)
		{
			Scale.X = (Scale.Y = Data.ScaleRange.GetRandomX());
		}
		else
		{
			Scale = Data.ScaleRange.GetRandom();
		}
		int[] array = null;
		int num = -1;
		if (Data.AnimDuration.IsZero)
		{
			num = ((!Data.FrameArray.IsNullOrEmpty()) ? Utils.GetRandomItem(Data.FrameArray) : 0);
		}
		else
		{
			array = Data.FrameArray;
		}
		if (Data.Atlas != null && (array != null || num >= 0))
		{
			float num2 = 0f;
			if (Data.AnimDuration.From > 0f)
			{
				num2 = Data.AnimDuration.GetRandom() / 1000f;
				if (Data.IsAnimDurationAsDelay && array != null)
				{
					num2 *= (float)array.Length;
				}
			}
			else if (Data.AnimDuration.From < 0f)
			{
				num2 = LifeTime;
			}
			_spriteRenderer = GameCore.Allocator.GetSpriteRenderer(Data.Atlas);
			if (array != null && array.Length != 0)
			{
				if (num2 > 0f && array.Length > 1)
				{
					_spriteRenderer.CreateAnimator(array, num2, Data.IsLooped);
				}
				else
				{
					_spriteRenderer.Frame = array[0];
				}
			}
			else if (num >= 0)
			{
				_spriteRenderer.Frame = num;
			}
		}
		IsDestroyed = false;
	}

	public void ClearParticle()
	{
		ClearTransformation(isInit: false);
		if (_spriteRenderer != null)
		{
			GameCore.Allocator.DeleteSpriteRenderer(_spriteRenderer);
			_spriteRenderer = null;
		}
	}

	private void UpdateTimeFunctions()
	{
		float time = _timer / LifeTime;
		if (Data.TimeFunctionColorList != null && Data.TimeFunctionColorList.Count > 0)
		{
			UpdateColorTimeFunction(time);
		}
		if (Data.TimeFunctionAlphaList != null && Data.TimeFunctionAlphaList.Count > 0)
		{
			UpdateAlphaTimeFunction(time);
		}
		if (Data.TimeFunctionScaleList != null && Data.TimeFunctionScaleList.Count > 0)
		{
			UpdateScaleTimeFunction(time);
		}
		if (Data.TimeFunctionVelocityList != null && Data.TimeFunctionVelocityList.Count > 0)
		{
			UpdateSpeedTimeFunction(time);
		}
		else
		{
			Velocity = StartupVelocity;
		}
	}

	private void UpdateScaleTimeFunction(float time)
	{
		_scaleFunctionIndex = Utils.Clamp(_scaleFunctionIndex, 0, Data.TimeFunctionScaleList.Count - 1);
		TimeFunctionVector2 timeFunctionVector = Data.TimeFunctionScaleList[_scaleFunctionIndex];
		if (_scaleFunctionIndex == 0 && time < timeFunctionVector.StartTime)
		{
			Vector2 vector = timeFunctionVector.StartValue - StartupScale;
			Scale = StartupScale + vector * (time / timeFunctionVector.StartTime);
			return;
		}
		if (time >= timeFunctionVector.StartTime + timeFunctionVector.Duration)
		{
			if (_scaleFunctionIndex < Data.TimeFunctionScaleList.Count - 1)
			{
				_scaleFunctionIndex++;
				timeFunctionVector = Data.TimeFunctionScaleList[_scaleFunctionIndex];
			}
			else
			{
				time = timeFunctionVector.StartTime + timeFunctionVector.Duration;
			}
		}
		Vector2 value = timeFunctionVector.GetValue(time);
		Scale = StartupScale * value;
	}

	private void UpdateSpeedTimeFunction(float time)
	{
		_speedFunctionIndex = Utils.Clamp(_speedFunctionIndex, 0, Data.TimeFunctionVelocityList.Count - 1);
		TimeFunctionVector2 timeFunctionVector = Data.TimeFunctionVelocityList[_speedFunctionIndex];
		if (_speedFunctionIndex == 0 && time < timeFunctionVector.StartTime)
		{
			Vector2 vector = timeFunctionVector.StartValue - StartupVelocity;
			Velocity = StartupVelocity + vector * (time / timeFunctionVector.StartTime);
			return;
		}
		if (time >= timeFunctionVector.StartTime + timeFunctionVector.Duration)
		{
			if (_speedFunctionIndex < Data.TimeFunctionVelocityList.Count - 1)
			{
				_speedFunctionIndex++;
				timeFunctionVector = Data.TimeFunctionVelocityList[_speedFunctionIndex];
			}
			else
			{
				time = timeFunctionVector.StartTime + timeFunctionVector.Duration;
			}
		}
		Vector2 value = timeFunctionVector.GetValue(time);
		if (StartupVelocityRotation != 0f)
		{
			Velocity = Utils.RotateVector2(Utils.RotateVector2(StartupVelocity, 0f - StartupVelocityRotation) * value, StartupVelocityRotation);
		}
		else
		{
			Velocity = StartupVelocity * value;
		}
	}

	private void UpdateAlphaTimeFunction(float time)
	{
		float num = -1f;
		_alphaFunctionIndex = Utils.Clamp(_alphaFunctionIndex, 0, Data.TimeFunctionAlphaList.Count - 1);
		TimeFunctionFloat timeFunctionFloat = Data.TimeFunctionAlphaList[_alphaFunctionIndex];
		if (_alphaFunctionIndex == 0 && time < timeFunctionFloat.StartTime)
		{
			float num2 = timeFunctionFloat.StartValue - (float)(int)StartupColor.A;
			num = (float)(int)StartupColor.A + num2 * (time / timeFunctionFloat.StartTime);
		}
		else
		{
			if (time >= timeFunctionFloat.StartTime + timeFunctionFloat.Duration)
			{
				if (_alphaFunctionIndex < Data.TimeFunctionAlphaList.Count - 1)
				{
					_alphaFunctionIndex++;
					timeFunctionFloat = Data.TimeFunctionAlphaList[_alphaFunctionIndex];
				}
				else
				{
					time = timeFunctionFloat.StartTime + timeFunctionFloat.Duration;
				}
			}
			num = MathHelper.Min(255f, (float)(int)StartupColor.A * timeFunctionFloat.GetValue(time));
		}
		if (num >= 0f)
		{
			Alpha = (byte)num;
			if (_spriteRenderer != null)
			{
				_spriteRenderer.Color = Color;
			}
		}
	}

	public Color GetZeroTimeColor()
	{
		Color result = ((Data.TimeFunctionColorList != null && Data.TimeFunctionColorList.Count != 0 && !(Data.TimeFunctionColorList[0].StartTime > 0f)) ? Data.TimeFunctionColorList[0].GetValue(0f) : StartupColor);
		result.A = StartupColor.A;
		if (Data.TimeFunctionAlphaList != null && Data.TimeFunctionAlphaList.Count > 0 && Data.TimeFunctionAlphaList[0].StartTime == 0f)
		{
			result.A = (byte)Utils.Clamp((float)(int)result.A * Data.TimeFunctionAlphaList[0].GetValue(0f), 0f, 255f);
		}
		return result;
	}

	public Vector2 GetZeroTimeScale()
	{
		if (Data.TimeFunctionScaleList == null || Data.TimeFunctionScaleList.Count == 0 || Data.TimeFunctionScaleList[0].StartTime > 0f)
		{
			return StartupScale;
		}
		return StartupScale * Data.TimeFunctionScaleList[0].GetValue(0f);
	}

	private void UpdateColorTimeFunction(float time)
	{
		_colorFunctionIndex = Utils.Clamp(_colorFunctionIndex, 0, Data.TimeFunctionColorList.Count - 1);
		TimeFunctionColor timeFunctionColor = Data.TimeFunctionColorList[_colorFunctionIndex];
		if (_colorFunctionIndex == 0 && time < timeFunctionColor.StartTime)
		{
			float num = timeFunctionColor.StartR - StartupColor.R;
			float num2 = timeFunctionColor.StartG - StartupColor.G;
			float num3 = timeFunctionColor.StartB - StartupColor.B;
			float num4 = time / timeFunctionColor.StartTime;
			SetColorRGB((byte)((float)(int)StartupColor.R + num * num4), (byte)((float)(int)StartupColor.G + num2 * num4), (byte)((float)(int)StartupColor.B + num3 * num4));
			if (_spriteRenderer != null)
			{
				_spriteRenderer.Color = Color;
			}
			return;
		}
		if (time >= timeFunctionColor.StartTime + timeFunctionColor.Duration)
		{
			if (_colorFunctionIndex < Data.TimeFunctionColorList.Count - 1)
			{
				_colorFunctionIndex++;
				timeFunctionColor = Data.TimeFunctionColorList[_colorFunctionIndex];
			}
			else
			{
				time = timeFunctionColor.StartTime + timeFunctionColor.Duration;
			}
		}
		Color value = timeFunctionColor.GetValue(time);
		SetColorRGB(value.R, value.G, value.B);
		if (_spriteRenderer != null)
		{
			_spriteRenderer.Color = Color;
		}
	}

	public void Destroy()
	{
		IsDestroyed = true;
	}

	public float GetLifePercentage()
	{
		return _timer / LifeTime;
	}

	public void CompensateScrolling(float elapsedTime)
	{
		if (MyEmitter != null && ParentTransformation == null && !GameCore.IsEditorEnabled)
		{
			if (Data.CompensateLevelScrollingMode == CompensateScrollingModeEnum.ParentThrowVelocity)
			{
				Position += MyEmitter.MainPart.ParentThrowVelocity * elapsedTime;
			}
			else if (Data.CompensateLevelScrollingMode == CompensateScrollingModeEnum.Full || (Data.CompensateLevelScrollingMode == CompensateScrollingModeEnum.ByLevel && MyEmitter.MyLevel.AlwaysCompensateScrolling) || (Data.CompensateLevelScrollingMode == CompensateScrollingModeEnum.Normal && !MyEmitter.IsDestroyed))
			{
				Position += MyEmitter.MyLevel.ScrollSpeed * elapsedTime;
			}
		}
	}

	public virtual void Update(float elapsedTime)
	{
		if ((double)LifeTime < 99.999)
		{
			_timer += elapsedTime;
			if (_timer > LifeTime)
			{
				_timer = LifeTime;
				Destroy();
			}
		}
		if (MyEmitter == null || MyEmitter.ParentObject == null || MyEmitter.ParentObject.IsDestroyed)
		{
			switch (Data.DestroyMode)
			{
			case ParticleDestroyModeEnum.DestroyWithParent:
				Destroy();
				return;
			case ParticleDestroyModeEnum.StopWithParent:
				Utils.ChangeValueToTarget(ref Velocity, Vector2.Zero, Velocity.Length() * 10f * elapsedTime);
				Utils.ChangeValueToTarget(ref StartupVelocity, Vector2.Zero, StartupVelocity.Length() * 10f * elapsedTime);
				break;
			}
		}
		if (MyEmitter != null && CreateTime == MyEmitter.MyLevel.Timer)
		{
			elapsedTime = 0f;
		}
		ResetPrecalculations();
		if (Data.RotationMode == RotationMode.ByVelocityContinual)
		{
			Rotation = Utils.GetRotationFromZero(Velocity) + StartupRotation;
		}
		else
		{
			Rotation += RotationSpeed * elapsedTime;
		}
		UpdateTimeFunctions();
		Position += Velocity * elapsedTime;
		CompensateScrolling(elapsedTime);
		if (ParentTransformation != null && ParentTransformation.WorldRotation != 0f)
		{
			ForceVelocity += Utils.RotateVector2(Force, 0f - ParentTransformation.WorldRotation) * elapsedTime;
		}
		else
		{
			ForceVelocity += Force * elapsedTime;
		}
		Position += ForceVelocity * elapsedTime;
		if (Data.DestroyOnLeftEdge && _spriteRenderer != null && GameCore.CurrentLevel.Camera.CurrentLayerPosToScreen(new Vector2(Position.X + ((float)_spriteRenderer.FrameRectangle.Width - _spriteRenderer.FramePivot.X) * Scale.X, Position.Y)).X < GameCore.CurrentLevel.Camera.ViewTopLeftOnScreen.X)
		{
			Destroy();
		}
		else if (_spriteRenderer != null)
		{
			_spriteRenderer.Update(elapsedTime);
		}
	}

	public virtual void Draw(SpriteBatch spriteBatch)
	{
		if (_spriteRenderer != null && _spriteRenderer.Atlas != null)
		{
			_spriteRenderer.Atlas.DrawFrameFlipable(spriteBatch, Frame, Color, WorldTransformation, _spriteRenderer.Texture, BlendMode, SpriteEffects);
			DebugInfo.CurrentLayer.Particles++;
		}
		else if (Text != null)
		{
			if (_textSize == Vector2.Zero)
			{
				_textSize = GameCore.FontNoBorder.MeasureFont(Text);
			}
			GameCore.FontNoBorder.Write(spriteBatch, Text, Position.X, Position.Y - _textSize.Y * Scale.Y / 2f, Scale, Color, TextAlign.Center);
		}
	}

	public static void DrawParticleList(Layer layer, List<Particle> particleList)
	{
		ParticleListRenderer.Draw(layer, particleList);
	}
}
