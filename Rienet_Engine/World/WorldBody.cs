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

        public static readonly Scene placeHolderScene = new(0, 1, 10, 10);

        /// <summary>
        /// the scenes 
        /// </summary>
        public readonly Dictionary<int, Scene> Scenes = new();
        public readonly Dictionary<int, Camera> Cameras = new();
        public double GameTime;

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