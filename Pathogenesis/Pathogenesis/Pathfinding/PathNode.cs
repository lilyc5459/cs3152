using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Collections;

namespace Pathogenesis.Pathfinding
{
    public class PathNode
    {
        public double G_score { get; set; }
        public double H_score { get; set; }
        public double Cost
        {
            get
            {
                return G_score + H_score;
            }
        }

        public Point Pos { get; set; }
        public PathNode Parent { get; set; }

        public PathNode(Point pos)
        {
            Pos = new Point((int)pos.X, (int)pos.Y);
        }
    }

    public class NodeCompararer : IComparer<PathNode>
    {
        int IComparer<PathNode>.Compare(PathNode a, PathNode b)
        {
            if (a.Cost > b.Cost)
            {
                return 1;
            }
            else if (a.Cost < b.Cost)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}
