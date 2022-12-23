using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BizHawk.Client.Common;
using BizHawk.Common;
using Microsoft.Extensions.Primitives;
using PokemonSolver.Memory.Global;
using PokemonSolver.Memory.Global.Rom;

namespace PokemonSolver.Memory
{
    public abstract class Utils
    {
        public static readonly int MaxLineLength = 120;
        public static readonly string[] CHARSET = new string[]
        {
            " ",
            "À",
            "Á",
            "Â",
            "Ç",
            "È",
            "É",
            "Ê",
            "Ë",
            "Ì",
            "",
            "Î",
            "Ï",
            "Ò",
            "Ó",
            "Ô",
            "Œ",
            "Ù",
            "Ú",
            "Û",
            "Ñ",
            "ß",
            "à",
            "á",
            "",
            "ç",
            "è",
            "é",
            "ê",
            "ë",
            "ì",
            "",
            "î",
            "ï",
            "ò",
            "ó",
            "ô",
            "œ",
            "ù",
            "ú",
            "û",
            "ñ",
            "º",
            "ª",
            "ᵉʳ",
            "&amp;",
            "+",
            "",
            "",
            "",
            "",
            "",
            "",
            "=",
            ";",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "▯",
            "¿",
            "¡",
            "PK",
            "MN",
            "PO",
            "Ké",
            "",
            "",
            "",
            "Í",
            "%",
            "(",
            ")",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "â",
            "",
            "",
            "",
            "",
            "",
            "",
            "í",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "⬆",
            "⬇",
            "⬅",
            "➡",
            "*",
            "*",
            "*",
            "*",
            "*",
            "*",
            "*",
            "ᵉ",
            "&lt;",
            "&gt;",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "ʳᵉ",
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "!",
            "?",
            ".",
            "-",
            "・",
            "...",
            "",
            "",
            "‘",
            "’",
            "♂",
            "♀",
            "pokedollar",
            ",",
            "×",
            "/",
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z",
            "a",
            "b",
            "c",
            "d",
            "e",
            "f",
            "g",
            "h",
            "i",
            "j",
            "k",
            "l",
            "m",
            "n",
            "o",
            "p",
            "q",
            "r",
            "s",
            "t",
            "u",
            "v",
            "w",
            "x",
            "y",
            "z",
            "▶",
            ":",
            "Ä",
            "Ö",
            "Ü",
            "ä",
            "ö",
            "ü",
            "",
            "",
            "",
            "",
        };

        public static readonly string[] Order =
        {
            "GAEM",
            "GAME",
            "GEAM",
            "GEMA",
            "GMAE",
            "GMEA",
            "AGEM",
            "AGME",
            "AEGM",
            "AEMG",
            "AMGE",
            "AMEG",
            "EGAM",
            "EGMA",
            "EAGM",
            "EAMG",
            "EMGA",
            "EMAG",
            "MGAE",
            "MGEA",
            "MAGE",
            "MAEG",
            "MEGA",
            "MEAG",
        };

        public static byte[] getByteArrayFromString(string s)
        {
            // Log($"converting {s}");
            byte[] convertedPattern = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                convertedPattern[i] = (byte)Array.IndexOf(CHARSET, $"{s[i]}");
                // Log($"[{i}] {s[i]} -> {convertedPattern[i]}");
            }

            return convertedPattern;
        }

        public static string GetStringFromByteArray(IEnumerable<byte> array, bool fullLength = false)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in array)
            {
                if (b == 255)
                {
                    break;
                }
                else if (b > CHARSET.Length)
                {
                    if (!fullLength)
                    {
                        break;
                    }

                    Utils.Log($"unknown char {b}");
                    sb.Append('?');
                }
                else
                    sb.Append(CHARSET[b]);
            }

            return sb.ToString();
            // return array.Select(b => CHARSET[b]).Aggregate("", (acc,c) => acc + c);
        }

        public static string GetStringFromByteArray(IList<byte> array, int index, int size)
        {
            return GetStringFromByteArray(new ArraySegment<byte>(array.ToArray(), index, size));
        }

        public static uint GetIntegerFromByteArray(IList<byte> array, int index = 0, int size = -1,
            bool bigEndian = false)
        {
            if (size <= -1)
                size = array.Count;
            uint sum = 0;
            for (int i = 0; i < size; i++)
            {
                sum *= 256;
                sum += array[index + (bigEndian ? i : size - 1 - i)];
            }

            // return array[index];
            return sum;
        }

        public static long getBits(IList<byte> bytes, int index = 0, int size = -1)
        {
            if (size == -1)
                size = bytes.Count * 8;

            var bytesFullSize = bytes.Count * 8;
            long fullNumber = GetIntegerFromByteArray(bytes, 0, -1, true);
            // Utils.Log($"received :{toBinary(fullNumber, bytesFullSize)}, index {index}, size {size}");
            // Utils.Log($"truncated left :{toBinary(fullNumber & ((1 << (bytesFullSize - index)) - 1), bytesFullSize)}");
            // Utils.Log($"truncated right :{toBinary((fullNumber & ((1 << (bytesFullSize - index)) - 1)) >> (bytesFullSize - (index + size)), bytesFullSize)}");
            return (fullNumber & ((1 << (bytesFullSize - index)) - 1)) >> (bytesFullSize - (index + size));
        }

        public static IList<byte> GetByteArrayFromInteger(uint intToConvert, int size)
        {
            IList<byte> bytes = new List<byte>(size);
            for (int i = 0; i < size; i++)
            {
                bytes.Add((byte)(intToConvert % 256));
                intToConvert /= 256;
            }

            return bytes;
        }

        public static string GetLocationLabelHorriblyInefficiently(IMemoryApi api, uint index)
        {
            var pointer = Address.EmeraldLocationNamesStart;
            var currentIndex = 0;

            while (currentIndex < index)
            {
                if (api.ReadByte(pointer++, MemoryDomain.ROM) == 0xFF)
                    currentIndex++;
            }

            return GetStringFromByteArray(api.ReadByteRange(pointer, 50, MemoryDomain.ROM));
        }

        public static void ReadAllLabels(IMemoryApi api)
        {
            var pointer = Address.EmeraldLocationNamesStart;
            int debugMax = 0;
            bool end = false;
            while (!end)
            {
                var bytes = new List<byte>();

                uint b = api.ReadByte(pointer++, MemoryDomain.ROM);
                if (b == 0)
                {
                    end = true;
                    break;
                }

                for (; b != 0xff; pointer++)
                {
                    bytes.Add((byte)b);
                    b = api.ReadByte(pointer, MemoryDomain.ROM);
                    // Utils.Log($"b = {b:x}");

                    // if (debugMax++ > 100)
                    // return;
                }

                Utils.Log(Utils.GetStringFromByteArray(bytes));
            }
        }

        public static string GetEncryptedDataOrder(byte personality)
        {
            return Order[personality % 24];
        }

        public static string ToBinary(long n, int size = -1)
        {
            var bin = Convert.ToString(n, 2);
            if (size != -1 && size > bin.Length)
            {
                bin = bin.PadLeft(size, '0');
            }

            return $"0b{bin}";
        }

        public static string ToBinary(IList<byte> bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Count * 8 + 2);
            sb.Append("0b");
            foreach (var b in bytes)
            {
                sb.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }

            return sb.ToString();
        }

        public static void Log(string? msg, bool verbose = false)
        {
            if (msg == null)
                return;
            var sb = new StringBuilder();
            foreach (var c in msg)
            {
                sb.Append(c);
                if (sb.Length < MaxLineLength)
                    continue;
                BizHawk.Common.Log.Note("Debug" + (verbose ? "-verbose" : ""), sb.ToString());
                sb.Clear();
            }
            BizHawk.Common.Log.Note("Debug" + (verbose ? "-verbose" : ""), sb.ToString());
        }

        public static void Error(string? msg)
        {
            BizHawk.Common.Log.Error("Debug", msg ?? "null");
        }
    }
}