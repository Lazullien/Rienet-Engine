using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public abstract class UI : GraphicsComponent
    {
        public GraphicsComponent DefaultGraphics;
        public bool Enabled = true;
        public bool Visible = true;
        public bool TakeInputFocus = false;

        protected UI(Vector2 Pos, Vector2 RawSize, bool UpdateFirst) : base(Pos, RawSize)
        {
            Enabled = UpdateFirst;
            Visible = UpdateFirst;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            GraphicsRenderer.DrawComponent(DefaultGraphics, spriteBatch);
        }
    }

    public static class UIHandler
    {
        //uis that can be loaded
        static readonly List<UI> UIsToHandle = new(); //parent array
        static readonly List<UI> UIsToUpdate = new();

        public static void AddUI(UI ui, bool UpdateHere)
        {
            UIsToHandle.Add(ui);
            if (UpdateHere) UIsToUpdate.Add(ui);
        }

        public static void RemoveUI(UI ui)
        {
            if (UIsToHandle.Contains(ui))
            {
                UIsToHandle.Remove(ui);
                UIsToUpdate.Remove(ui);
            }
        }

        public static void Update()
        {
            try
            {
                foreach (UI ui in UIsToUpdate)
                    if (ui.Enabled)
                    {
                        ui.Update();
                        if (ui.TakeInputFocus)
                            DeviceInput.TargetUI = ui;
                    }
            }
            catch (Exception e) {
                Debug.WriteLine(e);
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (UI ui in UIsToHandle)
                if (ui.Visible)
                    GraphicsRenderer.DrawComponent(ui, spriteBatch);
        }
    }
}