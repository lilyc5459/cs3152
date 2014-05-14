using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;

namespace Pathogenesis.Controllers
{
    public class Fader
    {
        public int fadeTime = 50;

        private int in_time;
        private int out_time;

        public int fadeCounter;
        private bool fadeIn;

        private Action<GameState> callback;
        private GameState arg;

        private bool noargs;
        private Action callback_noargs;

        public bool Fading
        {
            get { return fadeCounter > 0; }
        }

        public Fader()
        {
            fadeCounter = 0;
            fadeIn = false;
        }

        public void startFade(Action<GameState> callback, GameState arg, int in_time, int out_time)
        {
            noargs = false;
            this.in_time = in_time;
            this.out_time = out_time;
            this.fadeTime = out_time;
            this.callback = callback;
            this.arg = arg;
            fadeCounter++;
        }

        public void startFade(Action callback, int in_time, int out_time)
        {
            noargs = true;
            this.in_time = in_time;
            this.out_time = out_time;
            this.fadeTime = out_time;
            this.callback_noargs = callback;
            fadeCounter++;
        }

        public void Update()
        {   
            if (fadeCounter > 0)
            {
                SoundEffect.MasterVolume = 1 - (float)fadeCounter / fadeTime;
                if (!fadeIn)
                {
                    fadeCounter++;
                    if (fadeCounter >= fadeTime)
                    {
                        if (noargs)
                        {
                            callback_noargs();
                            callback_noargs = null;
                        }
                        else
                        {
                            callback(arg);
                            callback = null;
                        }
                        
                        fadeIn = true;
                        fadeTime = in_time;
                        fadeCounter = in_time;
                    }
                }
                else
                {
                    fadeCounter--;
                    if (fadeCounter <= 0)
                    {
                        fadeCounter = 0;
                        fadeIn = false;
                    }
                }
            }
        }
    }
}
