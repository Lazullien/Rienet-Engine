using Microsoft.Xna.Framework;

namespace Rienet
{
    public struct Transition
    {
        public Rectangle Area;
        public Scene R1;
        public Vector2 SpawnPosInR1;
        public Scene R2;
        public Vector2 SpawnPosInR2;

        public Transition(Rectangle Area, Scene R1, Vector2 SpawnPosInR1, Scene R2, Vector2 SpawnPosInR2)
        {
            this.Area = Area;
            this.R1 = R1;
            this.SpawnPosInR1 = SpawnPosInR1;
            this.R2 = R2;
            this.SpawnPosInR2 = SpawnPosInR2;
        }
    }
}