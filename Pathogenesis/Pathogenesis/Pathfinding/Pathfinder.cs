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
        public static Vector2[,] pointLocMap;

        /*
         * Finds the tile based path from start to end, given in world coordinates
         */
        public static List<Vector2> findPath(Map map, Vector2 start, Vector2 end, int limit, bool exploreAll)
        {
            if (!map.canMoveToWorldPos(end))
            {
                return null;
            }

            BinaryHeap<PathNode> node_queue = new BinaryHeap<PathNode>(new NodeCompararer()); // use comparator
            HashSet<PathNode> visited = new HashSet<PathNode>();

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
                PathNode current2 = getMin(node_queue.GetList());

                if (!exploreAll && current.Pos.Equals(endPoint))
                {
                    return constructPath(current);
                }
                if (current.G_score + 1 > limit)    // Don't explore tiles too far
                {
                    continue;
                }
                visited.Add(current);

                List<Point> adj = getAdjacent(current.Pos);
                foreach (Point position in adj)
                {
                    if (visited.Contains(new PathNode(position)) || !map.canMoveTo(position.X, position.Y))
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

            if (!exploreAll) return null;
            
            // Populate the point location map, initialize with (-1, -1)
            pointLocMap = new Vector2[map.HeightTiles, map.WidthTiles];
            for (int i = 0; i < pointLocMap.GetLength(0); i++)
            {
                for (int j = 0; j < pointLocMap.GetLength(1); j++)
                {
                    pointLocMap[i, j] = new Vector2(-1, -1);
                }
            }

            pointLocMap[(int)mapStart.Y, (int)mapStart.X] = mapStart;
            foreach(PathNode node in visited) {
                if (pointLocMap[node.Pos.Y, node.Pos.X] == new Vector2(-1, -1))
                {
                    Vector2 nodePos = new Vector2(node.Pos.X * Map.TILE_SIZE + Map.TILE_SIZE/2,
                        node.Pos.Y * Map.TILE_SIZE + Map.TILE_SIZE/2);
                    if (map.rayCastHasObstacle(nodePos, end, Map.TILE_SIZE/3))
                    {
                        pointLocMap[node.Pos.Y, node.Pos.X] = new Vector2(node.Parent.Pos.X, node.Parent.Pos.Y);
                    }
                    else
                    {
                        pointLocMap[node.Pos.Y, node.Pos.X] = mapEnd;
                    }
                    //collapseBlockedPaths(node, map);
                }
            }

            return null;
        }

        private static PathNode getMin(List<PathNode> list)
        {
            PathNode current = null;
            foreach (PathNode p in list)
            {
                if (current == null || p.Cost < current.Cost)
                {
                    current = p;
                }
            }
            return current;
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

        /*
         * Iterates through positions with obstacles to target of pointLocMap (player position), and
         * points them to the next position without an obstacle.
         * 
         * Vector2 end is given in tile coordinates
         * Assumes the 0,0 tile will be a wall, so it doesn't count as a position
         */
        private static void collapseBlockedPaths(PathNode start, Map map)
        {
            if (start.Parent == null) return;
            if (start.Pos.X == 5 && start.Pos.Y == 4)
            {
                int a = 0;
            }
            Vector2 startPos = new Vector2(start.Pos.X, start.Pos.Y);
            PathNode cur = start;
            Vector2 curPos = new Vector2(cur.Pos.X, cur.Pos.Y);
            Vector2 prevPos = new Vector2(-1, -1);

            List<Vector2> positionsInPath = new List<Vector2>();
            while(!map.rayCastHasObstacle(
                startPos*Map.TILE_SIZE + new Vector2(Map.TILE_SIZE/2, Map.TILE_SIZE/2),
                curPos*Map.TILE_SIZE + new Vector2(Map.TILE_SIZE/2, Map.TILE_SIZE),
                Map.TILE_SIZE/2-5))
            {
                positionsInPath.Add(curPos);
                cur = cur.Parent;
                if (cur == null)
                {
                    break;
                }
                curPos = new Vector2(cur.Pos.X, cur.Pos.Y);
            }
            
            foreach (Vector2 pos in positionsInPath)
            {
                pointLocMap[(int)pos.Y, (int)pos.X] = curPos;
            }
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
