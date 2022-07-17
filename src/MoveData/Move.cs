using System;
using System.Collections;
using System.Collections.Generic;
using Type = PokemonSolver.Memory.Type;

namespace PokemonSolver.MoveData
{
    public class Move
    {
        public byte Effect { get; private set; }
        public byte BasePower { get; private set; }
        public Type Type { get; private set; }
        public byte Accuracy { get; private set; }
        public byte PP { get; private set; }
        public byte EffectAccuracy { get; private set; }
        public byte AffectsWhom { get; private set; }
        public byte Priority { get; private set; }
        public byte Flags { get; private set; }
        
        public Move(IList<byte> memory)
        {
            Effect = memory[0];
            BasePower = memory[1];
            Type = (Type)memory[2];
            Accuracy = memory[3];
            PP = memory[4];
            EffectAccuracy = memory[5];
            AffectsWhom = memory[6];
            Priority = memory[7];
            Flags = memory[8];
        }

        public override string ToString()
        {
            return $"Move[Effect:{Effect}, BasePower:{BasePower}, Type:{Type}, Accuracy:{Accuracy}, PP:{PP}, EffectAccuracy:{EffectAccuracy}, AffectsWhom:{AffectsWhom}, Priority:{Priority}, Flags:{Flags}]";
        }
    }
}