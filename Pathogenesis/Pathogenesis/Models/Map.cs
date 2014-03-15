using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathogenesis.Models
{
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

    public class Map
    {
        // Size of the map tiles
        public const int TILE_SIZE = 20;

        // Map dimensions in tiles
        public int Width { get; set; }
        public int Height { get; set; }

        private int[,] tiles; 

        public Map(int width, int height)
        {
            tiles = new int[height, width];
            Width = width;
            Height = height;
        }

        public int getTileAt(int x, int y)
        {
            return tiles[y, x];
        }
    }
}
