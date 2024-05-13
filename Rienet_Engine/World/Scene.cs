using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public class Scene : IGameObject
    {
        public WorldBody world;
        public int ID;
        public bool specifiedCameraMobilitySize;
        public Vector2 pos, startPos, size, cameraMobilityPos = new(-1), cameraMobilitySize = new(-1);
        public int depth;
        public int layerZeroPoint = 0;
        public Hitbox[] Hitboxes;
        public List<Tile[,]> SceneMap;
        public Dictionary<Vector2, HitboxChunk> HitboxChunks = new();
        public List<PhysicsBody> BodiesInScene = new();
        public List<Entity> EntitiesInScene = new();
        /// <summary>
        /// 0~1, 0 being full dark, 1 being full lit
        /// </summary>
        public float DefaultGamma = 1;
        //The lightsources are drawn on a separate rendertarget2d
        public List<LightSource> LightSources = new();
        /// <summary>
        /// 2d represented by 1d, vector values corresponds to r, g, b, alpha
        /// </summary>
        public Color[] LightMap;
        /// <summary>
        /// Free sprites that are draw in the world at its corresponding position, unaffected by physics, note the pos of these graphicsComponents are their world positions, 
        /// </summary>
        public List<GraphicsComponent> FreeParticles = new();
        public Background BG;
        public GraphicsComponent Overlay;

        public List<Transition> Transitions = new();

        //the float here refers to time from request time
        public Dictionary<PhysicsBody, float> BodiesRequestingUpdate = new();

        //temp
        public List<Hitbox> hitboxestodraw = new();

        public Scene(int ID, int D, int W, int H, GraphicsComponent Overlay, Background background, WorldBody world)
        {
            this.world = world;
            this.ID = ID;
            background.Scene = this;
            BG = background;
            this.W = W; this.H = H;
            depth = D;
            SceneMap = new();
            for (int i = 0; i < D; i++)
                SceneMap.Add(new Tile[W, H]);

            this.Overlay = Overlay;
            OnCreation();
        }

        public Scene(int ID, int D, int W, int H, GraphicsComponent Overlay, WorldBody world)
        {
            this.world = world;
            this.ID = ID;
            this.W = W; this.H = H;
            depth = D;
            SceneMap = new();
            for (int i = 0; i < D; i++)
                SceneMap.Add(new Tile[W, H]);

            this.Overlay = Overlay;
            OnCreation();
        }

        public Scene(int D, int W, int H, GraphicsComponent Overlay, WorldBody world)
        {
            this.world = world;
            this.W = W; this.H = H;
            depth = D;
            SceneMap = new();
            for (int i = 0; i < D; i++)
                SceneMap.Add(new Tile[W, H]);

            this.Overlay = Overlay;
            OnCreation();
        }

        public Scene(int ID, int D, int W, int H, WorldBody world)
        {
            this.world = world;
            this.ID = ID;
            this.W = W; this.H = H;
            depth = D;
            SceneMap = new();
            for (int i = 0; i < D; i++)
                SceneMap.Add(new Tile[W, H]);

            OnCreation();
        }

        public Scene(int D, int W, int H, WorldBody world)
        {
            this.world = world;
            this.W = W; this.H = H;
            depth = D;
            SceneMap = new();
            for (int i = 0; i < D; i++)
                SceneMap.Add(new Tile[W, H]);

            OnCreation();
        }

        public Scene(int ID, int D, int W, int H, GraphicsComponent Overlay)
        {
            this.ID = ID;
            this.W = W; this.H = H;
            depth = D;
            SceneMap = new();
            for (int i = 0; i < D; i++)
                SceneMap.Add(new Tile[W, H]);

            this.Overlay = Overlay;
            OnCreation();
        }

        public Scene(int D, int W, int H, GraphicsComponent Overlay)
        {
            this.W = W; this.H = H;
            depth = D;
            SceneMap = new();
            for (int i = 0; i < D; i++)
                SceneMap.Add(new Tile[W, H]);

            this.Overlay = Overlay;
            OnCreation();
        }

        public Scene(int ID, int D, int W, int H)
        {
            this.ID = ID;
            this.W = W; this.H = H;
            depth = D;
            SceneMap = new();
            for (int i = 0; i < D; i++)
                SceneMap.Add(new Tile[W, H]);

            OnCreation();
        }

        public Scene(int D, int W, int H)
        {
            this.W = W; this.H = H;
            depth = D;
            SceneMap = new();
            for (int i = 0; i < D; i++)
                SceneMap.Add(new Tile[W, H]);
            HitboxChunks = new();
            BodiesInScene = new();
            EntitiesInScene = new();

            OnCreation();
        }

        public void SetCameraMobilityPos(Vector2 Pos)
        {
            cameraMobilityPos = Pos;
            specifiedCameraMobilitySize = true;
        }

        public void SetCameraMobilityPos(float x, float y)
        {
            cameraMobilityPos = new(x, y);
            specifiedCameraMobilitySize = true;
        }

        public void SetCameraMobilitySize(Vector2 Size)
        {
            cameraMobilitySize = Size;
            specifiedCameraMobilitySize = true;
        }

        public void SetCameraMobilitySize(float width, float height)
        {
            cameraMobilitySize = new(width, height);
            specifiedCameraMobilitySize = true;
        }

        public void OnCreation()
        {
            //divide hitboxchunks
            int ChunkColumnCount = (int)Math.Ceiling((float)W / HitboxChunk.W); //X
            int ChunkRowCount = (int)Math.Ceiling((float)H / HitboxChunk.H);

            for (int X = 0; X < ChunkColumnCount; X++)
            {
                for (int Y = 0; Y < ChunkRowCount; Y++)
                {
                    int TX = X * HitboxChunk.W, TY = Y * HitboxChunk.H;
                    HitboxChunks.Add(new Vector2(TX, TY), new HitboxChunk(TX, TY));
                }
            }

            LightMap = new Color[(int)(W * H) * GamePanel.PixelsInTile * GamePanel.PixelsInTile];
        }

        //don't update everything at once, only the requested ones
        public virtual void Update()
        {
            for (int i = 0; i < LightMap.Length; i++)
                LightMap[i] = new Color(0, 0, 0, 1 - DefaultGamma);
            try
            {
                //avoiding foreach to prevent invalidoperation exception
                for (int i = 0; i < EntitiesInScene.Count; i++)
                {
                    EntitiesInScene[i]?.Update();
                    //else body.Update();
                }

                foreach (PhysicsBody body in BodiesRequestingUpdate.Where(kvp => kvp.Value <= GamePanel.TimeInTicks * GamePanel.ElapsedTime).Select(kvp => kvp.Key))
                {
                    body?.Update();
                    if (!body.ConstantUpdate)
                        BodiesRequestingUpdate.Remove(body);
                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        public Tile[,] GetLayerMap(int depth)
        {
            return SceneMap[depth];
        }

        public bool GetGridInfo(Vector2 pos, Tile[,] Layer, out Tile tile)
        {
            int X = (int)pos.X, Y = (int)pos.Y;
            bool InRange = X >= 0 && X < W && Y >= 0 && Y < H;
            if (InRange) tile = Layer[X, Y]; else tile = null;
            return tile != null;
        }

        public bool TryGetHitboxChunk(float X, float Y, out HitboxChunk chunk)
        {
            //fit X, Y to positions in chunk sense
            int ChunkX = (int)(X - X % HitboxChunk.W), ChunkY = (int)(Y - Y % HitboxChunk.H);
            return HitboxChunks.TryGetValue(new Vector2(ChunkX, ChunkY), out chunk);
        }

        public bool TryRemoveBody(PhysicsBody body)
        {
            if (BodiesInScene.Contains(body))
                BodiesInScene.Remove(body);
            else return false;

            if (BodiesRequestingUpdate.ContainsKey(body))
                BodiesRequestingUpdate?.Remove(body);

            foreach (Hitbox hb in body.hitbox)
            {
                if (hitboxestodraw.Contains(hb))
                    hitboxestodraw.Remove(hb);
            }

            //min x to max x remove
            for (int x = (int)body.X; x < body.X + body.Width; x += HitboxChunk.W)
            {
                for (int y = (int)body.Y; y < body.Y + body.Height; y += HitboxChunk.H)
                {
                    if (TryGetHitboxChunk(x, y, out HitboxChunk chunk))
                        chunk.RemoveBody(body);
                }
            }

            return true;
        }

        public void AddBody(PhysicsBody body)
        {
            if (!BodiesInScene.Contains(body))
                BodiesInScene.Add(body);
            //add in chunks
            for (int x = (int)body.X; x < body.X + body.Width; x += HitboxChunk.W)
            {
                for (int y = (int)body.Y; y < body.Y + body.Height; y += HitboxChunk.H)
                {
                    if (TryGetHitboxChunk(x, y, out HitboxChunk chunk))
                        chunk.AddBody(body);
                }
            }
        }

        public void AddFreeParticle(GraphicsComponent graphicsComponent)
        {
            if (!FreeParticles.Contains(graphicsComponent))
                FreeParticles.Add(graphicsComponent);
        }

        public void RemoveFreeParticle(GraphicsComponent graphicsComponent)
        {
            if (FreeParticles.Contains(graphicsComponent))
                FreeParticles.Remove(graphicsComponent);
        }

        public virtual void OnDestruction() { }

        public virtual void Reload() { }

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

        public float StartX
        {
            get { return startPos.X; }
            set { startPos.X = value; }
        }

        public float StartY
        {
            get { return startPos.Y; }
            set { startPos.Y = value; }
        }

        public float W
        {
            get { return size.X; }
            set { size.X = value; }
        }

        public float H
        {
            get { return size.Y; }
            set { size.Y = value; }
        }

        public float CameraMobilityX
        {
            get { return cameraMobilityPos.X; }
            set { cameraMobilityPos.X = value; }
        }

        public float CameraMobilityY
        {
            get { return cameraMobilityPos.Y; }
            set { cameraMobilityPos.Y = value; }
        }

        public float CameraMobilityWidth
        {
            get { return cameraMobilitySize.X; }
            set { cameraMobilitySize.X = value; }
        }

        public float CameraMobilityHeight
        {
            get { return cameraMobilitySize.Y; }
            set { cameraMobilitySize.Y = value; }
        }
    }

    public class Background
    {
        public Scene Scene;
        public List<Layer> layers;
        public Vector2 pos, size;
        public float ScrollSpeed;

        public Background(List<Layer> layers, Scene Scene)
        {
            this.layers = layers;
            this.Scene = Scene;
        }

        public Background(List<Layer> layers)
        {
            this.layers = layers;
        }

        public void Draw(Vector2 POV, Camera Cam, SpriteBatch spriteBatch)
        {
            //draw the deepest layer to shallowest layer of backgrounds, fix to pixels
            foreach (Layer l in layers)
            {
                Vector2 Displacement = l.DisplacementScrollSpeed * (POV - l.Pos);
                Vector2 DrawposInScene = new Vector2(l.Pos.X, l.Pos.Y + l.Size.Y) + Displacement;
                DrawposInScene -= new Vector2(DrawposInScene.X % GamePanel.PixelSize, DrawposInScene.Y % GamePanel.PixelSize);
                Vector2 Drawpos = GraphicsRenderer.ToScreenPos(DrawposInScene, Cam.pos);
                spriteBatch.Draw(l.Texture, Drawpos, null, Color.White, 0, Vector2.Zero, new Vector2(GamePanel.TileSize / GamePanel.PixelsInTile, GamePanel.TileSize / GamePanel.PixelsInTile), SpriteEffects.None, 1);
            }
        }
    }

    public struct Layer
    {
        public Texture2D Texture { get; set; }
        public Vector2 Pos { get; set; }
        public Vector2 Size { get; set; }
        public float DisplacementScrollSpeed { get; set; }
        public Vector2 SelfScrollVelocity { get; set; }
        public Vector2 MaxDisplacement, MinDisplacement;

        public Layer(Texture2D Texture, Vector2 Pos, float DisplacementScrollSpeed, Vector2 SelfScrollVelocity, Vector2 MaxDisplacement, Vector2 MinDisplacement)
        {
            this.Texture = Texture;
            this.Pos = Pos;
            this.MaxDisplacement = MaxDisplacement;
            this.MinDisplacement = MinDisplacement; //in ratio of size
            if (Texture != null)
                Size = new Vector2((float)Texture.Width / GamePanel.PixelsInTile, (float)Texture.Height / GamePanel.PixelsInTile);
            else Size = Vector2.Zero;
            this.DisplacementScrollSpeed = DisplacementScrollSpeed;
            this.SelfScrollVelocity = SelfScrollVelocity;
        }
    }

    public class HitboxChunk
    {
        public const int W = 4, H = 4;
        public List<PhysicsBody> BodiesInGrid;
        public float X, Y;

        public HitboxChunk(int X, int Y)
        {
            this.X = X; this.Y = Y;
            BodiesInGrid = new List<PhysicsBody>();
        }

        public void AddBody(PhysicsBody Body)
        {
            if (!BodiesInGrid.Contains(Body)) BodiesInGrid.Add(Body);
        }

        public void RemoveBody(PhysicsBody Body)
        {
            if (BodiesInGrid.Contains(Body)) BodiesInGrid.Remove(Body);
        }
    }

    public class SceneWrapper
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
}