// when in doubt, check https://github.com/shinyquagsire23/MEH/blob/master/MEH.ini
// for map data : https://datacrystal.romhacking.net/wiki/Pok%C3%A9mon_3rd_Generation
//  ex : 24053E08B8615208CCC21D08EC6648086A0101000700020200000D0038
namespace PokemonSolver.Memory
{
    namespace Global
    {
        namespace Rom
        {
            public abstract class Address
            {
                // https://gamer2020.net/adding-more-attacks-to-fire-red-and-emerald/
                public const long EmeraldMoveNames = 0x31977C;
                public const long EmeraldMoveData = 0x31C898;
                public const long EmeraldMapBankHeader = 0x486578;
                public const long EmeraldLocationNamesStart = 0x5A0B10;
            }
        }

        namespace Ram
        {
            public abstract class Address
            {
                public const long EmeraldUsParty = 0x020244EC;
                public const long EmeraldCurrentMapBank = 0x0322E4;
                public const long EmeraldCurrentMapNumber = 0x0322E5;
            }

            namespace Character
            {
                public abstract class Address
                {
                    public const long X = 0x37360;
                    public const long Y = 0x37362;
                    public const long Direction = 0x037370;
                }

                public abstract class Size
                {
                    public const ushort X = 2;
                    public const ushort Y = 2;
                    public const ushort Direction = 2;
                }
                
            }
        }
    }
    
    namespace Number
    {
        public abstract class NumberOf
        {
            public const long Moves = 355;
        }
    }

    namespace Combat
    {
        public abstract class UIAddress
        {
            public const long SelectedMove = 0x0244B0;
        }

        public abstract class UISize
        {
            public const ushort SelectedMove = 1;
        }
    }


    namespace Local
    {
        public abstract class PokemonAddress
        {
            public const ushort PersonalityValue = 0;
            public const ushort OtId = 4;
            public const ushort Nickname = 8;
            public const ushort Language = 18;
            public const ushort OtName = 20;
            public const ushort Markings = 27;
            public const ushort Checksum = 28;
            public const ushort Data = 32;
            public const ushort Status = 80;
            public const ushort Level = 84;
            public const ushort Pokerus = 85;
            public const ushort CurrentHp = 86;
            public const ushort TotalHp = 88;
            public const ushort Attack = 90;
            public const ushort Defense = 92;
            public const ushort Speed = 94;
            public const ushort SpecialAttack = 96;
            public const ushort SpecialDefense = 98;
        }

        public abstract class PokemonSize
        {
            public const ushort PersonalityValue = 4;
            public const ushort OtId = 4;
            public const ushort Nickname = 10;
            public const ushort Language = 2;
            public const ushort OtName = 7;
            public const ushort Markings = 1;
            public const ushort Checksum = 2;
            public const ushort Data = 48;
            public const ushort Status = 4;
            public const ushort Level = 1;
            public const ushort Pokerus = 1;
            public const ushort CurrentHp = 2;
            public const ushort TotalHp = 2;
            public const ushort Attack = 2;
            public const ushort Defense = 2;
            public const ushort Speed = 2;
            public const ushort SpecialAttack = 2;
            public const ushort SpecialDefense = 2;
        }

        public abstract class PokemonGrowthAddress
        {
            public const ushort Species = 0;
            public const ushort ItemHeld = 2;
            public const ushort Experience = 4;
            public const ushort PpBonuses = 8;
            public const ushort Friendship = 9;
        }

        public abstract class PokemonGrowthSize
        {
            public const ushort Species = 2;
            public const ushort ItemHeld = 2;
            public const ushort Experience = 4;
            public const ushort PpBonuses = 1;
            public const ushort Friendship = 1;
        }

        public abstract class PokemonAttacksAddress
        {
            public const ushort Move1 = 0;
            public const ushort Move2 = 2;
            public const ushort Move3 = 4;
            public const ushort Move4 = 6;
            public const ushort Pp1 = 8;
            public const ushort Pp2 = 9;
            public const ushort Pp3 = 10;
            public const ushort Pp4 = 11;
        }

        public abstract class PokemonAttacksSize
        {
            public const ushort Move1 = 2;
            public const ushort Move2 = 2;
            public const ushort Move3 = 2;
            public const ushort Move4 = 2;
            public const ushort Pp1 = 1;
            public const ushort Pp2 = 1;
            public const ushort Pp3 = 1;
            public const ushort Pp4 = 1;
        }

        public abstract class PokemonEvsAndConditionAddress
        {
            public const ushort HpEv = 0;
            public const ushort AttackEv = 1;
            public const ushort DefenseEv = 2;
            public const ushort SpeedEv = 3;
            public const ushort SpecialAttackEv = 4;
            public const ushort SpecialDefenseEv = 5;
            public const ushort Coolness = 6;
            public const ushort Beauty = 7;
            public const ushort Cuteness = 8;
            public const ushort Smartness = 9;
            public const ushort Toughness = 10;
            public const ushort Feel = 11;
        }

        public abstract class PokemonEvsAndConditionSize
        {
            public const ushort HpEv = 1;
            public const ushort AttackEv = 1;
            public const ushort DefenseEv = 1;
            public const ushort SpeedEv = 1;
            public const ushort SpecialAttackEv = 1;
            public const ushort SpecialDefenseEv = 1;
            public const ushort Coolness = 1;
            public const ushort Beauty = 1;
            public const ushort Cuteness = 1;
            public const ushort Smartness = 1;
            public const ushort Toughness = 1;
            public const ushort Feel = 1;
        }

        public abstract class PokemonMiscellaneousAddress
        {
            public const ushort PokerusStatus = 0;
            public const ushort MetLocation = 1;
            public const ushort OriginsInfo = 2;
            public const ushort IvsEggAbility = 4;
            public const ushort RibbonsObedience = 8;
        }

        public abstract class PokemonMiscellaneousSize
        {
            public const ushort PokerusStatus = 1;
            public const ushort MetLocation = 1;
            public const ushort OriginsInfo = 2;
            public const ushort IvsEggAbility = 4;
            public const ushort RibbonsObedience = 4;
        }

        public abstract class MoveSize
        {
            public const ushort MoveDataSize = 12;
        }

        public abstract class MapAddress
        {
            public const ushort MapData = 0;
            public const ushort EventData = 4;
            public const ushort MapScripts = 8;
            public const ushort Connections = 12;
            public const ushort MusicIndex = 16;
            public const ushort MapPointerIndex = 18;
            public const ushort LabelIndex = 20;
            public const ushort Visibility = 21;
            public const ushort Wheather = 22;
            public const ushort MapType = 23;
            public const ushort Unknown = 24;
            public const ushort ShowLabelOnEntry = 26;
            public const ushort BattleFieldModelId = 27;
        }

        public abstract class MapSize
        {
            public const ushort MapData = 4;
            public const ushort EventData = 4;
            public const ushort MapScripts = 4;
            public const ushort Connections = 4;
            public const ushort MusicIndex = 2;
            public const ushort MapPointerIndex = 2;
            public const ushort LabelIndex = 1;
            public const ushort Visibility = 1;
            public const ushort Wheather = 1;
            public const ushort MapType = 1;
            public const ushort Unknown = 2;
            public const ushort ShowLabelOnEntry = 1;
            public const ushort BattleFieldModelId = 1;
        }

        public abstract class MapConnectionAddress
        {
            public const ushort Direction = 0;
            public const ushort Offset = 4;
            public const ushort MapBank = 8;
            public const ushort MapIndex = 9;
            public const ushort Filler = 10;
        }
        
        public abstract class MapConnectionSize
        {
            public const ushort Direction = 4;
            public const ushort Offset = 4;
            public const ushort MapBank = 1;
            public const ushort MapIndex = 1;
            public const ushort Filler = 2;
            public const ushort TotalSize = Direction + Offset + MapBank + MapIndex + Filler;
        }

        public abstract class MapDataAddress
        {
            public const ushort Width = 0;
            public const ushort Height = 4;
            public const ushort Border = 8;
            public const ushort TileStructure = 12;
            public const ushort GlobalTileset = 16;
            public const ushort LocalTileset = 20;
        }

        public abstract class MapDataSize
        {
            public const ushort Width = 4;
            public const ushort Height = 4;
            public const ushort Border = 4;
            public const ushort TileStructure = 4;
            public const ushort GlobalTileset = 4;
            public const ushort LocalTileset = 4;
            public const ushort Tile = 2;
        }
    }
}