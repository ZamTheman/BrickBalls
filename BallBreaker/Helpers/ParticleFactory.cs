using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BallBreaker.Helpers
{
    public class ParticleFactory
    {
        public int ParticleCount { get { return particles.Count; } }
        public Vector2 Position { get; set; }
        private List<Particle> particles;
        private Random rnd;

        public ParticleFactory()
        {
            particles = new List<Particle>();
            rnd = new Random();
        }

        public void SetupExplosion()
        {
            float startX = Position.X - 32;
            float startY = Position.Y - 32;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    particles.Add(new Particle()
                    {
                        Position = new Vector2(startX + i * 8, startY + j * 8),
                        Alpha = 255,
                        Velocity = GetRandomNormalizedVector2(),
                        Life = 1000
                    });
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                particles[i].Update(gameTime);
                if (particles[i].Alpha < 0)
                    particles.RemoveAt(i);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            foreach (var particle in particles)
            {
                var widthHeight = (int)(8 * particle.Scale);
                spriteBatch.Draw(pixel, new Rectangle((int)particle.Position.X - widthHeight, (int)particle.Position.Y - widthHeight, widthHeight, widthHeight), new Color(27, 50, 95, (int)particle.Alpha));
            }
        }

        private Vector2 GetRandomNormalizedVector2()
        {
            var vector = new Vector2(1, 0);
            var randomValue = rnd.Next(0, 100);
            return Vector2.Transform(vector, Matrix.CreateRotationZ(((float)Math.PI * 2) / 100 * randomValue));
        }

        private class Particle
        {
            public Vector2 Position { get; set; }
            public Vector2 Velocity { get; set; }
            public float Scale { get; set; }
            public double Alpha { get; set; }
            public double Life { get; set; }

            public void Update(GameTime gameTime)
            {
                Life -= gameTime.ElapsedGameTime.TotalMilliseconds;

                Alpha -= (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.5f;

                Scale = (float)Life / 1000;

                var lengthX = EaseIn(Velocity.X * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.5f, (float)Life / 1000);
                var lengthY = EaseIn(Velocity.Y * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.5f, (float)Life / 1000);

                Position += new Vector2(lengthX, lengthY);
            }

            private float EaseIn(float length, float t)
            {
                var factor = t * t * 0.5f;
                return length * factor;
            }
        }
    }
}
