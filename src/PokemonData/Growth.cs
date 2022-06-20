using System.Collections;
using System.Collections.Generic;
using PokemonSolver.Memory;
using PokemonSolver.Memory.Local;

namespace PokemonSolver.PokemonData
{
    public class Growth
    {
        public uint Species { get; private set; }
        public uint ItemHeld { get; private set; }
        public uint Experience { get; private set; }
        public uint PpBonuses { get; private set; }
        public uint Friendship { get; private set; }

        public Growth(IList<byte> memory)
        {
            Species = Utils.GetIntegerFromByteArray(memory, PokemonGrowthAddress.Species, PokemonGrowthSize.Species);
            ItemHeld = Utils.GetIntegerFromByteArray(memory, PokemonGrowthAddress.ItemHeld, PokemonGrowthSize.ItemHeld);
            Experience = Utils.GetIntegerFromByteArray(memory, PokemonGrowthAddress.Experience, PokemonGrowthSize.Experience);
            PpBonuses = Utils.GetIntegerFromByteArray(memory, PokemonGrowthAddress.PpBonuses, PokemonGrowthSize.PpBonuses);
            Friendship = Utils.GetIntegerFromByteArray(memory, PokemonGrowthAddress.Friendship, PokemonGrowthSize.Friendship);
        }
    }
}