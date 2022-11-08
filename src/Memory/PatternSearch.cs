using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BizHawk.Client.Common;
using BizHawk.Common;

namespace PokemonSolver.Memory
{
    public class PatternSearch
    {
        private readonly List<byte> rom;

        public const int LastRomAddress = 0xffffff;

        public PatternSearch(IMemoryApi memoryApi)
        {
            // var oldDomain = memoryApi.GetCurrentMemoryDomain();
            // memoryApi.UseMemoryDomain(MemoryDomain.ROM);
            rom = memoryApi.ReadByteRange(0, LastRomAddress, MemoryDomain.ROM);

            // memoryApi.UseMemoryDomain(oldDomain);
        }

        public IEnumerable<int> searchForByteArray(byte[] bytePattern, int start = 0, int end = LastRomAddress)
        {
            var occurrences = new List<int>();
            for (int i = start; i < end && i < rom.Count; i++)
            {
                // return;

                bool identical = true;
                for (int j = 0; j < bytePattern.Length; j++)
                {
                    if (rom[i + j] == bytePattern[j]) continue;
                    identical = false;
                    break;
                }

                if (identical)
                {
                    occurrences.Add(i);
                }
            }
            Utils.Log("Pattern Search OK");
            return occurrences;
        }

        public IEnumerable<int> searchForPokemonString(string pattern, int start = 0, int end = LastRomAddress)
        // public void searchForPokemonString(string pattern, int start = 0, int end = LastRomAddress)
        {
            byte[] bytePattern = Utils.getByteArrayFromString(pattern);

            // Utils.Log($"pattern size : {pattern.Length}");
            // Utils.Log($"bytePattern size : {bytePattern.Length}");

            return searchForByteArray(bytePattern);
            // var occurrences = new List<int>();
            // for (int i = start; i < end && i < rom.Count; i++)
            // {
            //     // return;
            //
            //     bool identical = true;
            //     for (int j = 0; j < pattern.Length; j++)
            //     {
            //         if (rom[i + j] == bytePattern[j]) continue;
            //         identical = false;
            //         break;
            //     }
            //
            //     if (identical)
            //     {
            //         occurrences.Add(i);
            //     }
            // }
            // Utils.Log("Pattern Search OK");
            // // return;
            // return occurrences;
        }
    }
}