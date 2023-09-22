using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public abstract class UI
    {
        public Texture2D DefaultTexture;
        public int X, Y, W, H; //On Screen

        public bool Enabled = true;
        public bool Visible = true;

        public UI(bool UpdateFirst)
        {
            GamePanel.uiHandler.AddUI(this, UpdateFirst);
        }

        public abstract void Update();

        public abstract void Draw();
    }

    public class UIHandler
    {
        //uis that can be loaded
        List<UI> UIsToHandle = new(); //parent array
        List<UI> UIsToUpdate = new();

        public void AddUI(UI ui, bool UpdateFirst)
        {
            UIsToHandle.Add(ui);
            if (UpdateFirst) UIsToUpdate.Add(ui);
        }

        public void Update()
        {
            foreach (UI ui in UIsToUpdate)
                if (ui.Enabled)
                    ui.Update();
        }

        public void Draw()
        {
            foreach (UI ui in UIsToHandle)
                if (ui.Enabled && ui.Visible)
                    ui.Draw();
        }
    }
}