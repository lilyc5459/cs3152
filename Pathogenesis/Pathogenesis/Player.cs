using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Pathogenesis
{
    public class Player : GameUnit
    {
        public Player(Texture2D texture) : base(texture, UnitType.PLAYER, UnitFaction.ALLY)
        {

        }
    }
}
