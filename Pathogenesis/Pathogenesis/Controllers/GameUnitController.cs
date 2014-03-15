using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Pathogenesis.Models;
using EpPathFinding;

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
        #endregion

        // A list of all the units currently in the game
        public List<GameUnit> Units { get; set; }

        // The player object
        public Player Player { get; set; }

        // Random number generator. Must use the same instance or number generated in quick succession will be the same
        private Random rand;

        #region Initialization
        public GameUnitController()
        {
            Units = new List<GameUnit>();
            rand = new Random();
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
         * Updates all game units
         */
        public void Update(Level level, InputController input_controller)
        {
            // Process player input
            CheckPlayerInput();

            // Set the next move for each unit
            foreach (GameUnit unit in Units)
            {
                setNextMove(unit, level);
            }

            // Apply the next move for each unit
            foreach (GameUnit unit in Units)
            {
                moveUnit(unit);
            }
        }

        private void CheckPlayerInput()
        {

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
            if (Player != null && Player.Exists && Player.inRange(unit, ENEMY_CHASE_RANGE))
            {
                switch (unit.Type)
                {
                    case UnitType.TANK:
                        // tank AI
                        //unit.Target = Player.Position;
                        JumpPointParam jpParam = new JumpPointParam(level.Map,
                            new GridPos((int)unit.Position.X, (int)unit.Position.Y),
                            new GridPos((int)unit.Target.X, (int)unit.Target.Y));
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
            }
            else if(rand.NextDouble() < 0.05)
            {
                // Random walk
                unit.Target = new Vector2(rand.Next(level.Width), rand.Next(level.Height));
            }

            if(unit.HasTarget())
            {
                // Calculate direction of acceleration
                Vector2 vel = unit.Vel;
                float x_mod = unit.Accel * ((unit.Target.X - unit.Position.X) > 0? 1 : -1);
                float y_mod = unit.Accel * ((unit.Target.Y - unit.Position.Y) > 0? 1 : -1);
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
            if (vel.X < 0) vel.X++;
            else if (vel.X > 0) vel.X--;
            if (vel.Y < 0) vel.Y++;
            else if (vel.Y > 0) vel.Y++;
            unit.Vel = vel;
        }

        #endregion
    }
}
