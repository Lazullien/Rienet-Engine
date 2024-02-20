using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using Atelo;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System.Text.Json;
using System.Data.Common;
using System.Text.Json.Nodes;

namespace Rienet
{
    //organize this 
    public class GamePanel : Game
    {
        public string Title;
        public Version _version;
        public static GamePanel Instance;
        public static RenderTarget2D renderTarget2D;
        public static RenderTarget2D renderTargetOnPause;
        public static GraphicsDevice graphicsDevice;
        public static ContentManager content;
        public static readonly Vector2 renderTargetSize = new(800f, 480f);
        //if save files are affiliated
        public static bool FormalPlaytestRun;

        internal static float VisibleWidth;
        internal static float VisibleHeight;

        public static float ScreenToNormalResolutionRatio = 1;
        public const int PixelsInTile = 8;
        public const float PixelSize = 1f / PixelsInTile;
        internal static readonly float TileSize = 24;
        internal static int Width, Height;
        internal static float ElapsedTime { get; private set; }
        internal static float RawElapsedTime { get; private set; }
        internal static readonly float TimePace = 1;
        internal static ulong TimeInTicks;
        internal static double FPS;
        internal static float TimeOneFrame = 1f / 60;
        public static bool Paused;

        public static KeyboardState keyState;
        public static MouseState mouseState;
        public static GamePadState gamePadState;

        public static WorldBody World;
        public static UIHandler uiHandler;
        public static Scene scene;
        public static DevConsole console;

        //MAYBE TO REMOVE
        public static Player pl;
        public static Camera cam;
        public static StatusBar bar;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;

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
            World = new WorldBody();
            uiHandler = new UIHandler();
            renderTarget2D = new(GraphicsDevice, (int)renderTargetSize.X, (int)renderTargetSize.Y);
            graphicsDevice = GraphicsDevice;
            console = new(Vector2.Zero, renderTargetSize, false);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteFont = Content.Load<SpriteFont>("DefaultTextFont");

            content = Content;

            //first load engine integrated content
            GraphicsComponent.LoadBlankRectangle();
            Tile.LoadTileGraphics(Content);

            //then load custom content
            Initializer.LoadAllContent(Content);
            Initializer.BuildWorld(World, Content);

            uiHandler.AddUI(console, true);

            pl = new Player(World.Scenes[1]);

            cam = new Camera(new Vector2(0, 0), new Vector2(35, 35), World.Scenes[1], World, this) { LockOnEntity = pl, LockOn = true };
            bar = new StatusBar(pl, true);
            //var d = new Dialogue(false, Vector2.One, new(400, 400), Vector2.One, new(15, 15), false, null, new() { "Hi" });
            //_ = new DialogueTrigger(World.Scenes[1], new(13, 8), new(2, 2), d);
            uiHandler.AddUI(bar, false);
            //uiHandler.AddUI(d, true);
        }

        protected override void Update(GameTime gameTime)
        {
            Width = Window.ClientBounds.Width;
            Height = Window.ClientBounds.Height;

            VisibleWidth = renderTargetSize.X / TileSize;
            VisibleHeight = renderTargetSize.Y / TileSize;

            RawElapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            ElapsedTime = RawElapsedTime * TimePace;

            GetInput();
            DeviceInput.Update();

            if (DeviceInput.keyboardInfo.Pressed(Keys.Escape))
            {
                Paused = !Paused;
                return;
            }

            if (!Paused)
                Run();

            if (DeviceInput.keyboardInfo.Pressed(Keys.OemTilde))
            {
                console.Enabled = !console.Enabled;
                console.Visible = console.Enabled;
            }

            TimeOneFrame = (float)gameTime.ElapsedGameTime.TotalSeconds;
            FPS = 1 / TimeOneFrame;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(default, BlendState.NonPremultiplied, SamplerState.PointClamp, default, default, default, default);
            GraphicsDevice.SetRenderTarget(renderTarget2D);
            cam.ProjectToScreen(_spriteBatch);
            uiHandler.Draw(_spriteBatch);
            _spriteBatch.End();

            _spriteBatch.Begin(default, BlendState.NonPremultiplied, SamplerState.PointClamp, default, default, default, default);
            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Draw(renderTarget2D, Vector2.Zero, null, Color.White, 0, Vector2.Zero, ScreenToNormalResolutionRatio, SpriteEffects.None, 0);
            _spriteBatch.DrawString(_spriteFont, pl.X + "," + pl.Y, new Vector2(0, 0), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        public void DrawContent()
        {

        }

        new static void Run()
        {
            uiHandler.Update();
            cam.Scene.Update();
            cam.Update();

#region Custom
            Events.Update();
#endregion

            renderTargetOnPause = renderTarget2D;
            TimeInTicks++;
        }

        static void Pause()
        {

        }

        static void GetInput()
        {
            keyState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            //gamePadState = GamePad.GetState();
        }

        public static class Initializer
        {
            public static void LoadAllContent(ContentManager Content)
            {
                Player.LoadSprite(Content);
                Player.Blooddrop.LoadSprite(Content);
                Player.BurstBody.LoadSprite(Content);
                WaterBlob.LoadSprite(Content);
                FloatingBlob.LoadSprite(Content);
                Noid.LoadSprite(Content);
                SnipingBlob.LoadSprite(Content);
                SnipingBlob.Spit.LoadSprite(Content);
                Layla.LoadSprite(Content);

                GetDash.LoadSprite(Content);
                GetSil.LoadSprite(Content);

                Dialogue.LoadSprite(Content);
                StatusBar.LoadSprite(Content);

                //ALWAYS IMPORT THIS THE SAME ORDER AS YOUR TILED TILESETS
                if (Resources.TryLoadTileset("Content/tilesets/complex.json", Content, out var tileset))
                    Resources.tilesets.Add(tileset);
                if (Resources.TryLoadTileset("Content/tilesets/spike.json", Content, out var tileset1))
                    Resources.tilesets.Add(tileset1);
            }

            //LOAD EACH ROOM INDIVIDUALLY UPON THE PLAYER REACHING THE LOCATION, THE PLAYER'S SCENE ID TRANSFER IS A SIGNAL
            //TO LOAD THAT CORRESPONDING ROOM, ENEMIES DO NOT FOLLOW PLAYERS THROUGH ROOMS UNLESS ON SPECIAL CONDITIONS
            public static void BuildWorld(WorldBody World, ContentManager Content)
            {
                foreach (var scene in World.Scenes.Values)
                {
                    foreach(var e in scene.EntitiesInScene)
                        e.OnDestruction();
                    scene.EntitiesInScene.Clear();
                    foreach (var b in scene.BodiesInScene)
                        scene.TryRemoveBody(b);
                    scene.BodiesInScene.Clear();
                    scene.HitboxChunks.Clear();
                }

                Resources.TryLoadRoom("Content/rooms/complex/complex_1.json", World, Content, out Room R1);
                Resources.TryLoadRoom("Content/rooms/complex/complex_2.json", World, Content, out Room R2);
                Resources.TryLoadRoom("Content/rooms/complex/complex_3.json", World, Content, out Room R3);
                Resources.TryLoadRoom("Content/rooms/complex/complex_4.json", World, Content, out Room R4);
                Resources.TryLoadRoom("Content/rooms/complex/complex_5.json", World, Content, out Room R5);
                Resources.TryLoadRoom("Content/rooms/complex/complex_6.json", World, Content, out Room R6);
                Resources.TryLoadRoom("Content/rooms/complex/complex_7.json", World, Content, out Room R7);
                Resources.TryLoadRoom("Content/rooms/complex/complex_8.json", World, Content, out Room R8);
                Resources.TryLoadRoom("Content/rooms/complex/complex_9.json", World, Content, out Room R9);
                Resources.TryLoadRoom("Content/rooms/complex/complex_10.json", World, Content, out Room R10);
                Resources.TryLoadRoom("Content/rooms/complex/complex_11.json", World, Content, out Room R11);
                Resources.TryLoadRoom("Content/rooms/complex/complex_12.json", World, Content, out Room R12);
                Resources.TryLoadRoom("Content/rooms/complex/complex_13.json", World, Content, out Room R13);
                Resources.TryLoadRoom("Content/rooms/complex/complex_14.json", World, Content, out Room R14);
                Resources.TryLoadRoom("Content/rooms/complex/complex_15.json", World, Content, out Room R15);
                Resources.TryLoadRoom("Content/rooms/complex/complex_16.json", World, Content, out Room R16);
                Resources.TryLoadRoom("Content/rooms/complex/complex_17.json", World, Content, out Room R17);
                Resources.TryLoadRoom("Content/rooms/complex/complex_18.json", World, Content, out Room R18);
                Resources.TryLoadRoom("Content/rooms/complex/complex_19.json", World, Content, out Room R19);
                Resources.TryLoadRoom("Content/rooms/complex/complex_20.json", World, Content, out Room R20);
                Resources.TryLoadRoom("Content/rooms/complex/complex_21.json", World, Content, out Room R21);
                //btw cache the original room state
                World.Scenes.Add(R1.ID, R1);
                World.Scenes.Add(R2.ID, R2);
                World.Scenes.Add(R3.ID, R3);
                World.Scenes.Add(R4.ID, R4);
                World.Scenes.Add(R5.ID, R5);
                World.Scenes.Add(R6.ID, R6);
                World.Scenes.Add(R7.ID, R7);
                World.Scenes.Add(R8.ID, R8);
                World.Scenes.Add(R9.ID, R9);
                World.Scenes.Add(R10.ID, R10);
                World.Scenes.Add(R11.ID, R11);
                World.Scenes.Add(R12.ID, R12);
                World.Scenes.Add(R13.ID, R13);
                World.Scenes.Add(R14.ID, R14);
                World.Scenes.Add(R15.ID, R15);
                World.Scenes.Add(R16.ID, R16);
                World.Scenes.Add(R17.ID, R17);
                World.Scenes.Add(R18.ID, R18);
                World.Scenes.Add(R19.ID, R19);
                World.Scenes.Add(R20.ID, R20);
                World.Scenes.Add(R21.ID, R21);
            }
        }
    }
}
