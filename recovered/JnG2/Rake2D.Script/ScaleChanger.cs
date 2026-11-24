using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class ScaleChanger : BehaviorScript
{
	private Vector2 _scaleFactor;

	private Vector2 _scaleEnd;

	private Vector2 _deltaScale;

	private float _scaleTime;

	private bool _bInitialized;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
		if (_doInEditor)
		{
			MyObject.OnEditorUpdate += Update;
		}
	}

	public override void Init()
	{
		base.Init();
		_scaleFactor = ParamVector2("scale_factor", 100f, 100f) / 100f;
		if (!HasParam("scale_time_percent"))
		{
			_scaleTime = Param("scale_time", 1000f) / 1000f;
		}
		else
		{
			_scaleTime = Param("scale_time_percent", 50f) / 100f * MyObject.LifeTime;
		}
	}

	public void Update(float elapsedTime)
	{
		if (MyObject.LifeTime <= 0f)
		{
			MyObject.OnUpdate -= Update;
			MyObject.OnEditorUpdate -= Update;
		}
		else if (_scaleTime >= MyObject.LifeTime)
		{
			if (!_bInitialized)
			{
				Vector2 vector = new Vector2(MyObject.WorldScale.X, MyObject.WorldScale.Y);
				_scaleEnd = vector * _scaleFactor;
				_deltaScale = vector - _scaleEnd;
				_bInitialized = true;
			}
			float num = MyObject.LifeTime / _scaleTime;
			MyObject.SetScale(_scaleEnd.X + _deltaScale.X * num, _scaleEnd.Y + _deltaScale.Y * num);
		}
	}
}
