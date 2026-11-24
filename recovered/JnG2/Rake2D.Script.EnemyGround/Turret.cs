using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script.EnemyGround;

public class Turret : BehaviorScript
{
	private TurretStateEnum _state;

	private float _activateX;

	private float _deactivateX;

	private float _aimAngle;

	private FloatRange _shotDelayRange;

	private IntRange _aimAngleRange;

	private float _shotTimer;

	private float _aimSpeed;

	private GameObject _bodyObject;

	private List<GameObject> _muzzleObjectList;

	private PartData _shotData;

	private PartData _fireData;

	private float _shotSpeed;

	private float _deactivate_angle;

	private int _stopState;

	private Vector2 _fireScale;

	private Vector2 _shotScale;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		MyObject.OnSpineAnimCompleted += OnAnimComplete;
		MyObject.OnSpineAnimEvent += OnSpineEvent;
		MyObject.OnKill += OnKill;
	}

	public override void Init()
	{
		base.Init();
		_shotData = GameCore.GetPartData(ParamString("shot_id"));
		_shotDelayRange = ParamRange("shot_delay") / 1000f;
		_shotSpeed = Param("shot_speed", 500f);
		_fireData = GameCore.GetPartData(ParamString("fire_id"));
		_fireScale = ParamVector2("fire_scale", 100f, 100f) / 100f;
		_shotScale = ParamVector2("shot_scale", 100f, 100f) / 100f;
		_aimAngleRange = ParamIntRange("aim_arc", -90, 90);
		_aimSpeed = Param("aim_speed", 360f);
		_bodyObject = MyObject.GetChildByIdDeep("body");
		_muzzleObjectList = GameCore.Allocator.GetGameObjectList();
		for (int num = ParamInt("muzzle_count"); num > 0; num--)
		{
			GameObject childByIdDeep = MyObject.GetChildByIdDeep($"muzzle{num}");
			if (childByIdDeep != null)
			{
				_muzzleObjectList.Add(childByIdDeep);
			}
		}
		SetState(TurretStateEnum.Inactive);
	}

	private void SetState(TurretStateEnum state)
	{
		switch (state)
		{
		case TurretStateEnum.Inactive:
			_activateX = ParamRandom("activate_x", 1920f);
			if (!MyObject.SetSpineAnimation(0, "inactive", bLooped: false))
			{
				MyObject.SetSpineAnimation(0, "idle", bLooped: false);
			}
			break;
		case TurretStateEnum.Activate:
			MyObject.PlaySound(ParamRandomString("activate_sound"));
			if (!MyObject.SetSpineAnimation(0, "activate", bLooped: false))
			{
				SetState(TurretStateEnum.Fire);
			}
			break;
		case TurretStateEnum.Fire:
			_deactivateX = ParamRandom("deactivate_x", 0f);
			_aimAngle = Param("activate_angle", -90f);
			_shotTimer = _shotDelayRange.GetRandom();
			MyObject.SetSpineAnimation(0, "idle", bLooped: false);
			break;
		case TurretStateEnum.Deactivate:
			_deactivate_angle = Param("deactivate_angle", -90f);
			_stopState = 0;
			break;
		case TurretStateEnum.Done:
			MyObject.SetSpineAnimation(0, "done", bLooped: false);
			break;
		}
		_state = state;
	}

	protected void OnUpdate(float elapsedTime)
	{
		switch (_state)
		{
		case TurretStateEnum.Inactive:
			if (MyObject.VirtualScreenPosition.X <= _activateX)
			{
				SetState(TurretStateEnum.Activate);
			}
			break;
		case TurretStateEnum.Fire:
			UpdateFire(elapsedTime);
			break;
		case TurretStateEnum.Deactivate:
			UpdateDeactivate(elapsedTime);
			break;
		case TurretStateEnum.Activate:
			break;
		}
	}

	protected void UpdateFire(float elapsedTime)
	{
		float num = Utils.RadToDeg(MyObject.GetRotationToNearestHero() - MyObject.WorldRotation);
		if (num > 180f)
		{
			num -= 360f;
		}
		else if (num < -180f)
		{
			num += 360f;
		}
		num = Utils.Clamp(num, _aimAngleRange);
		if (_aimAngle - num > 360f)
		{
			_aimAngle -= 360f;
		}
		else if (_aimAngle - num < -360f)
		{
			_aimAngle += 360f;
		}
		Utils.ChangeValueToTarget(ref _aimAngle, num, _aimSpeed * elapsedTime);
		if (_bodyObject.SpineConnector != null)
		{
			_bodyObject.SpineConnector.SetAngle(_aimAngle, bWorld: false);
		}
		_shotTimer -= elapsedTime;
		if (MyObject.VirtualScreenPosition.X <= _deactivateX)
		{
			SetState(TurretStateEnum.Deactivate);
		}
		else if (_shotTimer <= 0f)
		{
			MyObject.SetSpineAnimation(0, "fire", bLooped: false);
			_shotTimer = _shotDelayRange.GetRandom();
		}
	}

	public void UpdateDeactivate(float elapsedTime)
	{
		if (_stopState == 0)
		{
			if (Utils.ChangeValueToTarget(ref _aimAngle, _deactivate_angle, _aimSpeed * elapsedTime))
			{
				_stopState = 1;
				MyObject.PlaySound(ParamRandomString("deactivate_sound"));
				MyObject.SetSpineAnimation(0, "deactivate", bLooped: false);
			}
			if (_bodyObject.SpineConnector != null)
			{
				_bodyObject.SpineConnector.SetAngle(_aimAngle, bWorld: false);
			}
		}
	}

	protected void OnAnimComplete(int trackIndex, int loopCount)
	{
		switch (_state)
		{
		case TurretStateEnum.Activate:
			SetState(TurretStateEnum.Fire);
			break;
		case TurretStateEnum.Deactivate:
			if (_stopState == 1)
			{
				SetState(TurretStateEnum.Done);
			}
			break;
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
					gameObject.CreateChildByPartData(_fireData, createPhysBody: true).SetScale(_fireScale);
				}
				Vector2 worldPosition = gameObject.WorldPosition;
				GameObject gameObject2 = MyObject.MyLayer.CreateObjectByPartData(_shotData, worldPosition, _shotScale, gameObject.WorldRotation, MyObject.ZOrder - 1);
				gameObject2.SetRotationVelocity(_shotSpeed, gameObject2.Rotation);
				MyObject.PlaySound(ParamRandomString("shot_sound"));
				break;
			}
		}
	}

	public void OnKill()
	{
		MyObject.SetSpineAnimation(0, "wreck", bLooped: true);
		_state = TurretStateEnum.Wreck;
		MyObject.OnKill -= OnKill;
		MyObject.OnUpdate -= OnUpdate;
		MyObject.OnSpineAnimCompleted -= OnAnimComplete;
		MyObject.OnSpineAnimEvent -= OnSpineEvent;
	}
}
