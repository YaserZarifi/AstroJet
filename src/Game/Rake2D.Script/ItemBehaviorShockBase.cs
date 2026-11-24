using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public abstract class ItemBehaviorShockBase : ItemBehaviorScript
{
	private float _radius;

	private long _damage;

	private float _physForce;

	private TargetClass _targetClass = (TargetClass)8440;

	private DamageTypeEnum _damageType;

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		_radius = Param("radius", 100f);
		_damage = (long)Param("damage", MyItem.GetParam("damage"));
		_physForce = Param("phys_force", 500f);
		_damageType = DataUtils.GetDamageTypeByString(ParamString("damage_type", "explosion"));
	}

	public void Shock(Vector2 epicentrumPos, List<GameObject> ignoreList)
	{
		MyLevel.ShockHit(MyObject, epicentrumPos, _radius, _targetClass, _damageType, _damage, _physForce, ignoreList, onlyHitable: false, wantResults: false);
	}
}
