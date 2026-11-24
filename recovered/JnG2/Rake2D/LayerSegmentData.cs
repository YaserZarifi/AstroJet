using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D;

public class LayerSegmentData
{
	public Layer MyLayer;

	public List<LayerSegmentObjectData> ObjectDataList;

	public LayerSegmentData(Layer layer)
	{
		MyLayer = layer;
		ObjectDataList = new List<LayerSegmentObjectData>();
	}

	public LayerSegmentObjectData AddObjectData(string id)
	{
		PartData partData = GameCore.GetPartData(id);
		if (partData == null)
		{
			partData = MyLayer.MyLevel.Content.CreatePartData(id);
			PartData partData2 = GameCore.GetPartData("undefined_object");
			if (partData2 != null)
			{
				partData.CopyValuesFrom(partData2, mergeStates: false);
			}
			GameCore.LogErr("Unable to add object data '{0}' to layer segment.", id);
		}
		LayerSegmentObjectData layerSegmentObjectData = new LayerSegmentObjectData(partData);
		ObjectDataList.Add(layerSegmentObjectData);
		return layerSegmentObjectData;
	}

	public void Build(long pkOffset, Vector2 posOffset, Dictionary<long, GameObject> pkDictionary, List<KeyValuePair<GameObject, long>> linkList, bool bSkipExpiredObjects, ref List<GameObject> createdObjectList)
	{
		MyLayer.MyLevel.CurrentLayer = MyLayer;
		foreach (LayerSegmentObjectData objectData in ObjectDataList)
		{
			Vector2 pos = posOffset + objectData.Position;
			if ((!GameCore.IsEditorEnabled && (objectData.DifficultyRange.From > GameJnG.Difficulty || objectData.DifficultyRange.To < GameJnG.Difficulty || (objectData.GameModeList != null && !objectData.GameModeList.Contains((int)GameJnG.GameMode)) || (objectData.ForPlayerCount == 1 && GameJnG.PlayerCount != 1) || (objectData.ForPlayerCount == 2 && GameJnG.PlayerCount < 2))) || (bSkipExpiredObjects && GameCore.SkipExpiredObjects && (objectData.PartData.SkipExpired > 0 || (objectData.PartData.SkipExpired < 0 && ((objectData.PartData.TargetClass & TargetClass.EnemyAir) != 0 || (objectData.PartData.TargetClass & TargetClass.EnemyGround) != 0))) && pos.X > 1920f && pos.X < MyLayer.MyLevel.Camera.Position.X + 1920f))
			{
				continue;
			}
			GameObject gameObject = MyLayer.CreateObjectByPartData(objectData.PartData, pos, objectData.Scale, objectData.Rotation, objectData.ZOrder, objectData.CreatePhysBody);
			if (gameObject == null)
			{
				continue;
			}
			gameObject.Pk = objectData.Pk + pkOffset;
			gameObject.DifficultyRange = objectData.DifficultyRange;
			if (objectData.GameModeList != null)
			{
				gameObject.GameModeList = new List<int>(objectData.GameModeList);
			}
			gameObject.ForPlayerCount = objectData.ForPlayerCount;
			MyLayer.MyLevel.MaxObjectPk = Math.Max(MyLayer.MyLevel.MaxObjectPk, gameObject.Pk);
			gameObject.IsLevelObject = true;
			if (objectData.TriggerPoint != Vector2.Zero)
			{
				gameObject.TriggerPoint = objectData.TriggerPoint + posOffset;
			}
			gameObject.TriggerPointMode = objectData.TriggerMode;
			gameObject.SetColor(objectData.Color);
			if (objectData.Path != null)
			{
				gameObject.MyPath = objectData.Path.Clone(gameObject, pkOffset);
				gameObject.MyPath.Move(posOffset);
			}
			if (objectData.TimeLine != null)
			{
				gameObject.MyTimeLine = objectData.TimeLine.Clone(gameObject, pkOffset);
			}
			if (gameObject.PolygonRenderer != null && objectData.PolygonVertexArray != null)
			{
				gameObject.PolygonRenderer.TextureOffset = objectData.PolygonTextureOffset;
				gameObject.PolygonRenderer.TextureMappingMode = objectData.PolygonTextureMappingMode;
				gameObject.PolygonRenderer.IsTopEnabled = objectData.IsPolygonTopEnabled;
				gameObject.PolygonRenderer.IsBottomEnabled = objectData.IsPolygonBottomEnabled;
				gameObject.PolygonRenderer.SetVertices(objectData.PolygonVertexArray);
				gameObject.CreateRigidBodyByPoygonRenderer();
			}
			if (gameObject.Pk > 0)
			{
				try
				{
					pkDictionary.Add(gameObject.Pk, gameObject);
				}
				catch (Exception ex)
				{
					GameCore.LogErr("Unable to add pk {0} in building segment of level '{1}', layer {2}. Ex: {3}", gameObject.Pk, MyLayer.MyLevel.Id, MyLayer.Id, ex);
				}
			}
			if (objectData.LinkPkArrray != null)
			{
				int[] linkPkArrray = objectData.LinkPkArrray;
				foreach (int num in linkPkArrray)
				{
					linkList.Add(new KeyValuePair<GameObject, long>(gameObject, num + pkOffset));
				}
			}
			createdObjectList.Add(gameObject);
		}
	}
}
