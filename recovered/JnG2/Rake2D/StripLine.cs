using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Debug;

namespace Rake2D;

public class StripLine : Transformation
{
	public SpriteAnimator Animator;

	public List<Particle> ParticleList;

	public float TextureFlowSpeed1;

	public float TextureFlowSpeed2;

	public bool IsTextureCoordYFixed;

	public bool LastZeroAlpha;

	public BlendModeEnum BlendMode1;

	public BlendModeEnum BlendMode2;

	public Layer MyLayer;

	public float LightFactor;

	private float _textureFlowOffset1;

	private float _textureFlowOffset2;

	private Vector2 _frameUVTopLeft;

	private Vector2 _frameUVBottomRight;

	private bool _useFrame2;

	private Vector2 _frameUVTopLeft2;

	private Vector2 _frameUVBottomRight2;

	private Vector2 FrameSize;

	private int _addedParticles;

	private static int _maxStripLineParticles;

	private int _frame;

	public bool IsDestroyed { get; private set; }

	public SpriteAtlas Atlas { get; private set; }

	public Texture2D Texture
	{
		get
		{
			if (Atlas != null)
			{
				return Atlas.Texture;
			}
			return null;
		}
	}

	public bool HasParticles => !ParticleList.IsNullOrEmpty();

	public int Frame
	{
		get
		{
			if (Animator != null)
			{
				return Animator.AtlasFrame;
			}
			return _frame;
		}
		set
		{
			_frame = value;
		}
	}

	public StripLine(Layer layer)
	{
		_maxStripLineParticles = 256;
		BlendMode1 = BlendModeEnum.Normal;
		MyLayer = layer;
	}

	internal void ClearStripLine()
	{
		ClearTransformation(isInit: false);
		Animator = null;
		ParticleList = GameCore.Allocator.DeleteOwningParticleList(ParticleList);
		TextureFlowSpeed1 = 0f;
		TextureFlowSpeed2 = 0f;
		IsDestroyed = false;
		IsTextureCoordYFixed = false;
		LastZeroAlpha = false;
		BlendMode1 = BlendModeEnum.Normal;
		BlendMode2 = BlendModeEnum.Normal;
		MyLayer = null;
		LightFactor = 0f;
		_textureFlowOffset1 = 0f;
		_textureFlowOffset2 = 0f;
		Atlas = null;
		_frameUVTopLeft = Vector2.Zero;
		_frameUVBottomRight = Vector2.Zero;
		_useFrame2 = false;
		_frameUVTopLeft2 = Vector2.Zero;
		_frameUVBottomRight2 = Vector2.Zero;
		FrameSize = Vector2.Zero;
		_addedParticles = 0;
	}

	public void SetAtlas(SpriteAtlas atlas, int frame)
	{
		Atlas = atlas;
		if (Atlas != null)
		{
			Atlas.GetFrameUV(frame, out _frameUVTopLeft, out _frameUVBottomRight);
			Rectangle frameRectangle = Atlas.GetFrameRectangle(frame);
			FrameSize = new Vector2(frameRectangle.Width, frameRectangle.Height);
		}
		else
		{
			_frameUVTopLeft = Vector2.Zero;
			_frameUVBottomRight = Vector2.One;
			_useFrame2 = false;
			FrameSize = Vector2.One;
		}
	}

	public void SetFrame2(int frame2, BlendModeEnum blendMode)
	{
		if (Atlas != null && frame2 >= 0)
		{
			Atlas.GetFrameUV(frame2, out _frameUVTopLeft2, out _frameUVBottomRight2);
			if (_frameUVBottomRight2 != Vector2.Zero)
			{
				_useFrame2 = true;
				BlendMode2 = blendMode;
			}
			else
			{
				_useFrame2 = false;
			}
		}
		else
		{
			_useFrame2 = false;
		}
	}

	public void UpdateParticleAnimation()
	{
		if (Atlas != null && !ParticleList.IsNullOrEmpty())
		{
			int frame = ParticleList[0].Frame;
			Atlas.GetFrameUV(frame, out _frameUVTopLeft, out _frameUVBottomRight);
			Rectangle frameRectangle = Atlas.GetFrameRectangle(frame);
			FrameSize = new Vector2(frameRectangle.Width, frameRectangle.Height);
		}
	}

	internal float GetTextureCoordXByLastParticle(ref Vector2 currPosition, float scaleX)
	{
		if (Texture != null && ParticleList.Count > 0)
		{
			Particle particle = ParticleList.Last();
			return (currPosition - particle.WorldPosition).Length() / (float)Texture.Width;
		}
		return 0f;
	}

	internal float GetTextureCoordYByParticle(Particle particle, Vector2 currPosition)
	{
		if (Texture != null)
		{
			return (currPosition - particle.WorldPosition).Length() / FrameSize.Y;
		}
		return 0f;
	}

	internal void AddParticle(Particle particle, SortModeEnum sortMode)
	{
		if (ParticleList == null)
		{
			ParticleList = GameCore.Allocator.GetParticleList();
		}
		if (sortMode == SortModeEnum.AsFirst)
		{
			ParticleList.Insert(0, particle);
		}
		else
		{
			ParticleList.Add(particle);
		}
		_addedParticles++;
	}

	public void Update(float elapsedTime)
	{
		ResetPrecalculations();
		UpdateParticleAnimation();
		if (!HasParticles)
		{
			return;
		}
		for (int num = ParticleList.Count - _maxStripLineParticles; num >= 0; num--)
		{
			ParticleList[num].Destroy();
			GameCore.Allocator.DeleteParticle(ParticleList[num]);
			ParticleList.RemoveAt(num);
		}
		for (int num2 = ParticleList.Count - 1; num2 >= 0; num2--)
		{
			Particle particle = ParticleList[num2];
			if (particle.IsDestroyed)
			{
				particle.CompensateScrolling(elapsedTime);
			}
			else
			{
				particle.Update(elapsedTime);
			}
			if (particle.IsDestroyed && (num2 == ParticleList.Count - 1 || ParticleList[num2 + 1].IsDestroyed))
			{
				GameCore.Allocator.DeleteParticle(ParticleList[num2]);
				ParticleList.RemoveAt(num2);
			}
		}
		DebugInfo.StripLineParticles += ParticleList.Count;
		_textureFlowOffset1 -= TextureFlowSpeed1 * elapsedTime;
		_textureFlowOffset2 -= TextureFlowSpeed2 * elapsedTime;
	}

	public void Draw(TriangleBatchTextured triangleBatch)
	{
		if (LightFactor != GameCore.EffectLightBasic.LightFactor && BlendMode1 != BlendModeEnum.Additive)
		{
			GameCore.TriangleBatchTextured.End();
			GameCore.TriangleBatchTextured.Begin(BlendState.AlphaBlend, MyLayer.MyLevel.Camera.WorldMatrixCurrentLayer, SamplerState.LinearWrap, LightFactor);
		}
		if (HasParticles)
		{
			DrawByParticles(triangleBatch);
		}
	}

	public void DrawByParticles(TriangleBatchTextured triangleBatch)
	{
		int num = ParticleList.Count;
		Vector2 vector = Vector2.Zero;
		Particle particle = ((num > 0) ? ParticleList[num - 1] : null);
		if (ParentTransformation != null && num > 0)
		{
			num++;
			if (particle.Data.OffsetShape == ParticleEmitterShapeEnum.Rectangle)
			{
				Vector2 vector2 = particle.Data.OffsetRange.GetAverage();
				if (particle.MyEmitter.Data.TransformationMode == TransformationMode.ParentStart)
				{
					vector2 = Vector2.Transform(vector2, Matrix.CreateRotationZ(particle.MyEmitter.ParentObject.WorldRotation));
				}
				vector = WorldPosition + vector2;
			}
			else
			{
				vector = WorldPosition;
			}
		}
		if (num < 2)
		{
			return;
		}
		float num2 = 1f;
		float num3 = 1f;
		float num4 = 0f;
		float num5 = 0f;
		BatchVertextTextured[] batchVertextTextured = GameCore.Allocator.GetBatchVertextTextured(num * 2);
		int num6 = 0;
		for (int i = 0; i < num; i++)
		{
			Vector2 vector3;
			Color color;
			Vector2 vector4;
			if (i == ParticleList.Count)
			{
				vector3 = vector;
				if (particle.WorldPosition == vector3)
				{
					continue;
				}
				Vector2 zeroTimeScale = particle.GetZeroTimeScale();
				num3 = zeroTimeScale.X / 2f * FrameSize.X;
				num2 = zeroTimeScale.Y;
				color = particle.GetZeroTimeColor();
				float num7 = GetTextureCoordYByParticle(particle, vector3) / num2;
				num4 -= num7;
				num5 -= num7;
				vector4 = particle.WorldPosition - vector3;
			}
			else
			{
				Particle particle2 = ParticleList[i];
				vector3 = particle2.WorldPosition;
				num3 = particle2.Scale.X / 2f * FrameSize.X;
				color = particle2.SpriteColor;
				if (i == 0)
				{
					num4 = particle2.StripTextureCoordY + _textureFlowOffset1;
					num5 = particle2.StripTextureCoordY2 + _textureFlowOffset2;
				}
				else
				{
					num2 = ParticleList[i - 1].Scale.Y;
					if (IsTextureCoordYFixed)
					{
						num4 -= num2;
						num5 -= num2;
					}
					else
					{
						float num8 = GetTextureCoordYByParticle(ParticleList[i - 1], vector3) / num2;
						num4 -= num8;
						num5 -= num8;
					}
					particle2.StripTextureCoordY = num4 - _textureFlowOffset1;
					particle2.StripTextureCoordY2 = num5 - _textureFlowOffset2;
				}
				Vector2 vector5;
				if (i != ParticleList.Count - 1)
				{
					vector5 = ParticleList[i + 1].WorldPosition;
				}
				else
				{
					vector5 = ((ParentTransformation == null) ? vector3 : WorldPosition);
				}
				if (i > 0)
				{
					Vector2 vector6 = ParticleList[i - 1].WorldPosition - vector3;
					if (vector6 != Vector2.Zero)
					{
						vector6.Normalize();
					}
					Vector2 vector7 = vector3 - vector5;
					if (vector7 != Vector2.Zero)
					{
						vector7.Normalize();
					}
					vector4 = vector7 + vector6;
				}
				else
				{
					vector4 = vector3 - vector5;
					if (LastZeroAlpha)
					{
						color = Color.TransparentBlack;
					}
				}
			}
			if (vector4 != Vector2.Zero)
			{
				vector4.Normalize();
				batchVertextTextured[num6].OriginalColor = color;
				batchVertextTextured[num6].Color = Utils.GetColorByBlendMode(color, BlendMode1);
				batchVertextTextured[num6].Position = vector3 + new Vector2(0f - vector4.Y, vector4.X) * num3;
				batchVertextTextured[num6].TextureCoordinate.X = _frameUVTopLeft.X;
				batchVertextTextured[num6].TextureCoordinate.Y = num4;
				batchVertextTextured[num6].TextureCoordinateY2 = num5;
				num6++;
				batchVertextTextured[num6].OriginalColor = color;
				batchVertextTextured[num6].Color = batchVertextTextured[num6 - 1].Color;
				batchVertextTextured[num6].Position = vector3 + new Vector2(vector4.Y, 0f - vector4.X) * num3;
				batchVertextTextured[num6].TextureCoordinate.X = _frameUVBottomRight.X;
				batchVertextTextured[num6].TextureCoordinate.Y = num4;
				batchVertextTextured[num6].TextureCoordinateY2 = num5;
				num6++;
			}
		}
		triangleBatch.DrawStrip(batchVertextTextured, num6, Texture);
		if (_useFrame2)
		{
			if (BlendMode1 != BlendMode2)
			{
				for (int num9 = num6 - 1; num9 >= 0; num9--)
				{
					batchVertextTextured[num9].Color = Utils.GetColorByBlendMode(batchVertextTextured[num9].OriginalColor, BlendMode2);
				}
			}
			for (int num10 = num6 - 1; num10 >= 0; num10--)
			{
				if (num10 % 2 == 0)
				{
					batchVertextTextured[num10].TextureCoordinate.X = _frameUVTopLeft2.X;
				}
				else
				{
					batchVertextTextured[num10].TextureCoordinate.X = _frameUVBottomRight2.X;
				}
				batchVertextTextured[num10].TextureCoordinate.Y = batchVertextTextured[num10].TextureCoordinateY2;
			}
			triangleBatch.DrawStrip(batchVertextTextured, num6, Texture);
		}
		GameCore.Allocator.DeleteBatchVertextTextured(batchVertextTextured);
	}

	public void Destroy()
	{
		ParentTransformation = null;
		IsDestroyed = true;
	}
}
