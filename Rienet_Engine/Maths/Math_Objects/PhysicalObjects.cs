using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rienet
{
    public interface IBodyVenue : IGameObject
    {
        public void OnCollision(PhysicsBody Target);
    }

    public class AnimatedBody : PhysicsBody
    {
        readonly Dictionary<int, AnimatedBodyState[]> Animations = new();
        AnimatedBodyState[] CurrentBodyAnimation;
        readonly Animator animator = new(true);

        public AnimatedBody(IBodyVenue BelongedObject, bool Collidable, bool Animating) : base(BelongedObject, Collidable)
        {
        }

        public AnimatedBody(IBodyVenue BelongedObject, Vector2 pos, Vector2 size, AnimatedBodyState[] bodyAnimation, float inertia, float Roughness, bool Collidable, Scene BelongedScene, bool Animating) : base(BelongedObject, pos, size, bodyAnimation[0].hitbox, inertia, Roughness, Collidable, BelongedScene)
        {
            CurrentBodyAnimation = bodyAnimation;
        }

        public override void Update()
        {
            base.Update();

            animator.Update();
            //alter the state of the body to this
            //CurrentFrame;
            Animations.TryGetValue(AnimationID, out CurrentBodyAnimation);

            if (CurrentBodyAnimation != null && CurrentFrame < CurrentBodyAnimation.Length)
            {
                var CurrentBodyState = CurrentBodyAnimation[CurrentFrame];
                RelativePos = CurrentBodyState.RelativePos;
                hitbox = CurrentBodyState.hitbox;
                VelocityIgnoringFriction += CurrentBodyState.Velocity;
            }
        }

        public void AddAnimation(int Key, Animation Value)
        {
            animator.AddAnimation(Key, Value);
        }

        public void SetAnimation(int ID)
        {
            animator.SetAnimation(ID);
        }

        public void Pause(bool HasDesignatedFrame, int DesignatedFrame)
        {
            animator.Pause(HasDesignatedFrame, DesignatedFrame);
        }

        public void Reset()
        {
            animator.Reset();
        }

        public void Resume() => animator.Resume();

        public int CurrentFrame
        {
            get { return animator.CurrentFrame; }
        }

        public int AnimationID
        {
            get { return animator.AnimationID; }
        }
    }

    public struct AnimatedBodyState
    {
        public Vector2 RelativePos;
        public List<Hitbox> hitbox = new();
        public Vector2 Velocity;

        public AnimatedBodyState(Vector2 RelativePos, Vector2 Velocity)
        {
            this.RelativePos = RelativePos;
            this.Velocity = Velocity;
        }
    }

    public class PhysicsBody : IGameObject
    {
        public bool ConstantUpdate;
        public bool Collidable;
        public IBodyVenue BelongedObject;
        public Scene BelongedScene;
        public int layer = 0;
        public List<Hitbox> hitbox = new();
        public Vector2 pos, PixelPos, RelativePos, FinalPos, size, MainForcePoint; //position and total width and height
        public Vector2 PotentialVelocity; //stacked up and only usable midair
        public Vector2 VelocityForced;
        public Vector2 VelocityIgnoringFriction; //ONLY ONE VELOCITY SHOULD EXIST
        public Vector2 Velocity; //only slowed by friction
        public Vector2 TotalFriction;
        public List<Vector2> CollisionNormal = new();
        public bool VelocityCausesCollision;
        public Vector2 CollisionAtVelocity;
        public float inertia;
        public float momentumPotential;
        public float Roughness;

        //use action delegates for collisions
        public Action<PhysicsBody, Vector2> ActionOnCollision;
        public Action<PhysicsBody, Vector2> ActionOnNonCollidableCollision;
        public Action<PhysicsBody> ActionOnNoCollision;

        public Action action;
        public float actionTimer;

        public PhysicsBody(IBodyVenue BelongedObject, bool Collidable)
        {
            this.BelongedObject = BelongedObject; this.Collidable = Collidable;
        }

        public PhysicsBody(IBodyVenue BelongedObject, Vector2 pos, Vector2 size, List<Hitbox> hitbox, float inertia, float Roughness, bool Collidable, Scene BelongedScene)
        {
            this.BelongedObject = BelongedObject;
            this.hitbox = hitbox;
            this.pos = pos;
            this.size = size;
            this.inertia = inertia;
            this.Roughness = Roughness;
            CollisionNormal = new List<Vector2>();
            foreach (Hitbox b in hitbox)
                b.SetBelongedBody(this);
            this.Collidable = Collidable;

            this.BelongedScene = BelongedScene;
            BelongedScene?.AddBody(this);
        }

        public virtual void Pre_Update()
        {
            Velocity = Vector2.Zero;
            TotalFriction = Vector2.Zero;
        }

        public virtual void Update()
        {
            RemoveFromHitboxChunk();

            CollisionNormal.Clear();

            Velocity += VelocityForced;
            Velocity += VelocityIgnoringFriction;

            VelocityIgnoringFriction = Vector2.Zero;

            foreach (Hitbox b in hitbox)
            {
                b.SetVelocity(Velocity.X, Velocity.Y);
                b.SetPos(FinalX + b.DX, FinalY + b.DY);
            }

            Dynamics.VectorToMovement(this);
            Dynamics.VelocityToPosition(this);

            VelocityForced = Dynamics.ImplementFriction(VelocityForced, TotalFriction);

            FinalPos = pos + RelativePos;

            //set pos to pixel
            PixelPos = FinalPos - new Vector2(X % WorldBody.PXSize, Y % WorldBody.PXSize);

            MainForcePoint = FinalPos + (size / 2);

            BelongedScene?.AddBody(this);
            action?.Invoke();
        }

        //I'm not adding abstract to these cause i don't want every child class have the need to inherit them
        public virtual void OnNonCollidableCollision(PhysicsBody Target, Vector2 Intersection)
        {
            ActionOnNonCollidableCollision?.Invoke(Target, Intersection);
        }

        public virtual void OnCollision(PhysicsBody Target, Vector2 Intersection)
        {
            ActionOnCollision?.Invoke(Target, Intersection);
        }

        public virtual void OnNoCollision(PhysicsBody Target)
        {
            ActionOnNoCollision?.Invoke(Target);
        }

        public virtual void Draw(int X, int Y, SpriteBatch spriteBatch, Texture2D blankRect) { }

        public virtual void OnCreation() { }

        public virtual void OnDestruction()
        {
            BelongedScene.TryRemoveBody(this);
        }

        public void RemoveFromHitboxChunk()
        {
            for (int x = (int)X; x < Width + X; x += HitboxChunk.W)
            {
                for (int y = (int)Y; y < Height + Y; y += HitboxChunk.H)
                {
                    bool ChunkExists = BelongedScene.TryGetHitboxChunk(x, y, out HitboxChunk chunk);
                    if (ChunkExists)
                        chunk.RemoveBody(this);
                }
            }
        }

        public void UpdateHitboxVelocity()
        {
            foreach (Hitbox b in hitbox)
            {
                b.SetVelocity(Velocity.X, Velocity.Y);
            }
        }

        public void UpdateHitboxPosition()
        {
            foreach (Hitbox b in hitbox)
            {
                b.SetPos(X + b.DX, Y + b.DY);
            }
        }

        public void Animate(float time, Action<PhysicsBody> action)
        {
            if (GamePanel.TimeInTicks * GamePanel.ElapsedTime == time)
            {
                action(this);
            }
        }

        public float FinalX
        {
            get { return FinalPos.X; }
            set { FinalPos.X = value; }
        }

        public float FinalY
        {
            get { return FinalPos.Y; }
            set { FinalPos.Y = value; }
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

        public float FX
        {
            get { return MainForcePoint.X; }
            set { MainForcePoint.X = value; }
        }

        public float FY
        {
            get { return MainForcePoint.Y; }
            set { MainForcePoint.Y = value; }
        }

        public float Width
        {
            get { return size.X; }
            set { size.X = value; }
        }

        public float Height
        {
            get { return size.Y; }
            set { size.Y = value; }
        }
    }

    public class Hitbox
    {
        public float X, Y, W, H, VX, VY, DX, DY;
        public PhysicsBody belongedBody;

        public Hitbox(float X, float Y, float W, float H, float VX, float VY, PhysicsBody belongedBody)
        {
            this.X = X; this.Y = Y; this.W = W; this.H = H; this.VX = VX; this.VY = VY; this.belongedBody = belongedBody; DX = X - belongedBody.X; DY = Y - belongedBody.Y;
        }

        public Hitbox(float X, float Y, float W, float H, float VX, float VY)
        {
            this.X = X; this.Y = Y; this.W = W; this.H = H; this.VX = VX; this.VY = VY; belongedBody = default;
        }

        public Hitbox(float X, float Y, float W, float H)
        {
            this.X = X; this.Y = Y; this.W = W; this.H = H; this.VX = 0; this.VY = 0; belongedBody = default;
        }

        public Hitbox() { }

        public void SetBelongedBody(PhysicsBody belongedBody)
        {
            this.belongedBody = belongedBody; DX = X - belongedBody.X; DY = Y - belongedBody.Y;
        }

        public void SetVelocity(float VX, float VY)
        {
            this.VX = VX; this.VY = VY;
        }

        public void SetPos(float X, float Y)
        {
            this.X = X; this.Y = Y;
        }

        public void SetBounds(float X, float Y, float W, float H) //two most important values
        {
            this.X = X; this.Y = Y;
            this.W = W; this.H = H;
        }

        public Vector2 pos
        {
            get { return new(X, Y); }
            set { X = value.X; Y = value.Y; }
        }

        public Vector2 vel
        {
            get { return new(VX, VY); }
            set { VX = value.X; VY = value.Y; }
        }

        public Vector2 size
        {
            get { return new(W, H); }
            set { W = value.X; H = value.Y; }
        }
    }

    public class CircularHitbox : Hitbox
    {
        public float Radius;

        public CircularHitbox(float X, float Y, float Radius, float VX, float VY, PhysicsBody belongedBody)
        {
            this.X = X; this.Y = Y; this.Radius = Radius; this.VX = VX; this.VY = VY; this.belongedBody = belongedBody; DX = X - belongedBody.X; DY = Y - belongedBody.Y;
        }

        public CircularHitbox(float X, float Y, float Radius, float VX, float VY)
        {
            this.X = X; this.Y = Y; this.Radius = Radius; this.VX = VX; this.VY = VY;
        }

        public CircularHitbox(float X, float Y, float Radius)
        {
            this.X = X; this.Y = Y; this.Radius = Radius;
        }
    }
}