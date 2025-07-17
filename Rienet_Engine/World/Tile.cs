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
        public int ID { get; set; }
        public float friction { get; set; }
        public Vector2 pos;
        public int layerRelativeToBody { get; set; }
        /// <summary>
        /// 1 on default (to innately account distance of travelling it), more would mean this tile is actually hard to travel through
        /// </summary>
        public int travelCost { get; protected set; } = 1;
        public PhysicsBody body { get; set; }
        public Vector2 BodyTextureDifference;
        public Vector2 DrawBox = Vector2.One;
        public Scene BelongedScene { get; set; }
        public GraphicsComponent Graphics { get; set; }
        /// <summary>
        /// used for lighting, IMPLEMENT LAYER TODO
        /// </summary>
        public Texture2D NormalMap { get; set; }
        public List<GraphicsComponent> AssociatedGraphics { get; private set; } = new();
        public bool hasSource { get; set; }
        public Rectangle Source { get; set; }

        //IF THESE ACTIONS WISH TO OVERRIDE TILE METHODS, JUST ADD RETURN
        public Action onCollision { get; set; } = delegate { };
        public Action onCreation { get; set; } = delegate { };
        public Action update { get; set; } = delegate { };
        public Action onDestruction { get; set; } = delegate { };
        public Action draw { get; set; } = delegate { };

        public static Texture2D mono;

        //default
        internal static void LoadTileGraphics(ContentManager Content)
        {
            mono = Content.Load<Texture2D>("textures/tilesets/ExampleTile");
        }

        /// <summary>
        /// from t2 to t1
        /// </summary>
        public static double Distance(Tile t1, Tile t2)
        {
            return (t1.pos - t2.pos).Length();
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
            body = new(this, pos, new Vector2(1, 1), new() { new Hitbox(pos.X, pos.Y, 1, 1) }, 0, 0.01f, true, BelongedScene);
            body.FillOtherPosTypes();
        }

        protected void SetDrawPosInWorld(Vector2 Pos) => DrawPosInWorld = Pos;

        public virtual void OnCollision(PhysicsBody Target)
        {
            onCollision();
            //apply friction perpendicular to target pressure
            if (Target.Velocity != Vector2.Zero)
            {
                Target.TotalFriction += Dynamics.GetFrictionFromPressureAndCoefficient(Target.Velocity, friction);
                Target.momentumPotential -= friction;
                if (Target.momentumPotential < 0)
                    Target.momentumPotential = 0;
            }
        }

        public virtual void OnCreation()
        {
            onCreation();
        }

        public virtual void Update()
        {
            update();
            body.Update();
        }

        public virtual void OnDestruction()
        {
            onDestruction();
        }

        public virtual void Draw(Vector2 CenterPos, SpriteBatch spriteBatch, GamePanel gamePanel)
        {
            draw();

            //if has source then graphics must be a spritesheet or higher level
            if (hasSource)
            {
                GraphicsRenderer.DrawSpriteInSheet((SpriteSheet)Graphics, DrawPosInWorld, CenterPos, spriteBatch, Source);
            }
            else if (Graphics != null)
            {
                GraphicsRenderer.DrawComponent(Graphics, DrawPosInWorld, CenterPos, spriteBatch);
            }

            foreach (var g in AssociatedGraphics)
                GraphicsRenderer.DrawComponent(g, DrawPosInWorld, CenterPos, spriteBatch);
        }

        public virtual int nonRelativeLayerInScene
        {
            get { return body != null ? body.layer + layerRelativeToBody + BelongedScene.layerZeroPoint - 1 : layerRelativeToBody + BelongedScene.layerZeroPoint - 1; }
            set { body.layer = value - BelongedScene.layerZeroPoint + 1 - layerRelativeToBody; }
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
        static int HighestID = 0;
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
                TileType tile = new();
                var wrapper = w.tiles[i];
                //it seems like i have to -1 to get the right ID, WELL I MEAN
                //CURRENTLY THIS WORKS SO I'M REALLY GONNA DO THAT LATER
                tile.id = wrapper.id + HighestID + 1;
                int rela = wrapper.id;
                tile.relativeID = wrapper.id;
                tile.type = wrapper.type;
                tile.Graphics = Graphics;
                tile.Source = new(w.tilewidth * (int)Math.Floor((double)rela % width), w.tileheight * (int)Math.Floor((double)rela / width), w.tilewidth, w.tileheight);
                tile.baseSet = w.name;
                tile.properties = wrapper.properties;
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
        public PropertyWrapper[] properties { get; set; }
    }

    public class TileType
    {
        public int id;
        public int relativeID;
        public string type;
        public PropertyWrapper[] properties;
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

    /// <summary>
    /// A tile using entity properties so it can have drawing order sorted with entities, this is for tiles that represent for example a big object needing to show if an entity is in front or behind it, this one object can represent a large tile at one layer, whilst being sorted with other entities
    /// These SHOULD NOT exist in large quantities, or they cause lag, unless a manual update timer is added
    /// </summary>
    public class DimensionMimicTile : Entity
    {
        public int ID { get; set; }
        public float friction { get; set; }
        /// <summary>
        /// used for lighting, IMPLEMENT LAYER TODO
        /// </summary>
        public Texture2D NormalMap { get; set; }
        public bool hasSource { get; set; }
        public Rectangle Source { get; set; }

        //IF THESE ACTIONS WISH TO OVERRIDE TILE METHODS, JUST ADD RETURN
        public Action onCollision { get; set; } = delegate { };

        /// <summary>
        /// Because tiles usually have more than entities but usually stay static, add a timer to prevent unnecessary updates
        /// </summary>
        bool Updating;
        bool HasUpdateDelay = false;
        float TimeSinceUpdate = 0;
        float TimeBetweenUpdates = 0.1f;

        public DimensionMimicTile(Vector2 pos, GraphicsComponent Graphics, PhysicsBody Body, Scene BelongedScene, bool Updating, bool HasUpdateDelay, float TimeBetweenUpdates) : base(BelongedScene)
        {
            this.pos = pos;
            this.BelongedScene = BelongedScene;

            this.Graphics = Graphics ?? new Image(Vector2.Zero, Vector2.Zero, Tile.mono);
            body = Body;

            this.Updating = Updating;
            this.HasUpdateDelay = HasUpdateDelay;
            this.TimeBetweenUpdates = TimeBetweenUpdates;
        }

        //default body
        public DimensionMimicTile(Vector2 pos, GraphicsComponent Graphics, Scene BelongedScene, bool Updating, bool HasUpdateDelay, float TimeBetweenUpdates) : base(BelongedScene)
        {
            this.pos = pos;
            this.BelongedScene = BelongedScene;
            this.Graphics = Graphics ?? new Image(Vector2.Zero, Vector2.Zero, Tile.mono);

            DrawBox = new Vector2(1, 1);
            friction = 0.02f;
            SetDrawPosInWorld(new(X, Y + DrawBox.Y));
            body = new(this, pos, new Vector2(1, 1), new() { new Hitbox(pos.X, pos.Y, 1, 1) }, 0, 0.01f, true, BelongedScene);

            this.Updating = Updating;
            this.HasUpdateDelay = HasUpdateDelay;
            this.TimeBetweenUpdates = TimeBetweenUpdates;
        }

        protected void SetDrawPosInWorld(Vector2 Pos) => DrawPosInWorld = Pos;

        public override void OnCollision(PhysicsBody Target)
        {
            onCollision();
            //apply friction perpendicular to target pressure
            if (Target.Velocity != Vector2.Zero)
            {
                Target.TotalFriction += Dynamics.GetFrictionFromPressureAndCoefficient(Target.Velocity, friction);
                Target.momentumPotential -= friction;
                if (Target.momentumPotential < 0)
                    Target.momentumPotential = 0;
            }
        }

        public override void Update()
        {
            if (Updating)
            {
                if (HasUpdateDelay)
                {
                    TimeSinceUpdate += GamePanel.ElapsedTime;

                    if (TimeSinceUpdate > TimeBetweenUpdates)
                        base.Update();
                }
                else
                {
                    base.Update();
                }
            }
        }
    }

    /// <summary>
    /// not an actual tile, just holding its place, because sometimes algorithms requires a grid system with tile instance variables but some blocks don't have tiles, this will be what it checks instead, it can hold information similar to a tile, but doesn't need to actually exist and is only temporary
    /// </summary>
    public class TilePlace
    {
        public int ID { get; set; }
        public Vector2 pos;
        public int travelCost { get; protected set; } = 1;
        public bool hasBody { get; set; }
        public float friction { get; set; }
        public int absoluteLayer { get; set; }
        public Scene BelongedScene { get; set; }

        public TilePlace(int ID, Vector2 pos, int travelCost, bool hasBody, float friction, int absoluteLayer, Scene BelongedScene)
        {
            this.ID = ID;
            this.pos = pos;
            this.travelCost = travelCost;
            this.hasBody = hasBody;
            this.friction = friction;
            this.absoluteLayer = absoluteLayer;
            this.BelongedScene = BelongedScene;
        }

        public TilePlace(Vector2 pos, int absoluteLayer, bool hasBody, Scene BelongedScene)
        {
            this.pos = pos;
            this.absoluteLayer = absoluteLayer;
            this.hasBody = hasBody;
            this.BelongedScene = BelongedScene;
        }

        //only when there's for sure a tile
        public static TilePlace ToTilePlace(Tile tile)
        {
            return new TilePlace(tile.ID, tile.pos, tile.travelCost, tile.body != null, tile.friction, tile.nonRelativeLayerInScene, tile.BelongedScene);
        }

        //ensures only layer, position and belongedscene are varying, because other values may be irrelevant and interfering with equal checking
        public void Equalize()
        {
            ID = 0;
            travelCost = 1;
            friction = 0f;
        }
    }
}