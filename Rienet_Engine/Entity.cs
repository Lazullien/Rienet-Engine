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
        public const float IFrameTime = 1;
        public const float KnockbackGap = 1f / 8;

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

        public float Health;
        public float MaxHealth, MinHealth;
        public float TimeSinceDamage, TimeSinceKnockback;

        protected Entity(Scene Scene)
        {
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

            if (Health <= MinHealth) OnDestruction();

            TimeSinceDamage += GamePanel.ElapsedTime;
            TimeSinceKnockback += GamePanel.ElapsedTime;
        }

        public virtual void SelfKinetics(Vector2 Direction)
        {
            SelfVelocity += Direction / body.momentumPotential;

            var PreEdit = SelfVelocity;

            if (SelfVelocity != Vector2.Zero && Direction == Vector2.Zero)
                SelfVelocity -= OtherMaths.GetPositivity(SelfVelocity) / (body.momentumPotential * 10);

            if (Math.Sign(PreEdit.X) != Math.Sign(SelfVelocity.X))
                SelfVelocity.X = 0;
            if (Math.Sign(PreEdit.Y) != Math.Sign(SelfVelocity.Y))
                SelfVelocity.Y = 0;

            if (SelfVelocity.Length() * 10 == 0)
                SelfVelocity = Vector2.Zero;

            float percent = body.momentumPotential / body.inertia;
            if (SelfVelocity.Length() > percent)
            {
                SelfVelocity.Normalize();
                SelfVelocity *= percent;
            }

            if (!OtherMaths.VecNaN(SelfVelocity))
                Move = SelfVelocity * Speed;
        }

        //additional movement since they behave quite differently from other movements
        public virtual void SelfKineticX(bool west, bool east)
        {
            float momentumPotential = body.momentumPotential;
            if (west) SelfVelocity.X -= 0.05f / momentumPotential;
            if (east) SelfVelocity.X += 0.05f / momentumPotential;

            //stop if this changes positivity
            float PreSelfVelocityX = SelfVelocity.X;
            if (SelfVelocity.X != 0 && !west && !east)
                SelfVelocity.X -= OtherMaths.GetPositivity(SelfVelocity.X) / (momentumPotential * 10);

            if ((PreSelfVelocityX > 0 && SelfVelocity.X < 0) || (PreSelfVelocityX < 0 && SelfVelocity.X > 0))
                SelfVelocity.X = 0;

            float percent = momentumPotential / body.inertia;
            if (SelfVelocity.X < -1 * percent) SelfVelocity.X = -1 * percent;
            if (SelfVelocity.X > 1 * percent) SelfVelocity.X = 1 * percent;

            //rounding
            if ((int)(SelfVelocity.X * 10) == 0) SelfVelocity.X = 0;

            if (SelfVelocity.X != double.NaN)
                Move.X = SelfVelocity.X * Speed;
        }

        public virtual void SelfKineticY(bool up, bool down)
        {
            float momentumPotential = body.momentumPotential;
            if (up) SelfVelocity.Y -= 0.05f / momentumPotential;
            if (down) SelfVelocity.Y += 0.05f / momentumPotential;

            //stop if this changes positivity
            float PreSelfVelocityY = SelfVelocity.Y;
            if (SelfVelocity.Y != 0 && !up && !down)
                SelfVelocity.Y -= OtherMaths.GetPositivity(SelfVelocity.X) / (momentumPotential * 10);

            if ((PreSelfVelocityY > 0 && SelfVelocity.Y < 0) || (PreSelfVelocityY < 0 && SelfVelocity.Y > 0))
                SelfVelocity.Y = 0;

            float percent = momentumPotential / body.inertia;
            if (SelfVelocity.Y < -1 * percent) SelfVelocity.Y = -1 * percent;
            if (SelfVelocity.Y > 1 * percent) SelfVelocity.Y = 1 * percent;

            //rounding
            if ((int)(SelfVelocity.Y * 10) == 0) SelfVelocity.Y = 0;

            if (SelfVelocity.Y != double.NaN)
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

        public virtual void OnCreation()
        {
        }

        public virtual void OnDamage(float damage)
        {
        }

        public virtual void OnKnockback(Vector2 Knockback)
        {
        }

        public virtual void OnNonCollidableCollision(PhysicsBody Target, Vector2 Intersection)
        {
        }

        public virtual void OnCollision(PhysicsBody Target)
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