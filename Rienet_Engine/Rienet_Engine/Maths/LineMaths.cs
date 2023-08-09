using System;
using System.Collections.Generic;
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
            foreach(Vector2 pos in BaseLine)
            {
                FinalLine.Add(pos); FinalLine.Add(new Vector2(pos.X, pos.Y - 1)); FinalLine.Add(new Vector2(pos.X, pos.Y + 1));
            }
            //also add left and right for baseline's edges
            Vector2 Start = BaseLine[0], End = BaseLine[^1];
            FinalLine.Add(new Vector2(Start.X - 1, Start.Y)); FinalLine.Add(new Vector2(End.X + 1, End.Y));
            return FinalLine;
        }
    }

    public static class CustomMaths
    {
        public static void Swap(ref float a, ref float b)
        {
            (b, a) = (a, b);
        }
    }
}