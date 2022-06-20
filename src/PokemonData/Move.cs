using System;

namespace PokemonSolver.PokemonData
{
    public class Move
    {
        //TODO use ModeData:Move
        public uint Id { get; }
        public uint Pp { get; }

        public Move(uint id, uint pp)
        {
            this.Id = id;
            this.Pp = pp;
        }

        public override string ToString()
        {
            return String.Format("Move({0},{1})", Id, Pp);
        }
    }
}