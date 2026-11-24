using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Rake2D.Script;

namespace Rake2D;

public class BezierPath
{
	public string Id;

	public GameObject MyObject;

	public string FileName;

	public List<Vector2> ControlPointList;

	public List<BehaviorEventNode> EventNodeList;

	private int _segmentCount;

	private float[] _lengthArray;

	private int _segmentCountPerCurve = 200;

	public int CurveCount { get; private set; }

	public float Length { get; private set; }

	public BezierPath(string id, GameObject gameObject)
	{
		Id = id;
		ControlPointList = new List<Vector2>();
		EventNodeList = new List<BehaviorEventNode>();
		MyObject = gameObject;
	}

	public BezierPath Clone(GameObject clonedObject, long linkedPkOffset = -1L)
	{
		BezierPath bezierPath = new BezierPath(Id, clonedObject);
		foreach (Vector2 controlPoint in ControlPointList)
		{
			bezierPath.ControlPointList.Add(controlPoint);
		}
		foreach (BehaviorEventNode eventNode in EventNodeList)
		{
			BehaviorEventNode behaviorEventNode = bezierPath.AddEventNode(eventNode.Time);
			if (eventNode.TimeLine != null)
			{
				behaviorEventNode.TimeLine = eventNode.TimeLine.Clone(clonedObject, linkedPkOffset);
			}
			foreach (BehaviorEvent @event in eventNode.EventList)
			{
				behaviorEventNode.AddEvent(@event.ChildId, @event.BehaviorId, @event.Name, @event.ValueInt).ValueString = @event.ValueString;
			}
			if (eventNode.LinkedObjectList != null)
			{
				behaviorEventNode.LinkedObjectList = GameCore.Allocator.CloneGameObjectList(eventNode.LinkedObjectList);
			}
			behaviorEventNode.SetObject(clonedObject);
			behaviorEventNode.DifficultyRange = new IntRange(eventNode.DifficultyRange.From, eventNode.DifficultyRange.To);
			if (eventNode.LinkedPkList == null || linkedPkOffset < 0)
			{
				continue;
			}
			behaviorEventNode.LinkedPkList = new List<long>();
			foreach (long linkedPk in eventNode.LinkedPkList)
			{
				int num = (int)linkedPk;
				behaviorEventNode.LinkedPkList.Add(num + linkedPkOffset);
			}
		}
		return bezierPath;
	}

	public BehaviorEventNode AddEventNode(float time)
	{
		BehaviorEventNode behaviorEventNode = new BehaviorEventNode(time);
		EventNodeList.Add(behaviorEventNode);
		return behaviorEventNode;
	}

	public void RefreshEventNodes()
	{
		EventNodeList = EventNodeList.OrderBy((BehaviorEventNode o) => o.Time).ToList();
	}

	public void CalculateLength()
	{
		CurveCount = ControlPointList.Count / 3;
		_segmentCount = _segmentCountPerCurve * CurveCount;
		_lengthArray = new float[_segmentCount];
		Length = 0f;
		for (int i = 0; i < CurveCount; i++)
		{
			int num = i * 3;
			Vector2 value = GetPosition(0f, ControlPointList[num], ControlPointList[num + 1], ControlPointList[num + 2], ControlPointList[num + 3]);
			for (int j = 0; j < _segmentCountPerCurve; j++)
			{
				float t = (float)(j + 1) / (float)_segmentCountPerCurve;
				Vector2 position = GetPosition(t, ControlPointList[num], ControlPointList[num + 1], ControlPointList[num + 2], ControlPointList[num + 3]);
				float num2 = Vector2.Distance(value, position);
				Length += num2;
				_lengthArray[_segmentCountPerCurve * i + j] = num2;
				value = position;
			}
		}
		RefreshEventNodes();
	}

	public float GetEndPointTime(int endPointNumber)
	{
		return (float)(endPointNumber * _segmentCountPerCurve) / (float)_segmentCount;
	}

	public void Clear()
	{
		ControlPointList.Clear();
	}

	public void Move(Vector2 offset)
	{
		Matrix matrix = Matrix.CreateTranslation(offset.X, offset.Y, 0f);
		for (int i = 0; i < ControlPointList.Count; i++)
		{
			ControlPointList[i] = Vector2.Transform(ControlPointList[i], matrix);
		}
		CalculateLength();
	}

	public void Rotate(float rotationOffset)
	{
		Vector2 vector = ControlPointList[0];
		Matrix matrix = Matrix.CreateRotationZ(rotationOffset);
		for (int i = 0; i < ControlPointList.Count; i++)
		{
			ControlPointList[i] = Vector2.Transform(ControlPointList[i] - vector, matrix);
			ControlPointList[i] += vector;
		}
		CalculateLength();
	}

	public float GetNextPositionTime(float currTime, float arcLength)
	{
		if (float.IsNaN(currTime))
		{
			GameCore.LogErr("NaN time on path");
			return 1f;
		}
		if (arcLength == 0f)
		{
			return currTime;
		}
		int num = Math.Max(0, (int)(currTime * (float)_segmentCount));
		float num2 = (float)num / (float)_segmentCount;
		float num3 = 0f;
		int num4;
		for (num4 = num; num3 < arcLength; num3 += _lengthArray[num4++])
		{
			if (_lengthArray == null)
			{
				break;
			}
			if (num4 >= _lengthArray.Length - 1)
			{
				break;
			}
			if (num4 >= 2147483646)
			{
				break;
			}
		}
		float num5 = 0f;
		if (num4 > 0)
		{
			num5 = (num3 - arcLength) / _lengthArray[num4 - 1];
		}
		float num6 = num5 * 1f / (float)_segmentCount;
		return Utils.Clamp((float)num4 / (float)_segmentCount + (currTime - num2) - num6, 0f, 1f);
	}

	public Vector2 GetPosition(float currTime, out float angle)
	{
		Vector2 position = GetPosition(currTime);
		angle = Utils.GetRotationTo(position, GetPosition(currTime + 1f / (float)_segmentCount));
		return position;
	}

	public float GetRotation(float currTime, Vector2 pos)
	{
		return Utils.GetRotationTo(pos, GetPosition(currTime + 1f / (float)_segmentCount));
	}

	public int GetCurveNumber(float time)
	{
		int num = (int)(time * (float)_segmentCount) / _segmentCountPerCurve;
		if (num >= CurveCount)
		{
			num = CurveCount - 1;
		}
		return num;
	}

	public Vector2 GetPosition(float currTime)
	{
		if (float.IsNaN(currTime) || float.IsInfinity(currTime))
		{
			currTime = 0f;
		}
		int curveNumber = GetCurveNumber(currTime);
		if (curveNumber < 0)
		{
			return Vector2.Zero;
		}
		float num = (float)curveNumber / (float)CurveCount;
		float t = (currTime - num) * (float)CurveCount;
		int num2 = curveNumber * 3;
		return GetPosition(t, ControlPointList[num2], ControlPointList[num2 + 1], ControlPointList[num2 + 2], ControlPointList[num2 + 3]);
	}

	public Vector2 GetPositionFast(float currTime)
	{
		if (float.IsNaN(currTime))
		{
			currTime = 0f;
		}
		int num = (int)currTime;
		if (num > CurveCount - 1)
		{
			num = CurveCount - 1;
		}
		float t = currTime - (float)num;
		int num2 = num * 3;
		return GetPosition(t, ControlPointList[num2], ControlPointList[num2 + 1], ControlPointList[num2 + 2], ControlPointList[num2 + 3]);
	}

	public float GetDirection(float currTime)
	{
		return Utils.GetRotationTo(GetPosition(currTime), GetPosition(currTime + 1f / (float)_segmentCount));
	}

	private Vector2 GetPosition(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
	{
		float num = 3f * (p1.X - p0.X);
		float num2 = 3f * (p1.Y - p0.Y);
		float num3 = 3f * (p2.X - p1.X) - num;
		float num4 = 3f * (p2.Y - p1.Y) - num2;
		float num5 = p3.X - p0.X - num - num3;
		float num6 = p3.Y - p0.Y - num2 - num4;
		float num7 = t * t * t;
		float num8 = t * t;
		float x = num5 * num7 + num3 * num8 + num * t + p0.X;
		float y = num6 * num7 + num4 * num8 + num2 * t + p0.Y;
		return new Vector2(x, y);
	}

	public void SplitAt(float time)
	{
		int curveNumber = GetCurveNumber(time);
		int num = curveNumber * 3;
		float num2 = (float)curveNumber / (float)CurveCount;
		float num3 = (time - num2) * (float)CurveCount;
		Vector2 vector = ControlPointList[num];
		Vector2 vector2 = ControlPointList[num + 1];
		Vector2 vector3 = ControlPointList[num + 2];
		Vector2 vector4 = ControlPointList[num + 3];
		Vector2 vector5 = (vector2 - vector) * num3 + vector;
		Vector2 vector6 = (vector3 - vector2) * num3 + vector2;
		Vector2 vector7 = (vector4 - vector3) * num3 + vector3;
		Vector2 vector8 = (vector6 - vector5) * num3 + vector5;
		Vector2 vector9 = (vector7 - vector6) * num3 + vector6;
		Vector2 item = (vector9 - vector8) * num3 + vector8;
		ControlPointList[num + 1] = vector5;
		ControlPointList[num + 2] = vector7;
		ControlPointList.Insert(num + 2, vector8);
		ControlPointList.Insert(num + 3, item);
		ControlPointList.Insert(num + 4, vector9);
		CalculateLength();
		List<float> list = new List<float>();
		List<float> list2 = new List<float>();
		for (int i = 0; i <= CurveCount; i++)
		{
			list2.Add((float)i * 1f / (float)CurveCount);
		}
		bool flag = false;
		for (int j = 0; j <= CurveCount - 1; j++)
		{
			float num4 = (float)j * 1f / (float)(CurveCount - 1);
			if (!flag && num4 >= time)
			{
				list.Add(time);
				flag = true;
			}
			list.Add(num4);
		}
		foreach (BehaviorEventNode eventNode in EventNodeList)
		{
			int index = 0;
			int index2 = 0;
			for (int k = 0; k < list.Count; k++)
			{
				if (list[k] >= eventNode.Time)
				{
					index2 = k;
					break;
				}
				index = k;
			}
			float num5 = list[index];
			float num6 = list[index2];
			float num7 = list2[index];
			float num8 = list2[index2];
			float num9 = num6 - num5;
			if (num9 != 0f)
			{
				float time2 = (eventNode.Time - num5) * (num8 - num7) / num9 + num7;
				eventNode.Time = time2;
			}
			else
			{
				eventNode.Time = num7;
			}
		}
	}

	public PathPointType GetPointType(int pointNumber)
	{
		if (pointNumber < 0 || pointNumber >= ControlPointList.Count)
		{
			return PathPointType.Wrong;
		}
		if (pointNumber % 3 == 0)
		{
			return PathPointType.EndPoint;
		}
		if ((pointNumber - 1) % 3 == 0)
		{
			return PathPointType.ControlPoint1;
		}
		return PathPointType.ControlPoint2;
	}

	public void AddPoint(float x, float y)
	{
		AddPoint(new Vector2(x, y));
	}

	internal void AddPoint(Vector2 point)
	{
		ControlPointList.Add(point);
	}

	internal void InsertPoint(int index, Vector2 point)
	{
		ControlPointList.Insert(index, point);
	}

	public void AddCurve(Vector2 endPoint, bool recalculate = true, float rotation = 0f)
	{
		if (ControlPointList.Count == 0)
		{
			ControlPointList.Add(Vector2.Zero);
		}
		Vector2 vector = ControlPointList[ControlPointList.Count - 1];
		Vector2 vector2 = ((ControlPointList.Count <= 1) ? Vector2.Transform(new Vector2(100f, 0f), Matrix.CreateRotationZ(rotation)) : (ControlPointList[ControlPointList.Count - 2] - vector));
		Vector2 point = vector - vector2;
		AddPoint(point);
		point = endPoint + vector2;
		AddPoint(point);
		AddPoint(endPoint);
		if (recalculate)
		{
			CalculateLength();
		}
		else
		{
			CurveCount++;
		}
		float deltaTimeFactor = (float)(CurveCount - 1) / (float)CurveCount;
		EventNodeList.ForEach((BehaviorEventNode node) =>
		{
			node.Time *= deltaTimeFactor;
		});
	}

	public int GetEventNodeCount(float time)
	{
		int num = 0;
		foreach (BehaviorEventNode eventNode in EventNodeList)
		{
			if (eventNode.Time == time)
			{
				num++;
			}
		}
		return num;
	}

	public void RemoveCurve(int number)
	{
		float endPointTime = GetEndPointTime(number);
		if (number > 0 && number < CurveCount)
		{
			for (int num = 1; num >= -1; num--)
			{
				int index = number * 3 + num;
				ControlPointList.RemoveAt(index);
			}
		}
		else if (number == 0)
		{
			for (int num2 = 2; num2 >= 0; num2--)
			{
				int index2 = number * 3 + num2;
				ControlPointList.RemoveAt(index2);
			}
		}
		else if (number == CurveCount)
		{
			for (int num3 = 0; num3 >= -2; num3--)
			{
				int index3 = number * 3 + num3;
				ControlPointList.RemoveAt(index3);
			}
		}
		if (ControlPointList.Count < 3)
		{
			ControlPointList.Clear();
		}
		CalculateLength();
		float num4 = (float)(CurveCount + 1) / (float)CurveCount;
		for (int num5 = EventNodeList.Count - 1; num5 >= 0; num5--)
		{
			BehaviorEventNode behaviorEventNode = EventNodeList[num5];
			if (behaviorEventNode.Time < endPointTime)
			{
				behaviorEventNode.Time *= num4;
			}
			else
			{
				behaviorEventNode.Time -= (1f - behaviorEventNode.Time) * (1f / (float)CurveCount);
			}
			if (behaviorEventNode.Time > 1f || behaviorEventNode.Time < 0f)
			{
				behaviorEventNode.IsDestroyd = true;
				EventNodeList.RemoveAt(num5);
			}
		}
	}

	public string GetClipBoardString()
	{
		string text = $"[path = new_path] {Environment.NewLine}";
		Vector2 vector = ControlPointList[0];
		foreach (Vector2 controlPoint in ControlPointList)
		{
			Vector2 vector2 = controlPoint - vector;
			text += $"\tp = {(int)vector2.X}, {(int)vector2.Y}{Environment.NewLine}";
		}
		return text + Environment.NewLine;
	}

	internal void LoadFromIniFile(IniFile iniFile, GameContent content)
	{
		while (iniFile.NextLine())
		{
			if (!DataLoader.CheckParams(iniFile, content))
			{
				if (iniFile.ValueType == IniFileValueType.MainSection)
				{
					break;
				}
				if (iniFile.KeyEquals("p") && iniFile.TryGetFloatArray(2, out var resultArray))
				{
					AddPoint(resultArray[0], resultArray[1]);
				}
			}
		}
		FileName = iniFile.FileName;
		CalculateLength();
	}
}
