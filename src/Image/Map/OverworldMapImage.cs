using PokemonSolver.Mapping;

namespace PokemonSolver.Image.Map
{
    public class OverworldMapImage: ImageHandler
    {
        public OverworldMapImage(OverworldMap map)
        {
            DirectoryName += "/Maps";
            Filename = $"Overworld.png";
        }
    }
}