using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using PokemonSolver.Memory;
using PokemonSolver.Memory.Global;
using PokemonSolver.Memory.Local;
using PokemonSolver.Memory.Number;

namespace PokemonSolver.PokemonData
{
    public class Pokemon
    {
        public string? Misc { get; private set; }
        public uint PersonalityValue { get; private set; }
        public uint OTId { get; private set; }
        public string OTIdBinary { get; private set; }

        public string Nickname { get; private set; }

        //TODO language
        public string? OTName { get; private set; }

        //TODO markings
        //TODO checksum
        public Growth? Growth { get; private set; }
        public Move[]? Moves { get; private set; }
        public EVsAndCondition? EVsAndCondition { get; private set; }

        public Miscellaneous? Miscellaneous { get; private set; }

        //TODO status
        public uint Level { get; private set; }

        //TODO pokerus
        public uint CurrentHp { get; private set; }
        public uint TotalHp { get; private set; }
        public uint Attack { get; private set; }
        public uint Defense { get; private set; }
        public uint Speed { get; private set; }
        public uint SpecialAttack { get; private set; }
        public uint SpecialDefense { get; private set; }

        public Pokemon(IList<byte> memory)
        {
            PersonalityValue = Utils.GetIntegerFromByteArray(memory, PokemonAddress.PersonalityValue,
                PokemonSize.PersonalityValue);
            OTId = Utils.GetIntegerFromByteArray(memory, PokemonAddress.OtId, PokemonSize.OtId);
            OTIdBinary =
                BitConverter.ToString(
                    new ArraySegment<byte>(memory.ToArray(), PokemonAddress.OtId, PokemonSize.OtId).ToArray());
            Nickname = Utils.GetStringFromByteArray(memory, PokemonAddress.Nickname,
                PokemonSize.Nickname);
            OTName = Utils.GetStringFromByteArray(memory, PokemonAddress.OtName, PokemonSize.OtName);
            Level = Utils.GetIntegerFromByteArray(memory, PokemonAddress.Level, PokemonSize.Level);
            CurrentHp = Utils.GetIntegerFromByteArray(memory, PokemonAddress.CurrentHp, PokemonSize.CurrentHp);
            TotalHp = Utils.GetIntegerFromByteArray(memory, PokemonAddress.TotalHp, PokemonSize.TotalHp);
            Attack = Utils.GetIntegerFromByteArray(memory, PokemonAddress.Attack, PokemonSize.Attack);
            Defense = Utils.GetIntegerFromByteArray(memory, PokemonAddress.Defense, PokemonSize.Defense);
            Speed = Utils.GetIntegerFromByteArray(memory, PokemonAddress.Speed, PokemonSize.Speed);
            SpecialAttack =
                Utils.GetIntegerFromByteArray(memory, PokemonAddress.SpecialAttack, PokemonSize.SpecialAttack);
            SpecialDefense =
                Utils.GetIntegerFromByteArray(memory, PokemonAddress.SpecialDefense, PokemonSize.SpecialDefense);
            InitDataSubstructures(memory);
        }

        private IEnumerable<byte> DecryptData(IList<byte> encryptedData)
        {
            uint key = OTId ^ PersonalityValue;
            IList<byte> decryptedData = new List<byte>(encryptedData.Count);
            for (int i = 0; i < encryptedData.Count; i += 4)
            {
                uint encryptedChunk = Utils.GetIntegerFromByteArray(encryptedData, i, 4);
                uint decryptedChunk = key ^ encryptedChunk;
                foreach (byte b in Utils.GetByteArrayFromInteger(decryptedChunk, 4))
                {
                    decryptedData.Add(b);
                }
            }

            return decryptedData;
        }

        private void InitDataSubstructures(IEnumerable<byte> memory)
        {
            IList<byte> encryptedData =
                new ArraySegment<byte>(memory.ToArray(), PokemonAddress.Data, PokemonSize.Data);
            IEnumerable<byte> decryptedData = DecryptData(encryptedData);
            IList<int> offsets = GetSubstructuresOffsets();
            Misc = Utils.Order[PersonalityValue % 24] + "[" + string.Join(",", offsets) + "]";
            Growth = new Growth(new ArraySegment<byte>(decryptedData.ToArray(), offsets[0], 12));

            Moves = new Move[4];
            var moveMemory = new ArraySegment<byte>(decryptedData.ToArray(), offsets[1], 12);
            for (int i = 0; i < 4; i++)
            {
                // uint id = Utils.GetIntegerFromByteArray(moveMemory, i * PokemonAttacksSize.Move1,
                    // PokemonAttacksSize.Move1);

                // moveDataMemory = new ArraySegment<byte>(memory.ToArray(),RomAddress.EmeraldMoveData, MoveSize.MoveDataSize);
                // MoveData.Move moveData = new MoveData.Move();
                
                Moves[i] = new Move(
                    Utils.GetIntegerFromByteArray(moveMemory, i * PokemonAttacksSize.Move1, PokemonAttacksSize.Move1),
                    Utils.GetIntegerFromByteArray(
                        moveMemory,
                        PokemonAttacksAddress.Pp1 + i * PokemonAttacksSize.Pp1,
                        PokemonAttacksSize.Pp1
                    )
                );
            }
        }

        private IList<int> GetSubstructuresOffsets()
        {
            string order = Utils.Order[PersonalityValue % 24];
            int[] offsets = new int[4];
            for (int i = 0; i < 4; i++)
            {
                switch (order[i])
                {
                    case 'G':
                        offsets[0] = i * 12;
                        break;
                    case 'A':
                        offsets[1] = i * 12;
                        break;
                    case 'E':
                        offsets[2] = i * 12;
                        break;
                    case 'M':
                        offsets[3] = i * 12;
                        break;
                    default:
                        throw new Exception("Something has gone terribly wrong");
                }
            }

            return offsets;
        }

        // private IList<byte> GetGrowth(IList<byte> memory)
        // {
        // string order = Utils.Order[PersonalityValue % 24];
        // int offset = order.IndexOf('G') * 12;
        // }

        public string Serialize()
        {
            var options = new JsonSerializerOptions();
            options.WriteIndented = true;
            return JsonSerializer.Serialize(this, options);
        }
    }
}