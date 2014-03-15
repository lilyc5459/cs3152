using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathogenesis.Models;

namespace Pathogenesis
{
    public class LevelController
    {
        public Level CurLevel { get; set; }

        public LevelController()
        {
            //Test
            CurLevel = new Level(800, 640);
        }

        public void Update()
        {
            // Update level logic here
        }
    }
}
