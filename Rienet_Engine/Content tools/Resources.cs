using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Atelo;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Rienet
{
    //your custom resource scripts lie here
    public class Resources
    {
        public static readonly List<Tileset> tilesets = new();

        public static Entity ToEntity(EntityWrapper wrapper, Scene scene)
        {
            Entity entity = null;
            string type = wrapper.type;

            switch (type)
            {
                case "WaterBlob":
                    entity = new WaterBlob(scene, Hitbox.Zero, 1);
                    break;
                case "FloatingBlob":
                    entity = new FloatingBlob(scene, Hitbox.Zero, 1);
                    break;
                case "SnipingBlob":
                    entity = new SnipingBlob(scene, 1);
                    break;
                case "Noid":
                    entity = new Noid(scene, Hitbox.Zero, 1);
                    break;
                case "Layla":
                    entity = new Layla(scene);
                    break;

                case "GetDash":
                    entity = new GetDash(scene);
                    break;
            }

            entity?.SetPos(new(wrapper.x, wrapper.y));

            return entity;
        }

        public static Tile ToTile(TileType type, Vector2 pos, Scene scene)
        {
            //this is just a switch statement, but somehow i feel incredibly tired to do this
            //the type specifies the tileset it belongs to, while the id
            //specifies its individual 
            Tile result = default;

            switch (type.baseSet)
            {
                case "complex":
                    result = new Tile(pos, type.Graphics, scene);
                    break;
                case "spike":
                    result = new Spike(pos, type.Graphics, type.relativeID, scene);
                    break;
            }

            switch (type.type)
            {
                //associate the light with the object
                case "LightSource": //scene.LightSources.Add(new LightSource(pos, Vector2.Zero, result)); 
                    float gamma = (float)((JsonElement)type.properties[0].value).GetDecimal();
                    float radius = (float)((JsonElement)type.properties[1].value).GetDecimal();
                    //AARRGGBB to RGBA
                    int argb = int.Parse(((JsonElement)type.properties[2].value).GetString().Replace("#", ""), System.Globalization.NumberStyles.HexNumber);
                    var c = System.Drawing.Color.FromArgb(argb);
                    Color color = new(c.R, c.G, c.B, c.A);
                    LightSource l = new(pos, Vector2.Zero, radius, gamma, color, scene, result);
                    result.AssociatedGraphics.Add(l);
                    scene.LightSources.Add(l);
                    break;
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

        public static bool TryLoadEntity(JsonObject obj, out Entity entity)
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
                    Transition tr = new(new(t[0], t[1], t[2], t[3]), (int)t[4], new Vector2(t[5], t[6]), room);
                    tr.PreTransition = delegate (Entity e)
                    {
                        if (e is Player p && !p.InTransition)
                        {
                            p.KillOnFall = false;
                            //add blackening animated effect into room, and force the player to walk in the animation, and on end of animation call on transition
                            p.Exit.animator.OnEnd = delegate ()
                            {
                                p.KillOnFall = true;
                                GamePanel.cam.PlayAnimatedOverlay(p.BlackScreen);
                                tr.OnTransition(p);
                            };
                            GamePanel.cam.PlayAnimatedOverlay(p.Exit);
                            //player sth
                            p.InTransition = true;
                        }
                    };
                    tr.PostTransition = delegate (Entity e)
                    {
                        //auto walking out or jumping to designated area
                        if (e is Player p)
                        {
                            //add blackening animated effect into room, and force the player to walk in the animation, and on end of animation call on transition
                            p.Enter.animator.OnEnd = delegate ()
                            {
                                p.Enter.Reset();
                                p.InTransition = false;
                            };
                            GamePanel.cam.PlayAnimatedOverlay(p.Enter);
                            //player sth
                        }
                    };
                    room.Transitions.Add(tr);
                }

                foreach (EntityWrapper entity in wrapper.entities)
                {
                    ToEntity(entity, room);
                }

                for (int Z = 0; Z < wrapper.depth; Z++)
                {
                    Tile[,] LayerMap = room.GetLayerMap(Z);
                    for (int Y = 0; Y < wrapper.height; Y++)
                    {
                        for (int X = 0; X < wrapper.width; X++)
                        {
                            var pos = new Vector2(X, wrapper.height - Y - 1);
                            if (TryGetTileType(wrapper.map[Z][Y][X], pos, room, out var t))
                            {
                                Tile tile = ToTile(t, pos, room);
                                if (tile != null)
                                {
                                    tile.layer = Z - wrapper.mainlayer + 1;
                                    LayerMap[X, wrapper.height - Y - 1] = tile;
                                }
                            }
                        }
                    }
                }

                room.layerZeroPoint = wrapper.mainlayer;
                room.DefaultGamma = wrapper.DefaultGamma;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return false;
            }

            return true;
        }
    }

    #region Wrappers
    public class RoomWrapper
    {
        public int ID { get; set; }
        public float DefaultGamma { get; set; }
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
        public EntityWrapper[] entities { get; set; }
        public int[][][] map { get; set; }
    }

    public class BackgroundWrapper
    {
        public float X { get; set; }
        public float Y { get; set; }
        public LayerWrapper[] GraphicsLayers { get; set; }

        public BackgroundWrapper(float X, float Y, LayerWrapper[] GraphicsLayers)
        {
            this.X = X;
            this.Y = Y;
            this.GraphicsLayers = GraphicsLayers;
        }
    }

    public class LayerWrapper
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

        public LayerWrapper(string Texture, float X, float Y, float DisplacementScrollSpeed, float SelfScrollVelocityX, float SelfScrollVelocityY,
        float MaxDisplacementX, float MaxDisplacementY, float MinDisplacementX, float MinDisplacementY)
        {
            this.Texture = Texture;
            this.X = X;
            this.Y = Y;
            this.DisplacementScrollSpeed = DisplacementScrollSpeed;
            this.SelfScrollVelocityX = SelfScrollVelocityX;
            this.SelfScrollVelocityY = SelfScrollVelocityY;
            this.MaxDisplacementX = MaxDisplacementX;
            this.MaxDisplacementY = MaxDisplacementY;
            this.MinDisplacementX = MinDisplacementX;
            this.MinDisplacementY = MinDisplacementY;
        }
    }

    public class PropertyWrapper
    {
        public string name { get; set; }
        public string type { get; set; }
        public object value { get; set; }
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
    #endregion
}