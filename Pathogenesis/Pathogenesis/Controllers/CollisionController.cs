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
        private const int CELL_SIZE = 100;

        // Collisions cell structures
        private List<GameUnit>[,] cellGrid;
        private List<Item>[,] itemGrid;

        public CollisionController() {}

        /*
         * Calculates and processes all collisions that occur,
         * using the collision cell optimization structure
         */

        public void Update(List<GameUnit> units, Player player, Level level, ItemController item_controller)
        {
            ConstructCollisionGrids(units, item_controller.Items, player, level);

            foreach (GameUnit unit in units)
            {
                ProcessCollisions(unit, level.Map);
            }

            ProcessCollisions(player, level.Map);
            ProcessItems(player, item_controller);
        }

        /*
         * Populates the collision grid by bucketizing each unit on the map into a cell 
         * around which collisions will be processed
         */
        public void ConstructCollisionGrids(List<GameUnit> units, List<Item> items,
            Player player, Level level)
        {
            // Construct unit collision grid
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

            cellGrid = grid;

            // Contruct item collision grid
            itemGrid = new List<Item>[(int)level.Width / CELL_SIZE, (int)level.Height / CELL_SIZE];
            foreach (Item item in items)
            {
                int x_index = (int)MathHelper.Clamp((item.Position.X / CELL_SIZE), 0, grid.GetLength(1) - 1);
                int y_index = (int)MathHelper.Clamp((item.Position.Y / CELL_SIZE), 0, grid.GetLength(0) - 1);

                if (itemGrid[y_index, x_index] == null)
                {
                    itemGrid[y_index, x_index] = new List<Item>();
                }
                itemGrid[y_index, x_index].Add(item);
            }
        }

        /*
         * Process collisions for every unit
         */
        public void ProcessCollisions(GameUnit unit, Map map)
        {
            int x_index = (int)MathHelper.Clamp((unit.Position.X / CELL_SIZE),
                0, cellGrid.GetLength(1) - 1);
            int y_index = (int)MathHelper.Clamp((unit.Position.Y / CELL_SIZE),
                0, cellGrid.GetLength(0) - 1);

            List<Point> adjacent = getAdjacent(new Point(x_index, y_index));
            foreach (Point loc in adjacent)
            {
                foreach (GameUnit other in cellGrid[loc.Y, loc.X])
                {
                    // Don't check collision for the same units or if they are in the same position (will crash)
                    if (unit != other && unit.Position != other.Position && !unit.Ghost)
                    {
                        CheckUnitCollision(unit, other);
                    }
                }
            }
            CheckWallCollision(unit, map);
        }

        /*
         * Process item pickups
         */
        public void ProcessItems(Player player, ItemController item_controller)
        {
            int x_indexp = (int)MathHelper.Clamp((player.Position.X / CELL_SIZE), 0, itemGrid.GetLength(1) - 1);
            int y_indexp = (int)MathHelper.Clamp((player.Position.Y / CELL_SIZE), 0, itemGrid.GetLength(0) - 1);
            List<Point> adjacent = getAdjacent(new Point(x_indexp, y_indexp));
            foreach (Point loc in adjacent)
            {
                if (itemGrid[loc.Y, loc.X] != null)
                {
                    foreach (Item it in itemGrid[loc.Y, loc.X])
                    {
                        if (CheckItemCollision(player, it))
                        {
                            item_controller.RemoveItem(it);
                        }
                    }
                }
            }
        }

        /*
         * Handle a collision between two units
         */
        public void CheckUnitCollision(GameUnit g1, GameUnit g2)
        {
            Vector2 normal = g1.Position - g2.Position;
            float distance = normal.Length();

            // Don't check collision between player and ally
            if (g1.Faction == UnitFaction.ALLY && g2.Type == UnitType.PLAYER ||
                g2.Faction == UnitFaction.ALLY && g1.Type == UnitType.PLAYER ||
                distance == 0)
            {
                return;
            }

            normal.Normalize();

            if (distance < (g1.Size + g2.Size)/2)
            {
                //System.Diagnostics.Debug.WriteLine("1 pos: " + g1.Position + " 2pos: " + g2.Position +
                //    " 1size: " + g1.Size + " 2size: " + g2.Size + " norm: " + normal + " dist: " + distance);
                g1.Position += normal * ((g1.Size+g2.Size)/2 - distance)/2; 
                g2.Position -= normal * ((g2.Size+g1.Size)/2 - distance)/2;
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
                    int i = 0;
                    while (i++ < unit.Size && !map.canMoveToWorldPos(unit.Position + dir * unit.Size/2))
                    {
                        unit.Position -= dir;
                    }
                }
            }
        }

        /*
         * Handle collision between player and item
         */
        private bool CheckItemCollision(Player player, Item item)
        {
            if (player.distance(item) < (player.Size + Item.ITEM_SIZE)/2)
            {
                player.PickupItem(item);
                return true;
            }
            return false;
        }

        // Returns all the adjacent positions from the specified one
        private List<Point> getAdjacent(Point pos)
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
                Point loc = new Point(pos.X + dir.X, pos.Y + dir.Y);
                if (loc.Y > 0 && loc.X > 0 && loc.Y < cellGrid.GetLength(0) && loc.X < cellGrid.GetLength(1) &&
                        cellGrid[loc.Y, loc.X] != null)
                {
                    adjacent.Add(new Point(pos.X + dir.X, pos.Y + dir.Y));
                }
            }
            return adjacent;
        }
    }
}
