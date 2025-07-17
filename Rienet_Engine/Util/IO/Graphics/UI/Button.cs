using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Rienet
{
    /// <summary>
    /// A basic button with classic mouse and keyboard navigation, the key for selection (usually enter or z) are changable, 
    /// because this requires focus usually having a menu to update it would be better than using uihandler
    /// </summary>
    public class Button : UI
    {
        public static readonly Vector2 scale = new(GamePanel.TileSize / GamePanel.PixelsInTile * 2 / 3);
        public static bool AButtonWasPressed;
        public static bool AButtonPressedLock;

        public bool Selected { get; set; }
        public bool KeyboardSelected { get; set; }
        public bool MouseSelected { get; set; }
        public Action OnPress { get; set; }
        public Action WhenSelected { get; set; }
        public bool HaveKeySelect { get; set; } = true;
        public Keys KeyForSelection { get; set; } = Keys.Enter;

        //optional, a button doesn't really need text, if needed for debugging just use override and copy some old code, or just Debug.WriteLine() really
        //Vector2 relativeTextPos;
        //public Text textUI { get; set; }

        protected bool isSheeted;
        protected Rectangle Source;

        /// <summary>
        /// A basic button with classic mouse and keyboard navigation, the key for selection (usually enter or z) are changable, 
        /// because this requires focus usually having a menu to update it would be better than using uihandler
        /// </summary>
        public Button(Vector2 Pos, Vector2 RawSize, int sheetX, int sheetY, int pxwidth, int pxheight, bool UpdateFirst) : base(Pos, RawSize, UpdateFirst)
        {
            OnPress = DefaultOnPress;

            Source = new(pxwidth * sheetX, pxheight * sheetY, pxwidth, pxheight);
        }

        public Button(Vector2 Pos, Vector2 RawSize, bool UpdateFirst) : base(Pos, RawSize, UpdateFirst)
        {
            OnPress = DefaultOnPress;
        }

        public override void Update()
        {
            DefaultGraphics?.Update();
            MouseSelected = false;
            //if selected and hit enter, or left click mouse, onpress()
            if (Selected && DeviceInput.keyboardInfo.Pressed(KeyForSelection))
                OnPress();
            //if hovered over by mouse, selected = true, keyboard selection is more absolute and lasting
            if (DeviceInput.mouseInfo.InArea(new(Pos.X * GamePanel.ScreenToNormalResolutionRatio, Pos.Y * GamePanel.ScreenToNormalResolutionRatio, Scale.X * RawSize.X * GamePanel.ScreenToNormalResolutionRatio, Scale.Y * RawSize.Y * GamePanel.ScreenToNormalResolutionRatio)))
            {
                MouseSelected = true;
                Selected = true;

                if ((!(AButtonPressedLock && AButtonWasPressed)) && Selected && DeviceInput.mouseInfo.LeftOrRightPressed())
                {
                    OnPress();
                    AButtonWasPressed = true;
                }
            }
            else if (!KeyboardSelected) Selected = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //fixed size textbox
            if (isSheeted)
                GraphicsRenderer.DrawSpriteInSheet((SpriteSheet)DefaultGraphics, spriteBatch, Source);
            else
                base.Draw(spriteBatch);

            //something like this i'd prefer an override for versatility, leaving this comment here for romance
            //if (Selected && ShowSelectedGraphics)
            //GraphicsRenderer.DrawComponent(SelectedGraphics, spriteBatch);
        }

        void DefaultOnPress()
        {
            Debug.WriteLine("Pressed");
            Selected = false;
        }

        public void OnDestruction()
        {
            UIHandler.RemoveUI(this);
        }
    }
}