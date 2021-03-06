using System;
using System.Collections.Generic;
using BizHawk.Client.Common;
using PokemonSolver.Memory;
using PokemonSolver.Memory.Local;

namespace PokemonSolver.MapData
{
    public class MapData
    {
        public ushort Width { get; }
        public ushort Height { get; }
        // public Border Border { get; }
        public Tile[] Tiles { get; }
        // public TileSet TileSet { get; }
        
        public MapData(IMemoryApi rom, long offset)
        {
            // Utils.Log($"0b{Convert.ToString(Utils.getBits(new byte[] {0b11110000,0b10101010}, 4,8), 2)}");
            // return;
            
            Width = (ushort)Utils.GetIntegerFromByteArray(rom.ReadByteRange(offset + MapDataAddress.Width, MapDataSize.Width));
            Height = (ushort)Utils.GetIntegerFromByteArray(
                rom.ReadByteRange(offset + MapDataAddress.Height, MapDataSize.Height));
            
            Utils.Log($" Width : {Width}");
            Utils.Log($" Height : {Height}");

            var tileStructureOffset = rom.ReadU24(offset + MapDataAddress.TileStructure);
            Utils.Log($" Tile structure : 0x{tileStructureOffset:X}");

            var tilesData = rom.ReadByteRange(tileStructureOffset, (int)(Width * Height * MapDataSize.Tile));
            Tiles = new Tile[Width * Height];
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    int index = i * Height + j;
                    long tileOffset = tileStructureOffset + index * 2;
                    // Utils.Log($"  ({i},{j}) 0x{tileStructureOffset + index * 2:X}");
                    if (tileOffset > 0x9c02b0)
                    {
                        Utils.Log($"============WTF, index = {tileOffset}===================");
                    }

                    var tile = new Tile(rom.ReadByteRange(tileOffset, 2));
                    Utils.Log($"  ({i},{j} : {index} -> 0x{tileOffset:X}) -> 0x{rom.ReadU16(tileOffset):x4} -> {tile}");
                    Tiles[index] = tile;
                    // Utils.Log(Tiles[index].ToString());
                }
            }
            // Utils.Log($" width : {rom.ReadS32(offset)} (signed) OR {rom.ReadU32(offset)} (signed)");
        }
    }
}