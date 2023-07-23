using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using Atelo;
using System.Collections.Generic;

namespace Rienet
{
    public class GamePanel : Game
    {
        public static int PixelsInTile = 8;
        public KeyboardState keyState;
        public WorldBody World;
        public WorldProjection projection;
        public UIHandler uiHandler;
        public int TileSize;
        public int Width, Height;
        public Player pl;
        public Camera cam;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;

        public static string DebugValueToPrint;

        public ulong Time;
        public double FPS;

        public GamePanel()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            TileSize = 24;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            World = new WorldBody();
            uiHandler = new UIHandler(this);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteFont = Content.Load<SpriteFont>("DefaultTextFont");
            // TODO: use this.Content to load your game content here
            AteloInitializer.LoadAllContent(Content);

            //add objects to scene here
            Tester.LoadTestingObjects(Content);
            AteloInitializer.BuildWorld(World, this);
            cam = new Camera(new Vector2(0, 0), new Vector2(30, 30), World.Scenes[0], World, this);
            pl = new Player(this, World.Scenes[0]);
            Pawn pawn = new Pawn(this, World.Scenes[0], 1, 0.2f);

            Tester.LoadTestingObjects(Content);
            Tile.LoadTileGraphics(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            Width = Window.ClientBounds.Width;
            Height = Window.ClientBounds.Height;
            GetKeyboardInput();

            uiHandler.Update();

            cam.Scene.Update();
            cam.Update(pl.pos, pl.DrawBox);
            //cam.ChangeScene(0);
            Time++;
            FPS = 1 / gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, default);

            uiHandler.Draw();
            //BasicRenderingAlgorithms.DrawHitbox(new Hitbox(16,15,1,1), cam.pos, cam.Scene, _spriteBatch, this, cam.blankRect);

            cam.ProjectToScreen(_spriteBatch, GraphicsDevice);
            _spriteBatch.DrawString(_spriteFont, pl.pos.X + "," + pl.pos.Y, new Vector2(0, 0), Color.White);
            _spriteBatch.DrawString(_spriteFont, Time.ToString(), new Vector2(0, 20), Color.Yellow);
            //_spriteBatch.Draw(pl.current, pl.pos, null, Color.White, 0, new Vector2(1, 1), new Vector2(0.5f, 0.5f), SpriteEffects.None, 0);
            _spriteBatch.DrawString(_spriteFont, pl.Health.ToString(), new Vector2(0, 40), Color.Red);
            _spriteBatch.End();
            base.Draw(gameTime);
        }

        public void DrawContent()
        {

        }

        void GetKeyboardInput()
        {
            keyState = Keyboard.GetState();
        }
    }
}
