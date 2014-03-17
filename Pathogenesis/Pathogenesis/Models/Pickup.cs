using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Pathogenesis
{
    public class Pickup : GameEntity
    {
        public const int ITEM_SIZE = 20;

        public Pickup(Texture2D texture) : base(texture)
        {

        }

        public void Draw(GameCanvas canvas)
        {
            canvas.DrawSprite(Texture, Color.White,
                new Rectangle((int)Position.X, (int)Position.Y, ITEM_SIZE, ITEM_SIZE),
                new Rectangle(0, 0, Texture.Width, Texture.Height));
        }
    }
}
