using System.Collections.Generic;
using System.Drawing;
using System.Text;
using BizHawk.Client.Common;
using Microsoft.Extensions.Primitives;
using PokemonSolver.Memory;
using PokemonSolver.Memory.Local;

namespace PokemonSolver.Image.Tileset
{
    public class Palette
    {
        private static Dictionary<uint, Palette> palettes = new();

        public long Address { get; }
        private readonly Color[] _colors;

        public Palette(IList<byte> bytes, long address)
        {
            this.Address = address;
            _colors = new Color[bytes.Count / 2];
            for (var i = 0; i < bytes.Count / 2; i++)
            {
                var smolCol = (ushort)Utils.GetIntegerFromByteArray(bytes, 2 * i, 2);
                _colors[i] = ColorFrom6Bit(smolCol);
            }
        }

        private Color ColorFrom6Bit(ushort col)
        {
            var r = col % 32;
            var g = col % 1024 / 32;
            var b = col / 1024;
            return Color.FromArgb(r * 8, g * 8, b * 8);
        }

        public Color GetColor(int index)
        {
            return _colors[index];
        }

        public static Palette GetPalette(IMemoryApi rom, uint offset)
        {
            if (palettes.TryGetValue(offset, out var palette))
                return palette;

            palette = new Palette(rom.ReadByteRange(offset, PaletteSize.Total, MemoryDomain.ROM), offset);
            // Utils.Log($"Creating palette at 0x{offset:X} : {palette}");
            palettes[offset] = palette;
            return palette;
        }

        public static void DebugPalettes()
        {
            foreach (var pair in palettes)
            {
                var offset = pair.Key;
                var palette = pair.Value;
                Utils.Log($"[0x{offset:X}] {palette}", true);
            }
        }

        public static ICollection<Palette> GetPalettes()
        {
            return palettes.Values;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("Palette(");

            foreach (var color in _colors)
            {
                sb.Append(color);
                sb.Append(",");
            }

            sb.Append(")");
            return sb.ToString();
        }
    }
}