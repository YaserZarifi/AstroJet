using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D.Sound;

internal class SoundDataBass : SoundData
{
	internal class SoundSystemBass : ISoundSystem
	{
		public List<SoundData> _allSoundDataList;

		private float _lastTimeFactor;

		private int _musicChannel;

		private float _musicVolumeFactor;

		private float _soundVolumeFactor;

		private float _musicVolume;

		private float _soundVolume;

		public string MusicFileName { get; private set; }

		public float MusicVolume
		{
			get
			{
				return _musicVolume;
			}
			set
			{
				_musicVolume = value;
				float num = _musicVolumeFactor * 100f * (_musicVolumeFactor * 100f) / 100f;
				Bass.Configure(Configuration.GlobalStreamVolume, (int)(_musicVolume * num));
			}
		}

		public float MusicVolumeFactor
		{
			get
			{
				return _musicVolumeFactor;
			}
			set
			{
				_musicVolumeFactor = value;
				MusicVolume = MusicVolume;
			}
		}

		public bool IsMusicPlaying
		{
			get
			{
				if (_musicChannel != 0)
				{
					return Bass.ChannelIsActive(_musicChannel) != PlaybackState.Stopped;
				}
				return false;
			}
		}

		public float GlobalVolume
		{
			get
			{
				return 100f;
			}
			set
			{
			}
		}

		public float SoundVolumeFactor
		{
			get
			{
				return _soundVolumeFactor;
			}
			set
			{
				_soundVolumeFactor = value;
				SoundVolume = SoundVolume;
			}
		}

		public float SoundVolume
		{
			get
			{
				return _soundVolume;
			}
			set
			{
				_soundVolume = value;
				Bass.Configure(Configuration.GlobalSampleVolume, (int)(_soundVolume * 100f * _soundVolumeFactor));
			}
		}

		public void Init(IntPtr windowHandle)
		{
			Bass.Configure(Configuration.IncludeDefaultDevice, NewValue: true);
			if (!Bass.Init(-1, 44100, DeviceInitFlags.Default, windowHandle, (IntPtr)0))
			{
				GameCore.LogErr("Sound system - Bass ver.: {0} failed", Bass.Version);
			}
			else
			{
				GameCore.Log("Sound system - Bass ver.: {0}", Bass.Version);
			}
			Bass.UpdateThreads = 10;
			Bass.Start();
		}

		public void Destroy()
		{
			Bass.Stop();
			Bass.Free();
		}

		public void Clear()
		{
			Bass.Stop();
		}

		public void SetAllSoundData(List<SoundData> list)
		{
			_allSoundDataList = list;
		}

		public bool LoadMusic(string fileName)
		{
			MusicFileName = fileName;
			StopMusic();
			if (fileName == null)
			{
				_musicChannel = 0;
				return false;
			}
			_musicChannel = Bass.CreateStream(fileName, 0L, 0L);
			if (_musicChannel == 0)
			{
				GameCore.LogErr("Unable to load music '{0}' - error code: {1}", fileName, Bass.LastError);
				return false;
			}
			return true;
		}

		public void PlayMusic(bool isLooped)
		{
			if (_musicChannel != 0)
			{
				if (isLooped)
				{
					Bass.ChannelAddFlag(_musicChannel, BassFlags.Loop);
				}
				else
				{
					Bass.ChannelRemoveFlag(_musicChannel, BassFlags.Loop);
				}
				Bass.ChannelPlay(_musicChannel, Restart: true);
			}
		}

		public void StopMusic()
		{
			if (_musicChannel != 0)
			{
				Bass.ChannelStop(_musicChannel);
				Bass.ChannelSetPosition(_musicChannel, 0L);
				Bass.StreamFree(_musicChannel);
				_musicChannel = 0;
			}
		}

		public void PauseMusic()
		{
			if (_musicChannel != 0)
			{
				Bass.ChannelPause(_musicChannel);
			}
		}

		public void ResumeMusic()
		{
			if (_musicChannel != 0)
			{
				Bass.ChannelPlay(_musicChannel);
			}
		}

		public float GetMusicPositionInSec()
		{
			return (float)Bass.ChannelBytes2Seconds(_musicChannel, Bass.ChannelGetPosition(_musicChannel));
		}

		public void SetMusicPositionInSec(float sec)
		{
			Bass.ChannelSetPosition(_musicChannel, Bass.ChannelSeconds2Bytes(_musicChannel, sec));
		}

		public void PlayMusicFromPositionInSec(bool isLooped, float sec)
		{
			PlayMusic(isLooped);
			SetMusicPositionInSec(sec);
		}

		public int GetPlayingSoudsCount()
		{
			return 0;
		}

		public ISound2D PlaySound(SoundData soundData, float volume = 1f, float pan = 0f, bool looped = false)
		{
			if (!GameCore.IsApplicationActive)
			{
				return null;
			}
			int handle = ((SoundDataBass)soundData).Handle;
			int num = Bass.SampleGetChannel(handle);
			if (num == 0)
			{
				GameCore.LogErr("Unable to play sound ({0}) - Err code: {1}", handle, Bass.LastError);
				return null;
			}
			Sound2DBass sound2DBass = new Sound2DBass(num, soundData, volume);
			sound2DBass.SetPan(pan);
			sound2DBass.IsLooped = looped;
			if (_lastTimeFactor != 1f && !GameCore.IsShopMenuEnabled && !GameCore.IsInLevelMenuEnabled)
			{
				Bass.ChannelSetAttribute(num, ChannelAttribute.Frequency, (int)((float)Bass.ChannelGetInfo(num).Frequency * _lastTimeFactor));
			}
			Bass.ChannelPlay(num);
			return sound2DBass;
		}

		public void Pause()
		{
			Bass.Pause();
		}

		public void Resume()
		{
			Bass.Start();
		}

		public void PauseAllSounds()
		{
			foreach (SoundDataBass allSoundData in _allSoundDataList)
			{
				int[] array = Bass.SampleGetChannels(allSoundData.Handle);
				if (array != null)
				{
					int[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						Bass.ChannelPause(array2[i]);
					}
				}
			}
		}

		public void ResumeAllSounds()
		{
			foreach (SoundDataBass allSoundData in _allSoundDataList)
			{
				int[] array = Bass.SampleGetChannels(allSoundData.Handle);
				if (array != null)
				{
					int[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						Bass.ChannelPlay(array2[i]);
					}
				}
			}
		}

		public void StopAllSounds()
		{
			foreach (SoundDataBass allSoundData in _allSoundDataList)
			{
				int[] array = Bass.SampleGetChannels(allSoundData.Handle);
				if (array != null)
				{
					int[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						Bass.ChannelStop(array2[i]);
					}
				}
			}
		}

		public void StopAll()
		{
			Bass.Stop();
			Bass.Start();
		}

		public void Update(float elapsedTime)
		{
			if (_lastTimeFactor == GameCore.TimeFactor || GameCore.IsShopMenuEnabled)
			{
				return;
			}
			_lastTimeFactor = GameCore.TimeFactor;
			foreach (SoundDataBass allSoundData in _allSoundDataList)
			{
				int[] array = Bass.SampleGetChannels(allSoundData.Handle);
				if (array != null)
				{
					int[] array2 = array;
					foreach (int handle in array2)
					{
						Bass.ChannelSetAttribute(handle, ChannelAttribute.Frequency, (int)((float)Bass.ChannelGetInfo(handle).Frequency * _lastTimeFactor));
					}
				}
			}
		}

		public void DrawDebugInfo(SpriteBatch spriteBatch, BitmapFont font)
		{
			Color white = Color.White;
			Vector2 vector = new Vector2(200f, 40f);
			Vector2 pos = vector;
			int num = 0;
			foreach (SoundDataBass item in _allSoundDataList.OrderBy((SoundData d) => d.Id))
			{
				int[] array = Bass.SampleGetChannels(item.Handle);
				if (array == null || array.Length == 0)
				{
					continue;
				}
				int num2 = 0;
				for (int num3 = 0; num3 < array.Length; num3++)
				{
					if (Bass.ChannelIsActive(array[num3]) == PlaybackState.Playing)
					{
						num2++;
						num++;
					}
				}
				if (num2 != 0)
				{
					font.Write(spriteBatch, $"{item.Id}: {array.Length}", pos, white, TextAlign.Left, 250);
					pos.Y += 15f;
					if (pos.Y > 1030f)
					{
						pos.Y = vector.Y;
						pos.X += 300f;
					}
				}
			}
			font.Write(spriteBatch, $"Playing sounds: {num}", vector.X - 4f, vector.Y - 17f, Color.Yellow, TextAlign.Left, 250);
		}
	}

	public string FileName;

	private int _handle;

	public int Handle => _handle;

	public SoundDataBass(string id)
		: base(id)
	{
	}

	public override void LoadSample()
	{
		_handle = 0;
		try
		{
			using Stream stream = GameContent.GetFileStream(FileName);
			if (stream == null)
			{
				return;
			}
			using MemoryStream memoryStream = new MemoryStream();
			stream.CopyTo(memoryStream);
			memoryStream.Position = 0L;
			byte[] array = memoryStream.ToArray();
			_handle = Bass.SampleLoad(array, 0L, array.Length, MaxSimultaneous, BassFlags.Prescan);
			if (_handle == 0)
			{
				GameCore.LogErr("Unable to load sound '{0}' - error code: {1}", FileName, Bass.LastError.ToString());
			}
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Load sound exception: {0}\nIn file: {1}", ex, FileName);
		}
	}

	public override void FreeSample()
	{
		if (_handle > 0)
		{
			Bass.SampleFree(_handle);
		}
	}

	public override void LoadFromIniFile(IniFile iniFile, GameContent content)
	{
		FileName = iniFile.Value;
		while (iniFile.NextLine())
		{
			if (!DataLoader.CheckParams(iniFile, content))
			{
				if (iniFile.ValueType == IniFileValueType.MainSection)
				{
					break;
				}
				if (iniFile.KeyEquals("volume"))
				{
					DefaultVolume = (float)Utils.Clamp(iniFile.GetIntValue(), 0, 100) / 100f;
				}
				else if (iniFile.KeyEquals("max_sim"))
				{
					MaxSimultaneous = Utils.Clamp(iniFile.GetIntValue(), 1, 99);
				}
			}
		}
	}
}
