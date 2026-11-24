using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D.Sound;

public interface ISoundSystem
{
	string MusicFileName { get; }

	float GlobalVolume { get; set; }

	float SoundVolume { get; set; }

	float SoundVolumeFactor { get; set; }

	float MusicVolume { get; set; }

	float MusicVolumeFactor { get; set; }

	bool IsMusicPlaying { get; }

	void Init(IntPtr windowHandle);

	void Clear();

	bool LoadMusic(string fileName);

	void PlayMusic(bool isLooped);

	void StopMusic();

	ISound2D PlaySound(SoundData soundData, float volume = 1f, float pan = 0f, bool isLooped = false);

	int GetPlayingSoudsCount();

	void Pause();

	void Resume();

	void PauseAllSounds();

	void ResumeAllSounds();

	void StopAllSounds();

	void PauseMusic();

	void ResumeMusic();

	void StopAll();

	void SetAllSoundData(List<SoundData> list);

	void DrawDebugInfo(SpriteBatch spriteBatch, BitmapFont font);

	void Update(float elapsedTime);

	float GetMusicPositionInSec();

	void SetMusicPositionInSec(float sec);

	void PlayMusicFromPositionInSec(bool isLooped, float sec);

	void Destroy();
}
