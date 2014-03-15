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
        private const int CELLS_GRID_WIDTH = 20;
        private const int CELLS_GRID_HEIGHT = 20;

        // Collisions cell structures
        private ArrayList[,] cellGrid;

        public CollisionController()
        {
            Initialize();
        }

        private void Initialize()
        {

        }

        /*
         * Calculates and processes all collisions that occur,
         * using the collision cell optimization structure
         */ 
        public void Update(List<GameUnit> units, Level level)
        {
            // Clear collision cells
            cellGrid = new ArrayList[CELLS_GRID_HEIGHT, CELLS_GRID_WIDTH];

            foreach (GameUnit unit in units)
            {
                int x = (int)MathHelper.Clamp(unit.Position.Y / (level.Height/CELLS_GRID_HEIGHT), 0, CELLS_GRID_WIDTH-1);
                int y = (int)MathHelper.Clamp(unit.Position.X / (level.Width/CELLS_GRID_WIDTH), 0, CELLS_GRID_HEIGHT-1);
                if(cellGrid[y,x] == null) {
                    cellGrid[y,x] = new ArrayList();
                }
                cellGrid[y,x].Add(unit);
            }
        }
    }
}
