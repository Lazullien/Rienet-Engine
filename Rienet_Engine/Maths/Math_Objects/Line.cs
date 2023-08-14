public struct Line
{
    public float x1, y1, x2, y2;

    public Line(float x1, float y1, float x2, float y2)
    {
        this.x1 = x1; this.y1 = y1; this.x2 = x2; this.y2 = y2;
    }

    public static Line operator *(Line l, float scaler)
    {
        return new Line(l.x1, l.y1, (l.x2 - l.x1) * scaler + l.x1, (l.y2 - l.y1) * scaler + l.y1);
    }
}