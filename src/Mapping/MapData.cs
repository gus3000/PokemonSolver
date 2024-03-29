﻿using System.Drawing;
using BizHawk.Client.Common;
using PokemonSolver.Image.Tileset;
using PokemonSolver.Memory;
using PokemonSolver.Memory.Local;

namespace PokemonSolver.Mapping
{
    public class MapData
    {
        public long Offset { get; }
        public ushort Width { get; }

        public ushort Height { get; }

        // public Border Border { get; }
        public Tileset GlobalTileset { get; }
        public Tileset LocalTileset { get; }
        // public long GlobalTileset { get; }
        // public long LocalTileset { get; }
        public Tile[] Tiles { get; }

        public Color[] CustomColors { get; }
        // public TileSet TileSet { get; }

        public MapData(IMemoryApi rom, long offset)
        {
            // Utils.Log($"0b{Convert.ToString(Utils.getBits(new byte[] {0b11110000,0b10101010}, 4,8), 2)}");
            // return;
            Offset = offset;
            Width = (ushort)Utils.GetIntegerFromByteArray(rom.ReadByteRange(offset + MapDataAddress.Width, MapDataSize.Width, MemoryDomain.ROM));
            Height = (ushort)Utils.GetIntegerFromByteArray(
                rom.ReadByteRange(offset + MapDataAddress.Height, MapDataSize.Height, MemoryDomain.ROM));

            Utils.Log($" Width : {Width}", true);
            Utils.Log($" Height : {Height}", true);

            var globalTilesetOffset = rom.ReadU24(offset + MapDataAddress.GlobalTileset, MemoryDomain.ROM);
            var localTilesetOffset = rom.ReadU24(offset + MapDataAddress.LocalTileset, MemoryDomain.ROM);
            GlobalTileset = Tileset.GetTileset(rom, globalTilesetOffset);
            LocalTileset = Tileset.GetTileset(rom, localTilesetOffset);
            // GlobalTileset = offset + MapDataAddress.GlobalTileset;
            // LocalTileset = offset + MapDataAddress.LocalTileset;

            var tileStructureOffset = rom.ReadU24(offset + MapDataAddress.TileStructure, MemoryDomain.ROM);
            Utils.Log($" Tile structure : 0x{tileStructureOffset:X}", true);

            var tilesData = rom.ReadByteRange(tileStructureOffset, (int)(Width * Height * MapDataSize.Tile), MemoryDomain.ROM);
            Tiles = new Tile[Width * Height];
            CustomColors = new Color[Width * Height];
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    int index = i * Height + j;
                    long tileOffset = tileStructureOffset + index * 2;
                    // Utils.Log($"  ({i},{j}) 0x{tileStructureOffset + index * 2:X}", true);
                    if (tileOffset > 0x9c02b0)
                    {
                        Utils.Log($"============WTF, index = {tileOffset}===================");
                    }

                    var tile = new Tile(rom.ReadByteRange(tileOffset, MapDataSize.Tile, MemoryDomain.ROM), i, j);
                    // Utils.Log($"  ({i},{j} : {index} -> 0x{tileOffset:X}) -> 0x{rom.ReadU16(tileOffset):x4} -> {tile}", true);
                    Tiles[index] = tile;
                    // Utils.Log(Tiles[index].ToString(), true);

                    CustomColors[index] = Color.Transparent;
                }
            }

            // Utils.Log($" width : {rom.ReadS32(offset)} (signed) OR {rom.ReadU32(offset)} (signed)");
        }

        public Tile GetTile(int x, int y)
        {
            return Tiles[y * Width + x];
        }
    }
}