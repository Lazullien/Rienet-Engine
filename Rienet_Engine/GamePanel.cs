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
    public class GamePanel : Game
    {
        public string Title;
        public Version _version;
        public static GamePanel Instance;

        internal static float VisibleWidth;
        internal static float VisibleHeight;

        public const int PixelsInTile = 8;
        internal static readonly int TileSize = 24;
        internal static int Width, Height;
        internal static float ElapsedTime { get; private set; }
        internal static float RawElapsedTime { get; private set; }
        internal static readonly float TimePace = 1;
        internal static ulong TimeInTicks;
        internal static double FPS;
        internal static float TimeOneFrame = 1f / 60;

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
            Initializer.BuildWorld(World, Content);
            pl = new Player(World.Scenes[1]);
            var pawn = new Pawn(World.Scenes[2], 1, 0.2f);
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

            VisibleWidth = (float)Width / TileSize;
            VisibleHeight = (float)Height / TileSize;

            GetInput();
            DeviceInput.Update();

            uiHandler.Update();

            cam.Scene.Update();
            cam.Update();

            TimeInTicks++;
            TimeOneFrame = (float)gameTime.ElapsedGameTime.TotalSeconds;
            FPS = 1 / TimeOneFrame;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, default);

            uiHandler.Draw(_spriteBatch);
            //GraphicsRenderer.DrawHitbox(new Hitbox(16,15,1,1), cam.pos, cam.Scene, _spriteBatch, this, cam.blankRect);

            cam.ProjectToScreen(_spriteBatch);
            _spriteBatch.DrawString(_spriteFont, pl.X + "," + pl.Y, new Vector2(0, 0), Color.White);
            _spriteBatch.DrawString(_spriteFont, TimeInTicks.ToString(), new Vector2(0, 20), Color.Yellow);
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

        public static class Initializer
        {
            public static void LoadAllContent(ContentManager Content)
            {
                Player.LoadSprite(Content);
                Player.Blooddrop.LoadSprite(Content);
                Pawn.LoadSprite(Content);

                if (Importer.TryLoadTileset("Content/tileSets/complex.json", Content, out var tileset))
                    Resources.tilesets.Add(tileset);

                //load the resources with no belonging classes here
                Resources.damageBlob = Content.Load<Texture2D>("textures/damageBlob");
            }

            public static void BuildWorld(WorldBody World, ContentManager Content)
            {
                Importer.TryLoadRoom("Content/Rooms/waterlogged_complex/waterlogged_complex_1.json", World, Content, out Room R1);
                Importer.TryLoadRoom("Content/Rooms/waterlogged_complex/waterlogged_complex_2.json", World, Content, out Room R2);
                Importer.TryLoadRoom("Content/Rooms/waterlogged_complex/waterlogged_complex_3.json", World, Content, out Room R3);
                Importer.TryLoadRoom("Content/Rooms/waterlogged_complex/waterlogged_complex_4.json", World, Content, out Room R4);
                Importer.TryLoadRoom("Content/Rooms/waterlogged_complex/waterlogged_complex_5.json", World, Content, out Room R5);
                //btw cache the original room state
                World.Scenes.Add(R1.ID, R1);
                World.Scenes.Add(R2.ID, R2);
                World.Scenes.Add(R3.ID, R3);
                World.Scenes.Add(R4.ID, R4);
                World.Scenes.Add(R5.ID, R5);
            }
        }

        public class Resources
        {
            public static Texture2D damageBlob;
            public static readonly List<Tileset> tilesets = new();

            public static Tile ToTile(TileType type, Vector2 pos, Scene scene)
            {
                //this is just a switch statement, but somehow i feel incredibly tired to do this
                //the type specifies the tileset it belongs to, while the id
                //specifies its individual 
                Tile result = default;

                switch (type.baseSet)
                {
                    case "complex":

                        //assign stuff
                        result = new Tile(pos, type.Graphics, scene);

                        break;
                }

                switch (type.id)
                {
                    //too lazy to assign behavior
                }

                if (result != null)
                {
                    result.ID = type.id;
                    result.Graphics = type.Graphics;
                    result.Source = type.Source;
                    result.hasSource = true;
                }

                return result;
            }

            public static bool TryGetTileType(int ID, Vector2 pos, Scene Scene, out TileType type)
            {
                type = default;

                foreach (var set in tilesets)
                    if (ID >= set.BaseID && ID <= set.TopID)
                    {
                        //identify as set and get info from this set
                        type = set.types[ID - set.BaseID];
                        return true;
                    }

                return false;
            }
        }

        public static class Importer
        {
            public static bool TryLoadEntity(string path, out Entity entity)
            {
                entity = default;
                return true;
            }

            public static bool TryLoadTileset(string path, ContentManager Content, out Tileset tileset)
            {
                tileset = default;

                try
                {
                    tileset = Tileset.LoadTileset(path, Content);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    return false;
                }

                return true;
            }

            static bool TryLoadBackground(BackgroundWrapper wrapper, Scene scene, ContentManager Content, out Background background)
            {
                background = default;

                try
                {
                    List<Layer> layers = new();
                    for (int i = 0; i < wrapper.GraphicsLayers.Length; i++)
                    {
                        var item = wrapper.GraphicsLayers[i];
                        var tex = Content.Load<Texture2D>(item.Texture);
                        Vector2 pos = new(item.X, item.Y);
                        var dss = item.DisplacementScrollSpeed;
                        Vector2 ssv = new(item.SelfScrollVelocityX, item.SelfScrollVelocityY);
                        Vector2 maxDelta = new(item.MaxDisplacementX, item.MaxDisplacementY);
                        Vector2 minDelta = new(item.MinDisplacementX, item.MinDisplacementY);
                        layers.Add(new(tex, pos, dss, ssv, maxDelta, minDelta));
                    }

                    background = new(layers, scene)
                    {
                        pos = new(wrapper.X, wrapper.Y)
                    };
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    return false;
                }

                return true;
            }

            //json only
            public static bool TryLoadRoom(string ScenePath, WorldBody world, ContentManager Content, out Room room)
            {
                room = default;

                try
                {
                    string json = File.ReadAllText(ScenePath);
                    var wrapper = JsonSerializer.Deserialize<RoomWrapper>(json);

                    //convert the wrapper into room info
                    //start by instantiating the room
                    room = new(wrapper.ID, wrapper.depth, wrapper.width, wrapper.height, world) { ID = wrapper.ID };
                    if (wrapper.specifiedCamMobility)
                    {
                        room.SetCameraMobilityPos(wrapper.camMobilityX, wrapper.camMobilityY);
                        room.SetCameraMobilitySize(wrapper.camMobilityWidth, wrapper.camMobilityHeight);
                    }
                    if (wrapper.specifiedBackground && TryLoadBackground(wrapper.background, room, Content, out var background))
                    {
                        room.BG = background;
                    }
                    //adding transitions and entities
                    foreach (float[] t in wrapper.transitions)
                    {
                        room.Transitions.Add(new(new(t[0], t[1], t[2], t[3]), (int)t[4], new Vector2(t[5], t[6]), (int)t[7], new Vector2(t[8], t[9]), room));
                    }

                    for (int Z = 0; Z < wrapper.depth; Z++)
                    {
                        Tile[,] LayerMap = room.GetLayerMap(Z);
                        for (int Y = 0; Y < wrapper.height; Y++)
                        {
                            for (int X = 0; X < wrapper.width; X++)
                            {
                                var pos = new Vector2(X, wrapper.height - Y - 1);
                                if (Resources.TryGetTileType(wrapper.map[Z][Y][X], pos, room, out var t))
                                {
                                    Tile tile = Resources.ToTile(t, pos, room);
                                    if (tile != null)
                                    {
                                        tile.layer = Z - wrapper.mainlayer + 1;
                                        LayerMap[X, wrapper.height - Y - 1] = tile;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    return false;
                }

                return true;
            }

            public static bool TryLoadEntities(string entities)
            {
                return true;
            }
        }

        class RoomWrapper
        {
            public int ID { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int depth { get; set; }
            public int mainlayer { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public bool specifiedCamMobility { get; set; }
            public float camMobilityX { get; set; }
            public float camMobilityY { get; set; }
            public float camMobilityWidth { get; set; }
            public float camMobilityHeight { get; set; }
            public bool specifiedBackground { get; set; }
            public BackgroundWrapper background { get; set; }
            public float[][] transitions { get; set; }
            public int[][][] map { get; set; }
        }

        class BackgroundWrapper
        {
            public float X { get; set; }
            public float Y { get; set; }
            public LayerWrapper[] GraphicsLayers { get; set; }
        }

        class LayerWrapper
        {
            public string Texture { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public float DisplacementScrollSpeed { get; set; }
            public float SelfScrollVelocityX { get; set; }
            public float SelfScrollVelocityY { get; set; }
            public float MaxDisplacementX { get; set; }
            public float MaxDisplacementY { get; set; }
            public float MinDisplacementX { get; set; }
            public float MinDisplacementY { get; set; }
        }

        /**
        format:
        {
            //"ID":XX
            "X":XX,
            "Y":XX,
            "width":XX,
            "height":XX,
            "specifiedCamMobility":true,
            "camMobilityX": 0,
            "camMobilityY": 0,
            "camMobilityWidth": 28,
            "camMobilityHeight": 10,
            "specifiedBackground":true,
            "background":
            {
                "X":,
                "Y":,
                "GraphicsLayers":[
                    {
                        "Texture":"",
                        "X":,
                        "Y":,
                        "DisplacementScrollSpeed":,
                        "SelfScrollVelocityX":
                        "SelfScrollVelocityY":
                        "MaxDisplacementX":,
                        "MaxDisplacementY":,
                        "MinDisplacementX":,
                        "MinDisplacementY":
                    }
                ]
            }
            <or null if "specifiedBackground":false>
            "transitions":[
                x,x,x,x,x,x
            ],
            "map":[
                [X, X],
                [X, X],
            ],
        }
        **/
    }
}
