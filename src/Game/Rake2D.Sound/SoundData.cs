namespace Rake2D.Sound;

public class SoundData
{
	public string Id;

	public float DefaultVolume = 1f;

	public int MaxSimultaneous = 4;

	public virtual void LoadFromIniFile(IniFile iniFile, GameContent content)
	{
	}

	public virtual void LoadSample()
	{
	}

	public virtual void FreeSample()
	{
	}

	public SoundData(string id)
	{
		Id = id;
	}
}
