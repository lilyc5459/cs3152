using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using System.Diagnostics;

namespace Pathogenesis.Models
{
    public class SpawnPoint
    {
        public Vector2 Pos { get; set; }

        public int SpawnDelay { get; set; }

        private Stopwatch stopwatch;

        [XmlIgnoreAttribute]
        public Dictionary<UnitType, float> UnitProbabilities { get; set; }
        [XmlIgnoreAttribute]
        public Dictionary<int, float> LevelProbabilities { get; set; }

        public SpawnPoint()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public SpawnPoint(Vector2 pos, int spawndelay)
        {
            Pos = pos;
            SpawnDelay = spawndelay;

            UnitProbabilities = new Dictionary<UnitType, float>();
            LevelProbabilities = new Dictionary<int, float>();

            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public bool ShouldSpawn()
        {
            if (stopwatch.ElapsedMilliseconds >= SpawnDelay)
            {
                stopwatch.Restart();
                return true;
            }
            return false;
        }

        public void Activate()
        {
            stopwatch.Start();
        }
    }
}
