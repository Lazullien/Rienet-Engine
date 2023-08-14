using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Rienet
{
    public class Scene : IGameObject
    {
        public int ID;
        public Vector2 pos, startPos, size;
        public Hitbox[] Hitboxes;
        public Tile[,] SceneMap;
        public Dictionary<Vector2, HitboxChunk> HitboxChunks;
        public List<PhysicsBody> BodiesInScene;
        public List<Entity> EntitiesInScene; //eventually change this to objects in scene
        public Background BG;
        public GraphicsComponent Overlay;

        public List<Transition> Transitions;

        //the float here refers to time from request time
        public Dictionary<PhysicsBody, float> BodiesRequestingUpdate;

        //temp
        public List<Hitbox> hitboxestodraw = new();
        //Transition t = new(new Rectangle(0, 15, 1, 4), this, );

        public Scene(int ID, int W, int H, GraphicsComponent Overlay)
        {
            this.ID = ID;
            BG = new Background(new() { new Layer(Tester.TestBackground, Vector2.Zero, 0.5f, Vector2.Zero, Vector2.One, Vector2.Zero) }, this);
            this.W = W; this.H = H;
            SceneMap = new Tile[W, H];
            HitboxChunks = new();
            BodiesInScene = new();
            EntitiesInScene = new();

            Transitions = new();

            BodiesRequestingUpdate = new();
            this.Overlay = Overlay;
            OnCreation();
        }

        public Scene(int W, int H, GraphicsComponent Overlay)
        {
            BG = new Background(new() { new Layer(Tester.TestBackground, Vector2.Zero, 0.5f, Vector2.Zero, Vector2.One, Vector2.Zero) }, this);
            this.W = W; this.H = H;
            SceneMap = new Tile[W, H];
            HitboxChunks = new();
            BodiesInScene = new();
            EntitiesInScene = new();

            Transitions = new();

            BodiesRequestingUpdate = new();
            this.Overlay = Overlay;
            OnCreation();
        }

        public Scene(int ID, int W, int H)
        {
            BG = new Background(new() { new Layer(Tester.TestBackground, Vector2.Zero, 0.5f, Vector2.Zero, Vector2.One, Vector2.Zero) }, this);
            this.ID = ID;
            this.W = W; this.H = H;

            OnCreation();
        }

        public Scene(int W, int H)
        {
            BG = new Background(new() { new Layer(Tester.TestBackground, Vector2.Zero, 0.5f, Vector2.Zero, Vector2.One, Vector2.Zero) }, this);
            this.W = W; this.H = H;
            SceneMap = new Tile[W, H];
            HitboxChunks = new();
            BodiesInScene = new();
            EntitiesInScene = new();

            Transitions = new();

            BodiesRequestingUpdate = new();
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

        public bool GetGridInfo(Vector2 pos, out Tile tile)
        {
            int X = (int)pos.X, Y = (int)pos.Y;
            bool InRange = X >= 0 && X < W && Y >= 0 && Y < H;
            if (InRange) tile = SceneMap[X, Y]; else tile = null;
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

    public struct Transition
    {
        public Rectangle Area;
        public Scene R1;
        public Vector2 SpawnPosInR1;
        public Scene R2;
        public Vector2 SpawnPosInR2;

        public Transition(Rectangle Area, Scene R1, Vector2 SpawnPosInR1, Scene R2, Vector2 SpawnPosInR2)
        {
            this.Area = Area;
            this.R1 = R1;
            this.SpawnPosInR1 = SpawnPosInR1;
            this.R2 = R2;
            this.SpawnPosInR2 = SpawnPosInR2;
        }
    }
}