using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D;

public class TimeFunctionColor : TimeFunctionValue
{
	public int StartR;

	public int StartG;

	public int StartB;

	public int DeltaR;

	public int DeltaG;

	public int DeltaB;

	public List<TimeFunctionColor> MyList;

	public Color GetValue(float time)
	{
		if (Duration == 0f)
		{
			return new Color(StartR, StartG, StartB);
		}
		return new Color((int)((float)StartR + (float)DeltaR * (time - StartTime) / Duration), (int)((float)StartG + (float)DeltaG * (time - StartTime) / Duration), (int)((float)StartB + (float)DeltaB * (time - StartTime) / Duration));
	}

	public TimeFunctionColor Clone()
	{
		return new TimeFunctionColor
		{
			StartTime = StartTime,
			StartR = StartR,
			StartG = StartG,
			StartB = StartB,
			Duration = Duration,
			DeltaR = DeltaR,
			DeltaG = DeltaG,
			DeltaB = DeltaB
		};
	}
}
