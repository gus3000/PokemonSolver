using System;
using System.Collections.Generic;
using System.Text;
using BizHawk.Client.Common;
using PokemonSolver.Algoritm;
using PokemonSolver.Debug;
using PokemonSolver.Interaction;
using PokemonSolver.Memory;
using PokemonSolver.Memory.Local;

namespace PokemonSolver.Mapping
{
    public class Map : IShortStringable
    {
        public MapData MapData { get; }

        // public EventData EventData { get; }
        // public List<MapScript> MapScripts { get; }
        // public List<Connection> Connections { get; }
        public List<Connection> Connections { get; }
        public string Name { get; }
        public int Bank { get; }
        public int MapIndex { get; }

        public IList<Warp> Warps { get; protected set; }


        public Map(IMemoryApi rom, long offset, int bank, int mapIndex)
        {
            Bank = bank;
            MapIndex = mapIndex;

            var mapDataOffset = rom.ReadU24(offset + MapAddress.MapData, MemoryDomain.ROM);
            Utils.Log($"  map data offset : 0x{mapDataOffset:X}", true);
            MapData = new MapData(rom, mapDataOffset);

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
            Connections = new();
            var connectionsOffset = rom.ReadU24(offset + MapAddress.Connections, MemoryDomain.ROM);
            if (connectionsOffset != 0)
            {
                // Utils.Log($"connections Offset for {Name} : {connectionsOffset:X}",true);
                var numberOfConnections = rom.ReadU32(connectionsOffset, MemoryDomain.ROM);
                Utils.Log($"Connections (0x{connectionsOffset:X} -> {numberOfConnections}):", true);
                var connectionDataOffset = rom.ReadU24(connectionsOffset + 4, MemoryDomain.ROM);
                for (var i = 0; i < numberOfConnections; i++)
                {
                    var conOffset = connectionDataOffset + i * MapConnectionSize.TotalSize;
                    var con = new Connection(rom.ReadByteRange(conOffset, MapConnectionSize.TotalSize));
                    Connections.Add(con);

                    Utils.Log($"\t{con}");
                }
            }
            else
            {
                Utils.Log($"No Connection");
            }
        }

        public void InitWarps()
        {
            Warps = new List<Warp>();
            foreach (var connection in Connections)
            {
                foreach (var warp in GetWarps(connection))
                {
                    Warps.Add(warp);
                }
            }
        }

        private IList<Warp> GetWarps(Connection con)
        {
            Utils.Log($"Adding warps from connection {con}");
            var warps = new List<Warp>();
            if (con.Direction is Direction.Dive or Direction.Emerge)
                return warps;
            var conMap = OverworldEngine.GetInstance().GetMap(con.MapBank, con.MapIndex);
            int dx = 0, dy = 0;
            int destXDelta = 0, destYDelta = 0;
            int baseX, baseY;
            switch (con.Direction)
            {
                case Direction.Down or Direction.Up:
                    dx = 1;
                    baseX = 0;
                    //TODO add offset
                    if (con.Direction is Direction.Up)
                    {
                        baseY = 0;
                        destYDelta = conMap.MapData.Height - 1;
                    }
                    else
                    {
                        baseY = MapData.Height - 1;
                        destYDelta = -MapData.Height + 1;
                    }

                    break;
                case Direction.Left or Direction.Right:
                    dy = 1;
                    baseY = 0;
                    if (con.Direction is Direction.Left)
                    {
                        baseX = 0;
                        destXDelta = conMap.MapData.Width - 1;
                    }
                    else
                    {
                        baseX = MapData.Width - 1;
                        destXDelta = -MapData.Width + 1;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var delta = new Coordinates(dx, dy);
            var pos = new Position(Bank, MapIndex, baseX, baseY, Direction.Down, Altitude.Any);
            Utils.Log($"delta = {delta}, pos = {pos}");
            while (pos.X >= 0 && pos.X < MapData.Width && pos.Y >= 0 && pos.Y < MapData.Height)
            {
                if (MapData.GetTile(pos.X, pos.Y).Walkable)
                {
                    var newPos = new Position(
                        con.MapBank,
                        con.MapIndex,
                        pos.X + destXDelta,
                        pos.Y + destYDelta,
                        con.Direction,
                        pos.Altitude
                    );
                    //(pos + new Coordinates(destXDelta, destYDelta));

                    warps.Add(new Warp(pos, con.Direction, newPos));
                }

                pos += delta;
            }

            return warps;
        }

        public Map? GetNextMap(Coordinates coords)
        {
            return GetNextMap(coords.X, coords.Y);
        }

        public Map? GetNextMap(Position pos)
        {
            return GetNextMap(pos.X, pos.Y);
        }

        public Map? GetNextMap(int x, int y)
        {
            if (x >= 0 && x < MapData.Width && y >= 0 && y < MapData.Height)
                return this;

            foreach (var con in Connections)
            {
                if (con.Direction is Direction.Dive or Direction.Emerge)
                    continue;

                var map = OverworldEngine.GetInstance().GetMap(con);
                if (map == null)
                    return null;
                var coord = GetOffset(con);

                if (x >= coord.X && x < coord.X + map.MapData.Width && y >= coord.Y && y < coord.Y + map.MapData.Height)
                    return map;
            }

            return null;
        }

        public Coordinates GetOffset(Connection con)
        {
            var map = OverworldEngine.GetInstance().GetMap(con);
            int x = 0, y = 0;
            switch (con.Direction)
            {
                case Direction.Down:
                    x = con.Offset;
                    y = MapData.Height;
                    break;
                case Direction.Up:
                    x = con.Offset;
                    y = -map.MapData.Height;
                    break;
                case Direction.Left:
                    x = -map.MapData.Width;
                    y = con.Offset;
                    break;
                case Direction.Right:
                    x = MapData.Width;
                    y = con.Offset;
                    break;
                case Direction.Dive:
                    break;
                case Direction.Emerge:
                    break;
                default:
                    // Utils.Error($"Direction {con.Direction} does not exist on map {map}, connection {con}");
                    throw new ArgumentOutOfRangeException($"Direction {con.Direction} does not exist on map {map.ToShortString()}, connection {con}");
            }

            return new Coordinates(x, y);
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