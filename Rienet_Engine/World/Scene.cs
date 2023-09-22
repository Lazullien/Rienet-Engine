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
        public Vector2 pos, startPos, size;
        public int depth;
        public Hitbox[] Hitboxes;
        public List<Tile[,]> SceneMap;
        public Dictionary<Vector2, HitboxChunk> HitboxChunks = new();
        public List<PhysicsBody> BodiesInScene = new();
        public List<Entity> EntitiesInScene = new(); //eventually change this to objects in scene
        public Background BG;
        public GraphicsComponent Overlay;

        public List<Transition> Transitions = new();

        //the float here refers to time from request time
        public Dictionary<PhysicsBody, float> BodiesRequestingUpdate = new();

        //temp
        public List<Hitbox> hitboxestodraw = new();

        public Scene(int ID, int D, int W, int H, GraphicsComponent Overlay, WorldBody world)
        {
            this.world = world;
            this.ID = ID;
            BG = new Background(new() { new Layer(Tester.TestBackground, Vector2.Zero, 0.5f, Vector2.Zero, Vector2.One, Vector2.Zero) }, this);
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
            BG = new Background(new() { new Layer(Tester.TestBackground, Vector2.Zero, 0.5f, Vector2.Zero, Vector2.One, Vector2.Zero) }, this);
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
            BG = new Background(new() { new Layer(Tester.TestBackground, Vector2.Zero, 0.5f, Vector2.Zero, Vector2.One, Vector2.Zero) }, this);
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
            BG = new Background(new() { new Layer(Tester.TestBackground, Vector2.Zero, 0.5f, Vector2.Zero, Vector2.One, Vector2.Zero) }, this);
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
            BG = new Background(new() { new Layer(Tester.TestBackground, Vector2.Zero, 0.5f, Vector2.Zero, Vector2.One, Vector2.Zero) }, this);
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
            BG = new Background(new() { new Layer(Tester.TestBackground, Vector2.Zero, 0.5f, Vector2.Zero, Vector2.One, Vector2.Zero) }, this);
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
            BG = new Background(new() { new Layer(Tester.TestBackground, Vector2.Zero, 0.5f, Vector2.Zero, Vector2.One, Vector2.Zero) }, this);
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
            BG = new Background(new() { new Layer(Tester.TestBackground, Vector2.Zero, 0.5f, Vector2.Zero, Vector2.One, Vector2.Zero) }, this);
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
        }

        //don't update everything at once, only the requested ones
        public void Update()
        {
            try
            {
                //avoiding foreach to prevent invalidoperation exception
                for (int i = 0; i < EntitiesInScene.Count; i++)
                {
                    EntitiesInScene[i]?.Update();
                    //else body.Update();
                }

                foreach (PhysicsBody body in BodiesRequestingUpdate.Where(kvp => kvp.Value <= GamePanel.Time * GamePanel.ElapsedTime).Select(kvp => kvp.Key))
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
            if (!BodiesInScene.Contains(body))
                return false;

            BodiesInScene.Remove(body);

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

        public void OnDestruction() { }

        public void Reload() { }

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
    }

    public class Background
    {
        readonly Scene Scene;
        public List<Layer> layers;
        public Vector2 pos, size;
        public float ScrollSpeed;

        public Background(List<Layer> layers, Scene Scene)
        {
            this.layers = layers;
            this.Scene = Scene;
        }

        public void Draw(Vector2 POV, Camera Cam, SpriteBatch spriteBatch)
        {
            //draw the deepest layer to shallowest layer of backgrounds, fix to pixels
            foreach (Layer l in layers)
            {
                Vector2 Displacement = l.DisplacementScrollSpeed * (POV - l.Pos);
                Vector2 DrawposInScene = new Vector2(l.Pos.X, l.Pos.Y + l.Size.Y) + Displacement;
                DrawposInScene -= new Vector2(DrawposInScene.X % WorldBody.PXSize, DrawposInScene.Y % WorldBody.PXSize);
                Vector2 Drawpos = Renderer.ToScreenPos(DrawposInScene, Cam.pos);
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
        public Vector2 SelfScrollSpeed { get; set; }
        public Vector2 MaxDisplacement, MinDisplacement;

        public Layer(Texture2D Texture, Vector2 Pos, float DisplacementScrollSpeed, Vector2 SelfScrollSpeed, Vector2 MaxDisplacement, Vector2 MinDisplacement)
        {
            this.Texture = Texture;
            this.Pos = Pos;
            this.MaxDisplacement = MaxDisplacement;
            this.MinDisplacement = MinDisplacement; //in ratio of size
            if (Texture != null)
                Size = new Vector2((float)Texture.Width / GamePanel.PixelsInTile, (float)Texture.Height / GamePanel.PixelsInTile);
            else Size = Vector2.Zero;
            this.DisplacementScrollSpeed = DisplacementScrollSpeed;
            this.SelfScrollSpeed = SelfScrollSpeed;
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

    public struct Transition
    {
        public Hitbox Area;
        public int R1;
        public Vector2 SpawnPosInR1;
        public int R2;
        public Vector2 SpawnPosInR2;

        public Transition(Hitbox Area, int R1, Vector2 SpawnPosInR1, int R2, Vector2 SpawnPosInR2)
        {
            this.Area = Area;
            this.R1 = R1;
            this.SpawnPosInR1 = SpawnPosInR1;
            this.R2 = R2;
            this.SpawnPosInR2 = SpawnPosInR2;
        }
    }
}