using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public class Text : UI
    {
        public string text;
        //effects and fonts
        public SpriteFont spriteFont;
        public Color color;
        public float TextScale;

        public Text(Vector2 Pos, float TextScale, Vector2 RawSize, bool UpdateFirst) : base(Pos, RawSize, UpdateFirst)
        {
            this.TextScale = TextScale;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //separate the text based on sizes, and draw each line individually
            //TextScale * text.Length
        }
    }
}