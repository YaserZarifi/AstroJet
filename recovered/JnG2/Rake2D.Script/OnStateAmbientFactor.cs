using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnStateAmbientFactor : ExitStateBehavior
{
	private Vector3 _targetFactor;

	private float _changeSpeed;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		if (_doInEditor)
		{
			MyObject.OnEditorUpdate += OnUpdate;
		}
	}

	public override void Init()
	{
		base.Init();
		_targetFactor = ParamVector3("factor", 100f, 100f, 100f) / 100f;
		_changeSpeed = Param("speed") / 100f;
	}

	public void OnUpdate(float elaspsedTime)
	{
		if (Utils.ChangeValueToTarget(ref MyLevel.AmbientFactor, _targetFactor, _changeSpeed * elaspsedTime))
		{
			MyObject.OnUpdate -= OnUpdate;
			MyObject.OnEditorUpdate -= OnUpdate;
			StartExit();
		}
	}
}
