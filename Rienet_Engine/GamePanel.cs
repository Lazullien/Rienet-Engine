using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using Atelo;
using System;

namespace Rienet
{
    public class GamePanel : Game
    {
        public string Title;
        public Version _version;
        public static GamePanel Instance;

        public const int PixelsInTile = 8;
        public static int BasisFrameTime = 1;
        public static int TileSize = 24;
        public static int Width, Height;
        public static float ElapsedTime { get; private set; }
        public static float RawElapsedTime { get; private set; }
        public static float TimePace = 1;
        public static ulong Time;
        public static double FPS;
        public static float TimeOneFrame = 1f / 60;

        public static KeyboardState keyState;
        public static MouseState mouseState;
        public static GamePadState gamePadState;

        public static WorldBody World;
        public static UIHandler uiHandler;

        #region  toremove
        //these should be removed
        public Player pl;
        public static Camera cam;
        #endregion

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;

        public GamePanel()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Instance = this;
        }

        protected override void Initialize()
        {
            DeviceInput.Initialize();
            World = new WorldBody();
            uiHandler = new UIHandler();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteFont = Content.Load<SpriteFont>("DefaultTextFont");

            //first load engine integrated content
            Tile.LoadTileGraphics(Content);

            //then load custom content
            Initializer.LoadAllContent(Content);

            //add objects to scene here
            Tester.LoadTestingObjects(Content);
            Initializer.BuildWorld(World);
            pl = new Player(World.Scenes[1]);
            var pawn = new Pawn(World.Scenes[1], 1, 0.2f);
            cam = new Camera(new Vector2(0, 0), new Vector2(35, 35), World.Scenes[1], World, this) { LockOnEntity = pl, LockOn = true };
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            RawElapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            ElapsedTime = RawElapsedTime * TimePace;

            Width = Window.ClientBounds.Width;
            Height = Window.ClientBounds.Height;

            GetInput();
            DeviceInput.Update();

            uiHandler.Update();

            cam.Scene.Update();
            cam.Update();

            Time++;
            TimeOneFrame = (float)gameTime.ElapsedGameTime.TotalSeconds;
            FPS = 1 / TimeOneFrame;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, default);

            uiHandler.Draw();
            //Renderer.DrawHitbox(new Hitbox(16,15,1,1), cam.pos, cam.Scene, _spriteBatch, this, cam.blankRect);

            cam.ProjectToScreen(_spriteBatch);
            _spriteBatch.DrawString(_spriteFont, pl.X + "," + pl.Y, new Vector2(0, 0), Color.White);
            _spriteBatch.DrawString(_spriteFont, Time.ToString(), new Vector2(0, 20), Color.Yellow);
            //_spriteBatch.Draw(pl.current, pl.pos, null, Color.White, 0, new Vector2(1, 1), new Vector2(0.5f, 0.5f), SpriteEffects.None, 0);
            _spriteBatch.DrawString(_spriteFont, pl.Health.ToString(), new Vector2(0, 40), Color.Red);
            _spriteBatch.DrawString(_spriteFont, pl.DamageCharge.ToString(), new Vector2(0, 60), Color.Pink);
            _spriteBatch.End();
            base.Draw(gameTime);
        }

        public void DrawContent()
        {

        }

        static void GetInput()
        {
            keyState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            //gamePadState = GamePad.GetState();
        }
    }
}
