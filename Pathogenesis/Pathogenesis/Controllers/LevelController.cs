using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathogenesis.Models;
using Microsoft.Xna.Framework;
using Pathogenesis.Controllers;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

namespace Pathogenesis
{
    public class LevelController
    {
        #region Fields
        private const int TUTORIAL2_FREE_ALLIES = 6;
        private const int TUTORIAL_START_DELAY = 1500;
        private const int CREDITS_ROLL_TIME = 10000;

        public Level CurLevel { get; set; }     // Stores Current Level Object
        public int CurLevelNum { get; set; }    // Stores Current Level Number

        private GameEngine engine;
        private ContentFactory factory;
        private GameUnitController unit_controller;
        private ItemController item_controller;
        private SoundController sound_controller;
        private MenuController menu_controller;

        private Stopwatch tutorial_stopwatch;
        private Stopwatch credits_timer;

        private Texture2D WinBackground;

        public int NumLevels;

        #endregion

        public LevelController(GameEngine engine, ContentFactory factory, GameUnitController unit_controller,
            ItemController item_controller, SoundController sound_controller, MenuController menu_controller,
            Texture2D win_background)
        {
            this.engine = engine;
            this.factory = factory;
            this.unit_controller = unit_controller;
            this.item_controller = item_controller;
            this.sound_controller = sound_controller;
            this.menu_controller = menu_controller;

            tutorial_stopwatch = new Stopwatch();
            credits_timer = new Stopwatch();

            WinBackground = win_background;

            NumLevels = factory.GetNumLevels();
            CurLevelNum = -1;
        }

        /*
         * Update the currentl level
         * 
         * Returns true if the level is completed
         */
        public bool Update()
        {
            if (unit_controller.Player == null) return false;

            // Tutorial #1
            if (CurLevelNum == 0)
            {
                if (menu_controller.CurDialogue == 0)
                {
                    tutorial_stopwatch.Start();
                    if (tutorial_stopwatch.ElapsedMilliseconds >= TUTORIAL_START_DELAY)
                    {
                        //tip #1
                        engine.ChangeGameState(GameState.PAUSED);
                        menu_controller.LoadDialogue(0);
                        tutorial_stopwatch.Stop();
                        tutorial_stopwatch.Reset();
                    }
                }
                if (menu_controller.CurDialogue == 1)
                {
                    bool show = false;
                    foreach (GameUnit unit in unit_controller.Units)
                    {
                        if (unit.Type == UnitType.TANK && unit.Faction == UnitFaction.ENEMY &&
                            unit_controller.Player.inRange(unit, 300))
                        {
                            show = true;
                            break;
                        }
                    }
                    if (show)
                    {
                        //tip #2
                        engine.ChangeGameState(GameState.PAUSED);
                        menu_controller.LoadDialogue(1);
                    }
                }
                if (menu_controller.CurDialogue == 2)
                {
                    bool show = false;
                    foreach (GameUnit unit in CurLevel.Organs)
                    {
                        if (unit_controller.Player.inRange(unit, 350))
                        {
                            show = true;
                            break;
                        }
                    }
                    if (show)
                    {
                        //tip #3
                        engine.ChangeGameState(GameState.PAUSED);
                        menu_controller.LoadDialogue(2);
                    }
                }
                if (menu_controller.CurDialogue == 3)
                {
                    bool show = false;
                    foreach (GameUnit unit in CurLevel.Bosses)
                    {
                        if (unit_controller.Player.inRange(unit, 400))
                        {
                            show = true;
                            break;
                        }
                    }
                    if (show)
                    {
                        //tip #4
                        engine.ChangeGameState(GameState.PAUSED);
                        menu_controller.LoadDialogue(3);
                    }
                }
            }

            // Tutorial #2
            if (CurLevelNum == 1)
            {
                if (menu_controller.CurDialogue == 4)
                {
                    tutorial_stopwatch.Start();
                    if (tutorial_stopwatch.ElapsedMilliseconds >= TUTORIAL_START_DELAY)
                    {
                        //tip #1 - start
                        engine.ChangeGameState(GameState.PAUSED);
                        menu_controller.LoadDialogue(4);
                        tutorial_stopwatch.Stop();
                        tutorial_stopwatch.Reset();
                    }
                }
                if (menu_controller.CurDialogue == 5)
                {
                    bool show = false;
                    foreach (GameUnit unit in unit_controller.Units)
                    {
                        if (unit.Type == UnitType.TANK && unit.Faction == UnitFaction.ENEMY &&
                            unit_controller.Player.inRange(unit, 300))
                        {
                            show = true;
                            break;
                        }
                    }
                    if (show)
                    {
                        //tip #2 - big enemies
                        engine.ChangeGameState(GameState.PAUSED);
                        menu_controller.LoadDialogue(5);
                    }
                }
                if (menu_controller.CurDialogue == 6)
                {
                    bool show = false;
                    foreach (GameUnit unit in unit_controller.Units)
                    {
                        if (unit.Type == UnitType.FLYING && unit.Faction == UnitFaction.ENEMY &&
                            unit_controller.Player.inRange(unit, unit_controller.Player.InfectionRange))
                        {
                            show = true;
                            break;
                        }
                    }
                    if (show)
                    {
                        //tip #3 - flying enemies
                        engine.ChangeGameState(GameState.PAUSED);
                        menu_controller.LoadDialogue(6);
                    }
                }
            }

            return CurLevel.BossDefeated;
        }

        #region Methods
        /*
         * Starts the current level
         */
        public void StartLevel(SoundController sound_controller)
        {
            /*
            switch (CurLevelNum)
            {
                case 0:
                    sound_controller.loop(SoundType.MUSIC, "music1");
                    break;
                default:
                    break;
            }
            */
            sound_controller.loop(SoundType.MUSIC, "music1");
        }

        /*
         * Loads the next level
         */
        public void NextLevel(){
            LoadLevel(CurLevelNum+1);
        }

        /*
         * Reloads the current level
         */
        public void ResetLevel()
        {
            LoadLevel(CurLevelNum);
        }

        /*
         * Restarts the game
         */
        public void RestartGame()
        {
            LoadLevel(0);
        }

        /*
         * Loads the specified level
         */
        public void LoadLevel(int level_num)
        {
            CurLevel = factory.loadLevel(level_num);
            CurLevelNum = level_num;
            item_controller.Reset();
            unit_controller.Reset();
            if (level_num < 3)  // Reset player, player will be created in SetLevel function of unit_controller
            {
                unit_controller.Player = null;
            }
            unit_controller.SetLevel(CurLevel);
            menu_controller.CurDialogue = 0;

            if (level_num == 1) // Tutorial #2 extra starting allies
            {
                for (int i = 0; i < TUTORIAL2_FREE_ALLIES; i++)
                {
                    unit_controller.AddAlly(null);
                }
                menu_controller.CurDialogue = 4;
            }
                
            sound_controller.stopMusic();
        }

        public void Reset()
        {
            CurLevelNum = -1;
        }
        #endregion

        public void Draw(GameCanvas canvas)
        {
            CurLevel.Draw(canvas);
        }

        public void DrawTitle(GameCanvas canvas, Vector2 center)
        {
            CurLevel.DrawTitle(canvas, center);
        }

        public void DrawWinTitle(GameCanvas canvas, Vector2 center)
        {
            canvas.DrawSprite(WinBackground, new Color(90, 0, 0),
                new Rectangle((int)(center.X - canvas.Width / 2), (int)(center.Y - canvas.Height / 2),
                    canvas.Width, canvas.Height),
                new Rectangle(0, 0, WinBackground.Width, WinBackground.Height));
            canvas.DrawText("YOU WIN THE WHOLE GAME!", Menu.fontColor, new Vector2(center.X, center.Y - 100), "font3", true);

            if (!credits_timer.IsRunning)
            {
                credits_timer.Reset();
                credits_timer.Start();
            }
            else
            {
                int dist = (int)MathHelper.Lerp(-canvas.Height/2, 1700, (float)credits_timer.ElapsedMilliseconds/CREDITS_ROLL_TIME);
                canvas.DrawText("PATHOGENESIS", Menu.fontColor, new Vector2(center.X, center.Y - dist), "font2", true);
                canvas.DrawText("Programmers:", Color.Black, new Vector2(center.X, center.Y - dist + 200), "font2", true);
                canvas.DrawText("Michael Wang", Color.Black, new Vector2(center.X, center.Y - dist + 300), "font2", true);
                canvas.DrawText("Neil Parker", Color.Black, new Vector2(center.X, center.Y - dist + 350), "font2", true);
                canvas.DrawText("Suraj Bhatia", Color.Black, new Vector2(center.X, center.Y - dist + 400), "font2", true);
                canvas.DrawText("Artwork:", Color.Black, new Vector2(center.X, center.Y - dist + 600), "font2", true);
                canvas.DrawText("Lilian Chen", Color.Black, new Vector2(center.X, center.Y - dist + 700), "font2", true);
                canvas.DrawText("Marissa Lucey", Color.Black, new Vector2(center.X, center.Y - dist + 750), "font2", true);
                canvas.DrawText("Music:", Color.Black, new Vector2(center.X, center.Y - dist + 950), "font2", true);
                canvas.DrawText("Michael Wang", Color.Black, new Vector2(center.X, center.Y - dist + 1050), "font2", true);

                if (credits_timer.ElapsedMilliseconds >= CREDITS_ROLL_TIME)
                {
                    credits_timer.Stop();
                }
            }
        }
    }
}
