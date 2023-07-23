using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Rienet
{
    public class WorldBody
    {
        //the air friction or the friction on tiles without any collision
        public static float DefaultFriction = 0.012f;
        public static Vector2 West = new Vector2(-1, 0), East = new Vector2(1, 0), Up = new Vector2(0, 1), Down = new Vector2(0, -1);
        public static Vector2 Gravity = new Vector2(0, -0.023f);
        public static float PXSize = 1f / GamePanel.PixelsInTile;
        public Dictionary<int, Scene> Scenes;
        public ulong GameTime;

        public WorldBody()
        {
            Scenes = new Dictionary<int, Scene>();
        }

        public void LoadScenes()
        {

        }

        public bool GetGridInfo(int Scene, Vector2 pos, out Tile tile)
        {
            bool SceneExists = Scenes.TryGetValue(Scene, out Scene targetScene);
            if (SceneExists)
            {
                targetScene.GetGridInfo(pos, out tile);
                return true;
            }
            else
            {
                tile = null;
                return false;
            }
        }

        public void GetHitboxChunkInfo()
        {

        }
    }
}