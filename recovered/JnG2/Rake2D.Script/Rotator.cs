using System;

namespace Rake2D.Script;

public class Rotator : BehaviorScript
{
	private FloatRange _rotationSpeedRange;

	private float _rotationSpeed;

	public override void RegisterEvents()
	{
		if (GameCore.IsEditorEnabled && _doInEditor)
		{
			MyObject.OnEditorUpdate += OnUpdate;
		}
		else
		{
			MyObject.OnUpdate += OnUpdate;
		}
		_rotationSpeed = Utils.DegToRad(_rotationSpeedRange.GetRandom());
	}

	public override void Init()
	{
		base.Init();
		_rotationSpeedRange = ParamRange("speed", 360f);
	}

	private void OnUpdate(float elapsedTime)
	{
		MyObject.SetRotation((MyObject.Rotation + _rotationSpeed * elapsedTime) % ((float)Math.PI * 2f));
	}
}
