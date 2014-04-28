using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathogenesis.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathogenesis.Controllers
{
    public class ParticleEngine
    {
        private Random rand;

        public Vector2 EmitterPosition { get; set; }
        public List<Particle> particles;
        public List<Particle> DestroyedParticles;
        private List<Texture2D> textures;

        public ParticleEngine(List<Texture2D> textures)
        {
            rand = new Random();
            particles = new List<Particle>();
            DestroyedParticles = new List<Particle>();
            this.textures = textures;
        }

        /*
         * Add particles to the engine
         */
        public void GenerateParticle(int num, Color color, Vector2 emit_position, GameUnit target,
            bool homing, bool isProjectile, int damange, int size, int size_spread, float speed, float speed_spread)
        {
            GenerateParticle(num, color, emit_position, target, homing, isProjectile, 0,
                size, size_spread, speed, speed_spread, 60, 10, Vector2.Zero);
        }

        /*
         * Add particles to the engine with collision normal
         */
        public void GenerateParticle(int num, Color color, Vector2 emit_position, GameUnit target,
            bool homing, bool isProjectile, int damage, int size, int size_spread, float speed, float speed_spread,
            int ttl, int ttl_spread, Vector2 collision_normal)
        {
            for (int i = 0; i < num; i++)
            {
                particles.Add(GenerateNewParticle(color, emit_position, target, homing, isProjectile,
                    damage, size, size_spread, speed, speed_spread, ttl, ttl_spread, collision_normal));
            }
        }

        /*
         * Create a new particle
         */
        private Particle GenerateNewParticle(Color color, Vector2 emit_position, GameUnit target,
            bool homing, bool isProjectile, int damage, int size, int size_spread, float speed, float speed_spread,
            int ttl, int ttl_spread, Vector2 collision_normal)
        {
            Texture2D texture = textures[rand.Next(textures.Count)];

            Vector2 position = new Vector2();
            Vector2 velocity = new Vector2();

            if (target == null)
            {
                // If particle has no target, emit everywhere
                position = emit_position;
                velocity = new Vector2((float)rand.NextDouble() - 0.5f, (float)rand.NextDouble() - 0.5f);
                if (velocity.Length() != 0)
                {
                    velocity.Normalize();
                    velocity *= speed - speed_spread + (float)rand.NextDouble() * speed_spread * 2;
                }
                else
                {
                    velocity = new Vector2(0, speed);
                }
            }
            else
            {
                Vector2 normal = target.Position - emit_position;
                if (normal.Length() != 0)
                {
                    normal.Normalize();
                }

                // Calculate position, slightly in front of emitter
                position = emit_position + normal * 20;

                if (!isProjectile)
                {
                    color *= (float)(rand.NextDouble() * 150 + 100) / 250f;
                }

                // Calculate velocity, from emitter to target, with some spread
                velocity = normal * (speed - speed_spread + (float)rand.NextDouble() * speed_spread * 2);
                velocity.X += (float)rand.NextDouble() * speed_spread * 2 - speed_spread;
                velocity.Y += (float)rand.NextDouble() * speed_spread * 2 - speed_spread;
            }

            if (collision_normal.Length() != 0)
            {
                collision_normal.Normalize();
                velocity = collision_normal * (speed - speed_spread + (float)rand.NextDouble() * speed_spread * 2);
                velocity.X += (float)rand.NextDouble() * speed_spread * 2 - speed_spread;
                velocity.Y += (float)rand.NextDouble() * speed_spread * 2 - speed_spread;
            }

            float angle = 0;
            float angularVelocity = 0.1f * (float)(rand.NextDouble() * 2 - 1);
            size = (int)(size - size_spread + rand.NextDouble() * size_spread * 2);
            ttl = (int)(ttl - ttl_spread + rand.NextDouble() * ttl_spread * 2);

            Particle p = new Particle(texture, position, velocity, angle, angularVelocity,
                color, size, ttl, damage);
            p.Target = target;
            p.Homing = homing;
            p.isProjectile = isProjectile;
            return p;
        }

        /*
         * Update all particles
         */
        public void UpdateParticles()
        {
            // Remove destroyed particles
            foreach(Particle p in DestroyedParticles)
            {
                GenerateParticle(5, p.Color, p.Position, null, false, false, 0,
                    10, 5, 1, 1, 30, 10, p.Target.Position - p.Position);
                particles.Remove(p);
            }
            DestroyedParticles.Clear();

            // Update particle movement
            for (int i = 0; i < particles.Count; i++)
            {
                Particle p = particles[i];
                p.TTL--;

                bool remove = false;
                if(p.Target != null && p.Homing)
                {
                    float length = p.Velocity.Length();

                    Vector2 normal = p.Target.Position - p.Position;
                    float dist = normal.Length();

                    // Remove if close enough to target
                    if (dist < p.Target.Size / 2)
                    {
                        remove = true;
                    }

                    // Update velocity towards target
                    if (dist != 0)
                    {
                        normal.Normalize();
                    }
                    p.Velocity = normal * length;
                }

                p.Position += p.Velocity;
                p.Angle += p.AngularVelocity;
                if (p.TTL <= 0 || remove)
                {
                    particles.RemoveAt(i);
                    i--;
                }
            }
        }

        /*
         * Reset the particle engine
         */
        public void Reset()
        {
            particles.Clear();
        }

        public void Draw(GameCanvas canvas)
        {
            foreach (Particle p in particles)
            {
                p.Draw(canvas);
            }
        }
    }
}
