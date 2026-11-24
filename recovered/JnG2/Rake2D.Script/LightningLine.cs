using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class LightningLine : BehaviorScript
{
	public GameObject EndObject;

	private bool _isBuilded;

	private PartData _hitEffectObjectData;

	protected PhysClass _hitTo;

	protected TargetClass _extraTargetClass;

	protected TargetClass _damageClass;

	protected float _widthHalf = 30f;

	protected float _maxLength;

	public override void RegisterEvents()
	{
		if (MyObject.Emitter != null)
		{
			MyObject.OnUpdate += OnUpdate;
			if (_doInEditor)
			{
				MyObject.OnEditorUpdate += OnUpdate;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		_maxLength = Param("max_length", 2000f);
		_hitEffectObjectData = GameCore.GetPartData(ParamString("hit_object", "undefined"));
		string[] array = ParamStringArray("target_class");
		if (array != null)
		{
			TargetClass targetClassByStringArray = DataUtils.GetTargetClassByStringArray(array);
			_hitTo = DataUtils.GetPhysClassByTargetClass(targetClassByStringArray);
		}
		else
		{
			_hitTo = (PhysClass)43004984;
		}
		array = ParamStringArray("damage_class");
		if (array != null)
		{
			_damageClass = DataUtils.GetTargetClassByStringArray(array);
		}
		else
		{
			_damageClass = TargetClass.Undefined;
		}
		if (MyObject.MyItem != null && MyObject.MyItem.GetBehaviorByType(typeof(ItemWeapon)) is ItemWeapon { ExtraHitTo: not PhysClass.Undefined } itemWeapon)
		{
			_hitTo |= itemWeapon.ExtraHitTo;
			if ((itemWeapon.ExtraHitTo & PhysClass.MissileEnemy) != 0)
			{
				_extraTargetClass |= TargetClass.MissileEnemy;
			}
		}
		if (MyObject.Emitter == null)
		{
			LogErr("No emitter defined");
		}
		else
		{
			MyObject.Emitter.IsEmitedByBehavior = true;
		}
		string text = ParamString("end_object");
		if (text != null)
		{
			_hitEffectObjectData = GameCore.GetPartData(text);
		}
	}

	protected void Build(Vector2 startPos, Vector2 endPos, GameObject targetObject)
	{
		MyObject.Emitter.Reset();
		Vector2 vector = Vector2.Normalize(endPos - startPos);
		float distance = Utils.GetDistance(startPos, endPos);
		float num = 60f;
		Matrix matrix = Matrix.CreateRotationZ(Utils.GetRotationFromZero(vector));
		int num2 = (int)(distance / num);
		float num3 = ((num2 <= 0) ? 10f : (distance / (float)num2));
		float num4 = Utils.GetRandom(40, 100, bRandonNegative: true);
		Vector2 position = startPos;
		Particle particle = MyObject.Emitter.Emit(bIgnoreLoopCount: true);
		if (particle == null)
		{
			return;
		}
		particle.Position = position;
		particle.SpriteColor = new Color(particle.SpriteColor, 0);
		if (num2 > 1)
		{
			float num5 = (float)Math.PI * 2f / (float)num2 * Utils.GetRandom(1f, 5f);
			float random = Utils.GetRandom((float)Math.PI * -2f, (float)Math.PI * 2f);
			float num6 = (float)num2 * 1.5f;
			for (int i = 0; i < num2 - 1; i++)
			{
				position += vector * num3;
				particle = MyObject.Emitter.Emit(bIgnoreLoopCount: true);
				particle.Position = position;
				particle.Position += Vector2.Transform(new Vector2(Utils.Sin(random + num5 * (float)i) * Utils.GetRandom(num6 / 2f, num6), 0f), matrix);
				particle.StartupVelocity = Vector2.Transform(new Vector2(num4 + (float)Utils.GetRandom(20, 90, bRandonNegative: true), i * -10 + Utils.GetRandom(-30, -90)), matrix);
			}
		}
		Particle particle2 = MyObject.Emitter.Emit(bIgnoreLoopCount: true);
		particle2.Position = endPos;
		particle2.SpriteColor = new Color(particle2.SpriteColor, 0);
		particle2.StartupVelocity = particle.StartupVelocity * 1.2f;
		MyObject.Emitter.StripLine.ParentTransformation = null;
		MyObject.Emitter.StripLine.IsTextureCoordYFixed = true;
		_isBuilded = true;
	}

	protected void OnUpdate(float elapsedTime)
	{
		if (!_isBuilded)
		{
			if (MyObject.LinkedObjectList != null && MyObject.LinkedObjectList.Count > 0)
			{
				if (MyObject.IsMainPart)
				{
					MyObject.SetRotation(MyObject.GetRotationToTarget(MyObject.LinkedObjectList[0]));
				}
				else
				{
					MyObject.SetRotation(MyObject.GetRotationToTarget(MyObject.LinkedObjectList[0]) - MyObject.ParentObject.WorldRotation + Utils.GetRandom(-0.02f, 0.02f));
				}
			}
			Vector2 mainLayerPosition = MyObject.GetMainLayerPosition();
			Vector2 vector = ((EndObject != null) ? (EndObject.WorldPosition + new Vector2(100f, 0f)) : mainLayerPosition);
			List<RayCastResult> rayCastResultList = GetRayCastResultList(vector);
			Build(mainLayerPosition, vector, EndObject);
			List<GameObject> gameObjectList = GameCore.Allocator.GetGameObjectList();
			foreach (RayCastResult item in rayCastResultList)
			{
				GameObject targetObject = item.TargetObject;
				GameObject mainPart = targetObject.GetMainPart();
				if (!gameObjectList.Contains(mainPart))
				{
					gameObjectList.Add(mainPart);
					if (MyObject.MyItem != null)
					{
						MyObject.MyItem.DoOnHitCollision(MyObject, targetObject, item.CollisionPosition, item.CollisionNormal, MyObject.Damage);
					}
					if (_damageClass == TargetClass.Undefined || (targetObject.TargetClass & _damageClass) != 0)
					{
						targetObject.Hit(MyObject.Damage, MyObject.DamageType, MyObject);
					}
					if (!targetObject.IsKilled && !targetObject.IsDestroyed)
					{
						targetObject.DoOnCollision(EndObject, item.CollisionPosition, item.CollisionNormal);
					}
					if (_hitEffectObjectData != null)
					{
						MyObject.MyLayer.CreateObjectByPartData(_hitEffectObjectData, item.CollisionPosition, MyObject.WorldScale, MyObject.WorldRotation);
					}
				}
			}
			GameCore.Allocator.DeleteAliasingGameObjectList(gameObjectList);
			MyLevel.ShockHit(MyObject, vector, 60f, TargetClass.Man, MyObject.DamageType, MyObject.Damage, 100f, null, onlyHitable: false, wantResults: false);
		}
		if (MyObject.Emitter == null || MyObject.Emitter.StripLine == null)
		{
			return;
		}
		int count = MyObject.Emitter.StripLine.ParticleList.Count;
		if (count <= 0)
		{
			return;
		}
		Particle particle = MyObject.Emitter.StripLine.ParticleList[0];
		Vector2 vector2 = MyObject.WorldPosition - particle.Position;
		particle.Position = MyObject.WorldPosition;
		int num = MyObject.Emitter.StripLine.ParticleList.Count / 2;
		for (int i = 1; i < num; i++)
		{
			MyObject.Emitter.StripLine.ParticleList[i].Position += vector2 / (i / 4 + 1);
		}
		Particle particle2 = MyObject.Emitter.StripLine.ParticleList[count - 1];
		particle2.StartupColor = new Color(particle2.StartupColor, 0);
		if (EndObject != null)
		{
			vector2 = EndObject.GetMainLayerPosition() - particle2.Position;
			EndObject.ResetPrecalculations();
			particle2.Position = EndObject.GetMainLayerPosition();
			num = MyObject.Emitter.StripLine.ParticleList.Count / 2;
			for (int j = 1; j <= num; j++)
			{
				MyObject.Emitter.StripLine.ParticleList[MyObject.Emitter.StripLine.ParticleList.Count - j - 1].Position += vector2 / (j / 4 + 1);
			}
			if (count > 1)
			{
				Particle particle3 = MyObject.Emitter.StripLine.ParticleList[count - 2];
				float rotationTo = Utils.GetRotationTo(particle2.WorldPosition, particle3.WorldPosition);
				EndObject.SetRotation(rotationTo);
			}
		}
	}

	protected List<RayCastResult> GetRayCastResultList(Vector2 endPosition)
	{
		List<RayCastResult> rayCastResultList = MyObject.MyLevel.GetRayCastResultList(MyObject.WorldPosition, endPosition, _hitTo);
		List<GameObject> gameObjectList = GameCore.Allocator.GetGameObjectList();
		foreach (RayCastResult item in rayCastResultList)
		{
			gameObjectList.Add(item.TargetObject);
		}
		rayCastResultList.AddList(MyObject.MyLevel.GetRayCastResultList(Utils.GetParallelPoint(MyObject.WorldPosition, MyObject.WorldRotation, 0f - _widthHalf), Utils.GetParallelPoint(endPosition, MyObject.WorldRotation, 0f - _widthHalf), _hitTo, gameObjectList));
		GameCore.Allocator.DeleteAliasingGameObjectList(gameObjectList);
		rayCastResultList.OrderBy((RayCastResult rcr) => Vector2.Distance(MyObject.WorldPosition, rcr.CollisionPosition));
		return rayCastResultList;
	}
}
