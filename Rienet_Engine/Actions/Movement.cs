using Microsoft.Xna.Framework;

namespace Rienet
{
    public struct Movement
    {
        //basically a vector with time of movement and friction
        public Vector2 Velocity { get; set; }
        public ulong Duration { get; set; }
        public float Friction { get; set; }

        public Movement(Vector2 Velocity, ulong Duration)
        {
            this.Velocity = Velocity;
            this.Duration = Duration;
            Friction = 0;
        }

        public Movement(Vector2 Velocity, ulong Duration, float Friction)
        {
            this.Velocity = Velocity;
            this.Duration = Duration;
            this.Friction = Friction;
        }
    }
}