using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel.Design;

namespace Rienet
{
    public abstract class Entity : IBodyVenue
    {
        public Vector2 Spawnpoint;
        public Vector2 pos;
        public Vector2 DrawBox;
        public Vector2 SelfVelocity;
        public Vector2 Move;
        public float Speed;
        public bool Gravitational = true;
        public PhysicsBody body;
        public Moveset CurrentMoveSet;
        public Vector2 BodyTextureDifference;
        public Texture2D current;
        public int id;
        public Scene BelongedScene;
        public bool InTransition;
        public GraphicsComponent Graphics;

        protected Entity(Scene Scene)
        {
            SetScene(Scene);
        }

        public void SetScene(Scene BelongedScene)
        {
            this.BelongedScene = BelongedScene;
            if (!BelongedScene.EntitiesInScene.Contains(this))
                BelongedScene.EntitiesInScene.Add(this);
        }

        public void SetPos(Vector2 p)
        {
            pos = p;
            if (body != null)
            {
                body.pos = pos + BodyTextureDifference;
                body.PixelPos = body.pos - new Vector2(body.X % WorldBody.PXSize, body.Y % WorldBody.PXSize);
            }
        }

        public void SetMove(float x, float y)
        {
            Move = new Vector2(x, y);
        }

        public virtual void Update()
        {
            if (body != null)
            {
                body.BelongedScene = BelongedScene;
                //add gravity
                if (Gravitational)
                    body.VelocityForced += WorldBody.Gravity;
                //add move to velocity
                body.Velocity += Move;

                body.momentumPotential = body.inertia - body.Roughness;

                body.Update();
                pos = body.PixelPos - BodyTextureDifference;
            }
        }

        public virtual void Draw(Vector2 CenterPos, SpriteBatch spriteBatch, GamePanel gamePanel)
        {
            if (Graphics is SpriteSheet spriteSheet)
                GraphicsRenderer.DrawSpriteInSheet(spriteSheet, DrawPosInWorld, CenterPos, spriteBatch);
            else if (Graphics is Image image)
                GraphicsRenderer.DrawImage(image, DrawPosInWorld, CenterPos, spriteBatch);
            else if (Graphics != null)
                GraphicsRenderer.DrawComponent(Graphics, DrawPosInWorld, CenterPos, spriteBatch);
        }

        public virtual void OnCreation() {}

        public virtual void OnDamage(float damage) {}

        public virtual void OnKnockback(Vector2 Knockback) {}

        public virtual void OnNonCollidableCollision(PhysicsBody Target, Vector2 Intersection) {}

        public virtual void OnCollision(PhysicsBody Target) {}

        public virtual void OnDestruction() {}

        public virtual void PerspectiveFilter(SpriteBatch spriteBatch) {}

        public int layer
        {
            get { return body.layer; }
            set { body.layer = value; }
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
            get { return new Vector2(X, Y + Height); }
        }
    }
}