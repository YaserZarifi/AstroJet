using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Sound;

public static class SoundUtils
{
	public static float GetVolumePercentageByVirtualScreePosition(Vector2 vsPos)
	{
		float num = 1f;
		if (vsPos.X > 1920f)
		{
			num = Math.Max(0f, 1f - (vsPos.X - 1920f) / 150f);
		}
		else if (vsPos.X < 0f)
		{
			num = Math.Max(0f, 1f + vsPos.X / 150f);
		}
		if (vsPos.Y > 1080f)
		{
			num *= Math.Max(0f, 1f - (vsPos.Y - 1080f) / 300f);
		}
		else if (vsPos.Y < 0f)
		{
			num *= Math.Max(0f, 1f + vsPos.Y / 300f);
		}
		return num;
	}

	public static float GetPaningByVirtualScreePosition(Vector2 vsPos)
	{
		if (Options.SoundPanningLevel <= 0)
		{
			return 0f;
		}
		float num = Utils.Clamp(vsPos.X / 1920f, 0f, 1f) - 0.5f;
		if (Options.SoundPanningLevel == 1)
		{
			return num / 5f;
		}
		return num / 2f;
	}

	public static float GetExponentialVolume(float normalVolume)
	{
		return normalVolume * normalVolume / 100f;
	}

	public static float GetNormalVolume(float exponentialVolume)
	{
		return (float)Math.Round(Utils.Sqrt(exponentialVolume * 100f));
	}
}
