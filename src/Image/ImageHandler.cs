using System.Drawing;

namespace PokemonSolver.Image
{
    public abstract class ImageHandler
    {
        public Bitmap Image { get; protected set; }

        protected void paintImageWithColorGrid(Color[][] colors, int cellSize)
        {
            Image = new Bitmap(colors.Length * cellSize, colors[0].Length * cellSize);
            var g = Graphics.FromImage(Image);
            
            var brush = new SolidBrush(Color.Pink);
            var pen = new Pen(Color.Black, .2f);
            for (int x = 0; x < colors.Length; x++)
            {
                for (int y = 0; y < colors[x].Length; y++)
                {
                    brush.Color = colors[x][y];
                    var rect = new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize);
                    g.FillRectangle(brush, rect);
                    g.DrawRectangle(pen,rect);
                }
            }
        }
    }
}