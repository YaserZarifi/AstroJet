using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class SpineGun
{
	public GameObject MyObject;

	public float AimSpeed;

	public FloatRange ShotDelayRange;

	public FloatRange BurstDelayRange;

	public IntRange BurstSizeRange;

	public IntRange AimAngleRange;

	public int MuzzleCount = 1;

	public float ShotSpeed = 500f;

	public float DeactivateAngle = -90f;

	public bool AimRangeClockwise = true;

	public bool FullCircle;

	public Vector2 FireScale;

	public float AcivateAngle = -90f;

	public SpineGunMode Mode;

	public int ZOrderOffset = -1;

	private float _aimAngle;

	private float _shotTimer;

	private GameObject _bodyObject;

	private List<GameObject> _muzzleObjectList;

	private PartData _shotData;

	private PartData _fireData;

	private int _stopState;

	private int _burstFireCount;

	private int _burstSize;

	public SpineGunStateEnum State { get; private set; }

	public bool IsFireReady { get; private set; }

	public SpineGun(GameObject myObject, string bodyId)
	{
		MyObject = myObject;
		_bodyObject = MyObject.GetChildByIdDeep(bodyId);
		if (_bodyObject == null)
		{
			GameCore.LogErr("Unable to find body object '{0}' for Spine gun '{1}'", bodyId, MyObject.Id);
			_bodyObject = MyObject;
		}
		ShotDelayRange = new FloatRange(1.5f, 2.75f);
		AimAngleRange = new IntRange(-90, 90);
		AimSpeed = 100f;
		FireScale = Vector2.One;
		_burstSize = BurstSizeRange.GetRandom();
	}

	public void RegisterEvents()
	{
		MyObject.OnSpineAnimEnd += OnAnimEnd;
		MyObject.OnSpineAnimEvent += OnSpineEvent;
		MyObject.OnKill += OnKill;
	}

	public void Init()
	{
		_muzzleObjectList = GameCore.Allocator.GetGameObjectList();
		for (int num = MuzzleCount; num > 0; num--)
		{
			GameObject childByIdDeep = MyObject.GetChildByIdDeep($"muzzle{num}");
			if (childByIdDeep != null)
			{
				_muzzleObjectList.Add(childByIdDeep);
			}
		}
		SetState(SpineGunStateEnum.Inactive);
	}

	public void SetFireDataId(string id)
	{
		_fireData = GameCore.GetPartData(id);
	}

	public void SetShotDataId(string id)
	{
		_shotData = GameCore.GetPartData(id);
	}

	public void SetState(SpineGunStateEnum state)
	{
		if (State == SpineGunStateEnum.Wreck)
		{
			return;
		}
		State = state;
		switch (state)
		{
		case SpineGunStateEnum.Inactive:
			if (!MyObject.SetSpineAnimation(0, "inactive", bLooped: false))
			{
				MyObject.SetSpineAnimation(0, "idle", bLooped: true);
			}
			break;
		case SpineGunStateEnum.Activate:
			_aimAngle = AcivateAngle;
			if (!MyObject.SetSpineAnimation(0, "activate", bLooped: false))
			{
				SetState(SpineGunStateEnum.Fire);
			}
			break;
		case SpineGunStateEnum.Fire:
			_burstFireCount++;
			MyObject.SetSpineAnimation(0, "fire", bLooped: false);
			break;
		case SpineGunStateEnum.FireWait:
			IsFireReady = false;
			if (_burstFireCount >= _burstSize && _burstSize > 0)
			{
				SetState(SpineGunStateEnum.BurstWait);
				break;
			}
			_shotTimer = ShotDelayRange.GetRandom();
			if (!MyObject.SetSpineAnimation(0, "prepare", bLooped: true))
			{
				IsFireReady = true;
			}
			break;
		case SpineGunStateEnum.BurstWait:
			IsFireReady = false;
			_shotTimer = BurstDelayRange.GetRandom();
			MyObject.SetSpineAnimation(0, "idle", bLooped: true);
			break;
		case SpineGunStateEnum.Deactivate:
			_stopState = 0;
			break;
		case SpineGunStateEnum.Done:
			MyObject.SetSpineAnimation(0, "done", bLooped: false);
			break;
		case SpineGunStateEnum.Wreck:
			MyObject.SetSpineAnimation(0, "wreck", bLooped: true);
			MyObject.DestroyRigidBody();
			break;
		}
	}

	protected void OnAnimEnd(int trackIndex)
	{
		switch (State)
		{
		case SpineGunStateEnum.Activate:
			SetState(SpineGunStateEnum.FireWait);
			break;
		case SpineGunStateEnum.Fire:
			SetState(SpineGunStateEnum.FireWait);
			break;
		case SpineGunStateEnum.FireWait:
			IsFireReady = true;
			break;
		case SpineGunStateEnum.Deactivate:
			if (_stopState == 1)
			{
				SetState(SpineGunStateEnum.Done);
			}
			break;
		case SpineGunStateEnum.BurstWait:
			break;
		}
	}

	public void Update(float elapsedTime)
	{
		switch (State)
		{
		case SpineGunStateEnum.Fire:
			UpdateAim(elapsedTime);
			break;
		case SpineGunStateEnum.FireWait:
			UpdateAim(elapsedTime);
			if (IsFireReady && Mode == SpineGunMode.Auto)
			{
				_shotTimer -= elapsedTime;
				if (_shotTimer <= 0f)
				{
					SetState(SpineGunStateEnum.Fire);
				}
			}
			break;
		case SpineGunStateEnum.BurstWait:
			UpdateAim(elapsedTime);
			if (_shotTimer <= 0f)
			{
				_burstFireCount = 0;
				_burstSize = BurstSizeRange.GetRandom();
				SetState(SpineGunStateEnum.Fire);
			}
			break;
		case SpineGunStateEnum.Deactivate:
			UpdateDeactivate(elapsedTime);
			break;
		case SpineGunStateEnum.Inactive:
		case SpineGunStateEnum.Activate:
			break;
		}
	}

	protected void UpdateAim(float elapsedTime)
	{
		if (AimSpeed <= 0f)
		{
			return;
		}
		float num = Utils.RadToDeg(MyObject.GetRotationToNearestHero() - MyObject.WorldRotation);
		if (num > 180f)
		{
			num -= 360f;
		}
		else if (num < -180f)
		{
			num += 360f;
		}
		num = Utils.ClampAngle(num, AimAngleRange, AimRangeClockwise);
		if (FullCircle)
		{
			if (_aimAngle - num > 180f)
			{
				_aimAngle -= 360f;
			}
			else if (_aimAngle - num < -180f)
			{
				_aimAngle += 360f;
			}
		}
		if (AimRangeClockwise)
		{
			Utils.ChangeValueToTarget(ref _aimAngle, num, AimSpeed * elapsedTime);
		}
		else
		{
			Utils.ChangeAngleToTarget(ref _aimAngle, num, AimSpeed * elapsedTime, ccw: true);
		}
		if (_bodyObject.SpineConnector != null)
		{
			_bodyObject.SpineConnector.SetAngle(_aimAngle, bWorld: false);
		}
	}

	protected void UpdateDeactivate(float elapsedTime)
	{
		if (_stopState == 0)
		{
			if (AimSpeed <= 0f || ((!AimRangeClockwise) ? Utils.ChangeAngleToTarget(ref _aimAngle, DeactivateAngle, AimSpeed * elapsedTime, ccw: true) : Utils.ChangeValueToTarget(ref _aimAngle, DeactivateAngle, AimSpeed * elapsedTime)))
			{
				_stopState = 1;
				MyObject.SetSpineAnimation(0, "deactivate", bLooped: false);
			}
			if (_bodyObject.SpineConnector != null && AimSpeed > 0f)
			{
				_bodyObject.SpineConnector.SetAngle(_aimAngle, bWorld: false);
			}
		}
	}

	public void OnSpineEvent(string eventName, int intParam, float floatParam, string stringParam)
	{
		for (int num = _muzzleObjectList.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = _muzzleObjectList[num];
			if (Utils.Equals(eventName, $"fire/{gameObject.Id}"))
			{
				if (_fireData != null)
				{
					_ = gameObject.WorldPosition;
					gameObject.CreateChildByPartData(_fireData, createPhysBody: true).SetScale(FireScale);
				}
				if (_shotData != null)
				{
					Vector2 worldPosition = gameObject.WorldPosition;
					GameObject gameObject2 = MyObject.MyLayer.CreateObjectByPartData(_shotData, worldPosition, Vector2.One, gameObject.WorldRotation, MyObject.WorldZOrder + ZOrderOffset);
					if (gameObject2 != null)
					{
						gameObject2.SetHitAndTargetClass(PhysClass.ShotEnemy, TargetClass.None);
						gameObject2.SetRotationVelocity(ShotSpeed, gameObject2.Rotation);
					}
				}
				break;
			}
		}
	}

	public void OnKill()
	{
		SetState(SpineGunStateEnum.Wreck);
		MyObject.OnKill -= OnKill;
		MyObject.OnSpineAnimEnd -= OnAnimEnd;
		MyObject.OnSpineAnimEvent -= OnSpineEvent;
	}
}
