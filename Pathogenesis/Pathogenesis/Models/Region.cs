using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Pathogenesis.Models
{
    public class Region
    {
        public HashSet<Vector2> RegionSet { get; set; }

        public Vector2 Center { get; set; }

        public List<SpawnPoint> SpawnPoints { get; set; }

        public Region() { }
    }
}
