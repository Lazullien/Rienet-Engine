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
        public float friction;
        public Vector2 pos;
        public PhysicsBody body;
        public Vector2 BodyTextureDifference;
        public Vector2 DrawBox;
        public Scene BelongedScene;
        readonly GamePanel gp;

        public static Texture2D mono;

        public static void LoadTileGraphics(ContentManager Content)
        {
            mono = Content.Load<Texture2D>("TileTextures/ExampleTile");
        }

        protected Tile(Vector2 pos, Scene BelongedScene)
        {
            this.pos = pos;
            this.BelongedScene = BelongedScene;
        }

        public virtual void OnCollision(PhysicsBody Target)
        {
            //apply friction perpendicular to target pressure
            if (Target.Velocity != Vector2.Zero)
            {
                Target.TotalFriction += Kinetics.GetFrictionFromPressureAndCoefficient(Target.Velocity, friction);
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
    }
}