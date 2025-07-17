using System;

namespace Rienet
{
    public class SineWave
    {
        public float Frequency = 1;
        public float Magnitude = 1;

        public float X;

        public float Y
        {
            get
            {
                return Magnitude * (float)Math.Sin(Frequency * X);
            }
        }
    }

    public class CosineWave
    {
        public float Frequency = 1;
        public float Magnitude = 1;

        public float X;

        public float Y
        {
            get
            {
                return Magnitude * (float)Math.Cos(Frequency * X);
            }
        }
    }
}