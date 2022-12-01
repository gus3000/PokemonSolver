using System;
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
            var connectionsOffset = rom.ReadU24(offset + MapAddress.Connections, MemoryDomain.ROM);
            if (connectionsOffset != 0)
            {
                // Utils.Log($"connections Offset for {Name} : {connectionsOffset:X}",true);
                var numberOfConnections = rom.ReadU32(connectionsOffset, MemoryDomain.ROM);
                Connections = new List<Connection>();
                Utils.Log($"Connections (0x{connectionsOffset:X} -> {numberOfConnections}):", true);
                var connectionDataOffset = rom.ReadU24(connectionsOffset + 4, MemoryDomain.ROM);
                for (var i = 0; i < numberOfConnections; i++)
                {
                    var conOffset = connectionDataOffset + i * MapConnectionSize.TotalSize;
                    Utils.Log($"Connection {i} <- {conOffset:x}");
                    Connections.Add(new Connection(rom.ReadByteRange(conOffset, MapConnectionSize.TotalSize)));
                    Utils.Log($"\t{Connections[Connections.Count - 1].Debug()}");
                }
            }
            else
            {
                Utils.Log($"No Connection");
            }
        }
    }
}