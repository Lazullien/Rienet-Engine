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
    /*
    米娜桑扣你吉娃，人家是可可爱爱的木偶引擎哦，要说其他部分为器官，我可谓是我这完美肉身的颜面！
    家父年少无知毅然东施效颦造出的我，于是我可能有些小问题，不过无伤大雅
    我的功能还算完善的，各位绅士们心想的我皆可做到哦
    甚至家父当时对我望女成龙，加入了一些稍稍复杂的算法，所以大家应该是不需要出去找别的坏程序的，只有我就够啦！
    */
    public class GamePanel : Game
    {
        public string Title;
        public Version _version;
        public static GamePanel Instance { get; private set; }
        public static RenderTarget2D renderTarget2D { get; private set; }
        public static RenderTarget2D secondRenderTarget2D { get; private set; }
        public static Vector2 targetDrawPos { get; private set; }
        public static GraphicsDevice graphicsDevice { get; private set; }
        public static ContentManager content { get; private set; }
        public static SpriteFont DefaultLatinFont { get; private set; }
        public static SpriteFont DefaultSimplifiedChineseFont { get; private set; }
        public static SpriteFont DefaultTraditionalChineseFont { get; private set; }
        public static SpriteFont DefaultJapaneseFont { get; private set; }
        public static SpriteFont DefaultKoreanFont { get; private set; }
        public static SpriteFont CurrentFont { get; private set; }
        public static readonly Vector2 renderTargetSize = new(800f, 450f);
        //if save files are affiliated
        public static bool FormalPlaytestRun { get; private set; }

        internal static float VisibleWidth;
        internal static float VisibleHeight;

        public static float ScreenToNormalResolutionRatio { get; set; } = 1;
        public const int PixelsInTile = 8;
        public const float PixelSize = 1f / PixelsInTile;
        //preferable divisible by 8, or graphical glitches might happen due to rounding
        internal static readonly float TileSize = 16f;
        internal static int Width { get; private set; }
        internal static int Height { get; private set; }
        internal static float ElapsedTime { get; private set; }
        internal static float RawElapsedTime { get; private set; }
        internal static readonly float TimePace = 1;
        internal static ulong TimeInTicks;
        internal static double TimeInSeconds;
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

        private int widthBeforeFullScreen;
        private int heightBeforeFullScreen;

        public static Dictionary<int, Camera> CamerasOnDevice { get; private set; } = new();

        public static string[] overall { get; private set; }

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
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Resize;
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
            DefaultLatinFont = Content.Load<SpriteFont>("fonts/fusion-pixel-font-10px-monospaced-ttf-v2025.03.14/latin");
            DefaultSimplifiedChineseFont = Content.Load<SpriteFont>("fonts/fusion-pixel-font-10px-monospaced-ttf-v2025.03.14/zh_simplified");
            DefaultTraditionalChineseFont = Content.Load<SpriteFont>("fonts/fusion-pixel-font-10px-monospaced-ttf-v2025.03.14/zh_traditional");
            DefaultJapaneseFont = Content.Load<SpriteFont>("fonts/fusion-pixel-font-10px-monospaced-ttf-v2025.03.14/ja");
            DefaultKoreanFont = Content.Load<SpriteFont>("fonts/fusion-pixel-font-10px-monospaced-ttf-v2025.03.14/ko");

            //base this somewhere else
            CurrentFont = DefaultSimplifiedChineseFont;

            content = Content;

            //LOAD BASIC INFO OF THIS COPY OF THE GAME
            overall = File.ReadAllLines("saves/overall.at");

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

            Resources.TryImportSave("save_1", out var save1); Resources.LoadSave(GameRunningState.band, save1); GameRunningState.Initializer.BuildWorld(GameRunningState.World, Content);

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

                if (GameRunningState.player.controlsOn && DeviceInput.keyboardInfo.Pressed(Keys.Escape))
                {
                    if (CurrentState == gameRunningState)
                    {
                        pausedState.Reset();
                        NextState = pausedState;
                    }
                    else if (CurrentState == pausedState)
                    {
                        pausedState.End();
                        NextState = gameRunningState;
                    }
                    return;
                }

                //fullscreen toggle
                if (DeviceInput.keyboardInfo.Pressed(Keys.F11))
                {
                    _graphics.IsFullScreen = !_graphics.IsFullScreen;
                    if (_graphics.IsFullScreen)
                    {
                        widthBeforeFullScreen = Window.ClientBounds.Width;
                        heightBeforeFullScreen = Window.ClientBounds.Height;

                        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

                        _graphics.ApplyChanges();
                    }
                    else
                    {
                        _graphics.PreferredBackBufferWidth = widthBeforeFullScreen;
                        _graphics.PreferredBackBufferHeight = heightBeforeFullScreen;

                        _graphics.ApplyChanges();
                    }
                }

                //if (DeviceInput.keyboardInfo.Pressed(Keys.OemTilde))
                //{
                //  GameRunningState.console.Enabled = !GameRunningState.console.Enabled;
                // GameRunningState.console.Visible = GameRunningState.console.Enabled;
                //}

                TimeOneFrame = (float)gameTime.ElapsedGameTime.TotalSeconds;
                FPS = 1 / TimeOneFrame;
                TimeInSeconds += ElapsedTime;
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

        //propagates graphics to resized window (that is already resized by dragging or fullscreen toggle)
        //remember set position too
        protected void Resize(Object sender, EventArgs e)
        {
            //original graphics ratio is 16:9, for whichever axis that gains more weight in the new window ratio than presented in this ratio, propagate the corresponding graphic axis to its size

            //no point if nothing's displayed
            if (Window.ClientBounds.Height == 0)
                return;

            /*float ratio = (float)Window.ClientBounds.Width / Window.ClientBounds.Height;
            //x axis
            if (ratio > 16f / 9f)
            {
                ScreenToNormalResolutionRatio = Window.ClientBounds.Width / renderTargetSize.X;
            }*/
            //y axis (default
            //else
            //{
            ScreenToNormalResolutionRatio = Window.ClientBounds.Height / renderTargetSize.Y;
            //}

            //fit graphics to center
            targetDrawPos = 0.5f * (new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height) - (ScreenToNormalResolutionRatio * renderTargetSize));
        }

        /// <summary>
        /// call this to rewrite the code variable, it doesn't actually save the values to file upon game closing (or crash detection, somehow, as long as cases where closing the game is possible)
        /// <para >Also, to all solutions suggesting I read and rewrite the entire file, sincerely, fuck you. </para>
        /// </summary>
        public static void WriteToOverall(int variable, string value)
        {
            overall[variable] = overall[variable].Split(":")[0] + ":" + value;
        }

        /// <summary>
        /// On exiting the game, crashing, etc. Saves game data.
        /// </summary>
        protected override void OnExiting(Object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            File.WriteAllLines("saves/overall.at", overall);
        }
    }
}
