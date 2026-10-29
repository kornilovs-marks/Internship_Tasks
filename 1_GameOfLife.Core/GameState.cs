using System;
using System.Collections.Generic;
using System.Text;

namespace GameOfLife.Core
{
    public record GameState(int Width, int Height, long Iteration, bool[] Cells);
}
