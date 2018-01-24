using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus;

namespace lambhootDiscordBot
{
    class MyBot
    {
        public static System.Random rng = new System.Random();
        public static double randomDoubleRange(double min, double max)
        {
            return min + (rng.NextDouble() * (max - min));
        }
    }
}
