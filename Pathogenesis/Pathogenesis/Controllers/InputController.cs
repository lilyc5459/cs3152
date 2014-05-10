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
        DOUBLE,
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
            private Keys PAUSE = Keys.P;
            private Keys TOGGLE_HUD = Keys.Tab;
            private Keys ENTER = Keys.Enter;
            private Keys RESTART = Keys.R;
            private Keys SPAWN_ENEMY = Keys.I;
            private Keys SPAWN_ALLY = Keys.O;
            private Keys SPAWN_PLASMID = Keys.K;
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
            /// Whether the infection button is pressed.
            /// </summary>
            public bool StartConverting
            {
                get { return keyStates[CONVERT] == KeyState.DOWN; }
            }

            /// <summary>
            /// Whether the infection button is held.
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
            /// Whether the back button was pressed.
            /// </summary>
            public bool Back
            {
                get { return keyStates[Keys.Escape] == KeyState.DOWN ||
                    keyStates[Keys.Back] == KeyState.DOWN; }
            }

            /// <summary>
            /// Whether the enter button was pressed.
            /// </summary>
            public bool Enter
            {
                get { return keyStates[ENTER] == KeyState.DOWN; }
            }

            /// <summary>
            /// Whether the enter button was pressed.
            /// </summary>
            public bool Escape
            {
                get { return keyStates[Keys.Escape] == KeyState.DOWN; }
            }

            /// <summary>
            /// Whether the left button was pressed.
            /// </summary>
            public bool Left
            {
                get { return keyStates[LEFT] == KeyState.HELD || keyStates[Keys.Left] == KeyState.HELD; }
            }

            /// <summary>
            /// Whether the left button was pressed once.
            /// </summary>
            public bool LeftOnce
            {
                get { return keyStates[LEFT] == KeyState.DOWN || keyStates[Keys.Left] == KeyState.DOWN; }
            }

            /// <summary>
            /// Whether the right button was pressed.
            /// </summary>
            public bool Right
            {
                get { return keyStates[RIGHT] == KeyState.HELD || keyStates[Keys.Right] == KeyState.HELD; }
            }

            /// <summary>
            /// Whether the right button was pressed once.
            /// </summary>
            public bool RightOnce
            {
                get { return keyStates[RIGHT] == KeyState.DOWN || keyStates[Keys.Right] == KeyState.DOWN; }
            }

            /// <summary>
            /// Whether the up button was pressed.
            /// </summary>
            public bool Up
            {
                get { return keyStates[UP] == KeyState.HELD || keyStates[Keys.Up] == KeyState.HELD; }
            }

            /// Whether the up button was pressed once.
            /// </summary>
            public bool UpOnce
            {
                get { return keyStates[UP] == KeyState.DOWN || keyStates[Keys.Up] == KeyState.DOWN; }
            }

            /// <summary>
            /// Whether the down button was pressed.
            /// </summary>
            public bool Down
            {
                get { return keyStates[DOWN] == KeyState.HELD || keyStates[Keys.Down] == KeyState.HELD; }
            }

            /// <summary>
            /// Whether the down button was pressed.
            /// </summary>
            public bool DownOnce
            {
                get { return keyStates[DOWN] == KeyState.DOWN || keyStates[Keys.Down] == KeyState.DOWN; }
            }

            /// <summary>
            /// Whether the toggle HUD button was pressed.
            /// </summary>
            public bool Toggle_HUD
            {
                get { return keyStates[TOGGLE_HUD] == KeyState.DOWN; }
            }

            /// <summary>
            /// Whether the Restart button was pressed.
            /// </summary>
            public bool Restart
            {
                get { return keyStates[RESTART] == KeyState.DOWN; }
            }

            /// <summary>
            /// Whether the Spawn Enemy button was pressed.
            /// </summary>
            public bool Spawn_Enemy
            {
                get { return keyStates[SPAWN_ENEMY] == KeyState.HELD; }
            }

            /// <summary>
            /// Whether the Spawn Ally button was pressed.
            /// </summary>
            public bool Spawn_Ally
            {
                get { return keyStates[SPAWN_ALLY] == KeyState.HELD; }
            }

            /// <summary>
            /// Whether the Spawn Plasmid button was pressed.
            /// </summary>
            public bool Spawn_Plasmid
            {
                get { return keyStates[SPAWN_PLASMID] == KeyState.DOWN; }
            }

        #endregion

        #region Methods
            /// <summary>
            /// Creates a new input controller.
            /// </summary>
            public InputController()
            {
                keyStates = new Dictionary<Keys, KeyState>();
                keyStates.Add(Keys.Escape, KeyState.UP);
                keyStates.Add(Keys.Back, KeyState.UP);
                keyStates.Add(Keys.Up, KeyState.UP);
                keyStates.Add(Keys.Down, KeyState.UP);
                keyStates.Add(Keys.Left, KeyState.UP);
                keyStates.Add(Keys.Right, KeyState.UP);
                keyStates.Add(UP, KeyState.UP);
                keyStates.Add(DOWN, KeyState.UP);
                keyStates.Add(LEFT, KeyState.UP);
                keyStates.Add(RIGHT, KeyState.UP);

                keyStates.Add(CONVERT, KeyState.UP);
                keyStates.Add(RALLY, KeyState.UP);
                keyStates.Add(PAUSE, KeyState.UP);
                keyStates.Add(TOGGLE_HUD, KeyState.UP);
                keyStates.Add(ENTER, KeyState.UP);
                keyStates.Add(RESTART, KeyState.UP);
                keyStates.Add(SPAWN_ENEMY, KeyState.UP);
                keyStates.Add(SPAWN_ALLY, KeyState.UP);
                keyStates.Add(SPAWN_PLASMID, KeyState.UP);
            }

            /// <summary>
            /// Read Keyboard Input
            /// </summary>
            public void Update(bool disabled)
            {
                List<Keys> keys = new List<Keys>(keyStates.Keys);

                if (disabled)
                {
                    foreach(Keys k in keys)
                    {
                        keyStates[k] = KeyState.UP;
                    }
                }
                else
                {
                    KeyboardState keyboard = Keyboard.GetState();
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
            }
        #endregion
    }
}
