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
            Console.WriteLine("_LambHoot Discord Bot v1.4 Full nGram & James Bond_");

            Console.WriteLine("_Discord Bot_");
            MyBot lambhootBot = new MyBot();

            //Console.WriteLine("_Partial BiGram_");
            //PartialBiGram myPartialBiGram = new PartialBiGram();
            //Console.WriteLine("_vocab done_");

            //Word the = myPartialBiGram.vocabulary["the"];
            //Word letter = myPartialBiGram.vocabulary["letter"];
            //Word seven = myPartialBiGram.vocabulary["7/10."];
            //Word game = myPartialBiGram.vocabulary["game"];
            //Word same = myPartialBiGram.vocabulary["same"];

            //List<Word> sentence = new List<Word>();
            //sentence.Add(the);
            //var low = letter.probabilityGivenSentence(sentence);
            //var high = game.probabilityGivenSentence(sentence);
            //var high2 = same.probabilityGivenSentence(sentence);
            //var idk = the.probabilityGivenSentence(sentence);

            //sentence.Add(letter);
            //var low2 = seven.probabilityGivenSentence(sentence);

            //var a = letter.ProbabilityOfWordgivenB(the, 0);
            //var b = seven.ProbabilityOfWordgivenB(the, 1);
            //var c = game.ProbabilityOfWordgivenB(the, 0);
            //var d = same.ProbabilityOfWordgivenB(the, 0);
            //var e = the.ProbabilityOfWordgivenB(the, 0);

            //string s0 = myPartialBiGram.generateNewBiGramSentence("the");
            //Console.WriteLine("_____");
            //string s00 = myPartialBiGram.generateNewBiGramSentence("the");
            //Console.WriteLine("_____");
            //string s1 = myPartialBiGram.generateNewBiGramSentence("holy");
            //Console.WriteLine("_____");
            //string s2 = myPartialBiGram.generateNewBiGramSentence("astrology is");
            //Console.WriteLine("_____");
            //string s3 = myPartialBiGram.generateNewBiGramSentence("Denis is");
            //Console.WriteLine("_____");
            //string s4 = myPartialBiGram.generateNewBiGramSentence("applesauce");
            //Console.WriteLine("_____");
            //string s5 = myPartialBiGram.generateNewBiGramSentence();
            //Console.WriteLine("_____");
            //string s6 = myPartialBiGram.generateNewBiGramSentence();
            //Console.WriteLine("_____");
            //string s7 = myPartialBiGram.generateNewBiGramSentence();

            //var x = 0;

        }

    }
}
