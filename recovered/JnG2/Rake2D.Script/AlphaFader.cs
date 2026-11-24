namespace Rake2D.Script;

public class AlphaFader : BehaviorScript
{
	private float _fadeInTime;

	private float _fadeOutTime;

	private float _fullTime;

	private float _timer;

	private float _originalAlpha;

	private bool _bApplyToChildren;

	private bool _bFadeOutByLifeTime = true;

	private bool _bDestroyOnFadeOut = true;

	private bool _parentAlphaStart;

	private bool _useStateAlpha;

	private float _lifeTime
	{
		get
		{
			if (MyObject.LifeTime < float.MaxValue || MyObject.IsMainPart)
			{
				return MyObject.LifeTime;
			}
			return MyObject.ParentObject.LifeTime;
		}
	}

	public override void RegisterEvents()
	{
		if (_doInEditor && GameCore.IsEditorEnabled)
		{
			MyObject.OnEditorUpdate += Update;
		}
		MyObject.OnUpdate += Update;
		if (_useStateAlpha)
		{
			_originalAlpha = (float)(int)MyObject.Alpha / 255f;
		}
		_timer = 0f;
		if (MyObject.ParentObject != null && _parentAlphaStart)
		{
			MyObject.Alpha = MyObject.ParentObject.Alpha;
			MyObject.CurrentState.Color.A = MyObject.Alpha;
		}
		if (_fadeInTime > 0f && (_doInEditor || !GameCore.IsEditorEnabled))
		{
			MyObject.SetFadeAlpha(0f, _bApplyToChildren);
		}
	}

	public override void Init()
	{
		base.Init();
		_fadeInTime = Param("time_in") / 1000f;
		_fadeOutTime = Param("time_out") / 1000f;
		_originalAlpha = (float)(int)MyObject.Alpha / 255f;
		if (_fadeOutTime <= 0f)
		{
			int num = ParamInt("percent_out", 100);
			if (num < 100)
			{
				_fadeOutTime = _lifeTime - _lifeTime * (float)num / 100f;
			}
		}
		else
		{
			_bFadeOutByLifeTime = ParamBool("out_life_time", defaultValue: true);
			_bDestroyOnFadeOut = ParamBool("destroy_on_out", defaultValue: true);
		}
		if (!_bFadeOutByLifeTime)
		{
			_fullTime = Param("time_full") / 1000f;
		}
		_bApplyToChildren = ParamBool("apply_to_children", defaultValue: true);
		_parentAlphaStart = ParamBool("parent_alpha_start");
		_useStateAlpha = ParamBool("state_alpha");
	}

	public void Update(float elapsedTime)
	{
		_timer += elapsedTime;
		if (_fadeInTime > 0f && _timer <= _fadeInTime)
		{
			MyObject.SetFadeAlpha(_originalAlpha * (_timer / _fadeInTime), _bApplyToChildren);
		}
		else if (_fadeOutTime > 0f)
		{
			if (!_bFadeOutByLifeTime)
			{
				float num = 1f - (_timer - _fadeInTime - _fullTime) / _fadeOutTime;
				if (num <= 0f)
				{
					if (_bDestroyOnFadeOut)
					{
						MyObject.Destroy();
					}
					MyObject.SetFadeAlpha(0f, _bApplyToChildren);
					MyObject.OnUpdate -= Update;
					MyObject.OnEditorUpdate -= Update;
				}
				else
				{
					MyObject.SetFadeAlpha((num < 1f) ? (_originalAlpha * num) : _originalAlpha, _bApplyToChildren);
				}
			}
			else if (_fadeOutTime >= _lifeTime)
			{
				float num2 = _lifeTime / _fadeOutTime;
				if (num2 <= 0f)
				{
					if (_bDestroyOnFadeOut)
					{
						MyObject.Destroy();
					}
					else
					{
						MyObject.SetFadeAlpha(0f, _bApplyToChildren);
					}
				}
				else
				{
					MyObject.SetFadeAlpha(_originalAlpha * num2, _bApplyToChildren);
				}
			}
			else
			{
				MyObject.SetFadeAlpha(_originalAlpha, _bApplyToChildren);
			}
		}
		else
		{
			MyObject.SetFadeAlpha(_originalAlpha, _bApplyToChildren);
			MyObject.OnUpdate -= Update;
			MyObject.OnEditorUpdate -= Update;
			IsDone = true;
		}
	}
}
