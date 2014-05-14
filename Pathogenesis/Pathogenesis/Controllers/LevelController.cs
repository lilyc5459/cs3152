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

        private ContentFactory factory;
        private GameUnitController unit_controller;
        private ItemController item_controller;
        private SoundController sound_controller;
        private MenuController menu_controller;
        #endregion

        public LevelController(ContentFactory factory, GameUnitController unit_controller,
            ItemController item_controller, SoundController sound_controller, MenuController menu_controller)
        {
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
