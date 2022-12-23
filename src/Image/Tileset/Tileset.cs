using System.Collections.Generic;
using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using PokemonSolver.Memory;
using PokemonSolver.Memory.Local;

namespace PokemonSolver.Image.Tileset
{
    public class Tileset
    {
        private static Dictionary<uint, Tileset> tilesets = new();
        public bool Compressed { get; }
        public bool Primary { get; }

        public uint TilesetImage { get; }

        public Palette ColorPalette { get; }
        // public uint ColorPalette { get; }
        public bool Blocks { get; }
        public bool AnimationRoutine { get; }

        public bool BehaviourBackground { get; }
        // tileset 0x083DF71C
        // at addr 33A704 you can see palette 0
        //palette : 16 colors
        //color : 2 bytes, 0 to 7FFF each value goes 0 to 31 with total = r + g*32 + b*1024

        public Tileset(IMemoryApi rom, long offset)
        {
            Compressed = rom.ReadByte(offset + TilesetHeaderAddress.Compressed, MemoryDomain.ROM) != 0;
            Primary = rom.ReadByte(offset + TilesetHeaderAddress.Primary, MemoryDomain.ROM) != 0;
            TilesetImage = rom.ReadU24(offset + TilesetHeaderAddress.TilesetImage, MemoryDomain.ROM);
            var paletteOffset = rom.ReadU24(offset + TilesetHeaderAddress.ColorPalettes, MemoryDomain.ROM);

            // ColorPalette = paletteOffset;
            ColorPalette = Palette.GetPalette(rom, paletteOffset);
        }

        public static void DebugTilesets()
        {
            foreach (var pair in tilesets)
            {
                Utils.Log($"[0x{pair.Key:X}] {pair.Value}");
            }
        }

        public static Tileset GetTileset(IMemoryApi rom, uint offset)
        {
            if (tilesets.TryGetValue(offset, out var tileset))
                return tileset;

            // tileset = new Tileset(rom.ReadByteRange(offset, TilesetHeaderSize.Total, MemoryDomain.ROM));
            tileset = new Tileset(rom, offset);
            tilesets[offset] = tileset;
            return tileset;
        }

        public override string ToString()
        {
            return $"Tileset(Compressed={Compressed}, Primary={Primary}, TilesetImage={TilesetImage:X8}, Colorpalettes={ColorPalette:X8})";
        }
    }
}