using Microsoft.Xna.Framework;

namespace Rake2D.Script.Man;

public class Burn : BehaviorKillableMan
{
	private float _screamTimer;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
	}

	public override void Init()
	{
		base.Init();
		SetImmortal(isImmortal: true);
		_noBurn = true;
		SetDirection(BehaviorManBase.GetRandomDirection());
		MyObject.SpriteRenderer.Atlas = GameCore.GetSpriteAtlas("man_burn", logWhenNotFound: true);
		MyObject.SpriteRenderer.ChangeAnim("burn", 0.5f);
		_stateTimer = Utils.GetRandom(0.75f, 2f);
		_maxSpeed = new Vector2(240f, 999f);
	}

	protected void OnUpdate(float elapsedTime)
	{
		if (MyObject.LifeTime > 0.5f)
		{
			SetImmortal(isImmortal: false);
		}
		_screamTimer -= elapsedTime;
		if (_screamTimer <= 0f)
		{
			MyObject.PlaySound($"man_burn_{Utils.GetRandom(3)}");
			_screamTimer = Utils.GetRandom(0.75f, 2f);
		}
		_stateTimer -= elapsedTime;
		if (_stateTimer <= 0f)
		{
			if (Utils.GetRandom(3) == 0)
			{
				MyObject.SwitchToObject("man_burn_end", bAssumeChildren: true, _switchOffset, SwitchObjectKillModeEnum.Kill);
				return;
			}
			_stateTimer = Utils.GetRandom(0.75f, 2f);
			SwitchDirection();
		}
		ApplyDirectionForceX(2000f);
		base.Update(elapsedTime);
	}
}
