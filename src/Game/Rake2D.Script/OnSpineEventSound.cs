namespace Rake2D.Script;

public class OnSpineEventSound : OnSpineEventBase
{
	private int _volume;

	private bool _isLooped;

	public override void Init()
	{
		base.Init();
		_volume = ParamInt("volume", 100);
		_isLooped = ParamBool("sound_loop");
	}

	protected override void DoAction()
	{
		string text = ParamRandomString("sound_id");
		if (!text.IsNullOrEmpty())
		{
			if (Utils.Equals(text, "none"))
			{
				MyObject.StopSound(bLoopedOnly: false);
			}
			else
			{
				MyObject.PlaySound(text, _volume, _isLooped);
			}
		}
	}
}
