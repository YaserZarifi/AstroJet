using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class Lightning : BehaviorScript
{
	private bool _isBuilded;

	private PartData _endEffectObjectData;

	private GameObject _endEffectObject;

	protected PhysClass _hitTo;

	protected TargetClass _extraTargetClass;

	protected TargetClass _damageClass;

	protected float _widthHalf = 30f;

	protected bool _isChainCreated;

	protected GameObject _targetObject;

	protected float _chainDelay;

	protected int _maxChainCount;

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
		_maxChainCount = ParamInt("chain_count", 10);
		_maxLength = Param("max_length", 2000f);
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
		if (MyObject.MyItem != null)
		{
			ItemWeapon itemWeapon = MyObject.MyItem.GetBehaviorByType(typeof(ItemWeapon)) as ItemWeapon;
			if (itemWeapon.ExtraHitTo != PhysClass.Undefined)
			{
				_hitTo |= itemWeapon.ExtraHitTo;
				if ((itemWeapon.ExtraHitTo & PhysClass.MissileEnemy) != 0)
				{
					_extraTargetClass |= TargetClass.MissileEnemy;
				}
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
			_endEffectObjectData = GameCore.GetPartData(text);
		}
	}

	protected void Build(Vector2 startPos, Vector2 endPos, GameObject targetObject)
	{
		MyObject.Emitter.Reset();
		Vector2 vector = Vector2.Normalize(endPos - startPos);
		float distance = Utils.GetDistance(startPos, endPos);
		float num = 60f;
		float rotationFromZero = Utils.GetRotationFromZero(vector);
		Matrix matrix = Matrix.CreateRotationZ(rotationFromZero);
		int num2 = (int)(distance / num);
		float num3 = ((num2 <= 0) ? 10f : (distance / (float)num2));
		float num4 = Utils.GetRandom(40, 100, bRandonNegative: true);
		Vector2 position = startPos;
		Particle particle = MyObject.Emitter.Emit(bIgnoreLoopCount: true);
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
		_endEffectObject?.Destroy();
		if (_endEffectObjectData != null && targetObject != null)
		{
			_endEffectObject = targetObject.CreateChildByPartData(_endEffectObjectData, createPhysBody: true);
			_endEffectObject.SetPosition(targetObject.GetRelativePosition(particle2.WorldPosition - targetObject.MyCamera.Position * (Vector2.One - targetObject.MyLayer.ScrollFactor)));
			_endEffectObject.SetRotation(rotationFromZero);
			_endEffectObject.SetScale(MyObject.WorldScale.X / targetObject.WorldScale.X, MyObject.WorldScale.Y / targetObject.WorldScale.Y);
			_endEffectObject.Activate();
			_endEffectObject.IsMovable = true;
			Vector2 value = targetObject.GetTargetWorldPosition() - _endEffectObject.WorldPosition;
			_endEffectObject.SetVelocity(Vector2.Normalize(value) * Utils.Clamp(value.Length() * 10f, 10f, 100f));
		}
		_chainDelay = Utils.GetRandom(0.09f, 0.12f);
		_isBuilded = true;
	}

	protected void OnUpdate(float elapsedTime)
	{
		if (!_isBuilded)
		{
			_targetObject = null;
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
			RayCastResult rayCastResult = GetRayCastResult();
			_targetObject = rayCastResult.TargetObject;
			Vector2 mainLayerPosition = MyObject.GetMainLayerPosition();
			Vector2 collisionPosition = rayCastResult.CollisionPosition;
			if (collisionPosition.X > MyLevel.Camera.ViewBottomRightInWorld.X + _widthHalf)
			{
				float num = MyLevel.Camera.ViewBottomRightInWorld.X + _widthHalf;
				collisionPosition.Y -= (collisionPosition.Y - mainLayerPosition.Y) * (collisionPosition.X - num) / (collisionPosition.X - mainLayerPosition.X);
				collisionPosition.X = num;
				_targetObject = null;
			}
			else if (collisionPosition.X < MyLevel.Camera.ViewTopLeftInWorld.X - _widthHalf)
			{
				float num2 = MyLevel.Camera.ViewTopLeftInWorld.X - _widthHalf;
				collisionPosition.Y -= (collisionPosition.Y - mainLayerPosition.Y) * (collisionPosition.X - num2) / (collisionPosition.X - mainLayerPosition.X);
				collisionPosition.X = num2;
				_targetObject = null;
			}
			Build(mainLayerPosition, collisionPosition, _targetObject);
			if (_targetObject != null)
			{
				if (MyObject.MyItem != null)
				{
					MyObject.MyItem.DoOnHitCollision(MyObject, _targetObject, rayCastResult.CollisionPosition, rayCastResult.CollisionNormal, MyObject.Damage);
				}
				if (_damageClass == TargetClass.Undefined || (_targetObject.TargetClass & _damageClass) != 0)
				{
					_targetObject.Hit(MyObject.Damage, MyObject.DamageType, MyObject);
				}
				if (!_targetObject.IsKilled && !_targetObject.IsDestroyed)
				{
					_targetObject.DoOnCollision(_endEffectObject, rayCastResult.CollisionPosition, rayCastResult.CollisionNormal);
				}
			}
			MyLevel.ShockHit(MyObject, collisionPosition, 60f, TargetClass.Man, MyObject.DamageType, MyObject.Damage, 100f, null, onlyHitable: false, wantResults: false);
		}
		if (MyObject.Emitter != null && MyObject.Emitter.StripLine != null)
		{
			int count = MyObject.Emitter.StripLine.ParticleList.Count;
			if (count > 0)
			{
				Particle particle = MyObject.Emitter.StripLine.ParticleList[0];
				Vector2 vector = MyObject.WorldPosition - particle.Position;
				particle.Position = MyObject.WorldPosition;
				int num3 = MyObject.Emitter.StripLine.ParticleList.Count / 3;
				for (int i = 1; i < num3; i++)
				{
					MyObject.Emitter.StripLine.ParticleList[i].Position += vector / (i + 1);
				}
				Particle particle2 = MyObject.Emitter.StripLine.ParticleList[count - 1];
				particle2.StartupColor = new Color(particle2.StartupColor, 0);
				if (_endEffectObject != null)
				{
					vector = _endEffectObject.GetMainLayerPosition() - particle2.Position;
					_endEffectObject.ResetPrecalculations();
					particle2.Position = _endEffectObject.GetMainLayerPosition();
					num3 = MyObject.Emitter.StripLine.ParticleList.Count / 3;
					for (int j = 1; j < num3; j++)
					{
						MyObject.Emitter.StripLine.ParticleList[MyObject.Emitter.StripLine.ParticleList.Count - j - 1].Position += vector / (j + 1);
					}
					if (count > 1)
					{
						Particle particle3 = MyObject.Emitter.StripLine.ParticleList[count - 2];
						float rotationTo = Utils.GetRotationTo(particle2.WorldPosition, particle3.WorldPosition);
						_endEffectObject.SetRotation(rotationTo);
					}
				}
			}
		}
		if (!_isChainCreated && MyObject.Timer >= _chainDelay && _maxChainCount > 0)
		{
			CreateChain();
		}
		if (_endEffectObject != null && _endEffectObject.IsDestroyed)
		{
			_endEffectObject = null;
		}
	}

	private void CreateChain()
	{
		if (_targetObject == null || _endEffectObject == null || (MyObject.LinkedObjectList != null && MyObject.LinkedObjectList.Count > _maxChainCount))
		{
			_isChainCreated = true;
			return;
		}
		List<GameObject> gameObjectList = GameCore.Allocator.GetGameObjectList();
		if (MyObject.LinkedObjectList != null)
		{
			gameObjectList.AddRange(MyObject.LinkedObjectList);
		}
		gameObjectList.Add(_targetObject);
		if (!_targetObject.IsMainPart)
		{
			gameObjectList.Add(_targetObject.GetMainPart());
		}
		GameObject gameObject = null;
		if (_targetObject.TargetClass == TargetClass.Building || _targetObject.TargetClass == TargetClass.Other || Utils.GetRandom(3) != 0)
		{
			gameObject = _endEffectObject.FindNearestTarget((TargetClass)0x78 | _extraTargetClass, 800f, 100f, gameObjectList, bOnlyHittable: true);
		}
		if (gameObject == null && _targetObject.TargetClass != TargetClass.Building && _targetObject.TargetClass != TargetClass.Other)
		{
			gameObject = _endEffectObject.FindNearestTarget((TargetClass)8440, 800f, 100f, gameObjectList, bOnlyHittable: true);
		}
		if (gameObject != null)
		{
			GameObject parentObject = _endEffectObject.ParentObject;
			GameObject gameObject2;
			if (_endEffectObject.IsMainPart)
			{
				gameObject2 = _endEffectObject.MyLayer.CreateObjectByPartData(MyObject.PartData, _endEffectObject.WorldPosition, MyObject.WorldScale, 0f, MyObject.ZOrder);
			}
			else
			{
				gameObject2 = _endEffectObject.ParentObject.CreateChildByPartData(MyObject.PartData, createPhysBody: true);
				gameObject2.SetPosition(parentObject.GetRelativePosition(_endEffectObject.WorldPosition - parentObject.MyCamera.Position * (Vector2.One - parentObject.MyLayer.ScrollFactor)));
				gameObject2.SetScale(MyObject.WorldScale.X / parentObject.WorldScale.X, MyObject.WorldScale.Y / parentObject.WorldScale.Y);
				gameObject2.Activate();
			}
			gameObject2.MyItem = MyObject.MyItem;
			gameObject2.Damage = MyObject.Damage;
			gameObject2.AddLinkedObject(gameObject);
			gameObject2.LinkedObjectList.AddList(gameObjectList);
		}
		GameCore.Allocator.DeleteAliasingGameObjectList(gameObjectList);
		_isChainCreated = true;
	}

	protected RayCastResult GetRayCastResult()
	{
		List<GameObject> gameObjectList = GameCore.Allocator.GetGameObjectList();
		if (MyObject.LinkedObjectList != null && MyObject.LinkedObjectList.Count > 1)
		{
			int count = MyObject.LinkedObjectList.Count;
			for (int i = 1; i < count; i++)
			{
				gameObjectList.Add(MyObject.LinkedObjectList[i]);
			}
		}
		gameObjectList.Add(MyObject.GetMainPart());
		RayCastResult rayCastResult = MyObject.MyLevel.GetRayCastResult(MyObject.WorldPosition, MyObject.WorldRotation, _maxLength, _hitTo, gameObjectList);
		float num = rayCastResult.Length;
		MyObject.ResetPrecalculations();
		RayCastResult rayCastResult2 = MyObject.MyLevel.GetRayCastResult(Utils.GetParallelPoint(MyObject.WorldPosition, MyObject.WorldRotation, _widthHalf), MyObject.WorldRotation, _maxLength, _hitTo, gameObjectList);
		float length = rayCastResult2.Length;
		if (length < num)
		{
			num = length;
			rayCastResult = rayCastResult2;
		}
		rayCastResult2 = MyObject.MyLevel.GetRayCastResult(Utils.GetParallelPoint(MyObject.WorldPosition, MyObject.WorldRotation, 0f - _widthHalf), MyObject.WorldRotation, _maxLength, _hitTo, gameObjectList);
		length = rayCastResult2.Length;
		if (length < num)
		{
			num = length;
			rayCastResult = rayCastResult2;
		}
		GameCore.Allocator.DeleteAliasingGameObjectList(gameObjectList);
		return rayCastResult;
	}
}
