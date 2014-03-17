using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Pathogenesis
{
    public enum KeyState
    {
        DOWN,
        HELD,
        UP,
    }

    public class InputController
    {
        #region Fields
            private Dictionary<Keys, KeyState> keyStates;

            private Keys CONVERT = Keys.Space;
            private Keys LEFT = Keys.A;
            private Keys RIGHT = Keys.D;
            private Keys UP = Keys.W;
            private Keys DOWN = Keys.S;
            private Keys RALLY = Keys.E;
            private Keys PAUSE = Keys.Escape;
            private Keys TOGGLE_HUD = Keys.Tab;
            private Keys ENTER = Keys.Enter;

            private bool convertPressed;	// Convert pressed              [Game]
            private bool convertHeld;

            private bool rallyPressed;		// Rally pressed                [Game]
            private bool pausePressed;      // Pause pressed                [Menu/Game]
            private bool leftPressed;   // Left is pressed                  [Menu/Game]
            private bool rightPressed;  // Right is pressed                 [Menu/Game]     
            private bool upPressed;     // Up is pressed                    [Menu/Game]
            private bool downPressed;   // Down is pressed                  [Menu/Game]
            private bool enterPressed;  // Enter is pressed                 [Menu/Game]

            private bool toggleHUD;     // HUD is toggled
        #endregion

        #region Properties (READ-ONLY)
            /// <summary>
            /// Whether the rally button is pressed.
            /// </summary>
            public bool Rallying
            {
                get { return keyStates[RALLY] == KeyState.HELD; }
            }

            /// <summary>
            /// Whether the rally button is pressed.
            /// </summary>
            public bool Converting
            {
                get { return keyStates[CONVERT] == KeyState.HELD; }
            }

            /// <summary>
            /// Whether the pause button was pressed.
            /// </summary>
            public bool Pause
            {
                get { return keyStates[PAUSE] == KeyState.DOWN; }
            }

            /// <summary>
            /// Whether the enter button was pressed.
            /// </summary>
            public bool Enter
            {
                get { return keyStates[ENTER] == KeyState.DOWN; }
            }

            /// <summary>
            /// Whether the left button was pressed.
            /// </summary>
            public bool Left
            {
                get { return keyStates[LEFT] == KeyState.HELD; }
            }

            /// <summary>
            /// Whether the right button was pressed.
            /// </summary>
            public bool Right
            {
                get { return keyStates[RIGHT] == KeyState.HELD; }
            }

            /// <summary>
            /// Whether the up button was pressed.
            /// </summary>
            public bool Up
            {
                get { return keyStates[UP] == KeyState.HELD; }
            }

            /// <summary>
            /// Whether the down button was pressed.
            /// </summary>
            public bool Down
            {
                get { return keyStates[DOWN] == KeyState.HELD; }
            }

            /// <summary>
            /// Whether the toggle HUDE button was pressed.
            /// </summary>
            public bool Toggle_HUD
            {
                get { return keyStates[TOGGLE_HUD] == KeyState.DOWN; }
            }


        #endregion

        #region Methods
            /// <summary>
            /// Creates a new input controller.
            /// </summary>
            public InputController()
            {
                keyStates = new Dictionary<Keys, KeyState>();
                keyStates.Add(CONVERT, KeyState.UP);
                keyStates.Add(Keys.Up, KeyState.UP);
                keyStates.Add(Keys.Down, KeyState.UP);
                keyStates.Add(Keys.Left, KeyState.UP);
                keyStates.Add(Keys.Right, KeyState.UP);
                keyStates.Add(UP, KeyState.UP);
                keyStates.Add(DOWN, KeyState.UP);
                keyStates.Add(LEFT, KeyState.UP);
                keyStates.Add(RIGHT, KeyState.UP);
                keyStates.Add(RALLY, KeyState.UP);
                keyStates.Add(PAUSE, KeyState.UP);
                keyStates.Add(TOGGLE_HUD, KeyState.UP);
            }

            /// <summary>
            /// Read Keyboard Input
            /// </summary>
            public void Update()
            {
                KeyboardState keyboard = Keyboard.GetState();
                List<Keys> keys = new List<Keys>(keyStates.Keys);
                foreach (Keys k in keys)
                {
                    if (keyboard.IsKeyDown(k))
                    {
                        if (keyStates[k] == KeyState.UP) keyStates[k] = KeyState.DOWN;
                        else keyStates[k] = KeyState.HELD;
                    }
                    else
                    {
                        keyStates[k] = KeyState.UP;
                    }
                }
            }
        #endregion

    }
}
