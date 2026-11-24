using Microsoft.Xna.Framework;

namespace Rake2D.Script.Man;

public class BehaviorManBase : BehaviorScript
{
	protected bool _isInitialized;

	protected ManStateEnum _state;

	protected ManDirectionEnum _direction;

	protected float _stateTimer;

	protected Vector2 _maxSpeed;

	protected Vector2 _switchOffset;

	protected bool _isImmortal;

	public override void RegisterEvents()
	{
	}

	public void SetImmortal(bool isImmortal)
	{
		if (_isImmortal != isImmortal)
		{
			_isImmortal = isImmortal;
			if (_isImmortal)
			{
				MyObject.SetHitAndTargetClass(PhysClass.Ruins, TargetClass.None);
			}
			else
			{
				MyObject.SetHitAndTargetClass(PhysClass.Man, TargetClass.Man);
			}
		}
	}

	protected static ManDirectionEnum GetRandomDirection()
	{
		if (Utils.GetRandom(1) != 1)
		{
			return ManDirectionEnum.East;
		}
		return ManDirectionEnum.West;
	}

	protected virtual void Update(float elapsedTime)
	{
		Vector2 vector = Utils.Clamp(MyObject.Velocity, _maxSpeed * -1f, _maxSpeed);
		if (vector != MyObject.Velocity)
		{
			MyObject.SetVelocity(vector);
		}
	}

	protected void ApplyDirectionForceX(float forceX)
	{
		if (_direction == ManDirectionEnum.West)
		{
			MyObject.ApplyForce(0f - forceX, 0f);
		}
		else
		{
			MyObject.ApplyForce(forceX, 0f);
		}
	}

	protected virtual void SetState(ManStateEnum state, ManDirectionEnum direction)
	{
		SetDirection(direction);
	}

	protected virtual void SetDirection(ManDirectionEnum direction)
	{
		_direction = direction;
		if (_direction == ManDirectionEnum.East)
		{
			MyObject.SetFlip(horizontal: true, vertical: false);
		}
		else
		{
			MyObject.SetFlip(horizontal: false, vertical: false);
		}
	}

	protected void SetDirectionByFlip()
	{
		SetDirection((MyObject.WorldScale.X < 0f) ? ManDirectionEnum.East : ManDirectionEnum.West);
	}

	protected void SwitchDirection()
	{
		SetDirection((_direction == ManDirectionEnum.West) ? ManDirectionEnum.East : ManDirectionEnum.West);
	}

	protected GameObject TurnToSalto(Vector2 force, bool bUseOriginalVelocity)
	{
		GameObject gameObject = null;
		gameObject = MyObject.SwitchToObject("man_salto", bAssumeChildren: true, _switchOffset);
		if (gameObject != null)
		{
			int num = ((!(force.X < 0f)) ? 1 : (-1));
			gameObject.SetPosition(gameObject.Position.X, gameObject.Position.Y - 18f);
			if (bUseOriginalVelocity)
			{
				Vector2 velocity = new Vector2(gameObject.VelocityX + (float)(Utils.GetRandom(200, 600) * num), gameObject.VelocityY + force.Y);
				gameObject.SetVelocity(velocity);
			}
			else
			{
				gameObject.SetVelocity(force);
			}
			gameObject.SetRotationSpeedInDeg(Utils.GetRandom(150, 400) * num);
		}
		return gameObject;
	}
}
