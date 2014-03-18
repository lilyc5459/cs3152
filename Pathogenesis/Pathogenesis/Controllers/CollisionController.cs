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

        public void Update(List<GameUnit> units, Player player, Level level)
        {
            cellGrid = ConstructCollisionGrid(units, player, level);

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
        public List<GameUnit>[,] ConstructCollisionGrid(List<GameUnit> units, Player player, Level level)
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

            int x_indexp = (int)MathHelper.Clamp((player.Position.X / CELL_SIZE), 0, grid.GetLength(1) - 1);
            int y_indexp = (int)MathHelper.Clamp((player.Position.Y / CELL_SIZE), 0, grid.GetLength(0) - 1);
            if (grid[y_indexp, x_indexp] == null)
            {
                grid[y_indexp, x_indexp] = new List<GameUnit>();
            }
            grid[y_indexp, x_indexp].Add(player);

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
                            // Don't check collision for the same units or if they are in the same position (will crash)
                            if (unit != other && unit.Position != other.Position)
                            {
                                CheckUnitCollision(unit, other);
                            }
                        }
                    }
                }

                CheckWallCollision(unit, map);
            }
        }

        /*
         * Handle a collision between two units
         */
        public void CheckUnitCollision(GameUnit g1, GameUnit g2)
        {
            // Don't check collision between player and ally
            if (g1.Faction == UnitFaction.ALLY && g2.Type == UnitType.PLAYER ||
                g2.Faction == UnitFaction.ALLY && g1.Type == UnitType.PLAYER)
            {
                return;
            }

            Vector2 normal = g1.Position - g2.Position;
            float distance = normal.Length();
            normal.Normalize();

            if (distance < g1.Size)
            {
                //System.Diagnostics.Debug.WriteLine("1 pos: " + g1.Position + " 2pos: " + g2.Position +
                //    " 1size: " + g1.Size + " 2size: " + g2.Size + " norm: " + normal + " dist: " + distance);
                g1.Position += normal * (g1.Size - distance)/2; 
                g2.Position -= normal * (g2.Size - distance)/2;
                //System.Diagnostics.Debug.WriteLine(g1.Position + " " + g2.Position);

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
            List<Vector2> dirs = new List<Vector2>();
            dirs.Add(new Vector2(0, 1));
            dirs.Add(new Vector2(1, 0));
            dirs.Add(new Vector2(0, -1));
            dirs.Add(new Vector2(-1, 0));
            dirs.Add(new Vector2(1, 1));
            dirs.Add(new Vector2(1, -1));
            dirs.Add(new Vector2(-1, 1));
            dirs.Add(new Vector2(-1, -1));

            foreach (Vector2 dir in dirs)
            {
                if(!map.canMoveToWorldPos(unit.Position + dir * unit.Size/2))
                {
                    Vector2 a = (unit.Position + dir * unit.Size/2) / Map.TILE_SIZE;
                    int i = 0;
                     while (i++ < unit.Size && !map.canMoveToWorldPos(unit.Position + dir * unit.Size/2))
                    {
                        unit.Position -= dir;
                    }
                    Vector2 vel = unit.Vel;
                    if (dir.X != 0) vel.X = 0;
                    if (dir.Y != 0) vel.Y = 0;
                    unit.Vel = vel;
                    //unit.Vel = -dir * 5;
                }
            }

            /*
            float right_limit = Math.Min((unit.Position.X + unit.Size/2),map.Height);
            float left_limit = Math.Max((unit.Position.X - unit.Size/2),0);
            
            float up_limit = Math.Max((unit.Position.Y - unit.Size/2),0);
            float down_limit = Math.Min((unit.Position.Y + unit.Size/2),map.Width);

            for (float x = unit.Position.X; x < right_limit; x++)
            {
                if (CheckForWall(x,unit.Position.Y,map))
                {
                    unit.Vel = new Vector2(-unit.Vel.X,unit.Vel.Y);
                    return;
                }
            }

            for (float x = unit.Position.X; x > left_limit; x--)
            {
                if (CheckForWall(x, unit.Position.Y, map))
                {
                    unit.Vel = new Vector2(-unit.Vel.X, unit.Vel.Y);
                    return;
                }
            }

            for (float y = unit.Position.Y; y > up_limit; y--)
            {
                if (CheckForWall(unit.Position.X, y, map))
                {
                    unit.Vel = new Vector2(unit.Vel.X, -unit.Vel.Y);
                    return;
                }
            }

            for (float y = unit.Position.Y; y < down_limit; y++)
            {
                if (CheckForWall(unit.Position.X, y, map))
                {
                    unit.Vel = new Vector2(unit.Vel.X, -unit.Vel.Y);
                    return;
                }
            }
             * */
        }

        public Boolean CheckForWall(float x, float y, Map map)
        {
            Vector2 pos = new Vector2(x, y);
            pos = map.translateWorldToMap(pos);

            return map.getTileAt((int)pos.X, (int)pos.Y) == 1;
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
