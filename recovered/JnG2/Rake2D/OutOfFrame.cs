namespace Rake2D;

public enum OutOfFrame
{
	None = 0,
	OnLeft = 1,
	OnRight = 2,
	OnTop = 4,
	OnBottom = 8,
	Undefined = 64,
	OnLeftAndRight = 3,
	OnTopAndBottom = 12
}
