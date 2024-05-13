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
        public Vector2 pos;
        public Vector2 DrawBox;
        public Vector2 DrawOffset;
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
        public List<GraphicsComponent> AssociatedGraphics = new();
        public bool interacts;
        public List<InteractableObject> interactableObjects = new();

        protected Entity(Scene Scene)
        {
            SetScene(Scene);
        }

        public void SetScene(Scene BelongedScene)
        {
            this.BelongedScene?.TryRemoveBody(body);
            this.BelongedScene?.EntitiesInScene.Remove(this);
            this.BelongedScene = BelongedScene;
            if (BelongedScene != null && !BelongedScene.EntitiesInScene.Contains(this))
            {
                BelongedScene.EntitiesInScene.Add(this);
                if (body != null)
                {
                    BelongedScene.AddBody(body);
                    body.BelongedScene = BelongedScene;
                }
            }
        }

        public virtual void SetPos(Vector2 p)
        {
            pos = p;
            if (body != null)
            {
                body.pos = pos + BodyTextureDifference;
                body.PixelPos = body.pos - new Vector2(body.X % GamePanel.PixelSize, body.Y % GamePanel.PixelSize);
            }
        }

        public void SetMove(float x, float y)
        {
            Move = new Vector2(x, y);
        }

        public virtual void Update()
        {
            interactableObjects.Clear();

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
            if (Graphics != null)
                GraphicsRenderer.DrawComponent(Graphics, DrawPosInWorld, CenterPos, spriteBatch);
            foreach (var g in AssociatedGraphics)
                if (g != null)
                    GraphicsRenderer.DrawComponent(g, DrawPosInWorld, CenterPos, spriteBatch);
        }

        public virtual void OnCreation()
        {
            body.ActionOnOverallNonCollidableCollision = delegate (PhysicsBody b2)
            {
                if (b2.BelongedObject is InteractableObject i)
                    interactableObjects.Add(i);
            };
        }

        public virtual void OnDamage(float damage) { }

        public virtual void OnKnockback(Vector2 Knockback) { }

/// <summary>
/// REMEMBER TO ASSIGN THIS TO THE DELEGATION METHOD OF PHYSICSBODY FOR THIS EVERY TIME BECAUSE IT'S NOT NORMALLY CALLED IN COLLISION CLASS
/// </summary>
/// <param name="Target"></param>
        public virtual void OnNonCollidableCollision(PhysicsBody Target) { }

        public virtual void OnCollision(PhysicsBody Target) { }

        public virtual void OnDestruction() { }

        public virtual void PerspectiveFilter(SpriteBatch spriteBatch) { }

        public int nonRelativeLayerInScene
        {
            get { return body.layer + BelongedScene.layerZeroPoint - 1; }
            set { body.layer = value - BelongedScene.layerZeroPoint + 1; }
        }

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
            get { return DrawOffset + new Vector2(X, Y + Height); }
        }
    }

    //represented in tiled as "point/" object
    public struct EntityWrapper
    {
        public int id { get; set; }
        public string type { get; set; }
        public float x { get; set; }
        public float y { get; set; }
    }
}