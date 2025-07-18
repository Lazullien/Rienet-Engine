using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Rienet
{
    public interface IBodyVenue : IGameObject
    {
        public void OnCollision(PhysicsBody Target);
    }

    /// <summary>
    /// remember that if main pos is in void the body is still killed. 
    /// the actual body doesn't have size, only pos, and hitboxes for each frame are from the current bodystate
    /// </summary>
    public class AnimatedBody : PhysicsBody
    {
        public readonly Dictionary<int, BodyAnimation> Animations = new();
        BodyAnimation CurrentBodyAnimation;
        public Animator animator = new(true);

        public AnimatedBody(IBodyVenue BelongedObject, Vector2 pos, Vector2 size, Dictionary<int, BodyAnimation> bodyAnimations, float inertia, float Roughness, bool Collidable, Scene BelongedScene, bool Animating) : base(BelongedObject, pos, size, new(), inertia, Roughness, Collidable, BelongedScene)
        {
            Animations = bodyAnimations;
            foreach (int i in bodyAnimations.Keys)
            {
                var bodyAnimation = bodyAnimations[i];
                animator.AddAnimation(i, new(bodyAnimation.Frames, bodyAnimation.Delay, bodyAnimation.Loop));
            }
        }

        public override void Update()
        {
            base.Update();

            if (!Destroyed)
            {
                animator.Update();
                //alter the state of the body to this
                //CurrentFrame;
                Animations.TryGetValue(AnimationID, out CurrentBodyAnimation);

                if (CurrentFrame < CurrentBodyAnimation.Frames.Length)
                {
                    var CurrentBodyState = CurrentBodyAnimation.AnimatedBodyStates[CurrentFrame];
                    RelativePos = CurrentBodyState.RelativePos;
                    hitbox = CurrentBodyState.hitbox;
                    VelocityIgnoringFriction += CurrentBodyState.Velocity;
                }
            }
        }

        public override void OnDestruction()
        {
            foreach (var key in Animations.Keys)
            {
                var a = Animations[key];
                foreach (var s in a.AnimatedBodyStates.Keys)
                {
                    a.AnimatedBodyStates[s] = null;
                }
                Animations[key] = null;
            }
            Animations.Clear();
            animator.Dispose();
            animator = null;
            base.OnDestruction();
        }

        public void AddAnimation(int Key, Animation Value)
        {
            animator.AddAnimation(Key, Value);
        }

        public void SetRefreshedAnimation(int ID)
        {
            animator.SetAnimation(ID);
            Reset();
            Resume();
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

    public class BodyAnimation
    {
        public int[] Frames { get; set; }
        public float Delay { get; set; }
        public bool Loop { get; set; }
        public Dictionary<int, AnimatedBodyState> AnimatedBodyStates { get; private set; }

        public BodyAnimation(int FrameCount, float Delay, bool Loop, Dictionary<int, AnimatedBodyState> AnimatedBodyStates)
        {
            Frames = new int[FrameCount];
            for (int i = 0; i < Frames.Length; i++)
                Frames[i] = i;
            this.Delay = Delay;
            this.Loop = Loop;
            this.AnimatedBodyStates = AnimatedBodyStates;
        }

        public BodyAnimation(int[] Frames, float Delay, bool Loop, Dictionary<int, AnimatedBodyState> AnimatedBodyStates)
        {
            this.Frames = Frames;
            this.Delay = Delay;
            this.Loop = Loop;
            this.AnimatedBodyStates = AnimatedBodyStates;
        }
    }

    public class AnimatedBodyState
    {
        public Vector2 RelativePos { get; set; }
        public List<Hitbox> hitbox { get; private set; } = new();
        public Vector2 Velocity { get; set; }

        public AnimatedBodyState(Vector2 RelativePos, Vector2 Velocity, List<Hitbox> hitbox)
        {
            this.RelativePos = RelativePos;
            this.Velocity = Velocity;
            this.hitbox = hitbox;
        }
    }

    public class PhysicsBody : IGameObject
    {
        public bool ConstantUpdate { get; set; }
        //Collidable + Collidable = solid collision
        //Collidable + NonCollidable = solid collision
        //NonCollidable + NonColliable = two ghosts passing through each other
        //Ignore Solid body = ignore collidables
        public bool IgnoreSolidBodies { get; set; }
        public bool Collidable { get; set; }
        /// <summary>
        /// may be null, so don't use inner variables unless sure it exists (not that the class has any inner variables)
        /// </summary>
        public IBodyVenue BelongedObject { get; set; }
        public Scene BelongedScene { get; set; }
        public int layer { get; set; } = 0;
        public List<Hitbox> hitbox { get; protected set; } = new();
        //MinimumDif refers to influences of negative values in relative hitboxes
        public Vector2 pos, PixelPos, RelativePos, FinalPos, size, MinimumDif, WholeSize, MainForcePoint;
        public Vector2 PotentialVelocity; //stacked up and only usable midair
        public Vector2 VelocityForced;
        public Vector2 VelocityIgnoringFriction;
        public Vector2 Velocity; //Net Velocity
        /// <summary>
        /// Last velocity before collision and friction
        /// </summary>
        public Vector2 LastVelocity;
        public bool NoFixedVelocity { get; set; } = true;
        public Vector2 FixedVelocity;
        public Vector2 TotalFriction;
        public List<Vector2> CollisionNormal { get; private set; } = new();
        /// <summary>
        /// this doesn't really affect bodies when you're just setting their velocities, but you can definitely utilize it on your own
        /// </summary>
        public float inertia { get; set; }
        public float momentumPotential { get; set; }
        public float Roughness { get; set; }

        //use action delegates for collisions
        public Action<PhysicsBody, Vector2> ActionOnCollision { get; set; }
        public Action<PhysicsBody> ActionOnOverallCollision { get; set; }
        public Action<PhysicsBody> ActionOnSolidIntersection { get; set; }
        public Action<PhysicsBody, Vector2> ActionOnNonCollidableCollision { get; set; }
        public Action<PhysicsBody> ActionOnOverallNonCollidableCollision { get; set; }
        public Action<PhysicsBody> ActionOnNoCollision { get; set; }
        /// <summary>
        /// TO ENSURE THE BODY ISN'T FURTHER ADDED TO THE SCENE CHUNKS AFTER DESTRUCTION
        /// </summary>
        public bool Destroyed { get; set; }

        public Action action { get; set; }
        public float actionTimer { get; set; }

        /// <summary>
        /// For pathing algorithms, this values is from -1~1, at -1, this is considered an absolute obstacle, at 1, it is extremely favored to go towards, at 0, it is not accounted
        /// </summary>
        public float CollisionWillingness { get; set; }

        /// <summary>
        /// FOR FULL VERSION, IT'S (BelongedObject, pos, size, hitbox, inertia, Roughness, Collidable, BelongedScene)
        /// </summary>
        /// <param name="BelongedObject"></param>
        /// <param name="Collidable"></param>
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
            WholeSize = size;
            this.inertia = inertia;
            this.Roughness = Roughness;
            CollisionNormal = new List<Vector2>();
            foreach (Hitbox b in hitbox)
                b.SetBelongedBody(this);
            this.Collidable = Collidable;

            this.BelongedScene = BelongedScene;
            BelongedScene?.AddBody(this);
        }

        public void SetPos(Vector2 Pos)
        {
            RemoveFromHitboxChunk();
            pos = Pos;
        }

        //remember call especially for things without update
        public void FillOtherPosTypes()
        {
            FinalPos = pos + RelativePos;

            //set pos to pixel
            PixelPos = FinalPos - new Vector2(X % GamePanel.PixelSize, Y % GamePanel.PixelSize);

            MainForcePoint = FinalPos + (size / 2);
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

            if (NoFixedVelocity)
            {
                Velocity += VelocityForced;
                Velocity += VelocityIgnoringFriction;
            }
            else
                Velocity = FixedVelocity;

            VelocityIgnoringFriction = Vector2.Zero;

            foreach (Hitbox b in hitbox)
            {
                b.SetVelocity(Velocity.X, Velocity.Y);
                b.SetPos(FinalX + b.DX, FinalY + b.DY);
            }

            LastVelocity = Velocity;

            Dynamics.VelocityToPosition(this);

            VelocityForced = Dynamics.ImplementFriction(VelocityForced, TotalFriction);

            FillOtherPosTypes();

            if (!Destroyed)
                BelongedScene?.AddBody(this);
            action?.Invoke();
        }

        /// <summary>
        /// DON'T USE THESE METHODS UNLESS INTERSECTION IS NEEDED, MULTI INTERATIONS HAVE NO EFFECT, OR USE A BOOL LOCK SO IT CAN ONLY BE RUN ONCE, THIS IS CALLED ON EVERY INDIVIDUAL HITBOX COLLISION
        /// </summary>
        public virtual void OnNonCollidableCollision(PhysicsBody Target, Vector2 Intersection)
        {
            ActionOnNonCollidableCollision?.Invoke(Target, Intersection);
        }

        /// <summary>
        /// DON'T USE THESE METHODS UNLESS INTERSECTION IS NEEDED, MULTI INTERATIONS HAVE NO EFFECT, OR USE A BOOL LOCK SO IT CAN ONLY BE RUN ONCE, THIS IS CALLED ON EVERY INDIVIDUAL HITBOX COLLISION
        /// </summary>
        public virtual void OnCollision(PhysicsBody Target, Vector2 Intersection)
        {
            ActionOnCollision?.Invoke(Target, Intersection);
        }

        public virtual void OnCollision(PhysicsBody Target)
        {
            ActionOnOverallCollision?.Invoke(Target);
        }

        /// <summary>
        /// Same as OnCollision but doesn't need velocity or exact intersection, as long as they overlap
        /// </summary>
        public virtual void OnSolidIntersection(PhysicsBody Target)
        {
            ActionOnSolidIntersection?.Invoke(Target);
        }

        public virtual void OnNonCollidableCollision(PhysicsBody Target)
        {
            ActionOnOverallNonCollidableCollision?.Invoke(Target);
        }

        public virtual void OnNoCollision(PhysicsBody Target)
        {
            ActionOnNoCollision?.Invoke(Target);
        }

        public virtual void Draw(int X, int Y, SpriteBatch spriteBatch, Texture2D blankRect) { }

        public virtual void OnCreation() { }

        public virtual void OnDestruction()
        {
            Destroyed = true;
            BelongedScene.TryRemoveBody(this);
            hitbox.Clear();
        }

        public void RemoveFromHitboxChunk()
        {
            for (int x = (int)X; x < Width + X; x += HitboxChunk.W)
            {
                for (int y = (int)Y; y < Height + Y; y += HitboxChunk.H)
                {
                    bool ChunkExists = BelongedScene.TryGetHitboxChunk(x, y, out HitboxChunk chunk);
                    if (ChunkExists)
                    {
                        chunk.RemoveBody(this);
                    }
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

        /// <summary>
        /// Minimum position accounting negative values within the hitboxes
        /// </summary>
        public void SetMinimumDif(float x, float y)
        {
            MinimumDif = new(x, y);
        }

        /// <summary>
        /// actual layer id, the other one is relative to mainlayer, this layer uses 0, 1, 2 counting, not 1, 2, 3
        /// </summary>
        public int nonRelativeLayerInScene
        {
            get { return layer + BelongedScene.layerZeroPoint - 1; }
            set { layer = value - BelongedScene.layerZeroPoint + 1; }
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
        public readonly static Hitbox Zero = new(0, 0, 0, 0), One = new(0, 0, 1, 1);
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

        public Hitbox(float DX, float DY, float W, float H, bool IndicatorThisIsForFillingDeltaValues_PutAnythingYouWant)
        {
            this.DX = DX; this.DY = DY; this.W = W; this.H = H; VX = 0; VY = 0; belongedBody = default;
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