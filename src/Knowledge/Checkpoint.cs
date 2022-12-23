using PokemonSolver.Algoritm;

namespace PokemonSolver.Knowledge
{
    public class Checkpoint: Position
    {
        public Checkpoint(int bank, int index, int x, int y, Direction direction, Altitude altitude) : base(bank, index, x, y, direction, altitude)
        {
            
        }
    }
}