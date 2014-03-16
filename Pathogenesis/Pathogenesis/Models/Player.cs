using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Pathogenesis
{
    public class Player : GameUnit
    {
        public int NumAllies { get; set; }
        public bool MaxAllies { get; set; }

        public Player(Texture2D texture) : base(texture, UnitType.PLAYER, UnitFaction.ALLY)
        {

        }
    }
}
