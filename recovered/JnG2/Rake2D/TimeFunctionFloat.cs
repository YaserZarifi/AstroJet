using System.Collections.Generic;

namespace Rake2D;

public class TimeFunctionFloat : TimeFunctionValue
{
	public float StartValue;

	public float DeltaValue;

	public List<TimeFunctionFloat> MyList;

	public float GetValue(float time)
	{
		if (Duration == 0f)
		{
			return StartValue;
		}
		return StartValue + DeltaValue * (time - StartTime) / Duration;
	}

	public TimeFunctionFloat Clone()
	{
		return new TimeFunctionFloat
		{
			StartTime = StartTime,
			StartValue = StartValue,
			Duration = Duration,
			DeltaValue = DeltaValue
		};
	}
}
