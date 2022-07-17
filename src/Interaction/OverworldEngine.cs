using System.Collections.Generic;
using BizHawk.Client.Common;
using PokemonSolver.Memory;
using PokemonSolver.Memory.Global;

namespace PokemonSolver.Interaction
{
    public class OverworldEngine
    {
        private readonly IMemoryApi _memoryApi;
        
        public List<Map> Maps { get; }
        public OverworldEngine(IMemoryApi memoryMemoryApi)
        {
            _memoryApi = memoryMemoryApi;
            Maps = new List<Map>();
        }

        //PETALBURG CITY -> CABFCEBBC6BCCFCCC100BDC3CED3
        //LITTLEROOT TOWN -> C6C3CECEC6BFCCC9C9CE00CEC9D1C8
        //TRAINER HILL -> CECCBBC3C8BFCC00C2C3C6C6
        public void ComputeMaps()
        {
            var oldDomain = _memoryApi.GetCurrentMemoryDomain();
            _memoryApi.UseMemoryDomain(MemoryDomain.ROM);
            // Utils.ReadAllLabels(_memoryApi);
            
            // var location = "TRAINER HILL";
            // var bytes = Utils.getByteArrayFromString(location);
            // var m = "";
            // foreach (var b in bytes)
            // {
            //     m += $"{b:X} ";
            // }
            // Utils.Log($"{location} -> {m}");

            var pointer = RomAddress.EmeraldMapBankHeader;
            
            do
            {
                Utils.Log($"[0x{pointer:X}]");
                var mapBankOffset = _memoryApi.ReadU24(pointer);
                Utils.Log($" map bank : 0x{mapBankOffset:X}");
                var mapHeaderOffset = _memoryApi.ReadU24(mapBankOffset);
                Utils.Log($" map header : 0x{mapHeaderOffset:X}");
                Maps.Add(new Map(_memoryApi, mapHeaderOffset));
                pointer += 4;
                // break;
            } while (_memoryApi.ReadU8(pointer + 3) != 0);

            _memoryApi.UseMemoryDomain(oldDomain);
        }
    }
}