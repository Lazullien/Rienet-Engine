using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public class AnimatedSheet : SpriteSheet
    {
        //each individual animation is organized horizontally
        //the animations are listed vertically
        public float PaceRate = 1;

        private Animation CurrentAnimation;
        private bool Animating;
        readonly bool Looping;
        //in seconds
        private float AnimationTimer;
        //this is separate from timer because timer time isn't always added by 1, this, being the index of the animation frames array, is
        private int AnimationFrame;
        private int CurrentFrame;

        public Dictionary<int, Animation> Animations;

        //vertical position in spritesheet
        public int AnimationID { get; private set; }

        public AnimatedSheet(Vector2 Pos, Vector2 Size, Texture2D Texture, int SplitX, int SplitY, bool Animating, bool Looping) : base(Pos, Size, Texture, SplitX, SplitY)
        {
            this.Animating = Animating;
            this.Looping = Looping;
        }

        public void AddAnimation(int Key, Animation Value)
        {
            if (!Animations.ContainsKey(Key))
                Animations.Add(Key, Value);
        }

        public override void Update()
        {
            if (Animating && CurrentAnimation.Delay > 0)
            {
                //update timer
                AnimationTimer += GamePanel.ElapsedTime * PaceRate;

                //on next frame
                if (Math.Abs(AnimationTimer) >= CurrentAnimation.Delay)
                {
                    AnimationFrame += Math.Sign(AnimationTimer);
                    AnimationTimer -= Math.Sign(AnimationTimer) * CurrentAnimation.Delay;

                    //if animation ended
                    if (AnimationFrame < 0 || AnimationFrame >= CurrentAnimation.Frames.Length)
                    {
                        //loops?
                        if (CurrentAnimation.Loop)
                        {
                            AnimationFrame = 0;
                            CurrentFrame = CurrentAnimation.Frames[AnimationFrame];
                        }
                        //else end
                        else
                        {
                            if (AnimationFrame < 0)
                                AnimationFrame = 0;
                            else
                                AnimationFrame = CurrentAnimation.Frames.Length - 1;

                            Animating = false;
                            AnimationTimer = 0;
                        }
                    }
                    //else continue
                    else
                    {
                        CurrentFrame = CurrentAnimation.Frames[AnimationFrame];
                    }
                }

                IndexX = CurrentFrame;
                IndexY = AnimationID;
            }
        }
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
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle SourceRect = new(IndexX * SplitX, IndexY * SplitY, SplitX, SplitY);
            spriteBatch.Draw(Texture, Pos, SourceRect, Tint, Rotation, Vector2.Zero, Scale, Effects, Depth);
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

        public Image(Vector2 Pos, Vector2 Size, Texture2D Texture) : base(Pos, Size)
        {
            this.Texture = Texture;
            if (Size != Vector2.Zero)
                this.Size = new Vector2(Texture.Width, Texture.Height);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Pos, null, Tint, Rotation, Vector2.Zero, Scale, Effects, Depth);
        }
    }

    public abstract class GraphicsComponent
    {
        public static Texture2D BlankRect = new(GamePanel.Instance.GraphicsDevice, 1, 1);
        public Vector2 Pos;
        public Vector2 Size;
        public Vector2 Scale = new(GamePanel.TileSize / GamePanel.PixelsInTile);
        public float Rotation;
        public Color Tint = Color.White;
        public SpriteEffects Effects = SpriteEffects.None;
        public int Depth = 0;

        protected GraphicsComponent(Vector2 Pos, Vector2 Size)
        {
            //position and size are in screen pixel units
            this.Pos = Pos;
            this.Size = Size;
        }

        public virtual void Update()
        {
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
            get { return Size.X; }
            set { Size.X = value; }
        }

        public float Height
        {
            get { return Size.Y; }
            set { Size.Y = value; }
        }
    }

    //horizontal
    public struct Animation
    {
        public int[] Frames;
        public float Delay;
        public bool Loop;
    }
}