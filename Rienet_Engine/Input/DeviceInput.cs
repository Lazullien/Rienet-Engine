using Microsoft.Xna.Framework.Input;

namespace Rienet
{
    public static class DeviceInput
    {
        public static KeyboardInfo keyboardInfo { get; private set; }
        public static MouseInfo mouseInfo { get; private set; }
        public static GamePadInfo gamePadInfo { get; private set; }

        internal static void Initialize()
        {
            keyboardInfo = new KeyboardInfo();
            mouseInfo = new MouseInfo();
            gamePadInfo = new GamePadInfo();
        }

        internal static void Update()
        {
            keyboardInfo.Update();
            mouseInfo.Update();
            gamePadInfo.Update();
        }

        public class KeyboardInfo
        {
            public bool Enabled = true;

            KeyboardState previousState;
            KeyboardState keyState;
            //this could be used to check if keys are held instead of clicked

            public void Update()
            {
                previousState = keyState;
                keyState = GamePanel.keyState;
            }

            public bool Triggered(Keys key)
            {
                if (Enabled)
                    return keyState.IsKeyDown(key);

                return false;
            }

            public bool Terminated(Keys key)
            {
                return keyState.IsKeyDown(key) && previousState.IsKeyDown(key);
            }

            public bool Pressed(Keys key)
            {
                if (Enabled)
                    return keyState.IsKeyDown(key) && !previousState.IsKeyDown(key);

                return false;
            }

            public bool Held(Keys key)
            {
                if (Enabled)
                    return keyState.IsKeyDown(key) && previousState.IsKeyDown(key);

                return false;
            }

            public bool Released(Keys key)
            {
                if (Enabled)
                    return previousState.IsKeyDown(key) && !keyState.IsKeyDown(key);

                return false;
            }
        }

        public class MouseInfo
        {
            public bool Enabled = true;

            MouseState previousState;
            MouseState mouseState;
            //this could be used to check if keys are held instead of clicked

            public void Update()
            {
                previousState = mouseState;
                mouseState = GamePanel.mouseState;
            }

/*
            public bool Triggered(Keys key)
            {
                if (Enabled)
                    return mouseState.(key);

                return false;
            }

            public bool Terminated(Keys key)
            {
                return mouseState.IsKeyDown(key) && previousState.IsKeyDown(key);
            }

            public bool Pressed(Keys key)
            {
                if (Enabled)
                    return mouseState.IsKeyDown(key) && !previousState.IsKeyDown(key);

                return false;
            }

            public bool Held(Keys key)
            {
                if (Enabled)
                    return mouseState.IsKeyDown(key) && previousState.IsKeyDown(key);

                return false;
            }

            public bool Released(Keys key)
            {
                if (Enabled)
                    return previousState.IsKeyDown(key) && !mouseState.IsKeyDown(key);

                return false;
            }
            */
        }

        public class GamePadInfo
        {
            public void Update()
            {
            }
        }
    }
}