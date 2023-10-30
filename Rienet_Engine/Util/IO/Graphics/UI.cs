using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;

namespace Rienet
{
    public abstract class UI : GraphicsComponent
    {
        public Texture2D DefaultTexture;
        public bool Enabled = true;
        public bool Visible = true;

        protected UI(Vector2 Pos, Vector2 RawSize, bool UpdateFirst) : base(Pos, RawSize)
        {
            GamePanel.uiHandler.AddUI(this, UpdateFirst);
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

        public void Update()
        {
            foreach (UI ui in UIsToUpdate)
                if (ui.Enabled)
                    ui.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (UI ui in UIsToHandle)
                if (ui.Enabled && ui.Visible)
                    GraphicsRenderer.DrawComponent(ui, spriteBatch);
        }
    }
}