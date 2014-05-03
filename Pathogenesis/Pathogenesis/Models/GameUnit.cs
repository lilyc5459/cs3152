using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathogenesis.Models;


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
        BOSS,
        ORGAN,      // Infection point that will drop items
        SPAWN_ORGAN // Spawn point for enemies, and also a health leech infection point
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

    // Facing Direction
    public enum Direction
    {
        DOWN,
        UP,
        LEFT,
        RIGHT,
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

        public const int BASE_ATTACK = 10;
        public const int MAX_ATTACK = 10;

        public const int BASE_DEFENSE = 0;
        public const int MAX_DEFENSE = 0;

        public int max_health = 100;
        public int max_infection_vitality = 250;
        public int max_attack_cooldown = 50;

        #region Properties
        // Textures
        public Texture2D Texture { get; set; }

        public bool Lost { get; set; }
        public bool Immune { get; set; }
        public bool Invulnerable { get; set; }

        // Unit movement data
        public Direction Facing { get; set; }
        public Vector2 Target { get; set; }
        public Vector2 NextMove { get; set; }

        public int Size { get; set; }
        public float Mass { get; set; }

        // Unit type data
        public UnitType Type { get; set; }
        public UnitFaction Faction { get; set; }

        // Attacking fields
        public int AttackCoolDown { get; set; }
        public int AttackRange { get; set; }
        public int AttackLockRange { get; set; }
        public GameUnit Attacking { get; set; }     // currently targetting this unit

        // Unit stat fields
        public float Health { get; set; }
        public float InfectionVitality { get; set; }

        public int Attack { get; set; }
        public int Defense { get; set; }
        public float Speed { get; set; }
        public int Range { get; set; }
        public int AttackSpeed { get; set; }

        public int Level { get; set; }

        public Region Region { get; set; }      // The region that this unit is bound to
        #endregion

        #region Initialization
        public GameUnit() { }

        public GameUnit(Texture2D texture, UnitType type, UnitFaction faction,
            int level, bool immune)
        {
            Texture = texture;

            Type = type;
            Faction = faction;
            Level = level;
            Immune = immune;

            Target = new Vector2(-1, -1);
            NextMove = new Vector2(-1, -1);

            Initialize();
        }

        /*
         * Initializes all stats
         */
        private void Initialize()
        {
            // TODO load all stats from a config file
            // Test
            Size = (int)(Math.Pow(2, Level - 1) * BASE_UNIT_SIZE);

            max_health = (int)Math.Pow(2, Level-1) * BASE_HEALTH;
            Health = max_health;

            max_infection_vitality = (int)Math.Pow(3, Level - 1) * BASE_INFECTION_VITALITY;
            if (Type == UnitType.BOSS)
            {
                max_infection_vitality = ((Level + 1) * 3) * BASE_INFECTION_VITALITY;
            }
            if (Type == UnitType.ORGAN)
            {
                max_infection_vitality = (Level + 2) * BASE_INFECTION_VITALITY;
            }
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
            Attack = 5 * (Level-1) * BASE_ATTACK + BASE_ATTACK;
            AttackRange = 15;
            AttackLockRange = 30;
            //Defense = 5 * (Level - 1);
            Mass = 0.1f;

            if (Level == 2)
            {
                Mass = 5f;
                Speed *= 2.0f / 3;
            }

            if (Type == UnitType.FLYING)
            {
                AttackRange = 200;
                AttackLockRange = 300;
            }
            if (Type == UnitType.BOSS)
            {
                Size = 100;
                Speed = 3;
                Mass = 100f;
                Attack = 50;
                max_attack_cooldown = 200;
                AttackRange = 200;
                Invulnerable = true;
            }
            if (Type == UnitType.ORGAN)
            {
                Size = 100;
                Mass = 10f;
                AnimateResting = true;
                Directionless = true;
                Invulnerable = true;
            }
        }
        #endregion

        #region Update Functions
        // Increments animation frame according to the specified speed
        public void UpdateAnimation()
        {
            if (NumFrames > 0 && (Vel.Length() > 0 || Vel.Length() == 0 && AnimateResting))
            {
                FrameTimeCounter++;
                if (FrameTimeCounter >= FrameSpeed)
                {
                    Frame = (Frame + 1) % NumFrames;
                    FrameTimeCounter = 0;
                }
            }
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
            Texture2D texture = Texture;
            int facingFrame = 0;

            if (FrameSpeed > 0)
            {
                switch (Facing)
                {
                    case Direction.RIGHT:
                        facingFrame = 3;
                        break;
                    case Direction.LEFT:
                        facingFrame = 2;
                        break;
                    case Direction.UP:
                        facingFrame = 1;
                        break;
                    case Direction.DOWN:
                        facingFrame = 0;
                        break;
                }
            }

            Color color = Color.Lerp(Color.Red, Color.White, (Health+10) / max_health);
            if (Immune)
            {
                color = Color.Lerp(Color.Red, Color.Blue, (Health+10) /max_health);
            }
            if (Faction == UnitFaction.ALLY)    //TEMP tell them to make color darker
            {
                color = Color.Lerp(Color.Red, Color.LightGray, (Health + 10) / max_health);
            }
            if (!Exists)
            {
                color = Color.Gray;
            }

            //TEMP
            if (NumFrames > 1)
            {
                canvas.DrawSprite(texture, color,
                    new Rectangle((int)(Position.X - FrameSize.X / 2), (int)(Position.Y - FrameSize.Y / 2),
                        (int)FrameSize.X, (int)FrameSize.Y),
                    new Rectangle(Frame * (int)FrameSize.X + 1, facingFrame * (int)FrameSize.Y + 1, (int)FrameSize.X-2, (int)FrameSize.Y-2));
            }
            else
            {
                // get rid of this later when everything has animation frames
                canvas.DrawSprite(texture, color,
                    new Rectangle((int)(Position.X - texture.Width / 2), (int)(Position.Y - texture.Height / 2),
                        texture.Width, texture.Height),
                    new Rectangle(0, 0, texture.Width, texture.Height));
            }
            //canvas.DrawText("T", Color.Yellow, NextMove - new Vector2(20, 20), "font1", false);
        }
    }
}
