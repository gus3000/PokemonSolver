#include "C:\Users\gus3000\Git\BizhawkMods\PokemonSolver\src\Gpu\HLSL\map.h"

__kernel void mapImage(__write_only image2d_t dstImg) {
  int2 coord = (int2)(get_global_id(0), get_global_id(1));

  uint4 col = (uint4)(255, 0, 255, 255);
  write_imageui(dstImg, coord, col);
}
