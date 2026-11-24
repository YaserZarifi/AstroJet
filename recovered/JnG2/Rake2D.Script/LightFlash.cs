namespace Rake2D.Script;

public class LightFlash : BehaviorScript
{
	private float _fadeInTime;

	private float _fadeOutTime;

	private float _timer;

	private bool _bFadeOutByLifeTime = true;

	private bool _bDestroyOnFadeOut = true;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
		if (_doInEditor)
		{
			MyObject.OnEditorUpdate += Update;
		}
		_timer = 0f;
		if (_fadeInTime >= 0f)
		{
			MyObject.Alpha = 0;
		}
	}

	public override void Init()
	{
		_fadeInTime = Param("time_in") / 1000f;
		_fadeOutTime = Param("time_out") / 1000f;
		if (_fadeOutTime <= 0f)
		{
			int num = ParamInt("percent_out", 100);
			if (num < 100)
			{
				_fadeOutTime = MyObject.LifeTime - MyObject.LifeTime * (float)num / 100f;
			}
		}
		else
		{
			_bFadeOutByLifeTime = ParamBool("out_life_time", defaultValue: true);
			_bDestroyOnFadeOut = ParamBool("destroy_on_out", defaultValue: true);
		}
	}

	public void Update(float elapsedTime)
	{
		_timer += elapsedTime;
		if (_fadeInTime > 0f && _timer <= _fadeInTime)
		{
			MyObject.Alpha = (byte)((float)(int)MyObject.CurrentState.Color.A * _timer / _fadeInTime);
		}
		else if (_fadeOutTime > 0f)
		{
			if (!_bFadeOutByLifeTime)
			{
				float num = (float)(int)MyObject.CurrentState.Color.A - _timer / _fadeOutTime;
				if (num <= 0f)
				{
					if (_bDestroyOnFadeOut)
					{
						MyObject.Destroy();
					}
					else
					{
						MyObject.Alpha = 0;
					}
				}
				else
				{
					MyObject.Alpha = (byte)num;
				}
			}
			else if (_fadeOutTime >= MyObject.LifeTime)
			{
				float num2 = (float)(int)MyObject.CurrentState.Color.A * MyObject.LifeTime / _fadeOutTime;
				if (num2 <= 0f)
				{
					if (_bDestroyOnFadeOut)
					{
						MyObject.Destroy();
					}
					else
					{
						MyObject.Alpha = 0;
						MyObject.OnUpdate -= Update;
						if (_doInEditor)
						{
							MyObject.OnEditorUpdate -= Update;
						}
					}
				}
				else
				{
					MyObject.Alpha = (byte)num2;
				}
			}
			else
			{
				MyObject.Alpha = MyObject.CurrentState.Color.A;
			}
		}
		MyObject.SetScale(MyObject.PartData.DefaultPartState.Scale * ((float)(int)MyObject.Alpha / 255f));
	}
}
