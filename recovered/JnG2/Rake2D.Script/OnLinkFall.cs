using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnLinkFall : OnLinkBehavior
{
	private bool _isFalling;

	private float _startFallTime;

	private float _impactScale;

	private List<PartData> _impactDataList;

	private List<PartData> _exploDataList;

	private FloatRange _rotationSpeedRange;

	private FloatRange _radialVelocityRange;

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyObject.OnCollision += OnCollision;
		MyObject.OnBeforeCollision += OnBeforeCollision;
	}

	public override void Init()
	{
		base.Init();
		string[] array = ParamStringArray("impact_object_id");
		if (array != null)
		{
			_impactDataList = new List<PartData>();
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (Utils.Equals(text, "none"))
				{
					_impactDataList.Add(null);
					continue;
				}
				PartData partData = GameCore.GetPartData(text);
				if (partData != null)
				{
					_impactDataList.Add(partData);
				}
			}
		}
		_impactScale = Param("impact_scale", 100f) / 100f;
		array = ParamStringArray("explo_object_id");
		if (array != null)
		{
			_exploDataList = new List<PartData>();
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				if (Utils.Equals(text2, "none"))
				{
					_exploDataList.Add(null);
					continue;
				}
				PartData partData2 = GameCore.GetPartData(text2);
				if (partData2 != null)
				{
					_exploDataList.Add(partData2);
				}
			}
		}
		_rotationSpeedRange = ParamRange("rotation_speed", 10f, 50f);
		_radialVelocityRange = ParamRange("radial_velocity", 100f, 200f);
	}

	protected override void DoAction(GameObject senderObject, string linkIn, int value)
	{
		if (_isFalling)
		{
			return;
		}
		_isFalling = true;
		MyObject.IncreaseLevelStatsAsKill();
		MyObject.DestroyOutOfView = OutOfFrame.OnLeft;
		MyObject.ActiveMode = ActiveModeEnum.AlwaysActive;
		MyObject.SetHitAndTargetClass(PhysClass.Ruins, TargetClass.None);
		Vector2 velocityVector = Utils.GetVelocityVector2(_radialVelocityRange.GetRandom(), Utils.GetRotationTo(senderObject.WorldPosition, MyObject.WorldPosition) + Utils.GetRandom(-0.2f, 0.2f));
		if (MyObject.RigidBody == null)
		{
			MyObject.CreatePhysBody = true;
			Vector2 position = MyObject.Position;
			if (MyObject.IsMainPart)
			{
				MyObject.Position = MyObject.GetMainLayerPosition();
				MyObject.ResetPrecalculations();
			}
			MyObject.CreateRigidBodyByData();
			MyObject.Position = position;
		}
		if (MyObject.RigidBody != null)
		{
			MyObject.RigidBody.IsStatic = false;
			MyObject.RigidBody.RotationSpeed = Utils.DegToRad(_rotationSpeedRange.GetRandom() * (float)((!(velocityVector.X < 0f)) ? 1 : (-1)));
		}
		MyObject.SetVelocity(velocityVector);
		MyObject.DestroyOutOfLevel = (OutOfFrame)11;
		_startFallTime = MyObject.Timer;
	}

	public void OnBeforeCollision(GameObject collisionObject)
	{
		if (_isFalling && MyObject.Timer - _startFallTime < 0.1f && collisionObject.WorldPosition.Y <= MyObject.WorldPosition.Y)
		{
			MyObject.IgnoreCollisionWith(collisionObject);
		}
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (MyObject.IsDestroyed || !_isFalling)
		{
			return;
		}
		MyObject.Destroy();
		if (_impactDataList != null)
		{
			PartData randomListItem = Utils.GetRandomListItem(_impactDataList, null);
			if (randomListItem != null)
			{
				MyObject.MyLayer.CreateObjectByPartData(randomListItem, contactPoint - MyObject.MyCamera.Position * (Vector2.One - MyLayer.ScrollFactor), MyObject.WorldScale * _impactScale, 0f, MyObject.ZOrder);
			}
		}
		if (_exploDataList != null)
		{
			PartData randomListItem2 = Utils.GetRandomListItem(_exploDataList, null);
			if (randomListItem2 != null)
			{
				MyObject.MyLayer.CreateObjectByPartData(randomListItem2, MyObject.WorldPosition, MyObject.WorldScale, 0f, MyObject.ZOrder);
			}
		}
	}
}
