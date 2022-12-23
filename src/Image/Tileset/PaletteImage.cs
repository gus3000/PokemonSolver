using PokemonSolver.Image.Tileset;

namespace PokemonSolver.Image
{
    public class PaletteImage : ImageHandler
    {
        public PaletteImage(Palette palette)
        {
            DirectoryName += "/Palettes";
            Filename = $"{palette.Address:X6}.png";

            PaintWith(4, 4, 16, (x, y) => palette.GetColor(y * 4 + x));
        }
    }
}