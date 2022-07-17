using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BizHawk.Client.Common;
using PokemonSolver.Memory;

namespace PokemonSolver.Interaction
{
    public class Input
    {
        public enum Key
        {
            Up,
            Down,
            Left,
            Right,
            Start,
            Select,
            B,
            A,
            L,
            R
        }

        private Dictionary<string, bool> keys;

        public Input(Key[] pressedKeys)
        {
            keys = new Dictionary<string, bool>();
            if (pressedKeys.Length > 0)
                foreach (Key key in Enum.GetValues(typeof(Key)))
                {
                    keys[key.ToString()] = pressedKeys.Contains(key);
                }
        }

        public Input(IJoypadApi api)
        {
            keys = new Dictionary<string, bool>();
            foreach (var pressed in api.Get())
            {
                Key pressedKey;
                if (Enum.TryParse(pressed.Key, out pressedKey))
                {
                    keys[pressed.Key] = (bool)pressed.Value;
                }
                
            }
        }

        public void Set(Key key, bool value)
        {
            keys[key.ToString()] = value;
        }

        public void Register(IJoypadApi api)
        {
            // foreach (var key in Enum.GetNames(typeof(Key)))
            // {
            //     api.Set(key, keys[key]);
            // }
            // api.Set("Down", true);
            foreach (var pair in keys)
            {
                // Utils.Log($"{pair.Key} -> {pair.Value}");
                api.Set(pair.Key, pair.Value);
            }
            
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append(String.Join(",", keys.Where(pair => pair.Value).Select(pair => pair.Key)));
            // foreach (var pair in keys)
            // {
                // sb.Append($"{pair.Key}: {pair.Value}\n");
            // }
            
            sb.Append("]");
            return sb.ToString();
        }
    }
}