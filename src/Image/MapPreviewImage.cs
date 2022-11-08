using System.Drawing;
using BizHawk.Common;
using PokemonSolver.Memory;

namespace PokemonSolver.Image
{
    public class MapPreviewImage : ImageHandler
    {
        public MapPreviewImage(Map map)
        {
            Image = new Bitmap(map.MapData.Width, map.MapData.Height);
            // Utils.Log($"Loading map image with size ({Image.Width},{Image.Height})", true);

            Color[][] colors = new Color[Image.Width][];
            for (int x = 0; x < Image.Width; x++)
            {
                colors[x] = new Color[Image.Height];

                for (int y = 0; y < Image.Height; y++)
                {
                    // colors[x][y] = Color.FromArgb(255, x * 10 % 256, y * 10 % 256);
                    var tile = map.MapData.GetTile(x, y);
                    colors[x][y] = MovementPermissionColors.getColorFromPermissionByte(tile.MovementPermission);
                    // Utils.Log($"({x},{y}) {tile.MovementPermission}", true);
                    // Image.SetPixel(x, y, color);
                }
            }

            // for (int y = 0; y < Image.Height; y++)
            // for (int x = 0; x < Image.Width; x++)
                // Utils.Log($"({x},{y}) =>{colors[x][y]}", true);

            paintImageWithColorGrid(colors, 10);
            // paintImageWithColorGrid(new []{new []{Color.Blue, Color.Red}, new []{Color.Green, Color.Brown}}, 50);

            Image.SetResolution(100, 100);
        }
    }
}