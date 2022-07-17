using System.Collections.Generic;
using BizHawk.Client.Common;
using PokemonSolver.Memory;
using PokemonSolver.Memory.Combat;

namespace PokemonSolver.Interaction
{
    //EWRAM :
    // selected move : 0x0244b0
    public class CombatEngine
    {
        private readonly IMemoryApi _memoryApi;

        public CombatEngine(IMemoryApi memoryMemoryApi)
        {
            _memoryApi = memoryMemoryApi;
        }

        public uint GetSelectedMove()
        {
            var oldMemoryDomain = _memoryApi.GetCurrentMemoryDomain();
            _memoryApi.UseMemoryDomain(MemoryDomain.EWRAM);

            var move = _memoryApi.ReadByte(UIAddress.SelectedMove);

            _memoryApi.UseMemoryDomain(oldMemoryDomain);

            return move;
        }

        public List<Input> SelectMove(uint move)
        {
            var actions = new List<Input>();
            var selectedMove = GetSelectedMove();

            if (selectedMove / 2 > move / 2)
                actions.Add(new Input(new[] { Input.Key.Up }));
            else if (selectedMove / 2 < move / 2)
                actions.Add(new Input(new[] { Input.Key.Down }));

            if (selectedMove % 2 > move % 2)
                actions.Add(new Input(new[] { Input.Key.Left }));
            else if (selectedMove % 2 < move % 2)
                actions.Add(new Input(new[] { Input.Key.Right }));


            return actions;
        }
    }
}