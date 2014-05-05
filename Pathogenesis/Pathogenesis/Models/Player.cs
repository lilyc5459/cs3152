using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Pathogenesis
{
    public class Player : GameUnit
    {
        // Distance from player that allies will target when player moves in a direction
        private const int ALLY_FRONT_DISTANCE = 100;
        private const int BASE_INFECTION_RANGE = 180;
        private const float BASE_INFECTION_RECOVERY = 1f;

        #region Fields and Properties
        public bool Alive;
        public bool InfectEmpty { get; set; }

        // The position of the player's front in the direction they are going
        public Vector2 Front
        {
            get
            {
                Vector2 front = Vel;
                if (front.Length() != 0)
                {
                    front.Normalize();
                    front *= ALLY_FRONT_DISTANCE;
                }
                return Position + front;
            }
        }
        public int NumAllies { get; set; }
        public bool MaxAllies { get; set; }
        public int MaxInfectionPoints { get; set; }

        public bool AttackPowerActivated { get; set; }

        public GameUnit Infecting { get; set; }     // Unit that the player is currently infecting
        public float InfectionPoints { get; set; }    // Current infection points
        public int InfectionRange { get; set; }     // Player's infection range
        public float InfectionRecovery { get; set; }

        // Items that the player has picked up. Effects will be applied in update player phase
        private List<Item> pickups;
        public List<Item> Items
        {
            get { return pickups; }
            set { pickups = value; }
        }

        // Keeps track of the player's explored tiles
        public int[][] ExploredTiles { get; set; }
        #endregion

        public Player(Texture2D texture_sheet)
            : base(texture_sheet, UnitType.PLAYER, UnitFaction.ALLY, 1, false)
        {
            pickups = new List<Item>();
            InfectionPoints = MaxInfectionPoints;
            Health = 150;
            max_health = 150; //TEMP
            MaxInfectionPoints = 1000;
            InfectionRange = BASE_INFECTION_RANGE;
            InfectionRecovery = BASE_INFECTION_RECOVERY;
            Alive = true;

            ExploredTiles = new int[0][];
        }

        // Adds items to player's pickup list
        public bool PickupItem(Item item)
        {
            switch (item.Type)
            {
                case ItemType.PLASMID:
                    if (InfectionPoints == MaxInfectionPoints) return false;
                    break;
                case ItemType.HEALTH:
                    if (Health == max_health) return false;
                    break;
                case ItemType.ATTACK:
                    break;
            }
            pickups.Add(item);
            return true;
        }
    }
}
