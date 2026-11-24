using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script.HeroShot;

public class FxBeamHero : BeamHero
{
	private PartData _fxObjectData;

	private List<GameObject> _fxObjectList;

	private FloatRange _fxDelayRange;

	private float _fxSpeed;

	private float _fxTimer;

	public override void Init()
	{
		base.Init();
		string text = ParamString("fx_id");
		if (text != null)
		{
			_fxObjectData = GameCore.GetPartData(text);
			if (_fxObjectData != null)
			{
				_fxObjectList = GameCore.Allocator.GetGameObjectList();
			}
		}
		_fxSpeed = Param("fx_speed", 1000f);
		_fxDelayRange = ParamRange("fx_delay", 1000f) / 1000f;
	}

	protected override void OnUpdate(float elapsedTime)
	{
		base.OnUpdate(elapsedTime);
		if (_fxObjectData == null)
		{
			return;
		}
		if (Utils.ChangeValueToTarget(ref _fxTimer, 0f, elapsedTime))
		{
			_fxTimer = _fxDelayRange.GetRandom();
			GameObject gameObject = MyObject.CreateChildByPartData(_fxObjectData, createPhysBody: true);
			gameObject.SetVelocity(0f, _fxSpeed);
			_fxObjectList.Add(gameObject);
		}
		if (_startParticle == null || _endParticle == null)
		{
			return;
		}
		float rotation = GetRotation();
		float distance = Utils.GetDistance(_startParticle.WorldPosition, _endParticle.WorldPosition);
		Utils.GetVelocityVector2(distance, rotation);
		for (int num = _fxObjectList.Count - 1; num >= 0; num--)
		{
			GameObject gameObject2 = _fxObjectList[num];
			gameObject2.SetVelocity(0f, _fxSpeed);
			Vector2 position = new Vector2(0f, (0f - _fxSpeed) * gameObject2.Timer);
			if (position.Length() >= distance)
			{
				gameObject2.Destroy();
				gameObject2.SetPosition(0f, 0f - distance);
				gameObject2.DoOnCollision(_targetObject, _endParticle.WorldPosition, _collisionNormal);
				_fxObjectList.RemoveAt(num);
			}
			else
			{
				gameObject2.SetPosition(position);
			}
		}
	}
}
