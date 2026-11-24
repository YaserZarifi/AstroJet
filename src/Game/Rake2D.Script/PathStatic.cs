namespace Rake2D.Script;

public class PathStatic : BehaviorScript
{
	private float _startCameraX;

	private bool _isStartInitialized;

	private bool _isFixedToScrreenEdge;

	public override void RegisterEvents()
	{
		if (MyObject.MyPath != null)
		{
			MyObject.OnUpdateBegin += OnUpdateBegin;
		}
		MyObject.OnPathChanged += OnPathChanged;
		MyObject.HasScreenRelativePath = true;
	}

	public override void Init()
	{
		base.Init();
		_isFixedToScrreenEdge = ParamBool("screen_edge");
	}

	private void OnUpdateBegin(float elapsedTime)
	{
		if (!MyObject.IsTriggerFree)
		{
			return;
		}
		if (!_isStartInitialized)
		{
			if (_isFixedToScrreenEdge)
			{
				_startCameraX = MyObject.MyCamera.Position.X - (MyObject.MyCamera.Position.X + 1920f - ((MyObject.TriggerPoint.X == 0f) ? MyObject.WorldPosition.X : MyObject.TriggerPoint.X));
			}
			else
			{
				_startCameraX = MyObject.MyCamera.Position.X;
			}
			_isStartInitialized = true;
		}
		MyObject.PathOscilation.X = MyObject.MyCamera.Position.X - _startCameraX;
	}

	private void OnPathChanged()
	{
		_startCameraX = MyObject.MyCamera.Position.X;
	}
}
