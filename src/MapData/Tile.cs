using System.Collections.Generic;
using PokemonSolver.Memory;

namespace PokemonSolver.MapData
{
    public class Tile
    {
        // public byte Attribute { get; }
        // public ushort TileNumber { get; }
        public byte MovementPermission { get; }
        public Tile(List<byte> bytes)
        {
            // https://www.pokecommunity.com/showthread.php?t=103524 : <---- NOPE !
            // [Block-Nr-Teil-2 (1-Byte)][Pallet (1/2-byte)][Block-Nr-Teil-1 + Flip (1/2-byte)]
            // Attribute = (byte)(bytes[0] >> 2);
            // TileNumber = (ushort)(Utils.GetIntegerFromByteArray(bytes) & ((1 << 10) - 1));

            // byte blockNrTeil2 = bytes[0];
            // byte pallet = (byte)Utils.getBits(bytes, 8, 4);
            // byte blockNrTeil2PlusFlip = (byte)Utils.getBits(bytes, 12, 4);
            
            // Utils.Log($"  block data ({Utils.ToBinary(bytes)}): {Utils.ToBinary(blockNrTeil2,8)}, {Utils.ToBinary(pallet,4)}, {Utils.ToBinary(blockNrTeil2PlusFlip,4)}");

            MovementPermission = (byte) Utils.getBits(bytes,8,6);


        }

        public override string ToString()
        {
            return $"Tile(0x{MovementPermission:x})";
        }
    }
}