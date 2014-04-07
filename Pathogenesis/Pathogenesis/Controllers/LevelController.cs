using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathogenesis.Models;
using Microsoft.Xna.Framework;

namespace Pathogenesis
{
    public class LevelController
    {
        #region Fields
        public Level CurLevel { get; set; }       // Stores Current Level Object
        public int CurLevelNum { get; set; }            // Stores Current Level Number
        #endregion

        public LevelController()
        {
            CurLevelNum = 0;
        }

        /*
         * Update the currentl level
         * 
         * Returns true if the level is completed
         */
        public bool Update()
        {
            // Update level logic here
            return CurLevel.BossesDefeated == CurLevel.NumBosses;
        }

        #region Methods
        /*
         * Loads the next level
         */
        public void NextLevel(ContentFactory factory, GameUnitController unit_controller,
            ItemController item_controller){
            LoadLevel(factory, unit_controller, item_controller, CurLevelNum);
            CurLevelNum++;
        }

        /*
         * Reloads the current level
         */
        public void ResetLevel(ContentFactory factory, GameUnitController unit_controller,
            ItemController item_controller)
        {
            LoadLevel(factory, unit_controller, item_controller, CurLevelNum);
        }

        /*
         * Loads the specified level
         */
        public void LoadLevel(ContentFactory factory, GameUnitController unit_controller, 
            ItemController item_controller, int level_num)
        {
            CurLevel = factory.loadLevel(level_num);
            item_controller.Reset();
            unit_controller.Reset();
            unit_controller.SetLevel(CurLevel);
        }
        #endregion

        public void Draw(GameCanvas canvas)
        {
            CurLevel.Draw(canvas);
        }

    }
}
