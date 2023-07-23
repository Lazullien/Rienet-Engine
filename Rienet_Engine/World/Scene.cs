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
        public int X, Y, StartX, StartY, W, H;
        public Hitbox[] Hitboxes;
        public Tile[,] SceneMap;
        //readonly Quadtree HitboxTree;
        public Dictionary<Vector2, HitboxChunk> HitboxChunks;
        public List<PhysicsBody> BodiesInScene;
        public List<Entity> EntitiesInScene; //as a symbol of existance
        public Background BG;

        public List<Transition> Transitions;

        public Dictionary<PhysicsBody, ulong> BodiesRequestingUpdate;

        //temp
        public List<Hitbox> hitboxestodraw = new();
        //Transition t = new(new Rectangle(0, 15, 1, 4), this, );
        GamePanel gp;

        public Scene(int W, int H, GamePanel gp)
        {
            //construct a temp world for now, change to json format load later
            //new Tile(new Vector2(x,y), gp);
            BG = new Background(new() { new Layer(Tester.TestBackground, Vector2.Zero, 0.5f, Vector2.Zero, Vector2.One, Vector2.Zero) }, this, gp);
            this.W = W; this.H = H;
            SceneMap = new Tile[W, H];
            HitboxChunks = new();
            BodiesInScene = new();
            EntitiesInScene = new();

            Transitions = new();

            BodiesRequestingUpdate = new();
            this.gp = gp;
            OnCreation();
        }

        public Scene(int ID, int W, int H, GamePanel gp)
        {
            BG = new Background(new() { new Layer(Tester.TestBackground, Vector2.Zero, 0.5f, Vector2.Zero, Vector2.One, Vector2.Zero) }, this, gp);
            this.ID = ID;
            this.W = W; this.H = H;

            this.gp = gp;
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

                foreach (PhysicsBody body in BodiesRequestingUpdate.Where(kvp => kvp.Value == gp.Time).Select(kvp => kvp.Key))
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

        public void OnDestruction()
        {
            //
        }

        public void Reload()
        {
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
            for (int x = (int)body.pos.X; x < body.pos.X + body.size.X; x += HitboxChunk.W)
            {
                for (int y = (int)body.pos.Y; y < body.pos.Y + body.size.Y; y += HitboxChunk.H)
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
            for (int x = (int)body.pos.X; x < body.pos.X + body.size.X; x += HitboxChunk.W)
            {
                for (int y = (int)body.pos.Y; y < body.pos.Y + body.size.Y; y += HitboxChunk.H)
                {
                    if (TryGetHitboxChunk(x, y, out HitboxChunk chunk))
                        chunk.AddBody(body);
                }
            }
        }
    }
}