#include "C:\Users\gus3000\Git\BizhawkMods\PokemonSolver\src\Gpu\HLSL\map.h"

__kernel void test(__global int *input, __global int *output) {
  size_t i = get_global_id(0);

  output[i] = input[i] * 2;
}