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
        public Vector2 Screen_pos { get; set; }

        // Indicates if the entity exists in the game. If false, game engine will delete soon
        public bool Exists { get; set; }

        public bool Ghost { get; set; }
        #endregion

        public GameEntity()
        {
            Exists = true;
        }

        // Returns true if the other entity is within the specified range
        public bool inRange(GameEntity other, int range)
        {
            return distance(other) <= range;
        }

        // Calculates the euclidean distance between the entities
        public double distance(GameEntity other)
        {
            return (other.Position - Position).Length();
        }

        public double distance_sq(GameEntity other)
        {
            float xdiff = other.Position.X - Position.X;
            float ydiff = other.Position.Y - Position.Y;
            return xdiff * xdiff + ydiff * ydiff;
        }
    }
}
