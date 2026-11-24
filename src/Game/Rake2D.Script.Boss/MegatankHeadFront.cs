using Microsoft.Xna.Framework;

namespace Rake2D.Script.Boss;

public class MegatankHeadFront : MegatankHead
{
	private GameObject _sideLaserObject;

	private GameObject _topTurretObject;

	private GameObject _bottomTurretObject;

	private GameObject _bfgObject;

	private GameObject _plateObject;

	private int _turretKillCount;

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		_sideLaserObject.OnReRegisterEvents += MyMegatank.OnReRegisterEventsKillCounter;
		_topTurretObject.OnReRegisterEvents += MyMegatank.OnReRegisterEventsKillCounter;
		_bottomTurretObject.OnReRegisterEvents += MyMegatank.OnReRegisterEventsKillCounter;
		_bfgObject.OnReRegisterEvents += MyMegatank.OnReRegisterEventsKillCounter;
		_topTurretObject.OnReRegisterEvents += OnReregisterTurret;
		_bottomTurretObject.OnReRegisterEvents += OnReregisterTurret;
	}

	public void OnReregisterTurret(GameObject go)
	{
		go.OnKill += RaiseTurretKillCount;
	}

	private void RaiseTurretKillCount()
	{
		_turretKillCount++;
		if (_turretKillCount > 1)
		{
			_plateObject.IsImmortal = false;
		}
	}

	public MegatankHeadFront(GameObject go, Megatank megatank)
		: base(go, megatank)
	{
		_sideLaserObject = MyObject.GetChildByIdDeep("side_laser");
		_topTurretObject = MyObject.GetChildByIdDeep("turret_ft");
		_bottomTurretObject = MyObject.GetChildByIdDeep("turret_fb");
		_bfgObject = MyObject.GetChildByIdDeep("bfg");
		_sideGunObject = MyObject.GetChildByIdDeep("side_gun");
		_plateObject = MyObject.GetChildByIdDeep("hfplate");
		_plateObject.IsImmortal = true;
	}

	public override void SetPhase(MegatankPhaseEnum phase)
	{
		switch (phase)
		{
		case MegatankPhaseEnum.Intro:
			_sideLaserObject.ChangePartStateToAll("inactive");
			_topTurretObject.ChangePartStateToAll("inactive");
			_bottomTurretObject.ChangePartStateToAll("inactive");
			_bfgObject.ChangePartStateToAll("inactive");
			_sideGunObject.ChangePartStateToAll("inactive");
			_heartObject.ChangePartStateToAll("inactive");
			break;
		case MegatankPhaseEnum.Basic:
			if (_sideLaserObject.IsAlive)
			{
				_sideLaserObject.ChangePartStateToAll("idle");
			}
			if (_topTurretObject.IsAlive)
			{
				_topTurretObject.ChangePartStateToAll("idle");
			}
			if (_bottomTurretObject.IsAlive)
			{
				_bottomTurretObject.ChangePartStateToAll("idle");
			}
			if (_bfgObject.IsAlive)
			{
				_bfgObject.ChangePartStateToAll("idle");
			}
			_sideGunObject.ChangePartStateToAll("inactive");
			break;
		case MegatankPhaseEnum.FlyingHeads:
			if (_sideLaserObject.IsAlive)
			{
				_sideLaserObject.ChangePartStateToAll("idle");
			}
			if (_topTurretObject.IsAlive)
			{
				_topTurretObject.ChangePartStateToAll("idle");
			}
			if (_bottomTurretObject.IsAlive)
			{
				_bottomTurretObject.ChangePartStateToAll("idle");
			}
			if (_bfgObject.IsAlive)
			{
				_bfgObject.ChangePartStateToAll("idle");
			}
			break;
		}
		_phase = phase;
	}

	public override void Activate()
	{
		base.Activate();
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
	}

	public override void SetMoveState(MegatankHeadMoveStateEnum moveState)
	{
		base.SetMoveState(moveState);
		switch (MoveState)
		{
		case MegatankHeadMoveStateEnum.None:
			_heartObject.ChangePartStateToAll("inactive");
			break;
		case MegatankHeadMoveStateEnum.StartPrepare:
			MyObject.CreateChildByPartData("megatank_head_start", createPhysBody: false).SetPosition(-20f, 140f);
			break;
		case MegatankHeadMoveStateEnum.Start:
			_heartObject.ChangePartStateToAll("idle");
			break;
		case MegatankHeadMoveStateEnum.MoveToScreenX:
			_targetPosition = new Vector2(1500f, MyObject.ParentObject.WorldPosition.Y - 500f);
			break;
		case MegatankHeadMoveStateEnum.Open:
		case MegatankHeadMoveStateEnum.Close:
		case MegatankHeadMoveStateEnum.MoveWait:
			break;
		}
	}

	public override void UpdateMove(float elapsedTime)
	{
		base.UpdateMove(elapsedTime);
		switch (MoveState)
		{
		case MegatankHeadMoveStateEnum.MoveWait:
			_currentWorldPosition.X -= 50f * elapsedTime;
			break;
		case MegatankHeadMoveStateEnum.MoveToScreenX:
			Utils.ChangeValueToTarget(ref _moveSpeed, 400f, 200f * elapsedTime);
			break;
		}
	}
}
