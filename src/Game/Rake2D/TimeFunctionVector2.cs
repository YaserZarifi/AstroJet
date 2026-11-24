using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D;

public class TimeFunctionVector2 : TimeFunctionValue
{
	public Vector2 StartValue;

	public Vector2 DeltaValue;

	public List<TimeFunctionVector2> MyList;

	public Vector2 GetValue(float time)
	{
		if (Duration == 0f)
		{
			return StartValue;
		}
		return StartValue + DeltaValue * (time - StartTime) / Duration;
	}

	public TimeFunctionVector2 Clone()
	{
		return new TimeFunctionVector2
		{
			StartTime = StartTime,
			StartValue = StartValue,
			Duration = Duration,
			DeltaValue = DeltaValue
		};
	}
}
