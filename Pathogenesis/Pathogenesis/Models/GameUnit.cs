using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Pathogenesis
{

#region Enums
    /// <summary>
    /// Type of the game unit
    /// </summary>
    /// <remarks>
    /// Different types of units behave with different AI and have different stats.
    /// </remarks>
    public enum UnitType
    {
        TANK,       // High defense, slow
        RANGED,     // High speed, low defense, ranged attack
        FLYING,     // Can fly
        PLAYER,     // Controlled by player, able to use player powers
        BOSS
    };

    /// <summary>
    /// Faction of the game unit
    /// </summary>
    /// <remarks>
    /// Determines which side the unit fights on and who it will attack.
    /// </remarks>
    public enum UnitFaction
    {
        ALLY,       // Fights for the player
        ENEMY      // Fights against player
    };

#endregion

    /// <summary>
    /// GameUnit encapsulates the logic specific to a game unit that moves and fights
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public class GameUnit : GameEntity
    {
        // Constants
        public const int BASE_UNIT_SIZE = 40;
        public const int BASE_HEALTH = 100;
        public const int BASE_INFECTION_VITALITY = 100;
        

        public const int BASE_ATTACK = 5;
        public const int MAX_ATTACK = 5;

        public const int BASE_DEFENSE = 0;
        public const int MAX_DEFENSE = 0;

        public int max_health = 100;
        public int max_infection_vitality = 250;
        #region Properties
        // Textures
        public Texture2D Texture_L { get; set; }
        public Texture2D Texture_R { get; set; }

        public bool Lost { get; set; }
        public bool Immune { get; set; }

        // Unit movement data
        public Vector2 Vel { get; set; }
        public Vector2 Target { get; set; }
        public Vector2 NextMove { get; set; }
        public float Accel { get; set; }
        public float Decel { get; set; }

        public int Size { get; set; }
        public float Mass { get; set; }

        // Unit type data
        public UnitType Type { get; set; }
        public UnitFaction Faction { get; set; }

        // Attacking fields
        public int AttackCoolDown { get; set; }
        public int AttackRange { get; set; }

        // Unit stat fields
        public float Health { get; set; }
        public float InfectionVitality { get; set; }

        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Speed { get; set; }
        public int Range { get; set; }
        public int AttackSpeed { get; set; }

        public int Level { get; set; }
        #endregion

        #region Initialization
        public GameUnit(Texture2D texture_l, Texture2D texture_r, UnitType type,
            UnitFaction faction, int level, bool immune)
        {
            Texture_L = texture_l;
            Texture_R = texture_r;

            Type = type;
            Faction = faction;
            Level = level;
            Immune = immune;

            Target = new Vector2(-1, -1);
            NextMove = new Vector2(-1, -1);

            InitStats();
        }

        private void InitStats()
        {
            // TODO load stats from a config file
            // Test
            Size = (int)(Math.Pow(2, Level - 1) * BASE_UNIT_SIZE);

            max_health = (int)Math.Pow(2, Level-1) * BASE_HEALTH;
            Health = max_health;
            max_infection_vitality = (int)Math.Pow(2, Level - 1) * BASE_INFECTION_VITALITY;
            InfectionVitality = max_infection_vitality;

            AttackCoolDown = 0;
            if (Type == UnitType.PLAYER)
            {
                Speed = 6;
                Accel = 1.1f;
                Decel = 0.5f;
            }
            else if (Faction == UnitFaction.ENEMY)
            {
                Speed = 4;
                Accel = 0.4f;
                Decel = 0.2f;
            }
            else
            {
                Speed = 8;
                Accel = 1.1f;
                Decel = 0.5f;
            }
            Attack = 10 * (Level-1) * BASE_ATTACK + BASE_ATTACK;
            AttackRange = Size/2 + 15;
            Defense = 5 * (Level - 1);
            Mass = 0.5f;
        }
        #endregion

        public bool HasTarget()
        {
            return Target.X >= 0 && Target.Y >= 0;
        }

        public bool HasNextMove()
        {
            return NextMove.X >= 0 && NextMove.Y >= 0;
        }

        public void Draw(GameCanvas canvas)
        {
            // test
            if (Exists)
            {
                Texture2D texture = Vel.X > 0.5 ? Texture_R : Texture_L;
                canvas.DrawSprite(texture,
                    Immune? Color.Gold : new Color(250, InfectionVitality+150, InfectionVitality+150, 250),
                    new Rectangle((int)(Position.X - Size/2), (int)(Position.Y - Size/2), Size, Size),
                    new Rectangle(0, 0, texture.Width, texture.Height));
                canvas.DrawText("T", Color.Yellow, NextMove - new Vector2(20, 20));
            }
        }
    }
}
