using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public class AnimatedSheet : SpriteSheet
    {
        //each individual animation is organized horizontally
        //the animations are listed vertically
        readonly Animator animator;

        public AnimatedSheet(Vector2 Pos, Vector2 Size, Texture2D Texture, int SplitX, int SplitY, bool Animating) : base(Pos, Size, Texture, SplitX, SplitY)
        {
            animator = new(Animating)
            {
                Animations = new()
            };
        }

        public AnimatedSheet()
        {
        }

        public override void Update()
        {
            base.Update();

            animator.Update();
            IndexX = CurrentFrame;
            IndexY = AnimationID;
        }

        public void AddAnimation(int Key, Animation Value)
        {
            animator.AddAnimation(Key, Value);
        }

        public void SetAnimation(int ID)
        {
            animator.SetAnimation(ID);
        }

        public void Pause(bool HasDesignatedFrame, int DesignatedFrame)
        {
            animator.Pause(HasDesignatedFrame, DesignatedFrame);
        }

        public void Reset()
        {
            animator.Reset();
        }

        public void Resume() => animator.Resume();

        public int CurrentFrame
        {
            get { return animator.CurrentFrame; }
        }

        public int AnimationID
        {
            get { return animator.AnimationID; }
        }

        public bool Animating => animator.Animating;
    }

    public class SpriteSheet : Image
    {
        public int SplitX, SplitY;
        protected Rectangle DrawSourceRectangle;
        protected int IndexX, IndexY;

        public SpriteSheet(Vector2 Pos, Vector2 Size, Texture2D Texture, int SplitX, int SplitY) : base(Pos, Size, Texture)
        {
            this.SplitX = SplitX;
            this.SplitY = SplitY;
            ShownSize = new Vector2(SplitX, SplitY);
        }

        public SpriteSheet()
        {
        }

        public override void Update()
        {
            ShownSize = new Vector2(SplitX, SplitY);
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle source)
        {
            spriteBatch.Draw(Texture, Pos, source, Tint, Rotation, Vector2.Zero, Scale, Effects, Depth);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle SourceRect = new(IndexX * SplitX, IndexY * SplitY, SplitX, SplitY);
            spriteBatch.Draw(Texture, Pos, SourceRect, Tint, Rotation, Vector2.Zero, Scale, Effects, Depth);
        }

        public override void Replace(Color color, Rectangle source)
        {
            try
            {
                Color[] textureColor = new Color[Texture.Width * Texture.Height];

                int PX = IndexX * SplitY, PY = IndexY * SplitY;

                for (int X = source.X + PX; X < source.X + source.Width + PX; X++)
                    for (int Y = source.Y + PY; Y < source.Y + source.Height + PY; Y++)
                    {
                        textureColor[Y * Texture.Width + X] = color;
                    }

                Texture.SetData(textureColor);
            }
            catch (Exception e) //in case out of area
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public Vector2 IndividualSize
        {
            get { return new Vector2(SplitX, SplitY); }
            set { SplitX = (int)value.X; SplitY = (int)value.Y; }
        }
    }

    public class Image : GraphicsComponent
    {
        public Texture2D Texture;

        public Image(Vector2 Pos, Vector2 RawSize, Texture2D Texture) : base(Pos, RawSize)
        {
            this.Texture = Texture;
            if (RawSize == Vector2.Zero)
                this.RawSize = new Vector2(Texture.Width, Texture.Height);
            ShownSize = this.RawSize;
        }

        public Image()
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Pos, null, Tint, Rotation, Vector2.Zero, Scale, Effects, Depth);
        }

        public virtual void Replace(Color color, Rectangle source)
        {
            try
            {
                Color[] textureColor = new Color[Texture.Width * Texture.Height];
                Texture.GetData(textureColor);

                for (int X = source.X; X < source.X + source.Width; X++)
                    for (int Y = source.Y; Y < source.Y + source.Height; Y++)
                    {
                        textureColor[Y * Texture.Width + X] = color;
                    }

                Texture.SetData(textureColor);
            }
            catch (Exception e) //in case out of area
            {
                Debug.WriteLine(e.ToString());
            }
        }

        //source 1 refers to texture's, source 2 refers to tex's
        public virtual void Replace(Texture2D tex, Rectangle source1, Rectangle source2)
        {
        }
    }

    public abstract class GraphicsComponent
    {
        internal Texture2D BlankRect = new(GamePanel.Instance.GraphicsDevice, 1, 1);

        public Vector2 Pos;
        public Vector2 RawSize;
        public Vector2 ShownSize;
        public Vector2 Scale = new(GamePanel.TileSize / GamePanel.PixelsInTile);
        public float Rotation;
        public Color Tint = Color.White;
        public SpriteEffects Effects = SpriteEffects.None;
        public int Depth = 0;

        protected GraphicsComponent(Vector2 Pos, Vector2 RawSize)
        {
            //position and size are in screen pixel units
            this.Pos = Pos;
            this.RawSize = RawSize;
            ShownSize = RawSize;
        }

        protected GraphicsComponent()
        {
        }

        public virtual void Update()
        {
            ShownSize = RawSize;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(BlankRect, Pos, null, Tint, Rotation, Vector2.Zero, Scale, Effects, Depth);
        }

        public float X
        {
            get { return Pos.X; }
            set { Pos.X = value; }
        }

        public float Y
        {
            get { return Pos.Y; }
            set { Pos.Y = value; }
        }

        public float Width
        {
            get { return RawSize.X; }
            set { RawSize.X = value; }
        }

        public float Height
        {
            get { return RawSize.Y; }
            set { RawSize.Y = value; }
        }

        public float ShownWidth
        {
            get { return ShownSize.X; }
            set { ShownSize.X = value; }
        }

        public float ShownHeight
        {
            get { return ShownSize.Y; }
            set { ShownSize.Y = value; }
        }
    }
}