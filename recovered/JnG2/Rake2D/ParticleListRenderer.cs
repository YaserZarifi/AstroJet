using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Debug;

namespace Rake2D;

internal class ParticleListRenderer
{
	private class ParticleBucket
	{
		private Particle[] _particles;

		internal float _lightFactor;

		private int _count;

		private int _maxCount;

		internal bool AddParticle(Particle p)
		{
			if (p.Data.LightFactor != _lightFactor)
			{
				return false;
			}
			_particles[_count++] = p;
			return true;
		}

		internal void Initialize(List<Particle> particleList, Particle p)
		{
			if (_particles == null || _particles.Length < particleList.Count)
			{
				_particles = new Particle[particleList.Count];
				_maxCount = 0;
			}
			else
			{
				_maxCount = Math.Max(_maxCount, _count);
			}
			_particles[0] = p;
			_lightFactor = p.Data.LightFactor;
			_count = 1;
		}

		internal void Draw(Layer layer, SpriteBatch additiveBatch, SpriteBatch normalBatch)
		{
			bool flag = false;
			for (int i = 0; i < _count; i++)
			{
				Particle particle = _particles[i];
				if (particle.IsAdditive)
				{
					particle.Draw(additiveBatch);
					continue;
				}
				if (!flag)
				{
					GameCore.EffectParticle.SetLightFactor(layer, _lightFactor);
					if (_lightFactor == 0f)
					{
						normalBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, GameCore.RasterizerState, null, layer.MyLevel.Camera.WorldMatrixCurrentLayer);
					}
					else
					{
						normalBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, GameCore.RasterizerState, GameCore.EffectParticle.MyEffect, layer.MyLevel.Camera.WorldMatrixCurrentLayer);
					}
					flag = true;
				}
				particle.Draw(normalBatch);
			}
			if (flag)
			{
				normalBatch.End();
			}
		}

		internal void Clear()
		{
			if (_particles != null)
			{
				_maxCount = Math.Max(_maxCount, _count);
				Array.Clear(_particles, 0, _maxCount);
			}
		}
	}

	private SpriteBatch _spriteBatchNormal = new SpriteBatch(GameCore.GraphicsDeviceStatic);

	private SpriteBatch _spriteBatchAdditive = new SpriteBatch(GameCore.GraphicsDeviceStatic);

	private ParticleBucket[] _buckets;

	private int _bucketsUsed;

	private float _lastFrame;

	internal ParticleListRenderer()
	{
		Allocate(16);
	}

	private void Allocate(int bucketCount)
	{
		_buckets = new ParticleBucket[bucketCount];
		for (int i = 0; i < bucketCount; i++)
		{
			_buckets[i] = new ParticleBucket();
		}
	}

	private bool SplitIntoBuckets(List<Particle> particleList)
	{
		_bucketsUsed = 0;
		foreach (Particle particle in particleList)
		{
			int i;
			for (i = 0; i < _bucketsUsed && !_buckets[i].AddParticle(particle); i++)
			{
			}
			if (i != _bucketsUsed)
			{
				continue;
			}
			if (_bucketsUsed >= _buckets.Length)
			{
				return false;
			}
			ParticleBucket particleBucket = _buckets[_bucketsUsed];
			particleBucket.Initialize(particleList, particle);
			for (i = 0; i < _bucketsUsed; i++)
			{
				if (particleBucket._lightFactor < _buckets[i]._lightFactor)
				{
					Array.Copy(_buckets, i, _buckets, i + 1, _bucketsUsed - i);
					_buckets[i] = particleBucket;
					break;
				}
			}
			_bucketsUsed++;
			DebugInfo.CurrentLayer.AddParticleFactor(particle.Data.LightFactor);
		}
		return true;
	}

	internal void Draw(Layer layer, List<Particle> particleList)
	{
		if (particleList != null && particleList.Count != 0)
		{
			if (_lastFrame != GameCore.FrameTime)
			{
				_lastFrame = GameCore.FrameTime;
				Clear();
			}
			while (!SplitIntoBuckets(particleList))
			{
				Allocate(_buckets.Length * 2);
			}
			DebugInfo.CurrentLayer.ListParticleBegins += _bucketsUsed;
			GameCore.EffectParticle.SetShadowFactor(layer.ShadowMapFactor);
			GameCore.EffectParticle.SetHitFactor(0f);
			_spriteBatchAdditive.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, GameCore.RasterizerState, null, layer.MyLevel.Camera.WorldMatrixCurrentLayer);
			for (int i = 0; i < _bucketsUsed; i++)
			{
				_buckets[i].Draw(layer, _spriteBatchAdditive, _spriteBatchNormal);
			}
			_spriteBatchAdditive.End();
		}
	}

	internal void Clear()
	{
		ParticleBucket[] buckets = _buckets;
		for (int i = 0; i < buckets.Length; i++)
		{
			buckets[i].Clear();
		}
	}
}
