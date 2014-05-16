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
        private Dictionary<String, SoundEffect> effects;
        private List<Sound> effect_instances;
        private List<Sound> finished_effects;

        public SoundController(ContentFactory factory)
        {
            music = new Dictionary<string, Sound>();
            effects = new Dictionary<string, SoundEffect>();
            effect_instances = new List<Sound>();
            finished_effects = new List<Sound>();

            Dictionary<String, SoundEffect> loaded_music = factory.getMusic();
            foreach (String key in loaded_music.Keys)
            {
                music.Add(key.Substring(key.LastIndexOf("/") + 1), new Sound(loaded_music[key].CreateInstance(), key));
            }

            Dictionary<String, SoundEffect> loaded_effects = factory.getSoundEffects();
            foreach (String key in loaded_effects.Keys)
            {
                effects.Add(key.Substring(key.LastIndexOf("/") + 1), loaded_effects[key]);
            }
        }

        public void loop(SoundType type, String name)
        {
            Sound sound = null;
            if (type == SoundType.MUSIC && music.ContainsKey(name))
            {
                sound = music[name];
            }
            else if (type == SoundType.EFFECT && effects.ContainsKey(name))
            {
                sound = new Sound(effects[name].CreateInstance(), name);
                effect_instances.Add(sound);
            }

            if (sound != null)
            {
                if (!sound.IsLooped)
                {
                    sound.IsLooped = true;
                }
                sound.Restart();
            }
        }

        public void play(SoundType type, String name)
        {
            if (type == SoundType.EFFECT)
            {
                Sound s = new Sound(effects[name].CreateInstance(), name);
                effect_instances.Add(s);
                s.Restart();
            }
            else if(type == SoundType.MUSIC)
            {
                music[name].Restart();
            }
        }

        public void pause(SoundType type, String name)
        {
            if (type == SoundType.MUSIC && music.ContainsKey(name))
            {
                music[name].Pause();
            }
            else if (type == SoundType.EFFECT)
            {
                foreach (Sound sound in effect_instances)
                {
                    if (sound.Name.Equals(name))
                    {
                        sound.Pause();
                    }
                }
            }
        }

        public void stop(SoundType type, String name)
        {
            if (type == SoundType.MUSIC && music.ContainsKey(name))
            {
                music[name].Stop();
            }
            else if (type == SoundType.EFFECT)
            {
                foreach (Sound sound in effect_instances)
                {
                    if (sound.Name.Equals(name))
                    {
                        sound.Stop();
                    }
                }
            }
        }

        /*
         * Stop all clips
         */
        public void stopAll()
        {
            foreach (Sound sound in music.Values)
            {
                sound.Stop();
            }
            foreach (Sound sound in effect_instances)
            {
                sound.Stop();
            }
        }

        /*
         * Stop all music clips
         */
        public void stopMusic()
        {
            foreach (Sound sound in music.Values)
            {
                sound.Stop();
            }
        }

        /*
         * Mute all music
         */
        public void MuteSounds(SoundType type)
        {
            foreach (Sound sound in music.Values)
            {
                sound.Mute();
            }
            foreach (Sound sound in effect_instances)
            {
                sound.Mute();
            }
        }

        /*
         * Unmute all music
         */
        public void UnmuteSounds(SoundType type)
        {
            foreach (Sound sound in music.Values)
            {
                sound.Unmute();
            }
            foreach (Sound sound in effect_instances)
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
            foreach (Sound sound in effect_instances)
            {
                sound.Update();
                if (!sound.isPlaying)
                {
                    finished_effects.Add(sound);
                }
            }

            foreach (Sound sound in finished_effects)
            {
                effect_instances.Remove(sound);
            }
            finished_effects.Clear();
        }
    }
}
