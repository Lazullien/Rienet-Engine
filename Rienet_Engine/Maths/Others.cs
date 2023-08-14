using System;
using Microsoft.Xna.Framework;

namespace Rienet
{
    public static class OtherMaths
    {
        public static void Swap(ref float a, ref float b) => (b, a) = (a, b);

        public static int GetPositivity(float x) => (int)(x / Math.Abs(x));

        public static Vector2 GetPositivity(Vector2 x) => x / VecAbs(x);

        public static Vector2 VecAbs(Vector2 vec) => new(Math.Abs(vec.X), Math.Abs(vec.Y));

        public static bool VecNaN(Vector2 vec) => (!double.IsNaN(vec.X)) && (!double.IsNaN(vec.Y));
    }
}