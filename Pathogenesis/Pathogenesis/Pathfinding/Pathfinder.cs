using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.Xna.Framework;

namespace Pathogenesis.Pathfinding
{
    class Pathfinder
    {

        public ArrayList findPath(int[][] map, Vector2 start, Vector2 end)
        {
            MinHeap node_queue = new MinHeap();
            HashSet<Vector2> visited = new HashSet<Vector2>();

            while (node_queue.HasNext())
            {
                node_queue.Add(
            }

        }

        // Calculates a heuristic using euclidean distance
        private double calculateHeuristic(Vector2 start, Vector2 end)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(start.X - end.X), 2) + Math.Pow(Math.Abs(start.Y - end.Y), 2));
        }


    }
}
