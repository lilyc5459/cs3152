using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathogenesis.Models;

namespace Pathogenesis
{
    public class GameEntity
    {
        #region Properties
        public static int CurEntityID = 0;
        public int ID { get; set; }

        // Positioning
        public Vector2 Position { get; set; }
        public Vector2 TilePosition
        {
            get
            {
                return new Vector2((int)Position.X / Map.TILE_SIZE, (int)Position.Y / Map.TILE_SIZE);
            }
        }

        // Movement
        public Vector2 Vel { get; set; }
        public float Accel { get; set; }
        public float Decel { get; set; }

        // Animation
        public int Frame { get; set; }
        public int NumFrames { get; set; }
        public Vector2 FrameSize { get; set; }
        public int FrameSpeed { get; set; }
        public int FrameTimeCounter { get; set; }

        public bool Exists { get; set; }    // Indicates if the entity exists in the game
        public bool Static { get; set; }    // Indicates if the entity is immovable

        public bool Ghost { get; set; }
        #endregion

        public GameEntity()
        {
            Exists = true;
            ID = CurEntityID++;
        }

        // Returns true if the other entity is within the specified range
        public bool inRange(GameEntity other, int range)
        {
            return distance(other) <= range;
        }

        // Calculates the euclidean distance between the entities
        public double distance(GameEntity other)
        {
            if (other == null) return -1;
            return (other.Position - Position).Length();
        }

        // Calculates the euclidean distance between this entity and a point
        public double distance(Vector2 pos)
        {
            return (pos - Position).Length();
        }

        public double distance_sq(GameEntity other)
        {
            float xdiff = other.Position.X - Position.X;
            float ydiff = other.Position.Y - Position.Y;
            return xdiff * xdiff + ydiff * ydiff;
        }
    }
}
