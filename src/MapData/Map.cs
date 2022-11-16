using System.Collections.Generic;
using BizHawk.Client.Common;
using PokemonSolver.Memory;
using PokemonSolver.Memory.Local;

namespace PokemonSolver.MapData
{
    public class Map
    {
        public PokemonSolver.MapData.MapData MapData { get; }

        // public EventData EventData { get; }
        // public List<MapScript> MapScripts { get; }
        public List<Connection> Connections { get; }
        public string Name { get; }


        public Map(IMemoryApi rom, long offset)
        {
            var mapDataOffset = rom.ReadU24(offset + MapAddress.MapData, MemoryDomain.ROM);
            Utils.Log($"  map data offset : 0x{mapDataOffset:X}", true);
            MapData = new PokemonSolver.MapData.MapData(rom, mapDataOffset);

            var labelIndex = rom.ReadByte(offset + Memory.Local.MapAddress.LabelIndex, MemoryDomain.ROM);
            Utils.Log($"  label index : 0x{labelIndex:X}", true);
            Name = Utils.GetLocationLabelHorriblyInefficiently(rom, labelIndex);
            Utils.Log($"  label : {Name}", true);
            // Utils.Log(" label : " + Utils.GetLocationLabelHorriblyInefficiently(rom,labelIndex));

            // read map header
            // -> map footer offset
            // -> event offset
            // -> script offset
            // -> connection offset
            Connections = new List<Connection>();
        }
    }
}