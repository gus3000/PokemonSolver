using System.Collections.Generic;
using BizHawk.Client.Common;
using PokemonSolver.MapData;
using PokemonSolver.Memory.Global;
using PokemonSolver.Memory.Local;

namespace PokemonSolver.Memory
{
    public class Map
    {
        public MapData.MapData MapData { get; }
        // public EventData EventData { get; }
        // public List<MapScript> MapScripts { get; }
        public List<Connection> Connections { get; }
        public string Name { get; private set; }


        public Map(IMemoryApi rom, long offset)
        {
            var mapDataOffset = rom.ReadU24(offset + MapAddress.MapData);
            Utils.Log($" map data offset : 0x{mapDataOffset:X}");
            MapData = new MapData.MapData(rom,mapDataOffset);
            
            var labelIndex = rom.ReadByte(offset + Local.MapAddress.LabelIndex);
            Utils.Log($" label index : 0x{labelIndex:X}");
            Name = Utils.GetLocationLabelHorriblyInefficiently(rom, labelIndex);
            Utils.Log($" label : {Name}");
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