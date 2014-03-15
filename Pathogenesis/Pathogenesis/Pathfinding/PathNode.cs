using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Pathogenesis.Pathfinding
{
    public class PathNode
    {
        public double G_score { get; set; }
        public double Cost { get; set; }

        public Vector2 Pos { get; set; }
        public PathNode next { get; set; }

        public PathNode(Vector2 pos)
        {
            Pos = pos;
        }
    }
}
