using ManagedBass;

namespace Rake2D.Sound;

public class Sound2DBass : ISound2D
{
	private SoundData _soundData;

	private int _bassChannel;

	private float _startVolume;

	private GameObject _myObject;

	public float StartVolume
	{
		get
		{
			return _startVolume;
		}
		set
		{
			_startVolume = value;
		}
	}

	public bool IsFinished => Bass.ChannelIsActive(_bassChannel) == PlaybackState.Stopped;

	public bool IsLooped
	{
		get
		{
			return (Bass.ChannelFlags(_bassChannel, BassFlags.Default, BassFlags.Default) & BassFlags.Loop) != 0;
		}
		set
		{
			if (value)
			{
				Bass.ChannelFlags(_bassChannel, BassFlags.Loop, BassFlags.Loop);
			}
			else
			{
				Bass.ChannelFlags(_bassChannel, BassFlags.Default, BassFlags.Loop);
			}
		}
	}

	public float PlaybackSpeed
	{
		get
		{
			return 1f;
		}
		set
		{
		}
	}

	public Sound2DBass(int bassChannel, SoundData data, float volume)
	{
		_soundData = data;
		_bassChannel = bassChannel;
		StartVolume = volume;
		SetVolume(StartVolume);
	}

	public void SetVolume(float vol)
	{
		Bass.ChannelSetAttribute(_bassChannel, ChannelAttribute.Volume, _soundData.DefaultVolume * vol / 100f);
	}

	public void SetPan(float pan)
	{
		Bass.ChannelSetAttribute(_bassChannel, ChannelAttribute.Pan, pan);
	}

	public void Stop()
	{
		Bass.ChannelStop(_bassChannel);
	}
}
