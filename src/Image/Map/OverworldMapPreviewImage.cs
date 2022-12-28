using System;
using System.Drawing;
using PokemonSolver.Mapping;

namespace PokemonSolver.Image.Map
{
    public class OverworldMapPreviewImage : OverworldMapImage
    {
        public OverworldMapPreviewImage(OverworldMap overworldMap) : base(overworldMap)
        {
            var cellSize = 16;
            Image = new Bitmap(overworldMap.Width * cellSize, overworldMap.Height * cellSize);

            var g = Graphics.FromImage(Image);
            var stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            var random = new Random(0);
            foreach (var map in overworldMap.Maps)
            {
                var color = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
                var pen = new Pen(color, 2f);
                var fontBrush = new SolidBrush(color);
                var offset = overworldMap.GetOffset(map);
                var rect = new Rectangle(offset.X * cellSize, offset.Y * cellSize, map.MapData.Width * cellSize, map.MapData.Height * cellSize);
                g.DrawRectangle(pen, rect);
                g.DrawString(
                    $"({map.Bank}-{map.MapIndex})-{map.Name}",
                    new Font("Liberation Mono", cellSize),
                    fontBrush,
                    rect.X + rect.Width / 2f,
                    rect.Y + rect.Height / 2f,
                    stringFormat
                );
            }
        }
    }
}