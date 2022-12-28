using PokemonSolver.Algoritm;

namespace PokemonSolver.Mapping
{
    public class Warp
    {
        public Position From { get; }
        public Direction Dir { get; }
        public Position To { get; }

        public Warp(Position from, Direction dir, Position to)
        {
            From = from;
            Dir = dir;
            To = to;
        }

        public override string ToString()
        {
            return $"Warp({From} -> {Dir} -> {To})";
        }
    }
}