using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        PLAYER      // Controlled by player, able to use player powers
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
        private UnitType type;
        private UnitFaction faction;

        // Unit stat fields
        private int attack, defense, speed, range, attack_speed;

        public GameUnit(Texture2D texture, UnitType type, UnitFaction faction) : base(texture)
        {
            this.type = type;
            this.faction = faction;

            InitStats();
        }

        private void InitStats()
        {
            // TODO load stats from a config file
        }
    }
}
