using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public class HeroEquipmentZogonInsurance : ItemBehaviorScript
{
	public static int INSURANCE_PRICE = 500;

	private PartData _deployerData;

	private float _deployTimer;

	public override void Init()
	{
		base.Init();
		_deployerData = GameCore.GetPartData("zogon_insurance_deployer");
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyLevel.OnLevelBegin += OnLevelBegin;
	}

	private void OnLevelBegin()
	{
		_deployTimer = 0f;
	}

	public override void Update(float elapsedTime)
	{
		_deployTimer -= elapsedTime;
		if (MyHero.ShipHPPercent <= 0.25f && _deployTimer <= 0f && GameJnG.ZogonInsuranceCount <= 0)
		{
			Vector2 nearestSafePosition = MyLevel.GetNearestSafePosition(MyLevel.Camera.Position.X + 960f);
			MyLevel.LastMainLayer.CreateObjectByPartData(_deployerData, nearestSafePosition, Vector2.One, 0f, 1);
			GameJnG.ZogonInsuranceCount++;
			_deployTimer = 60f;
		}
	}
}
