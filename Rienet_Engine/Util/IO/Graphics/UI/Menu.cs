using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Rienet
{
    /// <summary>
    /// A higher level class for a common style of menu controls, 
    /// including a mouse based and keyboard based navigation system (from buttons class)
    /// when in focus only, update to check input
    /// </summary>
    public class Menu : UI
    {
        public Dictionary<int, Button> Options { get; protected set; }

        //having focus based controls, getting focus is important, MAYBE THIS DOESN'T NEED TO EXIST AND CAN USE ISACTIVATED OR ISVISIBLE INSTEAD
        protected bool InFocus = true;

        int currentSelection;
        Button previousSelection;
        public Keys NextOption { get; set; } = Keys.Down;
        public Keys PrevOption { get; set; } = Keys.Up;

        public Menu(Vector2 position, Vector2 rawSize, bool updateFirst, Dictionary<int, Button> options) : base(position, rawSize, updateFirst)
        {
            Options = options;
        }

        /// <summary>
        /// no shownsize = rawsize
        /// </summary>
        public override void Update()
        {
            if (InFocus)
            {
                //change current selection with keys
                if (DeviceInput.keyboardInfo.Pressed(NextOption) && currentSelection < Options.Count - 1)
                    currentSelection++;
                else if (DeviceInput.keyboardInfo.Pressed(PrevOption) && currentSelection > 0)
                    currentSelection--;

                if (previousSelection != null)
                {
                    previousSelection.Selected = false;
                    previousSelection.KeyboardSelected = false;
                }

                Button selected = Options[currentSelection];
                selected.Selected = true;
                selected.KeyboardSelected = true;
                previousSelection = selected;

                foreach (Button button in Options.Values)
                {
                    button.Update();
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            foreach (Button button in Options.Values)
                if (button.Visible)
                    button.Draw(spriteBatch);
        }
    }
}