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
        public static Vector2 ToScreenPos(Vector2 WorldPos, Vector2 CenterPos, Scene Scene, GamePanel gp)
        {
            float x = (float)(WorldPos.X - CenterPos.X), y = (float)(WorldPos.Y - CenterPos.Y);
            x *= gp.TileSize; y *= -gp.TileSize;
            x += gp.Width / 2;
            y += gp.Height / 2;
            return new Vector2(x, y);
        }

        public static bool DrawPosInScreen(Vector2 DrawPos, Vector2 DrawBox, GamePanel gp)
        {
            return DrawPos.X + DrawBox.X * gp.TileSize >= 0 && DrawPos.Y + DrawBox.Y * gp.TileSize >= 0 && DrawPos.X <= gp.Width && DrawPos.Y <= gp.Height;
        }

        //account bodytexdif
        public static void DrawTile(Tile tile, Vector2 CenterPos, SpriteBatch sb, GamePanel gp)
        {
            Vector2 drawPos = ToScreenPos(new Vector2(tile.pos.X, tile.pos.Y + tile.DrawBox.Y), CenterPos, tile.BelongedScene, gp);

            if (DrawPosInScreen(drawPos, tile.DrawBox, gp))
            {
                sb.Draw(Tile.mono, drawPos, null, Color.White, 0, new Vector2(0, 0), new Vector2(gp.TileSize / GamePanel.PixelsInTile, gp.TileSize / GamePanel.PixelsInTile), SpriteEffects.None, 0);
            }
        }

        public static void DrawEntity(Entity e, Vector2 CenterPos, SpriteBatch sb, GamePanel gp)
        {
            Vector2 drawPos = ToScreenPos(new Vector2(e.pos.X, e.pos.Y + e.DrawBox.Y), CenterPos, e.BelongedScene, gp);

            if (DrawPosInScreen(drawPos, e.DrawBox, gp))
            {
                sb.Draw(e.current, drawPos, null, Color.White, 0, new Vector2(0, 0), new Vector2(gp.TileSize / GamePanel.PixelsInTile, gp.TileSize / GamePanel.PixelsInTile), SpriteEffects.None, 0);
            }
        }

        //fault here
        public static void DrawHitbox(Hitbox hb, Vector2 CenterPos, Scene BelongedScene, SpriteBatch sb, GamePanel gp, Texture2D blankRect)
        {
            Vector2 drawPos = ToScreenPos(new Vector2(hb.X, hb.Y + hb.H), CenterPos, BelongedScene, gp);
            Vector2 size = new Vector2(hb.W, hb.H) * gp.TileSize;

            if (DrawPosInScreen(drawPos, size, gp))
            {
                DrawRectangle(sb, new Rectangle((int)drawPos.X, (int)drawPos.Y, (int)size.X, (int)size.Y), Color.Red, 3, blankRect);
            }
        }

        public static void DrawCircularHitbox(CircularHitbox circ, Vector2 CenterPos, Scene BelongedScene, SpriteBatch sb, GamePanel gp, Texture2D blankCirc)
        {
            Vector2 drawPos = ToScreenPos(new Vector2(circ.X, circ.Y), CenterPos, BelongedScene, gp);
            float screenRad = circ.Radius * gp.TileSize;

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