using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D;

public class PartMap
{
	public string SkinId;

	public Dictionary<string, string> MapIds;

	public Dictionary<string, float> PartOffsetXPercentages;

	public PartMap()
	{
		PartOffsetXPercentages = new Dictionary<string, float>();
	}

	public void AddItem(string originalId, string newPartId)
	{
		if (MapIds == null)
		{
			MapIds = new Dictionary<string, string>();
		}
		MapIds[originalId] = newPartId;
	}

	public void ApplyOffsetXToAllParts(GameObject mainPart)
	{
		foreach (KeyValuePair<string, float> partOffsetXPercentage in PartOffsetXPercentages)
		{
			GameObject childByIdDeep = mainPart.GetChildByIdDeep(partOffsetXPercentage.Key);
			if (childByIdDeep != null)
			{
				childByIdDeep.Position.X = childByIdDeep.PartData.TargetOffset.X + (childByIdDeep.PartData.TargetOffset.Y - childByIdDeep.PartData.TargetOffset.X) * partOffsetXPercentage.Value;
			}
		}
	}

	public PartData CreateModifiedData(PartData originalData, string id, string idPrefix = null)
	{
		PartData partData = new PartData(id);
		partData.CopyValuesFrom(originalData, mergeStates: false);
		if (MapIds != null)
		{
			foreach (KeyValuePair<string, string> mapId in MapIds)
			{
				PartData partData2 = partData.FindChildOnPath(mapId.Key);
				if (partData2 != null)
				{
					string text = ((idPrefix == null) ? mapId.Value : (idPrefix + mapId.Value));
					PartData partData3 = GameCore.GetPartData(text);
					if (partData3 != null)
					{
						Vector2 targetOffset = partData2.TargetOffset;
						partData2.CopyValuesFrom(partData3, mergeStates: true);
						partData2.TargetOffset = targetOffset;
					}
					else
					{
						GameCore.LogErr("Unable to find part '{0}' for original part '{1}'", text, partData2.GetHierarchyId());
					}
					partData2.SkinId = SkinId;
				}
			}
		}
		partData.SkinId = SkinId;
		return partData;
	}

	public void SaveToFile(StreamWriter file)
	{
		SaveGame.WriteLine($"[part_map={SkinId}]", file);
		if (MapIds != null)
		{
			foreach (KeyValuePair<string, string> mapId in MapIds)
			{
				SaveGame.WriteLine($"\tm.{mapId.Key}={mapId.Value}", file);
			}
		}
		if (PartOffsetXPercentages == null)
		{
			return;
		}
		foreach (KeyValuePair<string, float> partOffsetXPercentage in PartOffsetXPercentages)
		{
			SaveGame.WriteLine($"\tox.{partOffsetXPercentage.Key}={(int)(partOffsetXPercentage.Value * 100f)}", file);
		}
	}
}
