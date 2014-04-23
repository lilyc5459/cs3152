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
        private List<Texture2D> textures;

        public ParticleEngine(List<Texture2D> textures)
        {
            rand = new Random();
            particles = new List<Particle>();
            this.textures = textures;
        }

        /*
         * Add particles to the engine
         */
        public void GenerateParticle(Vector2 emit_position, GameUnit target)
        {
            int total = 1;

            for (int i = 0; i < total; i++)
            {
                particles.Add(GenerateNewParticle(emit_position, target));
            }
        }

        /*
         * Create a new particle
         */
        private Particle GenerateNewParticle(Vector2 emit_position, GameUnit target)
        {
            Texture2D texture = textures[rand.Next(textures.Count)];

            Vector2 position = new Vector2();
            Vector2 velocity = new Vector2();
            if (target == null)
            {
                // If particle has no target, emit everywhere
                position = emit_position;
                velocity = new Vector2((float)rand.NextDouble() * 20 - 10, (float)rand.NextDouble() * 20 - 10);
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

                // Calculate velocity, from emitter to target, with some spread
                velocity = normal * ((float)rand.NextDouble() * 15 + 5);
                velocity.X += (float)rand.NextDouble() * 20 - 10;
                velocity.Y += (float)rand.NextDouble() * 20 - 10;
            }

            float angle = 0;
            float angularVelocity = 0.1f * (float)(rand.NextDouble() * 2 - 1);
            Color color = new Color(0, 0, 0, (int)(rand.NextDouble() * 150 + 50));
            int size = (int)(rand.NextDouble() * 10 + 10);
            int ttl = 100 + rand.Next(20);

            Particle p = new Particle(texture, position, velocity, angle, angularVelocity, color, size, ttl);
            p.Target = target;
            return p;
        }

        /*
         * Update all particles
         */
        public void UpdateParticles()
        {
            for (int i = 0; i < particles.Count; i++)
            {
                Particle p = particles[i];
                p.TTL--;

                bool remove = false;
                if(p.Target != null)
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

        public void Draw(GameCanvas canvas)
        {
            foreach (Particle p in particles)
            {
                p.Draw(canvas);
            }
        }
    }
}
