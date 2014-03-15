using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathogenesis.Models
{
    public class Level
    {
        // Map layout of this level
        public Map Map { get; set; }

        // Dimensions
        public int Width { get; set; }
        public int Height { get; set; }

        public Level()
        {

        }
    }
}
