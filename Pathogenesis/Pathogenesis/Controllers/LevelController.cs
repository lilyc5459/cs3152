using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathogenesis.Models;
using Microsoft.Xna.Framework;
using Pathogenesis.Controllers;

namespace Pathogenesis
{
    public class LevelController
    {
        #region Fields
        public Level CurLevel { get; set; }     // Stores Current Level Object
        public int CurLevelNum { get; set; }    // Stores Current Level Number

        private GameEngine engine;
        private ContentFactory factory;
        private GameUnitController unit_controller;
        private ItemController item_controller;
        private SoundController sound_controller;
        private MenuController menu_controller;
        #endregion

        public LevelController(GameEngine engine, ContentFactory factory, GameUnitController unit_controller,
            ItemController item_controller, SoundController sound_controller, MenuController menu_controller)
        {
            this.engine = engine;
            this.factory = factory;
            this.unit_controller = unit_controller;
            this.item_controller = item_controller;
            this.sound_controller = sound_controller;
            this.menu_controller = menu_controller;

            CurLevelNum = -1;
        }

        /*
         * Update the currentl level
         * 
         * Returns true if the level is completed
         */
        public bool Update()
        {
            // Tutorial #1
            if (CurLevelNum == 0)
            {
                if (menu_controller.CurDialogue == 0 && unit_controller.Player != null)
                {
                    if (CurLevel.Regions[0].RegionSet.Contains(new Vector2(
                        (int)unit_controller.Player.Position.X / Map.TILE_SIZE,
                        (int)unit_controller.Player.Position.Y / Map.TILE_SIZE)))
                    {
                        //tip #1
                        engine.ChangeGameState(GameState.PAUSED);
                        menu_controller.LoadDialogue(0);
                    }
                }
                if (menu_controller.CurDialogue == 1 && unit_controller.Player != null)
                {
                    bool show = false;
                    foreach (GameUnit unit in unit_controller.Units)
                    {
                        if (unit.Type == UnitType.TANK && unit.Faction == UnitFaction.ENEMY &&
                            unit_controller.Player.inRange(unit, 300))
                        {
                            show = true;
                        }
                    }
                    if (show)
                    {
                        //tip #2
                        engine.ChangeGameState(GameState.PAUSED);
                        menu_controller.LoadDialogue(1);
                    }
                }
                if (menu_controller.CurDialogue == 2 && unit_controller.Player != null)
                {
                    bool show = false;
                    foreach (GameUnit unit in CurLevel.Organs)
                    {
                        if (unit_controller.Player.inRange(unit, 350))
                        {
                            show = true;
                        }
                    }
                    if (show)
                    {
                        //tip #3
                        engine.ChangeGameState(GameState.PAUSED);
                        menu_controller.LoadDialogue(2);
                    }
                }
                if (menu_controller.CurDialogue == 3 && unit_controller.Player != null)
                {
                    bool show = false;
                    foreach (GameUnit unit in CurLevel.Bosses)
                    {
                        if (unit_controller.Player.inRange(unit, 400))
                        {
                            show = true;
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

            return CurLevel.BossDefeated;
        }

        #region Methods
        /*
         * Starts the current level
         */
        public void StartLevel(SoundController sound_controller)
        {
            switch (CurLevelNum)
            {
                case 0:
                    sound_controller.loop(SoundType.MUSIC, "music1");
                    break;
                default:
                    break;
            }
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
            unit_controller.SetLevel(CurLevel);
            menu_controller.CurDialogue = 0;
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
    }
}
