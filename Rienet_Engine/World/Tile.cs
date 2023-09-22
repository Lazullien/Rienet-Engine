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
        //this would be naturally made from the script, but if i'm not satisfied
        //i can always manually edit the tile behavior
        public TileContainer[] Tiles;
        public SpriteSheet Graphics;

        public Tileset(TileContainer[] Tiles, SpriteSheet Graphics)
        {
            this.Tiles = Tiles;
            this.Graphics = Graphics;
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
            int count = height * width;

            var texture = Content.Load<Texture2D>(path);
            SpriteSheet Graphics = new(Vector2.Zero, Vector2.Zero, texture, w.tilewidth, w.tileheight);

            var Tiles = new TileContainer[w.tiles.Length];
            for (int i = 0; i < Tiles.Length; i++)
            {
                var tile = Tiles[i];
                var wrapper = w.tiles[i];
                int rela = wrapper.id - HighestID;
                tile.wrapper = wrapper;
                tile.Graphics = Graphics;
                tile.IndexY = rela / width;
                tile.IndexX = rela % width;
                tile.width = w.tilewidth;
                tile.height = w.tileheight;
            }

            var set = new Tileset(Tiles, Graphics)
            {
                BaseID = HighestID + 1
            };
            HighestIDsOfTilesets.Add(HighestID + count);

            //LATER CHECK IF THIS FITS THE ZERO RULE IF NOT ADJUST IT
            HighestID += count;

            return set;
        }
    }

    public class TileContainer
    {
        public TileWrapper wrapper;
        public GraphicsComponent Graphics;
        public int IndexX;
        public int IndexY;
        public int width;
        public int height;
        public delegate Tile ToTile();

        public Rectangle SourceRect
        {
            get { return new(IndexX * width, IndexY * height, width, height); }
        }
    }

    public class TilesetWrapper
    {
        public int columns;
        public string image;
        public int imageheight;
        public int imagewidth;
        public int margin;
        public string name;
        public int spacing;
        public int tilecount;
        public string tiledversion;
        public int tileheight;
        public TileWrapper[] tiles;
        public int tilewidth;
        public string type;
        public string version;
    }

    public class TileWrapper
    {
        public int id;
        public string type;
        public TileBehavior[] behavior;
    }

    public class TileBehavior
    {
        public string type;
        public string callback;
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
            //the x, y here refers to pos relative to graphics, 
            //the width, height refers to the hitbox
            "type":"class, x, y, width, height"
            "behavior": [
                {
                    //type is condition, callback is action
                    "type": ,
                    "callback": 
                }
            ]
        }
    ]
    "tilewidth":8,
    "type":"tileset",
    "version":"1.10"
    }
    */
}