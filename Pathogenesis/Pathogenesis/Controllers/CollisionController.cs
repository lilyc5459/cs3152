using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathogenesis.Models;
using System.Collections;
using Microsoft.Xna.Framework;

namespace Pathogenesis
{
    public class CollisionController
    {
        /*
         * Collision cell dimensions. Smaller dimensions means
         * higher resolution collision optimization, but too small means
         * some collisions won't be detected.
         */
        private const int CELLS_WIDTH = 20;
        private const int CELLS_HEIGHT = 20;

        // Collisions cell structures
        private ArrayList[,] cellGrid;

        public CollisionController()
        {
            
        }

        /*
         * Calculates and processes all collisions that occur,
         * using the collision cell optimization structure
         */ 
        public void Update(ArrayList units, Level level)
        {
            // Clear collision cells
            cellGrid = new ArrayList[CELLS_HEIGHT, CELLS_WIDTH];

            foreach (GameUnit unit in units)
            {
                cellGrid[(int)unit.Position.Y / CELLS_HEIGHT,
                         (int)unit.Position.X / CELLS_WIDTH].Add(unit);
            }
        }
    }
}
