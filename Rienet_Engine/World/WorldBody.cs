using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Rienet
{
    //a class for containing all scenes in a world
    public class WorldBody
    {
        //constants
        public const float DefaultFriction = 0.012f;
        public static readonly Vector2 West = new(-1, 0), East = new(1, 0), Up = new(0, 1), Down = new(0, -1);
        public static readonly Vector2 Gravity = new(0, -0.023f);

        //components
        public Dictionary<int, Scene> Scenes;
        public double GameTime;

        public WorldBody()
        {
            Scenes = new Dictionary<int, Scene>();
        }

        public bool GetGridInfo(int Scene, Vector2 pos, Tile[,] layer, out Tile tile)
        {
            bool SceneExists = Scenes.TryGetValue(Scene, out Scene targetScene);
            if (SceneExists)
            {
                targetScene.GetGridInfo(pos, layer, out tile);
                return true;
            }
            else
            {
                tile = null;
                return false;
            }
        }
    }
}