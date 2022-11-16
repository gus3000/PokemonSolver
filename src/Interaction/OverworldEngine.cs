using System.Collections.Generic;
using System.Linq;
using BizHawk.Client.Common;
using BizHawk.Common;
using PokemonSolver.Algoritm;
using PokemonSolver.MapData;
using PokemonSolver.Memory;
using PokemonSolver.Memory.Global;
using PokemonSolver.Memory.Global.Ram;
using PokemonSolver.Memory.Global.Rom;
using PokemonSolver.Memory.Local;
using Address = PokemonSolver.Memory.Global.Rom.Address;

namespace PokemonSolver.Interaction
{
    public class OverworldEngine
    {
        private readonly IMemoryApi _memoryApi;

        public List<Map> Maps { get; }
        public List<List<Map>> Banks { get; }

        // public Map CurrentMap
        // {
        // get
        // {

        // }
        // }

        public OverworldEngine(IMemoryApi memoryMemoryApi)
        {
            _memoryApi = memoryMemoryApi;
            Maps = new List<Map>();
            Banks = new List<List<Map>>();
            // ComputeMaps();
        }

        //PETALBURG CITY -> CABFCEBBC6BCCFCCC100BDC3CED3
        //LITTLEROOT TOWN -> C6C3CECEC6BFCCC9C9CE00CEC9D1C8
        //TRAINER HILL -> CECCBBC3C8BFCC00C2C3C6C6
        public void ComputeMaps()
        {
            var oldDomain = _memoryApi.GetCurrentMemoryDomain();
            _memoryApi.UseMemoryDomain(MemoryDomain.ROM);

            Maps.Clear();
            Banks.Clear();

            var mapBankPointer = Address.EmeraldMapBankHeader;
            var banksAvailable = true;
            var totalCalculations = 0;
            while (banksAvailable)
            {
                var bank = new List<Map>();
                Utils.Log($"[0x{mapBankPointer:X}]");
                var mapBankOffset = _memoryApi.ReadU24(mapBankPointer, MemoryDomain.ROM);
                Utils.Log($" map bank start : 0x{mapBankOffset:X}");
                var mapHeaderPointer = mapBankOffset;
                var next = _memoryApi.ReadU24(mapBankPointer + 4, MemoryDomain.ROM);
                banksAvailable = (_memoryApi.ReadU8(mapBankPointer + 7, MemoryDomain.ROM) > 0);
                Utils.Log($" next : 0x{next:X}");

                if (!banksAvailable)
                    next = mapHeaderPointer +
                           4; // harcode 1 map for the last bank, no idea how to get the size otherwise

                while (mapHeaderPointer != next && totalCalculations++ < 1000)
                {
                    var mapHeaderOffset = _memoryApi.ReadU24(mapHeaderPointer, MemoryDomain.ROM);
                    Utils.Log($"  calculation {totalCalculations}");
                    Utils.Log($"  map address : 0x{mapHeaderPointer:X}");
                    Utils.Log($"  map header address : 0x{mapHeaderOffset:X}");
                    var map = new Map(_memoryApi, mapHeaderOffset);
                    Maps.Add(map);
                    bank.Add(map);
                    Utils.Log("________________________________");
                    mapHeaderPointer += 4;
                }

                Banks.Add(bank);
                mapBankPointer += 4;
                // break;
            }

            _memoryApi.UseMemoryDomain(oldDomain);
        }

        public Map getMap(int bank, int number)
        {
            Utils.Log($"Fetching map[{bank}][{number}]", true);
            return Banks[bank][number];
        }

        public Map? getCurrentMap()
        {
            if (Banks.Count == 0)
            {
                Utils.Log("Banks empty, no current map", true);
                return null;
            }

            var map = getMap(
                _memoryApi.ReadS8(Memory.Global.Ram.Address.EmeraldCurrentMapBank, MemoryDomain.CombinedWRAM),
                _memoryApi.ReadS8(Memory.Global.Ram.Address.EmeraldCurrentMapNumber, MemoryDomain.CombinedWRAM)
            );

            return map;
        }

        public Position getCurrentPosition()
        {
            var oldDomain = _memoryApi.GetCurrentMemoryDomain();
            _memoryApi.UseMemoryDomain(MemoryDomain.CombinedWRAM);
            var rawX = _memoryApi.ReadU16(Memory.Global.Ram.Character.Address.X);
            var rawY = _memoryApi.ReadU16(Memory.Global.Ram.Character.Address.Y);
            var rawDir = _memoryApi.ReadU16(Memory.Global.Ram.Character.Address.Direction);

            _memoryApi.UseMemoryDomain(oldDomain);

            var x = rawX - Border.Size;
            var y = rawY - Border.Size;
            Direction dir = (Direction)rawDir;
            return new Position(x, y, dir);
        }
    }
}