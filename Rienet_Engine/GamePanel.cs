using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System.Text.Json;
using System.Data.Common;
using System.Text.Json.Nodes;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System.Data;
using TacticalBand;

namespace Rienet
{
    //organize this 
    public class GamePanel : Game
    {
        public string Title;
        public Version _version;
        public static GamePanel Instance { get; private set; }
        public static RenderTarget2D renderTarget2D { get; private set; }
        public static RenderTarget2D secondRenderTarget2D { get; private set; }
        public static GraphicsDevice graphicsDevice { get; private set; }
        public static ContentManager content { get; private set; }
        public static SpriteFont DefaultTextFont { get; private set; }
        public static readonly Vector2 renderTargetSize = new(800f, 450f);
        //if save files are affiliated
        public static bool FormalPlaytestRun { get; private set; }

        internal static float VisibleWidth;
        internal static float VisibleHeight;

        public static float ScreenToNormalResolutionRatio = 1;
        public const int PixelsInTile = 8;
        public const float PixelSize = 1f / PixelsInTile;
        internal static readonly float TileSize = 24;
        internal static int Width { get; private set; }
        internal static int Height { get; private set; }
        internal static float ElapsedTime { get; private set; }
        internal static float RawElapsedTime { get; private set; }
        internal static readonly float TimePace = 1;
        internal static ulong TimeInTicks;
        internal static double FPS { get; private set; }
        internal static float TimeOneFrame = 1f / 60;
        public static bool Paused;

        public static KeyboardState keyState { get; private set; }
        public static MouseState mouseState { get; private set; }
        public static GamePadState gamePadState { get; private set; }

        private static GraphicsDeviceManager _graphics;
        private static SpriteBatch _spriteBatch;
        private static SpriteFont _spriteFont;

        private static GameState CurrentState;
        private static GameState NextState;

        public static MenuState menuState { get; private set; }
        public static GameRunningState gameRunningState { get; private set; }
        public static PausedState pausedState { get; private set; }

        public static void ChangeState(GameState state)
        {
            CurrentState.End();
            NextState = state;
        }

        public static void SetScreenToNormalResolutionRatio(float Ratio)
        {
            SetWindowSize((int)(renderTarget2D.Width * Ratio), (int)(renderTarget2D.Height * Ratio));
        }

        public static void SetWindowSize(int Width, int Height)
        {
            _graphics.PreferredBackBufferWidth = Width;
            _graphics.PreferredBackBufferHeight = Height;
            _graphics.ApplyChanges();
        }

        public GamePanel()
        {
            Width = Window.ClientBounds.Width;
            Height = Window.ClientBounds.Height;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Instance = this;
        }

        protected override void Initialize()
        {
            DeviceInput.Initialize();
            renderTarget2D = new(GraphicsDevice, (int)renderTargetSize.X, (int)renderTargetSize.Y);
            secondRenderTarget2D = new(GraphicsDevice, (int)renderTargetSize.X, (int)renderTargetSize.Y);
            SetWindowSize((int)renderTargetSize.X, (int)renderTargetSize.Y);
            graphicsDevice = GraphicsDevice;

            Events.Initialize();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteFont = Content.Load<SpriteFont>("DefaultTextFont");
            DefaultTextFont = Content.Load<SpriteFont>("DefaultTextFont");

            content = Content;

            //first load engine integrated content
            GraphicsComponent.LoadBlankRectangle();
            Tile.LoadTileGraphics(Content);

            //then load custom content
            GameRunningState.Initializer.LoadAllContent(Content);

            //TEST SECTION
            //AudioHandler.LoadSoundEffect("music/2responseComplex", 2);
            //AudioHandler.Play(2);
            //uiHandler.AddUI(d, true);
            //set initial state to menustate
            menuState = new MenuState(this, graphicsDevice, Content);
            gameRunningState = new GameRunningState(this, graphicsDevice, Content);
            pausedState = new PausedState(this, graphicsDevice, Content);

Resources.TryImportSave("save_1", out var save1); GameRunningState.Initializer.BuildWorld(GameRunningState.World, Content); Resources.LoadSave(save1);
            ///REMOVE LATER
            CurrentState = gameRunningState;
        }

        protected override void Update(GameTime gameTime)
        {
            try
            {
                Width = Window.ClientBounds.Width;
                Height = Window.ClientBounds.Height;

                VisibleWidth = renderTargetSize.X / TileSize;
                VisibleHeight = renderTargetSize.Y / TileSize;

                RawElapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                ElapsedTime = RawElapsedTime * TimePace;

                GetInput();
                DeviceInput.Update();
                AudioHandler.Update(gameTime);

                if (NextState != null)
                {
                    CurrentState = NextState;
                    NextState = null;
                }

                CurrentState.Update(gameTime);
                CurrentState.PostUpdate(gameTime);
/*
                if (GameRunningState.player.controlsOn && DeviceInput.keyboardInfo.Pressed(Keys.Escape))
                {
                    if (CurrentState == gameRunningState)
                    {
                        pausedState.Reset();
                        NextState = pausedState;
                    }
                    else if (CurrentState == pausedState)
                        NextState = gameRunningState;
                    return;
                }*/

                //if (DeviceInput.keyboardInfo.Pressed(Keys.OemTilde))
                //{
                  //  GameRunningState.console.Enabled = !GameRunningState.console.Enabled;
                   // GameRunningState.console.Visible = GameRunningState.console.Enabled;
                //}

                TimeOneFrame = (float)gameTime.ElapsedGameTime.TotalSeconds;
                FPS = 1 / TimeOneFrame;
                TimeInTicks++;
                base.Update(gameTime);
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                Debug.WriteLine(s);
                File.WriteAllText("ErrorLog.txt", s);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            CurrentState.Draw(gameTime, _spriteBatch);

            base.Draw(gameTime);
        }

        static void GetInput()
        {
            keyState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            //gamePadState = GamePad.GetState();
        }
    }
}
