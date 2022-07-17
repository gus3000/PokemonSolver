using BizHawk.Client.Common;
using PokemonSolver.Memory;

namespace PokemonSolver.Interaction
{
    public class Engine
    {
        private IJoypadApi JoypadApi;
        private IMemoryApi MemoryApi;
        
        private CombatEngine CombatEngine;
        public Engine(
            IMemoryApi memoryApi,
            IJoypadApi joypadApi
            // InputApi api
            // EmulationApi api
            // EmuClientApi api
        )
        {
            MemoryApi = memoryApi;
            JoypadApi = joypadApi;
            CombatEngine = new CombatEngine(memoryApi);

            // var inputs = joypadApi.Get();
            // foreach (var o in inputs)
            // {
            //     Utils.Log($"{o.Key} -> {o.Value}");
            // }
            // var input = new Input(joypadApi);

            // input.Register(joypadApi);
        }

        public void HandleNextInput()
        {
            const uint move = 2;
            var inputs = CombatEngine.SelectMove(move);
            if (inputs.Count == 0)
            {
                Utils.Log($"Already on move {move}");
                return;
            }
            var nextInput = inputs[0];
            Utils.Log($"activating input {nextInput}");
            nextInput.Register(JoypadApi);
        }
        
        
    }
}