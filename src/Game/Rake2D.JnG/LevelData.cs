using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.JnG;

public class LevelData
{
	public string Id;

	public string TextId;

	public string FileName;

	public bool FileExists;

	public string LoadingAtlasId;

	public string WinToLevelId;

	public bool IsMandatory = true;

	public bool IsHidden;

	public bool IsFinalLevel;

	public int DebriefingMode = 1;

	public Vector2 PlanetMapPosition;

	public int PlanetNumber;

	public int PreviewNumber;

	public int PlanetInfoOffset;

	public float DamageToHeroFactor = -1f;

	public float HPFactor = -1f;

	public float ScoreFactor = -1f;

	public Dictionary<string, HashSet<int>> AvailableFindDictionary;

	public bool IsVisible
	{
		get
		{
			if (IsHidden && (!GameJnG.TryGetLevelStats(this, out var levelStats) || !levelStats.IsVisible))
			{
				return false;
			}
			return true;
		}
	}

	public LevelData()
	{
		AvailableFindDictionary = new Dictionary<string, HashSet<int>>();
	}

	public override string ToString()
	{
		return Id;
	}
}
