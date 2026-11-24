using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public class CheckPoint : BehaviorScript
{
	private GameObject _upPart;

	private GameObject _downPart;

	private Particle _startParticleUp;

	private Particle _endParticleUp;

	private Particle _startParticleDown;

	private Particle _endParticleDown;

	private Vector2 _storePostion;

	private float _hideTimer;

	private bool _bHide;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		MyObject.OnEditorUpdate += OnUpdate;
		MyObject.OnDestroy += OnDestroy;
		_hideTimer = 0f;
	}

	public override void Init()
	{
		base.Init();
		_upPart = MyObject.GetChildByIdDeep("up");
		_downPart = MyObject.GetChildByIdDeep("down");
		_storePostion = ParamVector2("store_pos");
	}

	private void OnUpdate(float elapsedTime)
	{
		if (_upPart.Emitter.StripLine == null || _upPart.Emitter.StripLine.ParticleList == null || _upPart.Emitter.StripLine.ParticleList.Count < 2)
		{
			_upPart.Emitter.Emit(bIgnoreLoopCount: true);
			_startParticleUp = _upPart.Emitter.StripLine.ParticleList[1];
			_endParticleUp = _upPart.Emitter.StripLine.ParticleList[0];
			_upPart.Emitter.StripLine.ParentTransformation = null;
			_upPart.Emitter.IsEmitedByBehavior = true;
		}
		if (_downPart.Emitter.StripLine == null || _downPart.Emitter.StripLine.ParticleList == null || _downPart.Emitter.StripLine.ParticleList.Count < 2)
		{
			_downPart.Emitter.Emit(bIgnoreLoopCount: true);
			_startParticleDown = _downPart.Emitter.StripLine.ParticleList[1];
			_endParticleDown = _downPart.Emitter.StripLine.ParticleList[0];
			_downPart.Emitter.StripLine.ParentTransformation = null;
			_downPart.Emitter.IsEmitedByBehavior = true;
		}
		if (_bHide && !GameCore.IsEditorEnabled)
		{
			_hideTimer += elapsedTime;
		}
		float num = (float)MyObject.FrameRectangle.Bottom * MyObject.WorldScale.Y - _hideTimer * _upPart.Emitter.Data.StripTextureFlowSpeed1 * 65f;
		if (num < (float)(-MyObject.FrameRectangle.Bottom) * MyObject.WorldScale.Y)
		{
			if (!GameCore.IsEditorEnabled && _bHide)
			{
				MyObject.Destroy();
				return;
			}
			num = (float)(-MyObject.FrameRectangle.Bottom) * MyObject.WorldScale.Y;
		}
		_startParticleUp.Position = new Vector2(MyObject.WorldPosition.X + _upPart.Position.X, MyObject.WorldPosition.Y + num);
		_endParticleUp.Position = new Vector2(MyObject.WorldPosition.X + _upPart.Position.X, MyObject.WorldPosition.Y - (float)MyObject.FrameRectangle.Bottom * MyObject.WorldScale.Y);
		Particle startParticleUp = _startParticleUp;
		Color spriteColor = (_endParticleUp.SpriteColor = _upPart.Color);
		startParticleUp.SpriteColor = spriteColor;
		_startParticleDown.Position = new Vector2(MyObject.WorldPosition.X + _downPart.Position.X, MyObject.WorldPosition.Y - num);
		_endParticleDown.Position = new Vector2(MyObject.WorldPosition.X + _downPart.Position.X, MyObject.WorldPosition.Y + (float)MyObject.FrameRectangle.Bottom * MyObject.WorldScale.Y);
		Particle startParticleDown = _startParticleDown;
		spriteColor = (_endParticleDown.SpriteColor = _downPart.Color);
		startParticleDown.SpriteColor = spriteColor;
		if (_bHide)
		{
			return;
		}
		RayCastResult rayCastResult = MyObject.MyLevel.GetRayCastResult(_startParticleUp.WorldPosition, 0f, (_endParticleUp.WorldPosition - _startParticleUp.WorldPosition).Length(), (PhysClass)67108865);
		if (rayCastResult.TargetObject == null)
		{
			return;
		}
		_bHide = true;
		rayCastResult.TargetObject.MyLayer.CreateObjectByPartData("checkpoint_reached", rayCastResult.CollisionPosition, Vector2.One, 0f, rayCastResult.TargetObject.ZOrder + 1, bCreateRigidBody: false);
		MyObject.MyLevel.AddWarningText(TextBundle.GetText("checkpoint_reached"), new Color(100, 200, 255));
		if (_storePostion != Vector2.Zero)
		{
			GameJnG.CheckpointInfo.Save(_storePostion);
		}
		else
		{
			GameJnG.CheckpointInfo.Save(MyObject.WorldPosition);
		}
		foreach (Hero hero in GameJnG.HeroList)
		{
			if (hero.HPPercent <= 0f && !hero.MyObject.IsAlive)
			{
				hero.SpawnInLevel(GameCore.GameLevel);
				hero.SetShipHP(hero.GetShipMaxHP() / 2f);
			}
			else if (hero.MyObject.CurrentHP < hero.GetShipMaxHP() / 2f)
			{
				hero.SetShipHP(hero.GetShipMaxHP() / 2f);
			}
			if (hero.IsJetPackActive && hero.JetPackObject != null && hero.JetPackObject.IsAlive && !hero.MyObject.IsAlive && hero.JetPackObject.CurrentHP < hero.JetPackObject.MaxHP / 2f)
			{
				hero.JetPackObject.CurrentHP = hero.JetPackObject.MaxHP / 2f;
			}
		}
	}

	private void OnDestroy()
	{
		_startParticleDown?.Destroy();
		_endParticleDown?.Destroy();
		_startParticleUp?.Destroy();
		_endParticleUp?.Destroy();
	}
}
