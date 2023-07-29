using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rienet
{
    public class Spike : Tile
    {
        public Spike(Vector2 pos, Scene BelongedScene) : base(pos, BelongedScene)
        {
            ID = 2;
            List<Hitbox> hitbox = new List<Hitbox> { new Hitbox(pos.X, pos.Y, 1, 1, 0, 0) };
            body = new DamageBody(4, 0, this, pos, new Vector2(1, 1), hitbox, 0, BelongedScene);
            //add to Scene
            DrawBox = new Vector2(1, 1); //default
            SetDrawPosInWorld(new(X, Y + DrawBox.Y));
        }
    }
}