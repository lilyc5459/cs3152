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

        #region Fields and Properties
        public bool Alive;

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

        // Unit that the player is currently infecting
        public GameUnit Infecting { get; set; }
        // Current infection points
        public int InfectionPoints { get; set; }

        // Items that the player has picked up, will be applied in update phase
        private List<Item> pickups;
        public List<Item> Items
        {
            get { return pickups; }
            set { pickups = value; }
        }
        #endregion

        public Player(Texture2D texture_l, Texture2D texture_r, Texture2D texture_u, Texture2D texture_d)
            : base(texture_l, texture_r, texture_u, texture_d, UnitType.PLAYER, UnitFaction.ALLY, 1, false)
        {
            pickups = new List<Item>();
            InfectionPoints = GameUnitController.MAX_PLAYER_CONVERSION_POINTS;

            Alive = true;
        }

        // Adds items to player's pickup list
        public void PickupItem(Item item) {
            pickups.Add(item);
        }
    }
}
