using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Rake2D.Script;

namespace Rake2D;

public class LayerSegmentObjectData : Transformation
{
	public long Pk;

	public PartData PartData;

	public IntRange DifficultyRange;

	public List<int> GameModeList;

	public int ZOrder;

	public int ForPlayerCount;

	public Vector2 TriggerPoint;

	public TriggerPointModeEnum TriggerMode;

	public bool CreatePhysBody;

	public int[] LinkPkArrray;

	public float[] PolygonVertexArray;

	public bool IsPolygonTopEnabled;

	public bool IsPolygonBottomEnabled;

	public Vector2 PolygonTextureOffset;

	public BezierPath Path;

	public BehaviorTimeLine TimeLine;

	public PolygonTextureMappingModeEnum PolygonTextureMappingMode;

	public LayerSegmentObjectData(PartData partData)
	{
		PartData = partData;
		IsPolygonTopEnabled = (IsPolygonBottomEnabled = true);
		DifficultyRange = new IntRange(0, 999);
	}
}
