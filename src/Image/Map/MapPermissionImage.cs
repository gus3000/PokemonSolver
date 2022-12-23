using PokemonSolver.Image.Colors;

namespace PokemonSolver.Image.Map
{
    public class MapPermissionImage : MapImage
    {
        public MapPermissionImage(MapData.Map map) : base(map)
        {
            DirectoryName += "/Permission";
            PaintWith(map.MapData.Width, map.MapData.Height, 16,
                (x, y) =>
                {
                    var tile = map.MapData.GetTile(x, y);
                    return MovementPermissionColors.getColorFromPermissionByte(tile.MovementPermission);
                },
                (x,y) => map.MapData.GetTile(x, y).MovementPermission.ToString("X")
            );
        }
    }
}