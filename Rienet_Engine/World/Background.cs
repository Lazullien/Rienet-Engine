using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rienet
{
    public class Background : IGameObject
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
                Vector2 Drawpos = BasicRenderingAlgorithms.ToScreenPos(DrawposInScene, Cam.pos);
                spriteBatch.Draw(l.Texture, Drawpos, null, Color.White, 0, Vector2.Zero, new Vector2(GamePanel.TileSize / GamePanel.PixelsInTile, GamePanel.TileSize / GamePanel.PixelsInTile), SpriteEffects.None, 1);
            }
        }

        public void OnCreation()
        {
        }

        public void Update()
        {
        }

        public void OnDestruction()
        {
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
}