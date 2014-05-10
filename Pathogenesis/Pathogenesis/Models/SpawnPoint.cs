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
        public const int MILLIS_IN_SECOND = 1000;

        public int Id { get; set; }
        public Vector2 Pos { get; set; }

        // Spawn delay time in seconds
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

            UnitProbabilities = new Dictionary<UnitType, float>();
            LevelProbabilities = new Dictionary<int, float>();
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
            if (stopwatch.ElapsedMilliseconds >= SpawnDelay * MILLIS_IN_SECOND)
            {
                stopwatch.Restart();
                return true;
            }
            return false;
        }
    }
}
