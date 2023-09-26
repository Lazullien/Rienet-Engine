using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public class Tile : IBodyVenue
    {
        public int ID;
        public float friction;
        public Vector2 pos;
        public PhysicsBody body;
        public Vector2 BodyTextureDifference;
        public Vector2 DrawBox = Vector2.One;
        public Scene BelongedScene;
        public GraphicsComponent Graphics;
        public bool hasSource;
        public Rectangle Source;

        //IF THESE ACTIONS WISH TO OVERRIDE TILE METHODS, JUST ADD RETURN
        public Action onCollision;
        public Action onCreation;
        public Action update;
        public Action onDestruction;
        public Action draw;

        public static Texture2D mono;

        //default
        internal static void LoadTileGraphics(ContentManager Content)
        {
            mono = Content.Load<Texture2D>("TileTextures/ExampleTile");
        }

        public Tile(Vector2 pos, GraphicsComponent Graphics, PhysicsBody Body, Scene BelongedScene)
        {
            this.pos = pos;
            this.BelongedScene = BelongedScene;

            this.Graphics = Graphics ?? new Image(Vector2.Zero, Vector2.Zero, mono);
            body = Body;
        }

        //default body
        public Tile(Vector2 pos, GraphicsComponent Graphics, Scene BelongedScene)
        {
            this.pos = pos;
            this.BelongedScene = BelongedScene;
            this.Graphics = Graphics ?? new Image(Vector2.Zero, Vector2.Zero, mono);

            DrawBox = new Vector2(1, 1);
            friction = 0.02f;
            SetDrawPosInWorld(new(X, Y + DrawBox.Y));
            body = new(this, pos, new Vector2(1, 1), new() { new Hitbox(pos.X, pos.Y, 1, 1) }, 0, 0.02f, true, BelongedScene);
        }

        protected void SetDrawPosInWorld(Vector2 Pos) => DrawPosInWorld = Pos;

        public virtual void OnCollision(PhysicsBody Target)
        {
            onCollision?.Invoke();
            //apply friction perpendicular to target pressure
            if (Target.Velocity != Vector2.Zero)
            {
                Target.TotalFriction += Kinetics.GetFrictionFromPressureAndCoefficient(Target.Velocity, friction);
                Target.momentumPotential -= friction;
                if (Target.momentumPotential < 0)
                    Target.momentumPotential = 0;
            }
        }

        public virtual void OnCreation()
        {
            onCreation?.Invoke();
        }

        public virtual void Update()
        {
            update?.Invoke();
            body.Update();
        }

        public virtual void OnDestruction()
        {
            onDestruction?.Invoke();
        }

        public virtual void Draw(Vector2 CenterPos, SpriteBatch spriteBatch, GamePanel gamePanel)
        {
            draw?.Invoke();

            //if has source then graphics must be a spritesheet or higher level
            if (hasSource)
            {
                Renderer.DrawSpriteInSheet((SpriteSheet)Graphics, DrawPosInWorld, CenterPos, spriteBatch);
            }
            else
            {
                if (Graphics is SpriteSheet spriteSheet)
                    Renderer.DrawSpriteInSheet(spriteSheet, DrawPosInWorld, CenterPos, spriteBatch);
                else if (Graphics is Image image)
                    Renderer.DrawImage(image, DrawPosInWorld, CenterPos, spriteBatch);
                else if (Graphics != null)
                    Renderer.DrawComponent(Graphics, DrawPosInWorld, CenterPos, spriteBatch);
            }
        }

        public int layer
        {
            get { return body.layer; }
            set { body.layer = value; }
        }

        public float X
        {
            get { return pos.X; }
            set { pos.X = value; }
        }

        public float Y
        {
            get { return pos.Y; }
            set { pos.Y = value; }
        }

        public float Width
        {
            get { return DrawBox.X; }
            set { DrawBox.X = value; }
        }

        public float Height
        {
            get { return DrawBox.Y; }
            set { DrawBox.Y = value; }
        }

        public float HitboxWidth
        {
            get { return body.size.X; }
            set { body.size.X = value; }
        }

        public float HitboxHeight
        {
            get { return body.size.Y; }
            set { body.size.Y = value; }
        }

        public Vector2 HitboxSize
        {
            get { return body.size; }
            set { body.size = value; }
        }

        public Vector2 VelocityForced
        {
            get { return body.VelocityForced; }
            set { body.VelocityForced = value; }
        }

        public Vector2 VelocityIgnoringFriction
        {
            get { return body.VelocityIgnoringFriction; }
            set { body.VelocityIgnoringFriction = value; }
        }

        public Vector2 PotentialVelocity
        {
            get { return body.PotentialVelocity; }
            set { body.PotentialVelocity = value; }
        }

        public Vector2 MainForcePoint
        {
            get { return body.MainForcePoint; }
            set { body.MainForcePoint = value; }
        }

        public Vector2 DrawPosInWorld
        {
            get { return new Vector2(X, Y + DrawBox.Y); }
            set { pos = value - new Vector2(0, DrawBox.Y); }
        }
    }

    //the tileset isn't a tile itself, it mere holds "classes" of similar
    //tile behaviors, these behaviors aren't tile objects, but mere scripts
    //they need to be "instantiated" to use
    //try to make this directly importable from tiled,
    public class Tileset
    {
        static int HighestID;
        //highest id each tileset has in one list
        //this is to help determine which tileset a given id belongs to
        public static List<int> HighestIDsOfTilesets = new();
        //minimum id of a certain tileset
        public int BaseID { get; private set; }
        //maximum id of a certain tileset
        public int TopID { get; private set; }
        public SpriteSheet Graphics;
        public TileType[] types;

        public Tileset(TileType[] types, SpriteSheet Graphics, int BaseID, int TopID)
        {
            this.types = types;
            this.Graphics = Graphics;
            this.BaseID = BaseID;
            this.TopID = TopID;
        }

        //editing all tiles of a tileset
        public delegate void EditAll();

        //this is in tiled format, all tiles of a tileset is uniform in imagesize
        public static Tileset LoadTileset(string path, ContentManager Content)
        {
            //read from json file data, also get id info based on splitting
            //the tile by size
            string json = File.ReadAllText(path);
            var w = JsonSerializer.Deserialize<TilesetWrapper>(json);
            int height = w.imageheight / w.tileheight;
            int width = w.imagewidth / w.tilewidth;

            var texture = Content.Load<Texture2D>(w.image);
            SpriteSheet Graphics = new(Vector2.Zero, Vector2.Zero, texture, w.tilewidth, w.tileheight);

            var types = new TileType[w.tilecount];
            for (int i = 0; i < types.Length; i++)
            {
                //for some reason the tile we're getting is null
                TileType tile = new();
                var wrapper = w.tiles[i];
                //it seems like i have to -1 to get the right ID, WELL I MEAN
                //CURRENTLY THIS WORKS SO I'M REALLY GONNA DO THAT LATER
                int rela = wrapper.id - HighestID;
                tile.id = wrapper.id;
                tile.Graphics = Graphics;
                tile.Source = new(w.tilewidth * rela / width, w.tileheight * rela % width, w.tilewidth, w.tileheight);
                tile.baseSet = w.name;
                types[i] = tile;
            }

            var set = new Tileset(types, Graphics, HighestID + 1, HighestID + w.tilecount);

            HighestIDsOfTilesets.Add(HighestID + w.tilecount);

            //LATER CHECK IF THIS FITS THE ZERO RULE IF NOT ADJUST IT
            HighestID += w.tilecount;

            return set;
        }
    }

    public class TilesetWrapper
    {
        public int columns { get; set; }
        public string image { get; set; }
        public int imageheight { get; set; }
        public int imagewidth { get; set; }
        public int margin { get; set; }
        public string name { get; set; }
        public int spacing { get; set; }
        public int tilecount { get; set; }
        public string tiledversion { get; set; }
        public int tileheight { get; set; }
        public TileWrapper[] tiles { get; set; }
        public int tilewidth { get; set; }
        public string type { get; set; }
        public string version { get; set; }
    }

    public class TileWrapper
    {
        public int id { get; set; }
        public string type { get; set; }
    }

    public class TileType
    {
        public int id;
        public string type;
        public string baseSet;
        public GraphicsComponent Graphics;
        public Rectangle Source;
    }

    /*
    { "columns":28,
    "image":"..\/Tilesets\/complex.png",
    "imageheight":80,
    "imagewidth":224,
    "margin":0,
    "name":"complex",
    "spacing":0,
    "tilecount":280,
    "tiledversion":"1.10.1",
    "tileheight":8,
    "tiles":[
        {
            "id":1,
            "type":"class"
        }
    ]
    "tilewidth":8,
    "type":"tileset",
    "version":"1.10"
    }
    */
}