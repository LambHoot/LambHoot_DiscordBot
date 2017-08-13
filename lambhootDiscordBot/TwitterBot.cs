using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Tweetinvi;
using Tweetinvi.Models;

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
            Tweetinvi.Models.ITweet lastLoggedTweet = null;
            DateTime timeSinceLastTrain = DateTime.Now;
            while (true)
            {
                if (timeSinceLastTrain.AddMinutes(30) < DateTime.Now)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"[{ DateTime.Now}] -> Retraining at 30 minutes");
                    file.Close();
                    botPartialBiGram.retrain();
                    file = new System.IO.StreamWriter(logFilePath, true);
                    Console.WriteLine("Retraining complete");
                    Console.ResetColor();
                    timeSinceLastTrain = DateTime.Now;
                }

                try {
                    var tweets = Timeline.GetMentionsTimeline(1);
                    var timelineTweets = Timeline.GetHomeTimeline(1);

                    //log new timelines tweets
                    if (timelineTweets != null)
                    {
                        if (lastLoggedTweet == null)
                            lastLoggedTweet = timelineTweets.First();
                        else if ((lastLoggedTweet.Id != timelineTweets.First().Id) && !String.Equals(timelineTweets.First().CreatedBy.ScreenName, "AceNickelback"))
                        {
                            lastLoggedTweet = timelineTweets.First();
                            string loggedString = lastLoggedTweet.Text;
                            logMessage(loggedString);
                        }
                    }

                    //reply logic
                    if (lastTweet == null)
                    {
                        lastTweet = tweets.First();
                        Console.WriteLine($"[{DateTime.Now}] -> lastTweet captured.");
                    }
                    else if (tweets.First().Id != lastTweet.Id)
                    {
                        lastTweet = tweets.First();
                        new Thread(() =>
                        {
                            Thread.CurrentThread.IsBackground = true;
                            sendResponseTweet(lastTweet);
                        }).Start();
                        //sendResponseTweet(lastTweet);
                    }
                    else
                    {
                        //do nothing
                        //Console.WriteLine($"[{DateTime.Now}] -> No new mentions.");
                        new Thread(() =>
                        {
                            Thread.CurrentThread.IsBackground = true;
                            if (!sendRandomTweet())
                            {
                                double chance = randomDoubleRange(0, 100);
                                if (chance < 1)
                                {
                                    sendResponseTweet(lastTweet);
                                }
                            }
                        }).Start();
                    }

                   //Console.WriteLine($"[{DateTime.Now}] -> sleep for 20 seconds");
                    System.Threading.Thread.Sleep(20000);
                }
                catch (Exception e)
                {
                    //if there's an exception thrown, it's likely the Twitter api call limit being reached
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{DateTime.Now}] -> Exception thrown: " + e.Message);

                    Console.WriteLine("Retraining first though...");
                    file.Close();
                    botPartialBiGram.retrain();
                    file = new System.IO.StreamWriter(logFilePath, true);
                    Console.WriteLine("Retraining complete");

                    Console.WriteLine($"[{DateTime.Now}] -> Waiting 3 minutes.");
                    Console.ResetColor();
                    System.Threading.Thread.Sleep(3 * 60000);
                }
            }
        }

        private void sendResponseTweet(IMention tweetReplyTo)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"[{DateTime.Now}] -> Generating response to " + tweetReplyTo.CreatedBy.ScreenName + "...");
            Console.ResetColor();

            PartialBiGram useLanguageModel;
            double random = randomDoubleRange(0, 100);
            if(random < 20)
                useLanguageModel = shakespeareGram;//20%
            else if (random < 40)
                useLanguageModel = lambhootGram;//20%
            else
                useLanguageModel = botPartialBiGram;//60%

            string newNGramSentence = useLanguageModel.generateNewBiGramSentence();

            string textToPublish = string.Format("@{0} {1}", tweetReplyTo.CreatedBy.ScreenName, newNGramSentence);
            textToPublish = textToPublish.Substring(0, Math.Min(140, textToPublish.Length));
            Tweet.PublishTweetInReplyTo(textToPublish, tweetReplyTo.Id);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[{DateTime.Now}] -> NEW MENTION: " + textToPublish);
            Console.ResetColor();
        }

        private bool sendRandomTweet()
        {
            double chance = randomDoubleRange(0, 100);
            if (chance < 1)
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

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now}] -> NEW RANDOM TWEET: " + tweetString);
                Console.ResetColor();

                Tweet.PublishTweet(tweetString);
                return true;
            }
            return false;
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

        //logs a single message
        public void logMessage(string msgContent)
        {
            if (String.IsNullOrWhiteSpace(msgContent))
                return;
            file.WriteLine(msgContent);
            file.Flush();
            Console.WriteLine("LOGGED: " + msgContent);
        }

    }
}
