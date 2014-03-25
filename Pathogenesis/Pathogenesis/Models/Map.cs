using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathogenesis.Models
{
    #region Enum
    /*
     * Four cardinal directions
     */
    public enum Direction
    {
        NORTH, SOUTH, EAST, WEST
    }

    public enum TileType
    {
        WALL,   // Walls 
        GROUND, // Walkable terrain
        SPAWN,  // Enemy spawn point
        GOAL    // A goal that the player must infect
    }
    #endregion

    public class Map
    {
        // Size of the map tiles
        public const int TILE_SIZE = 40;

        // Map dimensions
        public int Width { get; set; }
        public int Height { get; set; }
        public int WidthTiles
        {
            get
            {
                return Width / TILE_SIZE;
            }
        }
        public int HeightTiles
        {
            get
            {
                return Height / TILE_SIZE;
            }
        }

        // Tile array representation of the map
        private int[,] tiles;

        public Texture2D WallTexture { get; set; }

        public Map(int width, int height, Texture2D wall_texture)
        {
            tiles = new int[height/TILE_SIZE, width/TILE_SIZE];
            Width = width;
            Height = height;

            WallTexture = wall_texture;

            //test
            setTiles(tiles);
        }

        public void setTiles(int[,] tiles)
        {
            this.tiles = tiles;
            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    //Map Borders
                    if (i == 0 || j == 0 || i == tiles.GetLength(0)-1 || j == tiles.GetLength(1)-1)
                    {
                        tiles[i, j] = 1;
                    }

                    //Test
                    if (i == 10 && j > 5 && j < 7)
                    {
                        tiles[i, j] = 1;
                    }

                    if (i == 15 && j > 15 && j < 20)
                    {
                        tiles[i, j] = 1;
                    }

                    if (i == 20 && j > 40 && j < 60)
                    {
                        tiles[i, j] = 1;
                    }

                    if (j == 40 && i > 21 && i < 40)
                    {
                        tiles[i, j] = 1;
                    }

                    if (j == 10 && i > 18 && i < 27)
                    {
                        tiles[i, j] = 1;
                    }

                    if (i == 40 && j > 50 && j < 70)
                    {
                        tiles[i, j] = 1;
                    }

                    if (i == 40 && j > 80 && j < 100)
                    {
                        tiles[i, j] = 1;
                    }

                    if (j == 10 && i > 10 && i < 15)
                    {
                        tiles[i, j] = 1;
                    }

                    if (j == 80 && i > 10 && i < 15)
                    {
                        tiles[i, j] = 1;
                    }

                    if (j == 80 && i > 25 && i < 35)
                    {
                        tiles[i, j] = 1;
                    }


                }
            }
        }

        #region Map Methods
        public int getTileAt(int x, int y)
        {
            if (!canMoveTo(x, y)) return -1;
            return tiles[y, x];
        }

        public Vector2 translateWorldToMap(Vector2 worldPos)
        {
            return worldPos / TILE_SIZE;
        }

        public bool canMoveTo(int x, int y)
        {
            return x >= 0 && y >= 0 && x < tiles.GetLength(1) && y < tiles.GetLength(0) &&
                tiles[y, x] != 1;
        }

        public bool canMoveToWorldPos(Vector2 position)
        {
            Vector2 worldPos = translateWorldToMap(position);
            return canMoveTo((int)worldPos.X, (int)worldPos.Y);
        }

        /*
         * Returns true if obstacle in path
         */
        public bool rayCastHasObstacle(Vector2 start, Vector2 end)
        {
            Vector2 diff = end - start;

            if ((int)start.X/TILE_SIZE == (int)end.X/TILE_SIZE && (int)start.Y/TILE_SIZE == (int)end.Y/TILE_SIZE
                || !canMoveToWorldPos(start) || !canMoveToWorldPos(end) || diff.Length() < Map.TILE_SIZE)
            {
                return false;
            }

            bool steep = Math.Abs(diff.X) <= Math.Abs(diff.Y);
            int x0, x1, y0, y1;

            if (steep)
            {
                x0 = (int)start.Y;
                x1 = (int)end.Y;
                y0 = (int)start.X;
                y1 = (int)end.X;
            }
            else
            {
                x0 = (int)start.X;
                x1 = (int)end.X;
                y0 = (int)start.Y;
                y1 = (int)end.Y;
            }
            if (x0 > x1)
            {
                int t = x1;
                x1 = x0;
                x0 = t;
                t = y1;
                y1 = y0;
                y0 = t;
            }

            float slope = (float)(y1 - y0) / (x1 - x0);
            float b = y0 - slope * x0;

            for (int i = x0; i <= x1; i += TILE_SIZE / 4)
            {
                float x = steep ? slope * i + b : i;
                float y = steep ? i : slope * i + b;
                if (tiles[(int)(y / TILE_SIZE), (int)(x / TILE_SIZE)] == 1)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Draw
        public void Draw(GameCanvas canvas)
        {
            for(int i = 0; i < tiles.GetLength(0); i++) {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    if (tiles[i, j] == 1)
                    {
                        canvas.DrawSprite(WallTexture, Color.White,
                            new Rectangle(j*TILE_SIZE, i*TILE_SIZE, TILE_SIZE, TILE_SIZE),
                            new Rectangle(0, 0, WallTexture.Width, WallTexture.Height));
                    }
                }
            }
        }
        #endregion
    }
}
