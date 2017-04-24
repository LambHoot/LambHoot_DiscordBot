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
            Console.WriteLine("_LambHoot Discord Bot v1.3 Full nGram_");

            //Console.WriteLine("_Discord Bot_");
            //MyBot lambhootBot = new MyBot();

            Console.WriteLine("_Partial BiGram_");
            PartialBiGram myPartialBiGram = new PartialBiGram();


            string s0 = myPartialBiGram.generateNewBiGramSentence("holy shit");
            Console.WriteLine("_____");
            string s1 = myPartialBiGram.generateNewBiGramSentence("holy fucking shit");
            Console.WriteLine("_____");
            string s2 = myPartialBiGram.generateNewBiGramSentence("astrology is");
            Console.WriteLine("_____");
            string s3 = myPartialBiGram.generateNewBiGramSentence("Denis is a fucking");
            Console.WriteLine("_____");
            string s4 = myPartialBiGram.generateNewBiGramSentence("applesauce");
            Console.WriteLine("_____");
            string s5 = myPartialBiGram.generateNewBiGramSentence();
            Console.WriteLine("_____");
            string s6 = myPartialBiGram.generateNewBiGramSentence();
            Console.WriteLine("_____");
            string s7 = myPartialBiGram.generateNewBiGramSentence();

            var x = 0;

        }

    }
}
