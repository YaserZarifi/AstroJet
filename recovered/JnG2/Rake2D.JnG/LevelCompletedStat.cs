using Microsoft.Xna.Framework;

namespace Rake2D.JnG;

public class LevelCompletedStat
{
	public string Caption;

	public string Unit;

	public float CurrValue;

	public float TargetValue;

	public float MaxValue;

	public bool ShowPercent;

	public LevelCompletedStat CompareStat;

	private Color CaptionColor;

	private Color ValueColor;

	public LevelCompletedStat()
	{
		CaptionColor = Color.White;
		ValueColor = Color.Yellow;
	}

	public void Update(float elapsedTime)
	{
		Utils.ChangeValueToTarget(ref CurrValue, TargetValue, TargetValue / 2f * elapsedTime);
	}

	public int GetPercent()
	{
		if (MaxValue > 0f)
		{
			return (int)(CurrValue / MaxValue * 100f);
		}
		return 100;
	}
}
