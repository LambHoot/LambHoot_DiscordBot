using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//using DSharpPlus;

namespace lambhootDiscordBot
{
    class MyBot
    {
        public static ulong lhBotId = 303749228583321602;//userId of the bot
        private static ulong lambhootId = 234016471297032194;//my discord userId
        public static System.Random rng = new System.Random();
        public static bool logging = false;
        private System.IO.StreamWriter file;

        //private DiscordClient discord;
        private string botToken;
        private string logFilePath, shakespeareFilePath, lambhootFilePath;

        private PartialBiGram botPartialBiGram, shakespeareGram, lambhootGram;
        /*
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
            logFilePath = @"" + Console.ReadLine();
            Console.WriteLine("Shakespear filepath: ");
            shakespeareFilePath = @"" + Console.ReadLine();
            Console.WriteLine("LambHoot filepath: ");
            lambhootFilePath = @"" + Console.ReadLine();

            discord = new DiscordClient(new DiscordConfig
            {
                AutoReconnect = true,
                DiscordBranch = Branch.Stable,
                LargeThreshold = 250,
                LogLevel = LogLevel.Debug,
                Token = botToken,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = false
            });
            Console.WriteLine("_Partial NGram_");
            botPartialBiGram = new PartialBiGram(logFilePath);//closes the file when done
            Console.WriteLine("_Shakespear NGram_");
            shakespeareGram = new PartialBiGram(shakespeareFilePath);//closes the file when done
            Console.WriteLine("_LambHoot NGram_");
            lambhootGram = new PartialBiGram(lambhootFilePath);//closes the file when done
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
            //from: https://stackoverflow.com/questions/363377/how-do-i-run-a-simple-bit-of-code-in-a-new-thread
            discord.MessageCreated += e =>
            {
                //await respondToLiveMessage(e);
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    //Console.WriteLine("--- new message handling thread created ---");
                    respondToLiveMessage(e);//doesn't need to be awaited. Only async for I/O awaiting within itself.
                    //Console.WriteLine("--- new message handling thread ended --- ?");
                }).Start();
                //Console.WriteLine("--- new message handling task done ---");
                return Task.Delay(0);
            };

            await discord.Connect();
            await discord.UpdateStatus("preparing for the singularity");
            await Task.Delay(-1);
        }

        #endregion Discord Setup

        public async Task respondToLiveMessage(MessageCreateEventArgs msgEvent)
        {
            try
            {
                //ignore bots
                if ((msgEvent.Message.Author.IsBot || msgEvent.Guild.Name == null) && msgEvent.Message.Author.ID != lambhootId)
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
                        await msgEvent.Message.Respond(msgEvent.Message.Author.Mention + " leave the poor man alone 🙃");
                }


                //AI SPEAKING
                DiscordUser possiblyBot = messageContainsUser(msgEvent.Message, lhBotId);
                if (possiblyBot != null && msgEvent.MentionedUsers.Count() == 1)//only bot was mentioned
                {
                    string newNGramSentence;

                    //first, select the NGram to use
                    PartialBiGram usedBigram;
                    if (msgEvent.Message.Content.ToLower().Contains("shakespeare"))
                    {
                        usedBigram = shakespeareGram;
                    }
                    else if (msgEvent.Message.Content.ToLower().Contains("lambhootngram"))
                    {
                        usedBigram = lambhootGram;
                    }
                    else
                    {
                        usedBigram = botPartialBiGram;
                    }

                    if (msgEvent.Message.Content.Contains("from:"))//user provided input
                    {
                        string[] msgSplit = msgEvent.Message.Content.Split(new string[] { "from:" }, StringSplitOptions.None);

                        string input = null;
                        if (msgSplit.Count() > 0)
                        {
                            input = msgSplit.Last();
                            newNGramSentence = usedBigram.generateNewBiGramSentence(input);
                            await msgEvent.Message.Respond(newNGramSentence);
                            return;//no logging
                        }//else, continue as normal
                    }

                    newNGramSentence = usedBigram.generateNewBiGramSentence();
                    await msgEvent.Message.Respond(newNGramSentence);
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
                        Console.WriteLine("! RETRAIN REQUESTED !");
                        await msgEvent.Message.Respond("Retraining now ⏲️");
                        file.Close();
                        botPartialBiGram.retrain();
                        if (logging)
                            file = new System.IO.StreamWriter(logFilePath, true);
                        await msgEvent.Message.Respond("Trained and ready to roll 😎");
                        await msgEvent.Message.Respond("Test me!");
                        return;//do not log this
                    }

                    if (msgEvent.Message.Content.ToLower().Equals("update jamesbonds"))
                    {
                        await updateJamesBonds(msgEvent);
                    }

                    if (msgEvent.Message.Content.ToLower().Equals("change everyone's name to james bond"))
                    {
                        await updateJamesBondNames(msgEvent);
                    }
                }

                await RespondRandomly(msgEvent);

                //logging
                if (logging)
                {
                    logMessage(msgEvent.Message.Content);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("!!! FAILED RESPONDING TO MESSAGE !!!");
                await msgEvent.Message.Respond(msgEvent.Message.Author.Mention + " leave me alone 🔫😠");
            }

        }

        public async Task updateJamesBonds(MessageCreateEventArgs msgEvent)
        {
            //await msgEvent.Message.Respond("Updating those 007s");
            DiscordGuild lhServer = msgEvent.Guild;
            DiscordRole jamesBondRole = msgEvent.Guild.Roles.Where(x => x.Name.Equals("JamesBond")).First();

            List<DiscordMember> membersList = lhServer.Members.Where(x => !x.Roles.Contains(jamesBondRole.ID)).ToList<DiscordMember>();
            if (membersList.Count <= 0)
            {
                await msgEvent.Message.Respond("Everyone is already James Bond 🔫😎");
            }
            else
            {
                string andStr = "";
                foreach (DiscordMember member in membersList)
                {
                    if (member.User.ID != lhBotId)
                    {
                        try
                        {
                            List<ulong> newRoles = member.Roles;
                            newRoles.Add(jamesBondRole.ID);
                            await lhServer.ModifyMember(member.User.ID, "James Bond", newRoles, member.IsMuted, member.IsDeafened, 0);
                            await msgEvent.Message.Respond(member.User.Mention + andStr + " you're a James Bond");
                            andStr = " and";
                        }
                        catch (Exception e)//I'm not sure what the issue is, but it fails on some users. Based on bad documentation, I think users need to be online or else it fails
                        {
                            await msgEvent.Message.Respond(member.User.Mention + " I couldn't make you a James Bond, sorry!");
                        }
                    }
                }
                await msgEvent.Message.Respond("Everyone is a James Bond 😎👍");
            }
            return;
        }

        public async Task updateJamesBondNames(MessageCreateEventArgs msgEvent)
        {
            DiscordGuild lhServer = msgEvent.Guild;
            await msgEvent.Message.Respond("I'm trying father");

            Task<List<DiscordMember>> taskMembersList = lhServer.GetAllMembers();
            List<DiscordMember> membersList = taskMembersList.Result.Where(x => x.Nickname != "James Bond").ToList<DiscordMember>();
            if (membersList.Count <= 0)
            {
                await msgEvent.Message.Respond("Everyone is already James Bond 🔫😎");
            }
            else
            {
                await msgEvent.Message.Respond("Updating those 007s");
                string andStr = "";
                foreach (DiscordMember member in membersList)
                {
                    if (member.User.ID != lhBotId)
                    {
                        try
                        {
                            await lhServer.ModifyMember(member.User.ID, "James Bond", member.Roles, member.IsMuted, member.IsDeafened, 0);
                            await msgEvent.Message.Respond(member.User.Mention + andStr + " you're a James Bond");
                            andStr = " and";
                        }
                        catch (Exception e)//I'm not sure what the issue is, but it fails on some users. Based on bad documentation, I think users need to be online or else it fails
                        {
                            await msgEvent.Message.Respond(member.User.Mention + " I couldn't make you a James Bond, sorry!");
                        }
                    }
                }
                await msgEvent.Message.Respond("Everyone is a James Bond 😎👍");
            }
            return;
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

        public async Task RespondRandomly(MessageCreateEventArgs msgEvent)
        {
            double random = randomDoubleRange(0, 100);
            if (random <= 4.0)
            {//so a 5% chance
                string responseText = "";//50/50 chance now to get either type of response
                responseText = (random <= 2.0) ? botPartialBiGram.generateNewBiGramSentence() : shakespeareGram.generateNewBiGramSentence();
                await msgEvent.Message.Respond(msgEvent.Message.Author.Mention + " " + responseText);
            }
        }
        */


        //UTILS
        public static double randomDoubleRange(double min, double max)
        {
            return min + (rng.NextDouble() * (max - min));
        }

        //checks if a message contains a user and returns that user if it does
        /*
        private static DiscordUser messageContainsUser(DiscordMessage msg, ulong userId)
        {
            DiscordUser user = null;
            foreach (DiscordUser du in msg.Mentions)
            {
                if (du.ID == userId)
                {
                    user = du;
                    return user;
                }
            }
            return user;
        }

        */
    }
}
