using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Pathogenesis.Models
{
    public class Level
    {
        // Map layout of this level
        public Map Map { get; set; }

        public Texture2D BackgroundTexture { get; set; }

        // Dimensions in pixels
        public int Width { get; set; }
        public int Height { get; set; }

        public Level(int width, int height, Texture2D bg_texture, Texture2D wall_texture)
        {
            Width = width;
            Height = height;

            BackgroundTexture = bg_texture;

            Map = new Map(width, height, wall_texture);
        }

        public void Draw(GameCanvas canvas)
        {
            canvas.DrawOverlay(BackgroundTexture, Color.White, Vector2.Zero);
            Map.Draw(canvas);
        }
    }
}
