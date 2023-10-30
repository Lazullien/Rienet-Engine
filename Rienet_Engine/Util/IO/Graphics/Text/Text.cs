using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public class Text : UI
    {
        public string text;
        //effects and fonts
        public SpriteFont spriteFont;

        public Text(Vector2 Pos, Vector2 RawSize) : base(Pos, RawSize, false)
        {
        }

        public override void Update()
        {
            base.Update();
        }

        //draws text based on ui info
        public override void Draw(SpriteBatch spriteBatch)
        {
            //draw a rectangle as a test that the ui is in fact working
            spriteBatch.Draw(BlankRect, new(0,0,0,0), null, Color.Red, 0, Vector2.Zero, SpriteEffects.None, 1);

            //spriteBatch.Draw(Texture, Pos, null, Tint, Rotation, Vector2.Zero, Scale, Effects, Depth);
        }

    }
}