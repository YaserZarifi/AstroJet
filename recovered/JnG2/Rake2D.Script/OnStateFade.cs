using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnStateFade : ExitStateBehavior
{
	private FloatRange _targetAlphaRange;

	private FloatRange _durationRange;

	private float _targetAlpha;

	private float _duration;

	private float _speed;

	private float _currAlpha;

	private bool _destroy;

	private bool _useMainPart;

	private bool _useMainColor;

	private GameObject _fadePart;

	private GameObject _mainPart;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		if (_doInEditor)
		{
			MyObject.OnEditorUpdate += OnUpdate;
		}
		if (_useMainPart)
		{
			_fadePart = MyObject.GetMainPart();
		}
		else
		{
			_fadePart = MyObject;
		}
		_targetAlpha = MathHelper.Clamp(_targetAlphaRange.GetRandom(), 0f, 255f);
		_duration = _durationRange.GetRandom();
		_currAlpha = (int)_fadePart.Alpha;
		if (_duration > 0f)
		{
			_speed = Math.Abs(_currAlpha - _targetAlpha) / _duration;
		}
	}

	public override void Init()
	{
		base.Init();
		_targetAlphaRange = ParamRange("alpha");
		_durationRange = ParamRange("duration", 1000f) / 1000f;
		_useMainPart = ParamBool("main", defaultValue: true);
		_useMainColor = ParamBool("main_color");
		_mainPart = MyObject.GetMainPart();
		_destroy = ParamBool("destroy", _targetAlphaRange.IsZero);
	}

	private void OnUpdate(float elapsedTime)
	{
		if (Utils.ChangeValueToTarget(ref _currAlpha, _targetAlpha, _speed * elapsedTime))
		{
			if (_destroy)
			{
				_fadePart.Destroy();
				return;
			}
			MyObject.OnUpdate -= OnUpdate;
			MyObject.OnEditorUpdate -= OnUpdate;
			StartExit();
		}
		if (_useMainColor)
		{
			_fadePart.SetColorToAll(_mainPart.Color.R, _mainPart.Color.G, _mainPart.Color.B, (byte)_currAlpha);
		}
		else
		{
			_fadePart.SetAlphaToAll((byte)_currAlpha);
		}
	}
}
