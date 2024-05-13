using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    public class Text : UI
    {
        public string text;
        public string visibleText = "";
        public List<string> lines = new();
        //effects and fonts
        public SpriteFont spriteFont;
        public Color color = Color.White;
        public float TextScale;

        public Text(Vector2 Pos, float TextScale, Vector2 RawSize, bool UpdateFirst) : base(Pos, RawSize, UpdateFirst)
        {
            spriteFont = GamePanel.DefaultTextFont;
            this.TextScale = TextScale;
        }

        //REMEMBER TO ADD MULTIPLE WAYS TO SET LINES, THIS IS ONLY REALLY FOR LATIN BASED LANGUAGES
        public void SetLines()
        {
            lines.Clear();
            lines.Add("");
            string[] splitByWords = text.Split(" ");
            float previousXInLine = 0;
            int currentLineIndex = 0;

            foreach (var word in splitByWords)
            {
                var size = spriteFont.MeasureString(word + " ") * TextScale;
                previousXInLine += size.X;

                if (previousXInLine > ShownWidth)
                {
                    previousXInLine = 0;
                    currentLineIndex++;
                    lines.Add("");
                }
                lines[currentLineIndex] = lines[currentLineIndex].Insert(lines[currentLineIndex].Length, word + " ");
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //separate the text based on sizes, and draw each line individually, separated by space
            //get size of current line, compare it to ui size, if bigger, draw this line to the word before exceeding and move on to next line
            //
            float Height = spriteFont.MeasureString("l").Y + spriteFont.LineSpacing;

            int lastLength = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (visibleText.Length < lastLength + line.Length)
                {
                    spriteBatch.DrawString(spriteFont, visibleText[lastLength..], Pos + new Vector2(0, i * Height), color, 0, Vector2.Zero, TextScale, SpriteEffects.None, 0);
                    break;
                }
                lastLength += line.Length;

                spriteBatch.DrawString(spriteFont, line, Pos + new Vector2(0, i * Height), color, 0, Vector2.Zero, TextScale, SpriteEffects.None, 0);
            }
        }
    }
}