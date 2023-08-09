using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public class WorldProjection
    {
        readonly WorldBody world;
        //readonly Quadtree HitboxTree;
        Scene LoadedScene;
        Camera cam;
        public int SceneID;
        public Vector2 Center;

        public WorldProjection(WorldBody world, Camera cam)
        {
            this.world = world;
            this.cam = cam;
            //HitboxTree = new Quadtree(0, new Rectangle());
            Update(Vector2.Zero, 0);
        }

        public void Update(Vector2 Center, int NewScene)
        {
            SceneID = NewScene;
            world.Scenes.TryGetValue(NewScene, out LoadedScene);
            // cam.Update(Center);
            cam.ChangeScene(NewScene);
        }

        public void Project(SpriteBatch sb, GraphicsDevice gd)
        {
            //cam.ProjectToScreen(sb, gd);
        }
    }
}