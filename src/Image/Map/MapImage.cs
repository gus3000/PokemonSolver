namespace PokemonSolver.Image.Map
{
    public abstract class MapImage : ImageHandler
    {

        public MapImage(MapData.Map map)
        {
            DirectoryName += "/Maps";
            Filename = $"{map.Bank}-{map.MapIndex}-{map.Name}.png";
        }
    }
}