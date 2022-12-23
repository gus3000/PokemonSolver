using System;
using System.Drawing;
using System.IO;

namespace PokemonSolver.Image
{
    public abstract class ImageHandler
    {
        public string DirectoryName { get; protected set; }
        public string Filename { get; protected set; }
        public Bitmap Image { get; protected set; }

        public ImageHandler()
        {
            DirectoryName = "../Images";
        }
        
        protected void PaintImageWithColorGrid(Color[][] colors, int cellSize)
        {
            PaintWith(colors.Length, colors[0].Length, cellSize, (x, y) => colors[x][y]);
        }

        protected void PaintWith(int width, int height, int cellSize, Func<int, int, Color> colorCallback, Func<int, int, string> stringCallback = null)
        {
            Image = new Bitmap(width * cellSize, height * cellSize);
            var g = Graphics.FromImage(Image);
            var font = new Font("Liberation Mono", cellSize / 2f);
            var stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            var brush = new SolidBrush(Color.Pink);
            var pen = new Pen(Color.Black, .2f);
            var fontBrush = new SolidBrush(Color.Black);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    brush.Color = colorCallback(x, y);
                    var rect = new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize);
                    g.FillRectangle(brush, rect);
                    g.DrawRectangle(pen, rect);

                    if (stringCallback == null)
                        continue;
                    g.DrawString(stringCallback(x, y), font, fontBrush, (x + .5f) * cellSize, (y + .5f) * cellSize, stringFormat);
                }
            }
        }
        private void FixNames()
        {
            DirectoryName = String.Join("_", DirectoryName.Split(Path.GetInvalidPathChars(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            Filename = String.Join("_", Filename.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }
        public void Save()
        {
            FixNames();
            Directory.CreateDirectory(DirectoryName);
            Image.Save($"{DirectoryName}/{Filename}");
        }
    }
}