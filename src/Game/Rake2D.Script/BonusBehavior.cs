using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public abstract class BonusBehavior : BehaviorScript
{
	protected float _bonusValue;

	protected int _textMode;

	protected bool _isCounted;

	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
	}

	public override void Init()
	{
		base.Init();
		_bonusValue = Param("bonus_value");
		_textMode = ParamInt("text_mode");
		_isCounted = ParamBool("counted");
		if (_isCounted)
		{
			GameJnG.AchievementManager.RaiseLevelAvailableCount(AchievementCountTypeEnum.BonusCount);
		}
	}

	protected void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (collisionObject.MyHero != null)
		{
			ApplyBonus(collisionObject.MyHero);
			if (_isCounted)
			{
				GameJnG.AchievementManager.RaiseLevelAchievedCount(AchievementCountTypeEnum.BonusCount);
			}
		}
	}

	protected abstract void ApplyBonus(Hero hero);
}
