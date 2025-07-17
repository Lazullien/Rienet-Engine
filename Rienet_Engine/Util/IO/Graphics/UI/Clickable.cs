using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    //clicked means they are in area (even if they're not physically, if they're not, than set state to either inarea or outarea)
    public enum ClickableState
    {
        InArea, OutArea, LeftClicked, RightClicked, MiddleClicked, OutAreaClicked
    }

    //an object in scene that can be clicked
    public class Clickable : Entity
    {
        public Action ActionNextUpdate { get; set; } = delegate { };
        public Action OnLeftClick { get; set; } = delegate { };
        public Action OnRightClick { get; set; } = delegate { };
        public Action OnScrollWheelClick { get; set; } = delegate { };
        public Action OnNoClick { get; set; } = delegate { };
        public Action InArea { get; set; } = delegate { };
        public Action OutArea { get; set; } = delegate { };
        public Action OutAreaClick { get; set; } = delegate { };

        public Hitbox Area { get; set; }

        public bool Enabled { get; set; } = true;
        public bool Blocked { get; set; } = false;

        //base method contains set scene
        public Clickable(Scene Scene, Hitbox Area) : base(Scene)
        {
            this.Area = Area;
            body = new PhysicsBody(this, new(Area.X, Area.Y), new Vector2(Area.W, Area.H), new() { Area }, 0.5f, 0, false, BelongedScene);
        }

        public override void SetScene(Scene BelongedScene)
        {
            this.BelongedScene?.SceneClickableStates.Remove(this);
            base.SetScene(BelongedScene);
            BelongedScene.SceneClickableStates.Add(this, ClickableState.OutArea);
        }

        public override void Update()
        {
            ActionNextUpdate();

            if (Enabled)
            {
                //if mouse within area and clicked
                var mouse = DeviceInput.mouseInfo;
                bool inArea = false;
                bool clicked = false;
                foreach (Camera cam in GamePanel.CamerasOnDevice.Values)
                {
                    //screen pos is up to down y++, so needs to add Area.H to raise the start pos
                    var AreaPos = GraphicsRenderer.ToScreenPos(new(Area.X + pos.X, Area.Y + pos.Y + Area.H), cam.pos);
                    Hitbox screenArea = new(AreaPos.X, AreaPos.Y, Area.W * GamePanel.TileSize, Area.H * GamePanel.TileSize);
                    if (mouse.InArea(screenArea))
                    {
                        inArea = true;
                        if (mouse.LeftPressed())
                        {
                            clicked = true;
                            BelongedScene.SceneClickableStates[this] = ClickableState.LeftClicked;
                            OnLeftClick();
                        }
                        else if (mouse.RightClicked())
                        {
                            clicked = true;
                            BelongedScene.SceneClickableStates[this] = ClickableState.RightClicked;
                            OnRightClick();
                        }
                        else if (mouse.MiddleClicked())
                        {
                            clicked = true;
                            BelongedScene.SceneClickableStates[this] = ClickableState.MiddleClicked;
                            OnScrollWheelClick();
                        }
                        else
                        {
                            OnNoClick();
                            continue;
                        }
                        break;
                    }
                }
                if (inArea)
                {
                    InArea();
                    if (!clicked)
                        BelongedScene.SceneClickableStates[this] = ClickableState.InArea;
                }
                else
                {
                    OutArea();
                    if (mouse.LeftPressed() || mouse.RightPressed() || mouse.MiddlePressed())
                    {
                        BelongedScene.SceneClickableStates[this] = ClickableState.OutAreaClicked;
                        OutAreaClick();
                    }
                    else
                    {
                        BelongedScene.SceneClickableStates[this] = ClickableState.OutArea;
                    }
                }
            }
            else ///can't be detected if not enabled
                BelongedScene.SceneClickableStates[this] = ClickableState.OutArea;

            Blocked = false;
        }

        public bool InOtherClickable(bool GetOtherClickables, out List<Clickable> OtherClickable)
        {
            OtherClickable = new();
            bool InOtherClickable = false;

            foreach (var clickable in BelongedScene.SceneClickableStates.Keys)
            {
                if (clickable != this)
                {
                    var state = BelongedScene.SceneClickableStates[clickable];
                    if (state == ClickableState.InArea || state == ClickableState.LeftClicked || state == ClickableState.RightClicked || state == ClickableState.MiddleClicked)
                    {
                        InOtherClickable = true;
                        if (!GetOtherClickables)
                            break;

                        OtherClickable.Add(clickable);
                    }
                }
            }

            return InOtherClickable;
        }

        public static bool InClickable(Scene BelongedScene, bool GetOtherClickables, out List<Clickable> OtherClickable)
        {
            OtherClickable = new();
            bool InOtherClickable = false;

            foreach (var clickable in BelongedScene.SceneClickableStates.Keys)
            {
                var state = BelongedScene.SceneClickableStates[clickable];
                if (state == ClickableState.InArea || state == ClickableState.LeftClicked || state == ClickableState.RightClicked || state == ClickableState.MiddleClicked)
                {
                    InOtherClickable = true;
                    if (!GetOtherClickables)
                        break;

                    OtherClickable.Add(clickable);
                }
            }

            return InOtherClickable;
        }

        public override void OnDestruction()
        {
            BelongedScene.SceneClickableStates.Remove(this);
            base.OnDestruction();
        }

        public override void Draw(Vector2 CenterPos, SpriteBatch spriteBatch, GamePanel gamePanel)
        {
            if (Enabled)
            {
                base.Draw(CenterPos, spriteBatch, gamePanel);
            }
        }
    }
}