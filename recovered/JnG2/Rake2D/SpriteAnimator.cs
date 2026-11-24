namespace Rake2D;

public class SpriteAnimator
{
	public bool IsLooped;

	public AnimStateEnum AnimState;

	private int[] _frameArray;

	private float _timer;

	private int _currentFrame;

	public int AtlasFrame
	{
		get
		{
			if (_currentFrame >= 0)
			{
				return _frameArray[_currentFrame];
			}
			return -1;
		}
	}

	public float Duration { get; internal set; }

	public void SetFrames(int[] frameArray, float duration)
	{
		_frameArray = frameArray;
		Duration = duration;
		_timer = 0f;
		_currentFrame = 0;
		AnimState = AnimStateEnum.Start;
	}

	public void ClearSpriteAnimator()
	{
		IsLooped = false;
		AnimState = AnimStateEnum.Unknown;
		Duration = 0f;
		_frameArray = null;
		_timer = 0f;
		_currentFrame = 0;
	}

	public bool Update(float elapsedTime)
	{
		if (AnimState != AnimStateEnum.LastReached)
		{
			_timer += elapsedTime;
			if (_timer >= Duration)
			{
				if (IsLooped)
				{
					AnimState = AnimStateEnum.Forward;
					_timer = 0f;
				}
				else
				{
					AnimState = AnimStateEnum.LastReached;
					_currentFrame = _frameArray.Length - 1;
					_timer = Duration;
				}
				return true;
			}
			AnimState = AnimStateEnum.Forward;
			_currentFrame = (int)(_timer / (Duration / (float)_frameArray.Length));
		}
		return false;
	}
}
