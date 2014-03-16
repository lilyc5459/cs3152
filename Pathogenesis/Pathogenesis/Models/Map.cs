using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EpPathFinding;

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

    public class Map : StaticGrid
    {
        // Size of the map tiles
        public const int TILE_SIZE = 20;

        // Map dimensions in tiles
        public int Width { get; set; }
        public int Height { get; set; }

        private int[,] tiles;

        public Map(int width, int height) : base(width, height)
        {
            tiles = new int[height/TILE_SIZE, width/TILE_SIZE];
            Width = width;
            Height = height;

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
                    if (tiles[i, j] != 1)
                    {
                        SetWalkableAt(new GridPos(j, i), true);
                    }
                }
            }
        }

        public int getTileAt(int x, int y)
        {
            return tiles[y, x];
        }
    }
}
