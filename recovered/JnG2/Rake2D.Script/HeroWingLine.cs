using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class HeroWingLine : BehaviorScript
{
	private ParticleEmitter _emitter;

	private bool _isBack;

	public override void RegisterEvents()
	{
		if (MyObject.MyHero != null)
		{
			MyObject.OnUpdate += Update;
			MyObject.OnEditorUpdate += Update;
		}
	}

	public override void Init()
	{
		_emitter = MyObject.Emitter;
		_emitter.IsEmitedByBehavior = true;
		_isBack = ParamBool("is_back");
	}

	private void CreateParticles()
	{
		for (int i = ((_emitter.StripLine != null && _emitter.StripLine.HasParticles) ? _emitter.StripLine.ParticleList.Count : 0); i <= 10; i++)
		{
			Particle particle = _emitter.Emit(bIgnoreLoopCount: true);
			if (particle == null)
			{
				break;
			}
			if (i == 0)
			{
				particle.Alpha = 100;
			}
			else
			{
				particle.Alpha = (byte)(150 - i * 15);
			}
			particle.SpriteColor = new Color(particle.SpriteColor, (float)(int)particle.Alpha / 255f);
			particle.StartupColor = particle.SpriteColor;
		}
		_emitter.StripLine.ParentTransformation = null;
	}

	public void Update(float elapsedTime)
	{
		Vector2 vector = Utils.RotateVector2(MyObject.MyHero.MyObject.Velocity / MyObject.MyHero.SpeedRatio, MyObject.WorldRotation);
		if (vector.Y == 0f)
		{
			if (_emitter.StripLine != null)
			{
				GameCore.Allocator.DeleteOwningParticleList(_emitter.StripLine.ParticleList);
				_emitter.StripLine.ParticleList = GameCore.Allocator.GetParticleList();
			}
			MyObject.IsVisible = false;
			return;
		}
		MyObject.IsVisible = true;
		CreateParticles();
		float num = vector.Y / MyObject.MyHero.MaxSpeed;
		if (num < 0f)
		{
			num *= -1f;
		}
		if (num > 1f)
		{
			num = 1f;
		}
		MyObject.Alpha = (byte)(255f * num);
		int num2 = (_isBack ? 20 : 0);
		for (int i = 0; i < _emitter.StripLine.ParticleList.Count; i++)
		{
			Particle particle = _emitter.StripLine.ParticleList[i];
			if (i == 0)
			{
				particle.Position = MyObject.WorldPosition;
				continue;
			}
			Vector2 vector2 = MyObject.WorldPosition - Utils.RotateVector2(new Vector2((float)(i * 20) * num * MyObject.MyHero.MyObject.Scale.X, Utils.Sin(MyObject.Timer * 15f * num * (float)i + (float)num2) * 1.5f * MyObject.MyHero.MyObject.Scale.X), MyObject.WorldRotation);
			Vector2 currentValue = particle.WorldPosition;
			float distance = Utils.GetDistance(vector2, currentValue);
			Utils.ChangeValueToTarget(ref currentValue, vector2, (MyObject.MyHero.MaxSpeed + distance * (float)(200 - i * 19) - (float)(i * 18)) * elapsedTime);
			particle.Position = currentValue;
		}
	}
}
