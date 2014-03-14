using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathogenesis
{
    public class InputController
    {

        public void Update(GameState game_state)
        {
            // Check for input
            switch (game_state)
            {
                case GameState.MAIN_MENU:
                    // handle menu inputs here
                    break;
                case GameState.IN_GAME:
                    // handle in game inputs here
                    break;
                case GameState.PAUSED:
                    // handle pause menu inputs here
                    break;
            }
        }
    }
}
