using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Rienet
{
    public class AudioComponent
    {
        private SoundEffect soundEffect;
        public SoundEffectInstance Sound { get; private set; }
        public bool Looping { get; set; }
        public Action ActionOnEnd { get; set; } = delegate { };
        public Action ActionOnStop { get; set; } = delegate { };

        public AudioComponent(string file, ContentManager Content, bool Looping)
        {
            var SFX = Content.Load<SoundEffect>(file);
            soundEffect = SFX;
            Sound = SFX.CreateInstance();
            CurrentMaxVolume = Sound.Volume;
            this.Looping = Looping;
            progress = TimeSpan.Zero;
        }

        public void OnPlay()
        {
            progress = TimeSpan.Zero;
            Sound.Dispose();
            Sound = soundEffect.CreateInstance();
            Sound.Play();
            if (!AudioHandler.AudiosInPlay.Contains(this))
                AudioHandler.AudiosInPlay.Add(this);
        }

        TimeSpan progress = TimeSpan.Zero;

        public void Update(GameTime gameTime)
        {
            progress += gameTime.ElapsedGameTime;

            if (FadingIn)
                FadeIn(Increase);
            else if (FadingOut)
                FadeOut(Increase);

            if (progress.TotalMilliseconds >= soundEffect.Duration.TotalMilliseconds)
                OnEnd();
        }

        public void OnEnd()
        {
            progress = TimeSpan.Zero;

            if (Looping)
            {
                Reset();
                return;
            }
            ActionOnEnd();
            Sound.Stop();
        }

        public void Stop()
        {
            progress = TimeSpan.Zero;
            Sound.Stop();
            if (AudioHandler.AudiosInPlay.Contains(this))
                AudioHandler.AudiosInPlay.Remove(this);
            ActionOnStop();
        }

        public void Pause()
        {
            if (AudioHandler.AudiosInPlay.Contains(this))
                AudioHandler.AudiosInPlay.Remove(this);
            Sound.Pause();
        }

        public void Resume()
        {
            if (!AudioHandler.AudiosInPlay.Contains(this))
                AudioHandler.AudiosInPlay.Add(this);
            Sound.Resume();
        }

        public void Reset()
        {
            OnPlay();
        }

        float CurrentMaxVolume = AudioHandler.MasterVolume;

        public void SetVolume(float Volume)
        {
            Sound.Volume = Volume;
            CurrentMaxVolume = Volume;
        }

        bool FadingIn;
        bool FadingOut;
        float Increase;

        /// <summary>
        /// DO NOT SET TIME TO ZERO
        /// </summary>
        public void SetFadeIn(float Time)
        {
            Volume = 0f;
            FadingIn = true;
            Increase = CurrentMaxVolume / Time;
        }

        /// <summary>
        /// DO NOT SET TIME TO ZERO
        /// </summary>
        public void SetFadeOut(float Time)
        {
            Volume = CurrentMaxVolume;
            FadingOut = true;
            Increase = -CurrentMaxVolume / Time;
        }

        void FadeIn(float Increase)
        {
            if (Volume < CurrentMaxVolume)
            {
                float f = Volume + Increase;
                if (f <= CurrentMaxVolume)
                    Volume = f;
                else
                {
                    Volume = CurrentMaxVolume;
                    FadingIn = false;
                    this.Increase = 0;
                }
            }
        }

        void FadeOut(float Increase)
        {
            if (Volume > 0)
            {
                float f = Volume + Increase;
                if (f >= 0)
                    Volume = f;
                else
                {
                    Volume = 0f;
                    FadingOut = false;
                    this.Increase = 0;
                    Stop();
                }
            }
        }

        public float Volume
        {
            get { return Sound.Volume; }
            set { Sound.Volume = value; }
        }

        public float Pitch
        {
            get { return Sound.Pitch; }
            set { Sound.Pitch = value; }
        }

        public float Pan
        {
            get { return Sound.Pan; }
            set { Sound.Pan = value; }
        }

        public SoundState State
        {
            get { return Sound.State; }
        }
    }
}