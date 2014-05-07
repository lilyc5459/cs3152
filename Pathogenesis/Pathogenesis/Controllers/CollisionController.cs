using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathogenesis.Models;
using System.Collections;
using Microsoft.Xna.Framework;
using Pathogenesis.Controllers;

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
        private List<Particle>[,] particleGrid;

        private ParticleEngine particle_engine;
        private SoundController sound_controller;

        public CollisionController(SoundController sound_controller, ParticleEngine particle_engine)
        {
            this.sound_controller = sound_controller;
            this.particle_engine = particle_engine;
        }

        /*
         * Calculates and processes all collisions that occur,
         * using the collision cell optimization structure
         */

        public void Update(List<GameUnit> units, Player player, Dictionary<int, Vector2> previousPositions,
            Level level, List<Item> items, Dictionary<int, Vector2> previousItemPositions)
        {
            ConstructCollisionGrids(units, items, particle_engine.particles, player, level);

            foreach (GameUnit unit in units)
            {
                ProcessUnitCollisions(unit);
            }

            foreach (GameUnit unit in units)
            {
                if (unit.Type != UnitType.FLYING)
                {
                    HandleWallCollision(unit, level.Map, previousPositions);
                }
            }

            foreach (Item item in items)
            {
                HandleWallCollision(item, level.Map, previousItemPositions);
            }

            if (player != null)
            {
                ProcessUnitCollisions(player);
                ProcessItems(player);
                HandleWallCollision(player, level.Map, previousPositions);
            }

            /*
            foreach (Particle p in particle_engine.particles)
            {
                // If particle collides
                if (CheckParticleCollision(p))
                {
                    particle_engine.DestroyedParticles.Add(p);
                }
            }*/
        }

        /*
         * Populates the collision grid by bucketizing each unit on the map into a cell 
         * around which collisions will be processed
         */
        public void ConstructCollisionGrids(List<GameUnit> units, List<Item> items, List<Particle> particles,
            Player player, Level level)
        {
            // Construct unit collision grid
            List<GameUnit>[,] grid = new List<GameUnit>[
                (int)level.Width / CELL_SIZE, (int)level.Height / CELL_SIZE];

            foreach (GameUnit unit in units)
            {
                if (unit.Ghost) continue;

                int x_index = (int)MathHelper.Clamp((unit.Position.X / CELL_SIZE), 0, grid.GetLength(1)-1);
                int y_index = (int)MathHelper.Clamp((unit.Position.Y / CELL_SIZE), 0, grid.GetLength(0)-1);

                if (grid[y_index, x_index] == null)
                {
                    grid[y_index, x_index] = new List<GameUnit>();
                }
                grid[y_index, x_index].Add(unit);
            }

            if (player != null)
            {
                int x_indexp = (int)MathHelper.Clamp((player.Position.X / CELL_SIZE), 0, grid.GetLength(1) - 1);
                int y_indexp = (int)MathHelper.Clamp((player.Position.Y / CELL_SIZE), 0, grid.GetLength(0) - 1);
                if (grid[y_indexp, x_indexp] == null)
                {
                    grid[y_indexp, x_indexp] = new List<GameUnit>();
                }
                grid[y_indexp, x_indexp].Add(player);

                cellGrid = grid;
            }

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

            // Construct particle collision grid
            particleGrid = new List<Particle>[(int)level.Width / CELL_SIZE, (int)level.Height / CELL_SIZE];
            for (int i = 0; i < particleGrid.GetLength(0); i++)
            {
                for (int j = 0; j < particleGrid.GetLength(1); j++)
                {
                    particleGrid[i, j] = new List<Particle>();
                }
            }
            foreach (Particle p in particles)
            {
                if (!p.isProjectile) continue;

                int x_index = (int)MathHelper.Clamp((p.Position.X / CELL_SIZE), 0, grid.GetLength(1) - 1);
                int y_index = (int)MathHelper.Clamp((p.Position.Y / CELL_SIZE), 0, grid.GetLength(0) - 1);

                particleGrid[y_index, x_index].Add(p);
            }
        }

        /*
         * Process collisions for every unit
         */
        public void ProcessUnitCollisions(GameUnit unit)
        {
            if (unit == null) return;

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

                foreach (Particle p in particleGrid[loc.Y, loc.X])
                {
                    if (unit.Faction != p.Faction)
                    {
                        CheckUnitParticleCollision(unit, p);
                    }
                }
            }
        }

        /*
         * Process item pickups
         */
        public void ProcessItems(Player player)
        {
            int x_indexp = (int)MathHelper.Clamp((player.Position.X / CELL_SIZE), 0, itemGrid.GetLength(1) - 1);
            int y_indexp = (int)MathHelper.Clamp((player.Position.Y / CELL_SIZE), 0, itemGrid.GetLength(0) - 1);
            List<Point> adjacent = getAdjacent(new Point(x_indexp, y_indexp));
            foreach (Point loc in adjacent)
            {
                if (itemGrid[loc.Y, loc.X] != null)
                {
                    foreach (Item item in itemGrid[loc.Y, loc.X])
                    {
                        if (CheckItemCollision(player, item))
                        {
                            item.Destroyed = true;
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

            // Don't check collision between player and ally,
            // or between flying and non-flying units
            if (g1.Faction == UnitFaction.ALLY && g2.Type == UnitType.PLAYER ||
                g2.Faction == UnitFaction.ALLY && g1.Type == UnitType.PLAYER ||
                g1.Type == UnitType.FLYING && g2.Type != UnitType.FLYING ||
                g2.Type == UnitType.FLYING && g1.Type != UnitType.FLYING ||
                distance == 0)
            {
                return;
            }

            normal.Normalize();

            if (distance < (g1.Size + g2.Size)/2)
            {
                //System.Diagnostics.Debug.WriteLine("1 pos: " + g1.Position + " 2pos: " + g2.Position +
                //    " 1size: " + g1.Size + " 2size: " + g2.Size + " norm: " + normal + " dist: " + distance);
                if (g1.Static)
                {
                    g2.Position -= normal * ((g2.Size + g1.Size) / 2 - distance);
                }
                else if (g2.Static)
                {
                    g1.Position += normal * ((g1.Size + g2.Size) / 2 - distance);
                }
                else
                {
                    g1.Position += normal * ((g1.Size + g2.Size) / 2 - distance) / 2;
                    g2.Position -= normal * ((g2.Size + g1.Size) / 2 - distance) / 2;
                }
                //System.Diagnostics.Debug.WriteLine(g1.Position + " " + g2.Position);

                // Calculate and apply impulse
                Vector2 relVel = g1.Vel - g2.Vel;
                float impulse = (-(1 + COLL_COEFF) * Vector2.Dot(normal, relVel)) /
                    (Vector2.Dot(normal, normal) * (1 / g1.Mass + 1 / g2.Mass));
                if(!g1.Static) g1.Vel += (impulse / g1.Mass) * normal;
                if(!g2.Static) g2.Vel -= (impulse / g2.Mass) * normal;

                // Clamp velocities to speed
                Vector2 vel1 = g1.Vel;
                if (vel1.Length() > g1.Speed)
                {
                    vel1 *= g1.Speed / vel1.Length();
                }
                g1.Vel = vel1;

                Vector2 vel2 = g2.Vel;
                if (vel2.Length() > g2.Speed)
                {
                    vel2 *= g2.Speed / vel2.Length();
                }
                g2.Vel = vel2;
            }
        }

        /*
         * Handle a collision between a game entity and wall
         */ 
        public void HandleWallCollision(GameEntity entity, Map map, Dictionary<int, Vector2> previousPositions)
        {
            if (!previousPositions.ContainsKey(entity.ID)) return;

            Vector2 pos_change = entity.Position - previousPositions[entity.ID];
            float change_length = pos_change.Length();

            // Maybe the previous position thing is wrong cause it gets updated to a new, unwalkable position
            //// Maybe keep track of the last known uncollided previous position, update previous positions only if their new position is walkable
            List<Vector2> dirs = new List<Vector2>();
            if (pos_change.X > 0)
            {
                dirs.Add(new Vector2(1, 0));
            }
            else if (pos_change.X < 0)
            {
                dirs.Add(new Vector2(-1, 0));
            }
            if (pos_change.Y > 0)
            {
                dirs.Add(new Vector2(0, 1));
            }
            else if (pos_change.Y < 0)
            {
                dirs.Add(new Vector2(0, -1));
            }

            if (pos_change.X != 0 && pos_change.Y != 0)
            {
                dirs.Add(new Vector2(1, 1));
                dirs.Add(new Vector2(-1, -1));
                dirs.Add(new Vector2(1, -1));
                dirs.Add(new Vector2(-1, 1));
            }

            foreach (Vector2 dir in dirs)
            {
                if (!map.canMoveToWorldPos(entity.Position + dir * entity.Size / 2))
                {
                    int i = 0;
                    while (i++ < entity.Size * 3 && !map.canMoveToWorldPos(entity.Position + dir * entity.Size / 2))
                    {
                        entity.Position -= dir;
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
                return player.PickupItem(item);
            }
            return false;
        }

        /*
         * Handle particle collision
         * Easy because each particle has a target
         */
        private void CheckUnitParticleCollision(GameUnit unit, Particle p)
        {
            if (unit.Type == UnitType.ORGAN) return;

            if ((unit.Position - p.Position).Length() < (unit.Size + p.Size) / 2)
            {
                if (!unit.Invulnerable)
                {
                    unit.Health -= p.Damage;
                    unit.Damaged = true;
                }
                particle_engine.GenerateParticle(5, p.Color, p.Position, null, UnitFaction.ALLY, false, false, 0,
                    10, 5, 1, 1, 30, 10, unit.Position - p.Position);
                particle_engine.DestroyedParticles.Add(p);

                if (unit.Type == UnitType.PLAYER)
                {
                    sound_controller.play(SoundType.EFFECT, "ouch");
                }
            }
        }

        /*
         * Handle particle collision
         * Easy because each particle has a target
         */
        private bool CheckParticleCollision(Particle p)
        {
            GameUnit target = p.Target;
            if (p.isProjectile && target != null &&
                (target.Position - p.Position).Length() < (target.Size + p.Size) / 2)
            {
                target.Health -= p.Damage;
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
                if (loc.Y >= 0 && loc.X >= 0 && loc.Y < cellGrid.GetLength(0) && loc.X < cellGrid.GetLength(1) &&
                        cellGrid[loc.Y, loc.X] != null)
                {
                    adjacent.Add(new Point(pos.X + dir.X, pos.Y + dir.Y));
                }
            }
            return adjacent;
        }
    }
}
