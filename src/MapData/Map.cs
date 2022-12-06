using System;
using System.Collections.Generic;
using System.Text;
using BizHawk.Client.Common;
using PokemonSolver.Algoritm;
using PokemonSolver.Debug;
using PokemonSolver.Interaction;
using PokemonSolver.Memory;
using PokemonSolver.Memory.Local;

namespace PokemonSolver.MapData
{
    public class Map : IShortStringable
    {
        public PokemonSolver.MapData.MapData MapData { get; }

        // public EventData EventData { get; }
        // public List<MapScript> MapScripts { get; }
        // public List<Connection> Connections { get; }
        public List<Connection> Connections { get; }
        public string Name { get; }
        public int Bank { get; }
        public int MapIndex { get; }


        public Map(IMemoryApi rom, long offset, int bank, int mapIndex)
        {
            Bank = bank;
            MapIndex = mapIndex;
            
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
                Connections = new();
                Utils.Log($"Connections (0x{connectionsOffset:X} -> {numberOfConnections}):", true);
                var connectionDataOffset = rom.ReadU24(connectionsOffset + 4, MemoryDomain.ROM);
                for (var i = 0; i < numberOfConnections; i++)
                {
                    var conOffset = connectionDataOffset + i * MapConnectionSize.TotalSize;
                    var con = new Connection(rom.ReadByteRange(conOffset, MapConnectionSize.TotalSize));
                    Connections.Add(con);

                    Utils.Log($"\t{con.Debug()}");
                }
            }
            else
            {
                Utils.Log($"No Connection");
            }
        }

        public Map? GetNextMap(OverworldEngine overworldEngine, int x, int y)
        {
            if (x >= 0 && x < MapData.Width && y >= 0 && y < MapData.Height)
                return this;

            foreach (var con in Connections)
            {
                var map = overworldEngine.GetMap(con);
                int baseX, baseY;
                switch(con.Direction)
                {
                    case Direction.Down:
                        baseX = con.Offset;
                        baseY = MapData.Height;
                        break;
                    case Direction.Up:
                        baseX = con.Offset;
                        baseY = -map.MapData.Height;
                        break;
                    case Direction.Left:
                        baseX = -map.MapData.Width;
                        baseY = con.Offset;
                        break;
                    case Direction.Right:
                        baseX = MapData.Width;
                        baseY = con.Offset;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Direction {con.Direction} does not exist");
                }

                if (x >= baseX && x < baseX + map.MapData.Width && y >= baseY && y < baseY + map.MapData.Height)
                    return map;
            }

            return null;
        }

        public string Debug()
        {
            var conDebug = new StringBuilder();
            foreach (var con in Connections)
            {
                conDebug.Append($"{con.Debug()} ");
            }

            return $"Map(Name='{Name}', Connections={conDebug.ToString()})";
        }

        public string ToShortString()
        {
            return $"({Bank},{MapIndex}) {Name}";
        }
    }
}