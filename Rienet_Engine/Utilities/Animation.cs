using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public class Animator
    {
        public float PaceRate = 1;

        private Animation CurrentAnimation;
        public bool Animating { get; private set; }
        //in seconds
        private float AnimationTimer;
        //this is separate from timer because timer time isn't always added by 1, this, being the index of the animation frames array, is
        public int AnimationFrame { get; private set; }
        public int CurrentFrame { get; private set; }

        public Dictionary<int, Animation> Animations;

        public int AnimationID;

        public Animator(bool Animating)
        {
            this.Animating = Animating;
            Animations = new();
        }

        public void AddAnimation(int Key, Animation Value)
        {
            if (!Animations.ContainsKey(Key))
                Animations.Add(Key, Value);
        }

        public void SetAnimation(int ID)
        {
            if (Animations.ContainsKey(ID))
                AnimationID = ID;
        }

        public void Pause(bool HasDesignatedFrame, int DesignatedFrame)
        {
            if (HasDesignatedFrame)
            {
                AnimationFrame = DesignatedFrame;
                CurrentFrame = DesignatedFrame;
            }

            Animating = false;
        }

        public void Reset()
        {
            AnimationFrame = 0;
            CurrentFrame = CurrentAnimation.Frames[AnimationFrame];
        }

        public void Resume() => Animating = true;

        public void Update()
        {
            CurrentAnimation = Animations[AnimationID];

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
            }
        }
    }

    public struct Animation
    {
        public int[] Frames;
        public float Delay;
        public bool Loop;

        public Animation(int FrameCount, float Delay, bool Loop)
        {
            Frames = new int[FrameCount];
            this.Delay = Delay;
            this.Loop = Loop;
        }

        public Animation(int[] Frames, float Delay, bool Loop)
        {
            this.Frames = Frames;
            this.Delay = Delay;
            this.Loop = Loop;
        }
    }
}