using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public class AnimatedObject
    {

        Texture2D sprite;
        //change the parameter to Frame class instead
        List<Texture2D> tl = new List<Texture2D>();
        public Texture2D current;
        int frmIdx;
        //this keeps track of when frames should be updated
        ulong futureTick;
        public bool animated;
        bool empty;
        bool loop;
        float lastPace;

        public AnimatedObject()
        {
        }

        public AnimatedObject(Texture2D spriteSheet, int w, int h, bool loop)
        {
            this.sprite = spriteSheet;
            frmIdx = 0;
            int s = tl.Count;
            animated = s > 1;
            empty = s <= 0;
            this.loop = loop;
        }

        public void AddFrame(Texture2D frame)
        {
            tl.Add(frame);
            int s = tl.Count;
            animated = s > 1;
            empty = s <= 0;
            current = tl[frmIdx];
        }

        public void RemoveFrame(Texture2D frame)
        {
            tl.Remove(frame);
            int s = tl.Count;
            animated = s > 1;
            empty = s <= 0;
            current = tl[frmIdx];
        }

        //try setting new goal if pace has changed
        //base on self state and paused time to update rather than gametick time alone
        public void Animate(GamePanel gp, float pace)
        {
            //set the frame to be locked in a state until pace ends (reaches set future)
            //use graphicTick instead of gametick here
            //basically pace is fps, faster the pace, the higher the fps
            //if fps = -1 then don't change the state
            int fps = pace > 0 ? (int)(1 / pace) : -1;
            if (!empty && fps > 0 && gp.Time % (ulong) fps == 0)
            {
                futureTick = gp.Time + (ulong)(gp.FPS / pace);
                current = tl[frmIdx];
                if (animated) frmIdx = frmIdx < tl.Count - 1 ? frmIdx + 1 : loop ? 0 : frmIdx;
                lastPace = pace;
            }
        }
    }
}