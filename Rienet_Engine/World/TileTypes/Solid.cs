using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rienet
{
    public class Solid : Tile
    {
        public Solid(Vector2 pos, float Roughness, Scene BelongedScene) : base(pos, BelongedScene)
        {
            ID = 1;
            List<Hitbox> hitbox = new() { new Hitbox(pos.X, pos.Y, 1, 1) };
            body = new PhysicsBody(this, pos, new Vector2(1, 1), hitbox, 0, Roughness, true, BelongedScene);
            //add to Scene
            DrawBox = new Vector2(1, 1); //default
            friction = 0.05f;
            SetDrawPosInWorld(new(X, Y + DrawBox.Y));
        }
    }
}