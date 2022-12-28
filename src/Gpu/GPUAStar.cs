using PokemonSolver.Algoritm;

namespace PokemonSolver.Gpu
{
    public class GPUAStar
    {
        private AStar aStar;
        public GPUAStar()
        {
            aStar = new AStar();
        }

        public Node<Position>? Resolve(Position start, Position goal)
        {
            // var finalMap = aStar.MapConSearch(start, goal);
            return null; //TODO
        }
    }
}