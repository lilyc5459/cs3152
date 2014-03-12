using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Pathogenesis
{
    public class GameEntity
    {
        protected int x, y;
        protected int screen_x, screen_y;

        protected Texture2D texture;

        public GameEntity(Texture2D texture)
        {
            this.texture = texture;
        }

        public void placeAt(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
