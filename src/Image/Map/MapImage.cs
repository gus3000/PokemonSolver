namespace PokemonSolver.Image.Map
{
    public abstract class MapImage : ImageHandler
    {

        public MapImage(Mapping.Map map)
        {
            DirectoryName += "/Maps";
            Filename = $"{map.Bank}-{map.MapIndex}-{map.Name}.png";
        }
    }
}