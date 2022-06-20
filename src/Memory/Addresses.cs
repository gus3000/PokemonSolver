// when in doubt, check https://github.com/shinyquagsire23/MEH/blob/master/MEH.ini
namespace PokemonSolver.Memory
{
    namespace Global
    {
        public abstract class GlobalAddress
        {
            //GLOBAL
            public const long EmeraldUsParty = 0x020244EC;
        }

        public abstract class RomAddress
        {
            // public const long EmeraldMoveData = 0x1FB12C;
            // public const long EmeraldMoveData = 0x207BC8;
            public const long EmeraldMoveData = 0x250C04;
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
    }
}