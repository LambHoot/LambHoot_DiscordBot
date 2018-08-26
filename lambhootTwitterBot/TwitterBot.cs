using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Tweetinvi;
using Tweetinvi.Models;

//build my own api for this eventually
//https://www.codeproject.com/Tips/1076400/Twitter-API-for-beginners

namespace lambhootDiscordBot
{
    class TwitterBot
    {

        private static string customer_key = "";
        private static string customer_key_secret = "";
        private static string access_token = "";
        private static string access_token_secret = "";

        private static bool shutted_up = false;

        private bool retraining = false;
        private int generatingSentences = 0;

        //@LambH00t
        private long admin_id = 1197092900;

        private string logFilePath, shakespeareFilePath, lambhootFilePath;
        private PartialBiGram botPartialBiGram, shakespeareGram, lambhootGram;
        private System.IO.StreamWriter file;
        public static System.Random rng = new System.Random();

        private List<string> safetyTweetQueue = new List<string>();
        private List<long> adminMessageIdsHandled = new List<long>();

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

                    if (generatingSentences > 0)
                    {
                        Console.WriteLine("Couldn't retrain, busy generating sentence");
                    }
                    else
                    {
                        retraining = true;
                        file.Close();
                        botPartialBiGram.retrain();
                        file = new System.IO.StreamWriter(logFilePath, true);
                        Console.WriteLine("Retraining complete");
                        timeSinceLastTrain = DateTime.Now;
                        retraining = false;
                    }
                    Console.ResetColor();
                }

                try
                {
                    var tweets = Timeline.GetMentionsTimeline(1);
                    var timelineTweets = Timeline.GetHomeTimeline(1);

                    var dms = Message.GetLatestMessages();
                    //List<IMessage> admin_dms = new List<IMessage>();
                    //1197092900
                    foreach (IMessage dm in dms)
                    {
                        //if it's an admin dm AND it hasn't already been handled
                        if(dm.SenderId == admin_id && !adminMessageIdsHandled.Contains(dm.Id))
                        {
                            //admin_dms.Add(dm);
                            adminMessageIdsHandled.Add(dm.Id);
                            //if first loop, bot just started, don't handle all admin commands
                            //otherwise I get spammed with dms every launch
                            if (lastLoggedTweet != null)
                                handleAdminCommand(dm.Text, true);
                        }
                    }

                    var x = "caca";

                    //log new timelines tweets
                    if (timelineTweets != null)
                    {
                        if (lastLoggedTweet == null)
                            lastLoggedTweet = timelineTweets.First();
                        else if ((lastLoggedTweet.Id != timelineTweets.First().Id) && !String.Equals(timelineTweets.First().CreatedBy.ScreenName, "AceNickelback"))
                        {
                            lastLoggedTweet = timelineTweets.First();
                            //if (lastLoggedTweet.Truncated == false)
                            //{
                            string loggedString = lastLoggedTweet.FullText;
                            logMessage(loggedString);
                            //}
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
                                    //sendResponseTweet(lastTweet);//
                                }
                            }
                        }).Start();
                    }

                    //Console.WriteLine($"[{DateTime.Now}] -> sleep for 1 MINUTES");
                    Console.WriteLine($"[{DateTime.Now}] -> --");
                    System.Threading.Thread.Sleep((int)(1 * 60000));
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

                    Console.WriteLine($"[{DateTime.Now}] -> Waiting 5 minutes.");
                    Console.ResetColor();
                    System.Threading.Thread.Sleep(5 * 60000);
                }
            }
        }

        private void sendResponseTweet(IMention tweetReplyTo)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"[{DateTime.Now}] -> Generating response to " + tweetReplyTo.CreatedBy.ScreenName + "...");
            if (retraining)
            {
                Console.WriteLine("Couldn't generate response, busy retraining");
                Console.ResetColor();
                return;
            }
            Console.ResetColor();

            generatingSentences++;

            if (tweetReplyTo.CreatedBy.ScreenName == "LambH00t")
            {
                this.handleAdminCommand(tweetReplyTo.FullText);
            }

            PartialBiGram useLanguageModel;
            bool safetyNeeded = false;
            double random = randomDoubleRange(0, 100);
            if (random < 10)
                useLanguageModel = shakespeareGram;//10%
            else if (random < 45)
                useLanguageModel = lambhootGram;//45%
            else
            {
                useLanguageModel = lambhootGram;// botPartialBiGram;//45%
                //safetyNeeded = true;
            }

            string newNGramSentence = useLanguageModel.generateNewBiGramSentence();

            string textToPublish = string.Format("@{0}", tweetReplyTo.CreatedBy.ScreenName);

            List<Tweetinvi.Models.Entities.IUserMentionEntity> userMentions = tweetReplyTo.UserMentions;
            List<string> mentionScreenames = new List<string>();
            foreach (Tweetinvi.Models.Entities.IUserMentionEntity mentionEntity in userMentions)
            {
                if (mentionEntity.ScreenName != "AceNickelback")
                {
                    mentionScreenames.Add(mentionEntity.ScreenName);
                    textToPublish += string.Format(" @{0}", mentionEntity.ScreenName);
                }
            }

            textToPublish += string.Format(" {0}", newNGramSentence);
            textToPublish = textToPublish.Substring(0, Math.Min(250, textToPublish.Length));

            if (!shutted_up)
            {
                if (!safetyNeeded)
                    Tweet.PublishTweetInReplyTo(textToPublish, tweetReplyTo.Id);
                //else nothing, TODO make queue handle saving tweetReplyTo.Id eventually
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[{DateTime.Now}] -> NEW MENTION: " + textToPublish);
            Console.ResetColor();
            generatingSentences--;
        }

        private void handleAdminCommand(string adminCommand, bool isDm = false)
        {
            bool command_handled = false;
            if (adminCommand.Contains("stop"))
            {
                command_handled = true;
                shutted_up = true;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now}] -> Shutted Up by Admin.");
                Console.ResetColor();

                var message = Message.PublishMessage("stopped", admin_id);
            }

            if (adminCommand.Contains("continue"))
            {
                command_handled = true;
                shutted_up = false;
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"[{DateTime.Now}] -> Given permission to speak by Admin.");
                Console.ResetColor();

                var message = Message.PublishMessage("continued", admin_id);
            }

            if(adminCommand.Contains("view list"))
            {
                command_handled = true;
                string safetyQueueMsg = "";

                foreach(string safetyTweetString in safetyTweetQueue)
                {
                    safetyQueueMsg += "--" + safetyTweetQueue.IndexOf(safetyTweetString) + "-- : " + safetyTweetString + " END -- ";
                }
                if (safetyQueueMsg.Length <= 0)
                    safetyQueueMsg = "no pending tweets in the queue";
                var message = Message.PublishMessage(safetyQueueMsg, admin_id);
            }

            if (adminCommand.Contains("approve"))
            {
                command_handled = true;
                //input assumed : approve 4
                string stringIndexApproved = adminCommand.Substring("approve ".Length);
                int indexApproved = 0;
                Int32.TryParse(stringIndexApproved, out indexApproved);
                if (safetyTweetQueue.ElementAtOrDefault(indexApproved) != null) {
                    string tweetStringApproved = safetyTweetQueue[indexApproved];
                    safetyTweetQueue.RemoveAt(indexApproved);
                    Tweet.PublishTweet(tweetStringApproved);
                    var message = Message.PublishMessage("tweet " + indexApproved + " is sent", admin_id);
                }
                else
                {
                    var message = Message.PublishMessage("tweet " + indexApproved + " doesn't exist", admin_id);
                }
            }

            if (adminCommand.Contains("remove"))
            {
                command_handled = true;
                //input assumed : remove 4
                string stringIndexToRemove = adminCommand.Substring("remove ".Length);
                int indexToRemove = 0;
                Int32.TryParse(stringIndexToRemove, out indexToRemove);
                if (safetyTweetQueue.ElementAtOrDefault(indexToRemove) != null)
                {
                    safetyTweetQueue.RemoveAt(indexToRemove);
                    var message = Message.PublishMessage("tweet " + indexToRemove + " is removed", admin_id);
                }
                else
                {
                    var message = Message.PublishMessage("tweet " + indexToRemove + " doesn't exist", admin_id);
                }
            }

            if (!command_handled && isDm)
            {
                var message = Message.PublishMessage("no command found", admin_id);
            }
        }

        private bool sendRandomTweet()
        {
            double chance = randomDoubleRange(0, 100);
            if (chance <= 1.5)
            {
                if (retraining)
                {
                    Console.WriteLine("Couldn't generate random tweet, busy retraining");
                    Console.ResetColor();
                    return true;
                }

                generatingSentences++;

                PartialBiGram useLanguageModel;
                bool safetyNeeded = false;
                double random = randomDoubleRange(0, 100);
                if (random < 10)
                    useLanguageModel = shakespeareGram;//10%
                else if (random < 55)
                    useLanguageModel = lambhootGram;//45%
                else
                {
                    useLanguageModel = botPartialBiGram;//45%
                    safetyNeeded = true;
                }

                string newNGramSentence = useLanguageModel.generateNewBiGramSentence();
                string tweetString = newNGramSentence;
                tweetString = tweetString.Substring(0, Math.Min(250, newNGramSentence.Length));

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now}] -> NEW RANDOM TWEET: " + tweetString);
                Console.ResetColor();

                if (!shutted_up)
                {
                    if (!safetyNeeded)
                    {
                        Tweet.PublishTweet(tweetString);
                    }
                    else
                    {
                        safetyTweetQueue.Add(tweetString);
                        int tweetIndex = safetyTweetQueue.IndexOf(tweetString);
                        var admin_notification = Message.PublishMessage("new tweet " + tweetIndex + ": " + tweetString, admin_id);
                    }
                }

                generatingSentences--;
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

            Console.Clear();

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

            //handle case tweet checked is a retweet (RT)
            if (msgContent.Contains("RT @"))
            {
                int semi_colon_index = msgContent.IndexOf(':');
                if (semi_colon_index > 0)
                {
                    Console.WriteLine("Retweet logging...");
                    //skips two indices for ": "
                    msgContent = msgContent.Substring(semi_colon_index + 2);
                }
            }

            file.WriteLine(msgContent);
            file.Flush();
            Console.WriteLine("LOGGED: " + msgContent);
        }

    }
}
