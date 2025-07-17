using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using Microsoft.Win32.SafeHandles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public class AnimatedSheet : SpriteSheet
    {
        //each individual animation is organized horizontally
        //the animations are listed vertically
        public Animator animator { get; protected set; }
        bool Disposed;

        public AnimatedSheet(Vector2 Pos, Vector2 Size, Texture2D Texture, int SplitX, int SplitY, bool Animating) : base(Pos, Size, Texture, SplitX, SplitY)
        {
            animator = new(Animating)
            {
                Animations = new()
            };
        }

        public AnimatedSheet()
        {
        }

        /// <summary>
        /// in case starting with a different animation than 0,0, MUST CALL THIS
        /// </summary>
        public void UpdateIndexXandY()
        {
            IndexX = CurrentFrame;
            IndexY = AnimationID;
        }

        public override void Update()
        {
            base.Update();

            if (!Disposed)
            {
                animator.Update();
                IndexX = CurrentFrame;
                IndexY = AnimationID;
            }
        }

        public void OnDestruction()
        {
            animator.Dispose();
            animator = null;
            Disposed = true;
        }

        public void AddAnimation(int Key, Animation Value)
        {
            animator.AddAnimation(Key, Value);
        }

        public void SetRefreshedAnimation(int ID)
        {
            animator.SetAnimation(ID);
            Reset();
            Resume();
        }

        public void SetRefreshedAnimationRegardless(int ID)
        {
            animator.SetAnimationRegardless(ID);
            Reset();
            Resume();
        }

        public void SetAnimation(int ID) => animator.SetAnimation(ID);

        public void Pause(bool HasDesignatedFrame, int DesignatedFrame) => animator.Pause(HasDesignatedFrame, DesignatedFrame);

        public void Reset() => animator.Reset();

        public void Resume() => animator.Resume();

        public int CurrentFrame
        {
            get { return animator.CurrentFrame; }
        }

        public int AnimationID
        {
            get { return animator.AnimationID; }
        }

        public bool Finished
        {
            get { return animator.Finished; }
            set { animator.Finished = value; }
        }

        public bool Animating => animator.Animating;
    }

    public class SpriteSheet : Image
    {
        public int SplitX { get; set; }
        public int SplitY { get; set; }
        protected Rectangle DrawSourceRectangle;
        public int IndexX { get; set; }
        public int IndexY { get; set; }

        public SpriteSheet(Vector2 Pos, Vector2 Size, Texture2D Texture, int SplitX, int SplitY) : base(Pos, Size, Texture)
        {
            this.SplitX = SplitX;
            this.SplitY = SplitY;
            ShownSize = new Vector2(SplitX, SplitY);
        }

        public SpriteSheet()
        {
        }

        public override void Update()
        {
            ShownSize = new Vector2(SplitX, SplitY);
        }
        /*
                public override Texture2D RotateTexture()
                {
                    float cosT = (float)Math.Cos(RotationPixelated), sinT = (float)Math.Sin(RotationPixelated);

                    Color[] textureColor = new Color[Texture.Width * Texture.Height];
                    Texture.GetData(textureColor);

                    int newSizeX, newSizeY;
                    if (RotationPixelated <= 0)
                    {
                        newSizeX = (int)Math.Ceiling(Math.Abs(ShownSize.X * cosT + ShownSize.Y * sinT));
                        newSizeY = (int)Math.Ceiling(Math.Abs(ShownSize.X * sinT + ShownSize.Y * cosT));
                    }
                    else
                    {
                        double t = RotationPixelated - 1.5707963;
                        double cost = Math.Cos(t), sint = Math.Sin(t);
                        newSizeX = (int)Math.Ceiling(Math.Abs(ShownSize.Y * cost + ShownSize.X * sint));
                        newSizeY = (int)Math.Ceiling(Math.Abs(ShownSize.Y * sint + ShownSize.X * cost));
                    }

                    Texture2D rotatedTexture = new(GamePanel.graphicsDevice, 2 * newSizeX, 2 * newSizeY);

                    Color[] rotatedTextureColor = new Color[rotatedTexture.Width * rotatedTexture.Height];

                    //find smallest x, y, if < 0 absolute value to displacement
                    float farx = ShownSize.X * (cosT - sinT), fary = ShownSize.Y * (sinT + cosT);
                    float displacementX = farx < 0 ? -farx : 0, displacementY = fary < 0 ? -fary : 0;

                    float ix = IndexX * ShownSize.X, iy = IndexY * ShownSize.Y;

                    for (int x = 0; x <= ShownSize.X; x++)
                        for (int y = 0; y <= ShownSize.Y; y++)
                        {
                            //test where to round for best results
                            float X = x * Math.Abs(cosT - sinT), Y = y * Math.Abs(sinT + cosT);
                            rotatedTextureColor[(int)(Y * rotatedTexture.Width + X)] = textureColor[(int)((y + iy) * ShownSize.X + (x + ix))];
                        }
                    rotatedTexture.SetData(rotatedTextureColor);
                    return rotatedTexture;
                }
        */
        public void Draw(SpriteBatch spriteBatch, Rectangle source)
        {
            spriteBatch.Draw(Texture, Pos, source, Tint, Rotation, Origin, Scale, Effects, Depth);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle SourceRect = new(IndexX * SplitX, IndexY * SplitY, SplitX, SplitY);
            spriteBatch.Draw(Texture, Pos, SourceRect, Tint, Rotation, Origin, Scale, Effects, Depth);
        }

        public override void Replace(Color color, Rectangle source)
        {
            try
            {
                Color[] textureColor = new Color[Texture.Width * Texture.Height];

                int PX = IndexX * SplitY, PY = IndexY * SplitY;

                for (int X = source.X + PX; X < source.X + source.Width + PX; X++)
                    for (int Y = source.Y + PY; Y < source.Y + source.Height + PY; Y++)
                    {
                        textureColor[Y * Texture.Width + X] = color;
                    }

                Texture.SetData(textureColor);
            }
            catch (Exception e) //in case out of area
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public Vector2 IndividualSize
        {
            get { return new Vector2(SplitX, SplitY); }
            set { SplitX = (int)value.X; SplitY = (int)value.Y; }
        }
    }

    public class Image : GraphicsComponent
    {
        public Texture2D Texture;

        public Image(Vector2 Pos, Vector2 RawSize, Texture2D Texture) : base(Pos, RawSize)
        {
            this.Texture = Texture;
            if (RawSize == Vector2.Zero)
                this.RawSize = new Vector2(Texture.Width, Texture.Height);
            ShownSize = this.RawSize;
        }

        public Image()
        {
        }
        /*
                /// <summary>
                /// rotating on every draw takes too much time, when using this store the resulting texture elsewhere and run draw through drawrotatedpixelated with that texture.  
                /// overridden in spritesheet to adapt to source, FAULTY
                /// </summary>
                public virtual Texture2D RotateTexture()
                {
                    float cosT = (float)Math.Cos(RotationPixelated), sinT = (float)Math.Sin(RotationPixelated);

                    Color[] textureColor = new Color[Texture.Width * Texture.Height];
                    Texture.GetData(textureColor);

                    Texture2D rotatedTexture = new(GamePanel.graphicsDevice, (int)Math.Ceiling(Math.Abs(Texture.Width * cosT + Texture.Height * sinT)), (int)Math.Ceiling(Math.Abs(Texture.Width * sinT + Texture.Height * cosT)));

                    Color[] rotatedTextureColor = new Color[rotatedTexture.Width * rotatedTexture.Height];

                    //find smallest x, y, if < 0 absolute value to displacement
                    float farx = ShownSize.X * (cosT - sinT), fary = ShownSize.Y * (sinT + cosT);
                    float displacementX = farx < 0 ? -farx : 0, displacementY = fary < 0 ? -fary : 0;

                    for (int x = 0; x <= ShownSize.X; x++)
                        for (int y = 0; y <= ShownSize.Y; y++)
                        {
                            //test where to round for best results
                            float X = x * (cosT - sinT) + displacementX, Y = y * (sinT + cosT) + displacementY;
                            rotatedTextureColor[(int)(Y * rotatedTexture.Width + X)] = textureColor[y * Texture.Width + x];
                        }
                    rotatedTexture.SetData(rotatedTextureColor);
                    return rotatedTexture;
                }
        */
        /// <summary>
        /// just an alternative that can be used in overriden draw methods for rotated, twisted textures...
        /// </summary>
        public void DrawAlteredTexture(Texture2D alteredTexture, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(alteredTexture, Pos, null, Tint, Rotation, Vector2.Zero, Scale, Effects, Depth);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Pos, null, Tint, Rotation, Origin, Scale, Effects, Depth);
        }

        public virtual void Replace(Color color, Rectangle source)
        {
            try
            {
                Color[] textureColor = new Color[Texture.Width * Texture.Height];
                Texture.GetData(textureColor);

                for (int X = source.X; X < source.X + source.Width; X++)
                    for (int Y = source.Y; Y < source.Y + source.Height; Y++)
                    {
                        textureColor[Y * Texture.Width + X] = color;
                    }

                Texture.SetData(textureColor);
            }
            catch (Exception e) //in case out of area
            {
                Debug.WriteLine(e.ToString());
            }
        }

        //source 1 refers to texture's, source 2 refers to tex's
        public virtual void Replace(Texture2D tex, Rectangle source1, Rectangle source2)
        {
        }
    }

    /// <summary>
    /// Just an outer tool wrapped around an existing GraphicsComponent (Image, LightSource...) to help with changes in components, usually involving complex graphical calculations, which is why SpriteSheet is separate from this.
    /// </summary>
    public class DynamicComponent : GraphicsComponent
    {
        public bool inWaveAlter { get; set; }
        public SineWave wave { get; set; }
        public Animator animator { get; set; }
        public Action ActionOnUpdate { get; set; } = delegate { };
        public Action ActionOnDraw { get; set; } = delegate { };
        public GraphicsComponent indented { get; private set; }

        /// <summary>
        /// USAGE OF WAVE AND ANIMATOR REQUIRES MANUAL INSTANTIATION TO AVOID GARBAGE
        /// </summary>
        public DynamicComponent(GraphicsComponent indented) : base(indented.Pos, Vector2.Zero)
        {
            this.indented = indented;
        }

        public override void Update()
        {
            ActionOnUpdate();
            indented.Update();
            base.Update();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            ActionOnDraw();
            indented.Draw(spriteBatch);
        }
    }

    /// <summary>
    /// To save computing power, in this case, use a singular texture, gamma updates affect whole texture, but don't affect scene brightness, draws an independant overlay to entire screen, draw method ran in Camera object projecttoscreen
    /// </summary>
    public class ScreenFlash : LightSource
    {
        //defined using conditions in constructor
        Texture2D flashTexture;
        public Vector2 WorldSize;

        public ScreenFlash(Vector2 InWorldPosLeftBottom, float Width, float Height, float Gamma, Color Tint, Scene scene, IGameObject belongedObject) : base(new(InWorldPosLeftBottom.X, InWorldPosLeftBottom.Y + Height), 0, Gamma, Tint, scene, belongedObject)
        {
            WorldSize = new Vector2(Width, Height);
            //pixel size
            RawSize = GamePanel.PixelsInTile * new Vector2(Width, Height);
            ShownSize = RawSize;
            SetTexture();
        }

        public void SetTexture()
        {
            flashTexture = new Texture2D(GamePanel.graphicsDevice, (int)ShownSize.X, (int)ShownSize.Y);

            Color[] data = new Color[(int)(ShownSize.X * ShownSize.Y)];
            for (int i = 0; i < data.Length; i++)
                data[i] = Color.White;
            flashTexture.SetData(data);

            Tint = new Color(Tint.R, Tint.G, Tint.B, (int)(Gamma * 255));
        }

        public override void Update()
        {
            Tint = new Color(Tint.R, Tint.G, Tint.B, (int)(Gamma * 255));
        }

        //gamma and tint applied in draw
        public override void Draw(SpriteBatch spriteBatch)
        {//POS FUCKED UP
            spriteBatch.Draw(flashTexture, Pos, null, Tint, Rotation, Origin, Scale, Effects, Depth);
        }
    }

    public class UniformLight : LightSource
    {
        public float WorldWidth;
        public float WorldHeight;
        public bool FadingBorders;
        public bool DirectionalFade;
        public Vector2 FadeVector;
        public Vector2 StartFadePos;

        public UniformLight(Vector2 InWorldPos, float Width, float Height, float Gamma, Color Tint, /*float FadeRadius, bool FadingBorders,*/ Scene scene, IGameObject belongedObject) : base(InWorldPos, /*FadeRadius,*/ 0, Gamma, Tint, scene, belongedObject)
        {
            WorldWidth = Width;
            WorldHeight = Height;
            ShownSize = new Vector2(Width, Height);
            //this.FadingBorders = FadingBorders;
            //if (FadingBorders)
            //    Radius = FadeRadius;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //increase opacity of tint towards the center, 0~gamma
            //decrease opacity of darkness in scene
            if (!GamePanel.Paused)
            {
                for (float x = InWorldPos.X - Radius; x < InWorldPos.X + WorldWidth + Radius; x += GamePanel.PixelSize)
                {
                    int XInPixels = (int)(x * GamePanel.PixelsInTile);
                    for (float y = InWorldPos.Y - Radius; y < InWorldPos.Y + WorldHeight + Radius; y += GamePanel.PixelSize)
                    {
                        /*
                        float dist = new Vector2(x - InWorldPos.X, y - InWorldPos.Y).Length();
                        if (dist <= Radius)
                        {
                            int i = (int)(sceneHeight - y * GamePanel.PixelsInTile) * sceneWidth + XInPixels;
                            if (i >= 0 && i < scene.LightMap.Length)
                            {
                                Color c = scene.LightMap[i];
                                float cA = c.A / 255f;
                                float nA = Gamma * (Radius - dist) / Radius;
                                Color nLL = new(c.R, c.G, c.B, cA - (1 - cA) * nA);
                                //now, add a layer, with tint colors and alpha, but with increasing gamma towards the center
                                Color nT = new(Tint.R, Tint.G, Tint.B, (nA - cA) * Tint.A / 255f);

                                scene.LightMap[i] = GraphicsRenderer.OverlayColors(nLL, nT);
                            }
                        }*/
                        int i = (int)(sceneHeight - y * GamePanel.PixelsInTile) * sceneWidth + XInPixels;
                        if (i >= 0 && i < scene.LightMap.Length)
                        {
                            //if directional fade, ADD THIS CODE LATER
                            //if fadingborders, ADD THIS CODE LATER
                            Color c = scene.LightMap[i];
                            float cA = c.A / 255f;
                            //float nA = Gamma * (Radius - dist) / Radius;
                            Color nLL = new(c.R, c.G, c.B, cA - (1 - cA) * Gamma);
                            //now, add a layer, with tint colors and alpha, but with increasing gamma towards the center
                            Color nT = new(Tint.R, Tint.G, Tint.B, (Gamma - cA) * Tint.A / 255f);

                            scene.LightMap[i] = GraphicsRenderer.OverlayColors(nLL, nT);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Dot lights
    /// </summary>
    public class LightSource : GraphicsComponent
    {
        public float Radius;
        public float Gamma;
        readonly protected Scene scene;
        readonly protected int sceneWidth, sceneHeight;
        public IGameObject belongedObject;
        public Vector2 InWorldPos;

        /// <summary>
        /// Special component drawn on a PIXEL MAP (unless specifically stated otherwise), remember scale multiplication is not auto
        /// </summary>
        /// <param name="Radius"> ALWAYS REFERRING TO TILES, THIS GOES TRANSLATED TO PIXELS BEFORE BEING DRAWN ON PIXEL MAP </param>
        public LightSource(Vector2 InWorldPos, float Radius, float Gamma, Color Tint, Scene scene, IGameObject belongedObject) : base(Vector2.Zero, new(Radius * 2))
        {
            this.InWorldPos = InWorldPos;
            this.Radius = Radius;
            ShownSize = new(Radius * 2);
            this.Gamma = Gamma;
            this.Tint = Tint;
            this.scene = scene;
            sceneWidth = (int)scene.W * GamePanel.PixelsInTile;
            sceneHeight = (int)scene.H * GamePanel.PixelsInTile;
            this.belongedObject = belongedObject;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //increase opacity of tint towards the center, 0~gamma
            //decrease opacity of darkness in scene
            if (!GamePanel.Paused)
                for (float x = InWorldPos.X - Radius; x < InWorldPos.X + Radius; x += GamePanel.PixelSize)
                {
                    int XInPixels = (int)(x * GamePanel.PixelsInTile);
                    for (float y = InWorldPos.Y - Radius; y < InWorldPos.Y + Radius; y += GamePanel.PixelSize)
                    {
                        float dist = new Vector2(x - InWorldPos.X, y - InWorldPos.Y).Length();
                        if (dist <= Radius)
                        {
                            int i = (int)(sceneHeight - y * GamePanel.PixelsInTile) * sceneWidth + XInPixels;
                            if (i >= 0 && i < scene.LightMap.Length)
                            {
                                Color c = scene.LightMap[i];
                                float cA = c.A / 255f;
                                float nA = Gamma * (Radius - dist) / Radius;
                                Color nLL = new(c.R, c.G, c.B, cA - (1 - cA) * nA);
                                //now, add a layer, with tint colors and alpha, but with increasing gamma towards the center
                                Color nT = new(Tint.R, Tint.G, Tint.B, (nA - cA) * Tint.A / 255f);

                                scene.LightMap[i] = GraphicsRenderer.OverlayColors(nLL, nT);
                            }
                        }
                    }
                }
        }
    }

    public abstract class GraphicsComponent
    {
        static internal Texture2D BlankRect = new(GamePanel.Instance.GraphicsDevice, 1, 1);

        public Vector2 WorldPos;
        public Vector2 Pos;
        public Vector2 RawSize;
        public Vector2 ShownSize;
        public Vector2 Scale = new(GamePanel.TileSize / GamePanel.PixelsInTile);
        /// <summary>
        /// in radians, anti clockwise
        /// </summary>
        public float Rotation { get; set; }
        public Vector2 Origin;
        public float RotationPixelated { get; set; }
        public Color Tint = Color.White;
        public SpriteEffects Effects { get; set; } = SpriteEffects.None;
        public int Depth { get; set; }

        public static Texture2D blankRect;

        public static void LoadBlankRectangle()
        {
            blankRect = new Texture2D(GamePanel.graphicsDevice, 1, 1);
            blankRect.SetData(new[] { Color.White });
        }

        protected GraphicsComponent(Vector2 Pos, Vector2 RawSize)
        {
            //position and size are in screen pixel units
            this.Pos = Pos;
            this.RawSize = RawSize;
            ShownSize = RawSize;
        }

        protected GraphicsComponent()
        {
        }

        public virtual void Update()
        {
            ShownSize = RawSize;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(BlankRect, Pos, null, Tint, Rotation, Origin, Scale, Effects, Depth);
        }

        public float X
        {
            get { return Pos.X; }
            set { Pos.X = value; }
        }

        public float Y
        {
            get { return Pos.Y; }
            set { Pos.Y = value; }
        }

        public float Width
        {
            get { return RawSize.X; }
            set { RawSize.X = value; }
        }

        public float Height
        {
            get { return RawSize.Y; }
            set { RawSize.Y = value; }
        }

        public float ShownWidth
        {
            get { return ShownSize.X; }
            set { ShownSize.X = value; }
        }

        public float ShownHeight
        {
            get { return ShownSize.Y; }
            set { ShownSize.Y = value; }
        }
    }
}