using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathogenesis.Controllers
{
    public class Fader
    {
        public const int fadeSpeed = 5;
        public const int fadeTime = 250;

        public int fadeCounter;
        private bool fadeIn;

        private Action<GameState> callback;
        private GameState arg;

        public Fader()
        {
            fadeCounter = 0;
            fadeIn = false;
        }

        public void startFade(Action<GameState> callback, GameState arg)
        {
            fadeCounter += fadeSpeed;
            this.callback = callback;
            this.arg = arg;
        }

        public void Update()
        {
            if (fadeCounter > 0)
            {
                if (!fadeIn)
                {
                    fadeCounter += fadeSpeed;
                    if (fadeCounter >= fadeTime && callback != null)
                    {
                        callback(arg);
                        callback = null;
                        fadeIn = true;
                    }
                }
                else
                {
                    fadeCounter -= fadeSpeed;
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
