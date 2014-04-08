using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Pathogenesis.Models;

namespace Pathogenesis.Controllers
{
    public class SoundController
    {
        private Dictionary<String, Sound> sounds;

        public SoundController(ContentFactory factory)
        {
            sounds = new Dictionary<string, Sound>();

            Dictionary<String, SoundEffect> loaded_sounds = factory.getSounds();
            foreach (String key in loaded_sounds.Keys)
            {
                sounds.Add(key.Substring(key.LastIndexOf("/") + 1), new Sound(loaded_sounds[key].CreateInstance()));
            }
        }

        public void play(String name)
        {
            sounds[name].Restart();
        }

        public void pause(String name)
        {
            sounds[name].Pause();
        }

        public void stop(String name) {
            sounds[name].Stop();
        }

        public void Update()
        {
            foreach (Sound sound in sounds.Values)
            {
                sound.Update();
            }
        }
    }
}
