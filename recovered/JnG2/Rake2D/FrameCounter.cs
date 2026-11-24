namespace Rake2D;

public class FrameCounter
{
	public int ActualizeCount;

	private double _timer;

	private int _avgCounter;

	private double _avgTime;

	private const double _updateDelay = 0.5;

	public int AverageFramesPerSecond { get; private set; }

	public double CurrentFramesPerSecond { get; private set; }

	public void Update(double deltaTime)
	{
		if (!(deltaTime > 0.0))
		{
			return;
		}
		CurrentFramesPerSecond = 1.0 / deltaTime;
		_timer += deltaTime;
		_avgTime += CurrentFramesPerSecond;
		_avgCounter++;
		if (_timer >= 0.5)
		{
			AverageFramesPerSecond = (int)(_avgTime / (double)_avgCounter);
			_avgTime = 0.0;
			_avgCounter = 0;
			_timer -= 0.5;
			if (_timer > 0.5)
			{
				_timer = 0.0;
			}
			ActualizeCount++;
		}
	}
}
