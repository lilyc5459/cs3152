using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathogenesis
{
    public class GameEntity
    {
        protected Vector2 pos;
        protected Vector2 screen_pos;

        protected Texture2D texture;

        public GameEntity(Texture2D texture)
        {
            this.texture = texture;
        }

        public void placeAt(Vector2 pos)
        {
            this.pos = pos;
        }
    }
}
