namespace Rake2D.Script;

public class PathGiver : BehaviorScript
{
	public override void RegisterEvents()
	{
	}

	public override void Init()
	{
		if (GameCore.IsEditorEnabled || MyObject.LinkedObjectList == null)
		{
			return;
		}
		GameObject gameObject = null;
		foreach (GameObject linkedObject in MyObject.LinkedObjectList)
		{
			if (linkedObject.MyPath != null)
			{
				gameObject = linkedObject;
				break;
			}
		}
		if (gameObject == null)
		{
			LogErr("No linked object with path");
			return;
		}
		foreach (GameObject linkedObject2 in MyObject.LinkedObjectList)
		{
			if (linkedObject2 != gameObject && linkedObject2.MyPath == null)
			{
				linkedObject2.PathTime = gameObject.PathTime;
				BezierPath bezierPath = gameObject.MyPath.Clone(linkedObject2, -1L);
				bezierPath.ControlPointList[0] = linkedObject2.WorldPosition;
				bezierPath.CalculateLength();
				linkedObject2.ChangePath(bezierPath);
			}
			if (gameObject.MyTimeLine != null && linkedObject2 != gameObject && linkedObject2.MyTimeLine == null)
			{
				linkedObject2.MyTimeLine = gameObject.MyTimeLine.Clone(linkedObject2, -1L);
				linkedObject2.SetCurrentTimeLine(linkedObject2.MyTimeLine);
			}
		}
		MyObject.Destroy();
	}
}
