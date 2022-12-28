using System.Collections.Generic;
using System.Drawing;
using PokemonSolver.Algoritm;
using PokemonSolver.Image.Colors;

namespace PokemonSolver.Image.Map
{
    public class MapPreviewImage : ImageHandler
    {
        public MapPreviewImage(Mapping.Map map, List<KeyValuePair<Position, Color>> customColors)
        {
            Image = new Bitmap(map.MapData.Width, map.MapData.Height);
            // Utils.Log($"Loading map image with size ({Image.Width},{Image.Height})", true);

            Color[][] colors = new Color[Image.Width][];
            for (int x = 0; x < Image.Width; x++)
            {
                colors[x] = new Color[Image.Height];

                for (int y = 0; y < Image.Height; y++)
                {
                    var tile = map.MapData.GetTile(x, y);
                    colors[x][y] = MovementPermissionColors.getColorFromPermissionByte(tile.MovementPermission);
                    // Utils.Log($"({x},{y}) {tile.MovementPermission}", true);
                    // Image.SetPixel(x, y, color);
                }
            }

            foreach (var pair in customColors)
            {
                var pos = pair.Key;
                var col = pair.Value;
                if (map.Bank != pos.MapBank || map.MapIndex != pos.MapIndex)
                    continue;
                colors[pos.X][pos.Y] = col;
            }

            // for (int y = 0; y < Image.Height; y++)
            // for (int x = 0; x < Image.Width; x++)
            // Utils.Log($"({x},{y}) =>{colors[x][y]}", true);

            PaintImageWithColorGrid(colors, 10);
            // paintImageWithColorGrid(new []{new []{Color.Blue, Color.Red}, new []{Color.Green, Color.Brown}}, 50);

            Image.SetResolution(100, 100);
        }
    }
}