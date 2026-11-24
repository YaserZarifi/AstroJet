using System;
using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public class HeroJetPack : BehaviorScript
{
	private enum StateEnum
	{
		Eject,
		ControledByPlayer,
		LandingOnShip
	}

	private StateEnum _state;

	private int _stuckOnViewEdge;

	private bool _isWaitingToShip;

	private PartData _truckData;

	private PartData _shellData;

	private PartData _callShipData;

	private GameObject _shellOutPart;

	private float _controlTimer;

	private float _hitTimer;

	private PartData _coliEffectData;

	public override void Init()
	{
		base.Init();
		_state = StateEnum.Eject;
		_truckData = GameCore.GetPartData("hero_jet_pack_truck");
		_shellData = GameCore.GetPartData("shot_shell");
		_coliEffectData = GameCore.GetPartData("hero_jet_pack_coli_effect");
		_callShipData = GameCore.GetPartData("hero_jet_pack_call_ship");
		_shellOutPart = MyObject.GetChildByIdDeep("shell_out");
	}

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		MyObject.OnLink += OnLink;
		MyObject.OnCollision += OnCollision;
		MyObject.OnHitBegin += DoOnHitBegin;
		if (MyObject.MyHero == null)
		{
			MyObject.MyHero = GameJnG.HeroList[0];
		}
		_controlTimer = 0.1f;
		_hitTimer = 0f;
	}

	private void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (collisionObject.TargetClass == TargetClass.Man)
		{
			collisionObject.Hit(1f, DamageTypeEnum.Collision, MyObject);
		}
		else if (_state == StateEnum.ControledByPlayer && (collisionObject.TargetClass & TargetClass.AllEnemy) != 0)
		{
			MyObject.SetVelocity(normal * 400f);
			if (_hitTimer <= 0f)
			{
				MyLayer.CreateObjectByPartData(_coliEffectData, contactPoint, MyObject.Scale, 0f, 1, bCreateRigidBody: false);
				MyObject.Hit(1f, DamageTypeEnum.Collision, collisionObject);
				_hitTimer = 0.2f;
			}
		}
	}

	private void DoOnHitBegin(ref float damage, DamageTypeEnum damageType, GameObject hittingObject)
	{
		if (Cheat.NoDamage || GameCore.IsShopMenuEnabled)
		{
			damage = 0f;
		}
		if (!(damage <= 0f))
		{
			if (MyObject.MyHero.IsZeldorPanzerEnabled)
			{
				damage = 1f;
			}
			else
			{
				damage *= FactorManager.DamageToHeroFactor;
			}
			AddHPTextParticle(0f - damage, MyObject.WorldPosition);
		}
	}

	private void ParseInput(float elapsedTime)
	{
		Vector2 targetVelocityByInput = MyObject.MyHero.GetTargetVelocityByInput();
		if (GameJnG.IsTimeCompressorEnabled)
		{
			targetVelocityByInput /= GameCore.TimeFactor;
		}
		if (_state != StateEnum.ControledByPlayer && _state == StateEnum.Eject && targetVelocityByInput.Length() > 0f && Utils.ChangeValueToTarget(ref _controlTimer, 0f, elapsedTime))
		{
			_state = StateEnum.ControledByPlayer;
		}
		if (_state == StateEnum.ControledByPlayer)
		{
			targetVelocityByInput += MyLayer.ScrollSpeed;
			Vector2 currentValue = MyObject.Velocity;
			float num = 10000f;
			if (GameJnG.IsTimeCompressorEnabled)
			{
				num /= GameCore.TimeFactor;
				Utils.ChangeValueToTarget(ref currentValue, targetVelocityByInput, num * elapsedTime / GameCore.TimeFactor);
			}
			else
			{
				Utils.ChangeValueToTarget(ref currentValue, targetVelocityByInput, num * elapsedTime);
			}
			MyObject.SetVelocity(currentValue);
		}
		if (MyObject.MyHero.IsDestroyed && !_isWaitingToShip && (GameInput.KeyPressed(GameInput.KeyTypeEnum.PrimaryDevice) || GameInput.KeyPressed(GameInput.KeyTypeEnum.SecondaryDevice)))
		{
			if (MyObject.MyHero.ShipRecoveryValue >= MyObject.MyHero.GetShipMaxHP())
			{
				MyLevel.LastMainLayer.CreateObjectByPartData(_truckData, MyObject.WorldPosition, Vector2.One, 0f, 999, bCreateRigidBody: false).MyHero = MyObject.MyHero;
				_isWaitingToShip = true;
			}
			else
			{
				GameCore.PlaySound("menu_denied");
			}
			MyObject.CreateChildByPartData(_callShipData, createPhysBody: false);
		}
	}

	protected Particle AddHPTextParticle(float value, Vector2 position)
	{
		Color color = ((!(value >= 0f)) ? new Color(255, 0, 0) : new Color(255, 220, 80));
		return MyObject.MyLevel.AddTextParticle(string.Format("H.I. {0}{1}", (value > 0f) ? "+" : "-", (int)Math.Abs(value)), color, position);
	}

	protected void CheckOutOfGameView()
	{
		Vector2 position = GameCore.CurrentLevel.Camera.Position;
		if (MyObject.Position.X + (float)MyObject.VisibilityRect.Left < (float)(int)position.X)
		{
			if (_stuckOnViewEdge++ > 10 && MyObject.Position.X + (float)MyObject.VisibilityRect.Left < (float)((int)position.X - 20))
			{
				Vector2 nearestSafePosition = MyObject.MyLevel.GetNearestSafePosition(MyObject.WorldPosition.X);
				if (nearestSafePosition.Y + (float)MyObject.VisibilityRect.Bottom >= MyObject.MyCamera.Position.Y + 1080f)
				{
					MyObject.MyCamera.Position.Y = nearestSafePosition.Y + (float)MyObject.VisibilityRect.Bottom - 1080f;
				}
				else if (nearestSafePosition.Y + (float)MyObject.VisibilityRect.Top <= MyObject.MyCamera.Position.Y)
				{
					MyObject.MyCamera.Position.Y = nearestSafePosition.Y + (float)MyObject.VisibilityRect.Top;
				}
				position = GameCore.CurrentLevel.Camera.Position;
				MyObject.SetPosition(new Vector2((int)position.X - MyObject.VisibilityRect.Left, nearestSafePosition.Y));
			}
			else
			{
				MyObject.SetPosition(new Vector2((int)position.X - MyObject.VisibilityRect.Left, MyObject.Position.Y));
			}
			if (MyObject.VelocityX < GameCore.CurrentLevel.ScrollSpeed.X)
			{
				MyObject.VelocityX = GameCore.CurrentLevel.ScrollSpeed.X;
			}
		}
		else
		{
			_stuckOnViewEdge = 0;
			if (MyObject.Position.X + (float)MyObject.VisibilityRect.Right > position.X + 1920f)
			{
				MyObject.SetPosition(new Vector2((float)(int)position.X + 1920f - (float)MyObject.VisibilityRect.Right, MyObject.Position.Y));
				MyObject.VelocityX = GameCore.CurrentLevel.ScrollSpeed.X;
			}
		}
		if (MyObject.Position.Y + (float)MyObject.VisibilityRect.Top < position.Y)
		{
			MyObject.SetPosition(new Vector2(MyObject.Position.X, GameCore.CurrentLevel.Camera.Position.Y - (float)MyObject.VisibilityRect.Top));
			MyObject.VelocityY = GameCore.CurrentLevel.ScrollSpeed.Y;
		}
		else if (MyObject.Position.Y + (float)MyObject.VisibilityRect.Bottom > position.Y + 1080f)
		{
			MyObject.SetPosition(new Vector2(MyObject.Position.X, position.Y + 1080f - (float)MyObject.VisibilityRect.Bottom));
			MyObject.VelocityY = GameCore.CurrentLevel.ScrollSpeed.Y;
		}
	}

	private void OnUpdate(float elapsedTime)
	{
		if (MyObject.MyHero.IsZeldorPanzerEnabled)
		{
			MyObject.ChangeMaxHP(MyObject.PartData.MaxHP / 10);
		}
		else
		{
			MyObject.ChangeMaxHP(MyObject.PartData.MaxHP);
		}
		if (_isWaitingToShip && !MyObject.MyHero.IsDestroyed)
		{
			_state = StateEnum.LandingOnShip;
			MyObject.PhysClass = PhysClass.None;
		}
		if (_state == StateEnum.Eject)
		{
			float currentValue = MyObject.Velocity.Y;
			if (GameJnG.IsTimeCompressorEnabled)
			{
				Utils.ChangeValueToTarget(ref currentValue, 0f, 1500f / GameCore.TimeFactor * elapsedTime / GameCore.TimeFactor);
			}
			else
			{
				Utils.ChangeValueToTarget(ref currentValue, 0f, 1500f * elapsedTime);
			}
			MyObject.SetVelocity(MyObject.Velocity.X, currentValue);
			if (currentValue >= 0f)
			{
				_state = StateEnum.ControledByPlayer;
			}
		}
		Utils.ChangeValueToTarget(ref _hitTimer, 0f, elapsedTime);
		if (_state != StateEnum.LandingOnShip)
		{
			ParseInput(elapsedTime);
			CheckOutOfGameView();
		}
		else if (MyObject.MyHero.IsDestroyed)
		{
			_state = StateEnum.ControledByPlayer;
			MyObject.PhysClass = PhysClass.JetPack;
		}
		else
		{
			float distance = Utils.GetDistance(MyObject.WorldPosition, MyObject.MyHero.MyObject.WorldPosition);
			if (distance < 20f)
			{
				MyObject.MyHero.SetJetPackObject(null);
				MyObject.Destroy();
			}
			else
			{
				Vector2 velocityVector = Utils.GetVelocityVector2(Math.Max(900f, distance * 5f), MyObject.GetRotationToHero(MyObject.MyHero));
				if (GameJnG.IsTimeCompressorEnabled)
				{
					velocityVector /= GameCore.TimeFactor;
				}
				velocityVector += MyObject.MyHero.MyObject.Velocity;
				MyObject.SetVelocity(velocityVector);
			}
		}
		if (MyObject.MyHero.IsJetPackShellReady)
		{
			GameObject gameObject = _shellOutPart.MyLayer.CreateObjectByPartData(_shellData, _shellOutPart.WorldPosition, _shellOutPart.Scale, _shellOutPart.WorldRotation, 1);
			gameObject.SetVelocity(_shellOutPart.MyLevel.ScrollSpeed + Utils.RotateVector2(new Vector2(Utils.GetRandom(-80, -50), Utils.GetRandom(-200, -280)), _shellOutPart.WorldRotation));
			gameObject.SetRotationSpeedInDeg(Utils.GetRandom(-280, -400));
			MyObject.MyHero.IsJetPackShellReady = false;
		}
	}

	private void OnLink(GameObject senderobject, string linkName, int linkValue)
	{
		if (_state != StateEnum.LandingOnShip && Utils.Equals(linkName, "land_on_ship"))
		{
			_state = StateEnum.LandingOnShip;
			MyObject.PhysClass = PhysClass.None;
		}
	}
}
