using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Pathogenesis.Models
{
    public class Particle
    {
        public const int PROJECTILE_SIZE = 20;

        public Texture2D Texture { get; set; }        // The texture that will be drawn to represent the particle
        public Vector2 Position { get; set; }        // The current position of the particle        
        public Vector2 Velocity { get; set; }        // The speed of the particle at the current instance
        public float Angle { get; set; }            // The current angle of rotation of the particle
        public float AngularVelocity { get; set; }    // The speed that the angle is changing
        public Color Color { get; set; }            // The color of the particle
        public int Size { get; set; }                // The size of the particle
        public int MaxTTL { get; set; }             // Time to live of the particle
        public int TTL { get; set; }                // Time left to live

        public GameUnit Target { get; set; }        // Target unit of the particle
        public UnitFaction Faction { get; set; }    // Faction of the particle

        public bool Homing { get; set; }
        public bool isProjectile { get; set; }
        public int Damage { get; set; }

        public Particle(Texture2D texture, Vector2 position, Vector2 velocity,
            float angle, float angularVelocity, Color color, int size, int ttl, int damage)
        {
            Texture = texture;
            Position = position;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angularVelocity;
            Color = color;
            Size = size;
            MaxTTL = ttl;
            TTL = ttl;
            Damage = damage;
        }

        public void Draw(GameCanvas canvas)
        {
            canvas.DrawSprite(Texture, Color * (MathHelper.Lerp(0, Color.A, (float)TTL/MaxTTL)/250),
                new Rectangle((int)Position.X, (int)Position.Y, Size, Size),
                new Rectangle(0, 0, Texture.Width, Texture.Height));
            /*
            spriteBatch.Draw(Texture, Position, sourceRectangle, Color,
                Angle, origin, Size, SpriteEffects.None, 0f);
             * */
        }
    }
}
