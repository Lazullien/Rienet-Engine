using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rienet
{
    public static class Proximity
    {
        public static List<PhysicsBody> GetNearbyBodies(Vector2 Pos, float Radius, Scene SceneToCheck)
        {
            List<PhysicsBody> Bodies = new();
            //bres circle works, but abstracting the circle to a square is much easier and efficient for most cases
            float MinX = Pos.X - Radius, MaxX = Pos.X + Radius, MinY = Pos.Y - Radius, MaxY = Pos.Y + Radius;
            for (float X = MinX - (MinX % HitboxChunk.W); X <= MaxX - (MaxX % HitboxChunk.W); X += HitboxChunk.W)
            {
                for (float Y = MinY - (MinY % HitboxChunk.H); Y <= MaxY - (MaxY % HitboxChunk.H); Y += HitboxChunk.H)
                {
                    if (SceneToCheck.TryGetHitboxChunk(X, Y, out HitboxChunk Chunk))
                    {
                        foreach (PhysicsBody Body in Chunk.BodiesInGrid)
                        {
                            if (!Bodies.Contains(Body) && Vector2.Distance(Body.MainForcePoint, Pos) <= Radius)
                                Bodies.Add(Body);
                        }
                    }
                }
            }

            return Bodies;
        }
    }
}