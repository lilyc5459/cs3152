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
        private const float COLL_COEFF = 0.1f;
        private const int CELL_SIZE = 20;

        // Collisions cell structures
        private List<GameUnit>[,] cellGrid;

        public CollisionController()
        {
        }

        /*
         * Calculates and processes all collisions that occur,
         * using the collision cell optimization structure
         */

        public void Update(List<GameUnit> units, Level level)
        {
            cellGrid = ConstructCollisionGrid(units, level);

            for (int ii = 0; ii < cellGrid.GetLength(0); ii++)
            {
                for (int jj = 0; jj < cellGrid.GetLength(1); jj++)
                {
                    if (cellGrid[ii, jj] != null)
                    {
                        ProcessCollisions(jj, ii, level.Map);
                    }
                }
            }
        }

        /*
         * Populates the collision grid by bucketizing each unit on the map into a cell 
         * around which collisions will be processed
         */
        public List<GameUnit>[,] ConstructCollisionGrid(List<GameUnit> units, Level level)
        {
            List<GameUnit>[,] grid = new List<GameUnit>[
                (int)level.Width / CELL_SIZE, (int)level.Height / CELL_SIZE];

            foreach (GameUnit unit in units)
            {
                int x_index = (int)MathHelper.Clamp((unit.Position.X / CELL_SIZE), 0, grid.GetLength(1)-1);
                int y_index = (int)MathHelper.Clamp((unit.Position.Y / CELL_SIZE), 0, grid.GetLength(0)-1);

                if (grid[y_index, x_index] == null)
                {
                    grid[y_index, x_index] = new List<GameUnit>();
                }
                grid[y_index, x_index].Add(unit);
            }
            return grid;
        }

        /*
         * Process collisions for every unit
         */
        public void ProcessCollisions(int x, int y, Map map)
        {
            List<Point> adjacent = getAdjacent(new Point(x, y));
            foreach (GameUnit unit in cellGrid[y, x])
            {
                foreach (Point loc in adjacent)
                {
                    if (loc.Y > 0 && loc.X > 0 && loc.Y < cellGrid.GetLength(0) && loc.X < cellGrid.GetLength(1) &&
                        cellGrid[loc.Y, loc.X] != null)
                    {
                        foreach (GameUnit other in cellGrid[loc.Y, loc.X])
                        {
                            if (unit != other)
                            {
                                CheckUnitCollision(unit, other);
                            }
                        }
                    }
                }

                //CheckWallCollision(unit, map);
            }
        }

        /*
         * Handle a collision between two units
         */
        public void CheckUnitCollision(GameUnit g1, GameUnit g2)
        {
            Vector2 normal = g1.Position - g2.Position;
            float distance = normal.Length();
            normal.Normalize();

            if (distance < g1.Size)
            {
                g1.Position += normal * (g1.Size - distance) / 2;
                g2.Position -= normal * (g2.Size - distance) / 2;

                Vector2 relVel = g1.Vel - g2.Vel;

                float impulse = (-(1 + COLL_COEFF) * Vector2.Dot(normal, relVel)) / (Vector2.Dot(normal, normal) * (1 / g1.Mass + 1 / g2.Mass));

                g1.Vel += (impulse / g1.Mass) * normal;
                g2.Vel -= (impulse / g2.Mass) * normal;
            }
        }

        /*
         * Handle a collision between a unit and wall
         */ 
        public void CheckWallCollision(GameUnit unit, Map map)
        {


        }

        // Returns all the adjacent positions from the specified one
        private static List<Point> getAdjacent(Point pos)
        {
            List<Point> adjacent = new List<Point>();
            List<Point> dirs = new List<Point>();
            dirs.Add(new Point(0, 0));
            dirs.Add(new Point(0, 1));
            dirs.Add(new Point(1, 0));
            dirs.Add(new Point(0, -1));
            dirs.Add(new Point(-1, 0));
            dirs.Add(new Point(1, 1));
            dirs.Add(new Point(1, -1));
            dirs.Add(new Point(-1, 1));
            dirs.Add(new Point(-1, -1));
            foreach (Point dir in dirs)
            {
                adjacent.Add(new Point(pos.X + dir.X, pos.Y + dir.Y));
            }
            return adjacent;
        }
    }
}
