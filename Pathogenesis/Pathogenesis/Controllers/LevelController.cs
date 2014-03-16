using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathogenesis.Models;

namespace Pathogenesis
{
    public class LevelController
    {
        #region Fields
        public Level CurLevelObject { get; set; }       // Stores Current Level Object
        public int CurLevelNum { get; set; }            // Stores Current Level Number
        #endregion

        #region Methods
        public void NextLevel(ContentFactory gameContentFactory){
            //Load Next Level via content factory
            CurLevelNum++;
            CurLevelObject = gameContentFactory.createLevel(CurLevelNum);
        }

        public void ResetLevel(ContentFactory gameContentFactory){
            //Load Next Level via content factory
            CurLevelNum = 0;
            CurLevelObject = gameContentFactory.createLevel(CurLevelNum);
        }
        #endregion

    }
}
