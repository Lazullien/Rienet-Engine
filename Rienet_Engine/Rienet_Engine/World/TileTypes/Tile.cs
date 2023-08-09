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
        public Vector2 DrawPosInWorld;
        public Scene BelongedScene;
        public GraphicsComponent Graphics;
        readonly GamePanel gp;

        public static Texture2D mono;

        internal static void LoadTileGraphics(ContentManager Content)
        {
            mono = Content.Load<Texture2D>("TileTextures/ExampleTile");
        }

        protected Tile(Vector2 pos, GraphicsComponent Graphics, PhysicsBody Body, Scene BelongedScene)
        {
            this.pos = pos;
            this.BelongedScene = BelongedScene;
            DrawPosInWorld = new(X, Y + DrawBox.Y);

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
                Target.momentum -= friction;
                if (Target.momentum < 0)
                    Target.momentum = 0;
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
    }
}