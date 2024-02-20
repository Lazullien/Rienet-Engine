using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Rienet
{
    public class Camera
    {
        //Center Pos
        public Vector2 pos, dislocation, MovePos;
        //Origin Pos (bottom-west)
        public Vector2 origin;
        public Vector2 size;
        public Vector2 VisibleSize;
        public Scene Scene;
        public bool LockOn;
        public Entity LockOnEntity;
        readonly GamePanel gp;
        readonly WorldBody world;

        List<CameraShakeState> shakeStates = new();
        int currentStateIndex;

        public AnimatedSheet Overlay;

        public Camera(Vector2 pos, Vector2 size, Scene StartScene, WorldBody world, GamePanel gp)
        {
            this.gp = gp;
            this.pos = pos;
            this.size = size;
            Scene = StartScene;
            this.world = world;
        }

        public void Resize(float W, float H)
        {
            size = new Vector2(W, H);
        }

        //fix camera snapping when nearing end of Scene
        public void Update()
        {
            UpdateShake();
            UpdateAnimatedOverlay();

            Vector2 entityPosition = pos, entitySize = Vector2.Zero;

            if (LockOn)
            {
                entityPosition = LockOnEntity.pos;
                entitySize = LockOnEntity.DrawBox;
            }

            VisibleSize = new(GamePanel.VisibleWidth, GamePanel.VisibleHeight);
            Vector2 LastPos = pos;

            float width, height;
            float x = 0, y = 0;

            if (Scene.specifiedCameraMobilitySize)
            {
                x = Scene.CameraMobilityX;
                y = Scene.CameraMobilityY;
                width = Scene.CameraMobilityWidth;
                height = Scene.CameraMobilityHeight;
            }
            else
            {
                width = Scene.W;
                height = Scene.H;
            }

            //check if camera range is bigger than room, if so set axis at room center
            if (VisibleWidth > width + x)
            {
                pos.X = Scene.W / 2 + x;
            }
            else
            {
                float XMin = VisibleSize.X / 2 + x, XMax = width - (VisibleSize.X / 2) + x;
                bool GreaterX = entityPosition.X + (entitySize.X / 2) > XMin, MinorX = entityPosition.X + (entitySize.X / 2) < XMax;
                // Follow the player in the axis where the Scene didn't end.
                if (GreaterX && MinorX)
                    pos.X = entityPosition.X + (entitySize.X / 2);
                else pos.X = GreaterX ? XMax : MinorX ? XMin : 0;
            }

            if (VisibleHeight > height + y)
            {
                pos.Y = Scene.H / 2 + y;
            }
            else
            {
                float YMin = VisibleSize.Y / 2 + y, YMax = height - (VisibleSize.Y / 2) + y;
                bool GreaterY = entityPosition.Y + (entitySize.Y / 2) > YMin, MinorY = entityPosition.Y + (entitySize.Y / 2) < YMax;
                if (GreaterY && MinorY)
                    pos.Y = entityPosition.Y + (entitySize.Y / 2);
                else pos.Y = GreaterY ? YMax : MinorY ? YMin : 0;
            }

            MovePos = pos - LastPos;
            pos += dislocation; //put a placeholder for pos instead of editing real pos

            origin = new Vector2(pos.X - (size.X / 2), pos.Y - (size.Y / 2));
        }

        public void ChangeScene(int ID)
        {
            world.Scenes.TryGetValue(ID, out Scene);
        }

        public void ProjectToScreen(SpriteBatch spriteBatch)
        {
            Scene.BG?.Draw(origin, this, spriteBatch);

            // Render all the tiles that the camera touches.
            for (int z = 0; z < Scene.depth; z++)
            {
                Tile[,] layer = Scene.GetLayerMap(z);
                //TODO cache the layer here
                for (float x = origin.X; x < Math.Ceiling(origin.X + size.X); x++)
                {
                    for (float y = origin.Y; y < Math.Ceiling(origin.Y + size.Y); y++)
                    {
                        bool tileExists = Scene.GetGridInfo(new Vector2(x, y), layer, out Tile tile);
                        if (tileExists)
                            tile.Draw(pos, spriteBatch, gp);
                    }
                }
            }

            //render overlay in scene
            GraphicsComponent SceneOverlay = Scene.Overlay;

            if (SceneOverlay is SpriteSheet spriteSheet)
                GraphicsRenderer.DrawSpriteInSheet(spriteSheet, Vector2.Zero, pos, spriteBatch);
            else if (SceneOverlay is Image image)
                GraphicsRenderer.DrawImage(image, Vector2.Zero, pos, spriteBatch);
            else if (SceneOverlay != null)
                GraphicsRenderer.DrawComponent(SceneOverlay, Vector2.Zero, pos, spriteBatch);

            foreach (PhysicsBody body in Scene.BodiesInScene)
            {
                Vector2 DrawPos = GraphicsRenderer.ToScreenPos(body.pos, pos);
                body.Draw((int)DrawPos.X, (int)DrawPos.Y, spriteBatch, GraphicsComponent.blankRect);
            }

            //also don't draw every single entity in scene, just the ones close enough, this would be fine for atelo, but if i made an open world sandbox it would suck
            foreach (Entity e in Scene.EntitiesInScene)
                e.Draw(pos, spriteBatch, gp);

            if (LockOn)
                LockOnEntity.PerspectiveFilter(spriteBatch);

            foreach (Hitbox b in Scene.hitboxestodraw)
                if (b is CircularHitbox c)
                    GraphicsRenderer.DrawCircle(spriteBatch, c.pos, c.Radius, pos, Color.Red, gp.GraphicsDevice);
                else
                    GraphicsRenderer.DrawRectangle(spriteBatch, b.pos, b.size, pos, Color.Red, GraphicsComponent.blankRect);

            if (Overlay != null)
                GraphicsRenderer.DrawSpriteInSheet(Overlay, spriteBatch);

            int lmWidth = (int)Scene.W * GamePanel.PixelsInTile;
            int lmHeight = (int)Scene.H * GamePanel.PixelsInTile;

            //place the data into a texture, then draw with graphicsrenderer
            Texture2D lightTexture = new(GamePanel.graphicsDevice, lmWidth, lmHeight);
            lightTexture.SetData(Scene.LightMap);
            var img = new Image(Vector2.Zero, Vector2.Zero, lightTexture)
            {
                Pos = pos
            };
            GraphicsRenderer.DrawComponent(img, new(0, Scene.H), pos, spriteBatch);
        }

        public void PlayAnimatedOverlay(AnimatedSheet overlay)
        {
            EndAnimatedOverlay();
            Overlay = overlay;
        }

        public void UpdateAnimatedOverlay()
        {
            if (Overlay != null)
            {
                Overlay.Update();
                if (!Overlay.Animating)
                    EndAnimatedOverlay();
            }
        }

        public void EndAnimatedOverlay()
        {
            Overlay?.Reset();
            Overlay?.Resume();
            Overlay = new();
            Overlay = null;
        }

        public void Shake(float magnitude, float interval, float duration)
        {
            // play a list of actions, basically
            //dislocate the camera from its center within a certain time interval repeatedly at random value 0~1 * magnitude for duration
            EndShake();
            Random r = new();
            int changeCount = (int)(duration / interval);
            Vector2 lastTarget = Vector2.Zero;

            for (int i = 0; i < changeCount; i++)
            {
                Vector2 target = new((float)r.NextDouble() * magnitude, (float)r.NextDouble() * magnitude);
                shakeStates.Add(new((target - lastTarget) / interval, target));
                lastTarget = target;
            }
        }

        void UpdateShake()
        {
            if (shakeStates.Count > 0)
            {
                var state = shakeStates[currentStateIndex];
                //get velocity
                var target = state.target;
                var velocity = state.velocity;

                var orgDislocation = dislocation;
                dislocation += velocity;

                bool trueOnX = false; ;

                if ((orgDislocation.X - target.X) * (dislocation.X - target.X) <= 0)
                {
                    dislocation.X = target.X;
                    trueOnX = true;
                }
                if ((orgDislocation.Y - target.Y) * (dislocation.Y - target.Y) <= 0)
                {
                    dislocation.Y = target.Y;
                    if (trueOnX)
                        currentStateIndex++;
                }

                if (currentStateIndex >= shakeStates.Count)
                    EndShake();
            }
        }

        public void EndShake()
        {
            currentStateIndex = 0;
            dislocation = Vector2.Zero;
            shakeStates.Clear();
        }

        //public void Tint(Color color)
        //{

        //}

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

        public float OriginX
        {
            get { return origin.X; }
            set { origin.X = value; }
        }

        public float OriginY
        {
            get { return origin.Y; }
            set { origin.Y = value; }
        }

        public float Width
        {
            get { return size.X; }
            set { size.X = value; }
        }

        public float Height
        {
            get { return size.Y; }
            set { size.Y = value; }
        }

        public float VisibleWidth
        {
            get { return VisibleSize.X; }
            set { VisibleSize.X = value; }
        }

        public float VisibleHeight
        {
            get { return VisibleSize.Y; }
            set { VisibleSize.Y = value; }
        }
    }

    struct CameraShakeState
    {
        public Vector2 velocity, target;

        public CameraShakeState(Vector2 velocity, Vector2 target)
        {
            this.velocity = velocity;
            this.target = target;
        }
    }
}