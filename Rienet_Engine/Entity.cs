using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rienet
{
    public class Entity : IBodyVenue
    {
        public const float IFrameTime = 1;
        public const float KnockbackGap = 1f / 16;

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
        protected GamePanel game;
        public Scene BelongedScene;
        public bool InTransition;
        public GraphicsComponent Graphics;

        public float Health;
        public float MaxHealth, MinHealth;
        public float TimeSinceDamage, TimeSinceKnockback;

        public Entity(GamePanel game, Scene Scene)
        {
            this.game = game;
            MinHealth = 0;
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
            body.pos = pos + BodyTextureDifference;
            body.PixelPos = body.pos - new Vector2(body.X % WorldBody.PXSize, body.Y % WorldBody.PXSize);
        }

        public void SetMove(float x, float y)
        {
            Move = new Vector2(x, y);
        }

        public virtual void OnCreation()
        {
        }

        public virtual void Update()
        {
            //add gravity
            if (Gravitational)
                body.VelocityForced += WorldBody.Gravity;
            //add move to velocity
            body.Velocity += Move;

            body.Update();
            pos = body.PixelPos - BodyTextureDifference;

            if (Health <= MinHealth) OnDeath();

            TimeSinceDamage += GamePanel.ElapsedTime;
            TimeSinceKnockback += GamePanel.ElapsedTime;
        }

        public virtual void OnDamage(float damage)
        {
        }

        public virtual void OnKnockback(Vector2 Knockback)
        {
        }

        public virtual void SelfKineticX(bool west, bool east)
        {
            float momentum = body.momentum;
            if (west) SelfVelocity.X -= 0.05f / momentum;
            if (east) SelfVelocity.X += 0.05f / momentum;

            //stop if this changes positivity
            float PreSelfVelocityX = SelfVelocity.X;
            if (SelfVelocity.X != 0 && !west && !east)
                SelfVelocity.X -= momentum / 2 * SelfVelocity.X / (float)Math.Abs(SelfVelocity.X) / (momentum / 2);

            if ((PreSelfVelocityX > 0 && SelfVelocity.X < 0) || (PreSelfVelocityX < 0 && SelfVelocity.X > 0))
                SelfVelocity.X = 0;

            if (SelfVelocity.X < -1) SelfVelocity.X = -1;
            if (SelfVelocity.X > 1) SelfVelocity.X = 1;

            //rounding
            if ((int)(SelfVelocity.X * 10) == 0) SelfVelocity.X = 0;

            Move.X = SelfVelocity.X * Speed;
        }

        public virtual void SelfKineticY(bool up, bool down)
        {
            float momentum = body.momentum;
            if (up) SelfVelocity.Y -= 0.05f / momentum;
            if (down) SelfVelocity.Y += 0.05f / momentum;

            //stop if this changes positivity
            float PreSelfVelocityY = SelfVelocity.Y;
            if (SelfVelocity.Y != 0 && !up && !down)
                SelfVelocity.Y -= momentum / 2 * SelfVelocity.Y / (float)Math.Abs(SelfVelocity.Y) / (momentum / 2);

            if ((PreSelfVelocityY > 0 && SelfVelocity.Y < 0) || (PreSelfVelocityY < 0 && SelfVelocity.Y > 0))
                SelfVelocity.Y = 0;

            if (SelfVelocity.Y < -1) SelfVelocity.Y = -1;
            if (SelfVelocity.Y > 1) SelfVelocity.Y = 1;

            //rounding
            if ((int)(SelfVelocity.Y * 10) == 0) SelfVelocity.Y = 0;

            Move.Y = SelfVelocity.Y * Speed;
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

        public virtual void OnNonCollidableCollision(PhysicsBody Target, Vector2 Intersection)
        {
        }

        public virtual void OnCollision(PhysicsBody Target)
        {
        }

        public virtual void OnDeath()
        {
        }

        public virtual void OnDestruction()
        {
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

        public Vector2 DrawPosInWorld
        {
            get { return new Vector2(X, Y + DrawBox.Y); }
        }
    }
}