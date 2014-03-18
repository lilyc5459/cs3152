using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Pathogenesis.Models;
using EpPathFinding;
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
        public const int ENEMY_CHASE_RANGE = 300;   // Distance at which an enemy will start chasing the player
        public const int INFECT_RANGE = 200;        // Range of the infection ability
        public const int MAX_ALLIES = 100;          // Maximum number of allies allowed
        public const int STOP_DIST = 10;            // Distance at which a unit is considered "at" its target
        public const int ATTACK_COOLDOWN = 50;      // Attack cooldown
        public const int ATTACK_RANGE = 20;         // Attack cooldown
        public const int ENEMY_LOCK_RANGE = 100;     // Distance at which enemies and allies will lock on to each other
        public const int ALLY_FOLLOW_RANGE = 200;
        public const int INFECTION_SPEED = 2;
        #endregion

        private ContentFactory factory;

        // A list of all the units currently in the game
        public List<GameUnit> Units { get; set; }
        public List<GameUnit> DeadUnits { get; set; }

        // The player object
        public Player Player { get; set; }

        // Random number generator. Must use the same instance or number generated in quick succession will be the same
        private Random rand;

        #region Initialization
        public GameUnitController(ContentFactory factory)
        {
            this.factory = factory;
            Units = new List<GameUnit>();
            DeadUnits = new List<GameUnit>();
            rand = new Random();
        }

        /*
         * Add a unit to the game
         */
        public void AddUnit(GameUnit unit)
        {
            unit.ID = Units.Count;
            unit.AttackCoolDown = ATTACK_COOLDOWN;
            Units.Add(unit);
        }
        #endregion

        #region Update
        /*
         * Updates all game units
         */
        public void Update(Level level, InputController input_controller)
        {
            foreach (GameUnit unit in Units)
            {
                setNextMove(unit, level);
            }

            foreach (GameUnit unit in Units)
            {
                moveUnit(unit);
                ProcessCombat(unit);
                unit.InfectionVitality = (int)MathHelper.Clamp(
                    ++unit.InfectionVitality, 0, GameUnit.MAX_INFECTION_VITALITY);
                unit.AttackCoolDown = (int)MathHelper.Clamp(
                    --unit.AttackCoolDown, 0, ATTACK_COOLDOWN);
            }

            foreach (GameUnit unit in DeadUnits)
            {
                if (unit.Type == UnitType.PLAYER) Player.Exists = false;
                else Units.Remove(unit);
            }

            if (Player != null)
            {
                CheckPlayerInput(input_controller);
                moveUnit(Player);
            }
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
            vel.X = MathHelper.Clamp(vel.X, -Player.Speed, Player.Speed);
            vel.Y = MathHelper.Clamp(vel.Y, -Player.Speed, Player.Speed);
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
                if (!Player.Infecting.inRange(Player, INFECT_RANGE))
                {
                    Player.Infecting = null;
                    return;
                }
                if (Player.Infecting.Faction == UnitFaction.ALLY)
                {
                    return;
                }
                if (Player.Infecting.InfectionVitality == 0)
                {
                    Convert(Player.Infecting);
                    Player.NumAllies++;
                    if (Player.NumAllies == MAX_ALLIES) { Player.MaxAllies = true; }
                }
                else
                {
                    Player.Infecting.InfectionVitality -= INFECTION_SPEED;
                }
            }
            else
            {
                // Search for closest enemy within infection range
                GameUnit closestInRange = null;
                foreach (GameUnit unit in Units)
                {
                    if (unit.Faction == UnitFaction.ENEMY && Player.inRange(unit, INFECT_RANGE))
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

        /*
         * Converts an enemy to an ally, handling stat changes as necessary
         */
        private void Convert(GameUnit unit)
        {
            unit.Faction = UnitFaction.ALLY;
            Units.Remove(unit);
            AddUnit(factory.createUnit(unit.Type, UnitFaction.ALLY, unit.Position));
            // Change stats like speed etc as necessary
        }

        #endregion

        #region Movement and Pathfinding

        /*
         * Determine the next move for this unit with
         * targeting specific to each unit type AI
         */ 
        public void setNextMove(GameUnit unit, Level level)
        {
            // Select target
            Vector2 prev_target = unit.Target;
            
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
                        unit.Target = new Vector2(rand.Next(level.Width), rand.Next(level.Height));
                    }

                    if (unit.Faction == UnitFaction.ALLY)
                    {
                        Vector2 vel = Player.Vel;
                        if (Player.Vel.Length() > 0)
                        {
                            //vel.Normalize();
                        }
                        unit.Target = Player.Position + vel * 15;
                    }

                    foreach (GameUnit other in Units)
                    {
                        if (other != unit && other.Faction != unit.Faction && other.inRange(unit, ATTACK_RANGE))
                        {
                            unit.Target = other.Position;
                        }
                    }
                    if(unit.Faction == UnitFaction.ALLY && !Player.inRange(unit, ALLY_FOLLOW_RANGE))
                    {
                        Vector2 vel = Player.Vel;
                        if (Player.Vel.Length() > 0)
                        {
                            //vel.Normalize();
                        }
                        unit.Target = Player.Position + vel * 15;
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
            //&& !unit.Target.Equals(prev_target)
            if (unit.HasTarget() )
            {
                // Pathfind to target if necessary
                if (level.Map.rayCastHasObstacle(unit.Position, unit.Target))
                {
                    List<Vector2> path = Pathfinder.findPath(level.Map, unit.Position, unit.Target);
                    // Set the next move to the last node in the path with no obstacles in the way
                    for(int i = path.Count-1; i > 0; i--) {
                        if (!level.Map.rayCastHasObstacle(unit.Position, path[i]))
                        {
                            unit.NextMove = path[i];
                            break;
                        }
                    }
                }
            }

            if (unit.HasNextMove())
            {
                // Calculate direction of acceleration
                Vector2 vel = unit.Vel;
                float x_mod = unit.Accel * ((unit.NextMove.X - unit.Position.X) > 0? 1 : -1);
                float y_mod = unit.Accel * ((unit.NextMove.Y - unit.Position.Y) > 0? 1 : -1);

                if (Math.Abs(unit.Position.X - unit.NextMove.X) < STOP_DIST) x_mod = 0;
                if (Math.Abs(unit.Position.Y - unit.NextMove.Y) < STOP_DIST) y_mod = 0;

                //TEMP
                if (unit.Faction == UnitFaction.ALLY)
                {
                    if ((unit.Position - unit.Target).Length() < 20)
                    {
                        unit.Speed = (int) (Player.Speed * 1.5);
                    }
                    else
                    {
                        unit.Speed = Player.Speed * 3;
                    }
                }

                vel += new Vector2(x_mod, y_mod);

                // Clamp values to max speeds
                vel.X = MathHelper.Clamp(vel.X, -unit.Speed, unit.Speed);
                vel.Y = MathHelper.Clamp(vel.Y, -unit.Speed, unit.Speed);
                unit.Vel = vel;
            }
        }

        /*
         * Move this unit according to its current velocity vector
         */ 
        private void moveUnit(GameUnit unit)
        {
            unit.Position += unit.Vel;
            //unit.Position = unit.Target; //TESTs
            // Damping
            Vector2 vel = unit.Vel;
            if (vel.X < 0) vel.X += unit.Decel;
            else if (vel.X > 0) vel.X -= unit.Decel;
            if (vel.Y < 0) vel.Y += unit.Decel;
            else if (vel.Y > 0) vel.Y -= unit.Decel;

            if ((vel - Vector2.Zero).Length() < unit.Decel) { vel = Vector2.Zero; }
            unit.Vel = vel;
        }

        #endregion

        #region Combat
        private void ProcessCombat(GameUnit unit)
        {
            if (unit.AttackCoolDown > 0) return;

            // Attack other units
            //TODO make this more efficient by only checking nearby units
            foreach (GameUnit other in Units)
            {
                if (other != unit && other.Faction != unit.Faction && unit.inRange(other, ATTACK_RANGE))
                {
                    Attack(unit, other);
                }
            }
            // Attack player if not attacking other
            if (Player.Exists && unit.AttackCoolDown == 0 &&
                unit.Faction == UnitFaction.ENEMY && unit.inRange(Player, ATTACK_RANGE))
            {
                Attack(unit, Player);
            }
        }

        private void Attack(GameUnit aggressor, GameUnit victim)
        {
            aggressor.AttackCoolDown = ATTACK_COOLDOWN;
            victim.Health -= Math.Max(aggressor.Attack - victim.Defense, 0);
            if (victim.Health <= 0) DeadUnits.Add(victim);
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
