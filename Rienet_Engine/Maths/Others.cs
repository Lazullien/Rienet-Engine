using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Rienet
{
    public static class OtherMaths
    {
        public static void Swap(ref float a, ref float b) => (b, a) = (a, b);

        public static int GetPositivity(float x) => (int)(x / Math.Abs(x));

        public static bool NumOppositeSigns(float a, float b) => a * b < 0;

        public static Vector2 GetPositivity(Vector2 x) => x / VecRemoveZero(VecAbs(x));

        public static Vector2 VecRemoveZero(Vector2 vec) => new(vec.X == 0 ? 1 : vec.X, vec.Y == 0 ? 1 : vec.Y);

        public static Vector2 VecAbs(Vector2 vec) => new(Math.Abs(vec.X), Math.Abs(vec.Y));

        public static bool VecNotNaN(Vector2 vec) => (!double.IsNaN(vec.X)) && (!double.IsNaN(vec.Y));

        public static bool VecHasZero(Vector2 vec) => vec.X == 0 || vec.Y == 0;
        public static float VecDotPro(Vector2 v1, Vector2 v2) => v1.X * v2.X + v1.Y * v2.Y;
        public static Vector2 VecCrossPro(Vector2 v1, Vector2 v2) => new(v1.X * v2.Y, v1.Y * v2.X);

        public static double PointDistToLine(Vector2 point, Line line)
        {
            return Math.Abs((line.x2 - line.x1) * (point.Y - line.y1) - (line.y2 - line.y1) * (point.X - line.x1)) / line.Leng;
        }

        public static T[] Shuffle<T>(T[] ar, string Seed)
        {
            Random random = new(Seed.GetHashCode());
            return ar.OrderBy(o => random.Next()).ToArray();
        }

        public static List<T> Shuffle<T>(List<T> ar, string Seed)
        {
            Random random = new(Seed.GetHashCode());
            return ar.OrderBy(o => random.Next()).ToList();
        }

        public static void Shuffle<T>(this Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
    }
}