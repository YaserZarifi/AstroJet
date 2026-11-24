using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public class HeroDeviceJetPack : ItemBehaviorScript
{
	private float _jetPackHP = -1f;

	private PartData _jetPackData;

	private PartData _ejectData;

	private bool _isJetPackObjectActive => MyHero.JetPackObject != null;

	public override void RegisterEvents()
	{
		MyItem.OnActivated += OnActivated;
		MyHero.MyObject.OnKill += OnHeroKill;
		_jetPackHP = -1f;
	}

	public override void StartInSlot()
	{
		base.StartInSlot();
		MyHero.IsJetPackInstalled = true;
	}

	public override void Init()
	{
		base.Init();
		_jetPackData = GameCore.GetPartData("hero_jetpack");
		_ejectData = GameCore.GetPartData("hero_jet_pack_eject");
	}

	private void OnHeroKill()
	{
		if (!_isJetPackObjectActive)
		{
			EjectJetPack();
			MyLayer.CreateObjectByPartData(_ejectData, MyHero.MyObject.WorldPosition, MyHero.MyObject.Scale, 0f, -1, bCreateRigidBody: false);
		}
	}

	private void EjectJetPack()
	{
		if (MyHero.JetPackObject != null)
		{
			return;
		}
		GameObject gameObject = MyHero.MyObject.MyLayer.CreateObjectByPartData(_jetPackData, MyHero.MyObject.WorldPosition, Vector2.One, 0f, -1);
		if (gameObject != null)
		{
			if (_jetPackHP >= 0f)
			{
				gameObject.CurrentHP = _jetPackHP;
			}
			if (GameJnG.IsTimeCompressorEnabled)
			{
				gameObject.SetVelocity(MyLayer.ScrollSpeed.X, -900f / GameCore.TimeFactor);
			}
			else
			{
				gameObject.SetVelocity(MyLayer.ScrollSpeed.X, -900f);
			}
			MyHero.SetJetPackObject(gameObject);
		}
	}

	private void OnActivated()
	{
		if (_isJetPackObjectActive)
		{
			MyHero.JetPackObject.DoOnLink(MyHero.MyObject, "land_on_ship", 1, bApplyChildren: true);
			return;
		}
		EjectJetPack();
		MyHero.MyObject.CreateChildByPartData(_ejectData, createPhysBody: false);
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (MyHero.JetPackObject != null)
		{
			_jetPackHP = MyHero.JetPackObject.CurrentHP;
			if (MyHero.IsZeldorPanzerEnabled)
			{
				_jetPackHP *= 10f;
			}
		}
	}
}
