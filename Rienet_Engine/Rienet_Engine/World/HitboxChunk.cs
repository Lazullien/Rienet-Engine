using System.Collections.Generic;

namespace Rienet
{
    public class HitboxChunk
    {
        public const int W = 4, H = 4;
        public List<PhysicsBody> BodiesInGrid;
        public float X, Y;

        public HitboxChunk(int X, int Y)
        {
            this.X = X; this.Y = Y;
            BodiesInGrid = new List<PhysicsBody>();
        }

        public void AddBody(PhysicsBody Body)
        {
            if (!BodiesInGrid.Contains(Body)) BodiesInGrid.Add(Body);
        }

        public void RemoveBody(PhysicsBody Body)
        {
            if (BodiesInGrid.Contains(Body)) BodiesInGrid.Remove(Body);
        }
    }
}