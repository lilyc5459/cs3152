using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathogenesis
{
    public class GameEntity
    {
        #region Properties
        public int ID { get; set; }

        // Positioning
        public Vector2 Position { get; set; }
        public Vector2 Screen_pos { get; set; }

        // Image property
        public Texture2D Texture { get; set; }

        // Indicates if the entity exists in the game. If false, game engine will delete soon
        public bool Exists { get; set; }
        #endregion

        public GameEntity(Texture2D texture)
        {
            Texture = texture;
        }

        // Returns true if the other entity is within the specified range
        public bool inRange(GameEntity other, int range)
        {
            return distance(other) <= range;
        }

        // Calculates the euclidean distance between the entities
        public double distance(GameEntity other)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(other.Position.X - Position.X), 2) +
                             Math.Pow(Math.Abs(other.Position.Y - Position.Y), 2));
        }
    }
}
