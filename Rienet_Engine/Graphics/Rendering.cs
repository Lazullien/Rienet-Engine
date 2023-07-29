using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;

namespace Rienet
{
    public static class BasicRenderingAlgorithms
    {
        public static Vector2 ToScreenPos(Vector2 WorldPos, Vector2 CenterPos)
        {
            float x = (float)(WorldPos.X - CenterPos.X), y = (float)(WorldPos.Y - CenterPos.Y);
            x *= GamePanel.TileSize; y *= -GamePanel.TileSize;
            x += GamePanel.Width / 2;
            y += GamePanel.Height / 2;
            return new Vector2(x, y);
        }

        public static bool DrawPosInScreen(Vector2 DrawPos, Vector2 DrawBox)
        {
            return DrawPos.X + (DrawBox.X * GamePanel.TileSize) >= 0 && DrawPos.Y + (DrawBox.Y * GamePanel.TileSize) >= 0 && DrawPos.X <= GamePanel.Width && DrawPos.Y <= GamePanel.Height;
        }

        public static void DrawSpriteInSheet(SpriteSheet spriteSheet, Vector2 SourcePos, Vector2 CenterPos, SpriteBatch spriteBatch)
        {
            Vector2 DrawPos = ToScreenPos(SourcePos, CenterPos);
            spriteSheet.Pos = DrawPos;

            if (DrawPosInScreen(DrawPos, spriteSheet.IndividualSize * spriteSheet.Scale))
                spriteSheet.Draw(spriteBatch);
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
            if (DrawPosInScreen(graphicsComponent.Pos, graphicsComponent.ShownSize * graphicsComponent.Scale))
                graphicsComponent.Draw(spriteBatch);
        }

        //fault here
        public static void DrawHitbox(Hitbox hb, Vector2 CenterPos, Scene BelongedScene, SpriteBatch sb, GamePanel GamePanel, Texture2D blankRect)
        {
            Vector2 drawPos = ToScreenPos(new Vector2(hb.X, hb.Y + hb.H), CenterPos);
            Vector2 size = new Vector2(hb.W, hb.H) * GamePanel.TileSize;

            if (DrawPosInScreen(drawPos, size))
            {
                DrawRectangle(sb, new Rectangle((int)drawPos.X, (int)drawPos.Y, (int)size.X, (int)size.Y), Color.Red, 3, blankRect);
            }
        }

        public static void DrawCircularHitbox(CircularHitbox circ, Vector2 CenterPos, Scene BelongedScene, SpriteBatch sb, GamePanel GamePanel, Texture2D blankCirc)
        {
            Vector2 drawPos = ToScreenPos(new Vector2(circ.X, circ.Y), CenterPos);
            float screenRad = circ.Radius * GamePanel.TileSize;

            DrawCircle(sb, drawPos, screenRad, Color.Red, blankCirc);
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

        public static void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color, int lineWidth, Texture2D blankRect)
        {
            spriteBatch.Draw(blankRect, new Vector2(rectangle.X, rectangle.Y), null, Color.Chocolate, 0f, Vector2.Zero, new Vector2(rectangle.Width, rectangle.Height), SpriteEffects.None, 0f);
        }

        public static void DrawCircle(SpriteBatch spritebatch, Vector2 pos, float radius, Color color, Texture2D blankCirc)
        {
            spritebatch.Draw(blankCirc, new Vector2(pos.X - radius, pos.Y - radius), null, Color.Red, 0f, Vector2.Zero, new Vector2(radius * 2, radius * 2), SpriteEffects.None, 0f);
        }
    }
}