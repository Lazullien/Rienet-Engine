using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;

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

    public class UIHandler
    {
        //uis that can be loaded
        readonly List<UI> UIsToHandle = new(); //parent array
        readonly List<UI> UIsToUpdate = new();

        public void AddUI(UI ui, bool UpdateFirst)
        {
            UIsToHandle.Add(ui);
            if (UpdateFirst) UIsToUpdate.Add(ui);
        }

        public void RemoveUI(UI ui)
        {
            if (UIsToHandle.Contains(ui))
            {
                UIsToHandle.Remove(ui);
                UIsToUpdate.Remove(ui);
            }
        }

        public void Update()
        {
            DeviceInput.TargetUI = null;
            foreach (UI ui in UIsToUpdate)
                if (ui.Enabled)
                {
                    ui.Update();
                    if (ui.TakeInputFocus)
                        DeviceInput.TargetUI = ui;
                }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (UI ui in UIsToHandle)
                if (ui.Visible)
                    GraphicsRenderer.DrawComponent(ui, spriteBatch);
        }
    }
}