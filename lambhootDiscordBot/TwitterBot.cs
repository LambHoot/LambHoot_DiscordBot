using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tweetinvi;

namespace lambhootDiscordBot
{
    class TwitterBot
    {

        private static string customer_key = "";
        private static string customer_key_secret = "";
        private static string access_token = "";
        private static string access_token_secret = "";

        private string logFilePath, shakespeareFilePath, lambhootFilePath;
        private PartialBiGram botPartialBiGram, shakespeareGram, lambhootGram;
        private System.IO.StreamWriter file;
        public static System.Random rng = new System.Random();

        public TwitterBot()
        {
            Console.WriteLine("_TWITTER BOT_");
            setUp();
            TwitterLoop();
        }

        private void TwitterLoop()
        {
            Tweetinvi.Models.IMention lastTweet = null;
            while (true)
            {
                try {
                    var tweets = Timeline.GetMentionsTimeline(1);
                    if (lastTweet == null)
                    {
                        lastTweet = tweets.First();
                        Console.WriteLine($"[{DateTime.Now}] -> lastTweet captured.");
                    }
                    else if (tweets.First().Id != lastTweet.Id)
                    {
                        lastTweet = tweets.First();
                        sendResponseTweet(lastTweet.CreatedBy.ScreenName);
                    }
                    else
                    {
                        //do nothing
                        Console.WriteLine($"[{DateTime.Now}] -> No new mentions.");
                        sendRandomTweet();
                    }

                    Console.WriteLine($"[{DateTime.Now}] -> sleep for 20 seconds");
                    System.Threading.Thread.Sleep(20000);
                }
                catch(Exception e)
                {
                    //if there's an exception thrown, it's likely the Twitter api call limit being reached
                    Console.WriteLine($"[{DateTime.Now}] -> Exception thrown. Waiting 16 minutes.");
                    System.Threading.Thread.Sleep(16*60000);
                }
            }
        }

        private void sendResponseTweet(string screen_name)
        {
            string mentionName = ".@" + screen_name;
            Console.WriteLine($"[{DateTime.Now}] -> Generating response to " + mentionName + "...");

            PartialBiGram useLanguageModel;
            double random = randomDoubleRange(0, 100);
            if(random < 20)
                useLanguageModel = shakespeareGram;//20%
            else if (random < 40)
                useLanguageModel = botPartialBiGram;//20%
            else
                useLanguageModel = lambhootGram;//60%

            string newNGramSentence = useLanguageModel.generateNewBiGramSentence();
            string tweetString = mentionName + " " + newNGramSentence;
            tweetString = tweetString.Substring(0, Math.Min(140, newNGramSentence.Length));

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[{DateTime.Now}] -> NEW MENTION: " + tweetString);
            Console.ResetColor();

            Tweet.PublishTweet(tweetString);
        }

        private void sendRandomTweet()
        {
            double chance = randomDoubleRange(0, 100);
            if (chance < 2)
            {
                PartialBiGram useLanguageModel;
                double random = randomDoubleRange(0, 100);
                if (random < 20)
                    useLanguageModel = shakespeareGram;//20%
                else if (random < 50)
                    useLanguageModel = botPartialBiGram;//30%
                else
                    useLanguageModel = lambhootGram;//50%

                string newNGramSentence = useLanguageModel.generateNewBiGramSentence();
                string tweetString = newNGramSentence;
                tweetString = tweetString.Substring(0, Math.Min(140, newNGramSentence.Length));

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"[{DateTime.Now}] -> NEW RANDOM TWEET: " + tweetString);
                Console.ResetColor();

                Tweet.PublishTweet(tweetString);
            }
        }

        private void authenticate()
        {
            //get user inputs
            Console.WriteLine("customer_key: ");
            customer_key = Console.ReadLine();
            Console.WriteLine("customer_key_secret: ");
            customer_key_secret = Console.ReadLine();
            Console.WriteLine("access_token: ");
            access_token = Console.ReadLine();
            Console.WriteLine("access_token_secret: ");
            access_token_secret = Console.ReadLine();
            Auth.SetUserCredentials(customer_key, customer_key_secret, access_token, access_token_secret);
        }

        private void setUp()
        {
            authenticate();

            Console.WriteLine("Log filepath: ");
            logFilePath = @"" + Console.ReadLine();
            Console.WriteLine("Shakespear filepath: ");
            shakespeareFilePath = @"" + Console.ReadLine();
            Console.WriteLine("LambHoot filepath: ");
            lambhootFilePath = @"" + Console.ReadLine();

            Console.WriteLine("_Partial NGram_");
            botPartialBiGram = new PartialBiGram(logFilePath);//closes the file when done
            Console.WriteLine("_Shakespear NGram_");
            shakespeareGram = new PartialBiGram(shakespeareFilePath);//closes the file when done
            Console.WriteLine("_LambHoot NGram_");
            lambhootGram = new PartialBiGram(lambhootFilePath);//closes the file when done
            file = new System.IO.StreamWriter(logFilePath, true);
        }

        //UTILS
        public static double randomDoubleRange(double min, double max)
        {
            return min + (rng.NextDouble() * (max - min));
        }

    }
}
