using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Rienet
{
    public static class GraphicsRenderer
    {
        public static Color OverlayColors(Color c1, Color c2)
        {
            float m = c2.A * (255 - c1.A) / 255;
            float aOut = c1.A + m,
                rOut = (c1.R * c1.A + c2.R * m) / aOut,
                gOut = (c1.G * c1.A + c2.G * m) / aOut,
                bOut = (c1.B * c1.A + c2.B * m) / aOut;
            return new Color(rOut, gOut, bOut, aOut / 255f);
        }

        public static void SetRenderTarget(RenderTarget2D target, SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            if (target != null)
            {
                spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, default);
                GamePanel.graphicsDevice.SetRenderTarget(target);
            }
        }

        //these are conversions between window size and graphics display (rendertarget)
        public static Vector2 ToWindowPos(Vector2 RenderTargetPos)
        {
            return RenderTargetPos * GamePanel.ScreenToNormalResolutionRatio + GamePanel.targetDrawPos;
        }

        public static Vector2 ToRenderTargetPos(Vector2 WindowPos)
        {
            return (WindowPos - GamePanel.targetDrawPos) / GamePanel.ScreenToNormalResolutionRatio;
        }

        //these are conversions between graphics display and world coordinates
        public static Vector2 ToScreenPos(Vector2 WorldPos, Vector2 CenterPos)
        {
            float x = WorldPos.X - CenterPos.X, y = WorldPos.Y - CenterPos.Y;
            x *= GamePanel.TileSize; y *= -GamePanel.TileSize;
            x += GamePanel.renderTargetSize.X * 0.5f;
            y += GamePanel.renderTargetSize.Y * 0.5f;
            return new Vector2(x, y);
        }

        public static Vector2 ToWorldPos(Vector2 ScreenPos, Vector2 CenterPos)
        {
            float x = ScreenPos.X, y = ScreenPos.Y;
            x -= GamePanel.renderTargetSize.X * 0.5f;
            y -= GamePanel.renderTargetSize.Y * 0.5f;
            x /= GamePanel.TileSize; y /= -GamePanel.TileSize;
            return new Vector2(x + CenterPos.X, y + CenterPos.Y);
        }

        public static bool DrawPosInScreen(Vector2 DrawPos, Vector2 Size)
        {
            return DrawPos.X + Size.X >= 0 && DrawPos.Y + Size.Y >= 0 && DrawPos.X <= GamePanel.renderTargetSize.X && DrawPos.Y <= GamePanel.renderTargetSize.Y;
        }

        public static bool DrawPosInVirtualScreen(Vector2 DrawPos, Vector2 Size, Vector2 VirtualScreenSize)
        {
            return DrawPos.X + Size.X >= 0 && DrawPos.Y + Size.Y >= 0 && DrawPos.X <= VirtualScreenSize.X && DrawPos.Y <= VirtualScreenSize.Y;
        }

        /// <summary>
        /// Only actual necessary method than drawcomponent, lights have different areas, this is to avoid light being culled
        /// </summary>
        public static void DrawLight(LightSource light, Vector2 SourcePos, Vector2 CenterPos, SpriteBatch spriteBatch)
        {
            Vector2 DrawPos = ToScreenPos(SourcePos, CenterPos);
            light.Pos = DrawPos;
            Vector2 size = light.ShownSize * GamePanel.PixelsInTile * light.Scale;

            if ((light is UniformLight && DrawPosInScreen(DrawPos, size)) || DrawPosInScreen(DrawPos - (0.5f * size), size))
                light.Draw(spriteBatch);
        }

        public static void DrawSpriteInSheet(SpriteSheet spriteSheet, Vector2 SourcePos, Vector2 CenterPos, SpriteBatch spriteBatch, Rectangle source)
        {
            Vector2 DrawPos = ToScreenPos(SourcePos, CenterPos);
            spriteSheet.Pos = DrawPos;

            if (DrawPosInScreen(DrawPos, spriteSheet.IndividualSize * spriteSheet.Scale))
                spriteSheet.Draw(spriteBatch, source);
        }

        public static void DrawSpriteInSheet(SpriteSheet spriteSheet, Vector2 SourcePos, Vector2 CenterPos, SpriteBatch spriteBatch)
        {
            Vector2 DrawPos = ToScreenPos(SourcePos, CenterPos);
            spriteSheet.Pos = DrawPos;

            if (DrawPosInScreen(DrawPos, spriteSheet.IndividualSize * spriteSheet.Scale))
                spriteSheet.Draw(spriteBatch);
        }

        public static void DrawSpriteInSheet(SpriteSheet spriteSheet, SpriteBatch spriteBatch, Rectangle source)
        {
            if (DrawPosInScreen(spriteSheet.Pos, spriteSheet.IndividualSize * spriteSheet.Scale))
                spriteSheet.Draw(spriteBatch, source);
        }

        public static void DrawSpriteInSheet(SpriteSheet spriteSheet, SpriteBatch spriteBatch)
        {
            if (DrawPosInScreen(spriteSheet.Pos, spriteSheet.IndividualSize * spriteSheet.Scale))
                spriteSheet.Draw(spriteBatch);
        }

        public static void DrawImage(Image image, Vector2 SourcePos, Vector2 CenterPos, SpriteBatch spriteBatch)
        {
            Vector2 DrawPos = ToScreenPos(SourcePos, CenterPos);
            image.Pos = DrawPos;

            if (DrawPosInScreen(DrawPos, image.ShownSize * image.Scale))
                image.Draw(spriteBatch);
        }

        public static void DrawImage(Image image, SpriteBatch spriteBatch)
        {
            if (DrawPosInScreen(image.Pos, image.ShownSize * image.Scale))
                image.Draw(spriteBatch);
        }

        public static void DrawComponent(GraphicsComponent graphicsComponent, Vector2 SourcePos, Vector2 CenterPos, SpriteBatch spriteBatch)
        {
            Vector2 DrawPos = ToScreenPos(SourcePos, CenterPos);
            graphicsComponent.Pos = DrawPos;

            if (DrawPosInScreen(DrawPos, graphicsComponent.ShownSize * graphicsComponent.Scale))
                graphicsComponent.Draw(spriteBatch);
        }

        public static void DrawComponent(GraphicsComponent graphicsComponent, SpriteBatch spriteBatch)
        {
            if (graphicsComponent != null && DrawPosInScreen(graphicsComponent.Pos, graphicsComponent.ShownSize * graphicsComponent.Scale))
                graphicsComponent.Draw(spriteBatch);
        }

        public static void DrawLineSegment(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, int lineWidth, Texture2D blankRect)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);

            spriteBatch.Draw(blankRect, point1, null, color, angle, Vector2.Zero, new Vector2(length, lineWidth), SpriteEffects.None, 0f);
        }

        public static void DrawPolygon(SpriteBatch spriteBatch, Vector2[] vertex, int count, Color color, int lineWidth, Texture2D blankRect)
        {
            if (count > 0)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    DrawLineSegment(spriteBatch, vertex[i], vertex[i + 1], color, lineWidth, blankRect);
                }
                DrawLineSegment(spriteBatch, vertex[count - 1], vertex[0], color, lineWidth, blankRect);
            }
        }

        public static void DrawRectangle(SpriteBatch spriteBatch, Vector2 pos, Vector2 size, Vector2 CenterPos, Color color, Texture2D blankRect)
        {
            Vector2 drawPos = ToScreenPos(new Vector2(pos.X, pos.Y + size.Y), CenterPos);
            Vector2 drawSize = new Vector2(size.X, size.Y) * GamePanel.TileSize;

            if (DrawPosInScreen(drawPos, drawSize))
            {
                spriteBatch.Draw(blankRect, drawPos, null, color, 0f, Vector2.Zero, drawSize, SpriteEffects.None, 0f);
            }
        }

        public static void DrawCircle(SpriteBatch spritebatch, Vector2 pos, float radius, Vector2 CenterPos, Color color, GraphicsDevice device)
        {
            Vector2 drawPos = ToScreenPos(new Vector2(pos.X - radius, pos.Y + radius), CenterPos);
            Vector2 drawSize = new Vector2(radius * 2, radius * 2) * GamePanel.TileSize;

            if (DrawPosInScreen(drawPos, drawSize))
            {
                //try to make one of these cached during circle creation to reduce lag, but considering how rare this method is gonna be called there's probably no need
                spritebatch.Draw(GetCircleTexture(radius * GamePanel.TileSize, color, device), drawPos, null, color, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
            }
        }

        public static Texture2D GetCircleTexture(float Radius, Color color, GraphicsDevice graphicsDevice)
        {
            int Diameter = (int)(Radius * 2);
            Texture2D texture = new(graphicsDevice, Diameter, Diameter);
            Color[] colorData = new Color[Diameter * Diameter];

            float RadiusSq = Radius * Radius;

            for (int x = 0; x < Diameter; x++)
            {
                for (int y = 0; y < Diameter; y++)
                {
                    int index = x * Diameter + y;
                    Vector2 pos = new(x - Radius, y - Radius);
                    if (pos.LengthSquared() <= RadiusSq)
                    {
                        colorData[index] = color;
                    }
                    else
                    {
                        colorData[index] = Color.Transparent;
                    }
                }
            }

            texture.SetData(colorData);
            return texture;
        }

        public static void ColorTexture(Color color, Texture2D texture)
        {
            Color[] textureColor = new Color[texture.Width * texture.Height];
            texture.GetData(textureColor);

            //yes i can't cache this cause it's a structure, it pains me too
            for (int i = 0; i < textureColor.Length; i++)
                if (textureColor[i] != Color.Transparent)
                    textureColor[i] = color;

            texture.SetData(textureColor);
        }

        public static void ColorTexture(Color color, Rectangle area, Texture2D texture)
        {
            //colors a specific rectangle in the texture
            try
            {
                Color[] textureColor = new Color[texture.Width * texture.Height];
                texture.GetData(textureColor);

                for (int X = area.X; X < area.X + area.Width; X++)
                    for (int Y = area.Y; Y < area.Y + area.Height; Y++)
                    {
                        textureColor[Y * texture.Width + X] = color;
                    }

                texture.SetData(textureColor);
            }
            catch (Exception e) //in case out of area
            {
                Debug.WriteLine(e.ToString());
            }
        }

        //add a method for overlaying textures or colors with low opacity here
    }
}