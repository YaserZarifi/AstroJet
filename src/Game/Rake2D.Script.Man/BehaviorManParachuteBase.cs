using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script.Man;

public class BehaviorManParachuteBase : BehaviorManBase
{
	public override void RegisterEvents()
	{
		MyObject.OnShockHit += OnShockHit;
		MyObject.OnHitBegin += OnHitBegin;
	}

	protected virtual void KillMe(Vector2 physForce)
	{
		if (Utils.GetRandom(3) == 0)
		{
			MyObject.SwitchToObject("man_death_explo", bAssumeChildren: true, _switchOffset);
		}
		else
		{
			TurnToSalto(physForce, bUseOriginalVelocity: true);
		}
	}

	protected void OnShockHit(DamageTypeEnum damageType, Vector2 physForce, GameObject hittingObject)
	{
		if (!_isImmortal)
		{
			float num = Math.Max(Math.Abs(physForce.X), Math.Abs(physForce.Y));
			if (num > 1000f)
			{
				num = 1000f;
			}
			KillMe(Utils.GetRandomVector2(physForce.X / 4f, (0f - num) / 2f, physForce.X, 0f - num));
			MyObject.Kill();
		}
	}

	protected void OnHitBegin(ref float damage, DamageTypeEnum damageType, GameObject hittingObject)
	{
		if (_isImmortal)
		{
			damage = 0f;
		}
		else if (!(damage <= 0f))
		{
			KillMe(new Vector2(Utils.GetRandom(100, 600) * ((hittingObject.Velocity.X >= 0f) ? 1 : (-1)), Utils.GetRandom(-100, -600)));
		}
	}
}
