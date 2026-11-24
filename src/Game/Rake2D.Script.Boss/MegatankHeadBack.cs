using Microsoft.Xna.Framework;

namespace Rake2D.Script.Boss;

public class MegatankHeadBack : MegatankHead
{
	private GameObject _miniGunObject;

	private GameObject _launcherObject;

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		_miniGunObject.OnReRegisterEvents += MyMegatank.OnReRegisterEventsKillCounter;
		_launcherObject.OnReRegisterEvents += MyMegatank.OnReRegisterEventsKillCounter;
	}

	public MegatankHeadBack(GameObject go, Megatank megatank)
		: base(go, megatank)
	{
		_miniGunObject = MyObject.GetChildByIdDeep("minigun");
		_sideGunObject = MyObject.GetChildByIdDeep("side_gun");
		_launcherObject = MyObject.GetChildByIdDeep("launcher");
	}

	public override void SetPhase(MegatankPhaseEnum phase)
	{
		switch (phase)
		{
		case MegatankPhaseEnum.Intro:
			_sideGunObject.ChangePartStateToAll("inactive");
			_miniGunObject.ChangePartStateToAll("inactive");
			_launcherObject.ChangePartStateToAll("inactive");
			_heartObject.ChangePartStateToAll("inactive");
			break;
		case MegatankPhaseEnum.Basic:
			if (_phase != MegatankPhaseEnum.Intro)
			{
				_sideGunObject.ChangePartStateToAll("inactive");
				if (_miniGunObject.IsAlive)
				{
					_miniGunObject.ChangePartStateToAll("inactive");
				}
			}
			if (_phase == MegatankPhaseEnum.Intro && _launcherObject.IsAlive)
			{
				_launcherObject.ChangePartStateToAll("idle");
			}
			break;
		case MegatankPhaseEnum.FlyingHeads:
			if (_phase == MegatankPhaseEnum.Intro && _launcherObject.IsAlive)
			{
				_launcherObject.ChangePartStateToAll("idle");
			}
			if (_phase != MegatankPhaseEnum.Intro && _miniGunObject.IsAlive)
			{
				_miniGunObject.ChangePartStateToAll("inactive");
			}
			break;
		}
		_phase = phase;
	}

	public override void Activate()
	{
		base.Activate();
	}

	public override void SetMoveState(MegatankHeadMoveStateEnum moveState)
	{
		base.SetMoveState(moveState);
		switch (MoveState)
		{
		case MegatankHeadMoveStateEnum.None:
			_heartObject.ChangePartStateToAll("inactive");
			break;
		case MegatankHeadMoveStateEnum.Open:
			_heartObject.ChangePartStateToAll("idle");
			break;
		case MegatankHeadMoveStateEnum.Start:
			_heartObject.ChangePartStateToAll("idle");
			break;
		case MegatankHeadMoveStateEnum.MoveToScreenX:
			_targetPosition = new Vector2(1700f, MyObject.ParentObject.WorldPosition.Y - 200f);
			break;
		case MegatankHeadMoveStateEnum.MoveWait:
			if (_miniGunObject.IsAlive)
			{
				_miniGunObject.ChangePartStateToAll("idle");
			}
			break;
		case MegatankHeadMoveStateEnum.Land:
			if (_miniGunObject.IsAlive)
			{
				_miniGunObject.ChangePartStateToAll("deactivate");
			}
			break;
		case MegatankHeadMoveStateEnum.Close:
		case MegatankHeadMoveStateEnum.StartPrepare:
		case MegatankHeadMoveStateEnum.LandPrepare:
			break;
		}
	}

	public override void UpdateMove(float elapsedTime)
	{
		base.UpdateMove(elapsedTime);
		switch (MoveState)
		{
		case MegatankHeadMoveStateEnum.MoveWait:
			_currentWorldPosition.X += 50f * elapsedTime;
			break;
		case MegatankHeadMoveStateEnum.MoveToScreenX:
			Utils.ChangeValueToTarget(ref _moveSpeed, 400f, 200f * elapsedTime);
			break;
		}
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
	}
}
