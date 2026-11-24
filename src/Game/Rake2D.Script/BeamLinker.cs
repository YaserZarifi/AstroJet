using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class BeamLinker : BehaviorScript
{
	private GameObject _fromObject;

	private GameObject _toObject;

	private GameObject _fromEffectObject;

	private GameObject _toEffectObject;

	private GameObject _physObject;

	private PartData _physPartData;

	private Particle _startParticle;

	private Particle _endParticle;

	private int _effectZorder;

	private bool _killEndObjects;

	private bool _soloMode;

	private bool _isConnected;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		if (GameCore.IsEditorEnabled)
		{
			MyObject.OnEditorUpdate += OnUpdate;
			MyObject.OnEditorUpdate += OnEditorUpdate;
		}
		MyObject.OnDestroy += OnDestroy;
		MyObject.OnDeactivate += OnDeactivate;
	}

	public override void Init()
	{
		base.Init();
		if (MyObject.Emitter != null)
		{
			MyObject.Emitter.IsEmitedByBehavior = true;
		}
		string text = ParamString("phys_id");
		if (text != null)
		{
			_physPartData = GameCore.GetPartData(text);
		}
		_soloMode = ParamBool("solo_mode");
		_effectZorder = ParamInt("effect_zorder", 1);
		_killEndObjects = ParamBool("kill_end", defaultValue: true);
	}

	private void OnDeactivate()
	{
		DestroyBeam();
	}

	private void BuildBeam()
	{
		DestroyBeam();
		if (MyObject.LinkedObjectList.IsNullOrEmpty())
		{
			return;
		}
		if (_soloMode)
		{
			_fromObject = MyObject;
			_toObject = MyObject.LinkedObjectList[0];
		}
		else
		{
			_fromObject = MyObject.LinkedObjectList[0];
			if (MyObject.LinkedObjectList.Count > 1)
			{
				_toObject = MyObject.LinkedObjectList[1];
			}
			else
			{
				_toObject = _fromObject;
			}
		}
		if (_fromObject != null)
		{
			string text = ParamString("part_from");
			if (text != null)
			{
				GameObject childByIdDeep = _fromObject.GetChildByIdDeep(text);
				if (childByIdDeep != null)
				{
					_fromObject = childByIdDeep;
				}
			}
			text = ParamString("effect_from");
			if (text != null)
			{
				_fromEffectObject = MyObject.MyLayer.CreateObjectByPartData(text, _fromObject.WorldPosition, _fromObject.WorldScale, _fromObject.WorldRotation, _fromObject.GetMainPart().ZOrder + _effectZorder);
			}
			_isConnected = true;
		}
		if (_toObject != null)
		{
			string text = ParamString("part_to");
			if (text != null)
			{
				GameObject childByIdDeep2 = _toObject.GetChildByIdDeep(text);
				if (childByIdDeep2 != null)
				{
					_toObject = childByIdDeep2;
				}
			}
			text = ParamString("effect_to");
			if (text != null)
			{
				_toEffectObject = MyObject.MyLayer.CreateObjectByPartData(text, _toObject.WorldPosition, _toObject.WorldScale, _toObject.WorldRotation, _toObject.GetMainPart().ZOrder + _effectZorder);
			}
		}
		MyObject.Emitter.Emit(bIgnoreLoopCount: true);
		MyObject.Emitter.StripLine.ParentTransformation = null;
		_startParticle = MyObject.Emitter.StripLine.ParticleList[0];
		_endParticle = MyObject.Emitter.StripLine.ParticleList[1];
		BuildPhysObject();
	}

	private void BuildPhysObject()
	{
		_physObject?.Destroy();
		if (_physPartData != null && _fromEffectObject != null && _toEffectObject != null)
		{
			float rotationTo = Utils.GetRotationTo(_fromEffectObject.WorldPosition, _toEffectObject.WorldPosition);
			float num = (_fromEffectObject.WorldPosition - _toEffectObject.WorldPosition).Length();
			Vector2 pos = _fromObject.WorldPosition + Utils.GetVelocityVector2(num / 2f, rotationTo);
			Vector2 scale = new Vector2(1f, num / 100f);
			_physObject = MyLayer.CreateObjectByPartData(_physPartData, pos, scale, rotationTo, MyObject.ZOrder);
		}
	}

	private void OnDestroy()
	{
		DestroyBeam();
	}

	private void DestroyBeam()
	{
		_isConnected = false;
		_fromObject = null;
		_toObject = null;
		_fromEffectObject?.Destroy();
		_fromEffectObject = null;
		_toEffectObject?.Destroy();
		_toEffectObject = null;
		_physObject?.Destroy();
		_physObject = null;
		MyObject.Emitter?.Reset();
		_startParticle?.Destroy();
		_startParticle = null;
		_endParticle?.Destroy();
		_endParticle = null;
	}

	private void KillMe()
	{
		_isConnected = false;
		_fromEffectObject?.Kill();
		_fromEffectObject = null;
		_toEffectObject?.Kill();
		_toEffectObject = null;
		_startParticle?.Destroy();
		_startParticle = null;
		_endParticle?.Destroy();
		_endParticle = null;
		_physObject?.Destroy();
		_physObject = null;
		if (!GameCore.IsEditorEnabled && _killEndObjects)
		{
			_fromObject?.GetMainPart().Kill();
			_fromObject = null;
			_toObject?.GetMainPart().Kill();
			_toObject = null;
		}
		MyObject.OnUpdate -= OnUpdate;
		if (!GameCore.IsEditorEnabled)
		{
			MyObject.Destroy();
		}
	}

	private void UpdateBeam()
	{
		if (_fromObject == null && MyObject.LinkedObjectList != null && ((_soloMode && MyObject.LinkedObjectList.Count > 0) || MyObject.LinkedObjectList.Count > 1))
		{
			BuildBeam();
		}
		if (_startParticle != null)
		{
			_startParticle.Position = _fromObject.WorldPosition;
			_endParticle.Position = _toObject.WorldPosition;
		}
	}

	private void OnEditorUpdate(float elapsedTime)
	{
		if (GameCore.Editor.CurrentSelectionGroup == null && (MyObject.LinkedObjectList == null || (_fromObject != null && !_soloMode && !MyObject.LinkedObjectList.Contains(_fromObject.GetMainPart())) || (_toObject != null && !MyObject.LinkedObjectList.Contains(_toObject.GetMainPart()))))
		{
			DestroyBeam();
		}
	}

	private void UpdatePhys(float elapsedTime)
	{
		if (_physPartData != null && _fromEffectObject != null && _toEffectObject != null)
		{
			_physObject.IsVisible = MyObject.IsVisible;
			if (_physObject.IsVisible && _physObject.RigidBody == null)
			{
				_physObject.CreateRigidBodyByData();
			}
			else if (!_physObject.IsVisible && _physObject.RigidBody != null)
			{
				_physObject.DestroyRigidBody();
			}
			_physObject.Update(elapsedTime);
			float rotationTo = Utils.GetRotationTo(_fromEffectObject.WorldPosition, _toEffectObject.WorldPosition);
			float num = (_fromEffectObject.WorldPosition - _toEffectObject.WorldPosition).Length();
			_physObject.SetRotation(rotationTo);
			_physObject.SetPosition(_fromObject.WorldPosition + Utils.GetVelocityVector2(num / 2f, rotationTo));
			_physObject.SetScale(new Vector2(_physObject.CurrentState.Scale.X, num / 100f));
		}
	}

	private void OnUpdate(float elapsedTime)
	{
		if ((_fromObject != null && (_fromObject.IsDestroyed || _fromObject.IsKilled)) || (_toObject != null && (_toObject.IsDestroyed || _toObject.IsKilled)))
		{
			KillMe();
			return;
		}
		UpdateBeam();
		if (_isConnected)
		{
			if (_fromObject != null && _fromEffectObject != null)
			{
				_fromEffectObject.IsVisible = MyObject.IsVisible;
				_fromEffectObject.SetPosition(_fromObject.WorldPosition);
				_fromEffectObject.SetRotation(_physObject.WorldRotation);
			}
			if (_toObject != null && _toEffectObject != null)
			{
				_toEffectObject.IsVisible = MyObject.IsVisible;
				_toEffectObject.SetPosition(_toObject.WorldPosition);
				_toEffectObject.SetRotation(_physObject.WorldRotation);
			}
			UpdatePhys(elapsedTime);
		}
	}
}
