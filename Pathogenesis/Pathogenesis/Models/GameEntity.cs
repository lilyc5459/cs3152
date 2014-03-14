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
        // Properties
        public Vector2 Position { get; set; }
        public Vector2 Screen_pos { get; set; }

        public Texture2D Texture { get; set; }

        public GameEntity(Texture2D texture)
        {
            Texture = texture;
        }
    }
}
