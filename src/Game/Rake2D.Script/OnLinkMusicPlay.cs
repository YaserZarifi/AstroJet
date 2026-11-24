namespace Rake2D.Script;

public class OnLinkMusicPlay : OnLinkBehavior
{
	private string _musicFilename;

	public override void Init()
	{
		base.Init();
		_musicFilename = ParamString("music");
	}

	protected override void DoAction(GameObject senderObject, string linkIn, int linkValue)
	{
		GameCore.SoundSystem.LoadMusic(_musicFilename);
		GameCore.SoundSystem.PlayMusic(isLooped: true);
	}
}
