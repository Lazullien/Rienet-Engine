using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Rienet
{
    public static class Bresenham
    {
        public static List<Vector2> GetBresLine(Line l)
        {
            float x0 = l.x1, x1 = l.x2, y0 = l.y1, y1 = l.y2;
            List<Vector2> line = new();

            float dx = Math.Abs(x1 - x0);
            float dy = Math.Abs(y1 - y0);

            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;

            float err = dx - dy;
            float e2;

            int looped = 1;
            while (true)
            {
                line.Add(new Vector2(x0, y0));

                if (x0 == x1 && y0 == y1)
                    break;

                //a safety guard to prevent never ending loop
                if (looped > 10000)
                    break;

                e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
                looped++;
            }
            return line;
        }

        public static List<Vector2> GetAllPossibleIntegerLine(Line l)
        {
            List<Vector2> FinalLine = new();
            //get center line
            var BaseLine = GetBresLine(l);
            foreach (Vector2 pos in BaseLine)
            {
                FinalLine.Add(pos); FinalLine.Add(new Vector2(pos.X, pos.Y - 1)); FinalLine.Add(new Vector2(pos.X, pos.Y + 1));
            }
            //also add left and right for baseline's edges
            Vector2 Start = BaseLine[0], End = BaseLine[^1];
            FinalLine.Add(new Vector2(Start.X - 1, Start.Y)); FinalLine.Add(new Vector2(End.X + 1, End.Y));
            return FinalLine;
        }
    }

    public static class PerlinNoise
    {
        public static int[] GetTable(string Seed)
        {
            var Arranged = OtherMaths.Shuffle(Table, Seed);
            return Arranged.Concat(Arranged).ToArray();
        }

        public static double OctavePerlin(double x, double y, double octave, double persistence, int[] Table)
        {
            double total = 0;
            double frequency = 2;
            double amplitude = 1;
            double max = -1.5;
            for (int o = 0; o < octave; o++)
            {
                total += Perlin(x * frequency, y * frequency, Table) * amplitude;
                max += amplitude;
                amplitude *= persistence;
                frequency *= 2;
            }
            return total / max;
        }

        public static double Perlin(double x, double y, int[] Table)
        {
            int X = (int)Math.Floor(x) & 255,
                Y = (int)Math.Floor(y) & 255;
            x -= Math.Floor(x);
            y -= Math.Floor(y);
            double u = Fade(x),
                   v = Fade(y);
            int rn = Table[Table[X] + Y], rn2 = Table[Table[X + 1] + Y], rn3 = Table[Table[X] + Y + 1], rn4 = Table[Table[X + 1] + Y + 1];
            return Lerp(v, Lerp(u, Gradient(rn, x, y),
                                   Gradient(rn2, x - 1, y)),
                           Lerp(u, Gradient(rn3, x, y - 1),
                                   Gradient(rn4, x - 1, y - 1)));
        }

        static double Lerp(double a, double le, double ri)
        {
            return le * (1 - a) + a * ri;
        }

        static double Gradient(int hash, double x, double y)
        {
            return (hash & 3) switch
            {
                0 => x + y,
                1 => -x + y,
                2 => x - y,
                3 => -x - y,
                _ => 0,
            };
        }

        static double Fade(double y)
        {
            return y * y * y * (y * (y * 6 - 15) + 10);
        }

        readonly static int[] Table =
        { 151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142,
        8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117,
        35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134,
        139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46,
        245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200,
        196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5,
        202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183,
        170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19,
        98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210,
        144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157,
        184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243,
        141, 128, 195, 78, 66, 215, 61, 156, 180 };
    }
}