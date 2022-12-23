using System;
using System.Collections.Generic;
using PokemonSolver.Algoritm;
using PokemonSolver.Debug;
using PokemonSolver.Memory;
using Type = PokemonSolver.Memory.Type;

namespace PokemonSolver.MapData
{
    public class Tile : IShortStringable
    {
        
        public int X { get; }

        public int Y { get; }

        // public byte Attribute { get; }
        // public ushort TileNumber { get; }
        public byte MovementPermission { get; }
        public uint Block { get; }
        public bool Flooded { get; set; }

        public bool Walkable => MovementPermission % 4 != 1; //1, 5, D...   

        public Tile(IList<byte> bytes, int x, int y)
        {
            X = x;
            Y = y;
            // https://www.pokecommunity.com/showthread.php?t=103524 : <---- NOPE !
            // [Block-Nr-Teil-2 (1-Byte)][Pallet (1/2-byte)][Block-Nr-Teil-1 + Flip (1/2-byte)]
            // Attribute = (byte)(bytes[0] >> 2);
            // TileNumber = (ushort)(Utils.GetIntegerFromByteArray(bytes) & ((1 << 10) - 1));

            // byte blockNrTeil2 = bytes[0];
            // byte pallet = (byte)Utils.getBits(bytes, 8, 4);
            // byte blockNrTeil2PlusFlip = (byte)Utils.getBits(bytes, 12, 4);

            // Utils.Log($"  block data ({Utils.ToBinary(bytes)}): {Utils.ToBinary(blockNrTeil2,8)}, {Utils.ToBinary(pallet,4)}, {Utils.ToBinary(blockNrTeil2PlusFlip,4)}");

            MovementPermission = (byte)Utils.getBits(bytes, 8, 6);
            Block = Utils.GetIntegerFromByteArray(bytes, 0, 2) % (1 << 10);
            // Utils.Log($"({x},{y}) : {Block:X}:{MovementPermission:X}");
        }

        public override string ToString()
        {
            return $"Tile([{X},{Y}],0x{MovementPermission:x})";
        }


        public string ToShortString()
        {
            return $"({X},{Y},{MovementPermission:x})";
        }
    }
}