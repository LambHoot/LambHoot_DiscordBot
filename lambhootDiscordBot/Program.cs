using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;

// following https://github.com/NaamloosDT/DSharpPlus/wiki/Making-your-first-bot-in-C%23

namespace lambhootDiscordBot
{
    class Program
    {
        static void Main(string[] args)
        {
            //MyBot lambhootBot = new MyBot();
            PartialBiGraph myPartialBiGraph = new PartialBiGraph();

            string s1 = myPartialBiGraph.generateNewSentence();
            string s2 = myPartialBiGraph.generateNewSentence();
            string s3 = myPartialBiGraph.generateNewSentence();
            string s4 = myPartialBiGraph.generateNewSentence();
            string s5 = myPartialBiGraph.generateNewSentence();

            var x = 0;


        }

    }
}
