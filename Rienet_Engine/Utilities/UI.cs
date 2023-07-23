using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public interface IUI
    {
        public void Update(GamePanel game);
        public void Draw(GamePanel panel);
    }

    public abstract class GUI : IUI
    {
        GamePanel game;
        public Texture2D DefaultTexture;
        public int X, Y, W, H; //On Screen

        public GUI(GamePanel game, bool UpdateFirst)
        {
            this.game = game;
            game.uiHandler.AddUI(this, UpdateFirst);
        }

        public virtual void Update(GamePanel game)
        {
        }

        public virtual void Draw(GamePanel panel)
        {
        }
    }

    public class UIHandler
    {
        GamePanel panel;

        //uis that can be loaded
        List<IUI> UIsToHandle; //parent array
        List<IUI> UIsToUpdate;
        List<GUI> GUIsToDraw;

        public UIHandler(GamePanel panel)
        {
            this.panel = panel;
            UIsToHandle = new List<IUI>();
            UIsToUpdate = new List<IUI>();
            GUIsToDraw = new List<GUI>();
        }

        public void AddUI(IUI ui, bool UpdateFirst)
        {
            UIsToHandle.Add(ui);

            if (UpdateFirst) UIsToUpdate.Add(ui);
            if (ui is GUI gui) GUIsToDraw.Add(gui);
        }

        public void Update()
        {
            foreach (IUI ui in UIsToUpdate)
                ui.Update(panel);
        }

        public void Draw()
        {
            foreach (GUI gui in GUIsToDraw)
                gui.Draw(panel);
        }
    }
}