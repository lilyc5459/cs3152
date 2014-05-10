using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Pathogenesis.Models
{
    public class Level
    {
        // The name of this level
        public String Name { get; set; }

        // Map layout of this level
        public Map Map { get; set; }

        public Texture2D BackgroundTexture { get; set; }
        public Texture2D TitleTexture { get; set; }
        public List<Texture2D> WallTextures { get; set; }

        // Dimensions in pixels
        public int Width { get; set; }
        public int Height { get; set; }

        public Vector2 PlayerStart { get; set; }        // Player starting position tile coordinates
        public List<Region> Regions { get; set; }       // List of the regions in the level
        public List<GameUnit> Bosses { get; set; }      // List of Boss units in this level
        public List<GameUnit> Organs { get; set; }      // List of the infection point organs that drop items

        public bool BossDefeated { get; set; }          // Whether the player has defeate the boss

        public Level() { }

        public Level(int width, int height, Texture2D title_texture, Texture2D bg_texture,
            List<Texture2D> wall_textures)
        {
            Width = width;
            Height = height;

            BackgroundTexture = bg_texture;
            TitleTexture = title_texture;
            WallTextures = wall_textures;

            Map = new Map(width, height, wall_textures);
            Regions = new List<Region>();
            Bosses = new List<GameUnit>();
            Organs = new List<GameUnit>();

            BossDefeated = false;
        }

        /*
         * Create a copy of this level
         */
        public Level Clone()
        {
            Level l = new Level(Width, Height, TitleTexture, BackgroundTexture, WallTextures);
            l.Name = Name;
            l.Map = Map;
            l.PlayerStart = PlayerStart;

            foreach (GameUnit boss in Bosses)
            {
                l.Bosses.Add(boss.Clone());
            }
            foreach (GameUnit organ in Organs)
            {
                l.Organs.Add(organ.Clone());
            }
            foreach (Region region in Regions)
            {
                l.Regions.Add(region.Clone());
            }
            return l;
        }

        /*
         * Draw the level
         */
        public void Draw(GameCanvas canvas)
        {
            // Tile the background if necessary
            int tile_x = (int)(Width / BackgroundTexture.Width) + 1;
            int tile_y = (int)(Height / BackgroundTexture.Height) + 1;
            for (int i = 0; i < tile_x; i++)
            {
                for (int j = 0; j < tile_y; j++)
                {
                    int edge_diff_x = 0;
                    int edge_diff_y = 0;
                    if (i == tile_x)
                    {
                        edge_diff_x = i * BackgroundTexture.Width - Width;
                    }
                    if (j == tile_y)
                    {
                        edge_diff_y = j * BackgroundTexture.Height - Height;
                    }
                    canvas.DrawOverlay(BackgroundTexture, Color.White,
                        new Vector2(i * BackgroundTexture.Width, j * BackgroundTexture.Height),
                        new Rectangle(0, 0, BackgroundTexture.Width - edge_diff_x,
                            BackgroundTexture.Height - edge_diff_y));
                }
            }
            Map.Draw(canvas);
        }

        /*
         * Draw the title screen for this level
         */
        public void DrawTitle(GameCanvas canvas, Vector2 center)
        {
            canvas.DrawSprite(TitleTexture, new Color(90, 0, 0),
                new Rectangle((int)(center.X - canvas.Width/2), (int)(center.Y - canvas.Height/2),
                    canvas.Width, canvas.Height),
                new Rectangle(0, 0, TitleTexture.Width, TitleTexture.Height));
            canvas.DrawText(Name, Menu.fontColor, new Vector2(center.X, center.Y - 100), "font3", true);
        }
    }
}
