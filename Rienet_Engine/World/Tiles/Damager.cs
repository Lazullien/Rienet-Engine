using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rienet
{
    public class Damager : Tile
    {
        public Damager(Vector2 pos, float Roughness, GraphicsComponent Graphics, PhysicsBody Body, Scene BelongedScene) : base(pos, Graphics, Body, BelongedScene)
        {
            List<Hitbox> hitbox = new List<Hitbox> { new Hitbox(pos.X, pos.Y, 1, 1, 0, 0) };
            body = new DamageBody(4, 0, this, pos, new Vector2(1, 1), hitbox, 0, BelongedScene);
            //add to Scene
            DrawBox = new Vector2(1, 1); //default
            SetDrawPosInWorld(new(X, Y + DrawBox.Y));
        }
    }
}