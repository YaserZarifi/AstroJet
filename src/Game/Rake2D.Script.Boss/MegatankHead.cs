using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script.Boss;

public abstract class MegatankHead
{
	public GameObject MyObject;

	public Megatank MyMegatank;

	public bool IsOnPosition;

	protected GameObject _heartObject;

	protected float _moveSpeed = 300f;

	protected float _moveTimer;

	protected MegatankHeadStateEnum _state;

	protected Vector2 _targetPosition;

	protected Vector2 _currentWorldPosition;

	protected MegatankPhaseEnum _phase;

	protected bool _isActivated;

	protected GameObject _sideGunObject;

	private Transformation _parentTrasformation;

	public MegatankHeadMoveStateEnum MoveState { get; private set; }

	public MegatankHead(GameObject go, Megatank megatank)
	{
		MyObject = go;
		MyMegatank = megatank;
		_heartObject = go.GetChildByIdDeep("heart");
		_parentTrasformation = MyObject.ParentTransformation;
	}

	public virtual void RegisterEvents()
	{
	}

	public virtual void Activate()
	{
		_isActivated = true;
	}

	public virtual void ActivateSideGun()
	{
		_sideGunObject.ChangePartState("activate");
	}

	public virtual void SetMoveState(MegatankHeadMoveStateEnum moveState)
	{
		MoveState = moveState;
		IsOnPosition = false;
		switch (MoveState)
		{
		case MegatankHeadMoveStateEnum.None:
			if (MyObject.ParentTransformation != _parentTrasformation)
			{
				MyObject.ChangeParentTransformation(_parentTrasformation);
			}
			break;
		case MegatankHeadMoveStateEnum.StartPrepare:
			_moveTimer = 0.75f;
			break;
		case MegatankHeadMoveStateEnum.Start:
			if (MyObject.ParentTransformation != null)
			{
				MyObject.ChangeParentTransformation(null);
			}
			_currentWorldPosition = MyObject.WorldPosition;
			_targetPosition = _currentWorldPosition + new Vector2(0f, -300f);
			break;
		case MegatankHeadMoveStateEnum.MoveWait:
			_moveTimer = 1f;
			break;
		case MegatankHeadMoveStateEnum.Open:
		case MegatankHeadMoveStateEnum.Close:
		case MegatankHeadMoveStateEnum.MoveToScreenX:
		case MegatankHeadMoveStateEnum.LandPrepare:
			break;
		}
	}

	public virtual void SetState(MegatankHeadStateEnum state)
	{
		_state = state;
		switch (_state)
		{
		case MegatankHeadStateEnum.MoveOnScreen:
			if (MoveState == MegatankHeadMoveStateEnum.None)
			{
				SetMoveState(MegatankHeadMoveStateEnum.StartPrepare);
			}
			break;
		case MegatankHeadStateEnum.Idle:
			if (MoveState != MegatankHeadMoveStateEnum.None)
			{
				SetMoveState(MegatankHeadMoveStateEnum.None);
			}
			break;
		}
	}

	public abstract void SetPhase(MegatankPhaseEnum phase);

	public virtual void UpdateMove(float elapsedTime)
	{
		_moveTimer -= elapsedTime;
		switch (MoveState)
		{
		case MegatankHeadMoveStateEnum.StartPrepare:
			if (_moveTimer <= 0f)
			{
				SetMoveState(MegatankHeadMoveStateEnum.Start);
			}
			break;
		case MegatankHeadMoveStateEnum.Start:
			_moveSpeed = 20f + Math.Min(300f, (_currentWorldPosition - _targetPosition).Length());
			if (Utils.ChangeValueToTarget(ref _currentWorldPosition, _targetPosition, _moveSpeed * elapsedTime))
			{
				SetMoveState(MegatankHeadMoveStateEnum.MoveWait);
			}
			MyObject.SetPosition(_currentWorldPosition);
			break;
		case MegatankHeadMoveStateEnum.MoveWait:
			if (_moveTimer <= 0f || MyObject.VirtualScreenPosition.X <= 200f)
			{
				SetMoveState(MegatankHeadMoveStateEnum.MoveToScreenX);
			}
			MyObject.SetPosition(_currentWorldPosition);
			break;
		case MegatankHeadMoveStateEnum.MoveToScreenX:
		{
			Vector2 vector = MyObject.MyLayer.MyLevel.Camera.ScreenPosToCurrentLayer(_targetPosition);
			if (Utils.ChangeValueToTarget(ref _currentWorldPosition, new Vector2(vector.X, _targetPosition.Y), _moveSpeed * elapsedTime))
			{
				SetMoveState(MegatankHeadMoveStateEnum.LandPrepare);
			}
			MyObject.SetPosition(_currentWorldPosition);
			break;
		}
		case MegatankHeadMoveStateEnum.LandPrepare:
			_targetPosition = new Vector2(MyObject.ParentObject.WorldPosition.X, MyObject.ParentObject.WorldPosition.Y - 200f);
			_moveSpeed = 20f + Math.Min(300f, (_currentWorldPosition - _targetPosition).Length());
			Utils.ChangeValueToTarget(ref _currentWorldPosition, _targetPosition, _moveSpeed * elapsedTime, MyObject.MyLayer.MyLevel.ScrollSpeed * elapsedTime);
			if ((_currentWorldPosition - _targetPosition).Length() < 1f)
			{
				MyObject.SetPosition(_targetPosition);
				IsOnPosition = true;
			}
			MyObject.SetPosition(_currentWorldPosition);
			break;
		case MegatankHeadMoveStateEnum.Land:
			_targetPosition = new Vector2(MyObject.ParentObject.WorldPosition.X, MyObject.ParentObject.WorldPosition.Y);
			_moveSpeed = 20f + Math.Min(300f, (_currentWorldPosition - _targetPosition).Length());
			Utils.ChangeValueToTarget(ref _currentWorldPosition, _targetPosition, _moveSpeed * elapsedTime, MyObject.MyLayer.MyLevel.ScrollSpeed * elapsedTime);
			if ((_currentWorldPosition - _targetPosition).Length() < 1f)
			{
				MyObject.SetPosition(_targetPosition);
				IsOnPosition = true;
			}
			else
			{
				MyObject.SetPosition(_currentWorldPosition);
			}
			break;
		}
	}

	public virtual void Update(float elapsedTime)
	{
		if (elapsedTime != 0f && _isActivated)
		{
			MegatankHeadStateEnum state = _state;
			if (state == MegatankHeadStateEnum.MoveOnScreen)
			{
				UpdateMove(elapsedTime);
			}
		}
	}
}
