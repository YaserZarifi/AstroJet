using System.Collections.Generic;

namespace Rake2D.Debug;

public static class DebugUtils
{
	public static List<ParticleEmitterData> DumpUnusedEmitters(GameLevel level)
	{
		List<ParticleEmitterData> allParticleEmitterData = GetAllParticleEmitterData(level);
		List<ParticleEmitterData> list = new List<ParticleEmitterData>();
		foreach (ParticleEmitterData item in allParticleEmitterData)
		{
			foreach (Layer layer in level.LayerList)
			{
				_ = layer;
				foreach (PartData allPartDatum in GetAllPartData(level))
				{
					if (!IsUsingEmitterData(allPartDatum, item))
					{
						continue;
					}
					goto IL_008e;
				}
			}
			list.Add(item);
			IL_008e:;
		}
		GameCore.Log("--- Dump of unused emitters ---");
		foreach (ParticleEmitterData item2 in list)
		{
			GameCore.Log("  {0}", item2.Id);
		}
		GameCore.Log("Total count: {0} / {1}", list.Count, allParticleEmitterData.Count);
		GameCore.Log("------");
		return list;
	}

	private static bool IsUsingEmitterData(GameObject go, ParticleEmitterData data)
	{
		if (go.Emitter != null && go.Emitter.Data == data)
		{
			return true;
		}
		if (go.ChildObjectListFront != null)
		{
			foreach (GameObject item in go.ChildObjectListFront)
			{
				if (IsUsingEmitterData(item, data))
				{
					return true;
				}
			}
		}
		if (go.ChildObjectListBack != null)
		{
			foreach (GameObject item2 in go.ChildObjectListBack)
			{
				if (IsUsingEmitterData(item2, data))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IsUsingEmitterData(PartData partData, ParticleEmitterData data)
	{
		if (partData.EmitterData == data)
		{
			return true;
		}
		if (partData.ChildList != null)
		{
			foreach (PartData child in partData.ChildList)
			{
				if (IsUsingEmitterData(child, data))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static List<ParticleEmitterData> GetAllParticleEmitterData(GameLevel level)
	{
		List<ParticleEmitterData> list = new List<ParticleEmitterData>();
		list.AddRange(GameCore.GlobalContent.ParticleEmitterDatas.Values);
		list.AddRange(level.Content.ParticleEmitterDatas.Values);
		return list;
	}

	private static List<PartData> GetAllPartData(GameLevel level)
	{
		List<PartData> list = new List<PartData>();
		list.AddRange(GameCore.GlobalContent.PartDatas.Values);
		list.AddRange(level.Content.PartDatas.Values);
		return list;
	}
}
