using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Rienet
{
    public static class Collision
    {
        public static void GetCollisionOfDynamicBody(PhysicsBody body, Vector2 path, Scene Scene)
        {
            //make a slighty bigger hitbox for searching
            float X = body.X - body.MinimumDif.X, Y = body.Y - body.MinimumDif.Y, W = body.WholeSize.X, H = body.WholeSize.Y;
            float XMin = X, XMax = X + W, YMin = Y, YMax = Y + H;
            Line uw = new(XMin, YMax, XMin + path.X, YMax + path.Y);
            Line ue = new(XMax, YMax, XMax + path.X, YMax + path.Y);
            Line dw = new(XMin, YMin, XMin + path.X, YMin + path.Y);
            Line de = new(XMax, YMin, XMax + path.X, YMin + path.Y);

            var ChunksToCheck = GetChunksInPath(uw, ue, dw, de, Scene);

            List<PhysicsBody> BodiesToCheck = new();

            //use indexes of collided
            Dictionary<int, float> Sorter = new();

            //convert to a single array
            foreach (HitboxChunk chunk in ChunksToCheck)
                foreach (PhysicsBody Body in chunk.BodiesInGrid)
                    if (Body != body && Body.layer == body.layer)
                        BodiesToCheck.Add(Body);

            for (int i = 0; i < BodiesToCheck.Count; i++)
            {
                bool CanCollide = false;
                bool NonCollidableCollision = false;
                PhysicsBody Abody = BodiesToCheck[i];

                foreach (Hitbox bodyHB in body.hitbox)
                    foreach (Hitbox AbodyHB in Abody.hitbox)
                    {
                        //differ intersection algorithm used by situations (sq vs sq / sq vs circ / circ vs circ)
                        Vector2 intersection = Vector2.Zero, Normal = Vector2.Zero;
                        //first check for static intersection, DON"T FORGET TO ADD CIRCLES
                        if (!(Abody.Collidable || body.Collidable) &&
                        (bodyHB is not CircularHitbox) && AbodyHB is not CircularHitbox ? HitboxIntersectsHitbox(bodyHB, AbodyHB)
                        : bodyHB is CircularHitbox c && AbodyHB is CircularHitbox d ? CircleIntersectsCircle(c, d)
                        : (bodyHB is not CircularHitbox) && AbodyHB is CircularHitbox d2 ? HitboxIntersectsCircle(bodyHB, d2)
                        : bodyHB is CircularHitbox c3 && HitboxIntersectsCircle(AbodyHB, c3))
                        {
                            CanCollide = true;
                            //activate collision behavior without normal
                            body.OnNonCollidableCollision(Abody, body.pos);
                            Abody.OnNonCollidableCollision(body, Abody.pos);
                            body.UpdateHitboxVelocity();
                            NonCollidableCollision = true;
                        }
                        //add this to the collided check if it intersects, in the end the bodies who were never processed for collision will call OnNoCollision();
                        else if ((bodyHB is not CircularHitbox) && AbodyHB is not CircularHitbox ? DynamicHitboxIntersectsHitbox(bodyHB, AbodyHB, out intersection, out Normal, out float ContactTime)
                        : bodyHB is CircularHitbox a && AbodyHB is CircularHitbox b ? DynamicCircleIntersectsCircle(a, b, out intersection, out Normal, out ContactTime)
                        : bodyHB is CircularHitbox a3 && DynamicCircleIntersectsHitbox(a3, AbodyHB, out intersection, out Normal, out ContactTime))
                        {
                            CanCollide = true;
                            if (!Sorter.ContainsKey(i))
                                Sorter.Add(i, ContactTime);
                            else if (ContactTime < Sorter[i])
                                Sorter[i] = ContactTime;

                            body.OnSolidIntersection(Abody);
                            Abody.OnSolidIntersection(body);
                        }
                    }
                if (!CanCollide)
                {
                    body.OnNoCollision(Abody);
                    Abody.OnNoCollision(body);
                    body.UpdateHitboxVelocity();
                }
                else if (NonCollidableCollision)
                {
                    body.OnNonCollidableCollision(Abody);
                    Abody.OnNonCollidableCollision(body);
                    body.UpdateHitboxVelocity();
                }
            }

            foreach (var t in Sorter.OrderBy(s => s.Value).ToDictionary(s => s.Key, s => s.Value))
            {
                PhysicsBody BodyToCheck = BodiesToCheck[t.Key];
                bool Collides = false;
                bool Collidable = (BodyToCheck.Collidable || body.Collidable) && !(BodyToCheck.IgnoreSolidBodies || body.IgnoreSolidBodies);

                foreach (Hitbox bodyHB in body.hitbox)
                    foreach (Hitbox BodyToCheckHB in BodyToCheck.hitbox)
                    {

                        Vector2 intersection = Vector2.Zero, Normal = Vector2.Zero;

                        //somewhat reduntant, but hitbox velocities are in fact updated, thus the need for a recheck, try to fix later
                        if ((bodyHB is not CircularHitbox) && BodyToCheckHB is not CircularHitbox ? DynamicHitboxIntersectsHitbox(bodyHB, BodyToCheckHB, out intersection, out Normal, out float ContactTime)
                        : bodyHB is CircularHitbox a && BodyToCheckHB is CircularHitbox b ? DynamicCircleIntersectsCircle(a, b, out intersection, out Normal, out ContactTime)
                        : bodyHB is CircularHitbox a3 && DynamicCircleIntersectsHitbox(a3, BodyToCheckHB, out intersection, out Normal, out ContactTime))
                        {
                            if (Collidable)
                            {
                                body.Velocity += Normal * new Vector2(Math.Abs(body.hitbox[0].VX), Math.Abs(body.hitbox[0].VY));
                                body.VelocityForced += Normal * new Vector2(Math.Abs(body.VelocityForced.X), Math.Abs(body.VelocityForced.Y));
                                body.CollisionNormal.Add(Normal);
                                body.UpdateHitboxVelocity();

                                body.OnCollision(BodyToCheck, intersection);
                                BodyToCheck.OnCollision(body, intersection);
                            }

                            body.UpdateHitboxVelocity();
                            Collides = true;
                        }
                    }

                if (Collides)
                {
                    body.BelongedObject?.OnCollision(BodyToCheck);
                    BodyToCheck.BelongedObject?.OnCollision(body);
                    body.UpdateHitboxVelocity();

                    if (Collidable)
                    {
                        body.OnCollision(BodyToCheck);
                        BodyToCheck.OnCollision(body);
                    }
                }
                else
                {
                    body.OnNoCollision(BodyToCheck);
                    BodyToCheck.OnNoCollision(body);
                    body.UpdateHitboxVelocity();
                }
            }
        }

        //four lines at corners
        public static List<HitboxChunk> GetChunksInPath(Line uw, Line ue, Line dw, Line de, Scene Scene)
        {
            Vector2 Min = new(Math.Min(Math.Min(Math.Min(uw.x1, uw.x2), Math.Min(ue.x1, ue.x2)), Math.Min(Math.Min(dw.x1, dw.x2), Math.Min(de.x1, de.x2))),
                            Math.Min(Math.Min(Math.Min(uw.y1, uw.y2), Math.Min(ue.y1, ue.y2)), Math.Min(Math.Min(dw.y1, dw.y2), Math.Min(de.y1, de.y2)))),
                    Max = new(Math.Max(Math.Max(Math.Max(uw.x1, uw.x2), Math.Max(ue.x1, ue.x2)), Math.Max(Math.Max(dw.x1, dw.x2), Math.Max(de.x1, de.x2))),
                            Math.Max(Math.Max(Math.Max(uw.y1, uw.y2), Math.Max(ue.y1, ue.y2)), Math.Max(Math.Max(dw.y1, dw.y2), Math.Max(de.y1, de.y2))));

            //normalize Min to chunk pos
            Min = new(Min.X - (Min.X % HitboxChunk.W), Min.Y - (Min.Y % HitboxChunk.H));

            //get all chunks in path, min to max pos rect
            List<HitboxChunk> ChunksToCheck = new();
            for (int x = (int)Min.X; x < Max.X; x += HitboxChunk.W)
            {
                for (int y = (int)Min.Y; y < Max.Y; y += HitboxChunk.H)
                {
                    bool ChunkExists = Scene.TryGetHitboxChunk(x, y, out HitboxChunk chunk);
                    if (ChunkExists && !ChunksToCheck.Contains(chunk))
                        ChunksToCheck.Add(chunk);
                }
            }

            return ChunksToCheck;
        }

        public static bool DynamicHitboxIntersectsHitbox(Hitbox a, Hitbox b, out Vector2 Intersection, out Vector2 Normal, out float ContactTime)
        {
            Hitbox B_Expanded = new(b.X - (a.W / 2), b.Y - (a.H / 2), b.W + a.W, b.H + a.H);

            Vector2 LinePos = new(a.X + (a.W / 2), a.Y + (a.H / 2));

            return LineIntersectsHitbox(new Line(LinePos.X, LinePos.Y, LinePos.X + a.VX, LinePos.Y + a.VY), B_Expanded, out Intersection, out Normal, out ContactTime) && ContactTime >= 0 && ContactTime <= 1;
        }

        public static bool DynamicCircleIntersectsHitbox(CircularHitbox a, Hitbox b, out Vector2 Intersection, out Vector2 Normal, out float OverLap)
        {
            Vector2 PotentialPos = a.pos + a.vel;

            Vector2 NearestPoint = new(
                Math.Max(b.VX, Math.Min(PotentialPos.X, b.VX + 1)),
                Math.Max(b.VY, Math.Min(PotentialPos.Y, b.VY + 1))
            );

            var RayToNearestPoint = NearestPoint - PotentialPos;

            OverLap = a.Radius - RayToNearestPoint.Length();
            if (double.IsNaN(OverLap))
                OverLap = 0;

            if (OverLap > 0)
            {
                RayToNearestPoint.Normalize();
                PotentialPos = RayToNearestPoint * OverLap; //now actual landing pos
                Intersection = PotentialPos;
                Normal = a.pos - PotentialPos;
                Normal.Normalize();
                return true;
            }

            Normal = Vector2.Zero;
            Intersection = Vector2.Zero;

            return false;
        }

        public static bool DynamicCircleIntersectsCircle(CircularHitbox a, CircularHitbox b, out Vector2 Intersection, out Vector2 Normal, out float ContactTime)
        {
            CircularHitbox B_Expanded = new(b.X, b.Y, b.Radius + a.Radius);

            Vector2 LinePos = new(a.X + (a.W / 2), a.Y + (a.H / 2));

            return LineIntersectsCircle(new Line(LinePos.X, LinePos.Y, LinePos.X + a.VX, LinePos.Y + a.VY), B_Expanded, out Intersection, out Normal, out ContactTime) && ContactTime <= 1;
        }

        public static bool HitboxIntersectsHitbox(Hitbox a, Hitbox b)
        {
            float AXmin = a.X, AXmax = a.X + a.W, AYmin = a.Y, AYmax = a.Y + a.H,
                  BXmin = b.X, BXmax = b.X + b.W, BYmin = b.Y, BYmax = b.Y + b.H;
            return AXmin <= BXmax && AXmax >= BXmin && AYmin <= BYmax && AYmax >= BYmin;
        }

        public static bool HitboxIntersectsCircle(Hitbox a, CircularHitbox b)
        {
            float DX = Math.Abs(b.X - a.X - (a.W / 2)), DY = Math.Abs(b.Y - a.Y - (a.H / 2));
            if (DX > ((a.W / 2) + b.Radius)) return false;
            if (DY > ((a.H / 2) + b.Radius)) return false;

            if (DX <= (a.W / 2)) return true;
            if (DY <= (a.H / 2)) return true;

            float DXm = DX - (a.W / 2), DYm = DY - (a.H / 2);
            float D = (DXm * DXm) + (DYm * DYm);

            return D <= (b.Radius * b.Radius);
        }

        public static bool CircleIntersectsCircle(CircularHitbox a, CircularHitbox b)
        {
            float DX = a.X - b.X, DY = a.Y - b.Y;
            float Dist = (float)Math.Sqrt((DX * DX) + (DY * DY));
            //check radius with Distance
            return a.Radius + b.Radius >= Dist;
        }

        //if this won't work then use the other intersection method instead where we use 4 lines to represent hitbox
        public static bool LineIntersectsHitbox(Line l, Hitbox b, out Vector2 Intersection, out Vector2 Normal, out float t_hit_near)
        {
            Intersection = Vector2.Zero; Normal = Vector2.Zero; t_hit_near = 0; //on default

            //check ray dir
            Vector2 RayOrigin = new(l.x1, l.y1), RayDir = new(l.x2 - l.x1, l.y2 - l.y1);
            Vector2 TargetPos = new(b.X, b.Y), TargetSize = new(b.W, b.H);

            if (RayDir == Vector2.Zero) return false;

            Vector2 t_near = (TargetPos - RayOrigin) / RayDir, t_far = (TargetPos + TargetSize - RayOrigin) / RayDir;

            //swap
            if (t_near.X > t_far.X) (t_near.X, t_far.X) = (t_far.X, t_near.X);
            if (t_near.Y > t_far.Y) (t_near.Y, t_far.Y) = (t_far.Y, t_near.Y);

            if (t_near.X > t_far.Y || t_near.Y > t_far.X) return false;

            t_hit_near = Math.Max(t_near.X, t_near.Y);
            if (t_hit_near > 1) return false;

            float t_hit_far = Math.Min(t_far.X, t_far.Y);
            if (t_hit_far < 0) return false;

            Intersection = RayOrigin + (t_hit_near * RayDir);

            if (t_near.X > t_near.Y)
            {
                if (RayDir.X < 0)
                    Normal = new Vector2(1, 0); //rectangular for now
                else
                    Normal = new Vector2(-1, 0);
            }
            else if (t_near.X < t_near.Y)
            {
                if (RayDir.Y < 0)
                    Normal = new Vector2(0, 1);
                else
                    Normal = new Vector2(0, -1);
            }

            return true;
        }

        public static bool LineIntersectsCircle(Line l, CircularHitbox b, out Vector2 Intersection, out Vector2 Normal, out float t_hit_near)
        {
            Intersection = Vector2.Zero; Normal = Vector2.Zero; t_hit_near = 0;

            float A = Vector2.Distance(new Vector2(l.x1, l.y1), new Vector2(b.X, b.Y));
            float B = Vector2.Distance(new Vector2(l.x2, l.y2), new Vector2(b.X, b.Y));
            //first check if a point is in circle
            if (A <= b.Radius) return true;
            if (B <= b.Radius) return true;

            //else check short distance to line with radius
            //get area of triangle with heron's formula, then divide it by 3rd side's length for the height
            float C = Vector2.Distance(new Vector2(l.x1, l.y1), new Vector2(l.x2, l.y2));
            float p = (A + B + C) / 2;
            float S = (float)Math.Sqrt(p * (p - A) * (p - B) * (p - C));
            float H = 2 * S / C;

            //get distance of intersection from ends of line and check if they're bigger than the line
            double d1 = (A * A) - (H * H), d2 = (B * B) - (H * H);

            if (d1 <= C * C && d2 <= C * C)
            {
                //get intersection and normal with trigonometry
                //slope of line
                float k = (float)Math.Atan((l.y2 - l.y1) / (l.x2 - l.x1));
                float alpha1 = (float)Math.Asin(H / A);
                float alpha2 = (float)Math.Asin(H / B);

                float theta1 = 1.57079633f - k - alpha1;
                float theta2 = 1.57079633f - k - alpha2;

                Vector2 IT1 = new(b.X + ((float)Math.Sin(theta1) * b.Radius), b.Y + ((float)Math.Cos(theta1) * b.Radius));
                Vector2 IT2 = new(b.X + ((float)Math.Sin(theta2) * b.Radius), b.Y + ((float)Math.Cos(theta2) * b.Radius));

                //get nearest one based on distance from start of line
                Intersection = Vector2.Distance(IT1, new Vector2(l.x1, l.y1)) > Vector2.Distance(IT2, new Vector2(l.x1, l.y1)) ? IT2 : IT1;

                //just set normal in opposite direction of line fn
                Normal = new Vector2(l.x1 - l.x2, l.y1 - l.y2);
                Normal.Normalize();

                Vector2 t_near = ((Intersection - new Vector2(l.x1, l.x2)) / new Vector2(l.x2 - l.x1, l.y2 - l.y1));
                t_hit_near = Math.Max(t_near.X, t_near.Y);

                return true;
            }

            return false;
        }

        public static bool LineIntersectsLine(Line l1, Line l2, out Vector2 pointIfExists)
        {
            Vector2 A = new(l1.x1, l1.y1);
            Vector2 B = new(l1.x2, l1.y2);
            Vector2 C = new(l2.x1, l2.y1);
            Vector2 D = new(l2.x2, l2.y2);
            // Line AB represented as a1x + b1y = c1
            double a1 = B.Y - A.Y;
            double b1 = A.X - B.X;
            double c1 = (a1 * A.X) + (b1 * A.Y);

            // Line CD represented as a2x + b2y = c2
            double a2 = D.Y - C.Y;
            double b2 = C.X - D.X;
            double c2 = (a2 * C.X) + (b2 * C.Y);

            double determinant = (a1 * b2) - (a2 * b1);

            if (determinant == 0)
            {
                // The lines are parallel
                pointIfExists = Vector2.Zero;
                return false;
            }
            else
            {
                double x = ((b2 * c1) - (b1 * c2)) / determinant;
                double y = ((a1 * c2) - (a2 * c1)) / determinant;
                double minX1 = Math.Min(A.X, B.X), minY1 = Math.Min(A.Y, B.Y);
                double maxX1 = Math.Max(A.X, B.X), maxY1 = Math.Max(A.Y, B.Y);
                double minX2 = Math.Min(C.X, D.X), minY2 = Math.Min(C.Y, D.Y);
                double maxX2 = Math.Max(C.X, D.X), maxY2 = Math.Max(C.Y, D.Y);
                if (x >= minX1 && x <= maxX1 && y >= minY1 && y <= maxY1 &&
                    x >= minX2 && x <= maxX2 && y >= minY2 && y <= maxY2)
                {
                    pointIfExists = new Vector2((float)x, (float)y);
                    return true;
                }
                else
                {
                    pointIfExists = Vector2.Zero;
                    return false;
                }
            }
        }

        public static bool PointIntersectsHitbox(Vector2 p, Hitbox b)
        {
            return p.X >= b.X && p.Y >= b.Y && p.X < b.X + b.W && p.Y < b.Y + b.H;
        }
    }

    public static class Proximity
    {
        //faulty and causing lag
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

    public static class Dynamics
    {
        public static void VelocityToPosition(PhysicsBody body)
        {
            Collision.GetCollisionOfDynamicBody(body, body.Velocity, body.BelongedScene);
            //fix to a pixel coordinate
            body.pos += body.Velocity;
        }

        public static Vector2 GetFrictionFromPressureAndCoefficient(Vector2 Pressure, float Coefficient)
        {
            Vector2 Opposite = Vector2.Negate(Pressure);

            if (Opposite != Vector2.Zero)
                Opposite.Normalize();

            return Coefficient * Pressure.Length() * Opposite;
        }

        public static Vector2 ImplementFriction(Vector2 Velocity, Vector2 TotalFriction)
        {
            Vector2 BeforeOperation = Velocity;
            Vector2 Opposite = Vector2.Negate(Velocity);
            if (Opposite != Vector2.Zero)
                Opposite.Normalize();
            TotalFriction += Opposite * WorldBody.DefaultFriction;
            //if velocity changes direction after this set velocity to zero instead
            Velocity += TotalFriction;

            if ((BeforeOperation.X < 0 && Velocity.X > 0) || (BeforeOperation.X > 0 && Velocity.X < 0))
                Velocity.X = 0;
            if ((BeforeOperation.Y < 0 && Velocity.Y > 0) || (BeforeOperation.Y > 0 && Velocity.Y < 0))
                Velocity.Y = 0;

            return Velocity;
        }
    }
}