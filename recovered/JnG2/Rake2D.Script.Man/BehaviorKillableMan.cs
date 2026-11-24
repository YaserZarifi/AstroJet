using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script.Man;

public class BehaviorKillableMan : BehaviorManBase
{
	protected bool _noBurn;

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyObject.OnHitBegin += OnHitBegin;
		MyObject.OnShockHit += OnShockHit;
		MyObject.OnEnvironmentChanged += OnEnvironmentChanged;
	}

	protected void OnShockHit(DamageTypeEnum damageType, Vector2 physForce, GameObject hittingObject)
	{
		if (_isImmortal)
		{
			return;
		}
		physForce.X = Utils.Clamp(physForce.X, -750f, 750f);
		if (physForce.X < 0f && physForce.X > -200f)
		{
			physForce.X = -200f;
		}
		else if (physForce.X >= 0f && physForce.X < 200f)
		{
			physForce.X = 200f;
		}
		physForce.Y = Utils.Clamp(physForce.Y, -600f, 600f);
		float num = 0f - Math.Max(100f, Math.Abs(physForce.Y));
		if (!_noBurn && (damageType == DamageTypeEnum.Fire || damageType == DamageTypeEnum.Explosion) && Utils.GetRandom(3) == 0)
		{
			switch (Utils.GetRandom(1))
			{
			case 0:
				MyObject.SwitchToObject("man_burn", bAssumeChildren: false, _switchOffset, SwitchObjectKillModeEnum.Destroy);
				break;
			case 1:
				MyObject.SwitchToObject("man_burn_end", bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Kill);
				break;
			}
		}
		else if (damageType == DamageTypeEnum.Electric && Utils.GetRandom(2) != 0)
		{
			MyObject.SwitchToObject("man_electrocute", bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Kill);
		}
		else if (Utils.GetRandom(3) == 0)
		{
			RandomDeath();
		}
		else
		{
			TurnToSalto(Utils.GetRandomVector2(physForce.X / 2f, num / 3f, physForce.X, num), bUseOriginalVelocity: false);
			MyObject.Kill();
		}
	}

	protected GameObject RandomDeath()
	{
		GameObject gameObject = Utils.GetRandom(3) switch
		{
			0 => MyObject.SwitchToObject("man_death_explo", bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Kill), 
			1 => MyObject.SwitchToObject("man_death_1", bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Kill), 
			2 => MyObject.SwitchToObject("man_death_2", bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Kill), 
			_ => MyObject.SwitchToObject("man_death_3", bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Kill), 
		};
		if (gameObject != null)
		{
			GameObject gameObject2 = null;
			switch (Utils.GetRandom(1))
			{
			case 0:
				gameObject2 = gameObject.CreateChildByPartData("man_blood_splash_1", createPhysBody: false);
				break;
			case 1:
				gameObject2 = gameObject.CreateChildByPartData("man_blood_splash_2", createPhysBody: false);
				break;
			}
			if (gameObject2 != null)
			{
				gameObject2.Position.Y -= Utils.GetRandom(18, 22);
				gameObject2.SetRotation(Utils.GetRandom(-0.7f, 0.7f));
			}
		}
		return gameObject;
	}

	protected void OnHitBegin(ref float damage, DamageTypeEnum damageType, GameObject hittingObject)
	{
		if (_isImmortal)
		{
			damage = 0f;
		}
		else
		{
			if (damage <= 0f)
			{
				return;
			}
			if ((damageType == DamageTypeEnum.Fire || damageType == DamageTypeEnum.Explosion) && Utils.GetRandom(3) == 0)
			{
				switch (Utils.GetRandom(1))
				{
				case 0:
					MyObject.SwitchToObject("man_burn", bAssumeChildren: false, _switchOffset, SwitchObjectKillModeEnum.Destroy);
					break;
				case 1:
					MyObject.SwitchToObject("man_burn_end", bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Kill);
					break;
				}
			}
			else if (damageType == DamageTypeEnum.Electric && Utils.GetRandom(2) != 0)
			{
				MyObject.SwitchToObject("man_electrocute", bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Kill);
			}
			else if (hittingObject != null && Utils.GetRandom(2) == 0)
			{
				TurnToSalto(new Vector2(Utils.GetRandom(100, 600) * ((hittingObject.Velocity.X >= 0f) ? 1 : (-1)), Utils.GetRandom(-100, -600)), bUseOriginalVelocity: true);
				MyObject.Kill();
			}
			else
			{
				RandomDeath();
			}
		}
	}

	private void OnEnvironmentChanged(EnvironmentEnum env)
	{
		if (env == EnvironmentEnum.Water)
		{
			GameObject gameObject = MyObject.SwitchToObject("man_drown", bAssumeChildren: false, _switchOffset, SwitchObjectKillModeEnum.Destroy);
			gameObject.SetVelocity(gameObject.Velocity / 2.5f);
		}
	}
}
