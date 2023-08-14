using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public abstract class Tile : IBodyVenue
    {
        public int ID;
        public int TileStateID; //used for identical tiles with varying appearances
        public float friction;
        public Vector2 pos;
        public PhysicsBody body;
        public Vector2 BodyTextureDifference;
        public Vector2 DrawBox = Vector2.One;
        public Scene BelongedScene;
        public GraphicsComponent Graphics;

        public static Texture2D mono;

        //default
        internal static void LoadTileGraphics(ContentManager Content)
        {
            mono = Content.Load<Texture2D>("TileTextures/ExampleTile");
        }

        protected Tile(Vector2 pos, GraphicsComponent Graphics, PhysicsBody Body, Scene BelongedScene)
        {
            this.pos = pos;
            this.BelongedScene = BelongedScene;

            this.Graphics = Graphics ?? new Image(Vector2.Zero, Vector2.Zero, mono);
            body = Body;
        }

        protected void SetDrawPosInWorld(Vector2 Pos) => DrawPosInWorld = Pos;

        public virtual void OnCollision(PhysicsBody Target)
        {
            //apply friction perpendicular to target pressure
            if (Target.Velocity != Vector2.Zero)
            {
                Target.TotalFriction += Kinetics.GetFrictionFromPressureAndCoefficient(Target.Velocity, friction);
                Target.momentumPotential -= friction;
                if (Target.momentumPotential < 0)
                    Target.momentumPotential = 0;
            }
        }

        public virtual void OnCreation()
        {
        }

        public virtual void Update()
        {
        }

        public virtual void OnDestruction()
        {
        }

        public virtual void Draw(Vector2 CenterPos, SpriteBatch spriteBatch, GamePanel gamePanel)
        {
            if (Graphics is SpriteSheet spriteSheet)
                BasicRenderingAlgorithms.DrawSpriteInSheet(spriteSheet, DrawPosInWorld, CenterPos, spriteBatch);
            else if (Graphics is Image image)
                BasicRenderingAlgorithms.DrawImage(image, DrawPosInWorld, CenterPos, spriteBatch);
            else if (Graphics != null)
                BasicRenderingAlgorithms.DrawComponent(Graphics, DrawPosInWorld, CenterPos, spriteBatch);
        }

        public float X
        {
            get { return pos.X; }
            set { pos.X = value; }
        }

        public float Y
        {
            get { return pos.Y; }
            set { pos.Y = value; }
        }

        public float Width
        {
            get { return DrawBox.X; }
            set { DrawBox.X = value; }
        }

        public float Height
        {
            get { return DrawBox.Y; }
            set { DrawBox.Y = value; }
        }

        public float HitboxWidth
        {
            get { return body.size.X; }
            set { body.size.X = value; }
        }

        public float HitboxHeight
        {
            get { return body.size.Y; }
            set { body.size.Y = value; }
        }

        public Vector2 HitboxSize
        {
            get { return body.size; }
            set { body.size = value; }
        }

        public Vector2 VelocityForced
        {
            get { return body.VelocityForced; }
            set { body.VelocityForced = value; }
        }

        public Vector2 VelocityIgnoringFriction
        {
            get { return body.VelocityIgnoringFriction; }
            set { body.VelocityIgnoringFriction = value; }
        }

        public Vector2 PotentialVelocity
        {
            get { return body.PotentialVelocity; }
            set { body.PotentialVelocity = value; }
        }

        public Vector2 MainForcePoint
        {
            get { return body.MainForcePoint; }
            set { body.MainForcePoint = value; }
        }

        public Vector2 DrawPosInWorld
        {
            get { return new Vector2(X, Y + DrawBox.Y); }
            set { pos = value - new Vector2(0, DrawBox.Y); }
        }
    }
}