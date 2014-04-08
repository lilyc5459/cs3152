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

        public Vector2 PlayerStart { get; set; }        // Player starting position tile coordinates
        public List<GameUnit> Bosses { get; set; }      // List of Boss units in this level

        public int NumBosses { get; set; }              // The number of bosses in this level
        public int BossesDefeated { get; set; }         // The number of bosses that the player has defeated

        public Level() { }

        public Level(int width, int height, Texture2D bg_texture, Texture2D wall_texture, List<GameUnit> bosses)
        {
            Width = width;
            Height = height;

            BackgroundTexture = bg_texture;

            Map = new Map(width, height, wall_texture);
            Bosses = bosses;
            NumBosses = bosses.Count;
            BossesDefeated = 0;
        }

        public void Draw(GameCanvas canvas)
        {
            canvas.DrawOverlay(BackgroundTexture, Color.White, Vector2.Zero);
            Map.Draw(canvas);
        }
    }
}
