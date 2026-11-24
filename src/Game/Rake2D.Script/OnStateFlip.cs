using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnStateFlip : ExitStateBehavior
{
	private Vector2 _originalScale;

	private Vector2 _scaleFactor;

	private Vector2 _targetScaleFactor;

	private float _flipSpeed;

	private bool _oldIsDynamic;

	public override void Init()
	{
		base.Init();
		_flipSpeed = Param("flip_speed", 100f) / 100f;
		switch (ParamInt("flip_mode", 1))
		{
		case 2:
			_targetScaleFactor = new Vector2(1f, -1f);
			break;
		case 3:
			_targetScaleFactor = new Vector2(-1f, -1f);
			break;
		case 11:
			_targetScaleFactor = new Vector2((Utils.GetRandom(1) != 0) ? 1 : (-1), 1f);
			break;
		case 21:
			_targetScaleFactor = new Vector2((!(MyObject.Velocity.X > 0f)) ? 1 : (-1), 1f);
			break;
		default:
			_targetScaleFactor = new Vector2(-1f, 1f);
			break;
		}
	}

	public override void RegisterEvents()
	{
		if (_flipSpeed <= 0f)
		{
			MyObject.SetScale(MyObject.Scale * _targetScaleFactor);
			_exitPauseRange = new FloatRange(0.01f, 0.01f);
			StartExit();
		}
		else
		{
			_originalScale = MyObject.Scale;
			_scaleFactor = Vector2.One;
			MyObject.OnUpdate += OnUpdate;
			_oldIsDynamic = MyObject.IsDynamic;
		}
	}

	private void OnUpdate(float elapsedTime)
	{
		MyObject.IsDynamic = false;
		if (Utils.ChangeValueToTarget(ref _scaleFactor, _targetScaleFactor, elapsedTime * _flipSpeed))
		{
			StartExit();
			MyObject.OnUpdate -= OnUpdate;
			MyObject.IsDynamic = _oldIsDynamic;
		}
		MyObject.SetScale(_originalScale * _scaleFactor);
	}
}
