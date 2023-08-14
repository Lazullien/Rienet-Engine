using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rienet
{
    public class DamageBody : PhysicsBody
    {
        public float DamageValue, Knockback;

        public DamageBody(IBodyVenue BelongedObject, float DamageValue, float Knockback) : base(BelongedObject, false)
        {
            this.DamageValue = DamageValue;
            this.Knockback = Knockback;
        }

        public DamageBody(float DamageValue, float Knockback, IBodyVenue BelongedObject, Vector2 pos, Vector2 size, List<Hitbox> hitbox, float momentumPotential, Scene BelongedScene) : base(BelongedObject, pos, size, hitbox, momentumPotential, 0, false, BelongedScene)
        {
            this.DamageValue = DamageValue;
            this.Knockback = Knockback;
        }

        public override void OnNonCollidableCollision(PhysicsBody Target, Vector2 Intersection)
        {
            //deal damage & knockback
            if(Target != this && Target.BelongedObject is Entity e)
            {
                e.OnDamage(DamageValue);
            }
        }
    }
}