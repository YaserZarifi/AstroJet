using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D;

public class ParticleEmitter
{
	public ParticleEmitterData Data;

	public ParticleSpace Space;

	public StripLine StripLine;

	public float BeginTimer;

	public bool IsEnabled = true;

	public bool IsDestroyed;

	public bool IsPrewarmed;

	public bool IsEmitedByBehavior;

	private BlendModeEnum _alternateBlendMode = BlendModeEnum.Additive;

	private float _timer;

	private int _loopCount = int.MaxValue;

	private List<Particle> _prewarmedParticleList;

	public GameObject ParentObject { get; private set; }

	public GameObject MainPart { get; private set; }

	public GameLevel MyLevel
	{
		get
		{
			if (ParentObject != null && ParentObject.MyLayer != null)
			{
				return ParentObject.MyLayer.MyLevel;
			}
			return GameCore.CurrentLevel;
		}
	}

	public Layer MyLayer
	{
		get
		{
			if (ParentObject != null && ParentObject.MyLayer != null)
			{
				return ParentObject.MyLayer;
			}
			return MyLevel.CurrentLayer;
		}
	}

	public bool IsNeverending => _loopCount >= 9999;

	public void SetParticleBlendMode(Particle particle)
	{
		if (Data.BlendMode == BlendModeEnum.Random)
		{
			particle.BlendMode = ((Utils.GetRandom(0, 1) != 0) ? BlendModeEnum.Additive : BlendModeEnum.Normal);
		}
		else if (Data.BlendMode == BlendModeEnum.Alternate)
		{
			particle.BlendMode = ((_alternateBlendMode == BlendModeEnum.Normal) ? BlendModeEnum.Additive : BlendModeEnum.Normal);
			_alternateBlendMode = particle.BlendMode;
		}
		else
		{
			particle.BlendMode = Data.BlendMode;
		}
	}

	public ParticleEmitter(GameObject parentObject, ParticleEmitterData emitterData)
	{
		Data = emitterData;
		Space = Data.Space;
		ParentObject = parentObject;
		MainPart = ParentObject.GetMainPart();
		BeginTimer = Data.BeginDelayRange.GetRandom();
		Reset();
	}

	public void Reset()
	{
		_timer = 0f;
		_loopCount = Data.LoopCountRange.GetRandom();
		if (StripLine != null)
		{
			StripLine.Destroy();
			StripLine = null;
		}
	}

	public void Prewarm()
	{
		_prewarmedParticleList = GameCore.Allocator.GetParticleList();
		for (int i = 0; i < Data.PrewarmRepeatCount; i++)
		{
			float prewarmTime = Data.PrewarmTime;
			Update(prewarmTime);
			foreach (Particle prewarmedParticle in _prewarmedParticleList)
			{
				prewarmedParticle.CreateTime = -1f;
				prewarmedParticle.Update(prewarmTime);
			}
		}
		GameCore.Allocator.DeleteAliasingParticleList(_prewarmedParticleList);
		_prewarmedParticleList = null;
		IsPrewarmed = true;
	}

	public void EnableStripLine(ParticleSpace space, BlendModeEnum blendType)
	{
		if (StripLine != null)
		{
			StripLine.Destroy();
		}
		StripLine = GameCore.Allocator.GetStripLine(MyLayer);
		int frame = 0;
		if (!Data.FrameArray.IsNullOrEmpty())
		{
			frame = Data.FrameArray[0];
		}
		StripLine.SetAtlas(Data.Atlas, frame);
		StripLine.BlendMode1 = blendType;
		StripLine.ParentTransformation = ParentObject;
		StripLine.TextureFlowSpeed1 = Data.StripTextureFlowSpeed1;
		if (Data.StripFrame2 >= 0)
		{
			StripLine.TextureFlowSpeed2 = Data.StripTextureFlowSpeed2;
		}
		StripLine.LightFactor = Data.LightFactor;
		StripLine.LastZeroAlpha = Data.StripLastZeroAlpha;
		switch (space)
		{
		case ParticleSpace.LastParticleLayerFront:
			MyLevel.LastParticleLayer.AddStripLine(StripLine, space);
			return;
		case ParticleSpace.FirstMainLayerBack:
			MyLevel.FirstMainLayer.AddStripLine(StripLine, space);
			return;
		case ParticleSpace.MyLayerBack:
		case ParticleSpace.MyLayerFront:
			MyLayer.AddStripLine(StripLine, space);
			return;
		case ParticleSpace.LevelFront:
			MyLevel.FrontParticleLayer.AddStripLine(StripLine, space);
			return;
		case ParticleSpace.MainPartBack:
		case ParticleSpace.MainPartFront:
			MainPart.AddStripLine(StripLine, space);
			return;
		}
		if (ParentObject.ParentObject != null && (space == ParticleSpace.ParentPartFront || space == ParticleSpace.ParentPartBack))
		{
			ParentObject.ParentObject.AddStripLine(StripLine, space);
		}
		else if (ParentObject != null)
		{
			ParentObject.AddStripLine(StripLine, space);
		}
	}

	public Particle Emit(bool bIgnoreLoopCount = false)
	{
		Particle particle = null;
		if (Space == ParticleSpace.Distortion && !MyLayer.UseDistortion)
		{
			return null;
		}
		if (!IsNeverending && !bIgnoreLoopCount)
		{
			_loopCount--;
			if (_loopCount < 0)
			{
				return null;
			}
		}
		if (((Data.Environment & ParentObject.Environment) == 0 && (!ParentObject.IgnoreWater || ParentObject.Environment != EnvironmentEnum.Water)) || MyLayer == null)
		{
			return null;
		}
		int random = Data.CountRange.GetRandom();
		float num;
		Vector2 vector;
		if (random > 1)
		{
			num = (Data.OffsetRange.FromY - Data.OffsetRange.FromX) / (float)(random - 1);
			vector = new Vector2((Data.OffsetRange.ToX - Data.OffsetRange.FromX) / (float)(random - 1), (Data.OffsetRange.ToY - Data.OffsetRange.FromY) / (float)(random - 1));
		}
		else
		{
			num = 0f;
			vector = Vector2.Zero;
		}
		Vector2 position = default;
		for (int i = 0; i < random; i++)
		{
			if (Data.OffsetShape == ParticleEmitterShapeEnum.Rectangle)
			{
				switch (Data.SpreadModeX)
				{
				case 1:
					position.X = Data.OffsetRange.FromX + (float)i * vector.X;
					break;
				case 2:
					if (i == 0)
					{
						position.X = Data.OffsetRange.FromX + Utils.GetRandom(0f, vector.X / 2f);
					}
					else if (i == random - 1)
					{
						position.X = Data.OffsetRange.FromX + (float)i * vector.X + Utils.GetRandom((0f - vector.X) / 2f, 0f);
					}
					else
					{
						position.X = Data.OffsetRange.FromX + (float)i * vector.X + Utils.GetRandom((0f - vector.X) / 2f, vector.X / 2f);
					}
					break;
				default:
					position.X = Utils.GetRandom(Data.OffsetRange.FromX, Data.OffsetRange.ToX);
					break;
				}
				switch (Data.SpreadModeY)
				{
				case 1:
					position.Y = Data.OffsetRange.FromY + (float)i * vector.Y;
					break;
				case 2:
					if (i == 0)
					{
						position.Y = Data.OffsetRange.FromY + Utils.GetRandom(0f, vector.Y / 2f);
					}
					else if (i == random - 1)
					{
						position.Y = Data.OffsetRange.FromY + (float)i * vector.Y + Utils.GetRandom((0f - vector.Y) / 2f, 0f);
					}
					else
					{
						position.Y = Data.OffsetRange.FromY + (float)i * vector.Y + Utils.GetRandom((0f - vector.Y) / 2f, vector.Y / 2f);
					}
					break;
				default:
					position.Y = Utils.GetRandom(Data.OffsetRange.FromY, Data.OffsetRange.ToY);
					break;
				}
				if (Data.UseParentScale)
				{
					position *= ParentObject.WorldScale;
				}
			}
			else
			{
				position = Utils.GetVelocityVector2(rotation: Data.SpreadModeX switch
				{
					1 => Utils.DegToRad(Data.OffsetRange.FromX + (float)i * num), 
					2 => (i != 0) ? ((i != random - 1) ? Utils.DegToRad(Data.OffsetRange.FromX + (float)i * num + Utils.GetRandom((0f - num) / 2f, num / 2f)) : Utils.DegToRad(Data.OffsetRange.FromX + (float)i * num + Utils.GetRandom((0f - num) / 2f, 0f))) : Utils.DegToRad(Data.OffsetRange.FromX + Utils.GetRandom(0f, num / 2f)), 
					_ => Utils.DegToRad(Utils.GetRandom(Data.OffsetRange.FromX, Data.OffsetRange.FromY)), 
				}, speed: Utils.GetRandom(Data.OffsetRange.ToX, Data.OffsetRange.ToY));
			}
			particle = GameCore.Allocator.GetParticle();
			particle.Init(Data, position);
			particle.MyEmitter = this;
			particle.CreateTime = MyLevel.Timer;
			SetParticleBlendMode(particle);
			particle.Force = Data.ForceRange.GetRandom();
			particle.Force *= ParentObject.WorldScale;
			if (Data.ColorMode == ColorMode.ParentRGBA && ParentObject != null)
			{
				particle.StartupColor = ParentObject.Color;
				particle.StartupColor.A = (byte)((float)Data.StartAlphaRange.GetRandom() * ((float)(int)ParentObject.Alpha / 255f));
			}
			else if (Data.ColorMode == ColorMode.ParentAlpha && ParentObject != null)
			{
				particle.StartupColor = new Color(255, 255, 255, (int)((float)(Data.StartAlphaRange.GetRandom() * ParentObject.Alpha) / 255f));
			}
			else if (Data.ColorMode == ColorMode.ParentRGB && ParentObject != null)
			{
				particle.StartupColor = new Color(ParentObject.Color.R, particle.StartupColor.G, particle.StartupColor.B, Data.StartAlphaRange.GetRandom());
			}
			else
			{
				particle.StartupColor = new Color(255, 255, 255, Data.StartAlphaRange.GetRandom());
			}
			if (particle.BlendMode == BlendModeEnum.Additive)
			{
				particle.StartupColor.A = (byte)((float)(int)particle.StartupColor.A * Data.BlendFactor);
			}
			particle.SpriteColor = particle.StartupColor;
			if (Data.FlipVertical == 1 || (Data.FlipVertical == 2 && Utils.GetRandom(1) == 1))
			{
				particle.SpriteEffects |= SpriteEffects.FlipVertically;
			}
			if (Data.FlipHorizontal == 1 || (Data.FlipHorizontal == 2 && Utils.GetRandom(1) == 1))
			{
				particle.SpriteEffects |= SpriteEffects.FlipHorizontally;
			}
			if (Data.TransformationMode == TransformationMode.Parent)
			{
				particle.ParentTransformation = ParentObject;
			}
			else if (Data.TransformationMode == TransformationMode.ParentStart)
			{
				particle.Scale *= ParentObject.WorldScale;
				particle.Position *= ParentObject.WorldScale;
				if (ParentObject.WorldScale.X < 0f)
				{
					particle.Position = ParentObject.WorldPosition + Utils.RotateVector2(particle.Position, ParentObject.WorldRotation * -1f);
				}
				else
				{
					particle.Position = ParentObject.WorldPosition + Utils.RotateVector2(particle.Position, ParentObject.WorldRotation);
				}
				particle.Rotation += ParentObject.WorldRotation;
			}
			else
			{
				particle.Position += ParentObject.WorldPosition;
				if (Data.UseParentScale)
				{
					particle.Scale *= ParentObject.WorldScale;
				}
			}
			particle.StartupScale = particle.Scale;
			if (Data.VelocityMode == VelocityMode.Range || (Data.VelocityMode == VelocityMode.ParentRange && Data.TransformationMode == TransformationMode.Parent))
			{
				particle.Velocity = Data.VelocityRange.GetRandom();
			}
			else if (Data.VelocityMode == VelocityMode.Radial || Data.VelocityMode == VelocityMode.ParentRadial)
			{
				Vector2 vector2 = particle.WorldPosition - ParentObject.WorldPosition;
				if (vector2 != Vector2.Zero)
				{
					vector2.Normalize();
				}
				if (Data.TransformationMode == TransformationMode.Parent)
				{
					vector2 = Vector2.Transform(vector2, Matrix.CreateRotationZ(0f - ParentObject.WorldRotation));
				}
				particle.Velocity = vector2 * Data.VelocityRange.GetRandomX();
				if (Data.VelocityMode == VelocityMode.ParentRadial)
				{
					particle.StartupVelocityRotation = ParentObject.WorldRotation;
				}
			}
			else if (Data.VelocityMode == VelocityMode.ParentRange)
			{
				particle.StartupVelocityRotation = ParentObject.WorldRotation;
				particle.Velocity = Utils.RotateVector2(Data.VelocityRange.GetRandom(), ParentObject.WorldRotation);
			}
			else if (Data.VelocityMode == VelocityMode.ParentVelocityAngle)
			{
				Matrix matrix = Matrix.CreateRotationZ(Utils.GetRotationFromZero(ParentObject.Velocity) - (float)Math.PI);
				particle.Velocity = Vector2.Transform(Data.VelocityRange.GetRandom(), matrix);
			}
			if (Data.TransformationMode != TransformationMode.Parent)
			{
				particle.Velocity *= ParentObject.WorldScale;
			}
			if (Data.ParentVelocityFactor != 0f)
			{
				particle.Velocity += ParentObject.Velocity * Data.ParentVelocityFactor;
			}
			particle.StartupVelocity = particle.Velocity;
			if (Data.RotationMode == RotationMode.ByVelocity)
			{
				particle.Rotation = Utils.GetRotationFromZero(particle.Velocity - ParentObject.Velocity * Data.ParentVelocityFactor) + Data.RotationRange.GetRandom();
			}
			else if (Data.RotationMode == RotationMode.ByParentVelocity)
			{
				particle.Rotation = Utils.GetRotationFromZero(ParentObject.Velocity) + Data.RotationRange.GetRandom();
			}
			else if (Data.RotationMode == RotationMode.ByVelocityContinual)
			{
				particle.StartupRotation = Data.RotationRange.GetRandom();
				particle.Rotation = Utils.GetRotationFromZero(particle.Velocity) + particle.StartupRotation;
			}
			else if (Data.RotationMode == RotationMode.Parent)
			{
				if (Data.TransformationMode == TransformationMode.Parent)
				{
					particle.Rotation = Data.RotationRange.GetRandom();
				}
				else
				{
					particle.Rotation = ParentObject.WorldRotation + Data.RotationRange.GetRandom();
				}
			}
			else
			{
				particle.Rotation = Data.RotationRange.GetRandom();
			}
			particle.RotationSpeed = Data.RotationSpeedRange.GetRandom();
			if (Data.UseVelocityXToRotationDirection && particle.Velocity.X < 0f)
			{
				particle.RotationSpeed *= -1f;
			}
			particle.Update(0f);
			ParticleSpace particleSpace = Space;
			if (particleSpace == ParticleSpace.MyLayerRandom)
			{
				particleSpace = ((Utils.GetRandom(1) != 0) ? ParticleSpace.MyLayerBack : ParticleSpace.MyLayerFront);
			}
			else if (ParentObject.MyLayer.ScrollFactor != Vector2.One && !IsEmitedByBehavior)
			{
				switch (Space)
				{
				case ParticleSpace.FirstMainLayerBack:
					particleSpace = ParticleSpace.MyLayerBack;
					break;
				case ParticleSpace.LastParticleLayerFront:
				case ParticleSpace.LevelFront:
					particleSpace = ParticleSpace.MyLayerFront;
					break;
				case ParticleSpace.Distortion:
					particle.Position += ParentObject.MyCamera.Position * (Vector2.One - ParentObject.MyLayer.ScrollFactor);
					break;
				}
			}
			if (Data.IsStripLine)
			{
				if (StripLine == null)
				{
					EnableStripLine(particleSpace, Data.BlendMode);
					StripLine.SetFrame2(Data.StripFrame2, Data.StripBlendMode2);
				}
				StripLine.AddParticle(particle, Data.SortMode);
				continue;
			}
			switch (particleSpace)
			{
			case ParticleSpace.MyPartBack:
				ParentObject.AddParticle(particle, bAddToBack: true, Data.SortMode);
				break;
			case ParticleSpace.MyPartFront:
				ParentObject.AddParticle(particle, bAddToBack: false, Data.SortMode);
				break;
			case ParticleSpace.MainPartBack:
				MainPart.AddParticle(particle, bAddToBack: true, Data.SortMode);
				break;
			case ParticleSpace.MainPartFront:
				MainPart.AddParticle(particle, bAddToBack: false, Data.SortMode);
				break;
			case ParticleSpace.MyLayerBack:
				AddParticleToLayer(particle, MyLayer, bFront: false);
				break;
			case ParticleSpace.MyLayerFront:
				AddParticleToLayer(particle, MyLayer, bFront: true);
				break;
			case ParticleSpace.ParentPartBack:
				if (ParentObject.IsMainPart)
				{
					ParentObject.AddParticle(particle, bAddToBack: true, Data.SortMode);
				}
				else
				{
					ParentObject.ParentObject.AddParticle(particle, bAddToBack: true, Data.SortMode);
				}
				break;
			case ParticleSpace.ParentPartFront:
				if (ParentObject.IsMainPart)
				{
					ParentObject.AddParticle(particle, bAddToBack: false, Data.SortMode);
				}
				else
				{
					ParentObject.ParentObject.AddParticle(particle, bAddToBack: false, Data.SortMode);
				}
				break;
			case ParticleSpace.LastParticleLayerFront:
				AddParticleToLayer(particle, MyLevel.LastParticleLayer, bFront: true);
				break;
			case ParticleSpace.FirstMainLayerBack:
				AddParticleToLayer(particle, MyLevel.FirstMainLayer, bFront: false);
				break;
			case ParticleSpace.LevelFront:
				AddParticleToLayer(particle, MyLevel.FrontParticleLayer, bFront: true);
				break;
			case ParticleSpace.Distortion:
				if (Options.UseDistortionShader)
				{
					particle.StartupColor.A = (byte)MathHelper.Clamp((float)(int)particle.StartupColor.A * ParentObject.WorldScale.X * MyLevel.Camera.Zoom, 0f, 255f);
					MyLevel.ParticleListDistortion.Add(particle);
				}
				break;
			}
			if (_prewarmedParticleList != null)
			{
				_prewarmedParticleList.Add(particle);
			}
		}
		return particle;
	}

	private void AddParticleToLayer(Particle particle, Layer layer, bool bFront)
	{
		if (layer == null || !layer.IsUpdatable)
		{
			return;
		}
		if (bFront)
		{
			if (Data.SortMode == SortModeEnum.AsFirst || (Data.SortMode == SortModeEnum.Random && Utils.GetRandom(1) == 1))
			{
				layer.ParticleListFront.Insert(0, particle);
			}
			else
			{
				layer.ParticleListFront.Add(particle);
			}
		}
		else if (Data.SortMode == SortModeEnum.AsFirst || (Data.SortMode == SortModeEnum.Random && Utils.GetRandom(1) == 1))
		{
			layer.ParticleListBack.Insert(0, particle);
		}
		else
		{
			layer.ParticleListBack.Add(particle);
		}
	}

	public void Update(float elapsedTime)
	{
		if (!IsEnabled || Options.DebugNoParticles || IsEmitedByBehavior)
		{
			return;
		}
		if (BeginTimer > 0f)
		{
			BeginTimer -= elapsedTime;
			if (BeginTimer > 0f)
			{
				return;
			}
		}
		_timer -= elapsedTime;
		if (_timer <= 0f)
		{
			_timer = Data.DelayRange.GetRandom();
			Emit();
		}
	}

	public void ZeroTimer()
	{
		_timer = 0f;
	}

	public void ResetTimerAndLoop()
	{
		_timer = Data.DelayRange.GetRandom();
		_loopCount = Data.LoopCountRange.GetRandom();
	}

	public void Destroy()
	{
		if (StripLine != null)
		{
			if (!IsEmitedByBehavior)
			{
				Particle particle = Emit(bIgnoreLoopCount: true);
				if (particle != null)
				{
					particle.StartupColor.A = 0;
				}
			}
			StripLine.Destroy();
		}
		IsDestroyed = true;
	}
}
