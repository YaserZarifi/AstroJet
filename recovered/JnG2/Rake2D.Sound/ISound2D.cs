namespace Rake2D.Sound;

public interface ISound2D
{
	float StartVolume { get; set; }

	float PlaybackSpeed { get; set; }

	bool IsLooped { get; set; }

	bool IsFinished { get; }

	void Stop();

	void SetVolume(float vol);

	void SetPan(float pan);
}
