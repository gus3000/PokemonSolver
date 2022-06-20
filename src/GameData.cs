using System;
using System.Text.Json;
using BizHawk.Client.Common;
using PokemonSolver.Memory;
using PokemonSolver.Memory.Global;
using PokemonSolver.PokemonData;

namespace PokemonSolver
{
    public class GameData
    {
        public Pokemon[] Team { get; private set; }
        public Pokemon[]? OpponentTeam { get; private set; }
        public MoveData.Move[] Moves { get; private set; }
        public string debug { get; private set; }

        // private Pokemon[] boxes;
        public GameData(IMemoryApi memoryApi)
        {
            var oldDomain = memoryApi.GetCurrentMemoryDomain();
            Team = new Pokemon[6];
            for (int i = 0; i < 6; i++)
                Team[i] = new Pokemon(memoryApi.ReadByteRange(GlobalAddress.EmeraldUsParty + i * 100, 100));

            memoryApi.UseMemoryDomain(MemoryDomain.ROM);
            var move = new uint[9];
            for (var i = 0; i < 9; i++)
            {
                move[i] = memoryApi.ReadByte(RomAddress.EmeraldMoveData + i);
            }

            debug = String.Join("\n", move);
            
            memoryApi.UseMemoryDomain(oldDomain);
        }

        public string Serialize()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(this, options);
        }
    }
}