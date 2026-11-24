using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script.Boss;

public class Megatank : BehaviorScript
{
	private const int ANIM_TRACK_HEAD = 2;

	public int HeartKillCount;

	public int PartKillCount;

	private MegatankPhaseEnum _phase;

	private MegatankActionEnum _action;

	private MegatankMoveEnum _move;

	private float _targetVelocityX;

	private float _stateTimer;

	private float _moveTimer;

	private float _startHeadOpeningTimer;

	private float _introTimer;

	private MegatankHead _headFront;

	private MegatankHead _headBack;

	private GameObject _launcherObject;

	private GameObject _frontGearObject;

	private GameObject _wheelDustFrontObject;

	private GameObject _wheelDustBackObject;

	private GameObject _exhaustObject;

	private GameObject _exhaustSmokeObject;

	private GameObject _skullObject;

	private GameObject _droneStarterObject;

	private GameObject _doorObject;

	private GameObject _frontTowerObject;

	private GameObject _backTowerObject;

	private bool _isActivated;

	private bool _isSkullActivated;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
		MyObject.OnSpineAnimEnd += OnSpineAnimEnd;
		MyObject.OnLink += OnLink;
		_launcherObject.OnReRegisterEvents += OnReRegisterEventsKillCounter;
		_exhaustObject.OnReRegisterEvents += OnReRegisterEventsKillCounter;
		_headFront?.RegisterEvents();
		_headBack?.RegisterEvents();
	}

	public void OnReRegisterEventsKillCounter(GameObject go)
	{
		go.OnKill += RaisePartKillCount;
	}

	public void RaisePartKillCount()
	{
		PartKillCount++;
		if (PartKillCount == 2)
		{
			_headFront.ActivateSideGun();
			_headBack.ActivateSideGun();
		}
		if (PartKillCount == 4 && _phase != MegatankPhaseEnum.Skull && _phase != MegatankPhaseEnum.FlyingHeads)
		{
			SetPhase(MegatankPhaseEnum.FlyingHeads);
		}
	}

	public override void Init()
	{
		SetAction(MegatankActionEnum.Idle);
		SetMove(MegatankMoveEnum.Left);
		_introTimer = Param("intro_timer", 3000f) / 1000f;
		_frontTowerObject = MyObject.GetChildByIdDeep("tower_front");
		_backTowerObject = MyObject.GetChildByIdDeep("tower_back");
		_launcherObject = MyObject.GetChildByIdDeep("launcher_front");
		_exhaustObject = MyObject.GetChildByIdDeep("ex");
		_exhaustSmokeObject = MyObject.GetChildByIdDeep("ex_smoke");
		_skullObject = MyObject.GetChildByIdDeep("megatank_skull");
		_droneStarterObject = MyObject.GetChildByIdDeep("drone_starter");
		_doorObject = MyObject.GetChildByIdDeep("door");
		_wheelDustFrontObject = MyObject.GetChildByIdDeep("wheel_dust_front");
		_wheelDustBackObject = MyObject.GetChildByIdDeep("wheel_dust_back");
		_frontGearObject = MyObject.GetChildByIdDeep("front_gear_coli");
		GameObject childByIdDeep = MyObject.GetChildByIdDeep("megatank_head_front");
		_headFront = new MegatankHeadFront(childByIdDeep, this);
		childByIdDeep = MyObject.GetChildByIdDeep("megatank_head_back");
		_headBack = new MegatankHeadBack(childByIdDeep, this);
		SetPhase(MegatankPhaseEnum.Intro);
	}

	private void OnLink(GameObject senderobject, string eventName, int value)
	{
		if (senderobject == MyObject && Utils.Equals(eventName, "end_intro"))
		{
			SetPhase(MegatankPhaseEnum.Basic);
		}
	}

	private void SetPhase(MegatankPhaseEnum phase)
	{
		switch (phase)
		{
		case MegatankPhaseEnum.Intro:
			_startHeadOpeningTimer = 9999f;
			_frontTowerObject.ChangePartStateToAll("inactive");
			_backTowerObject.ChangePartStateToAll("inactive");
			_launcherObject.ChangePartStateToAll("inactive");
			break;
		case MegatankPhaseEnum.Basic:
			_startHeadOpeningTimer = 10f;
			MyObject.SetSpineAnimation(0, "run", bLooped: true);
			_frontTowerObject.ChangePartStateToAll("inactive");
			_backTowerObject.ChangePartStateToAll("inactive");
			if (_launcherObject.IsAlive)
			{
				_launcherObject.ChangePartStateToAll("idle");
			}
			break;
		case MegatankPhaseEnum.FlyingHeads:
			_frontTowerObject.ChangePartState("activate");
			_backTowerObject.ChangePartState("activate");
			if (_launcherObject.IsAlive && _phase != MegatankPhaseEnum.FlyingHeads && _phase != MegatankPhaseEnum.Basic)
			{
				_launcherObject.ChangePartStateToAll("idle");
			}
			break;
		}
		_headFront.SetPhase(phase);
		_headBack.SetPhase(phase);
		_phase = phase;
	}

	private void SetAction(MegatankActionEnum action)
	{
		_action = action;
		switch (_action)
		{
		case MegatankActionEnum.HeadFly:
			_headFront?.SetState(MegatankHeadStateEnum.MoveOnScreen);
			_headBack?.SetState(MegatankHeadStateEnum.MoveOnScreen);
			break;
		case MegatankActionEnum.Idle:
			_stateTimer = Utils.GetRandom(1, 2);
			break;
		case MegatankActionEnum.HeadOpen:
			_stateTimer = Utils.GetRandom(2, 3);
			MyObject.SetSpineAnimation(2, "head_open", bLooped: false);
			_headFront?.SetMoveState(MegatankHeadMoveStateEnum.Open);
			_headBack?.SetMoveState(MegatankHeadMoveStateEnum.Open);
			if (MyObject.VirtualScreenPosition.X > 960f && _droneStarterObject.IsAlive)
			{
				_droneStarterObject.ChangePartState("loop_1");
			}
			break;
		case MegatankActionEnum.HeadClose:
			_stateTimer = Utils.GetRandom(2, 3);
			MyObject.SetSpineAnimation(2, "head_close", bLooped: false);
			_headFront?.SetMoveState(MegatankHeadMoveStateEnum.Close);
			_headBack?.SetMoveState(MegatankHeadMoveStateEnum.Close);
			_droneStarterObject.ChangePartState("inactive");
			break;
		}
	}

	private void OnSpineAnimEnd(int trackIndex)
	{
		if (trackIndex == 2 && _action == MegatankActionEnum.HeadClose)
		{
			_headFront?.SetMoveState(MegatankHeadMoveStateEnum.None);
			_headBack?.SetMoveState(MegatankHeadMoveStateEnum.None);
		}
	}

	private void ActivateSkull()
	{
		_isSkullActivated = true;
		_skullObject.ChangePartState("activate");
		_doorObject.ChangePartState("idle");
		_droneStarterObject.ChangePartState("inactive");
		SetAction(MegatankActionEnum.Idle);
		SetMove(MegatankMoveEnum.Right);
	}

	private void SetMove(MegatankMoveEnum move)
	{
		_move = move;
		switch (_move)
		{
		case MegatankMoveEnum.Stay:
			_targetVelocityX = MyLayer.ScrollSpeed.X;
			_moveTimer = Utils.GetRandom(1.5f, 3f);
			break;
		case MegatankMoveEnum.StayOnScreen:
			_targetVelocityX = MyLayer.ScrollSpeed.X;
			break;
		case MegatankMoveEnum.Left:
			_targetVelocityX = 0f;
			break;
		case MegatankMoveEnum.Right:
			_targetVelocityX = 300f + MyLayer.ScrollSpeed.X;
			break;
		}
	}

	private void UpdateDust(float elapsedTime)
	{
		Color color = new Color(_wheelDustFrontObject.Color.R, _wheelDustFrontObject.Color.G, _wheelDustFrontObject.Color.B, (byte)Math.Min(Math.Abs(MyObject.Velocity.X) / 100f * 120f, 120f));
		_wheelDustFrontObject.Color = color;
		_wheelDustBackObject.Color = color;
	}

	private void UpdateMove(float elapsedTime)
	{
		if (MyObject.MyPath != null)
		{
			return;
		}
		_moveTimer -= elapsedTime;
		switch (_move)
		{
		case MegatankMoveEnum.Stay:
			if (_moveTimer <= 0f)
			{
				if (MyObject.VirtualScreenPosition.X <= 600f)
				{
					SetMove(MegatankMoveEnum.Right);
				}
				else
				{
					SetMove(MegatankMoveEnum.Left);
				}
			}
			break;
		case MegatankMoveEnum.Left:
			if (MyObject.VirtualScreenPosition.X <= 600f)
			{
				SetMove(MegatankMoveEnum.Stay);
			}
			break;
		case MegatankMoveEnum.Right:
			if (MyObject.VirtualScreenPosition.X >= 1420f)
			{
				SetMove(MegatankMoveEnum.Stay);
			}
			break;
		}
		float currentValue = MyObject.VelocityX;
		Utils.ChangeValueToTarget(ref currentValue, _targetVelocityX, 180f * elapsedTime);
		MyObject.SetVelocity(currentValue, 0f);
		float exSmokeScale = 1f + MathHelper.Clamp(Math.Abs(currentValue) / 300f, 0.5f, 1.5f);
		_exhaustSmokeObject.ChildObjectListFront?.ForEach((GameObject child) =>
		{
			child.SetScale(exSmokeScale);
		});
	}

	private void UpdateFrontHead(float elapsedTime)
	{
		_headFront.Update(elapsedTime);
	}

	protected void Update(float elapsedTime)
	{
		UpdateMove(elapsedTime);
		UpdateDust(elapsedTime);
		if (_headFront != null && (_headFront.MyObject.IsDestroyed || _headFront.MyObject.IsKilled))
		{
			_headFront.MyObject.Destroy();
			_headFront = null;
		}
		if (_headBack != null && (_headBack.MyObject.IsDestroyed || _headBack.MyObject.IsKilled))
		{
			_headBack.MyObject.Destroy();
			_headBack = null;
		}
		if (_headFront == null && _headBack == null && !_isSkullActivated)
		{
			ActivateSkull();
		}
		if (_isActivated)
		{
			_stateTimer -= elapsedTime;
		}
		else if (MyObject.Timer >= 2f)
		{
			_headFront?.Activate();
			_headBack?.Activate();
			_isActivated = true;
		}
		if (_headFront != null)
		{
			UpdateFrontHead(elapsedTime);
		}
		_headBack?.Update(elapsedTime);
		switch (_phase)
		{
		case MegatankPhaseEnum.Intro:
			_introTimer -= elapsedTime;
			if (_introTimer <= 0f)
			{
				SetPhase(MegatankPhaseEnum.Basic);
			}
			break;
		case MegatankPhaseEnum.Basic:
			_startHeadOpeningTimer -= elapsedTime;
			break;
		}
		switch (_action)
		{
		case MegatankActionEnum.HeadFly:
			if ((_headFront != null && _headFront.MoveState == MegatankHeadMoveStateEnum.LandPrepare) || _headBack == null || _headBack.MoveState == MegatankHeadMoveStateEnum.LandPrepare)
			{
				SetMove(MegatankMoveEnum.StayOnScreen);
			}
			if ((_headBack == null || (_headBack.MoveState == MegatankHeadMoveStateEnum.LandPrepare && _headBack.IsOnPosition)) && (_headFront == null || (_headFront.IsOnPosition && _headFront.MoveState == MegatankHeadMoveStateEnum.LandPrepare)))
			{
				_headFront?.SetMoveState(MegatankHeadMoveStateEnum.Land);
				_headBack?.SetMoveState(MegatankHeadMoveStateEnum.Land);
			}
			else if ((_headBack == null || (_headBack.MoveState == MegatankHeadMoveStateEnum.Land && _headBack.IsOnPosition)) && (_headFront == null || (_headFront.MoveState == MegatankHeadMoveStateEnum.Land && _headFront.IsOnPosition)))
			{
				_headFront?.SetState(MegatankHeadStateEnum.Idle);
				_headBack?.SetState(MegatankHeadStateEnum.Idle);
				SetAction(MegatankActionEnum.Idle);
				SetMove(MegatankMoveEnum.Stay);
				_moveTimer = 0f;
			}
			break;
		case MegatankActionEnum.Idle:
			if (!(_stateTimer <= 0f) || _isSkullActivated)
			{
				break;
			}
			switch (Utils.GetRandom(1))
			{
			case 0:
				if (_phase == MegatankPhaseEnum.FlyingHeads && !_isSkullActivated)
				{
					SetAction(MegatankActionEnum.HeadFly);
				}
				break;
			case 1:
				if (_startHeadOpeningTimer <= 0f)
				{
					SetAction(MegatankActionEnum.HeadOpen);
				}
				break;
			}
			break;
		case MegatankActionEnum.HeadOpen:
			if (_stateTimer <= 0f)
			{
				SetAction(MegatankActionEnum.HeadClose);
			}
			break;
		case MegatankActionEnum.HeadClose:
			if (_stateTimer <= 0f)
			{
				SetAction(MegatankActionEnum.Idle);
			}
			break;
		}
	}
}
