using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;

namespace lambhootDiscordBot
{
    class MyBot
    {
        public static ulong lhBotId = 303749228583321602;//userId of the bot
        private static ulong lambhootId = 234016471297032194;//my discord userId
        public static System.Random rng = new System.Random();
        public static bool logging = false;
        private System.IO.StreamWriter file;

        private DiscordClient discord;
        private string botToken;
        private string logFilePath;

        private PartialBiGram botPartialBiGram;


        public MyBot()
        {
            
            SetUp();
        }

        #region Discord Setup

        public void SetUp()
        {
            //get user inputs
            Console.WriteLine("Discord bot token: ");
            botToken = Console.ReadLine();
            Console.WriteLine("Log filepath: ");
            logFilePath = @""+Console.ReadLine();

            discord = new DiscordClient(new DiscordConfig
            {
                AutoReconnect = true,
                DiscordBranch = Branch.Stable,
                LargeThreshold = 250,
                LogLevel = LogLevel.Unnecessary,
                Token = botToken,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = false
            });
            Console.WriteLine("_Partial BiGraph_");
            botPartialBiGram = new PartialBiGram(logFilePath);//closes the file when done
            file = new System.IO.StreamWriter(logFilePath, true);

            Run().GetAwaiter().GetResult();
        }

        public async Task Run()
        {
            //logging stuff
            discord.DebugLogger.LogMessageReceived += (o, e) =>
            {
                Console.WriteLine($"[{e.TimeStamp}] [{e.Application}] [{e.Level}] {e.Message}");
            };

            discord.GuildAvailable += e =>
            {
                discord.DebugLogger.LogMessage(LogLevel.Info, "discord bot", $"Guild available: {e.Guild.Name}", DateTime.Now);
                return Task.Delay(0);
            };

            //new message event handling
            discord.MessageCreated += async e =>
            {
                await respondToLiveMessage(e);
            };

            await discord.Connect();
            await discord.UpdateStatus("preparing for the singularity");
            await Task.Delay(-1);
        }

        #endregion Discord Setup

        public async Task respondToLiveMessage(MessageCreateEventArgs msgEvent)
        {
            //ignore bots
            if (msgEvent.Message.Author.IsBot)
                return;

            //LOL responses
            if (msgEvent.Message.Content.ToLower().Contains("lol"))
            {
                if (msgEvent.Message.Author.ID == lambhootId)
                {
                    if (randomDoubleRange(0, 10) < 7)
                        await msgEvent.Message.Respond("lol");
                    else
                        await msgEvent.Message.Respond("LOL");
                }
                else
                {
                    if (randomDoubleRange(0, 10) < 4)
                        await msgEvent.Message.Respond("lol");
                }
            }

            //mentioned lambhoot
            DiscordUser possiblyLambHoot = messageContainsUser(msgEvent.Message, lambhootId);
            if (possiblyLambHoot != null)
            {
                //lambhoot was mentioned
                if (!possiblyLambHoot.Presence.Status.ToLower().Equals("online"))
                    await msgEvent.Message.Respond(msgEvent.Message.Author.Mention + " leave the poor man alone");
            }


            //AI SPEAKING
            DiscordUser possiblyBot = messageContainsUser(msgEvent.Message, lhBotId);
            if (possiblyBot != null && msgEvent.MentionedUsers.Count() == 1)//only bot was mentioned
            {
                string newBiGraphSentence = botPartialBiGram.generateNewSentence();
                await msgEvent.Message.Respond(newBiGraphSentence);
                return;//no logging of either of these messages
            }


            //LAMBHOOT SPECIFIC COMMANDS
            if (msgEvent.Message.Author.ID == lambhootId)
            {
                ////007 lol
                //if (msgEvent.Message.Content.ToLower().Contains("007")){
                //    DiscordRole jamesBondRoleName = msgEvent.Guild.Roles.Where(x => x.Name.Equals("JamesBond")).First();
                //    await msgEvent.Message.Respond("CALLING ALL " + jamesBondRoleName.Mention + "!");
                //}

                //logAllMessages
                if (msgEvent.Message.Content.ToLower().Equals("gg"))
                {
                    await msgEvent.Message.Respond("👌");
                }

                //logAllMessages
                if (msgEvent.Message.Content.ToLower().Equals("log"))
                {
                    await msgEvent.Message.Respond("Logging, hold up");
                    //await logAllMessages(msgEvent);
                    await msgEvent.Message.Respond("Logging done 👌");
                    await msgEvent.Message.Respond("(this doesn't actually work anymore aaaaaye lmao)");
                }

                //beginLog
                if (msgEvent.Message.Content.ToLower().Contains("begin log"))
                {
                    if (logging)
                        await msgEvent.Message.Respond("Already on it ya mango");
                    else
                    {
                        await msgEvent.Message.Respond("Hitting up that log, lemme know when to stop aight?");
                        file.Close();
                        file = new System.IO.StreamWriter(logFilePath, true);
                        logging = true;
                        Console.WriteLine("LOG STARTED!");
                    }
                    return;//so it doesn't log the log start
                }
                //endLog
                if (msgEvent.Message.Content.ToLower().Contains("end log"))
                {
                    if (!logging)
                        await msgEvent.Message.Respond("I'm not even doing it");
                    else
                    {
                        await msgEvent.Message.Respond("You got it 👌 check that log file!");
                        file.Close();
                        logging = false;
                        Console.WriteLine("LOG ENDED!");
                    }
                    return;//so it doesn't log the log end
                }

                //RETRAIN AI
                if (msgEvent.Message.Content.ToLower().Equals("retrain"))
                {
                    await msgEvent.Message.Respond("Retraining now ⏲️");
                    file.Close();
                    botPartialBiGram.retrain();
                    if(logging)
                        file = new System.IO.StreamWriter(logFilePath, true);
                    await msgEvent.Message.Respond("Trained and ready to roll 😎");
                    await msgEvent.Message.Respond("Test me!");
                    return;//do not log this
                }


            }

            //logging
            if (logging)
            {
                logMessage(msgEvent.Message.Content);
            }

        }

        //logs a single message
        public void logMessage(string msgContent)
        {
            if (String.IsNullOrWhiteSpace(msgContent))
                return;
            file.WriteLine(msgContent);
            file.Flush();
            Console.WriteLine("logged: " + msgContent);
        }

        //logs all messages (not working due to API issue I think)
        public static async Task logAllMessages(MessageCreateEventArgs msgEvent)
        {
            ulong currentBeforeId = 0;
            int limitRange = 25;
            List<DiscordMessage> messageList = new List<DiscordMessage>();
            //List<DiscordMessage> newMessages;
            currentBeforeId = msgEvent.Guild.Channels.First().LastMessageID;
            while (messageList.Count < 5000)
            {
                //FAILS TO WORK DUE TO PROBLEM WITH GetMessages()
                List<DiscordMessage> newMessages = await msgEvent.Guild.Channels.First().GetMessages(before: currentBeforeId);
                messageList.AddRange(newMessages);
                currentBeforeId = messageList.Last().ID;
            }
            var x = messageList;
        }



        //UTILS
        public static double randomDoubleRange(double min, double max)
        {
            return min + (rng.NextDouble() * (max - min));
        }

        //checks if a message contains a user and returns that user if it does
        private static DiscordUser messageContainsUser(DiscordMessage msg, ulong userId)
        {
            DiscordUser user = null;
            foreach (DiscordUser du in msg.Mentions)
            {
                if (du.ID == userId){
                    user = du;
                    return user;
                }
            }
            return user;
        }


    }
}
