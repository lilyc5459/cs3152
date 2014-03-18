using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.Xna.Framework;
using Pathogenesis.Models;

namespace Pathogenesis.Pathfinding
{
    class Pathfinder
    {
        /*
         * Finds the tile based path from start to end, given in world coordinates
         */
        public static List<Vector2> findPath(Map map, Vector2 start, Vector2 end, int limit)
        {
            if (!map.canMoveToWorldPos(end))
            {
                return null;
            }

            BinaryHeap<PathNode> node_queue = new BinaryHeap<PathNode>(new NodeCompararer()); // use comparator
            HashSet<Point> visited = new HashSet<Point>();

            Vector2 mapStart = map.translateWorldToMap(start);
            Vector2 mapEnd = map.translateWorldToMap(end);
            Point startPoint = new Point((int)mapStart.X, (int)mapStart.Y);
            Point endPoint = new Point((int)mapEnd.X, (int)mapEnd.Y);

            PathNode startNode = new PathNode(startPoint);
            startNode.G_score = 0;
            startNode.H_score = calculateHeuristic(startPoint, endPoint);
            node_queue.Insert(startNode);
            while (node_queue.Count > 0)
            {
                PathNode current = node_queue.RemoveRoot(); // O(logn)
                visited.Add(current.Pos);
                if (current.Pos.Equals(endPoint))
                {
                    return constructPath(current);
                }
                if (current.G_score + 1 > limit)    // Don't explore tiles too far
                {
                    continue;
                }

                List<Point> adj = getAdjacent(current.Pos);
                foreach (Point position in adj)
                {
                    if (visited.Contains(position) || !map.canMoveTo(position.X, position.Y))
                    {
                        continue;
                    }
                    PathNode node = nodeAtPosition(node_queue, position);   // O(n)
                    if (node != null && current.G_score + 1 < node.G_score)
                    {
                        node.G_score = current.G_score + 1;
                        node.Parent = current;
                        node_queue.RemoveNode(node);    // O(n)
                        node_queue.Insert(node);        // O(logn)
                    }
                    else if(node == null)
                    {
                        node = new PathNode(position);
                        node.G_score = current.G_score + 1;
                        node.H_score = calculateHeuristic(node.Pos, endPoint);
                        node.Parent = current;
                        node_queue.Insert(node);    // O(logn)
                    }   
                }
            }
            return null;
        }

        // Returns the node, if any, with the specified coordinate position
        private static PathNode nodeAtPosition(BinaryHeap<PathNode> queue, Point position)
        {
            
            foreach (PathNode node in queue.GetList())
            {
                if (node.Pos.Equals(position))
                {
                    return node;
                }
            }

            return null;
        }

        // Construct the path list, given the last pathnode
        private static List<Vector2> constructPath(PathNode last)
        {
            List<Vector2> path = new List<Vector2>();
            PathNode cur = last;
            while (cur != null)
            {
                path.Add(new Vector2(cur.Pos.X * Map.TILE_SIZE + Map.TILE_SIZE/2,
                                     cur.Pos.Y * Map.TILE_SIZE + Map.TILE_SIZE/2));
                cur = cur.Parent;
            }
            path.Reverse();
            return path;
        }

        // Calculates a heuristic using euclidean distance
        private static double calculateHeuristic(Point start, Point end)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(start.X - end.X), 2) + Math.Pow(Math.Abs(start.Y - end.Y), 2));
        }

        // Returns all the adjacent positions from the specified one
        private static List<Point> getAdjacent(Point pos)
        {
            List<Point> adjacent = new List<Point>();
            List<Point> dirs = new List<Point>();
            dirs.Add(new Point(0, 1));
            dirs.Add(new Point(1, 0));
            dirs.Add(new Point(0, -1));
            dirs.Add(new Point(-1, 0));
            foreach (Point dir in dirs)
            {
                adjacent.Add(new Point(pos.X + dir.X, pos.Y + dir.Y));
            }
            return adjacent;
        }

    }
}
