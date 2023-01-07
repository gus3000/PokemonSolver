__kernel void flood(__global int *input, __global int *output,
                    __global char *permissionBytes, __global char *continueFlag,
                    int width, int height, int xGoal, int yGoal) {
  size_t i = get_global_id(0);
  int y = i / width;
  int x = i % width;

  int out = input[i];
  if (out != -1)
    return;
  if (permissionBytes[i] == 1) {
    output[i] = -1;
    return;
  }

  for (int xx = x - 1; xx <= x + 1; xx++) {
    for (int yy = y - 1; yy <= y + 1; yy++) {
      if (xx == x && yy == y)
        continue;
      if (xx != x && yy != y)
        continue;
      if (!isInGrid(xx, yy, width, height))
        continue;
      int neighbourIndex = yy * width + xx;
      int neighbourValue = input[neighbourIndex];
      if (neighbourValue == -1)
        continue;
      if (out == -1)
        out = neighbourValue + 1;
      else
        out = min(out, neighbourValue + 1);
    }
  }
  if (x == xGoal && y == yGoal && out == -1)
    (*continueFlag) = 1;
  output[i] = out;
};

int isInGrid(int x, int y, int width, int height) {
  return x >= 0 && x < width && y >= 0 && y < height;
}
