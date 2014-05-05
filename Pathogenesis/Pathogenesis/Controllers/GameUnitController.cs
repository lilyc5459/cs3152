using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Pathogenesis.Models;
using Pathogenesis.Pathfinding;
using Pathogenesis.Controllers;

namespace Pathogenesis
{
    /*
     * Controls all unit logic and management,
     * including movement, pathfinding, AI, and updating
     */ 
    public class GameUnitController
    {
        #region Constants
        public const int COMBAT_GRID_CELL_SIZE = 100; // Must be enough to accomodate the largest unit size
        public const int NUM_ALLY_FINDERS = 10;

        public const int PLAYER_PATHFIND_FIELD_SIZE = 15;
        public const int MAX_ASTAR_DIST = 20;
        public const int LOST_ALLY_DISTANCE = 300;

        public const int ENEMY_CHASE_RANGE = 250;   // Distance at which an enemy will start chasing the player
        public const int MAX_ALLIES = 100;          // Maximum number of allies allowed
        public const int TARGET_STOP_DIST = 50;     // Distance at which a unit is considered "at" its target
        public const int MOVE_STOP_DIST = 5;
        public const int ATTACK_COOLDOWN = 50;      // Attack cooldown
        public const int ATTACK_LOCK_RANGE = 30;    // Distance at which enemies and allies will lock on to each other
        public const int ALLY_FOLLOW_RANGE = 200;
        public const int INFECTION_SPEED = 3;
        public const float INFECTION_RECOVER_SPEED = 0.5f;
        public const float ALLY_ATTRITION = 0.0f;

        public const float RANDOM_WALK_TARGET_PROB = 0.05f; // Probability that a unit will switch targets while random walking

        // Player parameters
        public const int MAX_PLAYER_INFECT_RANGE = 320;    // Max player infection range
        public const float MAX_PLAYER_SPEED = 9;    // Max overall player speed
        public const float MAX_PLAYER_HEALTH = 500;    // Max overall player health
        public const int MAX_PLAYER_INFECTION_POINTS = 2000;    // Max overall player infection points
        public const float MAX_PLAYER_INFECTION_REGEN = 1;    // Max overall player infection regen speed

        public const int EXPLORE_SIGHT_RANGE = 15;   // The range of the player's explore vision, updating the minimap
        
        // Spawning parameters
        public const float IMMUNE_SPAWN_PROB = 0.0f;    //TODO change
        public const int RANDOM_SPAWN_DIST = 10;

        // Item parameters
        public const int ORGAN_ITEM_DROP_NUM = 5;

        public const int PLASMID_POINTS = 120;      // Conversion points gained from picking up a plasmid
        public const int HEALTH_POINTS = 100;       // Health points gained from picking up a health item
        public const int ITEM_FREE_ALLY_NUM = 3;    // The number of allies received from an item pickup
        public const float ITEM_RANGE_INCREASE = 1.2f;  // The amount of range increse upon picking up range item
        public const float ITEM_SPEED_INCREASE = 1.1f;  // The amount of speed increse upon picking up speed item
        public const float ITEM_MAX_HEALTH_INCREASE = 1.2f;  // The amount of speed increse upon picking up speed item
        public const float ITEM_INFECT_POINTS_INCREASE = 1.2f;  // The amount of speed increse upon picking up speed item
        public const float ITEM_INFECTION_REGEN_INCREASE = 1.2f;  // The amount of speed increse upon picking up speed item
        #endregion

        #region Fields
        private ContentFactory factory;                     // Instance of the content factory
        private ParticleEngine particle_engine;             // Instance of the particle engine
        private SoundController sound_controller;
        private ItemController item_controller;             // Instance of the item controller

        public List<GameUnit> Units { get; set; }           // A list of all the units currently in the game
        public List<GameUnit> DeadUnits { get; set; }       // Dead units to be destroyed this frame
        public List<GameUnit> ConvertedUnits { get; set; }  // Units to be converted this frame
        public List<GameUnit> SpawnedUnits { get; set; }    // Units to be spawned this frame

        private List<GameUnit> lostUnits;                   // Lost allies

        // The player object
        public Player Player { get; set; }

        // Combat processing grid to reduce number of checks
        private List<GameUnit>[,] combatRangeGrid;

        // A dictionary of <unit ID, Position> of each unit's previous position. Used for collision.
        public Dictionary<int, Vector2> PreviousPositions { get; set; }

        // Random number generator. Must use the same instance or number generated in quick succession will be the same
        private Random rand;
        #endregion

        #region Initialization
        public GameUnitController(ContentFactory factory, SoundController sound_controller,
            ParticleEngine particle_engine, ItemController item_controller)
        {
            this.factory = factory;
            this.sound_controller = sound_controller;
            this.particle_engine = particle_engine;
            this.item_controller = item_controller;
            Units = new List<GameUnit>();
            DeadUnits = new List<GameUnit>();
            ConvertedUnits = new List<GameUnit>();
            SpawnedUnits = new List<GameUnit>();
            lostUnits = new List<GameUnit>();
            PreviousPositions = new Dictionary<int, Vector2>();

            rand = new Random();
        }

        public void Reset()
        {
            Units.Clear();
            DeadUnits.Clear();
            ConvertedUnits.Clear();
            SpawnedUnits.Clear();
        }

        /*
         * Add a unit to the game
         */
        public void AddUnit(GameUnit unit)
        {
            if (unit.Region != null)
            {
                unit.Region.NumUnits++;
            }
            Units.Add(unit);
        }

        /*
         * Replace the given unit with an ally
         */
        public GameUnit AddAlly(GameUnit unit)
        {
            GameUnit newUnit = null;
            if(unit == null)
            {
                newUnit = factory.createAlly(Player.Position); 
            }
            else
            {
                DeadUnits.Add(unit);
                newUnit = factory.createUnit(unit.Type, unit.Faction, unit.Level, unit.Position, unit.Immune);
            }
            AddUnit(newUnit);
            return newUnit;
        }

        /*
         * Initialize units for the specified level
         */
        public void SetLevel(Level level)
        {
            Reset();
            foreach (GameUnit boss in level.Bosses)
            {
                Units.Add(boss);
            }
            foreach (GameUnit organ in level.Organs)
            {
                Units.Add(organ);
            }
            if (Player == null)
            {
                Player = factory.createPlayer(new Vector2(0, 0));
            }
            Player.Position = level.PlayerStart * Map.TILE_SIZE;
            Player.Health = Player.max_health;
            Player.InfectionPoints = Player.MaxInfectionPoints;

            // Clear player explored tiles
            int[][] tiles = new int[level.Height / Map.TILE_SIZE][];
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = new int[level.Width / Map.TILE_SIZE];
            }
            Player.ExploredTiles = tiles;

            // Initial free allies
            for (int i = 0; i < ITEM_FREE_ALLY_NUM; i++)
            {
                AddAlly(null);
            }

            //Change TODO
            foreach (Region r in level.Regions)
            {
                if (r.NumUnits >= r.MaxUnits) continue;

                foreach (SpawnPoint sp in r.SpawnPoints)
                {
                    UnitType? type = selectTypeWithProbability(sp.UnitProbabilities);
                    int? unit_lvl = selectIntWithProbability(sp.LevelProbabilities);

                    if (type != null && unit_lvl != null)
                    {
                        // Create new unit
                        GameUnit unit = factory.createUnit((UnitType)type, UnitFaction.ENEMY, (int)unit_lvl,
                            sp.Pos * Map.TILE_SIZE +
                            new Vector2((float)rand.NextDouble() * RANDOM_SPAWN_DIST,
                                        (float)rand.NextDouble() * RANDOM_SPAWN_DIST),
                                        rand.NextDouble() < IMMUNE_SPAWN_PROB);
                        if (unit != null)
                        {
                            unit.Region = r;
                            AddUnit(unit);
                        }
                    }
                }
            }

        }
        #endregion

        #region Update
        /*
         * Update all units
         */
        public void Update(Level level, InputController input_controller)
        {
            ConstructCombatGrid(level);
            ConvertedUnits.Clear();
            SpawnedUnits.Clear();
            DeadUnits.Clear();

            // Handle player logic
            bool playerFrontBlocked = false;
            if (Player != null)
            {
                UpdatePreviousPosition(Player, level.Map);
                playerFrontBlocked =
                    level.Map.rayCastHasObstacle(Player.Position, Player.Front, Map.TILE_SIZE / 3);
                CheckPlayerInput(input_controller);
                moveUnit(Player);
                UpdatePlayer();
            }

            // Record each unit's position
            foreach (GameUnit unit in Units)
            {
                UpdatePreviousPosition(unit, level.Map);
            }

            // Execute moves and actions
            foreach (GameUnit unit in Units)
            {
                // Handle dead unit
                if (unit.Dead)
                {
                    DeadUnits.Add(unit);
                    continue;
                }

                // Set next moves
                if (unit.Faction == UnitFaction.ALLY)
                {
                    setAllyTarget(unit, level, playerFrontBlocked);
                }
                else
                {
                    setEnemyTarget(unit, level);
                }
                setVelocity(unit);
                moveUnit(unit);
                ProcessCombat(unit);
                UpdateUnit(unit, level);
            }

            // Convert units
            foreach (GameUnit unit in ConvertedUnits)
            {
                UpdatePreviousPosition(Convert(unit), level.Map);
            }

            // Spawn units from level
            SpawnUnits(level);

            // Spawning from other sources
            foreach (GameUnit unit in SpawnedUnits)
            {
                AddUnit(unit);
            }

            // Dispose of dead units
            foreach (GameUnit unit in DeadUnits)
            {
                if (unit.Type == UnitType.PLAYER)
                {
                    Player.Exists = false;
                    Player = null;
                }
                else
                {
                    if (unit.Region != null)
                    {
                        unit.Region.NumUnits--;
                    }
                    Units.Remove(unit);
                }
                PreviousPositions.Remove(unit.ID);
            }
        }
        #endregion

        #region Spawning
        /*
         * Spawn enemy units in the level
         */
        private void SpawnUnits(Level level)
        {
            foreach (Region r in level.Regions)
            {
                if (r.NumUnits >= r.MaxUnits) continue;

                foreach (SpawnPoint sp in r.SpawnPoints)
                {
                    if (!sp.ShouldSpawn()) continue;

                    UnitType? type = selectTypeWithProbability(sp.UnitProbabilities);
                    int? unit_lvl = selectIntWithProbability(sp.LevelProbabilities);

                    if(type != null && unit_lvl != null) {
                        // Create new unit
                        GameUnit unit = factory.createUnit((UnitType)type, UnitFaction.ENEMY, (int)unit_lvl,
                            sp.Pos * Map.TILE_SIZE +
                            new Vector2((float)rand.NextDouble() * RANDOM_SPAWN_DIST,
                                        (float)rand.NextDouble() * RANDOM_SPAWN_DIST),
                                        rand.NextDouble() < IMMUNE_SPAWN_PROB);
                        if (unit != null)
                        {
                            unit.Region = r;
                            AddUnit(unit);
                        }
                    }
                }
            }
        }

        // Selects a key according to the probability values
        private UnitType? selectTypeWithProbability(Dictionary<UnitType, float> prob_map)
        {
            float total = 0;
            foreach (float val in prob_map.Values)
            {
                total += val;
            }
            double index = rand.NextDouble() * total;
            float cur_val = 0;
            foreach (UnitType type in prob_map.Keys)
            {
                cur_val += prob_map[type];
                if (cur_val > index)
                {
                    return type;
                }
            }
            return null;
        }

        // Selects a key according to the probability values
        private int? selectIntWithProbability(Dictionary<int, float> prob_map)
        {
            float total = 0;
            foreach (float val in prob_map.Values)
            {
                total += val;
            }
            double index = rand.NextDouble() * total;
            float cur_val = 0;
            foreach (int level in prob_map.Keys)
            {
                cur_val += prob_map[level];
                if (cur_val > index)
                {
                    return level;
                }
            }
            return null;
        }
        #endregion

        #region Player logic
        /*
         * Process player input and apply to player object
         */
        private void CheckPlayerInput(InputController input_controller)
        {
            Player.UpdateAnimation();

            Vector2 vel = Player.Vel;
            if (input_controller.Left) { vel.X -= Player.Accel; }
            if (input_controller.Right) { vel.X += Player.Accel; }
            if (input_controller.Up) { vel.Y -= Player.Accel; }
            if (input_controller.Down) { vel.Y += Player.Accel; }

            // Clamp values to max speeds
            if (vel.Length() > Player.Speed)
            {
                vel.Normalize();
                vel *= Player.Speed;
            }
            
            Player.Vel = vel;
            if (input_controller.StartConverting)
            {
                BeginInfection();
            }
            else if (input_controller.Converting)
            {
                Infect();
            }
            else
            {
                Player.Infecting = null;
            }
        }

        /*
         * Applies the infection effect to the current infection target
         */
        private void Infect()
        {
            if (Player.Infecting != null)
            {
                if (Player.InfectionPoints > Player.InfectionRecovery &&
                    Player.Infecting.InfectionVitality > 0 && 
                    Player.Infecting.inRange(Player, Player.InfectionRange))
                {
                    Player.Infecting.InfectionVitality -= INFECTION_SPEED;

                    Player.InfectionPoints -= (int)INFECTION_SPEED;
                    Player.InfectionPoints = (int)MathHelper.Clamp(Player.InfectionPoints,
                        0, Player.MaxInfectionPoints);

                    particle_engine.EmitterPosition = Player.Position;
                    particle_engine.GenerateParticle(1, new Color(200, 200, 0), Player.Position,
                        Player.Infecting, UnitFaction.ALLY, true, false, 0, 15, 5, 12, 7);
                }
                else
                {
                    Player.Infecting = null;                    
                }
            }
        }

        /*
         * Searches for the closest enemy within infection range sets it as the player's infection target
         */
        private void BeginInfection()
        {
            // Search for closest enemy within infection range
            GameUnit closestInRange = null;
            foreach (GameUnit unit in Units)
            {
                if (unit.Exists && !unit.Immune && unit.Faction == UnitFaction.ENEMY &&
                    Player.inRange(unit, Player.InfectionRange))
                {
                    if (closestInRange == null || Player.distance(unit) < Player.distance(closestInRange))
                    {
                        closestInRange = unit;
                    }
                }
            }

            // Convert the enemy!
            if (closestInRange != null && !Player.MaxAllies)
            {
                Player.Infecting = closestInRange;
            }
        }

        private void UpdatePlayer()
        {
            if (Player.Health <= 0) DeadUnits.Add(Player);

            // Apply recovery. Only recover when the player is not currently infecting (otherwise they have infinite conversion power)
            if (Player.Infecting == null)
            {
                Player.InfectionPoints += Player.InfectionRecovery;
            }

            List<Vector2> explored = GetExploredTiles(Player.Position / Map.TILE_SIZE);
            foreach (Vector2 tile in explored)
            {
                if (tile.Y >= 0 && tile.X >= 0 && 
                    tile.Y < Player.ExploredTiles.Length && tile.X < Player.ExploredTiles[0].Length)
                {
                    Player.ExploredTiles[(int)tile.Y][(int)tile.X] = 1;
                }
            }

            // Apply items
            foreach(Item item in Player.Items)
            {
                switch (item.Type)
                {
                    case ItemType.PLASMID:
                        Player.InfectionPoints += PLASMID_POINTS;
                        break;
                    case ItemType.HEALTH:
                        Player.Health += HEALTH_POINTS;
                        break;
                    case ItemType.ATTACK:
                        break;
                    case ItemType.ALLIES:
                        for (int i = 0; i < ITEM_FREE_ALLY_NUM; i++)
                        {
                            AddAlly(null);
                        }
                        break;
                    case ItemType.RANGE:
                        Player.InfectionRange = (int)MathHelper.Clamp(
                            Player.InfectionRange * ITEM_RANGE_INCREASE, 0, MAX_PLAYER_INFECT_RANGE);
                        break;
                    case ItemType.SPEED:
                        Player.Speed = MathHelper.Clamp(
                            Player.Speed * ITEM_SPEED_INCREASE, 0, MAX_PLAYER_SPEED);
                        break;
                    case ItemType.MAX_HEALTH:
                        Player.max_health = (int)MathHelper.Clamp(
                            Player.max_health * ITEM_MAX_HEALTH_INCREASE, 0, MAX_PLAYER_HEALTH);
                        break;
                    case ItemType.MAX_INFECT:
                        Player.MaxInfectionPoints = (int)MathHelper.Clamp(
                            Player.MaxInfectionPoints * ITEM_INFECT_POINTS_INCREASE, 0, MAX_PLAYER_INFECTION_POINTS);
                        break;
                    case ItemType.INFECT_REGEN:
                        Player.InfectionRecovery = MathHelper.Clamp(
                            Player.InfectionRecovery * ITEM_INFECTION_REGEN_INCREASE, 0, MAX_PLAYER_INFECTION_REGEN);
                        break;
                    case ItemType.MYSTERY:
                        Player.Speed = MathHelper.Clamp(
                            Player.Speed * ITEM_SPEED_INCREASE, 0, MAX_PLAYER_SPEED);
                        break;
                }
            }

            // Set field limits
            Player.InfectionPoints = MathHelper.Clamp(Player.InfectionPoints,
                0, Player.MaxInfectionPoints);
            Player.Health = MathHelper.Clamp(Player.Health, 0, Player.max_health);
            Player.Items = new List<Item>();
        }

        /*
         * Returns a list of all tile positions within exploration range of the given position
         */
        private List<Vector2> GetExploredTiles(Vector2 pos)
        {
            List<Vector2> tiles = new List<Vector2>();
            for (int i = -EXPLORE_SIGHT_RANGE; i < EXPLORE_SIGHT_RANGE; i++)
            {
                for (int j = -EXPLORE_SIGHT_RANGE; j < EXPLORE_SIGHT_RANGE; j++)
                {
                    if (i * i + j * j < EXPLORE_SIGHT_RANGE * EXPLORE_SIGHT_RANGE)
                    {
                        tiles.Add(new Vector2(pos.X + i, pos.Y + j));
                    }
                }
            }
            return tiles;
        }
        #endregion

        #region Targeting and Pathfinding

        /*
         * Determine the next move for this ally unit with
         * targeting specific to each unit type AI
         */
        public void setAllyTarget(GameUnit unit, Level level, bool playerFrontBlocked)
        {
            if (!unit.Exists || unit.Position.X < 0 || unit.Position.Y < 0) return;

            UnitFaction faction = unit.Faction;     // Unit faction, only called once
            Vector2 prev_move = unit.NextMove;
            bool random_walk = false;

            // Handle lost allies or dead player
            if (unit.Lost || Player == null)
            {
                if (rand.NextDouble() < 0.01 && Player != null)     // Look for the player
                {
                    unit.Target = Player.Position;
                    findTarget(unit, Player.Position, level.Map, MAX_ASTAR_DIST);
                }
                else if (rand.NextDouble() < RANDOM_WALK_TARGET_PROB)      // Random walk
                {
                    unit.Target = unit.Position + new Vector2(rand.Next(600) - 300, rand.Next(600) - 300);
                    unit.NextMove = unit.Target;
                    random_walk = true;
                }
                return;
            }

            switch (unit.Type)
            {
                case UnitType.TANK:
                    if (playerFrontBlocked)
                    {
                        unit.Target = Player.Position;
                    }
                    else
                    {
                        unit.Target = Player.Front;
                    }

                    // Chase the closest enemy in range
                    GameUnit closest = findClosestEnemyInRange(unit, unit.AttackLockRange);
                    if (closest != null)
                    {
                        unit.Target = closest.Position;
                        unit.Attacking = closest;
                    }

                    if (!Player.inRange(unit, ALLY_FOLLOW_RANGE) ||
                        closest != null && closest == Player.Infecting)
                    {
                        unit.Attacking = null;
                        unit.Target = Player.Position;
                    }
                    break;
                case UnitType.RANGED:
                    break;
                case UnitType.FLYING:
                    goto case UnitType.TANK;
                    break;
                default:
                    break;
            }
            setNextMove(unit, level, prev_move, random_walk);
        }

        /*
         * Determine the next move for this enemy unit with
         * targeting specific to each unit type AI
         */ 
        public void setEnemyTarget(GameUnit unit, Level level)
        {
            if (!unit.Exists || unit.Position.X < 0 || unit.Position.Y < 0) return;

            UnitFaction faction = unit.Faction;     // Unit faction, only called once
            Vector2 prev_move = unit.NextMove;
            bool random_walk = false;
            
            // Select target
            switch (unit.Type)
            {
                case UnitType.TANK:     // tank AI
                    GameUnit closest = null;
                    // Immune lvl 1 enemies cluster around lvl 2 enemies
                    if (unit.Immune && unit.Level == 1)
                    {
                        closest = findClosestOfTypeInRange(unit, unit.Type, 2, unit.AttackLockRange);
                        if (closest != null)
                        {
                            unit.Target = closest.Position;
                        }
                    }

                    if (Player != null && Player.Exists) {
                        // Chase player
                        if (Player.inRange(unit, ENEMY_CHASE_RANGE) && !level.Map.rayCastHasObstacle(unit.Position, Player.Position, unit.Size / 2))
                        {
                            unit.Target = Player.Position;
                        }
                        // Pathfind pack to region center if outside of region
                        else if (unit.Region != null && !unit.Region.RegionSet.Contains(new Vector2(
                                (int)unit.Position.X / Map.TILE_SIZE,
                                (int)unit.Position.Y / Map.TILE_SIZE)))
                        {
                            unit.Target = unit.Region.Center * Map.TILE_SIZE;
                        }
                        // Random walk
                        else if (rand.NextDouble() < RANDOM_WALK_TARGET_PROB)
                        {
                            random_walk = true;
                        }
                    }
                    else if (rand.NextDouble() < RANDOM_WALK_TARGET_PROB)
                    {
                        random_walk = true;
                    }

                    // Random walk
                    if (random_walk)
                    {
                        unit.Target = unit.Position + new Vector2(rand.Next(600) - 300, rand.Next(600) - 300);
                    }
                    
                    // Chase the closest enemy in range
                    closest = findClosestEnemyInRange(unit, unit.AttackLockRange);
                    if (closest != null)
                    {
                        unit.Target = closest.Position;
                        unit.Attacking = closest;
                    }
                    break;
                case UnitType.RANGED:
                    // ranged AI
                    break;
                case UnitType.FLYING:
                    // flying AI
                    //TEMPPP
                    goto case UnitType.TANK;
                    break;
                case UnitType.BOSS:
                    switch (unit.Level)
                    {
                        case 1:
                            // Attack the player if in range
                            if (Player != null)
                            {
                                if (Player.inRange(unit, unit.AttackRange))
                                {
                                    unit.Attacking = Player;
                                }
                                if (Player.inRange(unit, ENEMY_CHASE_RANGE))
                                {
                                    unit.Target = Player.Position;
                                }
                            }
                            break;
                        case 2:
                            break;
                    }
                    break;
                default:
                    // Player case, do nothing
                    break;
            }
            setNextMove(unit, level, prev_move, random_walk);
        }

        /*
         * Set the next move for the unit based on the visibility of its target
         */
        private void setNextMove(GameUnit unit, Level level, Vector2 prev_move, bool random_walk)
        {
            unit.NextMove = unit.Target;
            if (unit.HasTarget() && unit.Type != UnitType.FLYING)
            {
                // Pathfind to target if necessary
                if (!random_walk && level.Map.rayCastHasObstacle(unit.Position, unit.Target, unit.Size / 2))
                {
                    if (rand.NextDouble() < 0.1)
                    {
                        findTarget(unit, unit.Target, level.Map, 1000);
                    }
                    else
                    {
                        unit.NextMove = prev_move;
                    }
                }
            }
        }

        /*
         * A* to target
         */
        private void findTarget(GameUnit unit, Vector2 target, Map map, int limit)
        {
            List<Vector2> path = Pathfinder.findPath(map, unit.Position, unit.Target, limit, false);
            // Set the next move to the last node in the path with no obstacles in the way
            if (path != null && path.Count > 1)
            {
                bool found = false;
                for (int i = path.Count - 1; i > 0; i--)
                {
                    if (!map.rayCastHasObstacle(unit.Position, path[i], unit.Size / 3))
                    {
                        unit.NextMove = path[i];
                        unit.Lost = false;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    unit.NextMove = path[1];
                }
            }
            else
            {
                if (unit.Faction == UnitFaction.ALLY &&
                    (Player == null || unit.distance(Player) > LOST_ALLY_DISTANCE))
                {
                    unit.Lost = true;
                }
            }
        }

        /*
         * Returns the closest unit of a different faction from the specified one
         */
        private GameUnit findClosestEnemyInRange(GameUnit unit, int range)
        {
            UnitFaction faction = unit.Faction;

            int x_index = (int)MathHelper.Clamp((unit.Position.X / COMBAT_GRID_CELL_SIZE),
                0, combatRangeGrid.GetLength(1) - 1);
            int y_index = (int)MathHelper.Clamp((unit.Position.Y / COMBAT_GRID_CELL_SIZE),
                0, combatRangeGrid.GetLength(0) - 1);

            // Find the closeset unit according to the combatRangeGrid. Performance optimized.
            GameUnit closest = null;
            double closestDistance = Double.MaxValue;

            List<Point> adjacent = getAdjacent(new Point(x_index, y_index));
            foreach (Point loc in adjacent)
            {
                foreach (GameUnit other in combatRangeGrid[loc.Y, loc.X])
                {
                    //double distance_sq = unit.distance_sq(other);

                    float xdiff = other.Position.X - unit.Position.X;
                    float ydiff = other.Position.Y - unit.Position.Y;
                    double distance_sq = xdiff * xdiff + ydiff * ydiff;

                    if (unit != other && faction != other.Faction && other.Type != UnitType.BOSS && other.Type != UnitType.ORGAN &&
                        distance_sq < (range + unit.Size / 2 + other.Size / 2) * (range + unit.Size / 2 + other.Size / 2) &&
                        (closest == null || distance_sq < closestDistance))
                    {
                        closest = other;
                        closestDistance = distance_sq;
                    }
                }
            }
            return closest;
        }

        /*
         * Returns the closest unit of a specified UnitType and level
         */
        private GameUnit findClosestOfTypeInRange(GameUnit unit, UnitType type, int level, int range)
        {
            int x_index = (int)MathHelper.Clamp((unit.Position.X / COMBAT_GRID_CELL_SIZE),
                0, combatRangeGrid.GetLength(1) - 1);
            int y_index = (int)MathHelper.Clamp((unit.Position.Y / COMBAT_GRID_CELL_SIZE),
                0, combatRangeGrid.GetLength(0) - 1);

            // Find the closeset unit according to the combatRangeGrid. Performance optimized.
            GameUnit closest = null;
            double closestDistance = Double.MaxValue;

            List<Point> adjacent = getAdjacent(new Point(x_index, y_index));
            foreach (Point loc in adjacent)
            {
                foreach (GameUnit other in combatRangeGrid[loc.Y, loc.X])
                {
                    float xdiff = other.Position.X - unit.Position.X;
                    float ydiff = other.Position.Y - unit.Position.Y;
                    double distance_sq = xdiff * xdiff + ydiff * ydiff;

                    if (unit != other && other.Type == type && other.Level >= level &&
                        distance_sq < (range + unit.Size / 2 + other.Size / 2) * (range + unit.Size / 2 + other.Size / 2) &&
                        (closest == null || distance_sq < closestDistance))
                    {
                        closest = other;
                        closestDistance = distance_sq;
                    }
                }
            }
            return closest;
        }

        /*
         * Returns all the adjacent positions from the specified one
         */
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
                if (loc.Y >= 0 && loc.X >= 0 &&
                    loc.Y < combatRangeGrid.GetLength(0) && loc.X < combatRangeGrid.GetLength(1) &&
                        combatRangeGrid[loc.Y, loc.X] != null)
                {
                    adjacent.Add(new Point(pos.X + dir.X, pos.Y + dir.Y));
                }
            }
            return adjacent;
        }
        #endregion

        #region Movement

        /*
         * Sets the velocity of the unit based on its next move
         */
        private void setVelocity(GameUnit unit)
        {
            Vector2 vel = unit.Vel;
            if (unit.HasNextMove())
            {
                // Calculate direction of acceleration
                Vector2 vel_mod = unit.NextMove - unit.Position;

                // If the unit is close enough to target don't move
                if (unit.NextMove == unit.Target &&  vel_mod.Length() < TARGET_STOP_DIST ||
                    unit.Attacking != null && unit.Target == unit.Attacking.Position && // TEMP
                    unit.inRange(unit.Attacking, unit.AttackRange + unit.Size / 2 + unit.Attacking.Size / 2))
                {
                    vel_mod = Vector2.Zero;
                }
                else
                {
                    vel_mod.Normalize();
                    vel_mod *= unit.Accel;
                }

                //TEMP
                if (unit.Faction == UnitFaction.ALLY && Player != null)
                {
                    if ((unit.Position - unit.Target).Length() < 50)
                    {
                        unit.Speed = (int)(Player.Speed * 1.2);
                    }
                    else
                    {
                        unit.Speed = Player.Speed * 2;
                    }
                }

                // Clamp values to max speeds
                vel += vel_mod;
            }

            if (vel.Length() > unit.Speed)
            {
                vel *= unit.Speed / vel.Length();
            }
            unit.Vel = vel;
        }

        /*
         * Move this unit according to its current velocity vector
         */ 
        private void moveUnit(GameUnit unit)
        {
            // Damping
            Vector2 vel = unit.Vel;

            Vector2 vel_mod = vel;
            if (vel_mod.Length() != 0)
            {
                vel_mod.Normalize();
                vel_mod *= unit.Decel;
                vel -= vel_mod;
            }

            // Apply drag
            if ((vel - Vector2.Zero).Length() < unit.Decel * 3/4) { vel = Vector2.Zero; }
            else if (Math.Abs(vel.X) < unit.Decel * 3 / 4) vel.X = 0;
            else if (Math.Abs(vel.Y) < unit.Decel * 3 / 4) vel.Y = 0;
            unit.Vel = vel;
            unit.Position += unit.Vel;

            if (unit.Faction == UnitFaction.ALLY && unit != Player && Player != null)
            {
                unit.Facing = Player.Facing;
            }
            else if(!unit.Directionless)
            {
                Direction dir = Direction.DOWN;
                if (vel_mod.X < -0.1) dir = Direction.LEFT;
                else if (vel_mod.X > 0.1) dir = Direction.RIGHT;
                if (vel_mod.Y < -0.1) dir = Direction.UP;
                else if (vel_mod.Y > 0.1) dir = Direction.DOWN;
                unit.Facing = dir;
            }
        }
        #endregion

        #region Conversion
        /*
         * Converts an enemy to an ally or vice versa, handling stat changes as necessary
         */
        private GameUnit Convert(GameUnit unit)
        {
            if (Player == null) return null;
            if (unit.Faction == UnitFaction.ALLY)
            {
                unit.Faction = UnitFaction.ENEMY;
                Player.NumAllies--;
                if (Player.NumAllies < MAX_ALLIES) { Player.MaxAllies = false; }
            }
            else if (unit.Faction == UnitFaction.ENEMY)
            {
                unit.Faction = UnitFaction.ALLY;
                Player.NumAllies++;
                if (Player.NumAllies >= MAX_ALLIES) { Player.MaxAllies = true; }
            }

            GameUnit ally = AddAlly(unit);
            return ally;
            // Change stats like speed etc as necessary
        }
        #endregion

        #region Combat
        /*
         * Construct the combat grid by placing units in cells where
         * they are in range of other cells
         */
        private void ConstructCombatGrid(Level level)
        {
            combatRangeGrid = new List<GameUnit>[
                level.Height / COMBAT_GRID_CELL_SIZE,
                level.Width / COMBAT_GRID_CELL_SIZE];

            foreach (GameUnit unit in Units)
            {
                int x_index = (int)MathHelper.Clamp((unit.Position.X / COMBAT_GRID_CELL_SIZE),
                    0, combatRangeGrid.GetLength(1) - 1);
                int y_index = (int)MathHelper.Clamp((unit.Position.Y / COMBAT_GRID_CELL_SIZE),
                    0, combatRangeGrid.GetLength(0) - 1);

                if (combatRangeGrid[y_index, x_index] == null)
                {
                    combatRangeGrid[y_index, x_index] = new List<GameUnit>();
                }
                combatRangeGrid[y_index, x_index].Add(unit);
            }

            if (Player != null)
            {
                int x_indexp = (int)MathHelper.Clamp((Player.Position.X / COMBAT_GRID_CELL_SIZE),
                    0, combatRangeGrid.GetLength(1) - 1);
                int y_indexp = (int)MathHelper.Clamp((Player.Position.Y / COMBAT_GRID_CELL_SIZE),
                    0, combatRangeGrid.GetLength(0) - 1);

                if (combatRangeGrid[y_indexp, x_indexp] == null)
                {
                    combatRangeGrid[y_indexp, x_indexp] = new List<GameUnit>();
                }
                combatRangeGrid[y_indexp, x_indexp].Add(Player);
            }
        }

        /*
         * Process combat between two units
         */
        private void ProcessCombat(GameUnit unit)
        {
            if (unit.AttackCoolDown > 0) return;
            if (unit.Attacking != null &&
                unit.inRange(unit.Attacking, unit.AttackRange + unit.Size/2 + unit.Attacking.Size/2))
            {
                Attack(unit, unit.Attacking);
                unit.Attacking = null;
            }

            // Attack other unit. If no other, attack player
            /*
            if(closest != null) {
                Attack(unit, closest);
            }
            else if (Player.Exists && unit.AttackCoolDown == 0 &&
                unit.Faction == UnitFaction.ENEMY && unit.inRange(Player, unit.AttackRange+Player.Size/2))
            {
                Attack(unit, Player);
            }*/
        }

        private void Attack(GameUnit aggressor, GameUnit victim)
        {
            if (victim.Invulnerable) return;

            if(!aggressor.AnimateAttack) aggressor.AttackingNow = true;
            aggressor.AttackCoolDown = aggressor.max_attack_cooldown;
            if (aggressor.Type == UnitType.TANK)
            {
                victim.Health -= Math.Max(aggressor.Attack - victim.Defense, 0);
                if (victim.Type == UnitType.PLAYER)
                {
                    sound_controller.play(SoundType.EFFECT, "ouch");
                }
            }
            else if (aggressor.Type == UnitType.FLYING)
            {
                Color color = Color.White;
                if (aggressor.Faction == UnitFaction.ALLY)
                {
                    color = Color.Green;
                }
                particle_engine.GenerateParticle(1, color, aggressor.Position, victim, aggressor.Faction,
                    false, true, aggressor.Attack, Particle.PROJECTILE_SIZE, 0, 5, 0, 100, 0, Vector2.Zero);
            }
            else if(aggressor.Type == UnitType.BOSS)
            {
                particle_engine.GenerateParticle(10, Color.Yellow, aggressor.Position, null, aggressor.Faction,
                    false, true, aggressor.Attack, Particle.PROJECTILE_SIZE, 0, 5, 0);
            }
        }
        #endregion

        #region UpdateUnit
        private void UpdateUnit(GameUnit unit, Level level)
        {
            if (!unit.Exists) return;
            
            // Increment animation frame
            unit.UpdateAnimation();

            // Infection vitality update
            if (unit.Lost)
            {
                unit.InfectionVitality -= INFECTION_RECOVER_SPEED;
                unit.InfectionVitality = MathHelper.Clamp(
                    unit.InfectionVitality, 0, unit.max_infection_vitality);
            }
            else
            {
                unit.InfectionVitality += INFECTION_RECOVER_SPEED;
                unit.InfectionVitality = MathHelper.Clamp(
                    unit.InfectionVitality, 0, unit.max_infection_vitality);
            }

            // If infection vitality is 0, handle the effect depending on the unit type
            if (unit.InfectionVitality == 0)
            {
                if (unit.Type == UnitType.BOSS)
                {
                    level.BossesDefeated++;
                    unit.Exists = false;
                    particle_engine.GenerateParticle(20, Color.Red, unit.Position, null, UnitFaction.ALLY,
                        false, false, 0, 12, 7, 10, 5);
                }
                else if (unit.Type == UnitType.ORGAN)
                {
                    unit.Exists = false;
                    for (int i = 0; i < ORGAN_ITEM_DROP_NUM; i++)
                    {
                        item_controller.AddRandomItem(unit.Position);
                    }
                }
                else
                {
                    ConvertedUnits.Add(unit);
                }
            }

            if (unit.Type == UnitType.BOSS && unit.AttackCoolDown == 5)
            {
                GameUnit newUnit = factory.createUnit(UnitType.TANK, unit.Faction, unit.Level,
                    unit.Position + new Vector2((float)rand.NextDouble() * 200 - 100, (float)rand.NextDouble() * 200 - 100), true);
                SpawnedUnits.Add(newUnit);
            }

            // Attack cooldown
            unit.AttackCoolDown = (int)MathHelper.Clamp(
                --unit.AttackCoolDown, 0, unit.max_attack_cooldown);

            // Apply ally attrition if they are outside of range
            if (unit.Faction == UnitFaction.ALLY && Player != null && !unit.inRange(Player, ALLY_FOLLOW_RANGE))
            {
                unit.Health -= ALLY_ATTRITION;
            }
            // Check health
            if (unit.Health <= 0 && !unit.AnimateDying)
            {
                unit.Dying = true;
            }
        }

        private void UpdatePreviousPosition(GameUnit unit, Map map)
        {
            if (unit == null) return;
            if (map.canMoveToWorldPos(unit.Position))
            {
                if (PreviousPositions.ContainsKey(unit.ID))
                {
                    PreviousPositions[unit.ID] = unit.Position;
                }
                else
                {
                    PreviousPositions.Add(unit.ID, unit.Position);
                }
            }
        }
        #endregion

        #region Draw
        public void Draw(GameCanvas canvas)
        {
            foreach (GameUnit unit in Units)
            {
                unit.Draw(canvas);
            }
            if (Player != null)
            {
                Player.Draw(canvas);
            }
        }

        #endregion
    }
}
