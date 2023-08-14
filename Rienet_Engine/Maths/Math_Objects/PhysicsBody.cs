using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rienet
{
    public class PhysicsBody : IGameObject
    {
        public bool ConstantUpdate;
        public bool Collidable;
        public IBodyVenue BelongedObject;
        public Scene BelongedScene;
        public List<Hitbox> hitbox;
        public Vector2 pos, PixelPos, size, MainForcePoint; //position and total width and height
        public Vector2 PotentialVelocity; //stacked up and only usable midair
        public Vector2 VelocityForced;
        public Vector2 VelocityIgnoringFriction; //ONLY ONE VELOCITY SHOULD EXIST
        public Vector2 Velocity; //only slowed by friction
        public Vector2 TotalFriction;
        public List<Vector2> CollisionNormal;
        public bool VelocityCausesCollision;
        public Vector2 CollisionAtVelocity;
        public float inertia;
        public float momentumPotential;
        public float Roughness;

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
                b.SetPos(X + b.DX, Y + b.DY);
            }

            Kinetics.VectorToMovement(this);
            Kinetics.VelocityToPosition(this);

            VelocityForced = Kinetics.ImplementFriction(VelocityForced, TotalFriction);

            //set pos to pixel
            PixelPos = pos - new Vector2(X % WorldBody.PXSize, Y % WorldBody.PXSize);

            MainForcePoint = pos + (size / 2);

            BelongedScene?.AddBody(this);
        }

        public virtual void OnNonCollidableCollision(PhysicsBody Target, Vector2 Intersection)
        {
        }

        public virtual void OnCollision(PhysicsBody Target, Vector2 Intersection)
        {
        }

        public virtual void OnNoCollision(PhysicsBody Target)
        {
        }

        public virtual void Draw(int X, int Y, SpriteBatch spriteBatch, Texture2D blankRect)
        {
        }

        public virtual void OnCreation()
        {
        }

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
}