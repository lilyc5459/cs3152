using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace Pathogenesis.Models
{
    public class Sound
    {
        public String Name { get; set; }
        SoundEffectInstance instance;
        bool restart;

        public bool isPlaying {
            get { return instance.State == SoundState.Playing; }
            set { isPlaying = value; } }

        public Sound(SoundEffectInstance instance, String name)
        {
            this.instance = instance;
            this.Name = name;
            restart = false;
        }

        public void Update()
        {
            if (restart)
            {
                instance.Play();
                restart = false;
            }
        }

        public void Play()
        {
            instance.Play();
        }

        public void Restart()
        {
            instance.Stop();
            restart = true;
        }

        public void Stop()
        {
            instance.Stop();
            restart = false;
        }

        public void Pause()
        {
            instance.Pause();
        }

        public void Mute()
        {
            instance.Volume = 0;
        }

        public void Unmute()
        {
            instance.Volume = 1.0f;
        }

        public SoundState State
        {
            get { return instance.State; }
        }

        public float Volume
        {
            set { instance.Volume = value; }
            get { return instance.Volume; }
        }

        public bool IsLooped
        {
            set { instance.IsLooped = value; }
            get { return instance.IsLooped; }
        }
    }
}
