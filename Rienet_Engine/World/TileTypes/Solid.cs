using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rienet
{
    public class Solid : Tile
    {
        public Solid(Vector2 pos, float Roughness, GraphicsComponent Graphics, PhysicsBody Body, Scene BelongedScene) : base(pos, Graphics, Body, BelongedScene)
        {
            //body = new PhysicsBody(this, pos, new Vector2(1, 1), hitbox, 0, Roughness, true, BelongedScene);
            //add to Scene
            DrawBox = new Vector2(1, 1); //default
            friction = 0.05f;
            SetDrawPosInWorld(new(X, Y + DrawBox.Y));
        }
    }
}