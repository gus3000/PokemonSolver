using System.Drawing;
using PokemonSolver.Image.Colors;
using PokemonSolver.Mapping;

namespace PokemonSolver.Image.Map
{
    public class OverworldMapPermissionImage : OverworldMapImage
    {
        public OverworldMapPermissionImage(OverworldMap map) : base(map)
        {
            DirectoryName += "/Permission";
            PaintWith(map.Width, map.Height, 16,
                (x, y) =>
                {
                    var tile = map.GetTile(x, y);
                    if (tile == null)
                        return Color.Black;
                    return MovementPermissionColors.getColorFromPermissionByte(tile.MovementPermission);
                },
                (x,y) => map.GetTile(x, y)?.MovementPermission.ToString("X") ?? ""
            );
        }

    }
}