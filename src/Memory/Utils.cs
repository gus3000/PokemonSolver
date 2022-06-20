using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokemonSolver.Memory
{
    public abstract class Utils
    {
        public static readonly string[] CHARSET = new string[]
        {
            "&nbsp;",
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

        public static string GetStringFromByteArray(IEnumerable<byte> array)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in array)
            {
                if (b == 0 || b >= CHARSET.Length)
                    break;
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

        public static uint GetIntegerFromByteArray(IList<byte> array, int index, int size)
        {
            uint sum = 0;
            for (int i = 0; i < size; i++)
            {
                sum *= 256;
                sum += array[index + size - 1 - i];
            }

            // return array[index];
            return sum;
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

        public static string GetEncryptedDataOrder(byte personality)
        {
            return Order[personality % 24];
        }


    }
}