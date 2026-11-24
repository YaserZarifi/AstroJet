using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Encoding = System.Text.Encoding;
using Android.Media;
using Microsoft.Xna.Framework.Graphics;
using Rake2D;
using Rake2D.Sound;

namespace JnG2.Mobile
{
    internal sealed class SoundDataAndroid : SoundData
    {
        internal string FileName;
        internal int SampleId;
        internal float Duration;
        public SoundDataAndroid(string id) : base(id) { }
        public override void LoadFromIniFile(IniFile ini, GameContent content)
        {
            FileName = ini.Value;
            while (ini.NextLine())
            {
                if (DataLoader.CheckParams(ini, content)) continue;
                if (ini.ValueType == IniFileValueType.MainSection) break;
                if (ini.KeyEquals("volume")) DefaultVolume = Utils.Clamp(ini.GetIntValue(), 0, 100) / 100f;
                else if (ini.KeyEquals("max_sim")) MaxSimultaneous = Utils.Clamp(ini.GetIntValue(), 1, 99);
            }
        }
        public override void LoadSample()
        {
            try
            {
                string cache = Path.Combine(DataUtils.GetAppDataDirectory(), "sound-cache");
                Directory.CreateDirectory(cache);
                string key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(FileName)));
                string path = Path.Combine(cache, key + Path.GetExtension(FileName));
                if (!File.Exists(path))
                {
                    using var input = GameContent.GetFileStream(FileName);
                    if (input == null) return;
                    using var output = File.Create(path);
                    input.CopyTo(output);
                }
                using var metadata = new MediaMetadataRetriever();
                metadata.SetDataSource(path);
                if (!float.TryParse(metadata.ExtractMetadata(MetadataKey.Duration), out float milliseconds))
                    throw new InvalidDataException("Sound duration is missing: " + FileName);
                Duration = milliseconds / 1000f;
                SampleId = SoundSystemAndroid.Instance.Pool.Load(path, 1);
            }
            catch (Exception ex) { GameCore.LogErr("Android sound loading failed: {0}: {1}", FileName, ex); }
        }
        public override void FreeSample()
        {
            if (SampleId != 0) SoundSystemAndroid.Instance.Pool.Unload(SampleId);
            SampleId = 0;
        }
    }

    public sealed class SoundSystemAndroid : ISoundSystem
    {
        internal static SoundSystemAndroid Instance;
        internal SoundPool Pool;
        private readonly List<Voice> voices = new List<Voice>();
        private readonly HashSet<int> loaded = new HashSet<int>();
        private MediaPlayer music;
        private float soundVolume = 100, musicVolume = 100, soundFactor = 1, musicFactor = 1;
        private bool soundsPaused, musicPaused;
        public string MusicFileName { get; private set; }
        public bool IsMusicPlaying => music?.IsPlaying == true;
        public float GlobalVolume { get; set; } = 100;
        public float SoundVolume { get => soundVolume; set { soundVolume = value; RefreshVoices(); } }
        public float SoundVolumeFactor { get => soundFactor; set { soundFactor = value; RefreshVoices(); } }
        public float MusicVolume { get => musicVolume; set { musicVolume = value; RefreshMusic(); } }
        public float MusicVolumeFactor { get => musicFactor; set { musicFactor = value; RefreshMusic(); } }
        public void Init(IntPtr windowHandle)
        {
            Instance = this;
            using var attributes = new AudioAttributes.Builder().SetUsage(AudioUsageKind.Game)
                .SetContentType(AudioContentType.Sonification).Build();
            Pool = new SoundPool.Builder().SetMaxStreams(48).SetAudioAttributes(attributes).Build();
            Pool.LoadComplete += OnLoadComplete;
        }
        private void OnLoadComplete(object sender, SoundPool.LoadCompleteEventArgs e)
        {
            if (e.Status == 0) lock (loaded) loaded.Add(e.SampleId);
        }
        public bool LoadMusic(string fileName)
        {
            StopMusic();
            MusicFileName = fileName;
            if (string.IsNullOrEmpty(fileName)) return false;
            try
            {
                music = new MediaPlayer();
                music.SetDataSource(Path.GetFullPath(fileName));
                music.Prepare();
                RefreshMusic();
                return true;
            }
            catch (Exception ex) { GameCore.LogErr("Android music loading failed: {0}", ex); StopMusic(); return false; }
        }
        public void PlayMusic(bool isLooped) { if (music == null) return; music.Looping = isLooped; music.SeekTo(0); music.Start(); musicPaused = false; }
        public void StopMusic() { music?.Release(); music?.Dispose(); music = null; musicPaused = false; }
        public void PauseMusic() { if (music?.IsPlaying == true) { music.Pause(); musicPaused = true; } }
        public void ResumeMusic() { if (music != null && musicPaused) { music.Start(); musicPaused = false; } }
        public float GetMusicPositionInSec() => (music?.CurrentPosition ?? 0) / 1000f;
        public void SetMusicPositionInSec(float seconds) => music?.SeekTo((int)(Math.Max(0, seconds) * 1000));
        public void PlayMusicFromPositionInSec(bool looped, float seconds) { PlayMusic(looped); SetMusicPositionInSec(seconds); }
        private void RefreshMusic()
        {
            float volume = Math.Clamp(musicVolume / 100f * musicFactor * GlobalVolume / 100f, 0, 1);
            music?.SetVolume(volume, volume);
        }
        private void RefreshVoices() { foreach (var voice in voices) voice.RefreshVolume(); }
        public ISound2D PlaySound(SoundData data, float volume = 1, float pan = 0, bool isLooped = false)
        {
            if (soundsPaused || !GameCore.IsApplicationActive || data is not SoundDataAndroid sound || sound.SampleId == 0) return null;
            lock (loaded) if (!loaded.Contains(sound.SampleId)) return null;
            var sameSample = voices.Where(v => !v.IsFinished && v.Data == sound).ToList();
            if (sameSample.Count >= sound.MaxSimultaneous) sameSample[0].Stop();
            int stream = Pool.Play(sound.SampleId, 0, 0, 1, isLooped ? -1 : 0, 1);
            if (stream == 0) return null;
            var result = new Voice(this, sound, stream, volume, pan, isLooped);
            voices.Add(result);
            return result;
        }
        public int GetPlayingSoudsCount() => voices.Count(v => !v.IsFinished);
        public void Update(float elapsedTime)
        {
            foreach (var voice in voices)
            {
                voice.PlaybackSpeed = GameCore.IsShopMenuEnabled || GameCore.IsInLevelMenuEnabled ? 1 : Math.Clamp(GameCore.TimeFactor, .5f, 2);
                if (!soundsPaused) voice.Advance(elapsedTime);
            }
            voices.RemoveAll(v => v.IsFinished);
        }
        public void PauseAllSounds() { Pool?.AutoPause(); soundsPaused = true; }
        public void ResumeAllSounds() { Pool?.AutoResume(); soundsPaused = false; }
        public void StopAllSounds() { foreach (var voice in voices) voice.Stop(); voices.Clear(); }
        public void Pause() { PauseAllSounds(); PauseMusic(); }
        public void Resume() { ResumeAllSounds(); ResumeMusic(); }
        public void StopAll() { StopAllSounds(); StopMusic(); }
        public void Clear() => StopAllSounds();
        public void SetAllSoundData(List<SoundData> list) { }
        public void DrawDebugInfo(SpriteBatch spriteBatch, BitmapFont font) { }
        public void Destroy() { StopAll(); if (Pool != null) { Pool.LoadComplete -= OnLoadComplete; Pool.Release(); Pool.Dispose(); Pool = null; } }

        private sealed class Voice : ISound2D
        {
            private readonly SoundSystemAndroid owner;
            internal readonly SoundDataAndroid Data;
            private readonly int stream;
            private float volume, pan, elapsed, speed = 1;
            private bool looped, stopped;
            public float StartVolume { get; set; }
            public bool IsFinished => stopped || !looped && elapsed >= Data.Duration;
            public float PlaybackSpeed { get => speed; set { speed = Math.Clamp(value, .5f, 2); owner.Pool.SetRate(stream, speed); } }
            public bool IsLooped { get => looped; set { looped = value; owner.Pool.SetLoop(stream, value ? -1 : 0); } }
            internal Voice(SoundSystemAndroid owner, SoundDataAndroid data, int stream, float volume, float pan, bool looped)
            {
                this.owner = owner; Data = data; this.stream = stream; StartVolume = this.volume = volume;
                this.pan = pan; this.looped = looped; RefreshVolume();
            }
            internal void RefreshVolume()
            {
                float gain = Math.Clamp(Data.DefaultVolume * volume / 100f * owner.SoundVolume / 100f * owner.SoundVolumeFactor * owner.GlobalVolume / 100f, 0, 1);
                owner.Pool.SetVolume(stream, gain * (1 - Math.Max(0, pan)), gain * (1 + Math.Min(0, pan)));
            }
            internal void Advance(float seconds) { elapsed += Math.Max(0, seconds) * speed; }
            public void SetVolume(float value) { volume = value; RefreshVolume(); }
            public void SetPan(float value) { pan = Math.Clamp(value, -1, 1); RefreshVolume(); }
            public void Stop() { if (!stopped) owner.Pool.Stop(stream); stopped = true; }
        }
    }
}
