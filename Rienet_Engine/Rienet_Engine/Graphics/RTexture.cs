using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public class RTexture
    {
        public Texture2D Texture { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public Vector2 Center { get { return new Vector2(Width / 2, Height / 2); } }
        public float Scale = 1;

        public RTexture(Texture2D Texture)
        {
            this.Texture = Texture;
            Width = Scale * Texture.Width / GamePanel.PixelsInTile;
        }

        public static RTexture Load(string filename, ContentManager Content)
        {
            var Texture = Content.Load<Texture2D>(filename);

            return new RTexture(Texture);
        }

        public void Unload()
        {
            Texture.Dispose();
            Texture = null;
        }
    }
}