using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class RetroLine : BehaviorScript
{
	private Particle _topParticle;

	private Particle _bottomParticle;

	private Particle _midParticle;

	public override void Init()
	{
		if (MyObject.Emitter != null)
		{
			MyObject.Emitter.IsEmitedByBehavior = true;
		}
	}

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		MyObject.OnDestroy += OnDestroy;
		MyObject.OnDisable += OnDestroy;
		MyObject.OnDeactivate += OnDestroy;
		if (DoInEditor)
		{
			MyObject.OnEditorUpdate += OnUpdate;
		}
	}

	protected void OnDestroy()
	{
		_topParticle?.Destroy();
		_bottomParticle?.Destroy();
	}

	private void OnUpdate(float elapsedTime)
	{
		float value = (MyObject.VirtualScreenPosition.X - 960f) / 960f;
		MyObject.Alpha = (byte)(255f - 255f * Math.Min(Math.Abs(value), 1f));
		if (MyObject.Emitter != null)
		{
			MyObject.Emitter.IsEmitedByBehavior = true;
			if (MyObject.Emitter.StripLine == null || MyObject.Emitter.StripLine.ParticleList.Count < 2)
			{
				MyObject.Emitter.Reset();
				MyObject.Emitter.Emit(bIgnoreLoopCount: true);
				MyObject.Emitter.StripLine.ParentTransformation = null;
			}
			if (MyObject.Emitter.StripLine.ParticleList.Count > 2)
			{
				_topParticle = MyObject.Emitter.StripLine.ParticleList[0];
				_midParticle = MyObject.Emitter.StripLine.ParticleList[1];
				_bottomParticle = MyObject.Emitter.StripLine.ParticleList[2];
			}
			else
			{
				_topParticle = MyObject.Emitter.StripLine.ParticleList[0];
				_midParticle = null;
				_bottomParticle = MyObject.Emitter.StripLine.ParticleList[1];
			}
			_topParticle.Position = MyObject.WorldPosition;
			_bottomParticle.Position = _topParticle.Position - Utils.GetVelocityVector2(1200f, Utils.GetRotationTo(MyObject.VirtualScreenPosition, new Vector2(960f, 540f)));
			if (_midParticle != null)
			{
				_midParticle.Position = _topParticle.Position - Utils.GetVelocityVector2(300f, Utils.GetRotationTo(MyObject.VirtualScreenPosition, new Vector2(960f, 540f)));
			}
			_bottomParticle.SpriteColor = MyObject.Color;
			if (_midParticle != null)
			{
				_topParticle.SpriteColor = new Color(MyObject.Color, 0);
				_midParticle.SpriteColor = new Color(MyObject.Color, 0.8f);
			}
			else
			{
				_topParticle.SpriteColor = MyObject.Color;
			}
		}
	}
}
