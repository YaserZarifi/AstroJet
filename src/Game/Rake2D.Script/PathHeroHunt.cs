using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public class PathHeroHunt : BehaviorScript
{
	private Vector2 _velocity;

	private Vector2 _huntMaxSpeed;

	private Vector2 _huntMaxRange;

	private Vector2 velocityToHero;

	public static string[] PathEvents = new string[4] { "hunt_speed_x", "hunt_speed_y", "hunt_max_x", "hunt_max_y" };

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
	}

	public override void Init()
	{
		_huntMaxSpeed = ParamVector2("hunt_speed");
		_huntMaxRange = ParamVector2("hunt_max_range", 999f, 999f);
	}

	public void Update(float elapsedTime)
	{
		Hero hero = MyObject.FindNearestHero();
		float num = 200f;
		float num2 = _huntMaxSpeed.X * 2f * elapsedTime;
		if (_huntMaxSpeed.X > 0f)
		{
			float num3 = hero.MyObject.WorldPosition.X - MyObject.WorldPosition.X;
			if (num3 < 0f - num)
			{
				velocityToHero.X -= num2;
				if (velocityToHero.X < 0f - _huntMaxSpeed.X)
				{
					velocityToHero.X = 0f - _huntMaxSpeed.X;
				}
			}
			else if (num3 > num)
			{
				velocityToHero.X += _huntMaxSpeed.X * 2f * elapsedTime;
				if (_velocity.X > _huntMaxSpeed.X)
				{
					velocityToHero.X = _huntMaxSpeed.X;
				}
			}
			else if (num3 > 0f)
			{
				velocityToHero.X = _huntMaxSpeed.X * Utils.Sqrt(num3 / num);
			}
			else if (num3 < 0f)
			{
				velocityToHero.X = (0f - _huntMaxSpeed.X) * Utils.Sqrt((0f - num3) / num);
			}
			if (MyObject.PathOscilation.X <= 0f - _huntMaxRange.X + num)
			{
				float num4 = Utils.Sqrt((_huntMaxRange.X + MyObject.PathOscilation.X) / num) * (0f - _huntMaxSpeed.X);
				if (num4 > velocityToHero.X)
				{
					velocityToHero.X = num4;
				}
			}
			else if (MyObject.PathOscilation.X >= _huntMaxRange.X - num)
			{
				float num5 = Utils.Sqrt((_huntMaxRange.X - MyObject.PathOscilation.X) / num) * _huntMaxSpeed.X;
				if (num5 < velocityToHero.X)
				{
					velocityToHero.X = num5;
				}
			}
			_velocity.X = velocityToHero.X;
			MyObject.PathOscilation.X += _velocity.X * elapsedTime;
			if (MyObject.PathOscilation.X <= 0f - _huntMaxRange.X)
			{
				velocityToHero.X = 0f;
				MyObject.PathOscilation.X = 0f - _huntMaxRange.X;
			}
			else if (MyObject.PathOscilation.X >= _huntMaxRange.X)
			{
				velocityToHero.X = 0f;
				MyObject.PathOscilation.X = _huntMaxRange.X;
			}
		}
		if (!(_huntMaxSpeed.Y > 0f))
		{
			return;
		}
		float num6 = hero.MyObject.WorldPosition.Y - MyObject.WorldPosition.Y;
		if (num6 < 0f - num)
		{
			velocityToHero.Y -= num2;
			if (velocityToHero.Y < 0f - _huntMaxSpeed.Y)
			{
				velocityToHero.Y = 0f - _huntMaxSpeed.Y;
			}
		}
		else if (num6 > num)
		{
			velocityToHero.Y += _huntMaxSpeed.Y * 2f * elapsedTime;
			if (_velocity.Y > _huntMaxSpeed.Y)
			{
				velocityToHero.Y = _huntMaxSpeed.Y;
			}
		}
		else if (num6 > 0f)
		{
			velocityToHero.Y = _huntMaxSpeed.Y * Utils.Sqrt(num6 / num);
		}
		else if (num6 < 0f)
		{
			velocityToHero.Y = (0f - _huntMaxSpeed.Y) * Utils.Sqrt((0f - num6) / num);
		}
		if (MyObject.PathOscilation.Y <= 0f - _huntMaxRange.Y + num)
		{
			float num7 = Utils.Sqrt((_huntMaxRange.Y + MyObject.PathOscilation.Y) / num) * (0f - _huntMaxSpeed.Y);
			if (num7 > velocityToHero.Y)
			{
				velocityToHero.Y = num7;
			}
		}
		else if (MyObject.PathOscilation.Y >= _huntMaxRange.Y - num)
		{
			float num8 = Utils.Sqrt((_huntMaxRange.Y - MyObject.PathOscilation.Y) / num) * _huntMaxSpeed.Y;
			if (num8 < velocityToHero.Y)
			{
				velocityToHero.Y = num8;
			}
		}
		_velocity.Y = velocityToHero.Y;
		MyObject.PathOscilation.Y += _velocity.Y * elapsedTime;
		if (MyObject.PathOscilation.Y <= 0f - _huntMaxRange.Y)
		{
			velocityToHero.Y = 0f;
			MyObject.PathOscilation.Y = 0f - _huntMaxRange.Y;
		}
		else if (MyObject.PathOscilation.Y >= _huntMaxRange.Y)
		{
			velocityToHero.Y = 0f;
			MyObject.PathOscilation.Y = _huntMaxRange.Y;
		}
	}

	public override void DoEvent(BehaviorEvent pathEvent, BehaviorTimeLine sourceTimeLine)
	{
		if (Utils.Equals(pathEvent.Name, PathEvents[0]))
		{
			_huntMaxSpeed.X = pathEvent.ValueInt;
		}
		if (Utils.Equals(pathEvent.Name, PathEvents[1]))
		{
			_huntMaxSpeed.Y = pathEvent.ValueInt;
		}
		if (Utils.Equals(pathEvent.Name, PathEvents[2]))
		{
			_huntMaxRange.X = pathEvent.ValueInt;
		}
		if (Utils.Equals(pathEvent.Name, PathEvents[3]))
		{
			_huntMaxRange.Y = pathEvent.ValueInt;
		}
	}
}
