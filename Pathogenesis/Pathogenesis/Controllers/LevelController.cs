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
        }

        public void Update()
        {
            // Update level logic here
        }

        #region Methods
        public void NextLevel(ContentFactory gameContentFactory){
            //Load Next Level via content factory
            CurLevelNum++;
            CurLevel = gameContentFactory.loadLevel(CurLevelNum);
        }

        public void ResetLevel(ContentFactory gameContentFactory){
            //Load Next Level via content factory
            CurLevelNum = 0;
            CurLevel = gameContentFactory.loadLevel(CurLevelNum);
        }
        #endregion

        public void Draw(GameCanvas canvas)
        {
            CurLevel.Draw(canvas);
        }

    }
}
