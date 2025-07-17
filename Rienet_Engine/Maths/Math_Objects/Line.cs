using System;
using Microsoft.Xna.Framework;

namespace Rienet
{
    public struct Line
    {
        public float x1, y1, x2, y2;

        public Line(float x1, float y1, float x2, float y2)
        {
            this.x1 = x1; this.y1 = y1; this.x2 = x2; this.y2 = y2;
        }

        public Line(Vector2 start, Vector2 end)
        {
            x1 = start.X;
            y1 = start.Y;
            x2 = end.X;
            y2 = end.Y;
        }

        public static Line operator *(Line l, float scaler)
        {
            return new Line(l.x1, l.y1, (l.x2 - l.x1) * scaler + l.x1, (l.y2 - l.y1) * scaler + l.y1);
        }

        public Vector2 Pos1
        {
            readonly get { return new Vector2(x1, y1); }
            set { x1 = value.X; y1 = value.Y; }
        }

        public Vector2 Pos2
        {
            readonly get { return new Vector2(x2, y2); }
            set { x2 = value.X; y2 = value.Y; }
        }

        public double Leng
        {
            get { return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)); }
        }
    }
}