using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public abstract class GameState
    {
        protected ContentManager Content;
        protected GraphicsDevice graphicsDevice;
        protected GamePanel Game;

        public GameState(GamePanel Game, GraphicsDevice graphicsDevice, ContentManager Content)
        {
            this.Game = Game;
            this.Content = Content;
            this.graphicsDevice = graphicsDevice;
        }

        public abstract void Reset();
        
        public abstract void End();

        public abstract void Update(GameTime gameTime);

        public abstract void PostUpdate(GameTime gameTime);

        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
    }
}