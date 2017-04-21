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
            Console.WriteLine("_LambHoot Discord Bot v1.1_");

            //Console.WriteLine("_Discord Bot_");
            //MyBot lambhootBot = new MyBot();

            Console.WriteLine("_Partial BiGram_");
            PartialBiGram myPartialBiGram = new PartialBiGram();

            string s1 = myPartialBiGram.generateNewSentence();
            Console.WriteLine("_____");
            string s2 = myPartialBiGram.generateNewSentence();
            Console.WriteLine("_____");
            string s3 = myPartialBiGram.generateNewSentence();
            Console.WriteLine("_____");
            string s4 = myPartialBiGram.generateNewSentence();
            Console.WriteLine("_____");
            string s5 = myPartialBiGram.generateNewSentence();
            Console.WriteLine("_____");
            string s6 = myPartialBiGram.generateNewSentence();
            Console.WriteLine("_____");
            string s7 = myPartialBiGram.generateNewSentence();

            var x = 0;

        }

    }
}
