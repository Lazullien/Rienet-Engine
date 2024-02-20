//change this into java file later
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Rienet;

public static class TiledToJsonRoom
{
    public static string TiledJsonToFormat(string file, int ID, int mainlayer, bool specifiedCamMobility, float camMobilityX, float camMobilityY,
    float camMobilityWidth, float camMobilityHeight, bool specifiedBackground)
    {
        string json = File.ReadAllText(file);
        var wrapper = JsonSerializer.Deserialize<Wrapper>(json);
        var r = new RoomWrapper();

        List<Wrapper.LayerWrapper> tileLayers = new();
        List<Wrapper.LayerWrapper> objectLayers = new();
        List<float[]> transitions = new();
        List<EntityWrapper> entities = new();

        Wrapper.LayerWrapper bg = null;

        foreach (var i in wrapper.layers)
            switch (i.type)
            {
                case "tilelayer":
                    tileLayers.Add(i);
                    break;
                case "objectgroup":
                    objectLayers.Add(i);
                    break;
                case "imagelayer":
                    bg = i;
                    break;
            }

        foreach (var j in objectLayers)
            foreach (var k in j.objects)
                if (k.type == "Transition")
                    if (k.properties != null)
                    {
                        transitions.Add(new float[] { k.x / 8, wrapper.height - (k.y / 8) - (k.height / 8), k.width / 8, k.height / 8, ((JsonElement)k.properties[0].value).GetInt32(), (float)((JsonElement)k.properties[1].value).GetDouble(), (float)((JsonElement)k.properties[2].value).GetDouble() });
                    }
                    else
                        transitions.Add(new float[] { k.x / 8, wrapper.height - (k.y / 8), k.width / 8, k.height / 8, 0, 0, 0 });
                else
                    entities.Add(new EntityWrapper
                    {
                        id = k.id,
                        type = k.type,
                        x = k.x / 8,
                        y = wrapper.height - (k.y / 8)
                    });

        //the variables that don't require manual editing
        r.depth = tileLayers.Count;
        r.width = wrapper.width;
        r.height = wrapper.height;
        r.transitions = transitions.ToArray();
        r.entities = entities.ToArray();
        var map = new int[r.depth][][];
        for (int z = 0; z < map.Length; z++)
        {
            var mapLayer = tileLayers[z];
            var mapz = new int[r.height][];
            for (int y = 0; y < mapz.Length; y++)
            {
                var mapzy = new int[r.width];
                for (int x = 0; x < mapzy.Length; x++)
                {
                    mapzy[x] = mapLayer.data[y * r.width + x];
                }
                mapz[y] = mapzy;
            }
            map[z] = mapz;
        }
        r.map = map;

        r.ID = ID;
        r.mainlayer = mainlayer;
        r.specifiedCamMobility = specifiedCamMobility;
        r.camMobilityX = camMobilityX;
        r.camMobilityY = camMobilityY;
        r.camMobilityWidth = camMobilityWidth;
        r.camMobilityHeight = camMobilityHeight;
        r.specifiedBackground = specifiedBackground;

        if (specifiedBackground)
        {
            List<LayerWrapper> backgroundLayers = new();
            foreach (Wrapper.LayerWrapper.PropertyWrapper pw in bg.properties)
            {
                var blw = JsonSerializer.Deserialize<Wrapper.LayerWrapper.BackgroundLayerWrapper>((JsonElement)pw.value);
                backgroundLayers.Add(new(blw.Texture, blw.X, blw.Y, blw.DisplacementScrollSpeed, blw.SelfScrollVelocityX, blw.SelfScrollVelocityY,
                    blw.MaxDisplacementX, blw.MaxDisplacementY, blw.MinDisplacementX, blw.MinDisplacementY));
            }
            r.background = new(bg.x, bg.y, backgroundLayers.ToArray());
        }

        return JsonSerializer.Serialize(r);
    }

    public class Wrapper
    {
        public int compressionlevel { get; set; }
        public int height { get; set; }
        public bool infinite { get; set; }
        public LayerWrapper[] layers { get; set; }
        public int nextlayerid { get; set; }
        public int nextobjectid { get; set; }
        public string orientation { get; set; }
        public string renderorder { get; set; }
        public string tiledversion { get; set; }
        public int tileheight { get; set; }
        public TilesetLinkWrapper[] tilesets { get; set; }
        public int tilewidth { get; set; }
        public string type { get; set; }
        public string version { get; set; }
        public int width { get; set; }

        public class LayerWrapper
        {
            public string draworder { get; set; } //objects only
            public int[] data { get; set; } //tiles only
            public int height { get; set; } //tiles only
            public int id { get; set; }
            public string name { get; set; }
            public ObjectWrapper[] objects { get; set; } //objects only
            public PropertyWrapper[] properties { get; set; } //backgrounds
            public int opacity { get; set; }
            public string type { get; set; }
            public bool visible { get; set; }
            public int width { get; set; } //tiles only
            public int x { get; set; }
            public int y { get; set; }

            public class ObjectWrapper
            {
                public float height { get; set; }
                public int id { get; set; }
                public string name { get; set; }
                public PropertyWrapper[] properties { get; set; }
                public bool point { get; set; }
                public float rotation { get; set; }
                public string type { get; set; }
                public bool visible { get; set; }
                public float width { get; set; }
                public float x { get; set; }
                public float y { get; set; }
            }

            public class PropertyWrapper
            {
                public string name { get; set; }
                public string type { get; set; }
                public string propertytype { get; set; }
                public object value { get; set; }
            }

            public class BackgroundLayerWrapper
            {
                public float DisplacementScrollSpeed { get; set; }
                public float MaxDisplacementX { get; set; }
                public float MaxDisplacementY { get; set; }
                public float MinDisplacementX { get; set; }
                public float MinDisplacementY { get; set; }
                public float SelfScrollVelocityX { get; set; }
                public float SelfScrollVelocityY { get; set; }
                public string Texture { get; set; }
                public float X { get; set; }
                public float Y { get; set; }
            }
        }

        public class TilesetLinkWrapper
        {
            public int firstgid { get; set; }
            public string source { get; set; }
        }
    }
}

/*
 * Input format:
 * { "compressionlevel":-1,
 "height":20,
 "infinite":false,
 "layers":[
        {
         "data":[19, 19, 19, 29, 29, 19, 19, 19, 19, 19, 19, 19, 29, 29, 19, 19, 19, 19, 19, 19,
            19, 19, 19, 29, 29, 19, 19, 19, 19, 19, 19, 19, 29, 29, 19, 19, 19, 19, 19, 19],
         "height":20,
         "id":2,
         "name":"Tile Layer 2",
         "opacity":1,
         "type":"tilelayer",
         "visible":true,
         "width":20,
         "x":0,
         "y":0
        }, 
        {
         "data":[7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 6, 0, 0, 8, 7, 7, 7, 7, 7,
            7, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 16, 0, 0, 15, 3, 3, 3, 4, 7],
         "height":20,
         "id":1,
         "name":"Tile Layer 1",
         "opacity":1,
         "type":"tilelayer",
         "visible":true,
         "width":20,
         "x":0,
         "y":0
        }, 
        {
         "draworder":"topdown",
         "id":4,
         "name":"Object Layer 1",
         "objects":[
                {
                 "height":0,
                 "id":28,
                 "name":"",
                 "point":true,
                 "rotation":0,
                 "type":"FloatingBlob",
                 "visible":true,
                 "width":0,
                 "x":61.3333,
                 "y":122.667
                }, 
                {
                 "height":0,
                 "id":29,
                 "name":"",
                 "point":true,
                 "rotation":0,
                 "type":"FloatingBlob",
                 "visible":true,
                 "width":0,
                 "x":83,
                 "y":86
                }, 
                {
                 "height":3,
                 "id":30,
                 "name":"",
                 "rotation":0,
                 "type":"Transition",
                 "visible":true,
                 "width":18.3333,
                 "x":94.3333,
                 "y":-1
                }, 
                {
                 "height":0,
                 "id":31,
                 "name":"",
                 "point":true,
                 "rotation":0,
                 "type":"FloatingBlob",
                 "visible":true,
                 "width":0,
                 "x":83.3333,
                 "y":51
                }],
         "opacity":1,
         "type":"objectgroup",
         "visible":true,
         "x":0,
         "y":0
        }, 
        {
         "data":[0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
         "height":20,
         "id":3,
         "name":"Tile Layer 3",
         "opacity":1,
         "type":"tilelayer",
         "visible":true,
         "width":20,
         "x":0,
         "y":0
        }],
 "nextlayerid":5,
 "nextobjectid":32,
 "orientation":"orthogonal",
 "renderorder":"right-down",
 "tiledversion":"1.10.1",
 "tileheight":8,
 "tilesets":[
        {
         "firstgid":1,
         "source":"..\/complex.tsx"
        }],
 "tilewidth":8,
 "type":"map",
 "version":"1.10",
 "width":20
}
 * 
 * Output format:
 * {
    "ID":3,
    "X":0,
    "Y":0,
    "depth":2,
    "mainlayer":2,
    "width":21,
    "height":19,
    "specifiedCamMobility":true,
    "camMobilityX": 8,
    "camMobilityY": -1,
    "camMobilityWidth": 20,
    "camMobilityHeight": 13,
    "specifiedBackground":true,
    "background":
    {
        "X":0,
        "Y":0,
        "GraphicsLayers":[
        {
                "Texture":"textures/backgrounds/complex",
                "X":0,
                "Y":-5,
                "DisplacementScrollSpeed":0,
                "SelfScrollVelocityX":0,
                "SelfScrollVelocityY":0,
                "MaxDisplacementX":0,
                "MaxDisplacementY":0,
                "MinDisplacementX":0,
                "MinDisplacementY":0
        }
        ]
    },
    "transitions":
    [
        [1,6,1,4, 2, 34,9.2, 3, 0,0]
    ],
    "entities":
    [
        {
            "id":1,
            "type":"FloatingBlob",
            "x":1,
            "y":1
        }
    ],
    "map":
    [
        [
            [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],
            [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],
        ],
        [
            [7,7,7,7,7,6,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],
            [7,7,7,7,7,6,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],
        ]
    ]
} 
 */