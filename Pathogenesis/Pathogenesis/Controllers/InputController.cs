using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Pathogenesis
{
    public class InputController
    {
        #region Fields
            private Vector2 offset;		    // How much did we move (x,y)   [Game]
            private bool convertPressed;	// Convert pressed              [Game]
            private bool rallyPressed;		// Rally pressed                [Game]
            private bool pausePressed;      // Pause pressed                [Game/Menu]

            private bool menuLeftPressed;   // Left is pressed              [Menu]
            private bool menuRightPressed;  // Right is pressed             [Menu]     
            private bool menuUpPressed;     // Up is pressed                [Menu] 
            private bool menuDownPressed;   // Down is pressed              [Menu]
            private bool menuEnterPressed;  // Enter is pressed             [Menu]
        #endregion

        #region Properties (READ-ONLY)
            /// <summary>
            /// The amount of sideways movement. (x,y) ---> -1 = left, 1 = right, 0 = still, -1 = up, 1= = down, 0 =still
            /// </summary>
            public Vector2 Movement
            {
                get { return offset; }
            }

            /// <summary>
            /// Whether the rally button is pressed.
            /// </summary>
            public bool Rallying
            {
                get { return rallyPressed; }
            }

            /// <summary>
            /// Whether the rally button is pressed.
            /// </summary>
            public bool Converting
            {
                get { return convertPressed; }
            }

            /// <summary>
            /// Whether the pause button was pressed.
            /// </summary>
            public bool Pause
            {
                get { return pausePressed; }
            }

            /// <summary>
            /// Whether the enter button was pressed.
            /// </summary>
            public bool Enter
            {
                get { return menuEnterPressed; }
            }

            /// <summary>
            /// Whether the left button was pressed.
            /// </summary>
            public bool MenuLeft
            {
                get { return menuLeftPressed; }
            }

            /// <summary>
            /// Whether the right button was pressed.
            /// </summary>
            public bool MenuRight
            {
                get { return menuRightPressed; }
            }

            /// <summary>
            /// Whether the up button was pressed.
            /// </summary>
            public bool MenuUp
            {
                get { return menuUpPressed; }
            }

            /// <summary>
            /// Whether the down button was pressed.
            /// </summary>
            public bool MenuDown
            {
                get { return menuDownPressed; }
            }
        #endregion

        #region Methods
            /// <summary>
            /// Creates a new input controller.
            /// </summary>
            public InputController() { }

            /// <summary>
            /// Read Keyboard Input
            /// </summary>
            private void ReadKeyboardInputGame()
            {
                KeyboardState keyboard = Keyboard.GetState();

                convertPressed = keyboard.IsKeyDown(Keys.Space);
                rallyPressed = keyboard.IsKeyDown(Keys.E);
                pausePressed = keyboard.IsKeyDown(Keys.Escape);
                menuEnterPressed = keyboard.IsKeyDown(Keys.Enter);

                offset = new Vector2(0,0);
                if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
                {
                    offset.X += 1.0f;
                    menuRightPressed = true;
                }
                if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
                {
                    offset.X -= 1.0f;
                    menuLeftPressed = true;
                }
                if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up))
                {
                    offset.Y -= 1.0f;
                    menuUpPressed = true;
                }
                if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
                {
                    offset.Y += 1.0f;
                    menuDownPressed = true;
                }
            }
        #endregion


        public void Update(GameState game_state)
        {
            // Check for input
            switch (game_state)
            {
                case GameState.MAIN_MENU:
                    // handle menu inputs here
                    break;
                case GameState.IN_GAME:
                    break;
                case GameState.PAUSED:
                    // handle pause menu inputs here
                    break;
            }
        }
    }
}
