namespace Rake2D.Script;

public class AlphaIntensitor : BehaviorScript
{
	private float _minIntensity;

	private float _targetIntensity;

	private float _changeSpeed;

	private float _currIntensity;

	private byte _initAlpha;

	private bool _isRandom;

	private bool _forward;

	private bool _applyToChildren;

	private bool _useParentAlpha;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
		if (DoInEditor)
		{
			MyObject.OnEditorUpdate += Update;
		}
		if (MyObject.CurrentState.UseColor)
		{
			_initAlpha = MyObject.CurrentState.Color.A;
		}
		else
		{
			_initAlpha = MyObject.Color.A;
		}
	}

	public override void Init()
	{
		base.Init();
		_minIntensity = Utils.Clamp(Param("intensity", 75f), 0f, 100f) / 100f;
		_useParentAlpha = ParamBool("use_parent_alpha");
		_changeSpeed = ParamRandomRange("speed", 100f) / 100f;
		int num = ParamInt("random", 1);
		_isRandom = num > 0;
		_applyToChildren = ParamBool("apply_to_children");
		_forward = true;
		SetTargetIntensity();
		switch (num)
		{
		case -1:
		case 0:
		case 1:
			_currIntensity = Utils.GetRandom(_minIntensity, 1f);
			break;
		case -2:
		case 2:
			_currIntensity = _minIntensity;
			break;
		case -3:
		case 3:
			_currIntensity = 1f;
			_targetIntensity = _minIntensity;
			break;
		}
	}

	private void SetTargetIntensity()
	{
		if (_isRandom)
		{
			_targetIntensity = Utils.GetRandom(_minIntensity, 1f);
		}
		else if (_forward)
		{
			_targetIntensity = 1f;
		}
		else
		{
			_targetIntensity = _minIntensity;
		}
		_forward = !_forward;
	}

	private void Update(float elapsedTime)
	{
		if (Utils.ChangeValueToTarget(ref _currIntensity, _targetIntensity, _changeSpeed * elapsedTime))
		{
			SetTargetIntensity();
		}
		byte b = (byte)((float)(int)_initAlpha * _currIntensity);
		if (_useParentAlpha && MyObject.ParentObject != null)
		{
			b = (byte)((float)(int)b * ((float)(int)MyObject.ParentObject.Alpha / 255f));
		}
		if (_applyToChildren)
		{
			MyObject.SetAlphaToAll(b);
		}
		else
		{
			MyObject.Alpha = b;
		}
	}
}
