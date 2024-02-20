using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;

namespace Rienet
{
    //interactions between entities
    public abstract class InteractableObject : Entity
    {
        //the body here defines the range needed for interaction and the behavior on interaction (oncollision & interaction)
        public List<Entity> WhoCanInteract = new();

        protected InteractableObject(Scene scene, PhysicsBody body) : base(scene)
        {
            Gravitational = false;
            this.body = body;
            if (body != null)
            {
                body.BelongedObject = this;
                OnCreation();
            }
        }

        public override void OnCreation()
        {
            body.ActionOnNoCollision = delegate (PhysicsBody b2)
            {
                if (b2.BelongedObject is Entity e && WhoCanInteract.Contains(e))
                {
                    WhoCanInteract.Remove(e);
                }
            };

            body.ActionOnOverallNonCollidableCollision = delegate (PhysicsBody b2)
            {
                if (b2.BelongedObject is Entity e && !WhoCanInteract.Contains(e))
                {
                    WhoCanInteract.Add(e);
                }
            };
        }

        public abstract void OnInteraction();
    }
}