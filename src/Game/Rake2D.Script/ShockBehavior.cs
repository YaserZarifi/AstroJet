using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public abstract class ShockBehavior : BehaviorScript
{
	protected TargetClass _targetClass = (TargetClass)73976;

	protected DamageTypeEnum _damageType;

	protected float _radius;

	protected float _damage;

	protected bool _useDamageFactor;

	protected float _chanceToAcid;

	protected float _chanceToBurn;

	protected float _physForce;

	public override void Init()
	{
		base.Init();
		_damageType = DataUtils.GetDamageTypeByString(ParamString("damage_type", "explosion"));
		string[] array = ParamStringArray("target_class");
		if (array != null)
		{
			_targetClass = DataUtils.GetTargetClassByStringArray(array);
		}
		_damage = Param("damage", -100f);
		_useDamageFactor = ParamBool("use_damage_factor", defaultValue: true);
		_physForce = Param("phys_force");
		_chanceToAcid = Param("chance_to_acid");
		_chanceToBurn = Param("chance_to_burn");
		_radius = Param("radius", 200f);
	}

	public float GetShockDamage()
	{
		float num = ((_damage < 0f) ? (MyObject.Damage * _damage / -100f) : _damage);
		if (_useDamageFactor)
		{
			num *= FactorManager.DamageFactor;
		}
		return num;
	}

	public void Shock(Vector2 epicentrumPos, List<GameObject> ignoreList)
	{
		List<GameObject> list = MyLevel.ShockHit(MyObject, epicentrumPos, _radius, _targetClass, _damageType, GetShockDamage(), _physForce, ignoreList, onlyHitable: false, wantResults: true);
		if (list != null && (_chanceToAcid > 0f || _chanceToBurn > 0f))
		{
			foreach (GameObject item in list)
			{
				if (_chanceToAcid > (float)Utils.GetRandom(0, 99))
				{
					FxEffect.StartAcid(item, MyObject.MyHero ?? MyObject.LastHittingObject?.MyHero ?? GameJnG.HeroList[0]);
				}
			}
		}
		GameCore.Allocator.DeleteAliasingGameObjectList(list);
	}
}
