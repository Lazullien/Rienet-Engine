using Microsoft.Xna.Framework;

namespace Rienet
{
    public class Hitbox
    {
        public float X, Y, W, H, VX, VY, DX, DY;
        public PhysicsBody belongedBody;

        public Hitbox(float X, float Y, float W, float H, float VX, float VY, PhysicsBody belongedBody)
        {
            this.X = X; this.Y = Y; this.W = W; this.H = H; this.VX = VX; this.VY = VY; this.belongedBody = belongedBody; DX = X - belongedBody.X; DY = Y - belongedBody.Y;
        }

        public Hitbox(float X, float Y, float W, float H, float VX, float VY)
        {
            this.X = X; this.Y = Y; this.W = W; this.H = H; this.VX = VX; this.VY = VY; belongedBody = default;
        }

        public Hitbox(float X, float Y, float W, float H)
        {
            this.X = X; this.Y = Y; this.W = W; this.H = H; this.VX = 0; this.VY = 0; belongedBody = default;
        }

        public Hitbox()
        {
        }

        public void SetBelongedBody(PhysicsBody belongedBody)
        {
            this.belongedBody = belongedBody; DX = X - belongedBody.X; DY = Y - belongedBody.Y;
        }

        public void SetVelocity(float VX, float VY)
        {
            this.VX = VX; this.VY = VY;
        }

        public void SetPos(float X, float Y)
        {
            this.X = X; this.Y = Y;
        }

        public void SetBounds(float X, float Y, float W, float H) //two most important values
        {
            this.X = X; this.Y = Y;
            this.W = W; this.H = H;
        }

        public Vector2 pos
        {
            get { return new(X, Y); }
            set { X = value.X; Y = value.Y; }
        }

        public Vector2 vel
        {
            get { return new(VX, VY); }
            set { VX = value.X; VY = value.Y; }
        }

        public Vector2 size
        {
            get { return new(W, H); }
            set { W = value.X; H = value.Y; }
        }
    }

    public class CircularHitbox : Hitbox
    {
        public float Radius;

        public CircularHitbox(float X, float Y, float Radius, float VX, float VY, PhysicsBody belongedBody)
        {
            this.X = X; this.Y = Y; this.Radius = Radius; this.VX = VX; this.VY = VY; this.belongedBody = belongedBody; DX = X - belongedBody.X; DY = Y - belongedBody.Y;
        }

        public CircularHitbox(float X, float Y, float Radius, float VX, float VY)
        {
            this.X = X; this.Y = Y; this.Radius = Radius; this.VX = VX; this.VY = VY;
        }

        public CircularHitbox(float X, float Y, float Radius)
        {
            this.X = X; this.Y = Y; this.Radius = Radius;
        }
    }
}