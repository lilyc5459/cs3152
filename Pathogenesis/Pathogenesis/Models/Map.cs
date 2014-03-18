using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EpPathFinding;
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
        public const int TILE_SIZE = 20;

        // Map dimensions in tiles
        public int Width { get; set; }
        public int Height { get; set; }

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
                    //Test
                    if (i == 10 && j > 10 && j < 30)
                    {
                        tiles[i, j] = 1;
                    }
                }
            }
        }

        #region Map Methods
        public int getTileAt(int x, int y)
        {
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
                || !canMoveToWorldPos(start) || !canMoveToWorldPos(end) || diff.Length() < GameUnit.UNIT_SIZE)
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

            for (int i = x0; i <= x1; i += TILE_SIZE / 2)
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
                        canvas.DrawSprite(WallTexture, Color.White, new Vector2(j * TILE_SIZE, i * TILE_SIZE), new Vector2(0.15f, 0.15f), 0f);
                    }
                }
            }
        }
        #endregion
    }
}
