using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokemonSolver.Interaction;
using PokemonSolver.Memory;

namespace PokemonSolver.Mapping
{
    public class OverworldMap
    {
        public int Width { get; private set;}
        public int Height { get; private set;}

        private Dictionary<Tuple<int, int>, Coordinates> _mapOffsets;
        public IEnumerable<Mapping.Map> Maps => from pouet in _mapOffsets select OverworldEngine.GetInstance().GetMap(pouet.Key.Item1, pouet.Key.Item2);

        public OverworldMap()
        {
            _mapOffsets = new();
            Queue<Mapping.Map> todoLater = new();
            var firstMap = OverworldEngine.GetInstance().GetMap(0, 9);
            todoLater.Enqueue(firstMap);
            // var firstMap = OverworldEngine.GetInstance().GetMap(0,0);
            // mapOffsets[new Tuple<int, int>(firstMap.Bank, firstMap.MapIndex)] = Coordinates.Zero;
            _mapOffsets[new Tuple<int, int>(firstMap.Bank, firstMap.MapIndex)] = Coordinates.Zero;
            // foreach (var map in maps)
            int maxIt = 1, it = 0;
            while (todoLater.Count > 0)
            {
                var map = todoLater.Dequeue();
                if (map.Connections.Count == 0)
                    continue;

                Coordinates offset;
                var key = new Tuple<int, int>(map.Bank, map.MapIndex);
                offset = _mapOffsets.ContainsKey(key) ? _mapOffsets[key] : Coordinates.Zero;

                foreach (var con in map.Connections)
                {
                    var conKey = new Tuple<int, int>(con.MapBank, con.MapIndex);
                    if (_mapOffsets.ContainsKey(conKey))
                    {
                        continue;
                    }

                    var conMap = OverworldEngine.GetInstance().GetMap(con.MapBank, con.MapIndex);
                    todoLater.Enqueue(conMap);
                    var conOffset = map.GetOffset(con);
                    Utils.Log($"offset from {map.Name} to {conMap.Name} : {conOffset}");

                    _mapOffsets[conKey] = conOffset + offset;
                }

                // if (it++ >= maxIt)
                    // break;
            }

            InitLayout();
        }

        public Coordinates GetOffset(Mapping.Map map)
        {
            return _mapOffsets[new Tuple<int, int>(map.Bank, map.MapIndex)];
        }

        private void InitLayout()
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue, maxXWidth = 0, maxYHeight = 0;
            // foreach (var offsets in _mapOffsets.Values)
            foreach (var kv in _mapOffsets)
            {
                var k = kv.Key;
                var offsets = kv.Value;
                if (offsets.X < minX)
                    minX = offsets.X;
                if (offsets.X > maxX)
                {
                    maxX = offsets.X;
                    maxXWidth = OverworldEngine.GetInstance().GetMap(k.Item1, k.Item2).MapData.Width;
                }
                
                if (offsets.Y < minY)
                    minY = offsets.Y;
                if (offsets.Y > maxY)
                {
                    maxY = offsets.Y;
                    maxYHeight = OverworldEngine.GetInstance().GetMap(k.Item1, k.Item2).MapData.Height;
                }
            }

            var baseCoord = new Coordinates(minX, minY);

            Width = maxX - minX + maxXWidth;
            Height = maxY - minY + maxYHeight;
            
            var keys = new List<Tuple<int, int>>(_mapOffsets.Keys);
            foreach (var k in keys)
            {
                _mapOffsets[k] -= baseCoord;
            }
        }

        public Tile? GetTile(int x, int y)
        {
            foreach (var kp in _mapOffsets)
            {
                var mapBank = kp.Key.Item1;
                var mapIndex = kp.Key.Item2;
                var coords = kp.Value;

                if (coords.X > x || coords.Y > y) continue;

                var m = OverworldEngine.GetInstance().GetMap(mapBank, mapIndex);
                var width = m.MapData.Width;
                var height = m.MapData.Height;
                if (coords.X + width <= x || coords.Y + height <= y)
                    continue;

                // if (coords.X <= x && coords.Y <= y && coords.X + width > x && coords.Y + height > y)
                // continue;
                // Utils.Log($"GetTile({x},{y}) => {mapBank}-{mapIndex} ({coords})");
                return m.MapData.GetTile(x - coords.X, y - coords.Y);
            }

            return null;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var pair in _mapOffsets)
            {
                sb.Append($"({pair.Key.Item1},{pair.Key.Item2}) -> ({pair.Value.X},{pair.Value.Y})\n");
            }

            return sb.ToString();
        }
    }
}