using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class GravityHole : BehaviorScript
{
	private List<GameObject> _targetList;

	private float _targetTimer;

	private float _gravityRadius;

	private float _killRadiusSq;

	private PartData _killEffectData;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
	}

	public override void Init()
	{
		base.Init();
		_targetList = GameCore.Allocator.GetGameObjectList();
		_gravityRadius = Param("gravity_radius", 190f);
		_killRadiusSq = Param("kill_radius", 60f);
		_killRadiusSq *= _killRadiusSq;
		_killEffectData = GameCore.GetPartData("gravity_hole_kill_effect");
	}

	private void OnUpdate(float elapsedTime)
	{
		if (Utils.ChangeValueToTarget(ref _targetTimer, 0f, elapsedTime))
		{
			_targetTimer = 0.1f;
			List<GameObject> visibleObjectsInRangeInAllPhysLayers = MyObject.MyLevel.GetVisibleObjectsInRangeInAllPhysLayers(MyObject.WorldPosition, TargetClass.ShotEnemy, _gravityRadius, _targetList, bOlyHittable: true);
			int count = visibleObjectsInRangeInAllPhysLayers.Count;
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = visibleObjectsInRangeInAllPhysLayers[i];
				if (!gameObject.IsDestroyed && !gameObject.IsKilled)
				{
					_targetList.Add(gameObject);
				}
			}
			GameCore.Allocator.DeleteAliasingGameObjectList(visibleObjectsInRangeInAllPhysLayers);
		}
		for (int num = _targetList.Count - 1; num >= 0; num--)
		{
			GameObject gameObject2 = _targetList[num];
			if (gameObject2.IsDestroyed || gameObject2.IsKilled)
			{
				_targetList.RemoveAt(num);
			}
			else
			{
				float currentAngle = MathHelper.ToDegrees(gameObject2.Rotation);
				float num2 = MathHelper.ToDegrees(gameObject2.GetRotationToTarget(MyObject, gameObject2.Rotation));
				Utils.ChangeAngleToTarget(ref currentAngle, num2, 300f * elapsedTime, currentAngle > num2);
				if (Vector2.DistanceSquared(MyObject.WorldPosition, gameObject2.WorldPosition) <= _killRadiusSq)
				{
					gameObject2.Kill();
					gameObject2.MyLayer.CreateObjectByPartData(_killEffectData, gameObject2.Position, Vector2.One, 0f, gameObject2.ZOrder, bCreateRigidBody: false);
					_targetList.RemoveAt(num);
				}
				gameObject2.SetRotation(MathHelper.ToRadians(currentAngle));
				gameObject2.SetVelocity(Utils.GetVelocityVector2(gameObject2.Velocity.Length(), gameObject2.Rotation));
			}
		}
	}
}
