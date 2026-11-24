using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class ShotStreamData
{
	public int Number;

	public float ShotDamage;

	public float Heat;

	public float ShotDelay;

	public float StartDelay;

	public int ZOrder;

	public FloatRange ArcRange;

	public float ArcSpeed;

	public int SpreadMode;

	public int ShotCount;

	public FloatRange ShotRotationRange;

	public bool IsShotAngleAbsolute;

	public FloatRange ShotRotationSpeedRange;

	public FloatRange ShotSpeedRange;

	public FloatRange ShotScaleRange;

	public FloatRange ShotLifeTimeRange;

	public bool IsLifeTimeUsedToDPS;

	public bool ResetFire;

	public PartData ShotData;

	public List<Vector2> PathControlPointList;

	public int PathFlipMode;

	public Vector2Range OffsetRange;

	public string SoundId;

	public string SoundStopId;

	public bool IsSoundLooped;

	public bool IsGunFireLooped;

	public bool IsSingleShot;

	public bool ShotAsChild;

	public bool GunFireUseStreamRotation;

	public Vector2 GunFireoOffset;

	public Vector2 GunFireScale;

	public bool IsSingleGunFire;

	public PartData GunFireData;

	public float FireRate => 1f / ShotDelay * (float)ShotCount;

	public string GunFireId
	{
		set
		{
			GunFireData = GameCore.GetPartData(value);
			if (GunFireData == null)
			{
				GameCore.LogErr("Unable to find gun fire data '{0}'", value);
			}
		}
	}

	public ShotStreamData()
	{
		ShotScaleRange = new FloatRange(1f);
		GunFireScale = new Vector2(1f, 1f);
	}

	public void SetPathControlPoints(float[] cpArray)
	{
		PathControlPointList = new List<Vector2>();
		for (int i = 0; i < cpArray.Length; i += 2)
		{
			PathControlPointList.Add(new Vector2(cpArray[i], cpArray[i + 1]));
		}
	}
}
