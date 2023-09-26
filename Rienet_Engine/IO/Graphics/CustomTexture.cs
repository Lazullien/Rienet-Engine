using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    //enables usage of layers for textures and pixel specification
    public class CustomTexture
    {
        public Texture2D texture { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public Rectangle source { get; private set; }
        public int layer { get; private set; }
        public Vector2 Center { get { return new Vector2(Width / 2, Height / 2); } }
        public float Scale = 1;

        public CustomTexture(Texture2D texture, Rectangle source)
        {
            this.texture = texture;
            Width = Scale * texture.Width / GamePanel.PixelsInTile;
            Height = Scale * texture.Height / GamePanel.PixelsInTile;
            this.source = source;
        }

        public static CustomTexture Load(string filename, ContentManager Content, Rectangle source)
        {
            var texture = Content.Load<Texture2D>(filename);

            return new CustomTexture(texture, source);
        }

        public void Unload()
        {
            texture.Dispose();
            texture = null;
        }
    }
}