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
        #endregion

        public LevelController()
        {
            CurLevelNum = -1;
        }

        /*
         * Update the currentl level
         * 
         * Returns true if the level is completed
         */
        public bool Update()
        {
            return CurLevel.NumBosses != 0 && CurLevel.BossesDefeated == CurLevel.NumBosses;
        }

        #region Methods
        /*
         * Loads the next level
         */
        public void NextLevel(ContentFactory factory, GameUnitController unit_controller,
            ItemController item_controller, SoundController sound_controller){
            LoadLevel(factory, unit_controller, item_controller, sound_controller, CurLevelNum+1);
        }

        /*
         * Reloads the current level
         */
        public void ResetLevel(ContentFactory factory, GameUnitController unit_controller,
            ItemController item_controller, SoundController sound_controller)
        {
            LoadLevel(factory, unit_controller, item_controller, sound_controller, CurLevelNum);
        }

        /*
         * Restarts the game
         */
        public void Restart(ContentFactory factory, GameUnitController unit_controller,
            ItemController item_controller, SoundController sound_controller)
        {
            LoadLevel(factory, unit_controller, item_controller, sound_controller, 0);
        }

        /*
         * Loads the specified level
         */
        public void LoadLevel(ContentFactory factory, GameUnitController unit_controller, 
            ItemController item_controller, SoundController sound_controller, int level_num)
        {
            CurLevel = factory.loadLevel(level_num);
            CurLevelNum = level_num;
            Reset(CurLevel);
            item_controller.Reset();
            unit_controller.Reset();
            unit_controller.SetLevel(CurLevel);
            
            switch (level_num)
            {
                case 0:
                    sound_controller.loop(SoundType.MUSIC, "music1");
                    break;
                default:
                    break;
            }
        }

        /*
         * Resets the specified level to its beginning state
         */
        private void Reset(Level level)
        {
            level.BossesDefeated = 0;
            foreach (GameUnit boss in level.Bosses)
            {
                boss.Exists = true;
                boss.InfectionVitality = boss.max_infection_vitality;
                //boss.Position = 
            }
            foreach (GameUnit organ in level.Organs)
            {
                organ.Exists = true;
                organ.InfectionVitality = organ.max_infection_vitality;
                //organ.Position
            }

            foreach (Region r in level.Regions)
            {
                r.NumUnits = 0;
            }
        }
        #endregion

        public void Draw(GameCanvas canvas)
        {
            CurLevel.Draw(canvas);
        }

    }
}
