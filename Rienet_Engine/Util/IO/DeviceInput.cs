using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Rienet
{
    public static class DeviceInput
    {
        public static KeyboardInfo keyboardInfo { get; private set; }
        public static MouseInfo mouseInfo { get; private set; }
        public static GamePadInfo gamePadInfo { get; private set; }
        /// <summary>
        /// Always remember this will NOT be automatically switched to null, it needs to be manually done after use, usage of this to detect selection or deselection is MANUAL, do NOT expect any effects just by changing this, whether a deviceinput is counted is based on YOUR settings based on this variable
        /// </summary>
        public static UI TargetUI;

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

            public void Update()
            {
                previousState = keyState;
                keyState = GamePanel.keyState;
            }

            public Keys[] GetPressedKeys()
            {
                if (Enabled)
                    return keyState.GetPressedKeys();

                return null;
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

            public void Update()
            {
                previousState = mouseState;
                mouseState = GamePanel.mouseState;
            }

/// <summary>
/// as long as the button's down
/// </summary>
/// <returns></returns>
            public bool LeftClicked()
            {
                if (Enabled)
                    return mouseState.LeftButton == ButtonState.Pressed;
                return false;
            }

/// <summary>
/// new press
/// </summary>
/// <returns></returns>
            public bool LeftPressed()
            {
                if (Enabled)
                    return mouseState.LeftButton == ButtonState.Pressed && previousState.LeftButton == ButtonState.Released;
                return false;
            }

/// <summary>
/// pressed for multiple frames
/// </summary>
/// <returns></returns>
            public bool LeftHeld()
            {
                if (Enabled)
                    return mouseState.LeftButton == ButtonState.Pressed && previousState.LeftButton == ButtonState.Pressed;
                return false;
            }

            public bool LeftReleased()
            {
                if (Enabled)
                    return mouseState.LeftButton == ButtonState.Released && previousState.LeftButton == ButtonState.Pressed;
                return false;
            }

            public bool MiddleClicked()
            {
                if (Enabled)
                    return mouseState.MiddleButton == ButtonState.Pressed;
                return false;
            }

            public bool MiddlePressed()
            {
                if (Enabled)
                    return mouseState.MiddleButton == ButtonState.Pressed && previousState.MiddleButton == ButtonState.Released;
                return false;
            }

            public bool MiddleHeld()
            {
                if (Enabled)
                    return mouseState.MiddleButton == ButtonState.Pressed && previousState.MiddleButton == ButtonState.Pressed;
                return false;
            }

            public bool MiddleReleased()
            {
                if (Enabled)
                    return mouseState.MiddleButton == ButtonState.Released && previousState.MiddleButton == ButtonState.Pressed;
                return false;
            }

            public bool RightClicked()
            {
                if (Enabled)
                    return mouseState.RightButton == ButtonState.Pressed;
                return false;
            }

            public bool RightPressed()
            {
                if (Enabled)
                    return mouseState.RightButton == ButtonState.Pressed && previousState.RightButton == ButtonState.Released;
                return false;
            }

            public bool RightHeld()
            {
                if (Enabled)
                    return mouseState.RightButton == ButtonState.Pressed && previousState.RightButton == ButtonState.Pressed;
                return false;
            }

            public bool RightReleased()
            {
                if (Enabled)
                    return mouseState.RightButton == ButtonState.Released && previousState.RightButton == ButtonState.Pressed;
                return false;
            }

            public bool LeftOrRightClicked()
            {
                return LeftClicked() || RightClicked();
            }

            public bool LeftOrRightPressed()
            {
                return LeftPressed() || RightPressed();
            }

            public bool LeftOrRightHeld()
            {
                return LeftHeld() || RightHeld();
            }

            public bool LeftOrRightReleased()
            {
                return LeftReleased() || RightReleased();
            }

            public int ScrolledValue()
            {
                if (Enabled)
                    return mouseState.ScrollWheelValue;
                return 0;
            }

            public bool InArea(Hitbox Area)
            {
                return Collision.PointIntersectsHitbox(Position(), Area);
            }

            public Vector2 Position()
            {
                return mouseState.Position.ToVector2();
            }

            public Vector2 WorldPosition()
            {
                Vector2 pos = Position();
                //based on the camera it's in, and account screen scale
                foreach (var cam in GamePanel.CamerasOnDevice.Values)
                {
                    Hitbox area = new(GamePanel.targetDrawPos.X + cam.ScreenPos.X, GamePanel.targetDrawPos.Y + cam.ScreenPos.Y,
                            GamePanel.ScreenToNormalResolutionRatio * cam.ScreenSize.X, GamePanel.ScreenToNormalResolutionRatio * cam.ScreenSize.Y);
                    if (Collision.PointIntersectsHitbox(pos, area))
                    {
                        return GraphicsRenderer.ToWorldPos(GraphicsRenderer.ToRenderTargetPos(pos), cam.pos);
                    }
                }
                return -Vector2.One;
            }
        }

        public class GamePadInfo
        {
            public void Update()
            {
            }
        }
    }
}