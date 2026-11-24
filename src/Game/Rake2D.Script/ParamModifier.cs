namespace Rake2D.Script;

public struct ParamModifier
{
	public bool IgnoreBaseValue;

	public float Increase;

	public float Percentage;

	public float GetValue(float baseValue)
	{
		return ((IgnoreBaseValue ? 0f : baseValue) + Increase) * (1f + Percentage);
	}
}
