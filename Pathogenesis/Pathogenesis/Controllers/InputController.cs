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
            private bool convertPressed;	// Convert pressed              [Game]
            private bool rallyPressed;		// Rally pressed                [Game]
            private bool pausePressed;      // Pause pressed                [Menu/Game]
            private bool leftPressed;   // Left is pressed                  [Menu/Game]
            private bool rightPressed;  // Right is pressed                 [Menu/Game]     
            private bool upPressed;     // Up is pressed                    [Menu/Game]
            private bool downPressed;   // Down is pressed                  [Menu/Game]
            private bool enterPressed;  // Enter is pressed                 [Menu/Game]
        #endregion

        #region Properties (READ-ONLY)
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
                get { return enterPressed; }
            }

            /// <summary>
            /// Whether the left button was pressed.
            /// </summary>
            public bool Left
            {
                get { return leftPressed; }
            }

            /// <summary>
            /// Whether the right button was pressed.
            /// </summary>
            public bool Right
            {
                get { return rightPressed; }
            }

            /// <summary>
            /// Whether the up button was pressed.
            /// </summary>
            public bool Up
            {
                get { return upPressed; }
            }

            /// <summary>
            /// Whether the down button was pressed.
            /// </summary>
            public bool Down
            {
                get { return downPressed; }
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
                enterPressed = keyboard.IsKeyDown(Keys.Enter);

                if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
                {
                    rightPressed = true;
                }
                if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
                {
                    leftPressed = true;
                }
                if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up))
                {
                    upPressed = true;
                }
                if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
                {
                    downPressed = true;
                }
            }
        #endregion

    }
}
