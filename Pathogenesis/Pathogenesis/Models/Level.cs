using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Pathogenesis.Models
{
    public class Level
    {
        // Map layout of this level
        public Map Map { get; set; }

        public Texture2D Texture { get; set; }

        // Dimensions in pixels
        public int Width { get; set; }
        public int Height { get; set; }

        public Level(int width, int height, Texture2D texture)
        {
            Width = width;
            Height = height;

            Texture = texture;

            Map = new Map(width, height);
        }
    }
}
