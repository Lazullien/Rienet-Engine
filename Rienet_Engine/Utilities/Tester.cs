using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public static class Tester
    {
        public static Texture2D TestBackground;
        public static void LoadTestingObjects(ContentManager Content)
        {
            TestBackground = Content.Load<Texture2D>("backgrounds/debugbg");
        }
    }
}