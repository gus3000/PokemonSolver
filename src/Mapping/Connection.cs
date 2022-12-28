using System;
using System.Collections.Generic;
using System.Linq;
using PokemonSolver.Algoritm;
using PokemonSolver.Memory;
using PokemonSolver.Memory.Local;

namespace PokemonSolver.Mapping
{
    public class Connection
    {
        public Direction Direction { get; }
        public int Offset { get; }
        public byte MapBank { get; }
        public byte MapIndex { get; }

        public Connection(IList<byte> bytes)
        {
            Direction = (Direction)Utils.GetIntegerFromByteArray(bytes, MapConnectionAddress.Direction, MapConnectionSize.Direction);
            Offset = BitConverter.ToInt32(bytes.ToArray(), MapConnectionAddress.Offset);
            // Utils.GetIntegerFromByteArray(bytes, MapConnectionAddress.Offset, MapConnectionSize.Offset);
            MapBank = (byte)Utils.GetIntegerFromByteArray(bytes, MapConnectionAddress.MapBank, MapConnectionSize.MapBank);
            MapIndex = (byte)Utils.GetIntegerFromByteArray(bytes, MapConnectionAddress.MapIndex, MapConnectionSize.MapIndex);
        }

        public override string ToString() => $"Connection({Direction},{Offset},{MapBank},{MapIndex})";
        public string Debug() => ToString();
    }
}