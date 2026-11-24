using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class PathChange : BehaviorScript
{
	public static string[] PathEvents = new string[1] { "change" };

	public override void RegisterEvents()
	{
	}

	public override void DoEvent(BehaviorEvent behEvent, BehaviorTimeLine sourceTimeLine)
	{
		if (GameCore.IsEditorEnabled || behEvent.ValueInt <= 0)
		{
			return;
		}
		GameObject randomItem = Utils.GetRandomItem(behEvent.EventNode.LinkedObjectList);
		if (randomItem != null && randomItem.MyPath != null)
		{
			MyObject.PathTime = 0f;
			BezierPath bezierPath = randomItem.MyPath.Clone(MyObject, -1L);
			if (randomItem.PathOscilation != Vector2.Zero)
			{
				bezierPath.Move(randomItem.PathOscilation);
			}
			bezierPath.ControlPointList[0] = MyObject.WorldPosition;
			bezierPath.CalculateLength();
			MyObject.ChangePath(bezierPath);
		}
	}
}
