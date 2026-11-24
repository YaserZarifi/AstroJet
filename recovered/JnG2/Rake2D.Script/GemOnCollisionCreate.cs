using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public class GemOnCollisionCreate : ItemBehaviorCreateBase
{
	private bool _normalRotation;

	private bool _aimToHero;

	private bool _targetIsParent;

	private bool _onlyHittable;

	public override void ApplyToItem(HeroItem item)
	{
		if (item != null)
		{
			item.OnHitCollision += OnHitCollision;
		}
	}

	public override void Init()
	{
		base.Init();
		_normalRotation = ParamBool("normal");
		_aimToHero = ParamBool("aim_to_hero");
		_targetIsParent = ParamBool("on_target");
		_onlyHittable = ParamBool("only_hittable");
		_damage = Param("damage");
	}

	public void OnHitCollision(GameObject shotObject, GameObject targetObject, Vector2 contactPoint, Vector2 normal, float damage)
	{
		if (!_onlyHittable || (!targetObject.IsImmortal && targetObject.GetMainPart().CurrentHP > 0f))
		{
			GameObject gameObject = CreateObject(contactPoint, _targetIsParent ? targetObject : shotObject);
			if (_damage >= 0f)
			{
				gameObject.Damage = (long)_damage;
			}
			if (_aimToHero && MyHero != null)
			{
				gameObject.SetRotation(Utils.GetRotationTo(gameObject.WorldPosition, MyHero.MyObject.WorldPosition) + _rotationRange.GetRandom());
			}
			else if (_normalRotation)
			{
				gameObject?.SetRotation(Utils.GetRotationFromZero(normal) + _rotationRange.GetRandom());
			}
		}
	}
}
