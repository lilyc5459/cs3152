using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;

namespace Pathogenesis.Models
{
    public class SpawnPoint
    {
        public Vector2 Pos { get; set; }

        [XmlIgnoreAttribute]
        public Dictionary<UnitType, float> UnitProbabilities { get; set; }
        [XmlIgnoreAttribute]
        public Dictionary<int, float> LevelProbabilities { get; set; }

        public SpawnPoint() { }

        public SpawnPoint(Vector2 pos)
        {
            Pos = pos;
        }
    }
}
