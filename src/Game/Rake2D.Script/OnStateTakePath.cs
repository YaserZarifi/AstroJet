namespace Rake2D.Script;

public class OnStateTakePath : BehaviorScript
{
	private string _objectId;

	public override void Init()
	{
		base.Init();
		_objectId = ParamString("object_id");
	}

	public override void RegisterEvents()
	{
		GameObject gameObject = MyLayer.GameObjectList.Find((GameObject go) => Utils.Equals(go.Id, _objectId));
		if (DoInEditor && gameObject != null && gameObject.MyPath != null)
		{
			BezierPath bezierPath = gameObject.MyPath.Clone(MyObject, -1L);
			bezierPath.ControlPointList[0] = MyObject.WorldPosition;
			bezierPath.CalculateLength();
			MyObject.ChangePath(bezierPath);
			if (gameObject.MyTimeLine != null)
			{
				MyObject.MyTimeLine = gameObject.MyTimeLine.Clone(MyObject, -1L);
				gameObject.SetCurrentTimeLine(gameObject.MyTimeLine);
			}
		}
	}
}
