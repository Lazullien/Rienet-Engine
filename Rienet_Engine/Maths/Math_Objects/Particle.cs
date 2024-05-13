using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    /// <summary>
    /// Basically a hyper simplified version of entity
    /// </summary>
    public class Particle : IBodyVenue
    {
        //These two pos and vel are same as body if a body exists
        public Vector2 Pos { get; set; }
        public Vector2 PixelPos { get; private set; }
        public Vector2 Vel { get; set; }
        /// <summary>
        /// accerleration
        /// </summary>
        public Vector2 Acc { get; set; }
        public float Orientation { get; set; }
        public float VelAng { get; set; }
        public float Time { get; set; }
        public float Duration { get; set; } = 5;
        public float Opacity { get; set; } = 1;
        public Color Color { get; set; } = Color.White;
        public float Scale { get; set; } = 1;
        public int Layer { get; set; } = 0;
        public GraphicsComponent Graphics { get; private set; }
        public bool Finished { get; set; }
        /// <summary>
        /// has only one hitbox
        /// </summary>
        public bool HasBody { get; private set; }
        public PhysicsBody Body { get; private set; }
        /// <summary>
        /// To help with allocation, if a camera is in this scene
        /// </summary>
        public Scene BelongedScene { get; set; }
        public Action ActionOnUpdate { get; set; } = delegate { };
        public Action ActionOnCollision { get; set; } = delegate { };
        public Action ActionOnDestruction { get; set; } = delegate { };

        /// <summary>
        /// For if you want to use all custom values, Size here refers to if a physicsbody exists for this particle
        /// </summary>
        public Particle(Vector2 Pos, Vector2 Vel, Vector2 Size, float Orientation, float VelAng, float Duration, float Opacity, Color Color, float Scale, GraphicsComponent Graphics, bool HasBody, Scene BelongedScene)
        {
            this.Pos = Pos;
            this.Vel = Vel;
            this.Orientation = Orientation;
            this.VelAng = VelAng;
            this.Duration = Duration;
            this.Opacity = Opacity;
            this.Color = Color;
            this.Scale = Scale;
            this.Graphics = Graphics;
            this.HasBody = HasBody;
            this.BelongedScene = BelongedScene;
        }

        public Particle(Vector2 Pos, Vector2 Vel, Vector2 Size, float Orientation, float VelAng, float Duration, GraphicsComponent Graphics, bool HasBody, Scene BelongedScene)
        {
            this.Pos = Pos;
            this.Vel = Vel;
            this.Orientation = Orientation;
            this.VelAng = VelAng;
            this.Duration = Duration;
            this.Graphics = Graphics;
            this.HasBody = HasBody;
            this.BelongedScene = BelongedScene;
        }

        /// <summary>
        /// without a body on default, no need for scene, the particle itself is not rendered as part of the scene, but rather to adjust to a specific camera, since these particles have no real effect on surroundings.
        /// </summary>
        public Particle(Vector2 Pos, Vector2 Vel, float VelAng, float Duration, GraphicsComponent Graphics, Scene BelongedScene)
        {
            this.Pos = Pos;
            this.Vel = Vel;
            this.VelAng = VelAng;
            this.Duration = Duration;
            this.Graphics = Graphics;
            this.BelongedScene = BelongedScene;
        }

        public void OnCreation() { }

        public void OnCollision(PhysicsBody Target)
        {
            ActionOnCollision();
        }

        /// <summary>
        /// Includes two sets of systems for if a body exists or not.
        /// If yes, then set body velocity to this velocity, update the body and set the position to match the body.
        /// If not, then update position by velocity directly, with no collision check.
        /// In both cases, acceleration is added on to velocity, most of the time the acceleration is preset
        /// </summary>
        public void Update()
        {
            ActionOnUpdate();
            Graphics.Tint = new Color(Graphics.Tint, Opacity);
            Graphics.Update();

            Vel += Acc;

            if (HasBody)
            {
                Body.BelongedScene = BelongedScene;
                Body.VelocityIgnoringFriction = Vel;
                Body.Update();
                Pos = Body.pos;
            }
            else
            {
                Pos += Vel;
            }

            PixelPos = Pos - new Vector2(Pos.X % GamePanel.PixelSize, Pos.Y % GamePanel.PixelSize);

            Time += GamePanel.ElapsedTime;

            if (Time > Duration)
                OnDestruction();
        }

        public void OnDestruction()
        {
            ActionOnDestruction();
        }

        public void Draw(Vector2 CenterPos, SpriteBatch spriteBatch)
        {
            GraphicsRenderer.DrawComponent(Graphics, new Vector2(PixelPos.X, PixelPos.Y + (Graphics.ShownHeight * GamePanel.PixelSize * Scale)), CenterPos, spriteBatch);
        }
    }
}