using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BallBreaker.GuiObjects
{
    public class GuiItem
    {
        private string header;
        private string text;
        private int posX;
        private int posY;
        private int width;
        private int height;
        private Texture2D grayCorner;
        private Texture2D darkGrayCorner;
        private Texture2D grayPixel;
        private Texture2D darkGrayPixel;
        private bool alignedLeft;
        private List<bool> roundedCorners;
        private SpriteFont textFont;
        SpriteFont headerFont;

        public GuiItem(
            string header,
            string text,
            int posX,
            int posY,
            int width,
            int height,
            Texture2D grayCorner,
            Texture2D darkGrayCorner,
            Texture2D grayPixel,
            Texture2D darkGrayPixel,
            SpriteFont textFont,
            SpriteFont headerFont,
            List<bool> roundedCorners,
            bool alignedLeft = true)
        {
            this.header = header;
            this.text = text;
            this.posX = posX;
            this.posY = posY;
            this.width = width;
            this.height = height;
            this.grayCorner = grayCorner;
            this.darkGrayCorner = darkGrayCorner;
            this.grayPixel = grayPixel;
            this.darkGrayPixel = darkGrayPixel;
            this.textFont = textFont;
            this.headerFont = headerFont;
            this.alignedLeft = alignedLeft;
            this.roundedCorners = roundedCorners;
        }

        public void UpdateText(string text)
        {
            this.text = text;
        }

        public string GetText()
        {
            return text;
        }

        public string GetHeader()
        {
            return header;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            DrawHeader(spriteBatch);

            var textSize = textFont.MeasureString(text);
            spriteBatch.DrawString(textFont, text, new Vector2(posX + width * 0.5f - textSize.X * 0.5f, posY + 57), Color.White);
        }

        private void DrawHeader(SpriteBatch spriteBatch)
        {
            int headerHeight = 32;
            int topMargin = 14;
            int sideMargin = 10;

            if (alignedLeft)
            {
                spriteBatch.Draw(
                    darkGrayPixel,
                    new Rectangle(posX, posY + topMargin, width - darkGrayCorner.Width - sideMargin, headerHeight),
                    new Rectangle(0, 0, 1, 1),
                    Color.White);

                spriteBatch.Draw(
                    darkGrayPixel,
                    new Rectangle(posX + width - darkGrayCorner.Width - sideMargin, posY + topMargin + darkGrayCorner.Height, darkGrayCorner.Width, headerHeight - darkGrayCorner.Height * 2),
                    new Rectangle(0, 0, 1, 1),
                    Color.White);

                spriteBatch.Draw(
                    darkGrayCorner,
                    new Rectangle(posX + width - darkGrayCorner.Width - sideMargin, posY + topMargin, darkGrayCorner.Width, darkGrayCorner.Height),
                    Color.White);

                spriteBatch.Draw(
                    darkGrayCorner,
                    new Vector2(posX + width - sideMargin, posY + headerHeight + darkGrayCorner.Height - 6),
                    null,
                    Color.White,
                    (float)Math.PI * 0.5f,
                    new Vector2(0, 0),
                    1,
                    SpriteEffects.None,
                    0f);

                var headerTextSize = headerFont.MeasureString(header);
                spriteBatch.DrawString(headerFont, header, new Vector2(posX + width - headerTextSize.X - sideMargin - darkGrayCorner.Width, posY + topMargin + 8), new Color(167, 167, 167));
            }
            else
            {
                spriteBatch.Draw(
                    darkGrayPixel,
                    new Rectangle(posX + sideMargin + darkGrayCorner.Width, posY + topMargin, width - darkGrayCorner.Width - sideMargin, headerHeight),
                    new Rectangle(0, 0, 1, 1),
                    Color.White);

                spriteBatch.Draw(
                    darkGrayPixel,
                    new Rectangle(posX + sideMargin, posY + topMargin + darkGrayCorner.Height, darkGrayCorner.Width, headerHeight - darkGrayCorner.Height * 2),
                    new Rectangle(0, 0, 1, 1),
                    Color.White);

                spriteBatch.Draw(
                    darkGrayCorner,
                    new Vector2(posX + sideMargin, posY + topMargin + darkGrayCorner.Height),
                    null,
                    Color.White,
                    (float)Math.PI * 1.5f,
                    new Vector2(0, 0),
                    1,
                    SpriteEffects.None,
                    0f);

                spriteBatch.Draw(
                    darkGrayCorner,
                    new Vector2(posX + sideMargin + darkGrayCorner.Width, posY + topMargin + headerHeight),
                    null,
                    Color.White,
                    (float)Math.PI,
                    new Vector2(0, 0),
                    1,
                    SpriteEffects.None,
                    0f);

                var headerTextSize = headerFont.MeasureString(header);
                spriteBatch.DrawString(headerFont, header, new Vector2(posX + sideMargin + darkGrayCorner.Width, posY + topMargin + 8), new Color(167, 167, 167));
            }

            
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {
            // Left upper corner
            if (roundedCorners[0] == true)
            {
                spriteBatch.Draw(
                    grayCorner,
                    new Vector2(posX, posY + grayCorner.Height),
                    null,
                    Color.White,
                    (float)Math.PI * 1.5f,
                    new Vector2(0, 0),
                    1,
                    SpriteEffects.None,
                    0f);
            }
            else
            {
                spriteBatch.Draw(
                    grayPixel,
                    new Rectangle(posX, posY, grayCorner.Width, grayCorner.Height),
                    new Rectangle(0, 0, 1, 1),
                    Color.White);
            }

            // Right upper corner
            if (roundedCorners[1] == true)
            {
                spriteBatch.Draw(
                    grayCorner,
                    new Vector2(posX + width - grayCorner.Width, posY),
                    null,
                    Color.White,
                    0,
                    new Vector2(0, 0),
                    1,
                    SpriteEffects.None,
                    0f);
            }
            else
            {
                spriteBatch.Draw(
                    grayPixel,
                    new Rectangle(posX + width - grayCorner.Width, posY, grayCorner.Width, grayCorner.Height),
                    new Rectangle(0, 0, 1, 1),
                    Color.White);
            }

            // Right lower corner
            if (roundedCorners[2] == true)
            {
                spriteBatch.Draw(
                    grayCorner,
                    new Vector2(posX + width, posY + height - grayCorner.Height),
                    null,
                    Color.White,
                    (float)Math.PI * 0.5f,
                    new Vector2(0, 0),
                    1,
                    SpriteEffects.None,
                    0f);
            }
            else
            {
                spriteBatch.Draw(
                    grayPixel,
                    new Rectangle(posX + width - grayCorner.Width, posY + height - grayCorner.Height, grayCorner.Width, grayCorner.Height),
                    new Rectangle(0, 0, 1, 1),
                    Color.White);
            }

            // Left lower corner
            if (roundedCorners[3] == true)
            {
                spriteBatch.Draw(
                    grayCorner,
                    new Vector2(posX + grayCorner.Width, posY + height),
                    null,
                    Color.White,
                    (float)Math.PI,
                    new Vector2(0, 0),
                    1,
                    SpriteEffects.None,
                    0f);
            }
            else
            {
                spriteBatch.Draw(
                    grayPixel,
                    new Rectangle(posX, posY + height - grayCorner.Height, grayCorner.Width, grayCorner.Height),
                    new Rectangle(0, 0, 1, 1),
                    Color.White);
            }

            // Top rect
            spriteBatch.Draw(
                    grayPixel,
                    new Rectangle(posX + grayCorner.Width, posY, width - grayCorner.Width * 2, grayCorner.Height),
                    new Rectangle(0, 0, 1, 1),
                    Color.White);

            // Right rect
            spriteBatch.Draw(
                    grayPixel,
                    new Rectangle(posX + width - grayCorner.Width, posY + grayCorner.Height, grayCorner.Width, height - grayCorner.Height * 2),
                    new Rectangle(0, 0, 1, 1),
                    Color.White);

            // Bottom rect
            spriteBatch.Draw(
                    grayPixel,
                    new Rectangle(posX + grayCorner.Width, posY + height - grayCorner.Height, width - grayCorner.Width * 2, grayCorner.Height),
                    new Rectangle(0, 0, 1, 1),
                    Color.White);

            // Left rect
            spriteBatch.Draw(
                    grayPixel,
                    new Rectangle(posX, posY + grayCorner.Height, grayCorner.Width, height - grayCorner.Height * 2),
                    new Rectangle(0, 0, 1, 1),
                    Color.White);

            spriteBatch.Draw(
                grayPixel,
                new Rectangle(posX + grayCorner.Width, posY + grayCorner.Height, width - grayCorner.Width * 2, height - grayCorner.Height * 2),
                new Rectangle(0, 0, 1, 1),
                Color.White);
        }
    }
}
