namespace Rake2D.Script;

public class PolySurfaceFlow : BehaviorScript
{
	private float[] _flowSpeedArray;

	public override void RegisterEvents()
	{
		if (_flowSpeedArray != null && _flowSpeedArray.Length != 0 && MyObject.PolygonRenderer != null && MyObject.PolygonRenderer.TopSurfaceInfoArray != null && MyObject.PolygonRenderer.TopSurfaceInfoArray.Length != 0)
		{
			MyObject.OnUpdate += OnUpdate;
			if (_doInEditor && GameCore.IsEditorEnabled)
			{
				MyObject.OnEditorUpdate += OnUpdate;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		_flowSpeedArray = ParamArrayMultiply("flow_speed", -0.01f);
	}

	private void OnUpdate(float elapsedTime)
	{
		for (int i = 0; i < MyObject.PolygonRenderer.TopSurfaceInfoArray.Length && i < _flowSpeedArray.Length; i++)
		{
			MyObject.PolygonRenderer.TopSurfaceInfoArray[i].FlowOffset += _flowSpeedArray[i] * elapsedTime;
		}
	}
}
