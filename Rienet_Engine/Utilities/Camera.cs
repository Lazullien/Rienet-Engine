using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public class Camera
    {
        //Center Pos
        public Vector2 pos, MovePos;
        //Origin Pos (bottom-west)
        public Vector2 origin;
        public Vector2 size;
        public Vector2 VisibleSize;
        public Scene Scene;
        readonly GamePanel gp;
        readonly WorldBody world;

        public Texture2D blankRect;
        public Texture2D blankCirc;

        public Camera(Vector2 pos, Vector2 size, Scene StartScene, WorldBody world, GamePanel gp)
        {
            this.gp = gp;
            this.pos = pos;
            this.size = size;
            Scene = StartScene;
            this.world = world;
            blankRect = new Texture2D(gp.GraphicsDevice, 1, 1);
            blankRect.SetData(new[] { Color.White });
            //blankCirc = new Texture2D(gp.GraphicsDevice);
            //ChangeScene(0);
        }

        public void Resize(float W, float H)
        {
            size = new Vector2(W, H);
        }

        //fix camera snapping when nearing end of Scene
        public void Update(Vector2 playerPosition, Vector2 playerSize)
        {
            VisibleSize = new((float)GamePanel.Width / GamePanel.TileSize, (float)GamePanel.Height / GamePanel.TileSize);
            Vector2 LastPos = pos;

            float XMin = VisibleSize.X / 2, XMax = Scene.W - (VisibleSize.X / 2), YMin = VisibleSize.Y / 2, YMax = Scene.H - (VisibleSize.Y / 2);
            bool GreaterX = playerPosition.X + (playerSize.X / 2) > XMin, MinorX = playerPosition.X + (playerSize.X / 2) < XMax, GreaterY = playerPosition.Y + (playerSize.Y / 2) > YMin, MinorY = playerPosition.Y + (playerSize.Y / 2) < YMax;
            // Follow the player in the axis where the Scene didn't end.
            if (GreaterX && MinorX)
                pos.X = playerPosition.X + (playerSize.X / 2);
            else pos.X = GreaterX ? XMax : MinorX ? XMin : 0;

            if (GreaterY && MinorY)
                pos.Y = playerPosition.Y + (playerSize.Y / 2);
            else pos.Y = GreaterY ? YMax : MinorY ? YMin : 0;

            MovePos = pos - LastPos;

            origin = new Vector2(pos.X - (size.X / 2), pos.Y - (size.Y / 2));
        }

        public void ChangeScene(int ID)
        {
            world.Scenes.TryGetValue(ID, out Scene);
        }

        public void ProjectToScreen(SpriteBatch spriteBatch, GraphicsDevice gd)
        {
            Scene.BG?.Draw(origin, this, spriteBatch);

            // Render all the tiles that the camera touches.
            for (float x = origin.X; x < Math.Ceiling(origin.X + size.X); x++)
            {
                for (float y = origin.Y; y < Math.Ceiling(origin.Y + size.Y); y++)
                {
                    bool tileExists = Scene.GetGridInfo(new Vector2(x, y), out Tile tile);
                    if (tileExists) BasicRenderingAlgorithms.DrawTile(tile, pos, spriteBatch, gp);
                }
            }
            //BasicRenderingAlgorithms.DrawTile(new Tile(gp), gp.pl.pos, spriteBatch, gp);
            foreach (PhysicsBody body in Scene.BodiesInScene)
            {
                Vector2 DrawPos = BasicRenderingAlgorithms.ToScreenPos(body.pos, pos);
                body.Draw((int)DrawPos.X, (int)DrawPos.Y, spriteBatch, blankRect);
            }

            foreach(Entity e in Scene.EntitiesInScene)
            {
                e.Draw(pos, spriteBatch, gp);//BasicRenderingAlgorithms.DrawEntity(e, pos, spriteBatch, gp);
            }

            foreach (Hitbox b in Scene.hitboxestodraw)
                BasicRenderingAlgorithms.DrawHitbox(b, pos, Scene, spriteBatch, gp, blankRect);
        }

        public void DebugProjection(GraphicsDevice gd)
        {
        }
    }
}
