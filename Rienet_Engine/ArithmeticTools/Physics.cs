using System;
using Microsoft.Xna.Framework;
using MonoGame;
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
            float X = body.X, Y = body.Y, W = body.Width, H = body.Height;
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
                BodiesToCheck.AddRange(chunk.BodiesInGrid);

            for (int i = 0; i < BodiesToCheck.Count; i++)
            {
                PhysicsBody Abody = BodiesToCheck[i];
                //differ intersection algorithm used by situations (sq vs sq / sq vs circ / circ vs circ)
                Vector2 intersection = Vector2.Zero, Normal = Vector2.Zero;

                //add this to the collided check if it intersects, in the end the bodies who were never processed for collision will call OnNoCollision();
                if ((!(body.hitbox[0] is CircularHitbox)) && !(Abody.hitbox[0] is CircularHitbox) ? DynamicHitboxIntersectsHitbox(body.hitbox[0], Abody.hitbox[0], out intersection, out Normal, out float ContactTime)
                : body.hitbox[0] is CircularHitbox a && Abody.hitbox[0] is CircularHitbox b ? DynamicCircleIntersectsCircle(a, b, out intersection, out Normal, out ContactTime)
                : (!(body.hitbox[0] is CircularHitbox)) && Abody.hitbox[0] is CircularHitbox b2 ? DynamicHitboxIntersectsCircle(body.hitbox[0], b2, out intersection, out Normal, out ContactTime)
                : body.hitbox[0] is CircularHitbox a3 && DynamicCircleIntersectsHitbox(a3, Abody.hitbox[0], out intersection, out Normal, out ContactTime))
                {
                    Sorter.Add(i, ContactTime);
                    if (!(Abody.Collidable || body.Collidable))
                    {
                        //activate collision behavior without normal
                        body.OnNonCollidableCollision(Abody, intersection);
                        Abody.OnNonCollidableCollision(body, intersection);
                        body.UpdateHitboxVelocity();
                    }
                }
                else
                {
                    body.OnNoCollision(Abody);
                    Abody.OnNoCollision(body);
                    body.UpdateHitboxVelocity();
                }
            }

            foreach (var t in Sorter.OrderBy(s => s.Value).ToDictionary(s => s.Key, s => s.Value))
            {
                PhysicsBody BodyToCheck = BodiesToCheck[t.Key];

                Vector2 intersection = Vector2.Zero, Normal = Vector2.Zero; float ContactTime = 0;

                if ((body.hitbox[0] is not CircularHitbox) && BodyToCheck.hitbox[0] is not CircularHitbox ? DynamicHitboxIntersectsHitbox(body.hitbox[0], BodyToCheck.hitbox[0], out intersection, out Normal, out ContactTime)
                : body.hitbox[0] is CircularHitbox a && BodyToCheck.hitbox[0] is CircularHitbox b ? DynamicCircleIntersectsCircle(a, b, out intersection, out Normal, out ContactTime)
                : (body.hitbox[0] is not CircularHitbox) && BodyToCheck.hitbox[0] is CircularHitbox b2 ? DynamicHitboxIntersectsCircle(body.hitbox[0], b2, out intersection, out Normal, out ContactTime)
                : body.hitbox[0] is CircularHitbox a3 && DynamicCircleIntersectsHitbox(a3, BodyToCheck.hitbox[0], out intersection, out Normal, out ContactTime))
                {
                    if (BodyToCheck.Collidable || body.Collidable)
                    {
                        body.Velocity += Normal * new Vector2(Math.Abs(body.hitbox[0].VX), Math.Abs(body.hitbox[0].VY));
                        body.VelocityForced += Normal * new Vector2(Math.Abs(body.VelocityForced.X), Math.Abs(body.VelocityForced.Y));
                        body.CollisionNormal.Add(Normal);
                        body.UpdateHitboxVelocity();

                        body.OnCollision(BodyToCheck, intersection);
                        BodyToCheck.OnCollision(body, intersection);
                    }

                    body.UpdateHitboxVelocity();

                    body.BelongedObject?.OnCollision(BodyToCheck);
                    BodyToCheck.BelongedObject?.OnCollision(body);

                    body.UpdateHitboxVelocity();
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
            Vector2 Min = new Vector2(Math.Min(Math.Min(Math.Min(uw.x1, uw.x2), Math.Min(ue.x1, ue.x2)), Math.Min(Math.Min(dw.x1, dw.x2), Math.Min(de.x1, de.x2))),
                            Math.Min(Math.Min(Math.Min(uw.y1, uw.y2), Math.Min(ue.y1, ue.y2)), Math.Min(Math.Min(dw.y1, dw.y2), Math.Min(de.y1, de.y2)))),
                    Max = new Vector2(Math.Max(Math.Max(Math.Max(uw.x1, uw.x2), Math.Max(ue.x1, ue.x2)), Math.Max(Math.Max(dw.x1, dw.x2), Math.Max(de.x1, de.x2))),
                            Math.Max(Math.Max(Math.Max(uw.y1, uw.y2), Math.Max(ue.y1, ue.y2)), Math.Max(Math.Max(dw.y1, dw.y2), Math.Max(de.y1, de.y2))));

            //normalize Min to chunk pos
            Min = new Vector2(Min.X - Min.X % HitboxChunk.W, Min.Y - Min.Y % HitboxChunk.H);

            //get all chunks in path, min to max pos rect
            List<HitboxChunk> ChunksToCheck = new List<HitboxChunk>();
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
            //yif(a.VX == 0 && a.VY == 0)
            //return false;

            Hitbox B_Expanded = new Hitbox(b.X - a.W / 2, b.Y - a.H / 2, b.W + a.W, b.H + a.H);

            Vector2 LinePos = new Vector2(a.X + a.W / 2, a.Y + a.H / 2);

            return LineIntersectsHitbox(new Line(LinePos.X, LinePos.Y, LinePos.X + a.VX, LinePos.Y + a.VY), B_Expanded, out Intersection, out Normal, out ContactTime) && ContactTime <= 1;
        }

        public static bool DynamicHitboxIntersectsCircle(Hitbox a, CircularHitbox b, out Vector2 Intersection, out Vector2 Normal, out float ContactTime)
        {
            Vector2 LinePos = new Vector2(a.X + a.W / 2, a.Y + a.H / 2);

            bool CircIntersection = LineIntersectsCircle(new Line(LinePos.X, LinePos.Y, LinePos.X + a.VX, LinePos.Y + a.VY), b, out Intersection, out Normal, out ContactTime) && ContactTime <= 1;

            if (!CircIntersection) return false;

            //modify intersection pos based on comparison with b's center, adding a's width and height, kinda like an eclipse
            Intersection += new Vector2(Intersection.X > b.X ? a.W / 2 : Intersection.X < b.X ? -a.W / 2 : 0, Intersection.Y > b.Y ? a.H / 2 : Intersection.Y < b.Y ? -a.H / 2 : 0);
            return true;
        }

        public static bool DynamicCircleIntersectsHitbox(CircularHitbox a, Hitbox b, out Vector2 Intersection, out Vector2 Normal, out float ContactTime)
        {
            Intersection = default; Normal = default; ContactTime = 0;

            Hitbox B_Expanded = new Hitbox(b.X - a.Radius, b.Y - a.Radius, b.W + a.Radius * 2, b.H + a.Radius * 2);

            Vector2 LinePos = new Vector2(a.X + a.Radius, a.Y + a.Radius);

            Line line = new Line(LinePos.X, LinePos.Y, LinePos.X + a.VX, LinePos.Y + a.VY);

            if (!LineIntersectsHitbox(line, B_Expanded, out Vector2 InitialInters, out Vector2 InitialNormal, out float InitalContactTime) && InitalContactTime <= 1) return false;

            //check if it's on the corners, if so check if circular corners still allows Collision
            //up west
            if (InitialInters.Y >= b.Y + b.H && InitialInters.X <= b.X) { return LineIntersectsCircle(line, new CircularHitbox(b.X, b.Y + b.H, a.Radius), out Intersection, out Normal, out ContactTime); }
            else if (InitialInters.Y > b.Y + b.H && InitialInters.X >= b.X + b.W) { return LineIntersectsCircle(line, new CircularHitbox(b.X, b.Y + b.H, a.Radius), out Intersection, out Normal, out ContactTime); }
            else if (InitialInters.Y < b.Y && InitialInters.X <= b.X) { return LineIntersectsCircle(line, new CircularHitbox(b.X, b.Y + b.H, a.Radius), out Intersection, out Normal, out ContactTime); }
            else if (InitialInters.Y > b.Y && InitialInters.X >= b.X + b.W) { return LineIntersectsCircle(line, new CircularHitbox(b.X, b.Y + b.H, a.Radius), out Intersection, out Normal, out ContactTime); }
            else { return true; }
        }

        public static bool DynamicCircleIntersectsCircle(CircularHitbox a, CircularHitbox b, out Vector2 Intersection, out Vector2 Normal, out float ContactTime)
        {
            CircularHitbox B_Expanded = new CircularHitbox(b.X, b.Y, b.Radius + a.Radius);

            Vector2 LinePos = new Vector2(a.X + a.W / 2, a.Y + a.H / 2);

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
            float DX = Math.Abs(b.X - a.X - a.W / 2), DY = Math.Abs(b.Y - a.Y - a.H / 2);
            if (DX > (a.W / 2 + b.Radius)) return false;
            if (DY > (a.H / 2 + b.Radius)) return false;

            if (DX <= (a.W / 2)) return true;
            if (DY <= (a.H / 2)) return true;

            float DXm = DX - a.W / 2, DYm = DY - a.H / 2;
            float D = DXm * DXm + DYm * DYm;

            return D <= (b.Radius * b.Radius);
        }

        public static bool CircleIntersectsCircle(CircularHitbox a, CircularHitbox b)
        {
            float DX = a.X - b.X, DY = a.Y - b.Y;
            float Dist = (float)Math.Sqrt(DX * DX + DY * DY);
            //check radius with Distance
            return a.Radius + b.Radius >= Dist;
        }

        //if this won't work then use the other intersection method instead where we use 4 lines to represent hitbox
        public static bool LineIntersectsHitbox(Line l, Hitbox b, out Vector2 Intersection, out Vector2 Normal, out float t_hit_near)
        {
            Intersection = Vector2.Zero; Normal = Vector2.Zero; t_hit_near = 0; //on default

            //check ray dir
            Vector2 RayOrigin = new Vector2(l.x1, l.y1), RayDir = new Vector2(l.x2 - l.x1, l.y2 - l.y1);
            Vector2 TargetPos = new Vector2(b.X, b.Y), TargetSize = new Vector2(b.W, b.H);

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

            Intersection = RayOrigin + t_hit_near * RayDir;

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
            double d1 = A * A - H * H, d2 = B * B - H * H;

            if (d1 <= C * C && d2 <= C * C)
            {
                //get intersection and normal with trigonometry
                //slope of line
                float k = (float)Math.Atan((l.y2 - l.y1) / (l.x2 - l.x1));
                float alpha1 = (float)Math.Asin(H / A);
                float alpha2 = (float)Math.Asin(H / B);

                float theta1 = 1.57079633f - k - alpha1;
                float theta2 = 1.57079633f - k - alpha2;

                Vector2 IT1 = new Vector2(b.X + (float)Math.Sin(theta1) * b.Radius, b.Y + (float)Math.Cos(theta1) * b.Radius);
                Vector2 IT2 = new Vector2(b.X + (float)Math.Sin(theta2) * b.Radius, b.Y + (float)Math.Cos(theta2) * b.Radius);

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
            Vector2 A = new Vector2(l1.x1, l1.y1);
            Vector2 B = new Vector2(l1.x2, l1.y2);
            Vector2 C = new Vector2(l2.x1, l2.y1);
            Vector2 D = new Vector2(l2.x2, l2.y2);
            // Line AB represented as a1x + b1y = c1
            double a1 = B.Y - A.Y;
            double b1 = A.X - B.X;
            double c1 = a1 * A.X + b1 * A.Y;

            // Line CD represented as a2x + b2y = c2
            double a2 = D.Y - C.Y;
            double b2 = C.X - D.X;
            double c2 = a2 * C.X + b2 * C.Y;

            double determinant = a1 * b2 - a2 * b1;

            if (determinant == 0)
            {
                // The lines are parallel
                pointIfExists = Vector2.Zero;
                return false;
            }
            else
            {
                double x = (b2 * c1 - b1 * c2) / determinant;
                double y = (a1 * c2 - a2 * c1) / determinant;
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
            return (p.X >= b.X && p.Y >= b.Y && p.X < b.X + b.W && p.Y < b.Y + b.H);
        }
    }

    public static class Kinetics
    {
        public static void VelocityToPosition(PhysicsBody body)
        {
            Collision.GetCollisionOfDynamicBody(body, body.Velocity, body.BelongedScene);
            //fix to a pixel coordinate
            body.pos += body.Velocity;
        }

        public static void VectorToMovement(PhysicsBody body)
        {
            if (body != null)
            {
                //based on addedforce, manipulate velocity
                //body.Velocity = GetVelocity(body.Velocity)
                //check if this velocity causes a collision
            }
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

        public static Vector2 ForceToVelocity(Vector2 OriginalVelocity, Vector2 Force, float mass)
        {
            Vector2 Velocity = OriginalVelocity;
            Velocity += Force / mass; //not accounting time cause its interval is just one frame

            return Velocity;
        }
    }
}