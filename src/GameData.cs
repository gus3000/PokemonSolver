using System;
using System.Linq;
using System.Text.Json;
using BizHawk.Client.Common;
using PokemonSolver.Memory;
using PokemonSolver.Memory.Global;
using PokemonSolver.Memory.Local;
using PokemonSolver.Memory.Number;
using PokemonSolver.MoveData;
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
            Moves = new MoveData.Move[NumberOf.Moves];
            for (int i = 0; i < NumberOf.Moves; i++)
            {
                Moves[i] = new MoveData.Move(memoryApi.ReadByteRange(RomAddress.EmeraldMoveData + i * MoveSize.MoveDataSize, MoveSize.MoveDataSize));
            }
            
            // var move = new uint[9];
            // for (var i = 0; i < 9; i++)
            // {
                // move[i] = memoryApi.ReadByte(RomAddress.EmeraldMoveData + 12 + i);
            // }

            debug = String.Join("\n", Moves.Select(m => m.ToString()));

            // debug = Utils.GetStringFromByteArray(memoryApi.ReadByteRange(RomAddress.EmeraldMoveData, 4615), true);
            // debug = "debug est OK, c'est ton adresse qui est pourrie";

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