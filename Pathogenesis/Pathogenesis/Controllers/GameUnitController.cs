using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Pathogenesis.Models;
using Pathogenesis.Pathfinding;

namespace Pathogenesis
{
    /*
     * Controls all unit logic and management,
     * including movement, pathfinding, AI, and updating
     */ 
    public class GameUnitController
    {
        #region Constants
        public const int PLAYER_PATHFIND_FIELD_SIZE = 15;
        public const int MAX_PLAYER_CONVERSION_POINTS = 1000; // Maximum conversion points for the player
        public const int MAX_ASTAR_DIST = 15;
        public const int LOST_ALLY_DISTANCE = 500;

        public const int ENEMY_CHASE_RANGE = 250;   // Distance at which an enemy will start chasing the player
        public const int INFECT_RANGE = 200;        // Range of the infection ability
        public const int MAX_ALLIES = 100;          // Maximum number of allies allowed
        public const int TARGET_STOP_DIST = 50;     // Distance at which a unit is considered "at" its target
        public const int MOVE_STOP_DIST = 5;
        public const int ATTACK_COOLDOWN = 50;      // Attack cooldown
        public const int ATTACK_LOCK_RANGE = 70;    // Distance at which enemies and allies will lock on to each other
        public const int ALLY_FOLLOW_RANGE = 200;
        public const int INFECTION_SPEED = 3;
        public const float INFECTION_RECOVER_SPEED = 0.5f;
        public const float ALLY_ATTRITION = 0.0f;

        public const int PLASMID_POINTS = 120;      // Conversion points gained from picking up a plasmid
        public const int HEALTH_POINTS = 100;       // Health points gained from picking up a health item
        #endregion

        private ContentFactory factory;

        // A list of all the units currently in the game
        public List<GameUnit> Units { get; set; }
        public List<GameUnit> DeadUnits { get; set; }
        public List<GameUnit> ConvertedUnits { get; set; }

        // The player object
        public Player Player { get; set; }
        private Vector2[,] playerLocationField;
        private List<GameUnit> lostUnits;

        // Random number generator. Must use the same instance or number generated in quick succession will be the same
        private Random rand;

        #region Initialization
        public GameUnitController(ContentFactory factory)
        {
            this.factory = factory;
            Units = new List<GameUnit>();
            DeadUnits = new List<GameUnit>();
            ConvertedUnits = new List<GameUnit>();
            lostUnits = new List<GameUnit>();

            rand = new Random();
        }

        public void Reset()
        {
            Units.Clear();
            DeadUnits.Clear();
            ConvertedUnits.Clear();
        }

        /*
         * Add a unit to the game
         */
        public void AddUnit(GameUnit unit)
        {
            unit.ID = Units.Count;
            Units.Add(unit);
        }
        #endregion

        #region Update
        /*
         * Update all units
         */
        public void Update(Level level, InputController input_controller)
        {
            //Pathfinder.findPath(level.Map, Player.Front, Player.Front, PLAYER_PATHFIND_FIELD_SIZE, true);
            //playerLocationField = Pathfinder.pointLocMap;
            //lostUnits.Clear();

            // Set next moves
            foreach (GameUnit unit in Units)
            {
                setNextMove(unit, level);
            }

            // Handle lost units
            if (lostUnits.Count != 0)
            {
                GameUnit captain = lostUnits.First();
                captain.Target = Player.Position;
                //findTarget(captain, Player.Position, level.Map, 100);
                foreach (GameUnit unit in lostUnits)
                {
                    if (unit != captain)
                    {
                        unit.Lost = true;
                        unit.Target = captain.Position;
                        unit.NextMove = unit.Target;
                    }
                }
            }

            // Execute moves and actions
            foreach (GameUnit unit in Units)
            {
                setVelocity(unit);
                moveUnit(unit);
                ProcessCombat(unit);
                UpdateUnit(unit);
            }

            // Handle player input
            if (Player != null && Player.Exists)
            {
                CheckPlayerInput(input_controller);
                moveUnit(Player);
                UpdatePlayer();
            }

            // Convert units
            foreach (GameUnit unit in ConvertedUnits)
            {
                Convert(unit);
            }
            ConvertedUnits.Clear();

            // Dispose of dead units
            foreach (GameUnit unit in DeadUnits)
            {
                if (unit.Type == UnitType.PLAYER) Player.Exists = false;
                else Units.Remove(unit);
            }
            DeadUnits.Clear();
        }

        #endregion

        #region Player logic
        /*
         * Process player input and apply to player object
         */
        private void CheckPlayerInput(InputController input_controller)
        {
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
            if (input_controller.Converting)
            {
                PlayerInfect();
            }
            else
            {
                Player.Infecting = null;
            }
        }

        /*
         * Searches for the closest enemy within infection range and converts it to an ally
         */
        private void PlayerInfect()
        {
            if (Player.Infecting != null)
            {
                if (Player.InfectionPoints > 0 && Player.Infecting.InfectionVitality > 0 && 
                    Player.Infecting.inRange(Player, INFECT_RANGE))
                {
                    Player.Infecting.InfectionVitality -= INFECTION_SPEED;

                    Player.InfectionPoints -= (int)INFECTION_SPEED;
                    Player.InfectionPoints = (int)MathHelper.Clamp(Player.InfectionPoints,
                        0, MAX_PLAYER_CONVERSION_POINTS);
                }
                else
                {
                    Player.Infecting = null;                    
                }
            }
            else
            {
                // Search for closest enemy within infection range
                GameUnit closestInRange = null;
                foreach (GameUnit unit in Units)
                {
                    if (!unit.Immune && unit.Faction == UnitFaction.ENEMY && Player.inRange(unit, INFECT_RANGE))
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
        }

        private void UpdatePlayer()
        {
            if (Player.Health <= 0) DeadUnits.Add(Player);

            foreach(Item item in Player.Items)
            {
                switch (item.Type)
                {
                    case ItemType.PLASMID:
                        Player.InfectionPoints += PLASMID_POINTS;
                        Player.InfectionPoints = (int)MathHelper.Clamp(Player.InfectionPoints,
                            0, MAX_PLAYER_CONVERSION_POINTS);
                        break;
                    case ItemType.HEALTH:
                        Player.Health += HEALTH_POINTS;
                        Player.Health = MathHelper.Clamp(Player.Health, 0, Player.max_health);
                        break;
                    case ItemType.ATTACK:
                        break;
                }
            }
            Player.Items = new List<Item>();
        }
        #endregion

        #region Movement and Pathfinding

        /*
         * Determine the next move for this unit with
         * targeting specific to each unit type AI
         */ 
        public void setNextMove(GameUnit unit, Level level)
        {
            if (unit.Position.X < 0 || unit.Position.Y < 0) return;

            // Select target
            Vector2 prev_move = unit.NextMove;
            
            if (unit.Faction == UnitFaction.ALLY && unit.Lost)
            {
                if (rand.NextDouble() < 0.01)
                {
                    unit.Target = Player.Position;
                    findTarget(unit, Player.Position, level.Map, MAX_ASTAR_DIST);
                }
                else if(rand.NextDouble() < 0.05)
                {
                    unit.Target = unit.Position + new Vector2(rand.Next(600) - 300, rand.Next(600) - 300);
                    unit.NextMove = unit.Target;
                }
                return;
            }

            switch (unit.Type)
            {
                case UnitType.TANK:
                    // tank AI
                    if (Player != null && Player.Exists &&
                        unit.Faction == UnitFaction.ENEMY && Player.inRange(unit, ENEMY_CHASE_RANGE))
                    {
                        unit.Target = Player.Position;
                    }
                    else if (rand.NextDouble() < 0.05)
                    {
                        // Random walk
                        unit.Target = unit.Position + new Vector2(rand.Next(600)-300, rand.Next(600)-300);
                    }

                    if (unit.Faction == UnitFaction.ALLY)
                    {
                        if (level.Map.rayCastHasObstacle(Player.Position, Player.Front, Map.TILE_SIZE / 3))
                        {
                            unit.Target = Player.Position;
                        }
                        else
                        {
                            unit.Target = Player.Front;
                        }
                    }
                    foreach (GameUnit other in Units)
                    {
                        if (other != unit && other.Faction != unit.Faction && other.inRange(unit, ATTACK_LOCK_RANGE))
                        {
                            unit.Target = other.Position;
                            break;
                        }
                    }
                    if(unit.Faction == UnitFaction.ALLY && !Player.inRange(unit, ALLY_FOLLOW_RANGE))
                    {
                        unit.Target = Player.Position;
                    }
                    break;
                case UnitType.RANGED:
                    // ranged AI
                    break;
                case UnitType.FLYING:
                    // flying AI
                    break;
                default:
                    // Player case, do nothing
                    break;
            }

            unit.NextMove = unit.Target;
            if (unit.HasTarget())
            {
                /*
                // If the target is the player, use the player location map
                if (unit.Target.Equals(Player.Position) &&
                    Math.Abs(unit.TilePosition.X - Player.TilePosition.X) +
                    Math.Abs(unit.TilePosition.Y - Player.TilePosition.Y) < PLAYER_PATHFIND_FIELD_SIZE)
                {
                    Vector2 moveToPlayerTile = findMoveToPlayer(unit, level.Map);
                    unit.NextMove = new Vector2(moveToPlayerTile.X * Map.TILE_SIZE,
                        moveToPlayerTile.Y * Map.TILE_SIZE);
                }
                 * */

                // Pathfind to target if necessary
                if (level.Map.rayCastHasObstacle(unit.Position, unit.Target, unit.Size/2))
                {
                    findTarget(unit, unit.Target, level.Map, MAX_ASTAR_DIST);
                }
            }
        }

        /*
         * Sets the velocity of the unit based on its next move
         */
        private void setVelocity(GameUnit unit)
        {
            if (unit.HasNextMove())
            {
                // Calculate direction of acceleration
                Vector2 vel = unit.Vel;
                Vector2 vel_mod = unit.NextMove - unit.Position;

                // If the unit is close enough to target don't move
                if (unit.NextMove == unit.Target && vel_mod.Length() < TARGET_STOP_DIST)
                {
                    vel_mod = Vector2.Zero;
                }
                else
                {
                    vel_mod.Normalize();
                    vel_mod *= unit.Accel;
                }

                //TEMP
                if (unit.Faction == UnitFaction.ALLY)
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

                if (vel.Length() > unit.Speed)
                {
                    vel.Normalize();
                    vel *= unit.Speed;
                }
                unit.Vel = vel;
            }
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
        }

        /*
         * A* to target
         */
        private void findTarget(GameUnit unit, Vector2 target, Map map, int limit)
        {
            List<Vector2> path = Pathfinder.findPath(map, unit.Position, unit.Target, limit, false);
            // Set the next move to the last node in the path with no obstacles in the way
            if (path != null)
            {
                for (int i = path.Count - 1; i > 0; i--)
                {
                    if (!map.rayCastHasObstacle(unit.Position, path[i], unit.Size / 2))
                    {
                        unit.NextMove = path[i];
                        unit.Lost = false;
                        return;
                    }
                }
            }
            else if (unit.Faction == UnitFaction.ALLY)
            {
                unit.Lost = true;
            }
        }

        /*
         * Use point location field to find the player (NOT IN USE)
         */
        private Vector2 findMoveToPlayer(GameUnit unit, Map map)
        {
            return playerLocationField[(int)unit.Position.Y / Map.TILE_SIZE, (int)unit.Position.X / Map.TILE_SIZE];
        }

        #endregion

        #region Conversion
        /*
         * Converts an enemy to an ally or vice versa, handling stat changes as necessary
         */
        private void Convert(GameUnit unit)
        {
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

            Units.Remove(unit);
            AddUnit(factory.createUnit(unit.Type, unit.Faction, unit.Level, unit.Position, unit.Immune));
            // Change stats like speed etc as necessary
        }
        #endregion

        #region Combat
        private void ProcessCombat(GameUnit unit)
        {
            if (unit.AttackCoolDown > 0) return;

            // Attack other units
            //TODO make this more efficient by only checking nearby units
            GameUnit closest = null;
            foreach (GameUnit other in Units)
            {
                // Change to use hitboxes so we don't have to use other.Size/2
                if (other != unit && other.Faction != unit.Faction && unit.inRange(other, unit.AttackRange+other.Size/2) &&
                    (closest == null || unit.distance(other) < unit.distance(closest)))
                {
                    closest = other;
                }
            }

            // Attack other unit. If no other, attack player
            if(closest != null) {
                Attack(unit, closest);
            }
            else if (Player.Exists && unit.AttackCoolDown == 0 &&
                unit.Faction == UnitFaction.ENEMY && unit.inRange(Player, unit.AttackRange+Player.Size/2))
            {
                Attack(unit, Player);
            }
        }

        private void Attack(GameUnit aggressor, GameUnit victim)
        {
            aggressor.AttackCoolDown = ATTACK_COOLDOWN;
            victim.Health -= Math.Max(aggressor.Attack - victim.Defense, 0);
        }
        #endregion

        #region UpdateUnit
        private void UpdateUnit(GameUnit unit)
        {
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
            if (unit.InfectionVitality == 0)
            {
                ConvertedUnits.Add(unit);
            }

            // Attack cooldown
            unit.AttackCoolDown = (int)MathHelper.Clamp(
                --unit.AttackCoolDown, 0, ATTACK_COOLDOWN);

            // Apply ally attrition if they are outside of range
            if (unit.Faction == UnitFaction.ALLY && !unit.inRange(Player, ALLY_FOLLOW_RANGE))
            {
                unit.Health -= ALLY_ATTRITION;
            }
            // Check health
            if (unit.Health <= 0)
            {
                DeadUnits.Add(unit);
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
