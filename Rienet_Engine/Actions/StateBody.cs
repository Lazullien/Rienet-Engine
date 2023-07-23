using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rienet
{
    public class StateBody : PhysicsBody
    {
        public Vector2 RelativePos; //the position relative to a source pos
        public Vector2 SourcePos;
        public ulong Duration;
        protected GamePanel gp;

        public StateBody(IBodyVenue BelongedObject, bool Collidable, Vector2 RelativePos, ulong Duration, GamePanel gp) : base(BelongedObject, Collidable)
        {
            this.RelativePos = RelativePos;
            this.Duration = Duration;
            this.gp = gp;
        }

        public StateBody(IBodyVenue BelongedObject, Vector2 RelativePos, Vector2 SourcePos, ulong Duration, Vector2 size, List<Hitbox> hitbox, float momentum, float Roughness, bool Collidable, Scene BelongedScene, GamePanel gp) : base(BelongedObject, RelativePos + SourcePos, size, hitbox, momentum, Roughness, Collidable, BelongedScene)
        {
            this.RelativePos = RelativePos;
            this.SourcePos = SourcePos;
            this.Duration = Duration;
            this.gp = gp;
        }

        public override void Update()
        {
            pos = RelativePos + SourcePos;
            base.Update();
        }
    }
}