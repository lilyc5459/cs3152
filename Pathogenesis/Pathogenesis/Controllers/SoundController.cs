using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Pathogenesis.Models;

namespace Pathogenesis.Controllers
{
    public enum SoundType
    {
        MUSIC,      // Game music   
        EFFECT      // Sound effect
    }

    public class SoundController
    {
        private Dictionary<String, Sound> music;
        private Dictionary<String, Sound> effects;

        public SoundController(ContentFactory factory)
        {
            music = new Dictionary<string, Sound>();
            effects = new Dictionary<string, Sound>();

            Dictionary<String, SoundEffect> loaded_music = factory.getMusic();
            foreach (String key in loaded_music.Keys)
            {
                music.Add(key.Substring(key.LastIndexOf("/") + 1), new Sound(loaded_music[key].CreateInstance()));
            }

            Dictionary<String, SoundEffect> loaded_effects = factory.getSoundEffects();
            foreach (String key in loaded_effects.Keys)
            {
                effects.Add(key.Substring(key.LastIndexOf("/") + 1), new Sound(loaded_effects[key].CreateInstance()));
            }
        }

        public void loop(SoundType type, String name)
        {
            Dictionary<String, Sound> sounds = filter(type);
            if (!sounds[name].IsLooped)
            {
                sounds[name].IsLooped = true;
            }
            sounds[name].Restart();
        }

        public void play(SoundType type, String name)
        {
            filter(type)[name].Restart();
        }

        public void pause(SoundType type, String name)
        {
            filter(type)[name].Pause();
        }

        public void stop(SoundType type, String name)
        {
            filter(type)[name].Stop();
        }

        private Dictionary<string, Sound> filter(SoundType type)
        {
            switch (type)
            {
                case SoundType.MUSIC:
                    return music;
                case SoundType.EFFECT:
                    return effects;
            }
            return null;
        }

        /*
         * Pause all clips
         */
        public void pauseAll()
        {
            foreach (Sound sound in music.Values)
            {
                sound.Pause();
            }
            foreach (Sound sound in effects.Values)
            {
                sound.Pause();
            }
        }

        /*
         * Mute all music
         */
        public void MuteSounds(SoundType type)
        {
            Dictionary<String, Sound> sounds = filter(type);
            foreach (Sound sound in sounds.Values)
            {
                sound.Mute();
            }
        }

        /*
         * Unmute all music
         */
        public void UnmuteSounds(SoundType type)
        {
            Dictionary<String, Sound> sounds = filter(type);
            foreach (Sound sound in sounds.Values)
            {
                sound.Unmute();
            }
        }

        public void Update()
        {
            foreach (Sound sound in music.Values)
            {
                sound.Update();
            }
            foreach (Sound sound in effects.Values)
            {
                sound.Update();
            }
        }
    }
}
