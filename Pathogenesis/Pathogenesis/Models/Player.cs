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
        private const int ALLY_FRONT_DISTANCE = 100;

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

        public GameUnit Infecting { get; set; }

        public Player(Texture2D texture_l, Texture2D texture_r)
            : base(texture_l, texture_r, UnitType.PLAYER, UnitFaction.ALLY, 1, false)
        {
        }
    }
}
