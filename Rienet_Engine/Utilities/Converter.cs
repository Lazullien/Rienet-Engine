using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;

namespace Rienet
{
    public class Converter
    {
        public static Tile ToTile(int ID, Vector2 pos, Scene Scene)
        {
            return ID switch
            {
                //0 => new Air(pos, Scene),
                1 => new Solid(pos, 0.02f, Scene),
                2 => new Spike(pos, Scene),
                3 => new SpawnPoint(pos, Scene),
                4 => new SpawnPointSetter(pos, Scene),
                _ => null,
            };
        }
    }

    public enum TileIDs
    {
        Solid = 1,
        Spike = 2
    }

    public enum SceneIDs
    {

    }
}