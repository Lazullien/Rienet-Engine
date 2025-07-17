using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TacticalBand;

namespace Rienet
{
    public class Camera
    {
        /// <summary>
        /// Usually true in top down oriented games, false in side view games, this makes drawn components more realistic by drawing ones in front in front
        /// </summary>
        public static bool SortByYInLayer { get; private set; } = true;
        //Center Pos
        public Vector2 pos, dislocation, MovePos;
        //Origin Pos (bottom-west)
        public Vector2 origin;
        public Vector2 visibleOrigin;
        public Vector2 size;
        public Vector2 ScreenPos { get; set; }
        public Vector2 ScreenSize { get; set; } = new(GamePanel.renderTargetSize.X, GamePanel.renderTargetSize.Y);
        public Vector2 VisibleSize;
        public Scene Scene;
        public bool LockOn { get; set; }
        public Entity LockOnEntity;
        readonly GamePanel gp;
        readonly WorldBody world;

        List<CameraShakeState> shakeStates = new();
        int currentStateIndex;

        /// <summary>
        /// sheet, layer
        /// </summary>
        public Dictionary<AnimatedSheet, int> Overlays { get; set; } = new();
        public GraphicsComponent AbsoluteTop { get; set; }

        public Camera(int id, Vector2 pos, Vector2 size, Scene StartScene, WorldBody world, GamePanel gp)
        {
            this.gp = gp;
            this.pos = pos;
            this.size = size;
            Scene = StartScene;
            this.world = world;
            world.Cameras.Add(id, this);
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
                Scene = LockOnEntity.BelongedScene;
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
                pos.X = Scene.W * 0.5f + x;
            }
            else
            {
                float XMin = VisibleSize.X * 0.5f + x, XMax = width - (VisibleSize.X * 0.5f) + x;
                bool GreaterX = entityPosition.X + (entitySize.X * 0.5f) > XMin, MinorX = entityPosition.X + (entitySize.X * 0.5f) < XMax;
                // Follow the player in the axis where the Scene didn't end.
                if (GreaterX && MinorX)
                    pos.X = entityPosition.X + (entitySize.X * 0.5f);
                else pos.X = GreaterX ? XMax : MinorX ? XMin : 0;
            }

            if (VisibleHeight > height + y)
            {
                pos.Y = Scene.H * 0.5f + y;
            }
            else
            {
                float YMin = VisibleSize.Y * 0.5f + y, YMax = height - (VisibleSize.Y * 0.5f) + y;
                bool GreaterY = entityPosition.Y + (entitySize.Y * 0.5f) > YMin, MinorY = entityPosition.Y + (entitySize.Y * 0.5f) < YMax;
                if (GreaterY && MinorY)
                    pos.Y = entityPosition.Y + (entitySize.Y * 0.5f);
                else pos.Y = GreaterY ? YMax : MinorY ? YMin : 0;
            }

            MovePos = pos - LastPos;
            pos += dislocation; //put a placeholder for pos instead of editing real pos

            origin = new Vector2(pos.X - (size.X * 0.5f), pos.Y - (size.Y * 0.5f));
            visibleOrigin = new Vector2(pos.X - (VisibleWidth * 0.5f), pos.Y - (VisibleHeight * 0.5f));
        }

        public void ChangeScene(int ID)
        {
            world.Scenes.TryGetValue(ID, out Scene);
        }

        public void ProjectToScreen(SpriteBatch spriteBatch)
        {
            Scene.BG?.Draw(origin, this, spriteBatch);

            Dictionary<int, List<PhysicsBody>> bodiesToDraw = new();
            Dictionary<int, List<Entity>> entitiesToDraw = new();
            Dictionary<int, List<Particle>> particlesToDraw = new();

            for (int z = 0; z < Scene.depth; z++)
            {
                bodiesToDraw.Add(z, new List<PhysicsBody>());
                entitiesToDraw.Add(z, new List<Entity>());
                particlesToDraw.Add(z, new List<Particle>());
            }

            foreach (PhysicsBody body in Scene.BodiesInScene)
                bodiesToDraw[body.nonRelativeLayerInScene].Add(body);

            foreach (Entity e in Scene.EntitiesInScene)
                entitiesToDraw[e.nonRelativeLayerInScene].Add(e);

            foreach (Particle p in ParticleEngine.Particles)
                if (p.BelongedScene == Scene)
                    particlesToDraw[p.Layer + Scene.layerZeroPoint - 1].Add(p);

            // Render all layered content (tiles, bodies, entities)
            //z not reaching extra entity layers
            for (int z = 0; z < Scene.depth; z++)
            {
                Tile[,] layer = Scene.GetLayerMap(z);
                for (float x = origin.X; x < Math.Ceiling(origin.X + size.X); x++)
                {
                    for (float y = origin.Y; y < Math.Ceiling(origin.Y + size.Y); y++)
                    {
                        bool tileExists = Scene.GetGridInfo(new Vector2(x, y), layer, out Tile tile);
                        if (tileExists)
                            tile.Draw(pos, spriteBatch, gp);
                    }
                }

                foreach (var body in bodiesToDraw[z])
                {
                    Vector2 DrawPos = GraphicsRenderer.ToScreenPos(body.pos, pos);
                    body.Draw((int)DrawPos.X, (int)DrawPos.Y, spriteBatch, GraphicsComponent.blankRect);
                }

                //sort drawing by highest to lowest y position if that is the selected sort mode
                if (SortByYInLayer)
                {
                    Dictionary<Entity, float> EntitySorter = new();
                    var entitiesInLayer = entitiesToDraw[z];
                    foreach (var e in entitiesToDraw[z])
                        EntitySorter.Add(e, -e.pos.Y);

                    foreach (var e in EntitySorter.OrderBy(s => s.Value).ToDictionary(s => s.Key, s => s.Value).Keys)
                        e.Draw(pos, spriteBatch, gp);
                }
                else
                {
                    //also don't draw every single entity in scene, just the ones close enough, this would be fine for TacticalBand, but if i made an open world sandbox it would suck
                    foreach (var e in entitiesToDraw[z])
                        e.Draw(pos, spriteBatch, gp);
                }

                foreach (var p in particlesToDraw[z])
                    p.Draw(pos, spriteBatch);

                foreach (var Overlay in Overlays)
                    if (Overlay.Value == z)
                        GraphicsRenderer.DrawSpriteInSheet(Overlay.Key, spriteBatch);
            }

            if (LockOn)
                LockOnEntity.PerspectiveFilter(spriteBatch);

            int lmWidth = (int)Scene.W * GamePanel.PixelsInTile;
            int lmHeight = (int)Scene.H * GamePanel.PixelsInTile;

            foreach (var light in Scene.LightSources)
                if (light is ScreenFlash)
                    GraphicsRenderer.DrawComponent(light, light.InWorldPos, pos, spriteBatch);
                else
                    GraphicsRenderer.DrawLight(light, light.InWorldPos, pos, spriteBatch);

            //place the data into a texture, then draw with graphicsrenderer
            Texture2D lightTexture = new(GamePanel.graphicsDevice, lmWidth, lmHeight);
            lightTexture.SetData(Scene.LightMap);
            var img = new Image(Vector2.Zero, Vector2.Zero, lightTexture)
            {
                Pos = pos
            };
            GraphicsRenderer.DrawComponent(img, new(0, Scene.H), pos, spriteBatch);

            if (Scene.Overlay != null)
                GraphicsRenderer.DrawComponent(Scene.Overlay, Vector2.Zero, pos, spriteBatch);

            UIHandler.Draw(spriteBatch);

            if (AbsoluteTop != null)
                GraphicsRenderer.DrawComponent(AbsoluteTop, visibleOrigin + new Vector2(0, VisibleHeight), pos, spriteBatch);
        }

        public void PlayAnimatedOverlay(int layer, AnimatedSheet overlay)
        {
            Overlays.Add(overlay, layer);
        }

        public void UpdateAnimatedOverlay()
        {
            var keys = Overlays.Keys.ToList();
            for (int i = 0; i < Overlays.Count; i++)
            {
                var Overlay = keys[i];
                if (Overlay != null)
                {
                    Overlay.Update();
                    if (Overlay != null && Overlay.Finished)
                        EndAnimatedOverlay(Overlay);
                }
            }
        }

        public void EndAnimatedOverlay(AnimatedSheet Overlay)
        {
            Overlay?.Reset();
            Overlay?.Resume();
            Overlays.Remove(Overlay);
        }

        public void EndAllAnimatedOverlays()
        {
            var keys = Overlays.Keys.ToList();
            for (int i = 0; i < Overlays.Count; i++)
            {
                EndAnimatedOverlay(keys[0]);
            }
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